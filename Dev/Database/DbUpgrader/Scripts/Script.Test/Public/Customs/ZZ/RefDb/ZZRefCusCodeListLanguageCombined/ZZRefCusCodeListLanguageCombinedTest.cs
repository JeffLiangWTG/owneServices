using System;
using System.Collections.Generic;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListLanguageCombined
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListLanguageCombined.ZZRefCusCodeListLanguageCombined))]
	class ZZRefCusCodeListLanguageCombined_Test : DbCreateScriptTest
	{
		public void TestZZRefCusCodeListLanguageView()
		{
			var codeList1PK = Guid.NewGuid();
			var codeList2PK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO RefDatabase_RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
												VALUES (NEWID(), 'ZHT', 'ChineseTraditional');
				INSERT INTO RefDatabase_RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
												VALUES (NEWID(), 'IT', 'Italian');
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
												VALUES (NEWID(), '_X', 'TEST', NULL);
				INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping)
												VALUES (NEWID(), '_XX_', 'JUST FOR TEST', 1, '_X');
				INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
												VALUES ('{codeList1PK}', '_XX_', '_X_1', '_X_1', '1900-01-01', '2079-06-06', '_X');
				INSERT INTO dbo.ZZRefCusCodeList (ZZD_PK, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_CountryOrGrouping)
									VALUES ('{codeList2PK}', '_XX_', '_X_2', '_X_2', '1900-01-01', '2079-06-06', '_X');
				INSERT INTO RefDatabase_RefCusCodeListLanguage (ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description)
														VALUES (newid(), 'ZHT', '{codeList1PK}', 'ZHT Description');
				INSERT INTO dbo.ZZRefCusCodeListLanguage (ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description, ZXA_SystemCreateTimeUtc, ZXA_SystemCreateUser, ZXA_SystemLastEditTimeUtc, ZXA_SystemLastEditUser)
														VALUES (newid(), 'IT', '{codeList2PK}', 'IT Description', '2079-06-06', '~BP', '2079-06-06', '~BP');");
			CombineAssertions(() =>
			{
				AssertSelectResult("codeList1PK RefCusCodeListLanguage", TestConnection, codeList1PK, new[] { "ZHT, ZHT Description" });
				AssertSelectResult("codeList2PK ZZRefCusCodeListLanguage", TestConnection, codeList2PK, new[] { "IT, IT Description" });
			});
		}

		void AssertSelectResult(string message, DbConnection connection, Guid codeListPK, string[] expectedResults)
		{
			var selectSql = $"SELECT * FROM dbo.ZZRefCusCodeListLanguageCombined WHERE ZXA_ZZD_CodeList = '{codeListPK}'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZXA_ZX6_NKLanguage"]}, {reader["ZXA_Description"]}");
			});
			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}
	}
}

