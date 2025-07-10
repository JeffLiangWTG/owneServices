using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using AsycudaContainer = Enterprise.Customs.ASYCUDA.Business.AsycudaContainer;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestCreateOrGetCountryForMultipleProvider()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_ActualVolume = 100m;

			Factory.Save();

			bool saveButtonEnabled = false;
			consol.HasChangesChanged += (object sender, HasChangesChangedEventArgs e) => { saveButtonEnabled = consol.HasChanges; };

			var wrapper = new ManifestHeadersWrapper(consol);
			AssertEquals("Precondition - Consol does not have changes yet", false, consol.HasChanges);
			AssertEquals("Wrapper has no existing headers", 0, wrapper.Headers.Count);

			wrapper.WR_CountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			wrapper.CreateCountry(Core.Constants.CountryCodes.UnitedKingdom, "GVM");

			var createdHeader = wrapper.Headers.Cast<AsycudaManifestHeader>().FirstOrDefault(h => h.AMA_RN_NKCountry == Core.Constants.CountryCodes.UnitedKingdom);
			Assert("Should have created a GVMS AsycudaManifestHeader", createdHeader is Integration.Customs.GB.GBGVMS.IAsycudaManifestHeader);
			Assert("Consol Has Changes", consol.HasChanges);
			Assert("HasChangesChanged on Consol was called in a way that will enable the Save button on the Form.", saveButtonEnabled);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.GB.GBGVMS.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestDefaultGetTypes()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
				AssertEquals("Default Container Type", typeof(AsycudaContainer), header.GetContainerType());
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		public void TestManifestNature()
		{
			var uymanifestTypes = new GVMSManifestType().All;
			var man = uymanifestTypes.FirstOrDefault(x => x.Code == GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms);
			var manNatures = man.ManifestNatures;
			var natureList = new GVMSManifestNature();
			AssertContainsExactElementsInAnyOrder(man.ManifestNatures, new GVMSManifestNature());
		}

		public void TestDefaultTransportMode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Header.AMA_TransportMode", Core.Constants.TransportModes.Road, header.AMA_TransportMode);
		}

		public void TestItemReferences()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var customsRef1 = header.GvmsCustomsReferenceCollection.AddNew();
			var customsRef2 = header.GvmsCustomsReferenceCollection.AddNew();
			var customsTransitReference = header.GvmsTransitReferenceCollection.AddNew();
			var customsEidrReference = header.GvmsEidrAndOralReferenceCollection.AddNew();
			var customsEidrReference2 = header.GvmsEidrAndOralReferenceCollection.AddNew();

			customsRef1.CSI_Code = GVMSCustomsReference.Codes.AtaCarnet;
			customsRef1.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsRef1.CSI_RN_NKCountryCode = "GB";
			customsRef1.CSI_ReferenceNumber = "ABC123";
			customsRef1.CSI_ReferenceNumber2 = "PalletRef1";
			customsRef1.CSI_Status = "Yes";

			customsRef2.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;
			customsRef2.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsRef2.CSI_RN_NKCountryCode = "GB";
			customsRef2.CSI_ReferenceNumber = "ABC234";
			customsRef2.CSI_ReferenceNumber2 = "PalletRef2";
			customsRef2.CSI_Status = "Yes";

			customsTransitReference.CSI_Code = GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber;
			customsTransitReference.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsTransitReference.CSI_RN_NKCountryCode = "GB";
			customsTransitReference.CSI_ReferenceNumber = "ABC345";
			customsTransitReference.CSI_ReferenceNumber2 = "PalletRef3";
			customsTransitReference.CSI_Status = "Yes";

			customsEidrReference.CSI_Code = GVMSCustomsReference.Codes.EntryInDeclarantsRecord;
			customsEidrReference.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsEidrReference.CSI_RN_NKCountryCode = "GB";
			customsEidrReference.CSI_ReferenceNumber = "ABC456";
			customsEidrReference.CSI_ReferenceNumber2 = "PalletRef4";
			customsEidrReference.CSI_Status = "Yes";

			customsEidrReference2.CSI_Code = GVMSCustomsReference.Codes.OralDeclaration;
			customsEidrReference2.CSI_DateOfIssue = new ZDateTime(2020, 12, 01);
			customsEidrReference2.CSI_RN_NKCountryCode = "GB";
			customsEidrReference2.CSI_ReferenceNumber = "ABC456";
			customsEidrReference2.CSI_ReferenceNumber2 = "PalletRef5";
			customsEidrReference2.CSI_Status = "Yes";

			AssertEquals(2, header.CustomsReferences.ToList().Count);
			AssertEquals(1, header.CustomsTransitReferences.ToList().Count);
			AssertEquals(2, header.CustomsEidrAndOralReferences.ToList().Count);

			CombineAssertions("Customs References", () =>
			{
				AssertEquals("GVM", header.CustomsReferences.ElementAt(0).CSI_Type);
				AssertEquals(GVMSCustomsReference.Codes.AtaCarnet, header.CustomsReferences.ElementAt(0).CSI_Code);
				AssertEquals("ABC123", header.CustomsReferences.ElementAt(0).CSI_ReferenceNumber);
				AssertEquals("ABC234", header.CustomsReferences.ElementAt(1).CSI_ReferenceNumber);
			});

			CombineAssertions("Customs Transit References", () =>
			{
				AssertEquals("GVM", header.CustomsTransitReferences.ElementAt(0).CSI_Type);
				AssertEquals(GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber, header.CustomsTransitReferences.ElementAt(0).CSI_Code);
				AssertEquals("ABC345", header.CustomsTransitReferences.ElementAt(0).CSI_ReferenceNumber);
				AssertEquals("PalletRef3", header.CustomsTransitReferences.ElementAt(0).CSI_ReferenceNumber2);
			});

			CombineAssertions("Customs EIDR and Oral References", () =>
			{
				AssertEquals("GVM", header.CustomsEidrAndOralReferences.ElementAt(1).CSI_Type);
				AssertEquals(GVMSCustomsReference.Codes.OralDeclaration, header.CustomsEidrAndOralReferences.ElementAt(1).CSI_Code);
				AssertEquals("ABC456", header.CustomsEidrAndOralReferences.ElementAt(1).CSI_ReferenceNumber);
				AssertEquals("Yes", header.CustomsEidrAndOralReferences.ElementAt(1).CSI_Status);
			});
		}
		public void TestRegistrationNumberWithoutSpaces()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "A B C D";

			AssertEquals("ABCD", header.RegistrationNumberWithoutSpaces);
		}

		public void TestHumanReadableNamePrefix()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(Constants.MessageSubTypePreFixes.GVMS, header.HumanReadableNamePrefix);
		}

		public void TestCalculatedRouteID()
		{
			GVMSTestHelper.SetupPortsForRouteCalculation(Factory);
			GVMSTestHelper.SetupRoutesForRouteCalculation(Factory);
			GVMSTestHelper.SetUpCarriersForRouteCalculation(Factory);

			var gvmsManifest = Factory.New<AsycudaManifestHeader>();
			gvmsManifest.AMA_RL_NKPortOfLoading = "BEZEE";
			AssertEquals("Value will be blank when only load port is populated", "", gvmsManifest.CalculatedRouteID);

			gvmsManifest.AMA_RL_NKPortOfDischarge = "GBHUL";
			AssertEquals("Value will be blank when only load port and discharge port is populated", "", gvmsManifest.CalculatedRouteID);

			gvmsManifest.AMA_CarrierCode = "ABC";
			AssertEquals("Calculated Route should be R1", "R1", gvmsManifest.CalculatedRouteID);
			AssertEquals("When RouteID is blank it should be populated when calculating the route", "R1", gvmsManifest.RouteId);

			gvmsManifest.AMA_RL_NKPortOfDischarge = "GBSTN";
			AssertEquals("Calculated Route should now be blank as by calling CalculatedRouteID the route is recalculated as it is invalidated " +
				"when either AMA_RL_NKPortOfLoading, AMA_RL_NKPortOfDischarge or AMA_CarrierCode are changed.", "", gvmsManifest.CalculatedRouteID);
		}

		public void TestRouteId()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.RouteId = "Route 123";
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("Route 123", header.RouteId);
		}

		public void TestIsUnaccompanied()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.IsUnaccompanied = true;
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			Assert(header.IsUnaccompanied);
		}

		public void TestEmptyVehicle()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(ZString.Empty, header.EmptyVehicle);
			AssertNoNotifications(header.EmptyVehicleInfo);

			header.EmptyVehicle = "XXX";
			AssertEquals("XXX", header.EmptyVehicle);
			AssertHasNotifications(header.EmptyVehicleInfo);
			Factory.Save();
			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("XXX", headerReloaded.EmptyVehicle);

			foreach (var value in new ZString[] { GVMSEmptyVehicle.Codes.VehicleIsNotEmpty, GVMSEmptyVehicle.Codes.EmptyVehicleIsBeingMovedUnderAContractOfCarriage, GVMSEmptyVehicle.Codes.EmptyVehicleIsNotBeingMovedViaAContractOfCarriage })
			{
				header.EmptyVehicle = value;
				AssertEquals(value, header.EmptyVehicle);
				AssertNoNotifications(header.EmptyVehicleInfo);
				Factory.Save();

				headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
				AssertEquals(value, headerReloaded.EmptyVehicle);
				AssertNoNotifications(headerReloaded.EmptyVehicleInfo);
			}
		}

		public void TestInspectionLocations()
		{
			var gvmsHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			var location1 = gvmsHeader.InspectionLocations.AddNew();
			location1.CY_Code = "1";
			location1.CY_Data = "L0029A";
			var location2 = gvmsHeader.InspectionLocations.AddNew();
			location2.CY_Code = "2";
			location2.CY_Data = "L0030A";
			Factory.Save();

			gvmsHeader = new BusinessObjectFactory().Load<AsycudaManifestHeader>(gvmsHeader.PK);
			AssertEquals(2, gvmsHeader.InspectionLocations.Count);
			AssertEquals("1", gvmsHeader.InspectionLocations[0].CY_Code);
			AssertEquals("L0029A", gvmsHeader.InspectionLocations[0].CY_Data);
			AssertEquals("2", gvmsHeader.InspectionLocations[1].CY_Code);
			AssertEquals("L0030A", gvmsHeader.InspectionLocations[1].CY_Data);

			var icsHeader = (AsycudaManifestHeader)GetNewBusinessObject();
			icsHeader.AMA_ManifestType = EUManifestTypes.Codes.ICS;
			AssertEquals("InspectionLocations is not applicable for non-GVMS manifest", null, icsHeader.InspectionLocations);
		}

		public void TestInspectionRequired()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.InspectionRequired = true;
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			Assert(header.InspectionRequired);
		}

		public void TestCusCodeDataTypesCorrectlyMapsCodeGVI()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals("GVI Lookup is expected to be a GvmsInspectionAtLocationCusCodeData", typeof(GvmsInspectionAtLocationCusCodeData), header.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.GVI]);
		}

		public void TestEnsureInspectionLocationIsGvmsInspectionAtLocationCusCodeData()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			Assert("InspectionLocations is expected to be a GvmsInspectionAtLocationCusCodeDataCollection", header.InspectionLocations is GvmsInspectionAtLocationCusCodeDataCollection);
			var location = header.InspectionLocations.AddNew();
			Assert("Location is expected to be a GvmsInspectionAtLocationCusCodeData", location is GvmsInspectionAtLocationCusCodeData);
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}

