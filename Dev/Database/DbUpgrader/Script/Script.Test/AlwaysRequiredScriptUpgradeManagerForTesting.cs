using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script.Test
{
	public sealed class AlwaysRequiredScriptUpgradeManagerForTesting : BaseUpgradeManager
	{
		readonly ScriptUpgraderForTesting testScriptUpgrader;

		public AlwaysRequiredScriptUpgradeManagerForTesting(ScriptUpgraderForTesting testScriptUpgrader)
		{
			this.testScriptUpgrader = testScriptUpgrader;
		}

		public override bool IsHosted => false;

		public override ValidationResponse Run()
		{
			if (testScriptUpgrader.hasCustomConnection)
			{
				testScriptUpgrader.UpgConnectionExposed.ThreadSentry.TakeThreadOwnership();
			}

			var dbInfo = String.Format("Server: {0} - Database: {1}",
				Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName);
			((IUpgradeManager)this).ShowInfoMessage(dbInfo);

			((IUpgradeManager)this).ActivateTaskProgress(testScriptUpgrader.EstimatedNumberOfTasks);

			testScriptUpgrader.CreateTestDbs();
			testScriptUpgrader.DoTestUpgrade();
			testScriptUpgrader.RunUpgrade();

			if (testScriptUpgrader.hasCustomConnection)
			{
				testScriptUpgrader.UpgConnectionExposed.ThreadSentry.RelinquishThreadOwnership();
			}

			var result = new ValidationResponse();
			result.Successful = true;
			return result;
		}

		public override VersionLabel SchemaVersionBeforeUpgrade
		{
			get { throw new NotImplementedException(); }
		}

		public override VersionLabel TransformationVersionBeforeUpgrade
		{
			get { throw new NotImplementedException(); }
		}
	}
}
