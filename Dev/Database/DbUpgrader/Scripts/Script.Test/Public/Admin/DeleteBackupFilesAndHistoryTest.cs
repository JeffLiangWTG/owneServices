using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(DeleteBackupFilesAndHistory))]
	class DeleteBackupFilesAndHistoryTest : DbCreateScriptTest
	{
		//Full tests are in Enterprise\DataTools\BackupBatchProcessor\Backup\LogBackupRunner.cs
	}
}

