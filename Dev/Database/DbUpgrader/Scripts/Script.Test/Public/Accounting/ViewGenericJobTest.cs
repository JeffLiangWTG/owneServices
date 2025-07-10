using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(ViewGenericJob))]
	class ViewGenericJobTest : EdwHashTest
	{
		public void TestSampleCall()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.JobConsol (JK_PK, JK_IsCFS, JK_IsForwarding) Values ('F0092952-960E-44f0-95EF-401F15E5A861', 1, 1)");

			TestConnection.Command("INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwarding, JS_Is");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from dbo.ViewGenericJob");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
		}

		public void TestTransportJobIsNotCompanySpecific()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var jobShipmentPK = helper.InsertShipment("S00001234", true);
			var jobCartagePK = helper.InsertJobCartage("S00001234/E", branchPK, jJ_E3_NKJobType: "FCI", jJ_ParentID: jobShipmentPK, jJ_ParentTableCode: "JS");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber");
			AssertEquals("Result should have rows", 2, result.Rows.Count);

			AssertEquals("Shipment Job", "S00001234", result.Rows[0]["VJ_JobNumber"].ToString());
			AssertEquals("Shipment Job Company PK", string.Empty, result.Rows[0]["VJ_CompanyPK"].ToString());

			AssertEquals("Declaration Job", "S00001234/E", result.Rows[1]["VJ_JobNumber"].ToString());
			AssertEquals("Declaration Job Company PK", string.Empty, result.Rows[1]["VJ_CompanyPK"].ToString());
		}

		public void TestJobSundryChargesActuallyReturnsJobNumber()
		{
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.JobSundryCharges (D4_PK, D4_SundriesJobType, D4_SundryJobMode, D4_SundryJobActivity, D4_JobNumber, D4_FromDate, D4_ToDate, D4_SundriesDescription, D4_AdditionalDetails, D4_OH_BillToParty, D4_GC)
				VALUES (newid(), 'PSN', 'STD', 'STD', 'SC00000001', '2020-02-07 00:00:00', '2020-02-12 00:00:00', '', '', 'C3F842EF-3BE5-448C-BED3-0017B232C624', Null),
				(newid(), 'PSN', 'STD', 'STD', 'SC00000002', '2020-02-13 00:00:00', '2020-02-18 00:00:00', '', '', 'C3F842EF-3BE5-448C-BED3-0017B232C624', Null)");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from dbo.ViewGenericJob order by vj_jobnumber");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			AssertEquals("First row should have correct job number", "SC00000001", result.Rows[0]["VJ_JobNumber"]);
			AssertEquals("Second row should have correct job number", "SC00000002", result.Rows[1]["VJ_JobNumber"]);
		}

		public void TestCFSAndForwardingConsolJobDoesNotCalculateETAETD()
		{
			string sql = @"DECLARE @CFSConsolPK uniqueidentifier;
DECLARE @ForwardingConsolPK uniqueidentifier;
SET @CFSConsolPK = (SELECT NEWID());
SET @ForwardingConsolPK = (SELECT NEWID());

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS)
VALUES (@CFSConsolPK, 'C00001234', 1)

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ETD, JW_ETA)
VALUES (newID(), @CFSConsolPK, '2011-03-03', '2011-04-04')

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_IsForwarding)
VALUES (@ForwardingConsolPK, 'C00009876', 0, 1)

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ETD, JW_ETA)
VALUES (newID(), @ForwardingConsolPK, '2011-03-03', '2011-04-04')
";
			TestConnection.ExecuteNonQuery(sql);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber");
			AssertEquals("Result should have rows", 2, result.Rows.Count);

			AssertEquals("CFS Consol ETD", string.Empty, result.Rows[0]["VJ_ETD"].ToString());
			AssertEquals("CFS Consol ETA", string.Empty, result.Rows[0]["VJ_ETA"].ToString());
			AssertEquals("Forwarding Consol ETD", string.Empty, result.Rows[1]["VJ_ETD"].ToString());
			AssertEquals("Forwarding Consol ETA", string.Empty, result.Rows[1]["VJ_ETA"].ToString());
		}

		public void TestGetsCorrectConsolJobConsumerType()
		{
			var sqlText = @"DECLARE @CFSConsolPK uniqueidentifier;
DECLARE @ForwardingConsolPK uniqueidentifier;
DECLARE @GatewayConsolPK1 uniqueidentifier;
DECLARE @GatewayConsolPK2 uniqueidentifier;
DECLARE @GatewayConsolPK3 uniqueidentifier;
DECLARE @GatewayConsolPK4 uniqueidentifier;
SET @CFSConsolPK = (SELECT NEWID());
SET @ForwardingConsolPK = (SELECT NEWID());
SET @GatewayConsolPK1 = (SELECT NEWID());
SET @GatewayConsolPK2 = (SELECT NEWID());
SET @GatewayConsolPK3 = (SELECT NEWID());
SET @GatewayConsolPK4 = (SELECT NEWID());

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_AgentType)
VALUES (@CFSConsolPK, 'C00001234', 1, 'AGT')

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_IsForwarding)
VALUES (@ForwardingConsolPK, 'C00002345', 0, 1)

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_IsForwarding, JK_AgentType, JK_SendingForwarderHandlingType)
VALUES (@GatewayConsolPK1, 'C00003456', 1, 1, 'AGT', 'GTT');

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_IsForwarding, JK_AgentType, JK_SendingForwarderHandlingType)
VALUES (@GatewayConsolPK2, 'C00003459', 1, 1, 'CLD', 'GTA');

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_IsForwarding, JK_AgentType, JK_ReceivingForwarderHandlingType)
VALUES (@GatewayConsolPK3, 'C00007889', 1, 1, 'AGT', 'GTT');

