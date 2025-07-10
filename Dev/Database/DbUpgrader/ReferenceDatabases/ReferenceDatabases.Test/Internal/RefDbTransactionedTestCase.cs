using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	abstract class RefDbTransactionedTestCase<TUpgrader> : TestCase where TUpgrader : ReferenceDbUpgrader
	{
		protected override void SetUp()
		{
			mockUpgradeContext = new Mock<IUpgradeContext>();
			refDbUpgrader = GetNewReferenceDbUpgrader();
			refDbUpgrader.DbPreparationStrategyForTest = RefDbPreparationStrategyHelper.TestDbPreparationStrategy;
			DropTestDb(refDbUpgrader.DbName);
			testConnection.BeginTransaction();
			logger = new UpgradeTaskWorkflowLoggerTestClass();
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();

			testConnection.RollbackTransaction();
			DropTestDb(refDbUpgrader.DbName);
			refDbUpgrader = null;
		}

		void DropTestDb(string testDbName)
		{
			using (var dropConn = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(dropConn, testDbName);
			}
		}

		protected virtual TUpgrader GetNewReferenceDbUpgrader()
		{
			return (TUpgrader)Activator.CreateInstance(typeof(TUpgrader), mockUpgradeContext.Object, testConnection, logger);
		}

		protected DbConnection testConnection = Db.NewExtraConnectionToMainDb();
		protected TUpgrader refDbUpgrader;
		protected UpgradeTaskWorkflowLoggerTestClass logger;
		protected Mock<IUpgradeContext> mockUpgradeContext;
	}

	class RefDbPreparationStrategyHelper
	{
		static internal IRefDbPreparationStrategy TestDbPreparationStrategy(string mainDbName, RefDbTypeEnum databaseType, string countryCode, DbConnection upgradeConnection)
		{
			if (TestingState.IsRunningTests)
			{
				return (mainDbName == Db.DatabaseName)
					? new RefDbPreparationStrategyForTesting(mainDbName, databaseType, countryCode, upgradeConnection)
					: new StandardRefDbPreparationStrategy(mainDbName, databaseType, countryCode, upgradeConnection);
			}
			return null;
		}
	}
}
