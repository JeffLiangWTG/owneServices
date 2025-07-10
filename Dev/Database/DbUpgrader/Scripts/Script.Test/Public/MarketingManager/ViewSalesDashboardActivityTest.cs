using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(ViewSalesDashboardActivity))]
	class ViewSalesDashboardActivityTest : DbCreateScriptTest
	{
		#region ActivityDate

		public void TestActivityDate_Opportunity()
		{
			var orgPk = Guid.NewGuid();
			InsertOrg(orgPk, "AAA");

			var oppWithAllDates = NewOrgOpportunityWithDates(orgPk, new DateTime(2012, 10, 10), new DateTime(2013, 1, 1), new DateTime(2013, 1, 2), "OppId1");
			var oppWithNoClosedDate = NewOrgOpportunityWithDates(orgPk, new DateTime(2012, 10, 10), new DateTime(2013, 1, 1), null, "OppId2");
			var oppWithNoEstimatedDate = NewOrgOpportunityWithDates(orgPk, new DateTime(2012, 10, 10), null, new DateTime(2013, 1, 2), "OppId3");
			var oppWithOnlyCreatedDate = NewOrgOpportunityWithDates(orgPk, new DateTime(2012, 10, 10), null, null, "OppId4");
			var oppWithProcessTaskDate = NewOrgOpportunityWithRelatedProcessTask(orgPk, new DateTime(2012, 10, 11), "OppId5");

			AssertActivityDate("Should be closed date", oppWithAllDates, new DateTime(2013, 1, 2));
			AssertActivityDate("Should be closed date", oppWithNoEstimatedDate, new DateTime(2013, 1, 2));
			AssertActivityDate("Should be estimate close date", oppWithNoClosedDate, new DateTime(2013, 1, 1));
			AssertActivityDate("Should be created date", oppWithOnlyCreatedDate, new DateTime(2012, 10, 10));
			AssertActivityDate("Should be process task date", oppWithProcessTaskDate, new DateTime(2012, 10, 11));
		}

		public void TestActivityDate_Inquiry()
		{
			var inquiryWithAllDates = NewSalesInquiryWithDates("I00000001", new DateTime(2012, 10, 10), new DateTime(2013, 1, 1));
			var inquiryWithOnlyCreatedDate = NewSalesInquiryWithDates("I00000002", new DateTime(2012, 10, 10), null);

			AssertActivityDate("Should be call date", inquiryWithAllDates, new DateTime(2013, 1, 1));
			AssertActivityDate("Should be created date", inquiryWithOnlyCreatedDate, new DateTime(2012, 10, 10));
		}

		public void TestActivityDate_Communication()
		{
			var orgPk = Guid.NewGuid();
			InsertOrg(orgPk, "AAA");
			var commWithAllDates = NewCommunicationWithDates("CM00000001", orgPk, new DateTime(2012, 10, 10), new DateTime(2013, 1, 1), new DateTime(2013, 1, 2));
			var commWithNoActualDate = NewCommunicationWithDates("CM00000002", orgPk, new DateTime(2012, 10, 10), new DateTime(2013, 1, 1), null);
			var commWithNoScheduledDate = NewCommunicationWithDates("CM00000003", orgPk, new DateTime(2012, 10, 10), null, new DateTime(2013, 1, 2));
			var commWithOnlyCreatedDate = NewCommunicationWithDates("CM00000004", orgPk, new DateTime(2012, 10, 10), null, null);

			AssertActivityDate("Should be actual date", commWithAllDates, new DateTime(2013, 1, 2));
			AssertActivityDate("Should be actual date", commWithNoScheduledDate, new DateTime(2013, 1, 2));
			AssertActivityDate("Should be scheduled date", commWithNoActualDate, new DateTime(2013, 1, 1));
			AssertActivityDate("Should be created date", commWithOnlyCreatedDate, new DateTime(2012, 10, 10));
		}

		public void TestQuotedBooking_QTE()
		{
			Guid treeId = Guid.NewGuid();
			Guid ratingPk1 = Guid.NewGuid();
			Guid ratingPk2 = Guid.NewGuid();

			DateTime date1 = new DateTime(2017, 08, 10, 23, 15, 0, DateTimeKind.Unspecified);
			DateTime date2 = new DateTime(2017, 09, 10, 23, 15, 0, DateTimeKind.Unspecified);

			InsertRatingHeader(ratingPk1, date1);
			InsertRatingHeader(ratingPk2, date2);

			InsertPivot(treeId, RatingHeaderSchema.Constants.Prefix, ratingPk1, RatingHeaderSchema.Constants.Prefix, ratingPk2);

			AssertEquals(date2, GetSalesRelationsLastEditUtc(ratingPk1));
			AssertEquals(date2, GetSalesRelationsLastEditUtc(ratingPk2));
		}

		public void TestViewSalesDashboardActivity_OneOffQuote()
		{
			var pk = Guid.NewGuid();
			InsertRatingHeader(pk, DateTime.Today);

			const string status = "ASN";
			const string staffMemberCode = "UAS";
			const string taskType = "UDF";
			const string taskDescription = "Test Description ASN";
			const string insertQuery = @"INSERT INTO dbo.ProcessTasks (P9_PK, P9_ParentID, P9_Status, P9_GS_NKAssignedStaffMember, P9_Description, P9_Type, P9_CompletedTimeUtc, P9_ParentTableCode)
			VALUES (@P9_PK, @P9_ParentID, @P9_Status, @P9_GS_NKAssignedStaffMember, @P9_Description, @P9_Type, @P9_CompletedTimeUtc, 'VB')";

			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@P9_PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@P9_ParentID", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@P9_Status", SqlDbType.NVarChar, status);
				command.AddParameter("@P9_GS_NKAssignedStaffMember", SqlDbType.NVarChar, staffMemberCode);
				command.AddParameter("@P9_Type", SqlDbType.NVarChar, taskType);
				command.AddParameter("@P9_Description", SqlDbType.NVarChar, taskDescription);
				command.AddParameter("@P9_CompletedTimeUtc", SqlDbType.DateTime, DateTime.Today);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command("SELECT * FROM dbo.ViewSalesDashboardActivity WHERE VSA_ParentID = @VSA_ParentID"))
			{
				command.AddParameterBasedOnDbColumn("@VSA_ParentID", pk, ViewSalesDashboardActivitySchema.VSA_ParentId);

				var result = new DataTable();
				result.Load(command.ExecuteReader());

				var row = result.Rows[0];
				AssertEquals(taskType, row["VSA_TaskType"].ToString());
				AssertEquals(taskDescription, row["VSA_TaskDescription"].ToString());
				AssertEquals(status, row["VSA_Status"].ToString());
				AssertEquals(staffMemberCode, row["VSA_GS_NKAssignedStaff"].ToString());
			}
		}

		DateTime? GetSalesRelationsLastEditUtc(Guid vsaPk)
		{
			using (var command = TestConnection.Command(
				@"SELECT VSA_SalesRelationsLastEditUtc FROM dbo.ViewSalesDashboardActivity WHERE VSA_PK=@VSA_PK"
			))
			{
				command.AddParameterBasedOnDbColumn("@VSA_PK", vsaPk, ViewSalesDashboardActivitySchema.PK);
				return (DateTime?)command.ExecuteScalar();
			}
		}

		void AssertActivityDate(string message, Guid pk, DateTime expectedDateTime)
		{
			var selectSql = "SELECT VSA_PK, VSA_ActivityDate FROM dbo.ViewSalesDashboardActivity WHERE VSA_PK = '" + pk + "'";
			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, selectSql))
			{
				AssertEquals("Activity should exist", 1, result.Rows.Count);
				AssertEquals(message, expectedDateTime, result.Rows[0]["VSA_ActivityDate"]);
			}
		}

		#endregion

		public void TestViewLastEditTimeAndUserOfOpportunities()
		{
			Guid orgPk = Guid.NewGuid();
			InsertOrg(orgPk, "ARF");
			InsertOrgOpportunityWithLastEdit(orgPk, new DateTime(2013, 06, 18, 11, 16, 17), "E");

			using (DbCommand command = TestConnection.Command("SELECT * FROM dbo.ViewSalesDashboardActivity"))
			{
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Pre-condition: should return a result", 1, result.Rows.Count);
				AssertEquals("P8_SystemLastEditUser", "E", result.Rows[0]["VSA_SystemLastEditUser"].ToString());
				AssertEquals("P8_SystemLastEditTimeUtc", "18/06/2013 11:16:00 AM", result.Rows[0]["VSA_SystemLastEditTimeUtc"].ToString());
			}
		}

		public void TestCommunicationWithNoOrgSet()
		{
			var salesInquiry = NewSalesInquiryWithDates("salesInquiryRef12", DateTime.Now, DateTime.Now);
			TestConnection.ExecuteNonQuery($@"
UPDATE dbo.OrgColdCallRegister SET O1_CompanyName = 'NoOrgName23', O1_ContactName = 'NoContactName34', O1_SystemLastEditTimeUtc = GETUTCDATE(), O1_SystemLastEditUser = 'E' WHERE O1_PK = '{salesInquiry}';");

			var communication = NewCommunicationWithDates("CM00000045", null, DateTime.Now, null, null);

			var linkSql = $@"
INSERT INTO dbo.RelatedActivityPivot (RAP_PK, RAP_ChildActivityID, RAP_ChildActivityTableCode, RAP_ParentActivityID, RAP_ParentActivityTableCode, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser) 
VALUES(NEWID(), '{communication}', 'OQ', '{salesInquiry}', 'O1', GetUtcDate(), 'E', GetUtcDate(), 'E')";

			TestConnection.ExecuteNonQuery(linkSql);

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.ViewSalesDashboardActivity WHERE VSA_PK = '{communication}'"))
			{
				AssertEquals(1, result.Rows.Count);
				var row = result.Rows[0];

				AssertEquals("COM", row["VSA_ActivityType"]);
				AssertEquals("CM00000045", row["VSA_ActivityParentID"]);
				AssertEquals(DBNull.Value, row["VSA_OH"]);
				AssertEquals(DBNull.Value, row["VSA_OrgCode"]);
				AssertEquals("NoOrgName23", row["VSA_OrgFullName"]);
				AssertEquals("NoContactName34", row["VSA_ActivityContactName"]);
			}
		}

		public void TestStatusForOneOffQuoteWithShipment()
		{
			var ratingHeaderPk = Guid.NewGuid();
			var rateOneOffShipmentPk = Guid.NewGuid();
			var shipmentPk = Guid.NewGuid();

			InsertOneOffQuoteWithShipment(ratingHeaderPk, rateOneOffShipmentPk, shipmentPk);

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.ViewSalesDashboardActivity WHERE VSA_PK = '{rateOneOffShipmentPk}'"))
			{
				AssertEquals(1, result.Rows.Count);
				var row = result.Rows[0];

				AssertEquals("BKD", row["VSA_ActivityStatus"]);
			}

			UpdateOneOffQuoteWithShipment(ratingHeaderPk, false, true);

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.ViewSalesDashboardActivity WHERE VSA_PK = '{rateOneOffShipmentPk}'"))
			{
				AssertEquals(1, result.Rows.Count);
				var row = result.Rows[0];

				AssertEquals("SHP", row["VSA_ActivityStatus"]);
			}

			UpdateOneOffQuoteWithShipment(ratingHeaderPk, true, false);

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.ViewSalesDashboardActivity WHERE VSA_PK = '{rateOneOffShipmentPk}'"))
			{
				AssertEquals(1, result.Rows.Count);
				var row = result.Rows[0];

				AssertEquals("CAN", row["VSA_ActivityStatus"]);
			}
		}

		public void TestActivityStage()
		{
			var orgPK = Guid.NewGuid();
			InsertOrg(orgPK, "TESTORG");

			var campaignItemPK = Guid.NewGuid();
			var campaignPK = Guid.NewGuid();
			InsertCampaignWithCampaignItemAndStage(campaignPK, campaignItemPK, orgPK, "Unit Test Campaign", "G01", "TST00001000");

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.ViewSalesDashboardActivity WHERE VSA_PK = '{campaignItemPK}'"))
			{
				AssertEquals(1, result.Rows.Count);
				var row = result.Rows[0];

				AssertEquals("G01", row["VSA_ActivityStage"]);
			}

			var opportunityPK = Guid.NewGuid();
			InsertNewOpportunityWithStage(opportunityPK, orgPK, "O00001000", "P81");

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.ViewSalesDashboardActivity WHERE VSA_PK = '{opportunityPK}'"))
			{
				AssertEquals(1, result.Rows.Count);
				var row = result.Rows[0];

				AssertEquals("P81", row["VSA_ActivityStage"]);
			}

			var ratingHeaderPk = Guid.NewGuid();
			var rateOneOffShipmentPk = Guid.NewGuid();
			var shipmentPk = Guid.NewGuid();

			InsertOneOffQuoteWithShipment(ratingHeaderPk, rateOneOffShipmentPk, shipmentPk);

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, $@"select * from dbo.ViewSalesDashboardActivity WHERE VSA_PK = '{rateOneOffShipmentPk}'"))
			{
				AssertEquals(1, result.Rows.Count);
				var row = result.Rows[0];

				AssertEquals(string.Empty, row["VSA_ActivityStage"]);
			}
		}

		public void TestActivityProjectWithoutContact()
		{
			var orgPk = Guid.NewGuid();
			var orgAddressPk = Guid.NewGuid();
			var orgContactPk = Guid.NewGuid();

			InsertOrg(orgPk, "TestOrg");
			InsertNewOrgAddress(orgAddressPk, orgPk);
			InsertNewOrgContact(orgContactPk, orgPk);

			InsertNewWorkProject("PRJ00001001", orgAddressPk);
			InsertNewWorkProject("PRJ00001002", orgAddressPk);
			InsertNewWorkProject("PRJ00001003", orgAddressPk);
			InsertNewWorkProject("PRJ00001004", orgAddressPk, orgContactPk);

			using (var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSalesDashboardActivity WHERE VSA_ActivityType = 'PRJ' ORDER BY VSA_ActivityParentID"))
			{
				AssertEquals(4, result.Rows.Count);
				AssertEquals("PRJ00001001", result.Rows[0]["VSA_ActivityParentID"]);
				AssertEquals("PRJ00001002", result.Rows[1]["VSA_ActivityParentID"]);
				AssertEquals("PRJ00001003", result.Rows[2]["VSA_ActivityParentID"]);
				AssertEquals("PRJ00001004", result.Rows[3]["VSA_ActivityParentID"]);
			}
		}

		public void TestQuotationsOrganisationAndContact()
		{
			var org1Pk = Guid.NewGuid();
			var org2Pk = Guid.NewGuid();
			var orgAddress1Pk = Guid.NewGuid();
			var orgAddress2Pk = Guid.NewGuid();
			var ratingHeaderPk = Guid.NewGuid();
			InsertOrg(org1Pk, "TES001");
			InsertNewOrgAddress(orgAddress1Pk, org1Pk);
			InsertOrg(org2Pk, "TES002");
			InsertNewOrgAddress(orgAddress2Pk, org2Pk);

			InsertRatingHeader(ratingHeaderPk, new DateTime(2020, 1, 1));

			var jobDocAddress1Pk = new Guid("6a50a96a-f4f2-4214-9103-b76304cde01f");
			var jobDocAddress2Pk = new Guid("42538ca1-4d5e-43c7-b538-28fea7d68362");

			InsertJobDocAddressForRatingHeader(jobDocAddress1Pk, "1 GEORGE SREET", "SYDNEY", "NSW", "2000", "AU", "FRED NERK", "LCA", orgAddress1Pk, ratingHeaderPk);
			InsertJobDocAddressForRatingHeader(jobDocAddress2Pk, "10 GEORGE SREET", "SYDNEY", "NSW", "2000", "AU", "AMANDA NERK", "RFA", orgAddress2Pk, ratingHeaderPk);

			using (var command = TestConnection.Command("SELECT * FROM dbo.ViewSalesDashboardActivity WHERE VSA_PK = @VSA_PK"))
			{
				command.AddParameter("@VSA_PK", SqlDbType.UniqueIdentifier, ratingHeaderPk);

				var result = new DataTable();
				result.Load(command.ExecuteReader());

				AssertEquals("Pre-condition: should return a result", 1, result.Rows.Count);
				AssertEquals("VSA_OrgCode", "TES001", result.Rows[0]["VSA_OrgCode"].ToString());
				AssertEquals("VSA_ActivityContactName", string.Empty, result.Rows[0]["VSA_ActivityContactName"].ToString());
			}
		}

		public void TestOneOffQuoteOrganisationAndContact()
		{
			var org1Pk = Guid.NewGuid();
			var org2Pk = Guid.NewGuid();
			var orgAddress1Pk = Guid.NewGuid();
			var orgAddress2Pk = Guid.NewGuid();
			var ratingHeaderPk = Guid.NewGuid();
			var rateOneOffShipmentPk = Guid.NewGuid();
			var shipmentPk = Guid.NewGuid();
			InsertOrg(org1Pk, "TES001");
			InsertNewOrgAddress(orgAddress1Pk, org1Pk);
			InsertOrg(org2Pk, "TES002");
			InsertNewOrgAddress(orgAddress2Pk, org2Pk);

			InsertOneOffQuoteWithShipment(ratingHeaderPk, rateOneOffShipmentPk, shipmentPk);

			var jobDocAddress1Pk = new Guid("6a50a96a-f4f2-4214-9103-b76304cde01f");
			var jobDocAddress2Pk = new Guid("42538ca1-4d5e-43c7-b538-28fea7d68362");

			InsertJobDocAddressForRatingHeader(jobDocAddress1Pk, "1 GEORGE SREET", "SYDNEY", "NSW", "2000", "AU", "FRED NERK", "LCA", orgAddress1Pk, ratingHeaderPk);
			InsertJobDocAddressForRatingHeader(jobDocAddress2Pk, "10 GEORGE SREET", "SYDNEY", "NSW", "2000", "AU", "AMANDA NERK", "RFA", orgAddress2Pk, ratingHeaderPk);

			using (var command = TestConnection.Command("SELECT * FROM dbo.ViewSalesDashboardActivity WHERE VSA_PK = @VSA_PK"))
			{
				command.AddParameter("@VSA_PK", SqlDbType.UniqueIdentifier, rateOneOffShipmentPk);

				var result = new DataTable();
				result.Load(command.ExecuteReader());

				AssertEquals("Pre-condition: should return a result", 1, result.Rows.Count);
				AssertEquals("VSA_OrgCode", "TES001", result.Rows[0]["VSA_OrgCode"].ToString());
				AssertEquals("VSA_ActivityContactName", "FRED NERK", result.Rows[0]["VSA_ActivityContactName"].ToString());
			}
		}

		#region Implementation

		void InsertOrg(Guid pk, string code)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}) VALUES ",
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.PK.Name,
				OrgHeaderSchema.OH_Code.Name);

			using (DbCommand command = TestConnection.Command(query + "(@OH_PK, @OH_Code)"))
			{
				command.AddParameter("@OH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@OH_Code", SqlDbType.VarChar, code);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgOpportunityWithLastEdit(Guid orgPk, DateTime lastEditTime, string lastEditUser)
		{
			var query = @"
				INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_OpportunityID, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
					VALUES (newid(), @P8_GC, @P8_OH, 'OppId', GetUtcDate(), 'E', @P8_SystemLastEditTimeUtc, @P8_SystemLastEditUser)";

			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameter("@P8_OH", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@P8_GC", SqlDbType.UniqueIdentifier, GlbCompanyPk);
				command.AddParameter("@P8_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, lastEditTime);
				command.AddParameter("@P8_SystemLastEditUser", SqlDbType.VarChar, lastEditUser);
				command.ExecuteNonQuery();
			}
		}

		Guid NewOrgOpportunityWithDates(Guid orgPk, DateTime createdTime, DateTime? estimatedCloseDate, DateTime? closedDate, string opportunityId)
		{
			var insertQuery = @"
				INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_EstimatedCloseDate, P8_ClosedDate, P8_OpportunityID, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
					VALUES (@P8_PK, @P8_GC, @P8_OH, @P8_EstimatedCloseDate, @P8_ClosedDate, @P8_OpportunityId, @P8_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')";

			var opportunityPk = Guid.NewGuid();
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@P8_PK", SqlDbType.UniqueIdentifier, opportunityPk);
				command.AddParameter("@P8_GC", SqlDbType.UniqueIdentifier, GlbCompanyPk);
				command.AddParameter("@P8_OH", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@P8_EstimatedCloseDate", SqlDbType.SmallDateTime, (object)estimatedCloseDate ?? DBNull.Value);
				command.AddParameter("@P8_ClosedDate", SqlDbType.SmallDateTime, (object)closedDate ?? DBNull.Value);
				command.AddParameter("@P8_OpportunityId", SqlDbType.VarChar, opportunityId);
				command.AddParameter("@P8_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createdTime);
				command.ExecuteNonQuery();
			}

			return opportunityPk;
		}

		Guid NewOrgOpportunityWithRelatedProcessTask(Guid orgPk, DateTime scheduledDateUTC, string opportunityId)
		{
			var insertQuery = @"
				INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_OpportunityID, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
					VALUES (@P8_PK, @P8_GC, @P8_OH, @P8_OpportunityId, @P8_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')";

			var opportunityPk = Guid.NewGuid();
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@P8_PK", SqlDbType.UniqueIdentifier, opportunityPk);
				command.AddParameter("@P8_GC", SqlDbType.UniqueIdentifier, GlbCompanyPk);
				command.AddParameter("@P8_OH", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@P8_OpportunityId", SqlDbType.VarChar, opportunityId);
				command.AddParameter("@P8_SystemCreateTimeUtc", SqlDbType.SmallDateTime, scheduledDateUTC.AddDays(-10));
				command.ExecuteNonQuery();
			}

			insertQuery = @"
				INSERT INTO dbo.ProcessTasks (P9_PK, P9_ParentID, P9_ScheduledDateUtc, P9_Status, P9_Type, P9_ParentTableCode)
					VALUES (@P9_PK, @P9_ParentID, @P9_ScheduledDateUtc, 'ASN', 'UDF', 'P8')";
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@P9_PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@P9_ParentID", SqlDbType.UniqueIdentifier, opportunityPk);
				command.AddParameter("@P9_ScheduledDateUtc", SqlDbType.SmallDateTime, scheduledDateUTC);
				command.ExecuteNonQuery();
			}

			return opportunityPk;
		}

		Guid NewSalesInquiryWithDates(string reference, DateTime createdTime, DateTime? callDate)
		{
			var insertQuery = string.Format(@"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7})
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_LeadCalledDate, @O1_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')",
				OrgColdCallRegisterSchema.Constants.TableName,
				OrgColdCallRegisterSchema.Constants.PK,
				OrgColdCallRegisterSchema.Constants.O1_LeadUniqueReference,
				OrgColdCallRegisterSchema.Constants.O1_LeadCalledDate,
				OrgColdCallRegisterSchema.Constants.O1_SystemCreateTimeUtc,
				OrgColdCallRegisterSchema.Constants.O1_SystemCreateUser,
				OrgColdCallRegisterSchema.Constants.O1_SystemLastEditTimeUtc,
				OrgColdCallRegisterSchema.Constants.O1_SystemLastEditUser);

			var inquiryPk = Guid.NewGuid();
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameter("@O1_PK", SqlDbType.UniqueIdentifier, inquiryPk);
				command.AddParameter("@O1_LeadUniqueReference", SqlDbType.VarChar, reference);
				command.AddParameter("@O1_LeadCalledDate", SqlDbType.SmallDateTime, (object)callDate ?? DBNull.Value);
				command.AddParameter("@O1_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createdTime);
				command.ExecuteNonQuery();
			}

			return inquiryPk;
		}

		Guid NewCommunicationWithDates(string id, Guid? orgPk, DateTime createdTime, DateTime? scheduledDate, DateTime? actualDate)
		{
			var insertCommunication = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})" +
				"VALUES (@OQ_PK, @OQ_CommunicationID, @OQ_OH, @OQ_NextCall, @OQ_CallDate, @OQ_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')",
				OrgSalesCallSchema.Constants.TableName,
				OrgSalesCallSchema.Constants.PK,
				OrgSalesCallSchema.Constants.OQ_CommunicationID,
				OrgSalesCallSchema.Constants.OQ_OH,
				OrgSalesCallSchema.Constants.OQ_NextCall,
				OrgSalesCallSchema.Constants.OQ_CallDate,
				OrgSalesCallSchema.Constants.OQ_SystemCreateTimeUtc,
				OrgSalesCallSchema.Constants.OQ_SystemCreateUser,
				OrgSalesCallSchema.Constants.OQ_SystemLastEditTimeUtc,
				OrgSalesCallSchema.Constants.OQ_SystemLastEditUser);

			var communicationPk = Guid.NewGuid();
			using (var command = TestConnection.Command(insertCommunication))
			{
				command.AddParameter("@OQ_PK", SqlDbType.UniqueIdentifier, communicationPk);
				command.AddParameter("@OQ_CommunicationID", SqlDbType.VarChar, id);
				command.AddParameter("@OQ_OH", SqlDbType.UniqueIdentifier, (object)orgPk ?? DBNull.Value);
				command.AddParameter("@OQ_NextCall", SqlDbType.SmallDateTime, (object)scheduledDate ?? DBNull.Value);
				command.AddParameter("@OQ_CallDate", SqlDbType.SmallDateTime, (object)actualDate ?? DBNull.Value);
				command.AddParameter("@OQ_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createdTime);
				command.ExecuteNonQuery();
			}

			return communicationPk;
		}

		public void InsertNewOpportunityWithStage(Guid pk, Guid orgPK, string opportunityID, string stage)
		{
			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_OpportunityID, P8_Stage, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
							  VALUES (@P8_PK, @P8_GC, @P8_OH, @P8_OpportunityID, @P8_Stage, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@P8_PK", pk, OrgOpportunitySchema.PK);
				command.AddParameterBasedOnDbColumn("@P8_GC", GlbCompanyPk, OrgOpportunitySchema.P8_GC);
				command.AddParameterBasedOnDbColumn("@P8_OH", orgPK, OrgOpportunitySchema.P8_OH);
				command.AddParameterBasedOnDbColumn("@P8_OpportunityID", opportunityID, OrgOpportunitySchema.P8_OpportunityID);
				command.AddParameterBasedOnDbColumn("@P8_Stage", stage, OrgOpportunitySchema.P8_Stage);
				command.ExecuteNonQuery();
			}
		}

		void InsertCampaignWithCampaignItemAndStage(Guid campaignPK, Guid campaignItemPK, Guid orgPK, string name, string stage, string campaignID)
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

			var orgContactPK = Guid.NewGuid();

			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.OrgContact (OC_PK, OC_OH)
										  VALUES (@OC_PK, @OC_OH)"
			))
			{
				command.AddParameterBasedOnDbColumn("@OC_PK", orgContactPK, OrgContactSchema.PK);
				command.AddParameterBasedOnDbColumn("@OC_OH", orgPK, OrgContactSchema.OC_OH);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
							  VALUES (@G8_PK, @G8_G0, @G8_RecipientTableCode, @G8_RecipientID, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@G8_PK", campaignItemPK, GlbCompanyCampaignItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@G8_G0", campaignPK, GlbCompanyCampaignItemSchema.G8_G0);
				command.AddParameterBasedOnDbColumn("@G8_RecipientTableCode", OrgContactSchema.Constants.Prefix, GlbCompanyCampaignItemSchema.G8_RecipientTableCode);
				command.AddParameterBasedOnDbColumn("@G8_RecipientID", orgContactPK, GlbCompanyCampaignItemSchema.G8_RecipientID);
				command.ExecuteNonQuery();
			}
		}

		void InsertRatingHeader(Guid pk, DateTime lastEditTime)
		{
			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.RatingHeader (TH_PK, TH_SystemLastEditTimeUtc, TH_RateType, TH_QuoteDate, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES (@TH_PK, @TH_SystemLastEditTimeUtc, @TH_RateType, @TH_QuoteDate, @TH_QuoteNumber, @TH_OneTimeQuote, @TH_SystemLastEditUser, @TH_SystemCreateTimeUtc, @TH_SystemCreateUser)"
			))
			{
				command.AddParameterBasedOnDbColumn("@TH_PK", pk, RatingHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@TH_SystemLastEditTimeUtc", lastEditTime, RatingHeaderSchema.TH_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@TH_RateType", "QTE", RatingHeaderSchema.TH_RateType);
				command.AddParameterBasedOnDbColumn("@TH_QuoteDate", DateTime.Today, RatingHeaderSchema.TH_QuoteDate);
				command.AddParameterBasedOnDbColumn("@TH_QuoteNumber", pk.ToString("N"), RatingHeaderSchema.TH_QuoteNumber);
				command.AddParameterBasedOnDbColumn("@TH_OneTimeQuote", false, RatingHeaderSchema.TH_OneTimeQuote);
				command.AddParameterBasedOnDbColumn("@TH_SystemLastEditUser", "~BP", RatingHeaderSchema.TH_SystemLastEditUser);
				command.AddParameterBasedOnDbColumn("@TH_SystemCreateTimeUtc", lastEditTime, RatingHeaderSchema.TH_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@TH_SystemCreateUser", "~BP", RatingHeaderSchema.TH_SystemCreateUser);
				command.ExecuteNonQuery();
			}
		}

		void InsertPivot(Guid treeId, string parentTableCode, Guid parentPk, string childTableCode, Guid childPk)
		{
			using (var command = TestConnection.Command(
				@"INSERT INTO dbo.RelatedActivityPivot 
					(RAP_PK, RAP_ChildActivityID, RAP_ChildActivityTableCode, RAP_ParentActivityID, RAP_ParentActivityTableCode, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
				VALUES
					(@RAP_PK, @RAP_ChildActivityID, @RAP_ChildActivityTableCode, @RAP_ParentActivityID, @RAP_ParentActivityTableCode, @RAP_SalesRelationTreeID, GetUtcDate(), 'E', GetUtcDate(), 'E')"
			))
			{
				command.AddParameterBasedOnDbColumn("@RAP_PK", Guid.NewGuid(), RelatedActivityPivotSchema.PK);
				command.AddParameterBasedOnDbColumn("@RAP_ChildActivityID", childPk, RelatedActivityPivotSchema.RAP_ChildActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_ChildActivityTableCode", childTableCode, RelatedActivityPivotSchema.RAP_ChildActivityTableCode);
				command.AddParameterBasedOnDbColumn("@RAP_ParentActivityID", parentPk, RelatedActivityPivotSchema.RAP_ParentActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_ParentActivityTableCode", parentTableCode, RelatedActivityPivotSchema.RAP_ParentActivityTableCode);
				command.AddParameterBasedOnDbColumn("@RAP_SalesRelationTreeID", treeId, RelatedActivityPivotSchema.RAP_SalesRelationTreeID);
				command.ExecuteNonQuery();
			}
		}

		void InsertOneOffQuoteWithShipment(Guid ratingHeaderPk, Guid rateOneOffShipmentPk, Guid shipmentPk)
		{
			using (var command = TestConnection.Command(
	@"INSERT INTO dbo.RatingHeader (TH_PK, TH_SystemLastEditTimeUtc, TH_RateType, TH_QuoteDate, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES (@TH_PK, @TH_SystemLastEditTimeUtc, @TH_RateType, @TH_QuoteDate, @TH_QuoteNumber, @TH_OneTimeQuote, '~BP', GetUtcDate(), '~BP')"
))
			{
				command.AddParameterBasedOnDbColumn("@TH_PK", ratingHeaderPk, RatingHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@TH_SystemLastEditTimeUtc", DateTime.Now, RatingHeaderSchema.TH_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@TH_RateType", "QTE", RatingHeaderSchema.TH_RateType);
				command.AddParameterBasedOnDbColumn("@TH_QuoteDate", DateTime.Today, RatingHeaderSchema.TH_QuoteDate);
				command.AddParameterBasedOnDbColumn("@TH_QuoteNumber", ratingHeaderPk.ToString("N"), RatingHeaderSchema.TH_QuoteNumber);
				command.AddParameterBasedOnDbColumn("@TH_OneTimeQuote", true, RatingHeaderSchema.TH_OneTimeQuote);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(
	@"INSERT INTO dbo.RateOneOffShipment (TT_PK, TT_TH, TT_SystemLastEditTimeUtc, TT_SystemLastEditUser, TT_SystemCreateTimeUtc, TT_SystemCreateUser) VALUES (@TT_PK, @TT_TH, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"
))
			{
				command.AddParameterBasedOnDbColumn("@TT_PK", rateOneOffShipmentPk, RateOneOffShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("@TT_TH", ratingHeaderPk, RateOneOffShipmentSchema.TT_TH);
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(
	@"INSERT INTO dbo.JobShipment (JS_PK, JS_TH_OneTimeQuote, JS_IsBooking) VALUES (@JS_PK, @JS_TH_OneTimeQuote, @JS_IsBooking)"
))
			{
				command.AddParameterBasedOnDbColumn("@JS_PK", shipmentPk, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("@JS_TH_OneTimeQuote", ratingHeaderPk, JobShipmentSchema.JS_TH_OneTimeQuote);
				command.AddParameterBasedOnDbColumn("@JS_IsBooking", true, JobShipmentSchema.JS_IsBooking);
				command.ExecuteNonQuery();
			}
		}

		void UpdateOneOffQuoteWithShipment(Guid ratingHeaderPk, bool isCancelled, bool isOneOffQuoteConsumed)
		{
			using (var command = TestConnection.Command(
@"UPDATE dbo.RatingHeader SET TH_IsOneOffQuoteConsumed = @TH_IsOneOffQuoteConsumed, TH_IsCancelled = @TH_IsCancelled WHERE TH_PK= @TH_PK"
))
			{
				command.AddParameterBasedOnDbColumn("@TH_PK", ratingHeaderPk, RatingHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@TH_IsOneOffQuoteConsumed", isOneOffQuoteConsumed, RatingHeaderSchema.TH_IsOneOffQuoteConsumed);
				command.AddParameterBasedOnDbColumn("@TH_IsCancelled", isCancelled, RatingHeaderSchema.TH_IsCancelled);
				command.ExecuteNonQuery();
			}
		}

		public void InsertNewOrgAddress(Guid orgAddressPk, Guid orgHeaderPk)
		{
			using (var command = TestConnection.Command("INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1) values (@OA_PK, @OA_OH, 'ADDRESS TEST')"))
			{
				command.AddParameterBasedOnDbColumn("@OA_PK", orgAddressPk, OrgAddressSchema.PK);
				command.AddParameterBasedOnDbColumn("@OA_OH", orgHeaderPk, OrgAddressSchema.OA_OH);
				command.ExecuteNonQuery();
			}
		}

		public void InsertNewOrgContact(Guid orgContactPk, Guid orgHeaderPk)
		{
			using (var command = TestConnection.Command("INSERT INTO dbo.OrgContact (OC_PK, OC_OH) VALUES(@OC_PK, @OC_OH)"))
			{
				command.AddParameterBasedOnDbColumn("@OC_PK", orgContactPk, OrgContactSchema.PK);
				command.AddParameterBasedOnDbColumn("@OC_OH", orgHeaderPk, OrgContactSchema.OC_OH);
				command.ExecuteNonQuery();
			}
		}

		public void InsertNewWorkProject(string projectNumber, Guid orgAddressPk, Guid? contactPk = null)
		{
			const string sql = @"
INSERT INTO dbo.WorkProject (WKP_PK, WKP_ProjectNumber, WKP_OA_ClientAddress, WKP_OC_Contact, WKP_SystemCreateTimeUtc, WKP_SystemLastEditTimeUtc, WKP_SystemCreateUser, WKP_SystemLastEditUser)
VALUES (NEWID(), @WKP_ProjectNumber, @WKP_OA_ClientAddress, @WKP_OC_Contact, GETUTCDATE(), GetUtcDate(), 'E', 'E')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@WKP_ProjectNumber", projectNumber, WorkProjectSchema.WKP_ProjectNumber);
				command.AddParameterBasedOnDbColumn("@WKP_OA_ClientAddress", orgAddressPk, WorkProjectSchema.WKP_OA_ClientAddress);

				if (contactPk == null)
				{
					command.AddParameterBasedOnDbColumn("@WKP_OC_Contact", DBNull.Value, WorkProjectSchema.WKP_OC_Contact);
				}
				else
				{
					command.AddParameterBasedOnDbColumn("@WKP_OC_Contact", contactPk, WorkProjectSchema.WKP_OC_Contact);
				}

				command.ExecuteNonQuery();
			}
		}

		public void InsertJobDocAddressForRatingHeader(Guid pk, string address, string cityName, string stateCode, string postCode, string countryCode, string contactName, string addressType, Guid addressPk, Guid ratingHeaderPk)
		{
			var sql = @"
INSERT INTO dbo.JobDocAddress(E2_PK, E2_Address1, E2_City, E2_State, E2_PostCode, E2_RN_NKCountryCode, E2_Contact, E2_AddressType, E2_AddressSequence, E2_OA_Address, E2_ParentID, E2_ParentTableCode)
VALUES(@E2_PK, @E2_Address1, @E2_City, @E2_State, @E2_Postcode, @E2_RN_NKCountryCode, @E2_Contact, @E2_AddressType, 0, @E2_OA_Address, @E2_ParentID, 'TH')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@E2_PK", pk, JobDocAddressSchema.PK);
				command.AddParameterBasedOnDbColumn("@E2_Address1", address, JobDocAddressSchema.E2_Address1);
				command.AddParameterBasedOnDbColumn("@E2_City", cityName, JobDocAddressSchema.E2_City);
				command.AddParameterBasedOnDbColumn("@E2_State", stateCode, JobDocAddressSchema.E2_State);
				command.AddParameterBasedOnDbColumn("@E2_Postcode", postCode, JobDocAddressSchema.E2_Postcode);
				command.AddParameterBasedOnDbColumn("@E2_RN_NKCountryCode", countryCode, JobDocAddressSchema.E2_RN_NKCountryCode);
				command.AddParameterBasedOnDbColumn("@E2_Contact", contactName, JobDocAddressSchema.E2_Contact);
				command.AddParameterBasedOnDbColumn("@E2_AddressType", addressType, JobDocAddressSchema.E2_AddressType);
				command.AddParameterBasedOnDbColumn("@E2_OA_Address", addressPk, JobDocAddressSchema.E2_OA_Address);
				command.AddParameterBasedOnDbColumn("@E2_ParentID", ratingHeaderPk, JobDocAddressSchema.E2_ParentID);

				command.ExecuteNonQuery();
			}
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
						command.AddParameter("@GC_PK", SqlDbType.UniqueIdentifier, glbCompanyPk);
						command.ExecuteNonQuery();
					}
				}

				return glbCompanyPk.Value;
			}
		}
		Guid? glbCompanyPk;
		#endregion
	}
}

