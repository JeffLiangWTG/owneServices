 Param(
  [string]$targetHost = $(throw '$targetHost is required. Use Get-Help to obtain information on usage.'),
  [string]$webServerInstallPath = $(throw '$webServerInstallPath is required. Use Get-Help to obtain information on usage.'),
  [string]$databaseServer = $(throw '$databaseServer is required. Use Get-Help to obtain information on usage.'),
  [string]$databaseName = $(throw '$databaseName is required. Use Get-Help to obtain information on usage.'),
  [string]$domain= $(throw '$domain is required. Use Get-Help to obtain information on usage.'),
  [boolean]$disallowDuplicateSites = $(throw '$disallowDuplicateSites is required. Use Get-Help to obtain information on usage.')
 ) #end param
 
$scriptBlockRemote = {
	$r_webServerInstallPath = $args[0]
	$r_databaseServer = $args[1]
	$r_databaseName = $args[2]
	$r_domain = $args[3]
	$r_disallowDuplicateSites = $args[4]
	#only error and output stream captured in Invoke-Command
	function WriteVerbose { param ($output) Write-Output "[Verbose]$($output)"}
	function WriteWarning { param ($output) Write-Output "[Warning]$($output)"}
	function WriteDebug { param ([Parameter(ValueFromPipeline=$true)]$output) Write-Output "[Debug]$($output)"}

	Add-Type -Path "$($r_webServerInstallPath)\Enterprise.Upgrades.dll"
	Add-Type -Path "$($r_webServerInstallPath)\CargoWiseOne.WebInfrastructure.dll"

	$tearDownSites = @()
	$tearDownSites += $r_domain

	# if $disallowDuplicateSites is true
	# enumerate all sites that are configured targeting the same system, in addition to the domain as specified
	# i.e. the underlying database server and database name configured for the sites are the same as the specified
	if ($r_disallowDuplicateSites -eq $true) {
		Get-Website | ? { $_.Name -ne $r_domain } | % {
			$site = $_
			$webPath = [CargoWiseOne.WebInfrastructure.WebAppPath]::Parse("$($site.Id)/")
			if ($webPath -ne $null) {
				$webDbConfig = [CargoWiseOne.WebInfrastructure.WebDbConfiguration]::GetConfiguration($webPath);
				if (($webDbConfig -ne $null) -AND ($webDbConfig.DatabaseName -eq $r_databaseName) -AND ($webDbConfig.ServerName -eq $r_databaseServer)) {
					$tearDownSites += $site.Name
				}
			}
		}
	}

	$mutex = New-Object Enterprise.Upgrades.UpgraderMutex([CargoWiseOne.WebInfrastructure.WebUpgradeManager]::UpdateVirtualAppPhysicalDirectoryMutextName)
	try
	{
		WriteVerbose "Trying to get Mutex [CargoWiseOne.WebInfrastructure.WebUpgradeManager]::UpdateVirtualAppPhysicalDirectoryMutextName"
		if (!$mutex.WaitOne())
		{
			throw "Mutex not obtained";
		}

		foreach ($siteName in $tearDownSites) {
			$domain = $siteName

			WriteVerbose "Looking for Website $($domain)"
			$site = Get-Website -Name $domain
			if ($site -ne $null) {
				Write-Output "Removing Website $($domain)"
				$appPools = New-Object System.Collections.ArrayList
				if ($site.ApplicationPool -ne "DefaultAppPool") {
					$appPools.Add($site.ApplicationPool) > $null
				}

				$apps = Get-WebApplication -Site $domain
				foreach ($app in $apps)	{
					# stop app pool before removing application
					$appPool = Get-IISAppPool -name $app["applicationPool"]
					if (($appPool.State -eq "Started") -or ($appPool.State -eq "Starting"))
					{
						WriteVerbose "Stop-WebAppPool -Name $($appPool.Name)"
						Stop-WebAppPool -Name $appPool.Name
					}

					$appPools.Add($app["applicationPool"]) > $null
					WriteVerbose "Remove-WebApplication -Name $($app["path"]) -Site $($domain)"
					Remove-WebApplication -Name $app["path"] -Site $domain
				}

				WriteVerbose "Remove-Website -Name $($domain)"
				Remove-Website -Name $domain
				foreach ($appPool in $appPools) {
					WriteVerbose "Remove-WebAppPool -Name $($appPool)"
					Remove-WebAppPool -Name $appPool
				}
			}
			else
			{
				Write-Output "Website $($domain) Not Found"

				$orphanedAppPools = Get-IISAppPool | Where-Object {$_.Name.StartsWith($domain)}
				foreach ($appPool in $orphanedAppPools) {
					$appoolName = $appPool.Name
					$Applications = (Get-WebConfigurationProperty "/system.applicationHost/sites/site/application[@applicationPool='$appoolName']" "machine/webroot/apphost" -name path)
					$NumberOfApplications = $Applications.Count
					if ($NumberOfApplications -eq 0) {
						WriteWarning "Found orphaned appPool $($appoolName) with $($NumberOfApplications) applications, trying to remove"
						Remove-WebAppPool -Name $appPool.Name
					} else {
						WriteWarning "Found misconfigured appPool $($appoolName) with $($NumberOfApplications) applications"
						foreach ($app in $Applications) {
							WriteWarning $app.ItemXPath
						}
					}
				}
			}
		}
	}
	finally
	{
		$mutex.Dispose();
	}
}

$session = new-pssession -computerName "$targetHost" # -auth CredSSP -credential $cred 

Write-Output "Running uninstall script on $($targetHost)"
Invoke-Command -Session $session -Args $webServerInstallPath,$databaseServer,$databaseName,$domain,$disallowDuplicateSites -OutVariable output -scriptblock $scriptBlockRemote
	
Remove-PSSession $session
