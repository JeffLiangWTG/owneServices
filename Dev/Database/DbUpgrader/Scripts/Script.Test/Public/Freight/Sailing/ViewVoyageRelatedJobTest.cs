using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Sailing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Sailing
{
	[TestedType(typeof(ViewVoyageRelatedJob))]
	class ViewVoyageRelatedJobTest : DbCreateScriptTest
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

			CreateLinkedTransport(voyage1PK, "CON", forwardingConsolPK);
			CreateLinkedTransport(voyage2PK, "CON", loadListPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 2, result.Rows.Count);
			AssertRowValues(result.Rows[0], forwardingConsolPK, voyage1PK, "C00001234", "CON", Guid.Empty, false);
			AssertRowValues(result.Rows[1], loadListPK, voyage2PK, "L00001234", "CLL", Guid.Empty, false);
		}

		public void TestShipments()
		{
			var cfsShipmentPK = Guid.NewGuid();
			var billOfLading1PK = Guid.NewGuid();
			var billOfLading2PK = Guid.NewGuid();
			var agencyBooking1PK = Guid.NewGuid();
			var agencyBooking2PK = Guid.NewGuid();
			var bookingPK = Guid.NewGuid();
			var forwardingShipmentPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_JX, JS_IsForwardRegistered, JS_IsBooking, JS_IsCFSRegistered, JS_IsShipping, JS_ShipmentStatus, JS_IsCancelled)
