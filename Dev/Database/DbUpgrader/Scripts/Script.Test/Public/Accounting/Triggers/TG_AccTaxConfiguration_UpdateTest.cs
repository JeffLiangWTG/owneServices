using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTaxConfiguration_Update))]
	class TG_AccTaxConfiguration_UpdateTest : DBCreateTriggerScriptTest
	{
		public void TestShouldNotAllowToUpdateValues()
		{
			AssertShouldNotAllowToUpdateValues("ETC_Ledger", "AP");
			AssertShouldNotAllowToUpdateValues("ETC_RN_NKCountry", "IN");
			AssertShouldNotAllowToUpdateValues("ETC_TaxAuthorityCode", "TA");
			AssertShouldNotAllowToUpdateValues("ETC_TaxSystemCode", "TS");

			void AssertShouldNotAllowToUpdateValues(string columnName, string value)
			{
				using (var newConnection = Db.NewExtraConnectionToMainDb())
				{
					newConnection.BeginTransaction();
					var dbHelper = new TestDbHelper(newConnection);
					var taxConfigurationPK = dbHelper.InsertTaxConfiguration("Config", TestDbHelper.DefaultCompanyPK);

					var expectedMessage = "Cannot update Ledger/Country/Tax Authority Code/Tax System Code of AccTaxConfiguration";
					var updateSql =
$@"UPDATE dbo.AccTaxConfiguration SET
{columnName} = '{value}',
-- the below is needed otherwise SystemLastEditTriggerSynchronizer will complain about running an update without updating both audit columns causing false positive test failure
-- e.g. System.Data.SqlClient.SqlException (0x80131904): Attempt to update without [ETC_SystemLastEditTimeUtc] for [AccTaxConfiguration].
ETC_SystemLastEditTimeUtc = GETDATE(),
ETC_SystemLastEditUser = 'E'
WHERE ETC_PK = @ETC_PK";
					AssertExceptionThrown<SqlException>("Should be: " + expectedMessage, expectedMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", delegate { dbHelper.RunSQL(new { ETC_PK = taxConfigurationPK }, updateSql); });
					newConnection.RollbackTransaction();
				}
			}
		}
	}
}
