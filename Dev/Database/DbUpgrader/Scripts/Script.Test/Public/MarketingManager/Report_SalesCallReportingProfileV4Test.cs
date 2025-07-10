using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.Build.Database.Script.Public.Test;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(Report_SalesCallReportingProfileV4))]
	class Report_SalesCallReportingProfileV4Test : DbCreateScriptTest
	{
		public void TestTradeLaneWithOnlyOrigin()
		{
			var primaryOrg = CreateOrg("TESTORGPMY");
			var communication = CreateCommunication("CM00001001", DateTime.Now, primaryOrg, "PHN");

			var orgSales = new OrgSales();
			orgSales.CommunicationPk = communication;
			orgSales.Primary = primaryOrg;
			orgSales.Origin = "AUSYD";
			orgSales.Product = "SHP";
			var sales = CreateOrgSales(orgSales);
			AssertReport("CM00001001", "TESTORGPMY", "SHP: AUSYD -> ");
		}

		public void TestTradeLaneConsignmentInfo()
		{
			var primaryOrg = CreateOrg("ORG_PMY");
			var buyer = CreateOrg("ORG_BUY");
			var supplier = CreateOrg("ORG_SUP");

			var communication = CreateCommunication("CM00001001", DateTime.Now, primaryOrg, "PHN");

			var orgSales = new OrgSales();
			orgSales.CommunicationPk = communication;
			orgSales.Primary = primaryOrg;
			orgSales.Origin = "AUSYD";
			orgSales.Destination = "AUBNE";
			orgSales.Product = "SHP";
			orgSales.Buyer = buyer;
			orgSales.Supplier = supplier;

			var sales = CreateOrgSales(orgSales);
			AssertReport("CM00001001", "ORG_PMY", "SHP: AUSYD -> AUBNE (CNR: ORG_SUP, CNE: ORG_BUY)");

			UpdateBuyerSupplier(sales, primaryOrg, supplier);
			AssertReport("CM00001001", "ORG_PMY", "SHP: AUSYD -> AUBNE (CNR: ORG_SUP)");

			UpdateBuyerSupplier(sales, Guid.Empty, supplier);
			AssertReport("CM00001001", "ORG_PMY", "SHP: AUSYD -> AUBNE (CNR: ORG_SUP)");

			UpdateBuyerSupplier(sales, buyer, primaryOrg);
			AssertReport("CM00001001", "ORG_PMY", "SHP: AUSYD -> AUBNE (CNE: ORG_BUY)");

			UpdateBuyerSupplier(sales, buyer, Guid.Empty);
			AssertReport("CM00001001", "ORG_PMY", "SHP: AUSYD -> AUBNE (CNE: ORG_BUY)");

			UpdateBuyerSupplier(sales, Guid.Empty, Guid.Empty);
			AssertReport("CM00001001", "ORG_PMY", "SHP: AUSYD -> AUBNE");

			UpdateBuyerSupplier(sales, primaryOrg, primaryOrg);
			AssertReport("CM00001001", "ORG_PMY", "SHP: AUSYD -> AUBNE");
		}

		public void TestBrokerWithImpAndExp()
		{
			var primaryOrg = CreateOrg("ORG_PMY");
			var buyer = CreateOrg("ORG_BUY");
			var supplier = CreateOrg("ORG_SUP");

			var communication = CreateCommunication("CM00001001", DateTime.Now, primaryOrg, "PHN");

			var orgSales = new OrgSales();
			orgSales.CommunicationPk = communication;
			orgSales.Primary = primaryOrg;
			orgSales.Origin = "AUSYD";
			orgSales.Destination = "AUBNE";
			orgSales.Product = "BRK";
			orgSales.Buyer = buyer;
			orgSales.Supplier = supplier;

			var sales = CreateOrgSales(orgSales);
			AssertReport("CM00001001", "ORG_PMY", "BRK: AUSYD (CNR: ORG_SUP, CNE: ORG_BUY)");

			UpdateBuyerSupplier(sales, primaryOrg, supplier);
			AssertReport("CM00001001", "ORG_PMY", "BRK: AUSYD (CNR: ORG_SUP)");

			UpdateBuyerSupplier(sales, buyer, primaryOrg);
			AssertReport("CM00001001", "ORG_PMY", "BRK: AUSYD (CNE: ORG_BUY)");
		}

		public void TestReportWithMultipleOrgs()
		{
			var saleGroup = GlbGeneratorForTests.NewGroup("S99");
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbGroup SET GG_IsSales = 1, GG_SystemLastEditUser = 'E', GG_SystemLastEditTimeUtc = GetDate() WHERE GG_PK = '{saleGroup}';");

			var staff = GlbGeneratorForTests.NewStaff("S99", new[] { "S99" });

			var primaryOrg = CreateOrg("TESTORGPMY");
			var communication = CreateCommunication("CM00001001", DateTime.Now, primaryOrg, "PHN", "S99");

			var orgSales = new OrgSales();
			orgSales.CommunicationPk = communication;
			orgSales.Primary = primaryOrg;
			orgSales.Origin = "AUSYD";
			orgSales.Product = "SHP";
			var sales = CreateOrgSales(orgSales);

			var orgPks = Enumerable.Range(0, 300).Select(x => Guid.NewGuid()).Union(new[] { primaryOrg });
			var saleTeamPks = Enumerable.Range(0, 300).Select(x => Guid.NewGuid()).Union(new[] { saleGroup });

			var report = RunReport(orgPks, saleTeamPks, null, Guid.Empty);
			AssertEquals(1, report.Rows.Count);
			var row = report.Rows[0];
			AssertEquals("CM00001001", row["CommunicationID"]);
			AssertEquals("TESTORGPMY", row["OrgCode"]);
			AssertEquals("SHP: AUSYD -> ", row["TradeLanes"]);
		}

		public void TestSalesRelationId()
		{
			var primaryOrg = CreateOrg("TESTORGPMY");
			var communicationPk = CreateCommunication("CM01001001", DateTime.Now, primaryOrg, "PHN", "S99");
			var communication2Pk = CreateCommunication("CM01001002", DateTime.Now, primaryOrg, "PHN", "S99");
			CreateCommunication("CM01001003", DateTime.Now, primaryOrg, "PHN", "S99");

			var relatedActivityId = Guid.NewGuid();
			CreateRelatedActivity(communicationPk, relatedActivityId);

			var relatedActivityId2 = Guid.NewGuid();
			CreateRelatedActivity(communication2Pk, relatedActivityId2);

			var orgPks = new Guid[] { primaryOrg };
			var saleTeamPks = Array.Empty<Guid>();

			var report = RunReport(orgPks, saleTeamPks, "CAM", Guid.Empty);
			AssertEquals(2, report.Rows.Count);

			report = RunReport(orgPks, saleTeamPks, "CAM", relatedActivityId);
			AssertEquals(1, report.Rows.Count);
			var row = report.Rows[0];
			AssertEquals("CM01001001", row["CommunicationID"]);
			AssertEquals("TESTORGPMY", row["OrgCode"]);
		}

		public void TestSalesTeam()
		{
			var group1Pk = Guid.NewGuid();
			CreateSalesTeam(group1Pk, "FRONT", "FRONT SALES");
			var group2Pk = Guid.NewGuid();
			CreateSalesTeam(group2Pk, "BACK", "BACK SALES");
			var group3Pk = Guid.NewGuid();
			CreateSalesTeam(group3Pk, "MAIN", "MAIN SALES");
			var staffPk1 = Guid.NewGuid();
			CreateStaffWithSalesTeams(staffPk1, "SRO", "SAM ROBERTS", "sroberts", new Guid[] { group1Pk });
			var staffPk2 = Guid.NewGuid();
			CreateStaffWithSalesTeams(staffPk2, "HSM", "HELEN SMITH", "hsmith", new Guid[] { group1Pk, group2Pk });
			var staffPk3 = Guid.NewGuid();
			CreateStaffWithSalesTeams(staffPk3, "FNK", "FRED NERK", "fnerk", new Guid[] { group1Pk, group2Pk, group3Pk });
			var staffPk4 = Guid.NewGuid();
			CreateStaffWithSalesTeams(staffPk4, "BJO", "BARRY JONES", "bjones", Array.Empty<Guid>());

			var primaryOrg = CreateOrg("TESTORGPMY");
			CreateCommunication("CM01001001", DateTime.Now, primaryOrg, "PHN", "SRO");
			CreateCommunication("CM01001002", DateTime.Now, primaryOrg, "PHN", "HSM");
			CreateCommunication("CM01001003", DateTime.Now, primaryOrg, "PHN", "FNK");
			CreateCommunication("CM01001004", DateTime.Now, primaryOrg, "PHN", "BJO");

			var orgPks = new Guid[] { primaryOrg };
			var saleTeamPks = Array.Empty<Guid>();

			var report = RunReport(orgPks, saleTeamPks, null, Guid.Empty);
			AssertEquals(4, report.Rows.Count);

			AssertEquals("Sales Team Codes for CM01001001", "FRONT", report.Rows[0]["SalesTeamCode"]);
			AssertEquals("Sales Team Codes for CM01001002", "BACK, FRONT", report.Rows[1]["SalesTeamCode"]);
			AssertEquals("Sales Team Codes for CM01001003", "BACK, FRONT, MAIN", report.Rows[2]["SalesTeamCode"]);
			AssertEquals("Sales Team Codes for CM01001004", "", report.Rows[3]["SalesTeamCode"]);

			AssertEquals("Sales Team Names for CM01001001", "FRONT SALES", report.Rows[0]["SalesTeamName"]);
			AssertEquals("Sales Team Names for CM01001002", "BACK SALES, FRONT SALES", report.Rows[1]["SalesTeamName"]);
			AssertEquals("Sales Team Names for CM01001003", "BACK SALES, FRONT SALES, MAIN SALES", report.Rows[2]["SalesTeamName"]);
			AssertEquals("Sales Team Names for CM01001004", "", report.Rows[3]["SalesTeamName"]);

			saleTeamPks = new Guid[] { group2Pk };

			report = RunReport(orgPks, saleTeamPks, null, Guid.Empty);
			AssertEquals(2, report.Rows.Count);

			AssertEquals("Sales Team Codes for CM01001002", "BACK", report.Rows[0]["SalesTeamCode"]);
			AssertEquals("Sales Team Codes for CM01001003", "BACK", report.Rows[1]["SalesTeamCode"]);

			AssertEquals("Sales Team Names for CM01001002", "BACK SALES", report.Rows[0]["SalesTeamName"]);
			AssertEquals("Sales Team Names for CM01001003", "BACK SALES", report.Rows[1]["SalesTeamName"]);
		}

		public void TestNamesWithRelatedInquiries()
		{
			var orgPk = TestDataCreator.CreateOrganisation("TESORGPMY", "TEST ORG PRIMARY");
			var contactPk = TestDataCreator.CreateContact(orgPk, "FRED NERK", "1111 2222");

			var comm1Pk = CreateCommunication("CM01001001", DateTime.Now, orgPk, "PHN", "E", contactPk);

			var inq1Pk = CreateSalesInquiry("01001001", "Test Company", "Test User", DateTime.Now, DateTime.Now);
			CreateParentRelatedActivityToCommunication(comm1Pk, "O1", inq1Pk);

			var inq2Pk = CreateSalesInquiry("01001002", "Test Company2", "Test User2", DateTime.Now, DateTime.Now);
			CreateParentRelatedActivityToCommunication(comm1Pk, "O1", inq2Pk);

			var orgPks = new Guid[] { orgPk };
			var saleTeamPks = Array.Empty<Guid>();
			var report = RunReport(orgPks, saleTeamPks, null, Guid.Empty);
			AssertEquals(1, report.Rows.Count);
			AssertEquals("CM01001001", report.Rows[0]["CommunicationID"]);
			AssertEquals("TEST ORG PRIMARY", report.Rows[0]["OrgName"]);
			AssertEquals("FRED NERK ", report.Rows[0]["ContactName"]);

			var groupPk = Guid.NewGuid();
			CreateSalesTeam(groupPk, "FRONT", "FRONT SALES");

			var staffPk = Guid.NewGuid();
			CreateStaffWithSalesTeams(staffPk, "SRO", "SAM ROBERTS", "sroberts", new Guid[] { groupPk });

			saleTeamPks = new Guid[] { groupPk };
			orgPks = Array.Empty<Guid>();

			var comm2Pk = CreateCommunication("CM01001002", DateTime.Now, Guid.Empty, "PHN", "SRO");

			var inq3Pk = CreateSalesInquiry("01001003", "Test Company3", "Test User3", DateTime.Now, DateTime.Now);
			CreateParentRelatedActivityToCommunication(comm2Pk, "O1", inq3Pk);

			var inq4Pk = CreateSalesInquiry("01001004", "Test Company4", "Test User4", DateTime.Now, DateTime.Now);
			CreateParentRelatedActivityToCommunication(comm2Pk, "O1", inq4Pk);

			report = RunReport(orgPks, saleTeamPks, null, Guid.Empty);
			AssertEquals(2, report.Rows.Count);
			AssertEquals("CM01001002", report.Rows[0]["CommunicationID"]);
			AssertEquals("Test Company3", report.Rows[0]["OrgName"]);
			AssertEquals("Test User3 ", report.Rows[0]["ContactName"]);

			AssertEquals("CM01001002", report.Rows[1]["CommunicationID"]);
			AssertEquals("Test Company4", report.Rows[1]["OrgName"]);
			AssertEquals("Test User4 ", report.Rows[1]["ContactName"]);
		}

		#region implement

		Guid CreateCommunication(string communicationID, DateTime callDate, Guid org, string typeOfCall, string salesRep = "E", Guid contact = default(Guid))
		{
			string callDateString = callDate.ToSqlFormat();
			string orgNullableString = org == Guid.Empty ? "NULL" : $"'{org}'";
			string contactNullableString = contact == Guid.Empty ? "NULL" : $"'{contact}'";
			var pk = Guid.NewGuid();
			var sql = $@"INSERT INTO dbo.OrgSalesCall (
	OQ_PK
	,OQ_CallDate
	,OQ_CallSummary
	,OQ_Category
	,OQ_CommunicationID
	,OQ_Duration
	,OQ_FollowupNotes
	,OQ_GS_NKLocationResource
	,OQ_GS_NKSalesRep
	,OQ_IsReminderClientFacing
	,OQ_LocationText
	,OQ_NextCall
	,OQ_OA_LocationAddress
	,OQ_OC
	,OQ_OH
	,OQ_SalesCallNotes
	,OQ_Status
	,OQ_SystemCreateTimeUtc
	,OQ_SystemCreateUser
	,OQ_SystemLastEditTimeUtc
	,OQ_SystemLastEditUser
	,OQ_TypeOfCall
	)
	VALUES (
	'{pk}'												--OQ_PK
	,'{callDateString}'									--OQ_CallDate
	,'a'												--OQ_CallSummary
	,''													--OQ_Category
	,'{communicationID}'								--OQ_CommunicationID
	,'{callDateString}'									--OQ_Duration
	,NULL												--OQ_FollowupNotes
	,''													--OQ_GS_NKLocationResource
	,'{salesRep}'										--OQ_GS_NKSalesRep
	,0													--OQ_IsReminderClientFacing
	,''													--OQ_LocationText
	,'{callDateString}'									--OQ_NextCall
	,NULL												--OQ_OA_LocationAddress
	,{contactNullableString}							--OQ_OC
	,{orgNullableString}								--OQ_OH
	,NULL												--OQ_SalesCallNotes
	,'COM'												--OQ_Status
	,'{callDateString}'									--OQ_SystemCreateTimeUtc
	,'E'												--OQ_SystemCreateUser
	,'{callDateString}'									--OQ_SystemLastEditTimeUtc
	,'E'												--OQ_SystemLastEditUser
	,'{typeOfCall}'										--OQ_TypeOfCall

	)";
			Db.Connection.ExecuteNonQuery(sql);
			return pk;
		}

		class OrgSales
		{
			public Guid CommunicationPk = Guid.Empty;
			public Guid Primary = Guid.Empty;
			public string Origin = "";
			public string Destination = "";
			public Guid Buyer = Guid.Empty;
			public Guid Supplier = Guid.Empty;
			public string Product = "";
			public string Service = "";
		}

		Guid CreateOrgSales(OrgSales orgSales)
		{
			var orgSalesPk = Guid.NewGuid();
			var origin = GetLocationPK(orgSales.Origin);
			var destination = GetLocationPK(orgSales.Destination);
			var originTableCode = origin != "NULL" ? "'RL'" : "''";
			var destinationTableCode = destination != "NULL" ? "'RL'" : "''";
			var date = new DateTime(2017, 1, 24, 0, 24, 0).ToSqlFormat();

			var sql = $@"INSERT INTO dbo.OrgSales (
	OW_PK
	,OW_AnnualRevenue
	,OW_DestinationID
	,OW_DestinationTableCode
	,OW_GC
	,OW_IsCustomRevenue
	,OW_LatestProspectDate
	,OW_MonthlyRevenue
	,OW_MP_Product
	,OW_OH_Buyer
	,OW_OH_Primary
	,OW_OH_Supplier
	,OW_OriginID
	,OW_OriginTableCode
	,OW_RX_NKRevenueCurrency
	,OW_Service
	,OW_IsTraded
	,OW_WW
	,OW_SystemCreateTimeUtc
	,OW_SystemCreateUser
	,OW_SystemLastEditTimeUtc
	,OW_SystemLastEditUser
	)
