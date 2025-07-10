Param(
	[string]$targetHost = $(throw '$targetHost is required. Use Get-Help to obtain information on usage.'),
	[string]$webServerInstallPath = $(throw '$webServerInstallPath is required. Use Get-Help to obtain information on usage.')
)

# main block
# -------------------------------------------------------------------------------
$cleanupOrphanedSitesScriptBlock = {
	$r_webServerInstallPath = $args[0]
	$dependentAssemblies = @(
		"$($r_webServerInstallPath)\CargoWiseOne.WebInfrastructure.dll"
	)

	# only error and output stream captured in Invoke-Command
	function WriteVerbose { param ($output) Write-Output "[Verbose]$($output)"}
	function WriteInfo { param ($output) Write-Output "[Info]$($output)"}
	function WriteWarning { param ($output) Write-Output "[Warning]$($output)"}
	function WriteDebug { param ([Parameter(ValueFromPipeline=$true)]$output) Process { Write-Output "[Debug]$($output)" }}

	# WiseRates database name pattern
	$wiseRatesDatabaseNamePattern = '.*WISERATES.*';

	# Tear-down a web site for the given site name
	# -------------------------------------------
	function TearDown-Site {
		param(
			[Parameter(ValueFromPipeline=$true)]
			[string]$siteName
		)

		WriteVerbose "Get-WebSite: $($siteName)"
		$site = Get-WebSite -Name $siteName
		if ($site -ne $null) {
			WriteVerbose "Removing web site: $($siteName)"

			$appPools = @()
			if ($site.ApplicationPool -ne "DefaultAppPool") {
				$appPools += $site.ApplicationPool
			}

			$apps = Get-WebApplication -Site $siteName
			if ($apps -ne $null) {
				foreach ($app in $apps) {
					$appPools += $app["applicationPool"]
					WriteVerbose "Remove-WebApplication: $($app["path"]) -Site $($siteName)"
					Remove-WebApplication -Name $app["path"] -Site $siteName
				}
			}

			WriteVerbose "Remove-Website: $($siteName)"
			Remove-Website -Name $siteName
			$appPools | ? { $_ -ne $null } | % {
				$appPool = $_
				WriteVerbose "Remove-WebAppPool: $($appPool)"
				Remove-WebAppPool -Name $appPool
			}
		} else {
			WriteVerbose "WebSite not found: $($siteName)"

			if ((Get-Module -Name "IISAdministration") -ne $null) {
				$orphanedAppPools = Get-IISAppPool | Where-Object {$_.Name.StartsWith($siteName)}
				foreach ($appPool in $orphanedAppPools) {
					$appoolName = $appPool.Name
					$Applications = (Get-WebConfigurationProperty "/system.applicationHost/sites/site/application[@applicationPool='$appoolName']" "machine/webroot/apphost" -name path)
					$NumberOfApplications = $Applications.Count
					if ($NumberOfApplications -eq 0) {
						WriteVerbose "Found orphaned appPool $($appoolName) with $($NumberOfApplications) applications, trying to remove"
						Remove-WebAppPool -Name $appPool.Name
					} else {
						WriteVerbose "Found misconfigured appPool $($appoolName) with $($NumberOfApplications) applications"
						foreach ($app in $Applications) {
							WriteWarning $app.ItemXPath
						}
					}
				}
			} else {
				WriteVerbose "IISAdministration Module not installed on this machine"
			}
		}

		WriteVerbose "End TearDown-Site: $($siteName)"
	}

	# Get web sites where the bound databases are torn down
	# either not exist from the target sql server or the server is not reachable
	# -------------------------------------------------------------------------------
	function Get-OrphanedSites {
		$orphanedSites = @()
		$allSites = @()

		Get-WebSite | ? { $_.Name -ne "Default Web Site" } | % {
			$site = $_
			$webAppPath = [CargoWiseOne.WebInfrastructure.WebAppPath]::Parse("$($site.Id)/")
			$webDbConfig = [CargoWiseOne.WebInfrastructure.WebDbConfiguration]::GetConfiguration($webAppPath)

			if ($webDbConfig -ne $null) {
				$serverName = $webDbConfig.ServerName
				$databaseName = $webDbConfig.DatabaseName
				$siteName = $site.Name

				$SiteInfo = New-Object PSObject -Property @{
					ServerName = $serverName
					DatabaseName = $databaseName
					SiteName = $siteName
				}

				$allSites += $SiteInfo
			}
		}

		$sitesGroupedByServerName = $allSites | Group-Object -Property ServerName
		foreach ($group in $sitesGroupedByServerName) {
			$serverName = $group.Name
			$siteName = $group.SiteName
			$distinctDatabaseNames = @()
			$group.Group | Select-Object -ExpandProperty DatabaseName -Unique | % {
				$dbName = $_
				if ($dbName -match $wiseRatesDatabaseNamePattern) {
					WriteVerbose "WiseRates site '${$siteName}' will be excluded from cleanup for database: '$($dbName)' on server: '$($serverName)'"
				} else {
					$distinctDatabaseNames += $dbName
				}
			}

			$nonExistDatabases = FilterNonExistDatabases -serverName $serverName -databaseNames $distinctDatabaseNames
			if ($nonExistDatabases.Length -gt 0) {
				WriteVerbose "Databases not found on '$($serverName)': $($nonExistDatabases -join ",")"
			}

			$group.Group | ? { $nonExistDatabases -contains $_.DatabaseName } | Select-Object -ExpandProperty SiteName | % {
				$orphanedSites += $_
			}
		}

		return $orphanedSites
	}

	# OdysseyAdmin login has to be used as we need to connect to master database and check main db list in this case
	function Open-PdsAdminConnection {
		param (
			[string]$binPath,
			[string]$dbServer
		)

		# Function will return all values outputted by statements, see https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions?view=powershell-7.4
		# The below is a workaround to return single and expected result from a function, see https://stackoverflow.com/questions/29556437/how-to-return-one-and-only-one-value-from-a-powershell-function
		$null = @(
			Add-Type -Path "$binPath\Microsoft.Extensions.DependencyInjection.dll"
			Add-Type -Path "$binPath\CargoWise.DataProtection.dll"
			Add-Type -Path "$binPath\CargoWise.DataProtection.SqlExtensions.dll"

			$services = [Microsoft.Extensions.DependencyInjection.ServiceCollection]::new()
			[CargoWise.DataProtection.ProtectedDataService]::ConfigureProtectedDataFactoryServices($services)
			[CargoWise.DataProtection.SqlExtensions]::ConfigureProtectedDataSqlExtensions($services, [CargoWise.DataProtection.ApplicationType]::Web)
			$serviceProvider = [Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions]::BuildServiceProvider($services)

			$connectionProvider = $serviceProvider.GetService([CargoWise.DataProtection.ISqlConnectionProvider])
		)

		# Open connection
		return $connectionProvider.OpenNewAdminSqlConnection(
			$dbServer,
			[System.Action[System.Data.SqlClient.SqlConnectionStringBuilder]] {
				param ($builder)

				$builder.PersistSecurityInfo = $false
				$builder["Application Name"] = "CargoWiseOne.Deployment.TestRig"
			})
	}

	# check if database exists on the specified SQL Server
	# -------------------------------------------------------------------------------
	function FilterNonExistDatabases {
		param(
			[string]$serverName,
			[string[]]$databaseNames = @()
		)

		try {
			$csvString = $databaseNames -join ","
			$quotedCsvString = "'" + $csvString.Replace(",", "','") + "'"
			$commandText = "SELECT name FROM sys.databases WHERE name in ($quotedCsvString)"

			$sqlConnection = Open-PdsAdminConnection $r_webServerInstallPath $serverName
			$command = $sqlConnection.CreateCommand()

			$command.CommandText = $commandText
			$reader = $command.ExecuteReader()

			$foundDatabases = @()
			while ($reader.Read()) {
				$foundDatabases += $reader["name"]
			}

			$noExistDatabases = $databaseNames | Where-Object { $_ -ne $null -and $_ -notin $foundDatabases }
			return $noExistDatabases
		} catch {
			$exception = $_
			WriteVerbose "FilterNonExistDatabases error: $($serverName), $($quotedCsvString), exception: $($exception)"
			return $false
		} finally {
			if ($command) {
				$command.Dispose()
			}

			if ($sqlConnection) {
				$sqlConnection.Dispose()
			}
		}
	}

	$computerSystem = Get-WmiObject -Class Win32_ComputerSystem
	WriteVerbose "Cleaning up orphaned websites on '$($computerSystem.Name).$($computerSystem.Domain)'"

	Import-Module -Name WebAdministration
	Add-Type -AssemblyName System.Data
	$dependentAssemblies | % { Add-Type -Path $_ }

	$orphanedSites = Get-OrphanedSites
	if ($orphanedSites.Length -le 0) {
		WriteVerbose "No orphaned sites found."
		return
	}

	# Process
	WriteVerbose "Orphaned websites found on '$($computerSystem.Name).$($computerSystem.Domain)': $($orphanedSites -join ",")"
	$orphanedSites | % {
		$siteName = $_

		try {
			WriteVerbose "---------------------------------------"
			WriteVerbose "TearDown-Site: $($siteName)"
			TearDown-Site -siteName $siteName
		}
		catch {
			$exception = $_
			Write-Error "TearDown-Site error on site $($siteName): $($exception)"
			continue
		}
	}
}

# cleanup orphaned sites
# -------------------------------------------------------------------------------
$session = New-PSSession -computerName "$targetHost" # -auth CredSSP -credential $cred

Write-Output "Running cleanup orphaned web script on $($targetHost)"
Invoke-Command -Session $session -Args $webServerInstallPath -OutVariable output -scriptblock $cleanupOrphanedSitesScriptBlock -ErrorAction Stop

Remove-PSSession $session
