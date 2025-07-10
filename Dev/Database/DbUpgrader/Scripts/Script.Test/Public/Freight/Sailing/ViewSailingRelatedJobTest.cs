using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Sailing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Sailing
{
	[TestedType(typeof(ViewSailingRelatedJob))]
	class ViewSailingRelatedJobTest : DbCreateScriptTest
	{
		public void TestConsols()
		{
			var forwardingConsolPK = Guid.NewGuid();
			var loadListPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsForwarding, JK_IsCFS)
VALUES 
	(@ForwardingConsolPK, 'C00001234', 1, 0),
	(@LoadListPK, 'L00001234', 0, 1)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("ForwardingConsolPK", forwardingConsolPK, JobConsolSchema.PK);
				command.AddParameterBasedOnDbColumn("LoadListPK", loadListPK, JobConsolSchema.PK);
				command.ExecuteNonQuery();
			}

			var transport1PK = CreateLinkedTransport(sailing1PK, "CON", forwardingConsolPK);
			var transport2PK = CreateLinkedTransport(sailing2PK, "CON", loadListPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 2, result.Rows.Count);
			AssertRowValues(result.Rows[0], transport1PK, sailing1PK, forwardingConsolPK, "C00001234", "CON", Guid.Empty, false);
			AssertRowValues(result.Rows[1], transport2PK, sailing2PK, loadListPK, "L00001234", "CLL", Guid.Empty, false);
		}

		public void TestShipments()
		{
			var cfsShipmentPK = Guid.NewGuid();
			var billOfLading1PK = Guid.NewGuid();
			var billOfLading2PK = Guid.NewGuid();
			var agencyBooking1PK = Guid.NewGuid();
			var agencyBooking2PK = Guid.NewGuid();
			var booking1PK = Guid.NewGuid();
			var booking2PK = Guid.NewGuid();
			var forwardingShipmentPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_JX, JS_IsForwardRegistered, JS_IsBooking, JS_IsCFSRegistered, JS_IsShipping, JS_ShipmentStatus, JS_IsCancelled)
