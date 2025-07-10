using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(vw_Report_SalesUnmanagedBusinessReport_NoRatesDetailed))]
	class vw_Report_SalesUnmanagedBusinessReport_NoRatesDetailedTest : DbCreateScriptTest
	{
		public void TestReportWhenTI_OriginLRCAndTI_DestinationLRCAreEmpty()
		{
			PrepareTestData();

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * From dbo.vw_Report_SalesUnmanagedBusinessReport_NoRatesDetailed");
			AssertEquals("Result should not have rows", 0, result.Rows.Count);
		}

		void PrepareTestData()
		{
			InitializeGlbDatatPK();
			InitializeOrgHeaderAndOrgAddressPK();

			var jobshipmentPK = AddJobShipment("AUSYD", "SGSIN");
			AddJobHeader(jobshipmentPK);

			var ratingHeaderPK1 = AddRatingHeader("SAL");

			AddRateEntry(ratingHeaderPK1, string.Empty, string.Empty);
		}

		#region Implementation

		void InitializeGlbDatatPK()
		{
			string sql = @"
			SELECT
				TOP(1) GB_PK, GC_PK, GE_PK
			FROM 
				dbo.Glbbranch
				JOIN dbo.GlbCompany ON GB_GC = GC_PK AND GC_RN_NKCountryCode != ''
				JOIN dbo.RefCountry ON RN_Code = GC_RN_NKCountryCode
				JOIN dbo.GlbDepartment ON GE_PK IS NOT NULL ";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					GlbBranchPK = Guid.Parse(reader["GB_PK"].ToString());
					GlbCompanyPK = Guid.Parse(reader["GC_PK"].ToString());
					GlbDepartmentPK = Guid.Parse(reader["GE_PK"].ToString());
				}
			}
		}

		void InitializeOrgHeaderAndOrgAddressPK()
		{
			string sql = @"SELECT TOP(1) OA_PK,OA_OH FROM dbo.OrgAddress WHERE OA_OH IS NOT NULL";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					OrgAddressPK = Guid.Parse(reader["OA_PK"].ToString());
					OrgHeaderPK = Guid.Parse(reader["OA_OH"].ToString());
				}
			}
		}

		Guid AddJobShipment(string origin, string destination)
		{
			Guid jobShipmentPK = Guid.NewGuid();

			string sql = @"
			INSERT INTO dbo.JobShipment
				(JS_PK, JS_IsForwardRegistered, JS_IsCancelled, JS_E_DEP , JS_RL_NKOrigin, JS_RL_NKDestination)
			VALUES
				(@jobShipmentPK, '1', '0', DATEADD(MONTH, -1, GETDATE()), @Origin, @Destination) ";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@JobShipmentPK", SqlDbType.UniqueIdentifier, jobShipmentPK);
				command.AddParameter("@Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, destination);
				command.ExecuteNonQuery();
			}

			return jobShipmentPK;
		}

		void AddJobHeader(Guid jobShipmentPK)
		{
			string sql = @"
			INSERT INTO dbo.JobHeader
				(JH_PK, JH_GB, JH_GC, JH_GE, JH_ParentID, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_Status)
			VALUES
				(NewID(), @GlbBranchPK, @GlbCompanyPK, @GlbDepartmentPK, @JobShipmentPK, @LocalChargesAddr, 'JS', 'WRK')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@GlbBranchPK", SqlDbType.UniqueIdentifier, GlbBranchPK);
				command.AddParameter("@GlbCompanyPK", SqlDbType.UniqueIdentifier, GlbCompanyPK);
				command.AddParameter("@GlbDepartmentPK", SqlDbType.UniqueIdentifier, GlbDepartmentPK);
				command.AddParameter("@JobShipmentPK", SqlDbType.UniqueIdentifier, jobShipmentPK);
				command.AddParameter("@LocalChargesAddr", SqlDbType.UniqueIdentifier, OrgAddressPK);
				command.ExecuteNonQuery();
			}
		}

		Guid AddRatingHeader(string rateType)
		{
			var ratingHeaderPK = Guid.NewGuid();

			string sql = @"
			INSERT INTO dbo.RatingHeader
				(TH_PK, TH_OH, TH_RateType, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
			VALUES
				(@RatingHeaderPK, @OrgHeaderPK, @RateType, @GlbCompanyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP') ";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@RatingHeaderPK", SqlDbType.UniqueIdentifier, ratingHeaderPK);
				command.AddParameter("@OrgHeaderPK", SqlDbType.UniqueIdentifier, OrgHeaderPK);
				command.AddParameter("@GlbCompanyPK", SqlDbType.UniqueIdentifier, GlbCompanyPK);
				command.AddParameter("@RateType", SqlDbType.Char, rateType);
				command.ExecuteNonQuery();
			}

			return ratingHeaderPK;
		}

		void AddRateEntry(Guid ratingHeaderPK, string origin, string destination)
		{
			string sql = @"
			INSERT INTO dbo.RateEntry
				(TI_PK, TI_GC_Publisher, TI_TH, TI_OriginLRC, TI_DestinationLRC, TI_RateStartDate, TI_RateEndDate, TI_RateCategory, TI_Mode, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
			VALUES 
				(NEWID(), @GlbCompanyPK, @RatingHeaderPK, @Origin, @Destination, @StartDate, NULL, 'AIR', 'LSE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@RatingHeaderPK", SqlDbType.UniqueIdentifier, ratingHeaderPK);
				command.AddParameter("@GlbCompanyPK", SqlDbType.UniqueIdentifier, GlbCompanyPK);
				command.AddParameter("@Origin", SqlDbType.VarChar, RateEntrySchema.TI_OriginLRC.MaxLength, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, RateEntrySchema.TI_DestinationLRC.MaxLength, destination);
				command.AddParameter("@StartDate", SqlDbType.Date, DateTime.Now.AddMonths(-6));
				command.ExecuteNonQuery();
			}
		}

		Guid OrgHeaderPK;
		Guid OrgAddressPK;
		Guid GlbCompanyPK;
		Guid GlbBranchPK;
		Guid GlbDepartmentPK;
		#endregion
	}
}