INSERT INTO dbo.JobConsol(JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_IsForwarding, JK_AgentType, JK_ReceivingForwarderHandlingType)
VALUES(@GatewayConsolPK4, 'C00007898', 1, 1, 'CLD', 'GTA')";
			TestConnection.ExecuteNonQuery(sqlText);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber");
			AssertEquals("Result should have rows", 6, result.Rows.Count);

			AssertEquals("CFS Consol Job Type. CFS is prefered to FWD", "CLL", result.Rows[0]["VJ_JobType"].ToString());
			AssertEquals("Forwarding Consol Job Type", "FCN", result.Rows[1]["VJ_JobType"].ToString());
			AssertEquals("When Sending Forwarder Handling Type is GTT, Gateway Consol Job Type overrides other types", "GCN", result.Rows[2]["VJ_JobType"].ToString());
			AssertEquals("When Sending Forwarder Handling Type is GTA, Gateway Consol Job Type overrides other types", "GCN", result.Rows[3]["VJ_JobType"].ToString());
			AssertEquals("When Receiving Forwarder Handling Type is GTT, Gateway Consol Job Type overrides other types", "GCN", result.Rows[4]["VJ_JobType"].ToString());
			AssertEquals("When Receiving Forwarder Handling Type is GTA, Gateway Consol Job Type overrides other types", "GCN", result.Rows[5]["VJ_JobType"].ToString());
		}

		public void TestViewGenericJobIncludeAllJobs()
		{
			string sql = @"
DECLARE @BranchPK uniqueidentifier;
DECLARE @CompanyPK uniqueidentifier;
DECLARE @CountryCode VARCHAR(2);
SELECT TOP 1 @CountryCode = GC_RN_NKCountryCode, @BranchPK = GB_PK, @CompanyPK = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK;
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsCancelled)
VALUES (NEWID(), 'S00001234', 1)

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsCancelled)
VALUES (NEWID(), 'S00001235', 0)

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_JS, JE_IsCancelled, JE_GB, JE_GC, JE_ClusterKey)
VALUES (NEWID(), @CountryCode, 'B0003', null, 1, @BranchPK, @CompanyPK, 1)

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_JS, JE_IsCancelled, JE_GB, JE_GC, JE_ClusterKey)
VALUES (NEWID(), @CountryCode, 'B0001', null, 0, @BranchPK, @CompanyPK, 2)
";
			TestConnection.ExecuteNonQuery(sql);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber, VJ_IsInactive");
			AssertEquals("Result should have two rows", 4, result.Rows.Count);
			AssertEquals("First row should be the declaration", "B0001", result.Rows[0]["VJ_JobNumber"]);
			AssertEquals("Second row should be the declaration", "B0003", result.Rows[1]["VJ_JobNumber"]);
			AssertEquals("Third row should be the shipment", "S00001234", result.Rows[2]["VJ_JobNumber"]);
			AssertEquals("Fourth row should be the shipment", "S00001235", result.Rows[3]["VJ_JobNumber"]);
		}

		public void TestStandaloneDeclaration()
		{
			string sql = @"
DECLARE @ShipmentPK uniqueidentifier;
DECLARE @BranchPK uniqueidentifier;
SET @ShipmentPK = (SELECT NEWID());
DECLARE @CompanyPK uniqueidentifier;
DECLARE @CountryCode VARCHAR(2);
SELECT TOP 1 @CountryCode = GC_RN_NKCountryCode, @BranchPK = GB_PK, @CompanyPK = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK;

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef)
VALUES (@ShipmentPK, 'S0001')

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_JS, JE_IsCancelled, JE_GB, JE_GC, JE_ClusterKey)
VALUES (NEWID(), @CountryCode, 'B0001', @ShipmentPK, 0, @BranchPK, @CompanyPK, 1)

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_JS, JE_IsCancelled, JE_GB, JE_GC, JE_ClusterKey)
VALUES (NEWID(), @CountryCode, 'B0002', null, 0, @BranchPK, @CompanyPK, 2)

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_JS, JE_IsCancelled, JE_GB, JE_GC, JE_ClusterKey)
VALUES (NEWID(), @CountryCode, 'B0003', @ShipmentPK, 1, @BranchPK, @CompanyPK, 3)