VALUES
	(@CfsShipmentPK, 'S00005001', @Sailing1PK, 0, 0, 1, 0, '', 0),
	(@BillOfLading1PK, 'S00005002', @Sailing2PK, 0, 0, 0, 1, 'CNF', 0),
	(@AgencyBooking1PK, 'S00005003', @Sailing3PK, 0, 0, 0, 1, 'BKD', 0),
	(@Booking1PK, 'S00005004', @Sailing1PK, 0, 1, 0, 0, '', 0),
	(@ForwardingShipmentPK, 'S00005005', null, 1, 0, 0, 0, '', 0),
	(@BillOfLading2PK, 'S00005006', null, 0, 0, 0, 1, 'WFI', 0),
	(@AgencyBooking2PK, 'S00005007', null, 0, 0, 0, 1, 'BKD', 0),
	(@Booking2PK, 'S00005008', @Sailing2PK, 0, 1, 1, 0, '', 0)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("CfsShipmentPK", cfsShipmentPK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("BillOfLading1PK", billOfLading1PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("AgencyBooking1PK", agencyBooking1PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("Booking1PK", booking1PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("ForwardingShipmentPK", forwardingShipmentPK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("BillOfLading2PK", billOfLading2PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("AgencyBooking2PK", agencyBooking2PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("Booking2PK", booking2PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("Sailing1PK", sailing1PK, JobShipmentSchema.JS_JX);
				command.AddParameterBasedOnDbColumn("Sailing2PK", sailing2PK, JobShipmentSchema.JS_JX);
				command.AddParameterBasedOnDbColumn("Sailing3PK", sailing3PK, JobShipmentSchema.JS_JX);
				command.ExecuteNonQuery();
			}

			var transport1PK = CreateLinkedTransport(sailing2PK, "SHP", forwardingShipmentPK);
			var transport2PK = CreateLinkedTransport(sailing3PK, "ASH", billOfLading2PK);
			var transport3PK = CreateLinkedTransport(sailing1PK, "ASH", agencyBooking2PK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 8, result.Rows.Count);
			AssertRowValues(result.Rows[0], cfsShipmentPK, sailing1PK, cfsShipmentPK, "S00005001", "CSH", Guid.Empty, false);
			AssertRowValues(result.Rows[1], billOfLading1PK, sailing2PK, billOfLading1PK, "S00005002", "ASH", Guid.Empty, false);
			AssertRowValues(result.Rows[2], agencyBooking1PK, sailing3PK, agencyBooking1PK, "S00005003", "AGB", Guid.Empty, false);
			AssertRowValues(result.Rows[3], booking1PK, sailing1PK, booking1PK, "S00005004", "QSH", Guid.Empty, false);
			AssertRowValues(result.Rows[4], transport1PK, sailing2PK, forwardingShipmentPK, "S00005005", "SHP", Guid.Empty, false);
			AssertRowValues(result.Rows[5], transport2PK, sailing3PK, billOfLading2PK, "S00005006", "ASH", Guid.Empty, false);
			AssertRowValues(result.Rows[6], transport3PK, sailing1PK, agencyBooking2PK, "S00005007", "AGB", Guid.Empty, false);
			AssertRowValues(result.Rows[7], booking2PK, sailing2PK, booking2PK, "S00005008", "QSH", Guid.Empty, false);
		}

		public void TestShipmentPreAdvices()
		{
			var preAdvicePK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobShipmentPreplanning (EF_PK, EF_PreshipID, EF_OA_BuyerAddress)
VALUES (@PreAdvicePK, 'PA00005001', @AddressPK)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("PreAdvicePK", preAdvicePK, JobShipmentPreplanningSchema.PK);
				command.AddParameterBasedOnDbColumn("AddressPK", addressPK, JobShipmentPreplanningSchema.EF_OA_BuyerAddress);
				command.ExecuteNonQuery();
			}

			var transport1PK = CreateLinkedTransport(sailing1PK, "SPA", preAdvicePK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], transport1PK, sailing1PK, preAdvicePK, "PA00005001", "SPA", Guid.Empty, false);
		}

		public void TestContainer()
		{
			var containerPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobContainer (JC_PK, JC_ContainerJobID, JC_JX)
VALUES (@ContainerPK, 'D00005001', @Sailing1PK)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("ContainerPK", containerPK, JobContainerSchema.PK);
				command.AddParameterBasedOnDbColumn("Sailing1PK", sailing1PK, JobContainerSchema.JC_JX);
				command.ExecuteNonQuery();
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], containerPK, sailing1PK, containerPK, "D00005001", "CNT", Guid.Empty, false);
		}

		public void TestDeclaration()
		{
			var declarationPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_IsCancelled, JE_ClusterKey)
VALUES (@DeclarationPK, @CountryCode, 'B00005001', @BranchPK, @CompanyPK, 0, 1)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("DeclarationPK", declarationPK, JobDeclarationSchema.PK);
				command.AddParameterBasedOnDbColumn("BranchPK", branchPK, JobDeclarationSchema.JE_GB);
				command.AddParameterBasedOnDbColumn("CompanyPK", companyPK, JobDeclarationSchema.JE_GC);
				command.AddParameterBasedOnDbColumn("CountryCode", countryCode, JobDeclarationSchema.JE_DataModel);
				command.ExecuteNonQuery();
			}

			var transport1PK = CreateLinkedTransport(sailing1PK, "DEC", declarationPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], transport1PK, sailing1PK, declarationPK, "B00005001", "DEC", companyPK, false);
		}

		public void TestCommercialInvoices()
		{
			var commercialInvoicePK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_InvoiceNumber, JZ_GB, JZ_ClusterKey)