VALUES (
	'{orgSalesPk}'							--OW_PK
	,0.0000									--OW_AnnualRevenue
	,{destination}							--OW_DestinationID
	,{destinationTableCode}					--OW_DestinationTableCode
	,NULL									--OW_GC
	,0										--OW_IsCustomRevenue
	,NULL									--OW_LatestProspectDate
	,0.0000									--OW_MonthlyRevenue
	,{GetProductPK(orgSales.Product)}		--OW_MP_Product
	,{GetGuidString(orgSales.Buyer)}		--OW_OH_Buyer
	,{GetGuidString(orgSales.Primary)}		--OW_OH_Primary
	,{GetGuidString(orgSales.Supplier)}		--OW_OH_Supplier
	,{origin}								--OW_OriginID
	,{originTableCode}						--OW_OriginTableCode
	,'AUD'									--OW_RX_NKRevenueCurrency
	,'{orgSales.Service}'					--OW_Service
	,0										--OW_IsTraded
	,NULL									--OW_WW
	,'{date}'								--OW_SystemCreateTimeUtc
	,'E'									--OW_SystemCreateUser
	,GetUtcDate()							--OW_SystemLastEditTimeUtc
	,'E'									--OW_SystemLastEditUser
	);

INSERT INTO dbo.OrgSalesValueAssociationPivot (
	SVP_PK
	,SVP_ActivityId
	,SVP_ActivityTableCode
	,SVP_TradeId
	,SVP_TradeTableCode
	,SVP_SystemCreateTimeUtc
	,SVP_SystemCreateUser
	,SVP_SystemLastEditTimeUtc
	,SVP_SystemLastEditUser
	)
