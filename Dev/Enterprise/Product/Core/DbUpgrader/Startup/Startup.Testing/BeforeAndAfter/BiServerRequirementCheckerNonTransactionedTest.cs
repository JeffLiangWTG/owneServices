using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BiServerRequirementCheckerNonTransactionedTest : TestCase
	{
		public void TestCheckSqlServerGeneration()
		{
			var checker = new BiServerRequirementCheckerForTest();

			AssertNoExceptionThrown(
				"No exception should be thrown because the running SQL Server version is supported.",
				() => checker.CheckSqlServerGeneration_Exposed(Db.Connection));
			using (Db.Connection.SetSqlServerVersionForTest("0.00.0000.0"))
			{
				AssertExceptionThrown("An exception should be thrown because the SQL Server version is not supported.",
					typeof(ServerRequirementsNotMetException),
					() => checker.CheckSqlServerGeneration_Exposed(Db.Connection));
			}
		}

		public void TestCheckSqlServerGenerationForNotYetSupportedSqlServer()
		{
			// Arrange
			var checker = new BiServerRequirementCheckerForTest();

			using (Db.Connection.SetSqlServerVersionForTest("99.00.0000.0"))
			{
				// Act
				// Assert
				AssertNoExceptionThrown("An exception should be thrown because the SQL Server version is not supported.",
					() => checker.CheckSqlServerGeneration_Exposed(Db.Connection));
			}
		}
	}
}