VALUES (@CommercialInvoicePK, @CountryCode, 'CI00005001', @BranchPK, 1)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("CommercialInvoicePK", commercialInvoicePK, JobComInvoiceHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("BranchPK", branchPK, JobComInvoiceHeaderSchema.JZ_GB);
				command.AddParameterBasedOnDbColumn("CountryCode", countryCode, JobComInvoiceHeaderSchema.JZ_DataModel);
				command.ExecuteNonQuery();
			}

			var transport1PK = CreateLinkedTransport(sailing1PK, "INV", commercialInvoicePK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], transport1PK, sailing1PK, commercialInvoicePK, "CI00005001", "INV", companyPK, false);
		}

		public void TestTransportBookings()
		{
			var helper = new TestDbHelper(TestConnection);
			var dtbConsolidationPK = helper.InsertDtbBookingConsolidation("BKG", "CM00005001");
			var dtbBookingPK = helper.InsertDtbBooking("TB00005001", dtbConsolidationPK, branchPK);
			var transport1PK = CreateLinkedTransport(sailing1PK, "DTB", dtbConsolidationPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], transport1PK, sailing1PK, dtbBookingPK, "TB00005001", "DTB", companyPK, false);
		}

		public void TestPortTransport()
		{
			var helper = new TestDbHelper(TestConnection);
			var jobCartagePK = helper.InsertJobCartage("T00005001", branchPK, sailing1PK, false);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], jobCartagePK, sailing1PK, jobCartagePK, "T00005001", "TRN", companyPK, false);
		}

		public void TestImporterSecurityFiling()
		{
			var cusISFHeaderPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.CusISFHeader (BF_PK, BF_JobReference, BF_GB, BF_IsCancelled)
VALUES (@CusISFHeaderPK, 'T00005001', @BranchPK, 0)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("CusISFHeaderPK", cusISFHeaderPK, CusISFHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("BranchPK", branchPK, CusISFHeaderSchema.BF_GB);
				command.ExecuteNonQuery();
			}

			var transport1PK = CreateLinkedTransport(sailing1PK, "ISF", cusISFHeaderPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], transport1PK, sailing1PK, cusISFHeaderPK, "T00005001", "ISF", companyPK, false);
		}

		public void TestAsycudaManifestHeader()
		{
			var asycudaManifestPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_ClusterKey, AMA_JobReference, AMA_IsActive, AMA_RN_NKCountry, AMA_SystemCreateUser, AMA_SystemCreateTimeUtc, AMA_SystemLastEditUser, AMA_SystemLastEditTimeUtc, AMA_GB)
