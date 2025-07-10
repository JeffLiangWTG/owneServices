## Copy Production To Test Local Testing for S3 Read-Only Access Keys

When running CP2T locally, if you wish to test the S3 read-only key functionality, there are a few setup steps you will need to do.

### 1. Save AWS assume role credentials to your local Windows Registry

1. Open a powershell window as administrator
1. Run the following script:
```powershell
Add-Type -AssemblyName System.Security

$data = '<accessKeyId>;<secretKey>;<externalId>'
$registryPath = 'HKLM:\SOFTWARE\WiseTech Global\CargoWise\Database Backup and Restore\AWS Credentials'
$registryKey = 'S3 Read-Only Access Key Generator'

$dataBytes = [System.Text.Encoding]::UTF8.GetBytes($data)
$encryptedBytes = [System.Security.Cryptography.ProtectedData]::Protect($dataBytes, $null, [System.Security.Cryptography.DataProtectionScope]::LocalMachine)
$encryptedData = [Convert]::ToBase64String($encryptedBytes)

New-Item -Path $registryPath -Force | Out-Null
Set-ItemProperty -Path $registryPath -Name $registryKey -Value $encryptedData
```
Replacing `<accessKeyId>`, `<secretKey>` and `<externalId>` with the corresponding access key ID, secret key ID and external ID of the AWS user with the correct assume role permissions.

You may want to confirm this script is updated to align with the script found under `<AWSCredentialDeployer code link>`.

### 2. Disable the "HostedWithCargoWiseOne" check

To properly execute the logic associated with S3 read-only keys, you will need to disable the "HostedWithCargoWiseOne" check. In a production environment, this check is used to determine if the application is hosted with CargoWise One, as opposed to Self-Hosted customers. This check is not necessary for local testing.

1. Navigate to `C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\DataTools\DbBackupAndRestore\DbBackupAndRestore.Business\Restore\DbRestoreManager.cs`
2. Under the `PerformCopyProductionToTestPostRestoreSteps()` method, look for the line `if (licence.WasRestoredDatabaseHostedWithCargoWise(connection, dbRestoreSettings.TargetDbName) && IsEDocsStorageProviderS3(connection, dbRestoreSettings.TargetDbName))`
3. Comment out the `WasRestoredDatabaseHostedWithCargoWise()` check, like so:
```c#
if (/*licence.WasRestoredDatabaseHostedWithCargoWise(connection, dbRestoreSettings.TargetDbName) && */IsEDocsStorageProviderS3(connection, dbRestoreSettings.TargetDbName))
```

### 3. Set up your local CargoWise One instance with the correct S3 Registry values

1. Launch your local CargoWise One instance, and open the Registry module
2. Navigate to `System > DocManager > S3 Storage`
3. Enter override values for `S3 Bucket Name`, `S3 Storage Credentials` and `S3 Storage URL`. These values should be the ones you are using for your testing.
	a. The bucket name should be in a format like `wced-apac-<aws-account-number>-abcdef-001-pri`
	b. The storage URL should be the S3 storage endpoint in your region, e.g. `http://s3-ap-southeast-2.amazonaws.com`
	c. The storage credentials should be read/write access keys to the S3 bucket, however for pure CP2T testing, this should not matter as they will be overwritten. 
1. Navigate to `System > DocManager > eDocs Storage` and override the value to `S3`.

### 4. Create a backup of your local Odyssey database

Run the DbBackupAndRestore tool to create a backup of the Odyssey database that you are using for your local testing.

### 5. Run the Copy Production To Test tool

You can now run a Copy Production to Test operation on your local machine, and you should see that S3 read-only access keys have been created in the output window. You can also verify this in the test system's CW1 Registry, or queried directly to the DB through SSMS. 
