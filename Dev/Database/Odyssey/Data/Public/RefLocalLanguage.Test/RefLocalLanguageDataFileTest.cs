using System;
using System.IO;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefLocalLanguageDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefLocalLanguageDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestRefLocalLanguageDataFile()
		{
			string insertSql = @"
				INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem) VALUES ('ABD8FF88-79E1-485B-8133-3174181917FA', 'ABC', 'CN', 'My Language 1', 1, 1)
				INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem) VALUES ('17C63AA4-F4DB-486F-BE91-76EA1919F0A8', 'DEF', 'US', 'My Language 2', 1, 1)";
			Db.Connection.ExecuteNonQuery(insertSql);

			var file = new RefLocalLanguageDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			Assert("RefLocalLanguage row count", data.Tables["RefLocalLanguage"].Rows.Count >= 2);
			Assert("Contains My Language 1", data.Tables["RefLocalLanguage"].Rows.Contains(new Guid("ABD8FF88-79E1-485B-8133-3174181917FA")));
			Assert("Contains My Language 2", data.Tables["RefLocalLanguage"].Rows.Contains(new Guid("17C63AA4-F4DB-486F-BE91-76EA1919F0A8")));
		}

		public void TestUserRowsAreNotLoaded()
		{
			string insertSql = @"
				INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem) VALUES ('ABD8FF88-79E1-485B-8133-3174181917FA', 'ABC', 'CN', 'My Language 1', 1, 1)
				INSERT INTO dbo.RefLocalLanguage (RA_PK, RA_Code, RA_RN_NKCountryCode, RA_Description, RA_IsActive, RA_IsSystem) VALUES ('17C63AA4-F4DB-486F-BE91-76EA1919F0A8', 'DEF', 'US', 'My Language 2', 1, 0)";

			Db.Connection.ExecuteNonQuery(insertSql);

			var file = new RefLocalLanguageDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			Assert("System language should have been loaded", data.Tables["RefLocalLanguage"].Rows.Contains(new Guid("ABD8FF88-79E1-485B-8133-3174181917FA")));
			Assert("User language should NOT have been loaded", !data.Tables["RefLocalLanguage"].Rows.Contains(new Guid("17C63AA4-F4DB-486F-BE91-76EA1919F0A8")));
		}
	}
}