VALUES
	(@CfsShipmentPK, 'S00005001', @Sailing1PK, 0, 0, 1, 0, '', 0),
	(@BillOfLading1PK, 'S00005002', @Sailing2PK, 0, 0, 0, 1, 'CNF', 0),
	(@AgencyBooking1PK, 'S00005003', @Sailing3PK, 0, 0, 0, 1, 'BKD', 0),
	(@BookingPK, 'S00005004', @Sailing1PK, 0, 1, 0, 0, '', 0),
	(@ForwardingShipmentPK, 'S00005005', null, 1, 0, 0, 0, '', 0),
	(@BillOfLading2PK, 'S00005006', null, 0, 0, 0, 1, 'WFI', 0),
	(@AgencyBooking2PK, 'S00005007', null, 0, 0, 0, 1, 'BKD', 0)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("CfsShipmentPK", cfsShipmentPK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("BillOfLading1PK", billOfLading1PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("AgencyBooking1PK", agencyBooking1PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("BookingPK", bookingPK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("ForwardingShipmentPK", forwardingShipmentPK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("BillOfLading2PK", billOfLading2PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("AgencyBooking2PK", agencyBooking2PK, JobShipmentSchema.PK);
				command.AddParameterBasedOnDbColumn("Sailing1PK", sailing1PK, JobShipmentSchema.JS_JX);
				command.AddParameterBasedOnDbColumn("Sailing2PK", sailing2PK, JobShipmentSchema.JS_JX);
				command.AddParameterBasedOnDbColumn("Sailing3PK", sailing3PK, JobShipmentSchema.JS_JX);
				command.ExecuteNonQuery();
			}

			CreateLinkedTransport(voyage2PK, "SHP", forwardingShipmentPK);
			CreateLinkedTransport(voyage3PK, "ASH", billOfLading2PK);
			CreateLinkedTransport(voyage1PK, "ASH", agencyBooking2PK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 7, result.Rows.Count);
			AssertRowValues(result.Rows[0], cfsShipmentPK, voyage1PK, "S00005001", "CSH", Guid.Empty, false);
			AssertRowValues(result.Rows[1], billOfLading1PK, voyage2PK, "S00005002", "ASH", Guid.Empty, false);
			AssertRowValues(result.Rows[2], agencyBooking1PK, voyage3PK, "S00005003", "AGB", Guid.Empty, false);
			AssertRowValues(result.Rows[3], bookingPK, voyage1PK, "S00005004", "QSH", Guid.Empty, false);
			AssertRowValues(result.Rows[4], forwardingShipmentPK, voyage2PK, "S00005005", "SHP", Guid.Empty, false);
			AssertRowValues(result.Rows[5], billOfLading2PK, voyage3PK, "S00005006", "ASH", Guid.Empty, false);
			AssertRowValues(result.Rows[6], agencyBooking2PK, voyage1PK, "S00005007", "AGB", Guid.Empty, false);
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

			CreateLinkedTransport(voyage1PK, "SPA", preAdvicePK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], preAdvicePK, voyage1PK, "PA00005001", "SPA", Guid.Empty, false);
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

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], containerPK, voyage1PK, "D00005001", "CNT", Guid.Empty, false);
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

			CreateLinkedTransport(voyage1PK, "DEC", declarationPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], declarationPK, voyage1PK, "B00005001", "DEC", companyPK, false);
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

			CreateLinkedTransport(voyage1PK, "INV", commercialInvoicePK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], commercialInvoicePK, voyage1PK, "CI00005001", "INV", companyPK, false);
		}

		public void TestTransportBookings()
		{
			var helper = new TestDbHelper(TestConnection);
			var dtbConsolidationPK = helper.InsertDtbBookingConsolidation("BKG", "CM00005001");
			var dtbBookingPK = helper.InsertDtbBooking("TB00005001", dtbConsolidationPK, branchPK, true);

			CreateLinkedTransport(voyage1PK, "DTB", dtbConsolidationPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], dtbBookingPK, voyage1PK, "TB00005001", "DTB", companyPK, false);
		}

		public void TestPortTransport()
		{
			var helper = new TestDbHelper(TestConnection);
			var jobCartagePK = helper.InsertJobCartage("T00005001", branchPK, sailing1PK, false);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], jobCartagePK, voyage1PK, "T00005001", "TRN", companyPK, false);
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

			CreateLinkedTransport(voyage1PK, "ISF", cusISFHeaderPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], cusISFHeaderPK, voyage1PK, "T00005001", "ISF", companyPK, false);
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

			CreateLinkedTransport(voyage1PK, "ASY", asycudaManifestPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ViewVoyageRelatedJob ORDER BY VJV_JobNumber");
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertRowValues(result.Rows[0], asycudaManifestPK, voyage1PK, "MAN0005001", "ASY", Guid.Empty, false);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			voyage1PK = Guid.NewGuid();
			voyage2PK = Guid.NewGuid();
			voyage3PK = Guid.NewGuid();

			sailing1PK = Guid.NewGuid();
			sailing2PK = Guid.NewGuid();
			sailing3PK = Guid.NewGuid();

			CreateVoyage(voyage1PK, sailing1PK, "AUBNE", "DEHAM", "AD1", "FANAL MARINER");
			CreateVoyage(voyage2PK, sailing2PK, "AUSYD", "SGSIN", "AS1", "SYDNEY STAR");
			CreateVoyage(voyage3PK, sailing3PK, "NZAKL", "HKHKG", "NH1", "TASCO");

			var sql = "SELECT TOP 1 GC_RN_NKCountryCode, GB_PK, GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			branchPK = (Guid)result.Rows[0]["GB_PK"];
			companyPK = (Guid)result.Rows[0]["GB_GC"];
			countryCode = (string)result.Rows[0]["GC_RN_NKCountryCode"];

			sql = "SELECT TOP 1 OA_PK FROM dbo.OrgAddress";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			addressPK = (Guid)result.Rows[0]["OA_PK"];
		}

		void CreateVoyage(Guid voyagePK, Guid sailingPK, string origin, string destination, string voyage, string vessel)
		{
			var sql = @"
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
				command.AddParameterBasedOnDbColumn("JobVoyagePK", voyagePK, JobVoyageSchema.PK);
				command.AddParameterBasedOnDbColumn("JobSailingPK", sailingPK, JobSailingSchema.PK);
				command.AddParameterBasedOnDbColumn("Voyage", voyage, JobVoyageSchema.JV_VoyageFlight);
				command.AddParameterBasedOnDbColumn("Vessel", vessel, JobVoyageSchema.JV_RV_NKVessel);
				command.AddParameterBasedOnDbColumn("PortOfLoading", origin, JobVoyOriginSchema.JA_RL_NKPortOfLoading);
				command.AddParameterBasedOnDbColumn("PortOfDischarge", destination, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge);
				command.ExecuteNonQuery();
			}
		}

		void CreateLinkedTransport(Guid voyagePK, string parentType, Guid parentPK)
		{
			var sql = @"
INSERT INTO dbo.JobConsolTransport (JW_PK, JW_TransportMode, JW_Vessel, JW_VoyageFlight, JW_IsLinked, JW_RL_NKLoadPort, JW_RL_NKDiscPort, JW_JX, JW_ParentType, JW_ParentGUID)
SELECT TOP 1 NEWID(), JV_AirSeaRoad, JV_RV_NKVessel, JV_VoyageFlight, 1, JA_RL_NKPortOfLoading, JB_RL_NKPortOfDischarge, JX_PK, @ParentType, @ParentPK
FROM dbo.JobSailing
	JOIN dbo.JobVoyOrigin ON JA_PK = JX_JA
	JOIN dbo.JobVoyDestination ON JB_PK = JX_JB
	JOIN dbo.JobVoyage ON JV_PK = JA_JV
WHERE JV_PK = @VoyagePK";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("VoyagePK", voyagePK, JobVoyageSchema.PK);
				command.AddParameterBasedOnDbColumn("ParentType", parentType, JobConsolTransportSchema.JW_ParentType);
				command.AddParameterBasedOnDbColumn("ParentPK", parentPK, JobConsolTransportSchema.JW_ParentGUID);
				command.ExecuteNonQuery();
			}
		}

		void AssertRowValues(DataRow row, Guid expectedPK, Guid expectedVoyagePK, string expectedJobNumber, string expectedJobType, Guid expectedCompanyPK, bool expectedIsCancelled)
		{
			AssertEquals("VJV_PK", expectedPK, (Guid)row["VJV_PK"]);
			AssertEquals("VJV_JV", expectedVoyagePK, (Guid)row["VJV_JV"]);
			AssertEquals("VJV_JobNumber", expectedJobNumber, row["VJV_JobNumber"].ToString());
			AssertEquals("VJV_JobType", expectedJobType, row["VJV_JobType"].ToString());
			AssertEquals("VJV_IsCancelled", expectedIsCancelled, (bool)row["VJV_IsCancelled"]);

			if (expectedCompanyPK == Guid.Empty)
			{
				AssertEquals("VJV_CompanyPK", DBNull.Value, row["VJV_GC"]);
			}
			else
			{
				AssertEquals("VJV_CompanyPK", expectedCompanyPK, (Guid)row["VJV_GC"]);
			}
		}

		Guid voyage1PK;
		Guid voyage2PK;
		Guid voyage3PK;

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

