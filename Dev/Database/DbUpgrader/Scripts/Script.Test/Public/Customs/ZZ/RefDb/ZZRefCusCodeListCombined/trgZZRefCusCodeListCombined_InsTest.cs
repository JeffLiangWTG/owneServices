using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListCombined
{
	[TestedType(typeof(trgZZRefCusCodeListCombined_Ins))]
	class trgZZRefCusCodeListCombined_Ins_Test : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var pk = new Guid("52C11A9D-131B-4061-B79E-B69181F24074");
			var startDate = DateTime.Today;
			var endDate = startDate.AddDays(1);
			var insert = $@"
	INSERT INTO dbo.ZZRefCusCodeListCombined
	([ZZD_PK], [ZZD_CountryOrGrouping], [ZZD_CodeType], [ZZD_Code], [ZZD_Description], [ZZD_StartDate], [ZZD_EndDate], [ZZD_IsAir], [ZZD_IsSea], [ZZD_IsFix], [ZZD_IsRai], [ZZD_IsRoa], [ZZD_IsMai], [ZZD_IsInw])
	VALUES ('{pk}', 'AU', 'TYPE', 'CODE', 'DESCRIPTION', '{startDate:yyyy-MM-dd}', '{endDate:yyyy-MM-dd}', 1, 0, 0, 0, 0, 0, 0)";

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insert));

				var select = $@"SELECT ZZD_CountryOrGrouping, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate FROM dbo.ZZRefCusCodeList where ZZD_PK='{pk}'";
				var cmd = TestConnection.Command(select);
				using (var reader = cmd.ExecuteReader())
				{
					AssertNoExceptionThrown(() => reader.Read());
					AssertEquals("AU", reader["ZZD_CountryOrGrouping"]);
					AssertEquals("TYPE", reader["ZZD_CodeType"]);
					AssertEquals("CODE", reader["ZZD_Code"]);
					AssertEquals("DESCRIPTION", reader["ZZD_Description"]);
					AssertEquals(startDate, reader["ZZD_StartDate"]);
					AssertEquals(endDate, reader["ZZD_EndDate"]);
				}

				select = $@"SELECT XA_PK, XA_Name, XA_Type, XA_Data FROM dbo.GenAddOnColumn where XA_ParentID='{pk}' and XA_ParentTableCode='ZZD'";
				cmd = TestConnection.Command(select);
				using (var reader = cmd.ExecuteReader())
				{
					AssertNoExceptionThrown(() => reader.Read());
					AssertNotNull(reader["XA_PK"]);
					AssertEquals("AIR", reader["XA_Name"]);
					AssertEquals("BOO", reader["XA_Type"]);
					AssertEquals("Y", reader["XA_Data"]);
				}
			});
		}
	}
}
