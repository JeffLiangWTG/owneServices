using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListLanguageCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListLanguageCombined
{
	[TestedType(typeof(trgZZRefCusCodeListLanguageCombined_Upd))]
	class trgZZRefCusCodeListLanguageCombined_Upd_Test : DbCreateScriptTest
	{
		public void TestZZRefCusCodeListLanguageCombinedUpdate()
		{
			var codeListPK = Guid.NewGuid();
			var codeListLanguagePK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO RefDatabase_RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
												VALUES (NEWID(), 'ZHT', 'ChineseTraditional');
				INSERT INTO RefDatabase_RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
												VALUES (NEWID(), 'IT', 'Italian');
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
												VALUES (NEWID(), '_X', 'TEST', NULL);
				INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping)
												VALUES (NEWID(), '_XX_', 'JUST FOR TEST', 1, '_X');
				INSERT INTO dbo.ZZRefCusCodeList (ZZD_PK, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_CountryOrGrouping)
												VALUES ('{codeListPK}', '_XX_', '_X_2', '_X_2', '1900-01-01', '2079-06-06', '_X');
				INSERT INTO dbo.ZZRefCusCodeListLanguage (ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description, ZXA_SystemCreateTimeUtc, ZXA_SystemCreateUser, ZXA_SystemLastEditTimeUtc, ZXA_SystemLastEditUser)
														VALUES ('{codeListLanguagePK}', 'IT', '{codeListPK}', 'IT Description', '2079-06-06', '~BP', '2079-06-06', '~BP');");

			AssertSelectResult("codeListLanguagePK ZZRefCusCodeListLanguage", TestConnection, codeListLanguagePK, new[] { "IT, IT Description" });

			TestConnection.ExecuteNonQuery($@"UPDATE dbo.ZZRefCusCodeListLanguageCombined
											SET ZXA_ZX6_NKLanguage = 'ZHT', ZXA_Description = 'ZHT Description'
											WHERE ZXA_PK = '{codeListLanguagePK}'");

			AssertSelectResult("codeListLanguagePK ZZRefCusCodeListLanguage", TestConnection, codeListLanguagePK, new[] { "ZHT, ZHT Description" });
		}

		void AssertSelectResult(string message, DbConnection connection, Guid codeListLanguagePK, string[] expectedResults)
		{
			var selectSql = $"SELECT * FROM dbo.ZZRefCusCodeListLanguage WHERE ZXA_PK = '{codeListLanguagePK}'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZXA_ZX6_NKLanguage"]}, {reader["ZXA_Description"]}");
			});
			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}
	}
}

