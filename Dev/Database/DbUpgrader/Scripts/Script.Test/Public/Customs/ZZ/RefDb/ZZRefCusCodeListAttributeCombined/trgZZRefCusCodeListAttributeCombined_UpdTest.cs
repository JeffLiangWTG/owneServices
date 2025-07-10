using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListAttributeCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListAttributeCombined
{
	[TestedType(typeof(trgZZRefCusCodeListAttributeCombined_Upd))]
	class trgZZRefCusCodeListAttributeCombined_Upd_Test : DbCreateScriptTest
	{
		[TestDate(2024, 11, 06)]
		public void TestUpdateStartEndDate()
		{
			var zzdPK = new Guid("E28030AF-9F72-4C1E-BE4C-C3D9AD3C5F2F");
			var zzePK = new Guid("B820DF9C-CFDD-4D74-9C52-F2202D793048");
			var today = DateTime.Today;
			var tomorrow = today.AddDays(1);
			var afterTomorrow = tomorrow.AddDays(1);

			var insert = $@"
	INSERT INTO dbo.ZZRefCusCodeList
	([ZZD_PK], [ZZD_CodeType], [ZZD_Code], [ZZD_StartDate], [ZZD_EndDate], [ZZD_CountryOrGrouping], [ZZD_Description], [ZZD_SystemCreateTimeUtc], [ZZD_SystemCreateUser], [ZZD_SystemLastEditTimeUtc], [ZZD_SystemLastEditUser])
	VALUES ('{zzdPK}', 'TYPE', 'CODE', GETDATE(), DATEADD(DAY,1,GETDATE()), 'AU', '', GETDATE(), '~bp', GETDATE(), '~bp');

	INSERT INTO dbo.ZZRefCusCodeListAttributeCombined
	([ZZE_PK], [ZZE_ZZD_CodeList], [ZZE_ZXE_NKName], [ZZE_Value], [ZZE_IsAir], [ZZE_IsSea], [ZZE_IsFix], [ZZE_IsRai], [ZZE_IsRoa], [ZZE_IsMai], [ZZE_IsInw], [ZZE_StartDate], [ZZE_EndDate])
	VALUES ('{zzePK}', '{zzdPK}', 'AU', 'Australia', 1, 0, 0, 0, 0, 0, 0, @StartDate, @EndDate)";

			TestConnection.ExecuteNonQuery(insert, command =>
			{
				command.AddParameter("@StartDate", System.Data.SqlDbType.DateTime, today);
				command.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, tomorrow);
			});

			var update = $"UPDATE dbo.ZZRefCusCodeListAttributeCombined SET [ZZE_StartDate] = @StartDate, [ZZE_EndDate] = @EndDate WHERE ZZE_PK='{zzePK}'";
			TestConnection.ExecuteNonQuery(update, command =>
			{
				command.AddParameter("@StartDate", System.Data.SqlDbType.DateTime, tomorrow);
				command.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, afterTomorrow);
			});

			var select = $"SELECT ZZE_StartDate, ZZE_EndDate FROM dbo.ZZRefCusCodeListAttribute WHERE ZZE_PK='{zzePK}'";
			var cmd = TestConnection.Command(select);
			using var reader = cmd.ExecuteReader();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => reader.Read());
				AssertEquals(tomorrow, reader["ZZE_StartDate"]);
				AssertEquals(afterTomorrow, reader["ZZE_EndDate"]);
			});
		}
	}
}

