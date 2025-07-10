 Param(
  [string]$targetHost = $(throw '$targetHost is required. Use Get-Help to obtain information on usage.'),
  [string]$webServerInstallPath = $(throw '$webServerInstallPath is required. Use Get-Help to obtain information on usage.'),
  [string]$databaseServer = $(throw '$databaseServer is required. Use Get-Help to obtain information on usage.'),
  [string]$databaseName = $(throw '$databaseName is required. Use Get-Help to obtain information on usage.'),
  [string]$domain= $(throw '$domain is required. Use Get-Help to obtain information on usage.'),
  [string[]]$sites = $(throw '$sites is required. Use Get-Help to obtain information on usage.'),
  [boolean]$disallowDuplicateSites = $(throw '$disallowDuplicateSites is required. Use Get-Help to obtain information on usage.'),
  [string]$clientCode = $(throw '$clientCode is required. Use Get-Help to obtain information on usage.')
 ) #end param

$scriptBlockRemote = {
	$r_webServerInstallPath = $args[0]
	$r_databaseServer = $args[1]
	$r_databaseName = $args[2]
	$r_domain = $args[3]
	$r_sites = $args[4]
	$r_disallowDuplicateSites = $args[5]
	$r_clientCode = $args[6]
	#only error and output stream captured in Invoke-Command
	function WriteVerbose { param ($output) Write-Output "[Verbose]$($output)"}
	function WriteWarning { param ($output) Write-Output "[Warning]$($output)"}
	function WriteDebug { param ([Parameter(ValueFromPipeline=$true)]$output) Process {Write-Output "[Debug]$($output)"}}

	function Open-RestrictedWriterLoginConnection {
		param (
			[string]$binPath,
			[string]$dbServer,
			[string]$dbName
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

			$protectedDataServiceFactory = $serviceProvider.GetService([CargoWise.DataProtection.IProtectedDataServiceFactory])
			$pds = $protectedDataServiceFactory.CreateSystemService($dbServer, $dbName)

			$connectionProvider = $serviceProvider.GetService([CargoWise.DataProtection.ISqlConnectionProvider])

			# Make a generic method to create connection
			$openNewSqlConnectionMethodInfo = [CargoWise.DataProtection.ISqlConnectionProvider].GetMethod("OpenNewSqlConnection")
			$openNewSqlConnectionMethod = $openNewSqlConnectionMethodInfo.MakeGenericMethod([CargoWise.DataProtection.RestrictedWriterLoginCredentials])
		)

		# Open connection
		return $openNewSqlConnectionMethod.Invoke($connectionProvider, @(
			$pds,
			[System.Action[System.Data.SqlClient.SqlConnectionStringBuilder]] {
				param ($builder)

				$builder.PersistSecurityInfo = $false
				$builder["Application Name"] = "CargoWiseOne.Deployment.TestRig"
				$builder["Data Source"] = $dbServer
				$builder["Initial Catalog"] = $dbName
			}))
	}

	WriteVerbose "Setup SqlConnection"
	$connection = Open-RestrictedWriterLoginConnection $r_webServerInstallPath $r_databaseServer $r_databaseName

	WriteVerbose "Getting all valid sites"
	Add-Type -Path "$($r_webServerInstallPath)\CargoWiseOne.WebInfrastructure.dll"
	$installSiteItems = [CargoWiseOne.WebInfrastructure.InstallSiteItem]::GetSitesToInstall()

	if ($r_clientCode -eq "EDI") {
		WriteVerbose "Found Client Code EDI, Adding ZClientWebEDI to valid sites"
		$constructorArgs = @("ZClientWebEDI", "www", "ZClientWebEDI", "Client Web For EDI")
		$newObjectParameters = @{
			TypeName = 'CargoWiseOne.WebInfrastructure.InstallSiteItem'
			ArgumentList = $constructorArgs
		}
		$clientSite = New-Object @newObjectParameters
		$installSiteItems = [Collections.Generic.List[CargoWiseOne.WebInfrastructure.InstallSiteItem]]@($installSiteItems)
		$installSiteItems.Add($clientSite)
	}

	$installer = New-Object -TypeName CargoWiseOne.WebInfrastructure.RemoteSiteInstaller
	foreach ($site in $r_sites)	{
		$valid = $FALSE
		WriteVerbose "Verify site $($site) is valid, searching all valid sites"
		foreach ($installSiteItem in $installSiteItems) {
			if ($installSiteItem.FolderName -ceq $site) {
				$valid = $TRUE
				if ($r_disallowDuplicateSites) {
					Write-Verbose "ValidateDuplicateWebsites"
					$installer.ValidateDuplicateWebsites($installSiteItem.WebAddress, $r_databaseServer, $r_databaseName)
				}
				$installSiteItem.Install = $TRUE
				$installSiteItem.WebAddress = "$($r_domain)/$($installSiteItem.DefaultApplicationPath)"
				Write-Output "Found valid site to install: $($site) => $($installSiteItem.Description) https://$($installSiteItem.WebAddress)"
			}
		}
		if (!$valid)
		{
			WriteWarning "$($site) not found in valid sites ($($installSiteItems.FolderName -Join ', '))"
		}
	}

	WriteDebug "Display dll file and assembly versions"
	Get-ChildItem $r_webServerInstallPath -Filter *.dll | Select-Object Name,@{n='FileVersion';e={$_.VersionInfo.FileVersion}},@{n='AssemblyVersion';e={[Reflection.AssemblyName]::GetAssemblyName($_.FullName).Version}} | WriteDebug

	WriteVerbose "calling CargoWiseOne.WebInfrastructure.RemoteSiteInstaller.Install()"
	$installer.Install($r_databaseServer, $connection, $installSiteItems)
}

$session = new-pssession -computerName "$targetHost" # -auth CredSSP -credential $cred

Write-Output "Running install script on $($targetHost)"
Invoke-Command -Session $session -Args $webServerInstallPath,$databaseServer,$databaseName,$domain,$sites,$disallowDuplicateSites,$clientCode -OutVariable output -scriptblock $scriptBlockRemote

Remove-PSSession $session
