Add-Type -AssemblyName System.IO.Compression.FileSystem
# --------------------------------------------------------------------------------
# Use this script to pack zip file produced by $Dev/Bin/WebDeployBuilder.exe
# into a new Edp file to comply with edp package expected folder structures
# --------------------------------------------------------------------------------
$currentDirectory = Get-Location
$timeStamp = (Get-Date).ToString('yy_MM_dd_fff_HHmmss')
$bin = "Bin"
$binDir = [System.IO.Path]::Combine($currentDirectory, $bin)
$webDeployBuilder = [System.IO.Path]::Combine($currentDirectory, $bin, "net8.0", "WebDeployBuilder.exe")
$enterpriseWebDeploy = [System.IO.Path]::Combine($currentDirectory, $bin, "EnterpriseWebDeploy.zip")
$edpFile = [System.IO.Path]::Combine($currentDirectory, $bin, "WebDeployPackage_$timeStamp.edp")
$sharpZipLib = [System.IO.Path]::Combine($currentDirectory, $bin, "ICSharpCode.SharpZipLib.dll")

# Call WebDeployBuilder.exe to generate web deploy zip files
# --------------------------------------------------------------------------------
function GenerateWebDeployZipFiles {
	if (-not(Test-Path $webDeployBuilder))
	{
		Write-Host $webDeployBuilder "is not found"
		Write-Host "You need to run the powershell script from $Dev root path"
		Exit 1
	}

	Write-Host "Executing" $webDeployBuilder
	& $webDeployBuilder
}

# Create edp package file to comply with the expected folder structures
# --------------------------------------------------------------------------------
function CreateEdpPackageFile() {
	$tempPath = [System.IO.Path]::GetTempPath()
	$guid = [System.Guid]::NewGuid()
	$packageTempDir = [System.IO.Path]::Combine($tempPath, $guid)

	try {
		$appDir = [System.IO.Path]::Combine($packageTempDir, "Distribution", "Application")
		[System.IO.Directory]::CreateDirectory($appDir);

		$binFiles = @(
			"Enterprise.exe",
			"CargoWiseOne.exe",
			"CargoWiseOneAnyCpu.exe",
			"CargoWiseOne.WebInfrastructure.Updater.exe",
			"CargoWiseOne.WebInfrastructure.dll",
			"CargoWise.Shared.40.dll",
			"Enterprise.Upgrades.dll",
			"CargoWise.ApplicationManager.Common.dll")

		foreach ($fileName in $binFiles) {
			$filePath = [System.IO.Path]::Combine($binDir, $fileName)
			$destFilePath = [System.IO.Path]::Combine($appDir, $fileName)
			[System.IO.File]::Copy($filePath, $destFilePath)
		}

		Get-ChildItem -Path $binDir\*.zip | ForEach-Object {
			$zipFile = $_.FullName
			$webDeployZipFilePath = [System.IO.Path]::Combine($appDir, $_.Name)
			[System.IO.File]::Copy($zipFile, $webDeployZipFilePath)
		}

		$tempEdpFile = [System.IO.Path]::GetTempFileName()
		$fastZip = New-Object ICSharpCode.SharpZipLib.Zip.FastZip
		$fastZip.CreateZip($tempEdpFile, $packageTempDir, $true, "")

		[System.IO.File]::Move($tempEdpFile, $edpFile)

		if (Test-Path($edpFile)) {
			Write-Host $edpFile "has been successfully created"
		}
	}
	finally {
		[System.IO.Directory]::Delete($packageTempDir, $true)
	}
}

# main
# --------------------------------------------------------------------------------
# Generate web deploy zip files if not exist
if (-not(Test-Path $enterpriseWebDeploy)) {
	Write-Host "Generating WebDeploy Zip Files"
	GenerateWebDeployZipFiles
}

# Delete old edp file if found
if (Test-Path $edpFile) {
	Remove-Item $edpFile
}

# Reference sharpZipLib
Add-Type -Path $sharpZipLib

# Create EDP package file and move to $binDir
CreateEdpPackageFile
# --------------------------------------------------------------------------------
# (end)