";

			TestConnection.ExecuteNonQuery(sql);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber");
			AssertEquals("Result should have two rows", 2, result.Rows.Count);
			AssertEquals("First row should be second declaration", "B0002", result.Rows[0]["VJ_JobNumber"]);
			AssertEquals("Second row should be shipment", "S0001", result.Rows[1]["VJ_JobNumber"]);
		}

		public void TestViewGenericJob_IncludesWarehouseAdHocServiceJobs()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("ORG").AppendInsertAndReturnObject(sql);
			var adHocServiceJob = new WhsAdHocServiceJob(whs.PK, client.PK, "WI00000001", "REF", DateTime.Now).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("Row should be warehouse ad hoc service job", adHocServiceJob.WSJ_JobNumber, result.Rows[0]["VJ_JobNumber"]);
		}

		public void TestSuspendConvertingToShipment()
		{
			var sql = @"
DECLARE @CompanyPk AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775433'
DECLARE @DepartmentPk AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FB'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)

INSERT INTO dbo.RatingHeader(TH_PK, TH_IsCancelled,TH_OneTimeQuote,TH_QuoteNumber,TH_QuoteDate,TH_QuoteEndDate,TH_RateType, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES('1e63fd5b-b700-4b74-b7de-2bff8026e91a',0,1,'00000001',GETDATE(), GETDATE(),'QTE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status) VALUES('58e03d50-d2ef-49e7-be3a-a90a54938ff3', '00000001','1e63fd5b-b700-4b74-b7de-2bff8026e91a','TH','JOB',  @BranchPk, @DepartmentPk, @CompanyPk, 'WRK')

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsForwardRegistered, JS_TH_OneTimeQuote) VALUES('476440a5-ee77-47b6-8611-72bd2f7f23f8', 'S0001', 1, 1, '1e63fd5b-b700-4b74-b7de-2bff8026e91a')

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status) VALUES('6930151b-3d0e-4215-8f3a-ee96d835a0d9', '00000002','476440a5-ee77-47b6-8611-72bd2f7f23f8','JS','JOB',  @BranchPk, @DepartmentPk, @CompanyPk, 'WRK')


INSERT INTO dbo.RatingHeader(TH_PK, TH_IsCancelled,TH_OneTimeQuote,TH_QuoteNumber,TH_QuoteDate,TH_QuoteEndDate,TH_RateType, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES('8390ec8e-24c6-4a64-92df-87368b27ed4c',0,1,'00000002',GETDATE(), GETDATE(),'QTE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status) VALUES('3d7ebc2f-b017-414c-aa9e-c13f812013bf', '00000003','8390ec8e-24c6-4a64-92df-87368b27ed4c','TH','JOB',  @BranchPk, @DepartmentPk, @CompanyPk, 'WRK')

INSERT INTO dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsForwardRegistered, JS_TH_OneTimeQuote) VALUES('6a3b3a51-ec62-4350-bd53-51f4fad8a4d2', 'S0002', 1, 0, '8390ec8e-24c6-4a64-92df-87368b27ed4c')


INSERT INTO dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsForwardRegistered) VALUES('f2d849a5-9de7-4006-9e90-551d2ddfcdc9', 'S0003', 0, 1)

INSERT INTO dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsForwardRegistered) VALUES('de7e0fa8-08bd-447f-9891-8c80bced7531', 'S0004', 1, 1)";

			TestConnection.ExecuteNonQuery(sql);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber,VJ_TableName");

			AssertEquals(5, result.Rows.Count);
			AssertEquals("S0001", result.Rows[0]["VJ_JobNumber"]);
			AssertEquals("SHP", result.Rows[0]["VJ_JobType"]);
			AssertEquals("JobShipment", result.Rows[0]["VJ_TableName"]);

			AssertEquals("S0002", result.Rows[1]["VJ_JobNumber"]);
			AssertEquals("SHP", result.Rows[1]["VJ_JobType"]);
			AssertEquals("JobShipment", result.Rows[1]["VJ_TableName"]);

			AssertEquals("S0002", result.Rows[2]["VJ_JobNumber"]);
			AssertEquals("QSH", result.Rows[2]["VJ_JobType"]);
			AssertEquals("RatingHeader", result.Rows[2]["VJ_TableName"]);

			AssertEquals("S0003", result.Rows[3]["VJ_JobNumber"]);
			AssertEquals("SHP", result.Rows[3]["VJ_JobType"]);
			AssertEquals("JobShipment", result.Rows[3]["VJ_TableName"]);

			AssertEquals("S0004", result.Rows[4]["VJ_JobNumber"]);
			AssertEquals("SHP", result.Rows[4]["VJ_JobType"]);
			AssertEquals("JobShipment", result.Rows[4]["VJ_TableName"]);
		}

		public void TestNotContainsCusInBondHeaders_WhichBH_ApplicationCodeIsAMSOrINB()
		{
			var jobNumber = "C202201171543";
			var sql = $@"
DECLARE @CompanyPk AS UNIQUEIDENTIFIER = '{Guid.NewGuid()}'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '{Guid.NewGuid()}'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS) VALUES ('{Guid.NewGuid()}', '{jobNumber}', 1)

