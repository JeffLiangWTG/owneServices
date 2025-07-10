using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	[SetUpFixture]
	public class SetUpFixture
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Db.InitializeDatabaseDetails(System.Environment.MachineName, Helper.MainDatabaseNameOutsideTestCase);

			EnterpriseApplicationConfiguration.ConfigureObjectFactory();

			disposableVersionCheckAction = Db.DisableSchemaVersionCheck();
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				templateDbCreator = new TestMainDbCreator(Mock.Of<IUpgradeManager>(), Db.DatabaseName);
				templateDbCreator.CreateDropExisting();
			}

			using (var adminConnection = Db.NewAdminConnection())
			{
				DbRegistry.SingleRefDatabaseName.SaveValue(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef), adminConnection);
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);

				adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit)}];
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW)}];
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SD)}];
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository)}];
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef)}];
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef)}];
DROP DATABASE IF EXISTS [{Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)}];
");
			}

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				templateDbCreator.Drop();
			}

			disposableVersionCheckAction?.Dispose();
			RefDbTableNameResolver.ResetSingleRefDatabaseName();
		}

		IAuxiliaryDbCreator templateDbCreator;
		IDisposable disposableVersionCheckAction;
	}
}
