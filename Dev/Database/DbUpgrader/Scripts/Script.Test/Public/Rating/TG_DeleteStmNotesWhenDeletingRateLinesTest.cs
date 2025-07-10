using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating
{
	[TestedType(typeof(TG_DeleteStmNotesWhenDeletingRateLines))]
	class TG_DeleteStmNotesWhenDeletingRateLinesTest : DBCreateTriggerScriptTest
	{
		public void TestDeletingRateLinesDeleteStmNotes()
		{
			var ratePK = Guid.NewGuid();
			var entryPK = Guid.NewGuid();
			var linePK = Guid.NewGuid();
			var localChargeCodePK = Guid.NewGuid();

			const string insertSql = @"
				INSERT INTO dbo.RatingHeader (TH_PK, TH_OH, TH_RateType, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
				VALUES (@RatePK, (SELECT TOP 1 OH_PK FROM dbo.OrgHeader), 'SAL', @companyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
				VALUES (@EntryPK, @RatePK, @companyPK, 'AIR', 'LSE', @TI_RateStartDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.AccChargeCode(AC_PK, AC_Code, AC_Desc, AC_GC)
				SELECT TOP 1 @LocalChargeCodePK, AC_Code, AC_Desc, @companyPK FROM dbo.AccChargeCode;

				INSERT INTO dbo.RateLines (TL_PK, TL_RateCalculator, TL_AC, TL_TI, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
				VALUES (@LinePK, 'FLT', @LocalChargeCodePK, @EntryPK, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
					
				INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Table)
				VALUES (NEWID(), @LinePK, 'RateLines');
			";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@RatePK", SqlDbType.UniqueIdentifier, ratePK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompanyPK);
				command.AddParameter("@EntryPK", SqlDbType.UniqueIdentifier, entryPK);
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, linePK);
				command.AddParameter("@LocalChargeCodePK", SqlDbType.UniqueIdentifier, localChargeCodePK);
				command.AddParameter("@TI_RateStartDate", SqlDbType.Date, DateTime.Today.AddMonths(-6));
				command.ExecuteNonQuery();
			}

			TestConnection.ExecuteNonQuery("DELETE FROM dbo.RateLines");

			var countSql = "SELECT COUNT(*) FROM dbo.StmNote WHERE ST_ParentID=@PK";
			using (var command = TestConnection.Command(countSql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, linePK);
				var stmNotesCount = (int)command.ExecuteScalar();
				AssertEquals(0, stmNotesCount);
			}
		}

		static Guid GlbCompanyPK
		{
			get
			{
				var glbCompany = new ActiveRowWrapper(GlbCompanySchema.Instance)
				{
					[GlbCompanySchema.GC_Code] = "NUU",
					[GlbCompanySchema.GC_Name] = "Company = NUU",
					[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
					[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
				};
				glbCompany.Save();

				return glbCompany.PK;
			}
		}
	}
}

