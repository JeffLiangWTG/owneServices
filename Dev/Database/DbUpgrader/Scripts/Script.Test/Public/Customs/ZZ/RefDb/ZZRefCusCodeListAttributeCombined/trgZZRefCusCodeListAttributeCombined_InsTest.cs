using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListAttributeCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListAttributeCombined
{
	[TestedType(typeof(trgZZRefCusCodeListAttributeCombined_Ins))]
	class trgZZRefCusCodeListAttributeCombined_Ins_Test : DbCreateScriptTest
	{
		[TestDate(2024, 11,06)]
		public void TestInsert()
		{
			var zzdPK = new Guid("E28030AF-9F72-4C1E-BE4C-C3D9AD3C5F2F");
			var zzePK = new Guid("B820DF9C-CFDD-4D74-9C52-F2202D793048");
			var today = DateTime.Today;
			var tomorrow = today.AddDays(1);

			var insert = $@"
	INSERT INTO dbo.ZZRefCusCodeList
	([ZZD_PK], [ZZD_CodeType], [ZZD_Code], [ZZD_StartDate], [ZZD_EndDate], [ZZD_CountryOrGrouping], [ZZD_Description], [ZZD_SystemCreateTimeUtc], [ZZD_SystemCreateUser], [ZZD_SystemLastEditTimeUtc], [ZZD_SystemLastEditUser])
	VALUES ('{zzdPK}', 'TYPE', 'CODE', GETDATE(), DATEADD(DAY,1,GETDATE()), 'AU', '', GETDATE(), '~bp', GETDATE(), '~bp');

	INSERT INTO dbo.ZZRefCusCodeListAttributeCombined
	([ZZE_PK], [ZZE_ZZD_CodeList], [ZZE_ZXE_NKName], [ZZE_Value], [ZZE_IsAir], [ZZE_IsSea], [ZZE_IsFix], [ZZE_IsRai], [ZZE_IsRoa], [ZZE_IsMai], [ZZE_IsInw], [ZZE_StartDate], [ZZE_EndDate])
	VALUES ('{zzePK}', '{zzdPK}', 'AU', 'Australia', 1, 0, 0, 0, 0, 0, 0, @StartDate, @EndDate)";

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insert, command =>
				{
					command.AddParameter("@StartDate", System.Data.SqlDbType.DateTime, today);
					command.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, tomorrow);
				}));

				var select = $"SELECT ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate FROM dbo.ZZRefCusCodeListAttribute where ZZE_PK='{zzePK}'";
				var cmd = TestConnection.Command(select);
				using (var reader = cmd.ExecuteReader())
				{
					AssertNoExceptionThrown(() => reader.Read());
					AssertEquals(zzdPK, reader["ZZE_ZZD_CodeList"]);
					AssertEquals("AU", reader["ZZE_ZXE_NKName"]);
					AssertEquals("Australia", reader["ZZE_Value"]);
					AssertEquals(today, reader["ZZE_StartDate"]);
					AssertEquals(tomorrow, reader["ZZE_EndDate"]);
				}

				select = $"SELECT XA_PK, XA_Name, XA_Type, XA_Data FROM dbo.GenAddOnColumn where XA_ParentID='{zzePK}' and XA_ParentTableCode='ZZE'";
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