INSERT INTO dbo.CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_SystemCreateUser, BH_SystemLastEditUser,BH_SystemCreateTimeUtc, BH_SystemLastEditTimeUtc)
VALUES ('{Guid.NewGuid()}', @BranchPk, '{jobNumber}', 'AMS', '~BP', '~BP', GetUtcDate(), GetUtcDate())

INSERT INTO dbo.CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_SystemCreateUser, BH_SystemLastEditUser,BH_SystemCreateTimeUtc, BH_SystemLastEditTimeUtc)
VALUES ('{Guid.NewGuid()}', @BranchPk, '{jobNumber}', 'INB', '~BP', '~BP', GetUtcDate(), GetUtcDate())";

			TestConnection.ExecuteNonQuery(sql);

			AssertEquals("There are one JobConsol which JobNumber is C202201171543", 1, DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{jobNumber}'").Rows.Count);
			AssertEquals("There are two CusInBondHeader which JobNumber is C202201171543", 2, DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.CusInBondHeader WHERE BH_JobReference = '{jobNumber}'").Rows.Count);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.ViewGenericJob WHERE VJ_JobNumber =  '{jobNumber}'");
			AssertEquals("There is only one ViewGenericJob which JobNumber is C202201171543", 1, result.Rows.Count);
			AssertEquals("The only one ViewGenericJob's TableName is JobConsol", "JobConsol", result.Rows[0]["VJ_TableName"]);
		}

		public void TestQuickBookingShouldHaveQSHJobType()
		{
			var sql = @"
DECLARE @CompanyPk AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775433'
DECLARE @DepartmentPk AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FB'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status) VALUES('58e03d50-d2ef-49e7-be3a-a90a54938ff3', '00000001','476440a5-ee77-47b6-8611-72bd2f7f23f8','JS','JOB',  @BranchPk, @DepartmentPk, @CompanyPk, 'WRK')

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsForwardRegistered) VALUES('476440a5-ee77-47b6-8611-72bd2f7f23f8', 'S0001', 1, 0)

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status) VALUES('58e03d50-d2ef-49e7-be3a-a90a54938ff2', '00000002','476440a5-ee77-47b6-8611-72bd2f7f23f7','JS','JOB',  @BranchPk, @DepartmentPk, @CompanyPk, 'WRK')

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsForwardRegistered,JS_IsCFSRegistered) VALUES('476440a5-ee77-47b6-8611-72bd2f7f23f7', 'S0002', 1, 0, 1)";

			TestConnection.ExecuteNonQuery(sql);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber,VJ_TableName");

			AssertEquals(2, result.Rows.Count);
			AssertEquals("S0001", result.Rows[0]["VJ_JobNumber"]);
			AssertEquals("QSH", result.Rows[0]["VJ_JobType"]);
			AssertEquals("JobShipment", result.Rows[0]["VJ_TableName"]);

			AssertEquals("S0002", result.Rows[1]["VJ_JobNumber"]);
			AssertEquals("QSH", result.Rows[1]["VJ_JobType"]);
			AssertEquals("JobShipment", result.Rows[1]["VJ_TableName"]);
		}

		public void TestViewGenericJob_IncludesTemporaryStorageAsycudaManifestHeaders()
		{
			var branchInfoDataTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT TOP 1 GB_PK, GB_GC FROM dbo.GlbBranch");
			AssertEquals("[PRE-REQUISITE] Branch DataTable result should have rows", 1, branchInfoDataTable.Rows.Count);

			var branchPK = Guid.Parse(branchInfoDataTable.Rows[0]["GB_PK"].ToString());
			var companyPK = Guid.Parse(branchInfoDataTable.Rows[0]["GB_GC"].ToString());
			var manifestPK = TestDataCreator.CreateAsycudaManifestHeader(branchPK, "NUM 1", "AU", 1, "STO");
			TestDataCreator.CreateAsycudaManifestHeader(branchPK, "NUM 2", "AU", 2, "NVC");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewGenericJob ORDER BY VJ_JobNumber");
			AssertEquals("ViewGenericJob DataTable result should have rows", 1, result.Rows.Count);

			CombineAssertions(() =>
			{
				AssertEquals("VJ_PK", manifestPK, result.Rows[0]["VJ_PK"]);
				AssertEquals("VJ_TableName", "AsycudaManifestHeader", result.Rows[0]["VJ_TableName"].ToString());
				AssertEquals("VJ_JobNumber", "NUM 1", result.Rows[0]["VJ_JobNumber"].ToString());
				AssertEquals("VJ_JobType", "STO", result.Rows[0]["VJ_JobType"].ToString());
				AssertEquals("VJ_ETD", DBNull.Value, result.Rows[0]["VJ_ETD"]);
				AssertEquals("VJ_ETA", DBNull.Value, result.Rows[0]["VJ_ETA"]);
				AssertEquals("VJ_HouseBillNumber", "", result.Rows[0]["VJ_HouseBillNumber"].ToString());
				AssertEquals("VJ_MasterBillNumber", "", result.Rows[0]["VJ_MasterBillNumber"].ToString());
				AssertEquals("VJ_CompanyPK", companyPK, result.Rows[0]["VJ_CompanyPK"]);
				AssertEquals("VJ_IsInactive", false, result.Rows[0]["VJ_IsInactive"]);
			});
		}
	
		protected override string expectedMainDbFunctionHash => "3316566728ED9480221A7E92EFA9EAB3E786AC467C1DF941B9DA513E5736F54E";
		protected override string expectedEdwDbFunctionHash => "D2BA7802CC6F942BD4CBF89F1FFE05E7E8C89DEBD3B5E693E753831FF50F4684";

		protected override string edwScriptPath => "ReportFunctions/Accounting/ViewGenericJob.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ViewGenericJob();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.ViewGenericJob();
		}
	}
}