VALUES (@AsycudaManifestPK, 1, 'MAN0005001', 1, 'AU', 'E', GETUTCDATE(), 'E', GETUTCDATE(), @BranchPK)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("AsycudaManifestPK", asycudaManifestPK, AsycudaManifestHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("BranchPK", branchPK, AsycudaManifestHeaderSchema.AMA_GB);
				command.ExecuteNonQuery();
			}

			var transport1PK = CreateLinkedTransport(sailing1PK, "ASY", asycudaManifestPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewSailingRelatedJob ORDER BY VJX_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], transport1PK, sailing1PK, asycudaManifestPK, "MAN0005001", "ASY", Guid.Empty, false);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			sailing1PK = CreateSailing("AUBNE", "DEHAM", "AD1", "FANAL MARINER");
			sailing2PK = CreateSailing("AUSYD", "SGSIN", "AS1", "SYDNEY STAR");
			sailing3PK = CreateSailing("NZAKL", "HKHKG", "NH1", "TASCO");

			var sql = "SELECT TOP 1 GC_RN_NKCountryCode, GB_PK, GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			branchPK = (Guid)result.Rows[0]["GB_PK"];
			companyPK = (Guid)result.Rows[0]["GB_GC"];
			countryCode = (string)result.Rows[0]["GC_RN_NKCountryCode"];

			sql = "SELECT TOP 1 OA_PK FROM dbo.OrgAddress";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			addressPK = (Guid)result.Rows[0]["OA_PK"];
		}

		Guid CreateSailing(string origin, string destination, string voyage, string vessel)
		{
			var sailingPK = Guid.NewGuid();

			var sql = @"
DECLARE @JobVoyagePK UNIQUEIDENTIFIER = NEWID()
DECLARE @JobVoyOriginPK UNIQUEIDENTIFIER = NEWID()
DECLARE @JobVoyDestinationPK UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.JobVoyage (JV_PK, JV_AirSeaRoad, JV_VoyageFlight, JV_RV_NKVessel)
VALUES (@JobVoyagePK, 'SEA', @Voyage, @Vessel)

INSERT INTO dbo.JobVoyOrigin (JA_PK, JA_RL_NKPortOfLoading, JA_JV)
VALUES (@JobVoyOriginPK, @PortOfLoading, @JobVoyagePK)

INSERT INTO dbo.JobVoyDestination (JB_PK, JB_RL_NKPortOfDischarge, JB_JV)
VALUES (@JobVoyDestinationPK, @PortOfDischarge, @JobVoyagePK)

INSERT INTO dbo.JobSailing (JX_PK, JX_JA, JX_JB)
VALUES (@JobSailingPK, @JobVoyOriginPK, @JobVoyDestinationPK)
";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("JobSailingPK", sailingPK, JobSailingSchema.PK);
				command.AddParameterBasedOnDbColumn("Voyage", voyage, JobVoyageSchema.JV_VoyageFlight);
				command.AddParameterBasedOnDbColumn("Vessel", vessel, JobVoyageSchema.JV_RV_NKVessel);
				command.AddParameterBasedOnDbColumn("PortOfLoading", origin, JobVoyOriginSchema.JA_RL_NKPortOfLoading);
				command.AddParameterBasedOnDbColumn("PortOfDischarge", destination, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge);
				command.ExecuteNonQuery();
			}

			return sailingPK;
		}

		Guid CreateLinkedTransport(Guid sailingPK, string parentType, Guid parentPK)
		{
			var transportPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobConsolTransport (JW_PK, JW_TransportMode, JW_Vessel, JW_VoyageFlight, JW_IsLinked, JW_RL_NKLoadPort, JW_RL_NKDiscPort, JW_JX, JW_ParentType, JW_ParentGUID)
SELECT TOP 1 @TransportPK, JV_AirSeaRoad, JV_RV_NKVessel, JV_VoyageFlight, 1, JA_RL_NKPortOfLoading, JB_RL_NKPortOfDischarge, @SailingPK, @ParentType, @ParentPK
FROM dbo.JobSailing
	JOIN dbo.JobVoyOrigin ON JA_PK = JX_JA
	JOIN dbo.JobVoyDestination ON JB_PK = JX_JB
	JOIN dbo.JobVoyage ON JV_PK = JA_JV
WHERE JX_PK = @SailingPK";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("TransportPK", transportPK, JobConsolTransportSchema.PK);
				command.AddParameterBasedOnDbColumn("SailingPK", sailingPK, JobSailingSchema.PK);
				command.AddParameterBasedOnDbColumn("ParentType", parentType, JobConsolTransportSchema.JW_ParentType);
				command.AddParameterBasedOnDbColumn("ParentPK", parentPK, JobConsolTransportSchema.JW_ParentGUID);
				command.ExecuteNonQuery();
			}

			return transportPK;
		}

		void AssertRowValues(DataRow row, Guid expectedPK, Guid expectedSailingPK, Guid expectedRelatedJobPK, string expectedJobNumber, string expectedJobType, Guid expectedCompanyPK, bool expectedIsCancelled)
		{
			AssertEquals("VJX_PK", expectedPK, (Guid)row["VJX_PK"]);
			AssertEquals("VJX_JX", expectedSailingPK, (Guid)row["VJX_JX"]);
			AssertEquals("VJX_RelatedJobID", expectedRelatedJobPK, (Guid)row["VJX_RelatedJobID"]);
			AssertEquals("VJX_JobNumber", expectedJobNumber, row["VJX_JobNumber"].ToString());
			AssertEquals("VJX_JobType", expectedJobType, row["VJX_JobType"].ToString());
			AssertEquals("VJX_IsCancelled", expectedIsCancelled, (bool)row["VJX_IsCancelled"]);

			if (expectedCompanyPK == Guid.Empty)
			{
				AssertEquals("VJX_CompanyPK", DBNull.Value, row["VJX_GC"]);
			}
			else
			{
				AssertEquals("VJX_CompanyPK", expectedCompanyPK, (Guid)row["VJX_GC"]);
			}
		}

		Guid sailing1PK;
		Guid sailing2PK;
		Guid sailing3PK;

		Guid branchPK;
		Guid companyPK;
		string countryCode;

		Guid addressPK;
		#endregion
	}
}

