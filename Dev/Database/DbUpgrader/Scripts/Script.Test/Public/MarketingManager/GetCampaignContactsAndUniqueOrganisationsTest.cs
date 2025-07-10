using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Test
{
	[TestedType(typeof(GetCampaignContactsAndUniqueOrganisations))]
	class GetCampaignContactsAndUniqueOrganisationsTest : DbCreateScriptTest
	{
		public void TestReturnTrackingStatusAndTotals()
		{
			var org1PK = Guid.NewGuid();
			var org2PK = Guid.NewGuid();
			var org3PK = Guid.NewGuid();

			var campaignPK = Guid.NewGuid();
			InsertOrganisation(org1PK, "TS1", "Test Organisation 1");
			InsertOrganisation(org2PK, "TS2", "Test Organisation 2");
			InsertOrganisation(org3PK, "TS3", "Test Organisation 3");

			InsertCampaign(campaignPK, "Test Campaign One", "STG", "TST00001000");

			InsertCampaignItemForOrgContact(campaignPK, org1PK, "Alice", "UNV");
			InsertCampaignItemForOrgContact(campaignPK, org2PK, "Ben", "UNV");
			InsertCampaignItemForOrgContact(campaignPK, org3PK, "Brian", "UNV");
			InsertCampaignItemForEnquiry(campaignPK, org1PK, "TS1", "T00001077", "UNV");
			InsertCampaignItemForEnquiry(campaignPK, null, "TS1", "T00001078", "UNV");
			InsertCampaignItemForGlbStaff(campaignPK, "TS3", "UNV");
			InsertCampaignItemForHRApplicant(campaignPK, "Fred Nerk", "fnerk@a.com", "UNV");

			InsertCampaignItemForOrgContact(campaignPK, org1PK, "Cathy", "VER");
			InsertCampaignItemForOrgContact(campaignPK, org2PK, "David", "VER");

			var sql = $"SELECT * FROM GetCampaignContactsAndUniqueOrganisations('{campaignPK}', 'TYPE', 'CAT')";

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 4, result.Rows.Count);
			AssertEquals("First row tracking status", "UNV", result.Rows[0]["TrackingStatus"]);
			AssertEquals("First row emails count", 7, result.Rows[0]["EmailsCount"]);
			AssertEquals("First row organisation count", 3, result.Rows[0]["OrganisationsCount"]);

			AssertEquals("Second row tracking status", "VER", result.Rows[1]["TrackingStatus"]);
			AssertEquals("Second row emails count", 2, result.Rows[1]["EmailsCount"]);
			AssertEquals("Second row organisation count", 2, result.Rows[1]["OrganisationsCount"]);

			AssertEquals("Third row tracking status", "TOTALS", result.Rows[2]["TrackingStatus"]);
			AssertEquals("Third row emails count", 9, result.Rows[2]["EmailsCount"]);
			AssertEquals("Third row organisation count", 3, result.Rows[2]["OrganisationsCount"]);
		}

		Guid GlbCompanyPk
		{
			get
			{
				if (!glbCompanyPk.HasValue)
				{
					glbCompanyPk = Guid.NewGuid();
					var insertSql = @"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@GC_PK, 'DAN', 'AU company', 'AU', 'AUD')";
					using (var command = TestConnection.Command(insertSql))
					{
						command.AddParameterBasedOnDbColumn("@GC_PK", glbCompanyPk, GlbCompanySchema.PK);
						command.ExecuteNonQuery();
					}
				}

				return glbCompanyPk.Value;
			}
		}
		Guid? glbCompanyPk;

		void InsertOrganisation(Guid orgPK, string code, string name)
		{
			var sql = @"
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_FullName)
VALUES (@organisationPK, @code, @name)
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@organisationPK", orgPK, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@name", name, OrgHeaderSchema.OH_FullName);
				command.ExecuteNonQuery();
			}
		}

		void InsertCampaign(Guid campaignPK, string name, string stage, string campaignID)
		{
			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_Stage, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
				  VALUES (@G0_PK, @G0_GC, @G0_CampaignName, @G0_CampaignID, @G0_Stage, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@G0_PK", campaignPK, GlbCompanyCampaignSchema.PK);
				command.AddParameterBasedOnDbColumn("@G0_GC", GlbCompanyPk, GlbCompanyCampaignSchema.G0_GC);
				command.AddParameterBasedOnDbColumn("@G0_CampaignName", name, GlbCompanyCampaignSchema.G0_CampaignName);
				command.AddParameterBasedOnDbColumn("@G0_Stage", stage, GlbCompanyCampaignSchema.G0_Stage);
				command.AddParameterBasedOnDbColumn("@G0_CampaignID", campaignID, GlbCompanyCampaignSchema.G0_CampaignID);
				command.ExecuteNonQuery();
			}
		}

		void InsertCampaignItemForOrgContact(Guid campaignPK, Guid orgPK, string name, string trackingStatus)
		{
			var orgContactPK = Guid.NewGuid();
			var campaignItemPK = Guid.NewGuid();

			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName)
										  VALUES (@OC_PK, @OC_OH, @OC_ContactName)"
			))
			{
				command.AddParameterBasedOnDbColumn("@OC_PK", orgContactPK, OrgContactSchema.PK);
				command.AddParameterBasedOnDbColumn("@OC_OH", orgPK, OrgContactSchema.OC_OH);
				command.AddParameterBasedOnDbColumn("@OC_ContactName", name, OrgContactSchema.OC_ContactName);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
							  VALUES (@G8_PK, @G8_G0, @G8_RecipientTableCode, @G8_RecipientID, @G8_TrackingStatus, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@G8_PK", campaignItemPK, GlbCompanyCampaignItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@G8_G0", campaignPK, GlbCompanyCampaignItemSchema.G8_G0);
				command.AddParameterBasedOnDbColumn("@G8_RecipientTableCode", OrgContactSchema.Constants.Prefix, GlbCompanyCampaignItemSchema.G8_RecipientTableCode);
				command.AddParameterBasedOnDbColumn("@G8_RecipientID", orgContactPK, GlbCompanyCampaignItemSchema.G8_RecipientID);
				command.AddParameterBasedOnDbColumn("@G8_TrackingStatus", trackingStatus, GlbCompanyCampaignItemSchema.G8_TrackingStatus);
				command.ExecuteNonQuery();
			}
		}

		void InsertCampaignItemForEnquiry(Guid campaignPK, Guid? orgPK, string staffCode, string leadReference, string trackingStatus)
		{
			var enquiryPK = Guid.NewGuid();
			var campaignItemPK = Guid.NewGuid();

			using (var command = TestConnection.Command(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_OH_ConvertedToQualifiedLead, O1_GS_NKRepAssigned, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_OH_ConvertedToQualifiedLead, @O1_GS_NKRepAssigned, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", enquiryPK, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", leadReference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.AddParameterBasedOnDbColumn("@O1_OH_ConvertedToQualifiedLead", (orgPK != null) ? orgPK : DBNull.Value, OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead);
				command.AddParameterBasedOnDbColumn("@O1_GS_NKRepAssigned", staffCode, OrgColdCallRegisterSchema.O1_GS_NKRepAssigned);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
							  VALUES (@G8_PK, @G8_G0, @G8_RecipientTableCode, @G8_RecipientID, @G8_TrackingStatus, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@G8_PK", campaignItemPK, GlbCompanyCampaignItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@G8_G0", campaignPK, GlbCompanyCampaignItemSchema.G8_G0);
				command.AddParameterBasedOnDbColumn("@G8_RecipientTableCode", OrgColdCallRegisterSchema.Constants.Prefix, GlbCompanyCampaignItemSchema.G8_RecipientTableCode);
				command.AddParameterBasedOnDbColumn("@G8_RecipientID", enquiryPK, GlbCompanyCampaignItemSchema.G8_RecipientID);
				command.AddParameterBasedOnDbColumn("@G8_TrackingStatus", trackingStatus, GlbCompanyCampaignItemSchema.G8_TrackingStatus);
				command.ExecuteNonQuery();
			}
		}

		void InsertCampaignItemForGlbStaff(Guid campaignPK, string staffCode, string trackingStatus)
		{
			var staffPK = Guid.NewGuid();
			var campaignItemPK = Guid.NewGuid();

			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_ISResource, GS_IsSystemAccount, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES (@GS_PK, @GS_Code, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			"))
			{
				command.AddParameterBasedOnDbColumn("@GS_PK", staffPK, GlbStaffSchema.PK);
				command.AddParameterBasedOnDbColumn("@GS_Code", staffCode, GlbStaffSchema.GS_Code);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
							  VALUES (@G8_PK, @G8_G0, @G8_RecipientTableCode, @G8_RecipientID, @G8_TrackingStatus, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@G8_PK", campaignItemPK, GlbCompanyCampaignItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@G8_G0", campaignPK, GlbCompanyCampaignItemSchema.G8_G0);
				command.AddParameterBasedOnDbColumn("@G8_RecipientTableCode", GlbStaffSchema.Constants.Prefix, GlbCompanyCampaignItemSchema.G8_RecipientTableCode);
				command.AddParameterBasedOnDbColumn("@G8_RecipientID", staffPK, GlbCompanyCampaignItemSchema.G8_RecipientID);
				command.AddParameterBasedOnDbColumn("@G8_TrackingStatus", trackingStatus, GlbCompanyCampaignItemSchema.G8_TrackingStatus);
				command.ExecuteNonQuery();
			}
		}

		void InsertCampaignItemForHRApplicant(Guid campaignPK, string applicantName, string emailAddress, string trackingStatus)
		{
			var applicantPK = Guid.NewGuid();
			var personPK = Guid.NewGuid();
			var campaignItemPK = Guid.NewGuid();

			using (var command = TestConnection.Command("INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName, PER_RN_NKCountry) values(@PER_PK, @PER_FullName, 'AU')"))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPK, GlbPersonSchema.PK);
				command.AddParameterBasedOnDbColumn("@PER_FullName", applicantName, GlbPersonSchema.PER_FullName);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command("INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES (@HA_PK, @HA_EmailAddress, @HA_PER, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameterBasedOnDbColumn("@HA_PK", applicantPK, HRJobApplicantSchema.PK);
				command.AddParameterBasedOnDbColumn("@HA_EmailAddress", emailAddress, HRJobApplicantSchema.HA_EmailAddress);
				command.AddParameterBasedOnDbColumn("@HA_PER", personPK, HRJobApplicantSchema.HA_PER);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
							  VALUES (@G8_PK, @G8_G0, @G8_RecipientTableCode, @G8_RecipientID, @G8_TrackingStatus, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@G8_PK", campaignItemPK, GlbCompanyCampaignItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@G8_G0", campaignPK, GlbCompanyCampaignItemSchema.G8_G0);
				command.AddParameterBasedOnDbColumn("@G8_RecipientTableCode", HRJobApplicantSchema.Constants.Prefix, GlbCompanyCampaignItemSchema.G8_RecipientTableCode);
				command.AddParameterBasedOnDbColumn("@G8_RecipientID", applicantPK, GlbCompanyCampaignItemSchema.G8_RecipientID);
				command.AddParameterBasedOnDbColumn("@G8_TrackingStatus", trackingStatus, GlbCompanyCampaignItemSchema.G8_TrackingStatus);
				command.ExecuteNonQuery();
			}
		}
	}
}

