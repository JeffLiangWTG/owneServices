using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListLanguageCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListLanguageCombined
{
	[TestedType(typeof(trgZZRefCusCodeListLanguageCombined_Ins))]
	class trgZZRefCusCodeListLanguageCombined_Ins_Test : DbCreateScriptTest
	{
		public void TestZZRefCusCodeListLanguageCombinedInsert()
		{
			var codeListPK = Guid.NewGuid();
			var codeListLanguage1PK = Guid.NewGuid();
			var codeListLanguage2PK = Guid.NewGuid();

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
				INSERT INTO dbo.ZZRefCusCodeListLanguageCombined (ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description, ZXA_SystemCreateTimeUtc, ZXA_SystemCreateUser, ZXA_SystemLastEditTimeUtc, ZXA_SystemLastEditUser)
														VALUES ('{codeListLanguage1PK}', 'ZHT', '{codeListPK}', 'ZHT Description', '2079-06-06', '~BP', '2079-06-06', '~BP');;
				INSERT INTO dbo.ZZRefCusCodeListLanguageCombined (ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description, ZXA_SystemCreateTimeUtc, ZXA_SystemCreateUser, ZXA_SystemLastEditTimeUtc, ZXA_SystemLastEditUser)
														VALUES ('{codeListLanguage2PK}', 'IT', '{codeListPK}', 'IT Description', '2079-06-06', '~BP', '2079-06-06', '~BP');");
			CombineAssertions(() =>
			{
				AssertSelectResult("codeList1PK RefCusCodeListLanguage", TestConnection, codeListLanguage1PK, new[] { "ZHT, ZHT Description" });
				AssertSelectResult("codeList2PK ZZRefCusCodeListLanguage", TestConnection, codeListLanguage2PK, new[] { "IT, IT Description" });
			});
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

