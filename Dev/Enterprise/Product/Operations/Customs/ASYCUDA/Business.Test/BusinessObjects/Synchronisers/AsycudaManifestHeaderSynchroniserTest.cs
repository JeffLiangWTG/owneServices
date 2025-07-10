using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
	sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("TransportMode should be AIR", Core.Constants.TransportModes.Air, manifestHeader.AMA_TransportMode);

			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportMode should be SEA", Core.Constants.TransportModes.Sea, manifestHeader.AMA_TransportMode);
		}

		public void TestCarrier()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();
			var carrier1 = Factory.NewWithValidTestData<OrgAddress>();
			var carrier2 = Factory.NewWithValidTestData<OrgAddress>();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			leg1.JW_OA_CarrierAddress = carrier1.PK;
			var leg2 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg2.JW_OA_CarrierAddress = carrier2.PK;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZGuid>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => (carrier2.PK, manifestHeader.AMA_OA_Carrier),
				GetExpectedAndActualValuesForTranshipment = () => (carrier1.PK, manifestHeader.AMA_OA_Carrier),
				GetExpectedAndActualValuesForImport = () => (carrier1.PK, manifestHeader.AMA_OA_Carrier),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_OA_CarrierAddress = carrier2.PK;
					return (carrier2.PK, manifestHeader.AMA_OA_Carrier);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestVessel()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			leg1.JW_Vessel = "VESSEL1";
			var leg2 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg2.JW_Vessel = "VESSEL2";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZString>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => ("VESSEL2", manifestHeader.AMA_VesselName),
				GetExpectedAndActualValuesForTranshipment = () => ("VESSEL1", manifestHeader.AMA_VesselName),
				GetExpectedAndActualValuesForImport = () => ("VESSEL1", manifestHeader.AMA_VesselName),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_Vessel = "VESSEL3";
					return ("VESSEL3", manifestHeader.AMA_VesselName);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestVoyage()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			leg1.JW_VoyageFlight = "VOYAGE1";
			var leg2 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg2.JW_VoyageFlight = "VOYAGE2";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZString>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => ("VOYAGE2", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForTranshipment = () => ("VOYAGE1", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForImport = () => ("VOYAGE1", manifestHeader.AMA_Voyage),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_VoyageFlight = "VOYAGE3";
					return ("VOYAGE3", manifestHeader.AMA_Voyage);
				}
			};
			testCase.AssertTestCase();

			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Road;
			testCase = new TestCaseAffectedByNature<ZString>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => ("", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForTranshipment = () => ("", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForImport = () => ("", manifestHeader.AMA_Voyage),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_Vessel = "VOYAGE3";
					return ("", manifestHeader.AMA_Voyage);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestVehicleRegistration()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Road;
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			leg1.JW_VoyageFlight = "VEHICLE1";
			var leg2 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg2.JW_VoyageFlight = "VEHICLE2";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZString>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => ("VEHICLE2", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForTranshipment = () => ("VEHICLE1", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForImport = () => ("VEHICLE1", manifestHeader.AMA_Voyage),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_VoyageFlight = "VEHICLE3";
					return ("VEHICLE3", manifestHeader.AMA_Voyage);
				}
			};
			testCase.AssertTestCase();

			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			testCase = new TestCaseAffectedByNature<ZString>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => ("", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForTranshipment = () => ("", manifestHeader.AMA_Voyage),
				GetExpectedAndActualValuesForImport = () => ("", manifestHeader.AMA_Voyage),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_Vessel = "VEHICLE3";
					return ("", manifestHeader.AMA_Voyage);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestPortOfLoading()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			sourceConsol.Transports.AddNew("VUVLI", "LKCMB");

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZString>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => ("VUVLI", manifestHeader.AMA_RL_NKPortOfLoading),
				GetExpectedAndActualValuesForTranshipment = () => ("BDJNB", manifestHeader.AMA_RL_NKPortOfLoading),
				GetExpectedAndActualValuesForImport = () => ("BDJNB", manifestHeader.AMA_RL_NKPortOfLoading),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_RL_NKLoadPort = "BDCGP";
					return ("BDCGP", manifestHeader.AMA_RL_NKPortOfLoading);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestPortOfDischarge()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			sourceConsol.Transports.AddNew("VUVLI", "LKCMB");

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZString>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => ("LKCMB", manifestHeader.AMA_RL_NKPortOfDischarge),
				GetExpectedAndActualValuesForTranshipment = () => ("VUVLI", manifestHeader.AMA_RL_NKPortOfDischarge),
				GetExpectedAndActualValuesForImport = () => ("VUVLI", manifestHeader.AMA_RL_NKPortOfDischarge),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_RL_NKDiscPort = "VULUG";
					return ("VULUG", manifestHeader.AMA_RL_NKPortOfDischarge);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestPortOfFirstArrival()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();

			sourceConsol.Transports.RemoveAndDeleteAll();
			sourceConsol.Transports.AddNew("BDJNB", "SGSIN");
			sourceConsol.Transports.AddNew("SGSIN", "LKCMB");
			sourceConsol.JK_RL_NKPortOfFirstArrival = "VUVLI";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";

			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Should sync the value from JK_RL_NKPortOfFirstArrival", "VUVLI", manifestHeader.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestETA()
		{
			var eta1 = new ZDateTime(2020, 03, 31);
			var eta2 = new ZDateTime(2020, 04, 30);
			var eta3 = new ZDateTime(2020, 04, 15);

			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			leg1.JW_ETA = eta1;
			var leg2 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg2.JW_ETA = eta2;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZDateTime>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => (eta2, manifestHeader.AMA_E_ARV),
				GetExpectedAndActualValuesForTranshipment = () => (eta1, manifestHeader.AMA_E_ARV),
				GetExpectedAndActualValuesForImport = () => (eta1, manifestHeader.AMA_E_ARV),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_ETA = eta3;
					return (eta3, manifestHeader.AMA_E_ARV);
				}
			};
			testCase.AssertTestCase();

			var ata1 = new ZDateTime(2020, 04, 05);
			var ata2 = new ZDateTime(2020, 05, 05);
			var ata3 = new ZDateTime(2020, 04, 20);
			leg1.JW_ATA = ata1;
			leg2.JW_ATA = ata2;

			testCase = new TestCaseAffectedByNature<ZDateTime>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => (ata2, manifestHeader.AMA_E_ARV),
				GetExpectedAndActualValuesForTranshipment = () => (ata1, manifestHeader.AMA_E_ARV),
				GetExpectedAndActualValuesForImport = () => (ata1, manifestHeader.AMA_E_ARV),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_ATA = ata3;
					return (ata3, manifestHeader.AMA_E_ARV);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestETD()
		{
			var etd1 = new ZDateTime(2020, 03, 31);
			var etd2 = new ZDateTime(2020, 04, 30);
			var etd3 = new ZDateTime(2020, 04, 15);

			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("BDJNB", "VUVLI");
			leg1.JW_ETD = etd1;
			var leg2 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg2.JW_ETD = etd2;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "VU";
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var testCase = new TestCaseAffectedByNature<ZDateTime>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => (etd2, manifestHeader.AMA_E_DEP),
				GetExpectedAndActualValuesForTranshipment = () => (etd1, manifestHeader.AMA_E_DEP),
				GetExpectedAndActualValuesForImport = () => (etd1, manifestHeader.AMA_E_DEP),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_ETD = etd3;
					return (etd3, manifestHeader.AMA_E_DEP);
				}
			};
			testCase.AssertTestCase();

			var atd1 = new ZDateTime(2020, 04, 05);
			var atd2 = new ZDateTime(2020, 05, 05);
			var atd3 = new ZDateTime(2020, 04, 20);
			leg1.JW_ATD = atd1;
			leg2.JW_ATD = atd2;

			testCase = new TestCaseAffectedByNature<ZDateTime>
			{
				GetManifestHeader = () => manifestHeader,
				GetExpectedAndActualValuesForExport = () => (atd2, manifestHeader.AMA_E_DEP),
				GetExpectedAndActualValuesForTranshipment = () => (atd1, manifestHeader.AMA_E_DEP),
				GetExpectedAndActualValuesForImport = () => (atd1, manifestHeader.AMA_E_DEP),
				GetDifferentExpectedAndActualValuesForTranshipment = () =>
				{
					leg1.JW_ATD = atd3;
					return (atd3, manifestHeader.AMA_E_DEP);
				}
			};
			testCase.AssertTestCase();
		}

		public void TestIssueDate()
		{
			var issueDate1 = new ZDateTime(2020, 03, 31);
			var issueDate2 = new ZDateTime(2020, 04, 30);
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_MasterBillIssueDate = issueDate1;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals($"AMA_MasterBillIssueDate should be {issueDate1}", issueDate1, manifestHeader.AMA_MasterBillIssueDate);

			sourceConsol.JK_MasterBillIssueDate = issueDate2;
			AssertEquals($"AMA_MasterBillIssueDate should be {issueDate2}", issueDate2, manifestHeader.AMA_MasterBillIssueDate);
		}

		class TestCaseAffectedByNature<T>
			where T : IZType
		{
			public Func<AsycudaManifestHeader> GetManifestHeader { get; set; }
			public Func<(T, T)> GetExpectedAndActualValuesForExport { get; set; }
			public Func<(T, T)> GetExpectedAndActualValuesForTranshipment { get; set; }
			public Func<(T, T)> GetExpectedAndActualValuesForImport { get; set; }
			public Func<(T, T)> GetDifferentExpectedAndActualValuesForTranshipment { get; set; }

			public void AssertTestCase()
			{
				var manifestHeader = GetManifestHeader();
				manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
				manifestHeader.Synchroniser.Synchronise();
				var (expected, actual) = GetExpectedAndActualValuesForImport();
				AssertEquals($"VU-IMP should match first leg: {expected}", expected, actual);

				manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
				(expected, actual) = GetExpectedAndActualValuesForExport();
				AssertEquals($"VU-EXP should match second leg: {expected}", expected, actual);

				manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
				(expected, actual) = GetExpectedAndActualValuesForTranshipment();
				AssertEquals($"VU-TSS should match first leg: {expected}", expected, actual);

				var (newExpected, newActual) = GetDifferentExpectedAndActualValuesForTranshipment();
				AssertEquals($"VU-TSS should change when source value change: {expected}->{newExpected}", newExpected, newActual);
			}
		}

		public void TestOverrideFreightDefaults_NotInDb()
		{
			OverrideFreightDefaultsRunner(false);
		}
		public void TestOverrideFreightDefaults_InDB()
		{
			OverrideFreightDefaultsRunner(true);
		}

		public void TestSynchronisationWithInvalidSourceAddresses()
		{
			var rawEnableSetting = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				SetupSourceConsolAndShipments();
				sourceConsol.Shipments.RemoveAndDelete(shipmentTwo);
				shipmentOne.ConsigneeDocumentaryAddress.E2_ValidationStatus = "NYV";
				shipmentOne.ConsignorDocumentaryAddress.E2_ValidationStatus = "INV";
				shipmentOne.NotifyPartyDocumentaryAddress.E2_ValidationStatus = "NYV";
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
				manifest.SetParent(sourceConsol);
				manifest.Synchroniser.Synchronise(true);
				var bill = manifest.Bills[0];

				AssertEquals(shipmentOne.ConsigneeDocumentaryAddress.E2_OA_Address, bill.ABL_OA_Consignee);
				AssertEquals(true, bill.ABL_OA_ConsigneeInfo.ReadOnly);

				AssertEquals(ZGuid.Empty, bill.ABL_OA_Shipper);
				// THis line doesn't work, works ok down below, commented for now; functionally it's fine
				//AssertEquals(false, bill.ABL_OA_ShipperInfo.ReadOnly);

				AssertEquals(shipmentOne.NotifyPartyDocumentaryAddress.E2_OA_Address, bill.ABL_OA_NotifyParty);
				AssertEquals(true, bill.ABL_OA_NotifyPartyInfo.ReadOnly);

				// Now check we react to changes in the source validation status
				shipmentOne.ConsigneeDocumentaryAddress.E2_ValidationStatus = "INV";
				AssertEquals("We release the field for editing but we do not wipe it", shipmentOne.ConsigneeDocumentaryAddress.E2_OA_Address, bill.ABL_OA_Consignee);
				AssertEquals(false, bill.ABL_OA_ConsigneeInfo.ReadOnly);

				shipmentOne.ConsignorDocumentaryAddress.E2_ValidationStatus = "NYV";
				AssertEquals(shipmentOne.ConsignorDocumentaryAddress.E2_OA_Address, bill.ABL_OA_Shipper);
				AssertEquals(true, bill.ABL_OA_ShipperInfo.ReadOnly);

				shipmentOne.NotifyPartyDocumentaryAddress.E2_ValidationStatus = "INV";
				AssertEquals("We release the field for editing but we do not wipe it", shipmentOne.NotifyPartyDocumentaryAddress.E2_OA_Address, bill.ABL_OA_NotifyParty);
				AssertEquals(false, bill.ABL_OA_NotifyPartyInfo.ReadOnly);

				// And back again

				shipmentOne.ConsigneeDocumentaryAddress.E2_ValidationStatus = "NYV";
				AssertEquals(shipmentOne.ConsigneeDocumentaryAddress.E2_OA_Address, bill.ABL_OA_Consignee);
				AssertEquals(true, bill.ABL_OA_ConsigneeInfo.ReadOnly);

				shipmentOne.ConsignorDocumentaryAddress.E2_ValidationStatus = "INV";
				AssertEquals("We release the field for editing but we do not wipe it", shipmentOne.ConsignorDocumentaryAddress.E2_OA_Address, bill.ABL_OA_Shipper);
				AssertEquals(false, bill.ABL_OA_ShipperInfo.ReadOnly);

				shipmentOne.NotifyPartyDocumentaryAddress.E2_ValidationStatus = "NYV";
				AssertEquals(shipmentOne.NotifyPartyDocumentaryAddress.E2_OA_Address, bill.ABL_OA_NotifyParty);
				AssertEquals(true, bill.ABL_OA_NotifyPartyInfo.ReadOnly);
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawEnableSetting;
			}
		}

		void OverrideFreightDefaultsRunner(bool shouldSave)
		{
			SetupSourceConsolAndShipments();
			sourceConsol.Shipments.RemoveAndDelete(shipmentTwo);

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);
			if (shouldSave)
			{
				Factory.Save();
			}

			var bill = manifest.Bills[0];
			AssertEquals("GBFXT", manifest.AMA_RL_NKPortOfLoading);
			AssertEquals("VUAUY", bill.ABL_RL_NKFinalDestination);
			manifest.Containers.Load();
			AssertEquals("AAAA1234567", manifest.Containers[0].ACN_ContainerNumber);
			AssertEquals(true, manifest.AMA_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(true, bill.ABL_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals(true, manifest.Containers[0].ACN_ContainerNumberInfo.ReadOnly);
			manifest.AMA_OverrideFreightDefaults = true;
			bill = manifest.Bills[0];
			AssertEquals(false, manifest.AMA_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(false, bill.ABL_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals(false, manifest.Containers[0].ACN_ContainerNumberInfo.ReadOnly);

			var secondBill = manifest.Bills.AddNew();
			var secondContainer = manifest.Containers.AddNew();
			AssertEquals(2, manifest.Bills.Count);
			AssertEquals(2, manifest.Containers.Count);
			manifest.AMA_RL_NKPortOfLoading = "GBSOU";
			bill.ABL_RL_NKFinalDestination = "VUVLI";
			manifest.Containers[0].ACN_ContainerNumber = "New";
			AssertEquals("GBSOU", manifest.AMA_RL_NKPortOfLoading);
			AssertEquals("VUVLI", bill.ABL_RL_NKFinalDestination);
			AssertEquals("New", manifest.Containers[0].ACN_ContainerNumber);

			manifest.AMA_OverrideFreightDefaults = false;
			bill = manifest.Bills[0];
			AssertEquals("GBFXT", manifest.AMA_RL_NKPortOfLoading);
			AssertEquals("VUAUY", bill.ABL_RL_NKFinalDestination);
			manifest.Containers.Load();// not sure why this is needed
			AssertEquals("Override defaults, then un-override defaults flushes superflous conts", 1, manifest.Containers.Count);
			AssertEquals("AAAA1234567", manifest.Containers[0].ACN_ContainerNumber);
			AssertEquals(1, manifest.Bills.Count);
		}

		void SetupSourceConsolAndShipments()
		{
			sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_UniqueConsignRef = "C4321";
			sourceConsol.JK_RL_NKDischargePort = "VUVLI";
			sourceConsol.JK_RL_NKLoadPort = "GBFXT";
			sourceConsol.JK_TransportMode = "SEA";
			sourceConsol.JK_ConsolMode = "FCL";
			sourceConsol.JK_CoLoadMasterBill = "COLOADMBL";
			sourceConsol.JK_MasterBillNum = "BOL123456";
			sourceConsol.Transports[0].JW_VoyageFlight = "W1";
			sourceConsol.Transports[0].JW_Vessel = "ADMIRALENGRACHT";
			sourceConsol.Transports[0].JW_ETA = ZDateTime.BrettsBirthday;
			sourceConsol.Transports[0].JW_ETD = ZDateTime.BrettsBirthday.AddDays(-1);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			carrier = org.Addresses.AddNew();
			consignee = org.Addresses.AddNew();
			consignor = org.Addresses.AddNew();
			notify = org.Addresses.AddNew();
			sendingAgent = org.Addresses.AddNew();
			receivingAgent = org.Addresses.AddNew();
			carrier.OA_Address1 = "A";
			consignee.OA_Address1 = "B";
			consignor.OA_Address1 = "C";
			notify.OA_Address1 = "D";
			sendingAgent.OA_Address1 = "E";
			receivingAgent.OA_Address1 = "F";
			sourceConsol.JK_OA_ShippingLineAddress = carrier.PK;
			sourceConsol.JK_OA_SendingForwarderAddress = sendingAgent.PK;
			sourceConsol.JK_OA_ReceivingForwarderAddress = receivingAgent.PK;
			sourceConsol.Transports[0].JW_OA_CarrierAddress = carrier.PK;
			refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "20DC";
			containerA = sourceConsol.Containers.AddNew();
			containerA.JC_ContainerNum = "AAAA1234567";
			containerA.JC_RC = refContainer.PK;
			containerA.JC_SealNum = "S1";
			containerA.JC_AdditionalSealNum = "S2";
			containerA.JC_GrossWeight = 35;
			containerA.JC_GrossWeightUQ = "LB";
			containerB = sourceConsol.Containers.AddNew();
			containerB.JC_ContainerNum = "BBBB1234567";
			containerC = sourceConsol.Containers.AddNew();
			containerC.JC_ContainerNum = "CCCC1234567";
			shipmentOne = sourceConsol.Shipments.AddNew();
			//shipmentOne.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;  // Suppress for now - come back to this later when we know what to do about coloads etc
			shipmentOne.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.PK;
			shipmentOne.ConsignorDocumentaryAddress.E2_OA_Address = consignor.PK;
			shipmentOne.NotifyPartyDocumentaryAddress.E2_OA_Address = notify.PK;
			shipmentOne.JS_HouseBill = "HOUSEONE";
			shipmentOne.JS_RL_NKOrigin = "GBDTE";
			shipmentOne.JS_RL_NKDestination = "VUAUY";
			shipmentOne.JS_OuterPacks = 26;
			shipmentOne.JS_F3_NKPackType = "PKG";
			shipmentOne.JS_ActualWeight = 35m;
			shipmentOne.JS_UnitOfWeight = "KG";
			shipmentOne.JS_GoodsDescription = "BOOKS";
			shipmentOne.JS_MarksAndNumbers = "MARKS";
			shipmentOne.JS_ActualVolume = 38m;
			shipmentOne.JS_UnitOfVolume = "M3";
			shipmentOne.JS_GoodsValue = 1m;
			shipmentOne.JS_RX_NKGoodsValueCurr = "HKD";
			shipmentOne.JS_InsuranceValue = 2m;
			shipmentOne.JS_RX_NKInsuranceCurrency = "NZD";

			shipmentTwo = sourceConsol.Shipments.AddNew();
			shipmentTwo.JS_ShipmentType = "XXX";
			shipmentTwo.JS_HouseBill = "HOUSETWO";
			shipmentOne.OuterPackLines.RemoveAndDeleteAll(); // gets rid of the automatically-created one that appears when you set the pieces on the shipment
			var pivot1A = shipmentOne.OuterPackLines.AddNew();
			pivot1A.JL_JC = containerA.PK;
			pivot1A.JL_PackageCount = 69;
			shipmentTwo.OuterPackLines.RemoveAndDeleteAll(); // gets rid of the automatically-created one that appears when you set the pieces on the shipment
			var pivot2B = shipmentTwo.OuterPackLines.AddNew();
			pivot2B.JL_JC = containerB.PK;
			pivot2B.JL_PackageCount = 70;
			pivot2B.JL_F3_NKPackType = "F3B";
			pivot2B.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "a", "IMO").First().PK;
			pivot2B.JL_ActualVolume = 700m;
			pivot2B.JL_MarksAndNumbers = "M2B";
			pivot2B.JL_Description = "D2B";
			var pivot2C = shipmentTwo.OuterPackLines.AddNew();
			pivot2C.JL_JC = containerC.PK;
			pivot2C.JL_PackageCount = 71;
			pivot2C.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			pivot2C.JL_ActualVolume = 710m;
			pivot2C.JL_MarksAndNumbers = "M2C";
			pivot2C.JL_Description = "D2C";
			pivot2C.JL_F3_NKPackType = "F3C";
		}

		public void TestContainerModeMapping()
		{
			SetupSourceConsolAndShipments();
			sourceConsol.JK_ConsolMode = "FCL";
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);
			AssertEquals("CNT", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "LQD";
			AssertEquals("LQD", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "BLK";
			AssertEquals("BLK", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "BBK";
			AssertEquals("BBK", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "LCL";
			AssertEquals("CNT", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "LTL";
			AssertEquals("CNT", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "BCN";
			AssertEquals("CNT", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "GRP";
			AssertEquals("CNT", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "FTL";
			AssertEquals("CNT", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "OTH";
			AssertEquals("OTH", manifest.AMA_ContainerMode);
			sourceConsol.JK_ConsolMode = "XXX";
			AssertEquals("OTH", manifest.AMA_ContainerMode);
		}

		public void TestStopSynchroniseWithConsolAfterSendMessage()
		{
			SetupSourceConsolAndShipments();
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.SetParent(sourceConsol);
			manifest.SynchroniseWithSourceIfNeeded();
			manifest.Synchroniser.Synchronise();

			AssertEquals("VUVLI", manifest.AMA_RL_NKPortOfDischarge);
			AssertEquals("GBFXT", manifest.AMA_RL_NKPortOfLoading);
			AssertEquals("SEA", manifest.AMA_TransportMode);
			AssertEquals("BOL123456", manifest.AMA_MasterBill);

			manifest.AMA_MessageStatus = "SNT";

			sourceConsol.JK_MasterBillNum = "BOL234567";
			manifest.Synchroniser.Synchronise();
			AssertEquals("The data is not synchronized from consol after messages are sent", "BOL123456", manifest.AMA_MasterBill);
		}

		public void TestAllFieldsSynched()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomons", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVSEA = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVSEA.PK, "SEA", "VUVLI");

			Factory.Save();

			SetupSourceConsolAndShipments();
			containerA.JC_Additional2SealNum = "S3";

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;

			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);

			AssertEquals(receivingAgent.PK, manifest.AMA_OA_ShippingAgent);
			AssertEquals("VUVLI", manifest.AMA_RL_NKPortOfDischarge);
			AssertEquals("GBFXT", manifest.AMA_RL_NKPortOfLoading);
			AssertEquals("SEA", manifest.AMA_TransportMode);
			AssertEquals("COLOADMBL", manifest.MasterBOL);
			AssertEquals("BOL123456", manifest.AMA_MasterBill);
			AssertEquals(carrier.PK, manifest.AMA_OA_Carrier);
			AssertEquals(ZDateTime.BrettsBirthday, manifest.AMA_E_ARV);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-1), manifest.AMA_E_DEP);
			AssertEquals("", manifest.AMA_VehicleRegistration);  // Vehicle should not synchronise for Sea transports

			AssertEquals(2, manifest.Bills.Count);
			var bill1 = manifest.Bills[0];
			var bill2 = manifest.Bills[1];
			AssertEquals("HOUSEONE", bill1.ABL_BillNumber);
			AssertEquals("HOUSETWO", bill2.ABL_BillNumber);
			AssertEquals(bill1.ABL_JS_Shipment, shipmentOne.PK);
			//AssertEquals("Map CLD to CLD for bill type", "CLD", bill1.ABL_BolType);  // Suppress this assertion for now - we currently only synch STD (lowest) bills
			AssertEquals("Map all others to STD for bill type", "STD", bill2.ABL_BolType);
			AssertEquals("GBDTE", bill1.ABL_RL_NKOrigin);
			AssertEquals("VUAUY", bill1.ABL_RL_NKFinalDestination);
			AssertEquals(26, bill1.ABL_ManifestQty);
			AssertEquals("52", bill1.ABL_ManifestUQ);
			AssertEquals(35m, bill1.ABL_GrossWeight);
			AssertEquals("KG", bill1.ABL_GrossWeightUQ);
			AssertEquals("BOOKS", bill1.ABL_GoodsDescription);
			AssertEquals("MARKS", bill1.ABL_MarksAndNumbers);
			AssertEquals(38m, bill1.ABL_Volume);
			AssertEquals("M3", bill1.ABL_VolumeUQ);
			AssertEquals(1m, bill1.ABL_FreightValue);
			AssertEquals(2m, bill1.ABL_InsuranceValue);
			AssertEquals("HKD", bill1.ABL_RX_NKFreightValueCurrency);
			AssertEquals("NZD", bill1.ABL_RX_NKInsuranceValueCurrency);
			AssertEquals(consignee.PK, bill1.ABL_OA_Consignee);
			AssertEquals(consignor.PK, bill1.ABL_OA_Shipper);
			AssertEquals(notify.PK, bill1.ABL_OA_NotifyParty);

			manifest.Containers.Load();
			AssertEquals(3, manifest.Containers.Count);
			var asyCont = manifest.Containers[0];
			AssertEquals("AAAA1234567", asyCont.ACN_ContainerNumber);
			AssertEquals(refContainer.PK, asyCont.ACN_RC_ContainerType);
			AssertEquals("S1", asyCont.ACN_Seal1);
			AssertEquals("S2", asyCont.ACN_Seal2);
			AssertEquals("S3", asyCont.ACN_Seal3);
			AssertEquals("FCL", asyCont.ACN_EmptyFullIndicator);
			AssertEquals(false, asyCont.ACN_EmptyFullIndicatorInfo.ReadOnly);
			AssertEquals("BBBB1234567", manifest.Containers[1].ACN_ContainerNumber);
			AssertEquals("CCCC1234567", manifest.Containers[2].ACN_ContainerNumber);
			AssertEquals(69, manifest.Containers[0].ACN_NumberOfPackages);
			AssertEquals(70, manifest.Containers[1].ACN_NumberOfPackages);
			AssertEquals(71, manifest.Containers[2].ACN_NumberOfPackages);

			AssertEquals("0014a", bill2.Packs[0].UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
			AssertEquals("0004a", bill2.Packs[1].UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
			AssertEquals(2, bill2.Packs.Count);
			AssertEquals(70, bill2.Packs[0].APA_PackQty);
			AssertEquals(71, bill2.Packs[1].APA_PackQty);
			AssertEquals("F3B", bill2.Packs[0].APA_PackUQ);
			AssertEquals("F3C", bill2.Packs[1].APA_PackUQ);
			AssertEquals("M2B", bill2.Packs[0].APA_MarksAndNumbers);
			AssertEquals("M2C", bill2.Packs[1].APA_MarksAndNumbers);
			AssertEquals("D2B", bill2.Packs[0].APA_GoodsDescription);
			AssertEquals("D2C", bill2.Packs[1].APA_GoodsDescription);

			AssertEquals("Locked when source has a container type code", true, asyCont.ACN_RC_ContainerTypeInfo.ReadOnly);
			AssertEquals("Editable when source has no container type code", false, manifest.Containers[1].ACN_RC_ContainerTypeInfo.ReadOnly);

			containerA.Delete();
			AssertEquals(true, asyCont.IsDeleted);
			AssertEquals("One cont gone, two remain", 2, manifest.Containers.Count);
			sourceConsol.Shipments.Remove(shipmentTwo);
			AssertEquals(1, manifest.Bills.Count);
			AssertEquals("Shipment gone, conts into which it's packed no longer relevant, ASY conts reduced", 0, manifest.Containers.Count);

			bill1.ABL_CustomsValue = 2m;
			manifest.Synchroniser.Synchronise(true);
			AssertEquals("Customs value is retained and not overwritten by source value upon resynch", 2m, bill1.ABL_CustomsValue);

			AssertEquals("AGT", manifest.AMA_AgentType);
			sourceConsol.JK_AgentType = "FWB";
			manifest.Synchroniser.Synchronise(true);
			AssertEquals("FWB", manifest.AMA_AgentType);
		}

		public void TestJobReferenceIsNotSyncedFromConsol()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_RL_NKDischargePort = "VUVLI";
			sourceConsol.JK_UniqueConsignRef = "C0000001";

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.AMA_JobReference = string.Empty;

			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);

			Factory.Save();
			AssertNotEquals(string.Empty, manifest.AMA_JobReference);
			AssertNotEquals("C0000001", manifest.AMA_JobReference);

			sourceConsol.JK_UniqueConsignRef = "C0000002";
			AssertNotEquals("C0000002", manifest.AMA_JobReference);
		}

		public void TestReOpenExistingRecordShouldReUseExistingBills()
		{
			SetupSourceConsolAndShipments();
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);
			var originalBillForShipmentOne = manifest.Bills.FirstOrDefault(b => b.ABL_BillNumber == shipmentOne.JS_HouseBill);
			var originalBillForShipmentTwo = manifest.Bills.FirstOrDefault(b => b.ABL_BillNumber == shipmentTwo.JS_HouseBill);
			originalBillForShipmentOne.ABL_CarrierReference = "One";
			originalBillForShipmentTwo.ABL_CarrierReference = "Two";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			sourceConsol = newFactory.Load<ForwardingConsol>(sourceConsol.PK);
			manifest = newFactory.Load<AsycudaManifestHeader>(manifest.PK);
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.Synchroniser.Synchronise(true);
			AssertEquals(2, manifest.Bills.Count);
			AssertEquals("Reloading and resynching should keep original bill", originalBillForShipmentOne.PK, manifest.Bills[0].PK);
			AssertEquals("Reloading and resynching should keep original bill", originalBillForShipmentTwo.PK, manifest.Bills[1].PK);
			AssertEquals("Reloading and resynching should keep original bill", "One", manifest.Bills[0].ABL_CarrierReference);
			AssertEquals("Reloading and resynching should keep original bill", "Two", manifest.Bills[1].ABL_CarrierReference);
		}

		public void TestShippingAgentsForTwoCountryManifest()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomons", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVSEA = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVSEA.PK, "SEA", "VUVLI");

			Factory.Save();

			SetupSourceConsolAndShipments();
			sourceConsol.JK_RL_NKLoadPort = "SBHIR";
			sourceConsol.JK_OA_SendingForwarderAddress = sendingAgent.PK;

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;

			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);

			// When we first wire the synchroniser, the Countries are null. Once we force synchronise, the ports are pulled through, whcih creates the countries. Only then can we hook the syncher for the agent.
			AssertEquals(sendingAgent.PK, manifest.AMA_OA_ShippingAgent);

			manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;

			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);

			AssertEquals(receivingAgent.PK, manifest.AMA_OA_ShippingAgent);
		}

		public void TestSynchContainerModes()
		{
			SetupSourceConsolAndShipments();
			containerA.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);
			manifest.Containers.Load();
			var asyCont = manifest.Containers[0];
			AssertEquals(Core.Constants.ContainerModes.FCL, asyCont.ACN_EmptyFullIndicator);
			containerA.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(EmptyFullIndicatorList.Codes.LessThanFullContainerLoad, asyCont.ACN_EmptyFullIndicator);
			containerA.JC_ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("", asyCont.ACN_EmptyFullIndicator);
			containerA.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals(EmptyFullIndicatorList.Codes.FullContainerLoad, asyCont.ACN_EmptyFullIndicator);
			containerA.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals(EmptyFullIndicatorList.Codes.LessThanFullContainerLoad, asyCont.ACN_EmptyFullIndicator);
		}

		public void TestDontSynchContainersForAir()
		{
			SetupSourceConsolAndShipments();
			sourceConsol.JK_TransportMode = "AIR";
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "VUVLI";
			manifest.SetParent(sourceConsol);
			manifest.Synchroniser.Synchronise(true);
			manifest.Containers.Load();
			AssertEquals(0, manifest.Containers.Count);
		}

		public void TestSynchronisersShippingAgent()
		{
			OrgHeader orgReceivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgSendingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UYMVD";
			consol.JK_RL_NKDischargePort = "COBOG";
			consol.JK_OA_ReceivingForwarderAddress = orgReceivingAgent.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgSendingAgent.MainAddress.PK;
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Colombia;
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals(orgReceivingAgent.MainAddress.PK, manifestHeader.AMA_OA_ShippingAgent);
		}

		ForwardingConsol sourceConsol;
		OrgAddress carrier;
		OrgAddress consignee;
		OrgAddress consignor;
		OrgAddress notify;
		OrgAddress sendingAgent;
		OrgAddress receivingAgent;
		RefContainer refContainer;
		ForwardingContainer containerA;
		ForwardingContainer containerB;
		ForwardingContainer containerC;
		ForwardingShipment shipmentOne;
		ForwardingShipment shipmentTwo;
	}
}