VALUES (
	NEWID()
	,'{orgSales.CommunicationPk}'
	,'OQ'
	,'{orgSalesPk}'
	,'OW'
	,GetUtcDate()
	,'E'
	,GetUtcDate()
	,'E'
	);";
			Db.Connection.ExecuteNonQuery(sql);
			return orgSalesPk;
		}

		void CreateRelatedActivity(Guid commPk, Guid relatedActivityId)
		{
			var sql =
@"
INSERT INTO dbo.RelatedActivityPivot
(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
(newid(), 'OQ', @RAP_ParentActivityId, 'G0', @RAP_ChildActivityID, @RAP_SalesRelationTreeID, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@RAP_ParentActivityId", commPk, RelatedActivityPivotSchema.RAP_ParentActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_ChildActivityID", relatedActivityId, RelatedActivityPivotSchema.RAP_ChildActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_SalesRelationTreeID", relatedActivityId, RelatedActivityPivotSchema.RAP_SalesRelationTreeID);
				command.ExecuteNonQuery();
			}
		}

		void CreateParentRelatedActivityToCommunication(Guid commPk, string parentTableCode, Guid parentRelatedActivityId)
		{
			var sql =
@"
INSERT INTO dbo.RelatedActivityPivot
(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
(newid(), @RAP_ParentActivityTableCode, @RAP_ParentActivityId, 'OQ', @RAP_ChildActivityID, @RAP_SalesRelationTreeID, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@RAP_ParentActivityTableCode", parentTableCode, RelatedActivityPivotSchema.RAP_ParentActivityTableCode);
				command.AddParameterBasedOnDbColumn("@RAP_ParentActivityId", parentRelatedActivityId, RelatedActivityPivotSchema.RAP_ParentActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_ChildActivityID", commPk, RelatedActivityPivotSchema.RAP_ChildActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_SalesRelationTreeID", commPk, RelatedActivityPivotSchema.RAP_SalesRelationTreeID);
				command.ExecuteNonQuery();
			}
		}

		void CreateSalesTeam(Guid pk, string code, string name)
		{
			var @sql = @"
INSERT dbo.GlbGroup(GG_PK, GG_Code, GG_Desc, GG_IsSales) VALUES(@Pk, @Code, @Name, 1)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@Pk", pk, GlbGroupSchema.PK);
				command.AddParameterBasedOnDbColumn("@Code", code, GlbGroupSchema.GG_Code);
				command.AddParameterBasedOnDbColumn("@Name", name, GlbGroupSchema.GG_Desc);
				command.ExecuteNonQuery();
			}
		}

		void CreateStaffWithSalesTeams(Guid pk, string code, string name, string loginName, Guid[] teamPks)
		{
			var sql = @"
			INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
			VALUES(@Pk, @Code, @Name, @LoginName, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@Pk", pk, GlbStaffSchema.PK);
				command.AddParameterBasedOnDbColumn("@Code", code, GlbStaffSchema.GS_Code);
				command.AddParameterBasedOnDbColumn("@Name", name, GlbStaffSchema.GS_FullName);
				command.AddParameterBasedOnDbColumn("@LoginName", loginName, GlbStaffSchema.GS_LoginName);
				command.ExecuteNonQuery();
			}

			var sqlLink = @"
INSERT dbo.GlbGroupLink(GK_PK, GK_GS, GK_GG) VALUES(NEWID(), @Pk, @TeamPk)
";
			foreach (var teamPk in teamPks)
			{
				using (var command = TestConnection.Command(sqlLink))
				{
					command.AddParameterBasedOnDbColumn("@Pk", pk, GlbGroupLinkSchema.GK_GS);
					command.AddParameterBasedOnDbColumn("@TeamPk", teamPk, GlbGroupLinkSchema.GK_GG);
					command.ExecuteNonQuery();
				}
			}
		}

		Guid CreateSalesInquiry(string reference, string companyName, string contactName, DateTime createdTime, DateTime? callDate)
		{
			var insertQuery = string.Format(@"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_LeadCalledDate, @O1_CompanyName, @O1_ContactName, @O1_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')",
				OrgColdCallRegisterSchema.Constants.TableName,
				OrgColdCallRegisterSchema.Constants.PK,
				OrgColdCallRegisterSchema.Constants.O1_LeadUniqueReference,
				OrgColdCallRegisterSchema.Constants.O1_LeadCalledDate,
				OrgColdCallRegisterSchema.Constants.O1_CompanyName,
				OrgColdCallRegisterSchema.Constants.O1_ContactName,
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
				command.AddParameter("@O1_CompanyName", SqlDbType.VarChar, companyName);
				command.AddParameter("@O1_ContactName", SqlDbType.VarChar, contactName);
				command.AddParameter("@O1_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createdTime);
				command.ExecuteNonQuery();
			}

			return inquiryPk;
		}

		DataTable RunReport()
		{
			return RunReport(null, null, null, Guid.Empty);
		}

		DataTable RunReport(IEnumerable<Guid> organisationPKs, IEnumerable<Guid> salesTeamPKs, string salesRelation, Guid relatedActivityId)
		{
			using (var conn = Db.NewAdminConnection())
			{
				conn.ExecuteNonQuery("exec sp_updatestats;");
			}

			const string runReportSql = @"

SELECT *
FROM Report_SalesCallReportingProfileV4(NULL, --@CompanyPK uniqueidentifier,
		@OrganisationPKs,			--@OrganisationPKs dbo.TVP_UNIQUEIDENTIFIER READONLY,
		@OrganisationPKsIsEmpty,	--@OrganisationPKsIsEmpty bit,
		'N', --@IncludeManagedOrgs char(1),
		NULL, --@TradeLanesDirection AS CHAR(3),
		NULL, --@TradeLanesOrigin AS VARCHAR(5),
		NULL, --@TradeLanesDestination AS VARCHAR(5),
		NULL, --@RecentActivityType AS varchar(3),
		@SalesRelation, --@RelatedActivityType AS varchar(3),
		NULL, --@ActualDateFrom as datetime,
		NULL, --@ActualDateTo as datetime,
		@SalesTeamPKs,			--@SalesTeamPKs dbo.TVP_UNIQUEIDENTIFIER READONLY,
		@SalesTeamPKsIsEmpty,	--@SalesTeamPKsIsEmpty bit,
		@RelatedActivityId --@RelatedActivityId AS UNIQUEIDENTIFIER
	) OPTION (RECOMPILE);
";
			using (var sqlCommand = Db.Connection.Command(runReportSql))
			{
				sqlCommand.AddTableValuedParameter("@OrganisationPKs", "dbo.TVP_uniqueidentifier", organisationPKs ?? Enumerable.Empty<Guid>());
				sqlCommand.AddTableValuedParameter("@SalesTeamPKs", "dbo.TVP_uniqueidentifier", salesTeamPKs ?? Enumerable.Empty<Guid>());

				sqlCommand.AddParameter("@OrganisationPKsIsEmpty", SqlDbType.Bit, (organisationPKs ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
				sqlCommand.AddParameter("@SalesTeamPKsIsEmpty", SqlDbType.Bit, (salesTeamPKs ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
				sqlCommand.AddParameter("@SalesRelation", SqlDbType.VarChar, salesRelation ?? (object)DBNull.Value);
				sqlCommand.AddParameter("@RelatedActivityId", SqlDbType.UniqueIdentifier, relatedActivityId == Guid.Empty ? DBNull.Value : relatedActivityId);

				using (var reader = sqlCommand.ExecuteReader())
				{
					var dt = new DataTable();
					dt.Load(reader);
					dt.DefaultView.Sort = "CommunicationID ASC";
					return dt.DefaultView.ToTable();
				}
			}
		}

		string GetGuidString(Guid guid)
		{
			return (guid != Guid.Empty ? $"'{guid}'" : "NULL");
		}

		string GetProductPK(string product)
		{
			var pk = Db.Connection.ExecuteScalar($@"SELECT MP_PK FROM dbo.OrgSalesProduct WHERE MP_Code = '{product}';");
			return (pk is Guid) ? $"'{pk}'" : "NULL";
		}

		string GetLocationPK(string rlCode)
		{
			var pk = Db.Connection.ExecuteScalar($@"SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = '{rlCode}';");
			return (pk is Guid) ? $"'{pk}'" : "NULL";
		}

		Guid CreateOrg(string orgCode)
		{
			var pk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES('{pk}', '{orgCode}')");
			return pk;
		}

		void UpdateBuyerSupplier(Guid salesPK, Guid buyer, Guid supplier)
		{
			var sql = $@"UPDATE dbo.OrgSales SET OW_OH_Buyer = {GetGuidString(buyer)}, OW_OH_Supplier = {GetGuidString(supplier)}, OW_SystemLastEditTimeUtc = GETUTCDATE(), OW_SystemLastEditUser = 'E' WHERE OW_PK = '{salesPK}';";
			Db.Connection.ExecuteNonQuery(sql);
		}

		void AssertReport(string communicationID, string orgCode, string tradeLanes)
		{
			var report = RunReport();
			AssertEquals(1, report.Rows.Count);
			var row = report.Rows[0];
			AssertEquals(communicationID, row["CommunicationID"]);
			AssertEquals(orgCode, row["OrgCode"]);
			AssertEquals(tradeLanes, row["TradeLanes"]);
		}
		#endregion implement
	}
}
