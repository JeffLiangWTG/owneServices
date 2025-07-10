using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_DeleteOldLogBackupFiles))]
	class ep_DeleteOldLogBackupFilesTest : DbCreateScriptTest
	{
		//Full tests are in Enterprise\DataTools\BackupBatchProcessor\Backup\LogBackupRunner.cs
	}
}

