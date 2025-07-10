using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	[TestedType(typeof(Report_Client_eTailShipmentProfile))]
	internal class Report_Client_eTailShipmentProfileTest : DbCreateScriptTest
	{
		public void TestColumnsLCL_ULD_LSE_AvailableDateAreAdded()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var lclAvailableDate = DateTime.Today.AddDays(2);
				var uldAvailableDate = DateTime.Today.AddDays(3);
				var lseAvailableDate = DateTime.Today.AddDays(4);

				var testDbHelper = new TestDbHelper(Db.Connection);
				var userCode = TestDbHelper.UserStaffCode;
				var organisationPK = TestDataCreator.CreateOrganisation(orgCode, orgName);
				var addressPK = TestDataCreator.CreateAddress(organisationPK, addCode, address1);
				var shipmentPK = testDbHelper.InsertShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);
				testDbHelper.InsertJobDocsAndCartage(shipmentPK, "JS", lclAvailableDate);
				var voyagePK = testDbHelper.InsertJobVoyage("Turtle");
				var originPK1 = testDbHelper.InsertJobVoyOrigin("USCHI", voyagePK);
				var destinationPK1 = testDbHelper.InsertJobVoyDestination("AUMEL", voyagePK, uldAvailableDate.AddDays(-1));
				var originPK2 = testDbHelper.InsertJobVoyOrigin("AUMEL", voyagePK);
				var destinationPK2 = testDbHelper.InsertJobVoyDestination("AUSYD", voyagePK, uldAvailableDate);
				var sailingPK1 = testDbHelper.InsertJobSailing(originPK1, destinationPK1, lseAvailableDate.AddDays(-5));
				var sailingPK2 = testDbHelper.InsertJobSailing(originPK2, destinationPK2, lseAvailableDate);
				var consolPK = testDbHelper.InsertConsol("C00012345", "USCHI", "AUSYD");
				testDbHelper.InsertJobConShipLink(consolPK, shipmentPK);
				testDbHelper.InsertJobConsolTransport(consolPK, "CON", sailingPK1, DateTime.Today, DateTime.Today.AddDays(1), 1);
				testDbHelper.InsertJobConsolTransport(consolPK, "CON", sailingPK2, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), 2);

				var hvlvBookingHeaderPK = CreateHVLVBookingHeader(addressPK, clusterKey, bookingReference, userCode);
				var hvlvConsignmentPK = CreateHVLVConsignment(hvlvBookingHeaderPK, clusterKey, userCode, consignmentId);
				CreateHVLVItem(hvlvConsignmentPK, clusterKey, shipmentPK, itemId);

				DataTable result = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM Report_Client_eTailShipmentProfile(null, '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '')");

				AssertEquals("Result should have one row", 1, result.Rows.Count);
				AssertEquals($"LCL Available Date should be [{lclAvailableDate}]", lclAvailableDate, result.Rows[0]["LCLAvailableDate"]);
				AssertEquals($"ULD Available Date should be [{uldAvailableDate}]", uldAvailableDate, result.Rows[0]["ULDAvailableDate"]);
				AssertEquals($"LSE Available Date should be [{lseAvailableDate}]", lseAvailableDate, result.Rows[0]["LSEAvailableDate"]);
			}
		}

		public void TestReport_eTailShipmentProfile()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var testDbHelper = new TestDbHelper(Db.Connection);
				var userCode = TestDbHelper.UserStaffCode;
				var organisationPK = TestDataCreator.CreateOrganisation(orgCode, orgName);
				var addressPK = TestDataCreator.CreateAddress(organisationPK, addCode, address1);
				var shipmentPK = testDbHelper.InsertShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);

				var hvlvBookingHeaderPK = CreateHVLVBookingHeader(addressPK, clusterKey, bookingReference, userCode);
				var hvlvConsignmentPK = CreateHVLVConsignment(hvlvBookingHeaderPK, clusterKey, userCode, consignmentId);
				var hvlvItemPK = CreateHVLVItem(hvlvConsignmentPK, clusterKey, shipmentPK, itemId);

				DataTable result = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM Report_Client_eTailShipmentProfile(null, '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '')");

				AssertEquals("Result should have one row", 1, result.Rows.Count);
				AssertEquals($"ShipmentID should be [{shipmentNumber}]", shipmentNumber, result.Rows[0]["ShipmentID"]);
				AssertEquals($"Origin should be [{origin}]", origin, result.Rows[0]["Origin"]);
				AssertEquals($"Origin should be [{destination}]", destination, result.Rows[0]["Destination"]);
			}
		}

		public void TestReport_eTailShipmentProfile_ExceptionItems()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var testDbHelper = new TestDbHelper(Db.Connection);
				var userCode = TestDbHelper.UserStaffCode;
				var organisationPK = TestDataCreator.CreateOrganisation(orgCode, orgName);
				var addressPK = TestDataCreator.CreateAddress(organisationPK, addCode, address1);
				var shipmentPK = testDbHelper.InsertShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);

				var hvlvBookingHeaderPK = CreateHVLVBookingHeader(addressPK, clusterKey, bookingReference, userCode);
				var hvlvConsignmentPK = CreateHVLVConsignment(hvlvBookingHeaderPK, clusterKey, userCode, consignmentId);
				var hvlvItemPK = CreateHVLVItem(hvlvConsignmentPK, clusterKey, shipmentPK, itemId, SurplusAtDestinationDepot);

				var result = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM Report_Client_eTailShipmentProfile(null, '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', 'Y')");

				AssertEquals("Result should have one row", 1, result.Rows.Count);
			}
		}

		public void TestReport_eTailShipmentProfile_FilterClient()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var testDbHelper = new TestDbHelper(Db.Connection);

				var shipmentPK1 = PrepTestReport_eTailShipmentItem(testDbHelper, "shipment001", "TSTORG1", clusterKey: 1, bookingReference: "TSTBookRef1");
				var shipmentPK2 = PrepTestReport_eTailShipmentItem(testDbHelper, "shipment002", "TSTORG2", clusterKey: 2, bookingReference: "TSTBookRef2");
				var shipmentPK3 = PrepTestReport_eTailShipmentItem(testDbHelper, "shipment003", "TSTORG3", clusterKey: 3, bookingReference: "TSTBookRef3");
				var shipmentPK4 = PrepTestReport_eTailShipmentItem(testDbHelper, "shipment004", "TSTORG4", clusterKey: 4, bookingReference: "TSTBookRef4");

				var organisationPK1 = TestDataCreator.CreateOrganisation("TSTORG11", "Test Organisation 11");
				var addressPk1 = TestDataCreator.CreateAddress(organisationPK1, "Address1", "Test Address 1");
				_ = TestDataCreator.CreateDocAddress(addressPk1, "", shipmentPK1, "JS", "CRD");

				var organisationPK2 = TestDataCreator.CreateOrganisation("TSTORG22", "Test Organisation 22");
				var addressPk2 = TestDataCreator.CreateAddress(organisationPK2, "Address2", "Test Address 2");
				_ = TestDataCreator.CreateDocAddress(addressPk2, "", shipmentPK2, "JS", "CED");

				var organisationPK3 = TestDataCreator.CreateOrganisation("TSTORG33", "Test Organisation 33");
				var addressPk3 = TestDataCreator.CreateAddress(organisationPK3, "Address3", "Test Address 3");
				CreateJobHeader("TestJobNum", shipmentPK3, addressPk3);

				_ = TestDataCreator.CreateDocAddress(addressPk1, "", shipmentPK4, "JS", "CRD");
				_ = TestDataCreator.CreateDocAddress(addressPk2, "", shipmentPK4, "JS", "CED");

				var result1 = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM Report_Client_eTailShipmentProfile(null, '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '')");
				AssertEquals("Result should have four row when Client is null", 4, result1.Rows.Count);

				var result2 = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM Report_Client_eTailShipmentProfile('{organisationPK1}', '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '')");
				AssertEquals("Result should have two row when Client is eTailer", 2, result2.Rows.Count);

				var result3 = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM Report_Client_eTailShipmentProfile('{organisationPK2}', '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '')");
				AssertEquals("Result should have two row when Client is Consignee", 2, result3.Rows.Count);

				var result4 = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM Report_Client_eTailShipmentProfile('{organisationPK3}', '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '')");
				AssertEquals("Result should have one row when Client is Local Client", 1, result4.Rows.Count);
			}

			void CreateJobHeader(string jobNum, Guid shipmentPK, Guid localClientAddress)
			{
				var companyPK = TestDbHelper.DefaultCompanyPK;
				var branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "AAA", homePort: "TESTHOME", countryCode: "AU");
				var departmentPK = TestDataCreator.CreateDepartment("BBB");

				var jobHeader = new ActiveRowWrapper(JobHeaderSchema.Instance)
				{
					[JobHeaderSchema.JH_JobNum] = jobNum,
					[JobHeaderSchema.JH_ParentTableCode] = "JS",
					[JobHeaderSchema.JH_ParentID] = shipmentPK,
					[JobHeaderSchema.JH_OA_LocalChargesAddr] = localClientAddress,
					[JobHeaderSchema.JH_HeaderType] = "JOB",
					[JobHeaderSchema.JH_Status] = "WRK",
					[JobHeaderSchema.JH_GB] = branchPK,
					[JobHeaderSchema.JH_GE] = departmentPK,
					[JobHeaderSchema.JH_GC] = companyPK,
				};

				jobHeader.Save();
			}

			Guid PrepTestReport_eTailShipmentItem(TestDbHelper testDbHelper, string shipmentNumber, string orgCode, int clusterKey, string bookingReference)
			{
				var userCode = TestDbHelper.UserStaffCode;
				var organisationPK = TestDataCreator.CreateOrganisation(orgCode, orgName);
				var addressPK = TestDataCreator.CreateAddress(organisationPK, addCode, address1);
				var shipmentPK = testDbHelper.InsertShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);

				var hvlvBookingHeaderPK = CreateHVLVBookingHeader(addressPK, clusterKey, bookingReference, userCode);
				var hvlvConsignment = CreateHVLVConsignment(hvlvBookingHeaderPK, clusterKey, userCode, consignmentId);

				_ = CreateHVLVItem(hvlvConsignment, clusterKey, shipmentPK, itemId, SurplusAtDestinationDepot);
				return shipmentPK;
			}
		}

		#region Implementation

		static readonly int clusterKey = 1000;
		static readonly string orgCode = "TSTORG";
		static readonly string orgName = "Test Organisation";
		static readonly string addCode = "TSTORGADD";
		static readonly string address1 = "Test Organisation Address1";
		static readonly string shipmentNumber = "TSTShipment";
		static readonly string transportMode = "SEA";
		static readonly string packingMode = "LSE";
		static readonly string origin = "NZAKL";
		static readonly string destination = "AUSYD";
		static readonly string bookingReference = "TSTBookRef";
		static readonly string consignmentId = "TSTConsignmentId";
		static readonly string itemId = "TSTItemId";
		const string ManifestedByETailer = "MAN";
		const string SurplusAtDestinationDepot = "SUD";

		Guid CreateHVLVBookingHeader(Guid addressPK, int clusterKey, string bookingReference, string userCode)
		{
			var hvlvBookingHeader = new ActiveRowWrapper(HVLVBookingHeaderSchema.Instance)
			{
				[HVLVBookingHeaderSchema.HVH_BookingReference] = bookingReference,
				[HVLVBookingHeaderSchema.HVH_ClusterKey] = clusterKey,
				[HVLVBookingHeaderSchema.HVH_OA_BillToParty] = addressPK,
				[HVLVBookingHeaderSchema.HVH_SystemCreateTimeUtc] = DateTime.UtcNow,
				[HVLVBookingHeaderSchema.HVH_SystemCreateUser] = userCode,
				[HVLVBookingHeaderSchema.HVH_SystemLastEditTimeUtc] = DateTime.UtcNow,
				[HVLVBookingHeaderSchema.HVH_SystemLastEditUser] = userCode,
			};

			hvlvBookingHeader.Save();
			return hvlvBookingHeader.PK;
		}

		Guid CreateHVLVConsignment(Guid hvlvBookingHeaderPK, int clusterKey, string userCode, string consignmentId)
		{
			var hvlvConsignment = new ActiveRowWrapper(HVLVConsignmentSchema.Instance)
			{
				[HVLVConsignmentSchema.HVC_ClusterKey] = clusterKey,
				[HVLVConsignmentSchema.HVC_ConsignmentId] = consignmentId,
				[HVLVConsignmentSchema.HVC_HVH_BookingHeader] = hvlvBookingHeaderPK,
				[HVLVConsignmentSchema.HVC_Status] = "BKD",
				[HVLVConsignmentSchema.HVC_SystemCreateTimeUtc] = DateTime.UtcNow,
				[HVLVConsignmentSchema.HVC_SystemCreateUser] = userCode,
				[HVLVConsignmentSchema.HVC_SystemLastEditTimeUtc] = DateTime.UtcNow,
				[HVLVConsignmentSchema.HVC_SystemLastEditUser] = userCode,
			};

			hvlvConsignment.Save();
			return hvlvConsignment.PK;
		}

		Guid CreateHVLVItem(Guid hvlvConsignmentPK, int clusterKey, Guid shipmentPK, string itemId, string status = null)
		{
			var hvlvItem = new ActiveRowWrapper(HVLVItemSchema.Instance)
			{
				[HVLVItemSchema.HVI_ClusterKey] = clusterKey,
				[HVLVItemSchema.HVI_HVC_Consignment] = hvlvConsignmentPK,
				[HVLVItemSchema.HVI_JS_LoadedOnShipment] = shipmentPK,
				[HVLVItemSchema.HVI_ItemId] = itemId,
				[HVLVItemSchema.HVI_Status] = status ?? ManifestedByETailer,
				[HVLVItemSchema.HVI_IsScannedAtDestination] = status == SurplusAtDestinationDepot,
			};

			hvlvItem.Save();
			return hvlvItem.PK;
		}
		#endregion
	}
}
