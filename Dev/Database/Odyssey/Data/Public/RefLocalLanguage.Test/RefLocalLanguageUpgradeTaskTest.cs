using System;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefLocalLanguageUpgradeTaskTest : TransactionedTestCase
	{
		public void TestDataTables()
		{
			Task.Run();

			var tempFile = new RefLocalLanguageDataFile();
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals(RefLocalLanguageSchema.Constants.TableName, data.Tables[0].TableName);
		}

		public void TestParentColumnIsUpdated()
		{
			var sqlText = @"INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem) VALUES ('ABD8FF88-79E1-485B-8133-3174181917FA', 'EN', 'US', 'User Language', 1, 0)
							INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem, RA_RA_ParentLanguage) VALUES ('A38B1377-5BA7-447E-96B8-11916BBC9122', 'ABC', 'US', 'Child User Language', 1, 0, 'ABD8FF88-79E1-485B-8133-3174181917FA')
							INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem) VALUES ('8EC97942-9B31-497A-A6D7-84865757C2FA', 'OTH', 'US', 'Other Language', 1, 0)
							INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem, RA_RA_ParentLanguage) VALUES ('6C049612-1462-40F1-9D3D-2D8872ADC279', 'DEF', 'US', 'Other Child Language', 1, 0, '8EC97942-9B31-497A-A6D7-84865757C2FA')";

			TestConnection.ExecuteNonQuery(sqlText);
			Task.Run();
			var systemDefinedEngLanguagePK = GetLanguageColumnValueInDatabase("EN", "US", true, RefLocalLanguageSchema.Constants.PK);
			var parentLanguagePkOfUpdatedChildLanguage = GetLanguageColumnValueInDatabase("ABC", "US", false, RefLocalLanguageSchema.Constants.RA_RA_ParentLanguage);
			Assert("User language having the same code as system defined should be deleted.", !IsLanguageInDatabase(new Guid("ABD8FF88-79E1-485B-8133-3174181917FA"), "EN", "US", false));
			Assert("System language should be inserted.", systemDefinedEngLanguagePK != null && (Guid)systemDefinedEngLanguagePK != Guid.Empty);
			AssertEquals("Parent language of child language should be updated if it refers to a user language having the same code as system language", systemDefinedEngLanguagePK, parentLanguagePkOfUpdatedChildLanguage);

			var parentLanguagePkOfNonUpdatedChildLanguage = GetLanguageColumnValueInDatabase("DEF", "US", false, RefLocalLanguageSchema.Constants.RA_RA_ParentLanguage);
			Assert("User language doesn't have same code as system defined should NOT be changed", IsLanguageInDatabase(new Guid("8EC97942-9B31-497A-A6D7-84865757C2FA"), "OTH", "US", false));
			AssertEquals("Parent language of child language should NOT be updated if it doesn't refers to a user language having the same code as system language",
				new Guid("8EC97942-9B31-497A-A6D7-84865757C2FA"), parentLanguagePkOfNonUpdatedChildLanguage);
		}

		bool IsLanguageInDatabase(Guid pk, string code, string countryCode, bool isSystem)
		{
			var isSystemString = isSystem ? "1" : "0";
			string sqlText = $"SELECT count(*) FROM dbo.RefLocalLanguage WHERE RA_PK = '{pk.ToString()}' AND RA_Code = '{code}' AND RA_IsSystem = {isSystemString}";
			int rowCount = (int)TestConnection.ExecuteScalar(sqlText);
			return (rowCount == 1);
		}

		object GetLanguageColumnValueInDatabase(string code, string countryCode, bool isSystem, string columnName)
		{
			var isSystemString = isSystem ? "1" : "0";
			string sqlText =
				$"SELECT {columnName} FROM dbo.RefLocalLanguage WHERE RA_Code = '{code}' AND RA_RN_NKCountryCode = '{countryCode}' AND RA_IsSystem = {isSystemString}";
			return TestConnection.ExecuteScalar(sqlText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataHelpers.ClearTable(RefLocalLanguageSchema.Constants.TableName);
			Task = new RefLocalLanguageUpgradeTask();
		}

		RefLocalLanguageUpgradeTask Task;
	}
}
