using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ImportManifestFromSailingCreatorTest : TestCaseWithFactory
	{
		public void TestTypeOfPackTypeEqual()
		{
			var bill = Factory.New<BillOfLading>();
			CommonContainer container = bill.RealContainers.AddNew();
			var packLine = Factory.New<PackLine>();
			container.PackLines.Add(packLine);

			var manifest = Factory.New<CusSeaManTranHead>();
			var oceanBill = manifest.OceanBills.AddNew();
			var billDetail = oceanBill.Details.AddNew();

			AssertEquals("Not good", SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(packLine.JL_F3_NKPackType), billDetail.BD_PackType);
		}

		public void TestCreateImportManifests()
		{
			CreateAndAssertVoyageAndBillsOfLading();

			var holder = manager.CreateImportManifests();
			var collection = new ImportManifestItemCollection(holder.Manifests[0].ImportManifest, voyage.Sailings[0], true);

			AssertEquals(1, holder.Manifests.Count);

			collection.SelectAll(true);

			holder.Factory.Save();

			var manifests = new CusSeaManTranHeadCollection(Factory);
			manifests.Load();

			AssertEquals(1, manifests.Count);
			AssertEquals(true, manifests[0].IsInDatabase);

			var manifest1 = FetchManifestForVesselAndVoyage(manifests, "ADMIRALENGRACHT", "001");
			AssertContainsExactElementsInAnyOrder("OBL",
				new string[] { "BILL1", "SOME BILL", "BILL3", "RORO-IMPORT", "FCL-TRANSHIP", "RORO-TRANSHIP" },
				Array.ConvertAll(manifest1.OceanBills.ToArray<CusSeaManOBLHeader>(), (d) => d.BO_OceanBill.ToString()));

			var header1 = FetchOBLFromManifest(manifest1.OceanBills, "BILL1");
			var header2 = FetchOBLFromManifest(manifest1.OceanBills, "SOME BILL");
			var header3 = FetchOBLFromManifest(manifest1.OceanBills, "BILL3");
			var header4 = FetchOBLFromManifest(manifest1.OceanBills, "RORO-IMPORT");

			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("header1",
					new string[] { "C11", "C31" },
					Array.ConvertAll(header1.Details.ToArray<CusSeaManOBLDetail>(), (d) => d.BD_ContainerNumber.ToString()));

				AssertContainsExactElementsInAnyOrder("header2",
					new string[] { "C31", "C31" },
					Array.ConvertAll(header2.Details.ToArray<CusSeaManOBLDetail>(), (d) => d.BD_ContainerNumber.ToString()));

				AssertContainsExactElementsInAnyOrder("header3",
					new string[] { "C13" },
					Array.ConvertAll(header3.Details.ToArray<CusSeaManOBLDetail>(), (d) => d.BD_ContainerNumber.ToString()));

				AssertContainsExactElementsInAnyOrder("header4",
					new string[] { "BREAK BULK" },
					Array.ConvertAll(header4.Details.ToArray<CusSeaManOBLDetail>(), (d) => d.BD_ContainerNumber.ToString()));
			});

			var expectedDescription = ZString.Replicate('1', CusSeaManOBLDetailSchema.BD_GoodsDescription.MaxLength - 1) + "2";

			AssertEquals("NZAKA", manifest1.BT_RL_NKPortOfLastForeignPort);
			AssertEquals(new ZDateTime(2006, 11, 3), manifest1.BT_PortOfLastForeignPortATD);
			AssertEquals("001", manifest1.BT_VoyageNum);
			AssertEquals("ADMIRALENGRACHT", manifest1.BT_VesselName);
			AssertEquals("Origin Should be default from BillOfLading", "SGSIN", header2.BO_RL_NKOriginPort);
			AssertEquals("Destination Should be default from BillOfLading", "AUBNE", header2.BO_RL_NKDestinationPort);
			AssertEquals("Load Should be default from BillOfLading", "NZAKL", header2.BO_RL_NKLoadPort);
			AssertEquals("Discharge Should be default from BillOfLading", "AUSYD", header2.BO_RL_NKDischargePort);
			AssertEquals(CMRMethodsOfPayment.Codes.Collect, header1.BO_PaymentMethod);

			AssertEquals("ConsigneeAddress1", header1.BO_ConsigneeAddress1);
			AssertEquals("ConsigneeAddress2", header1.BO_ConsigneeAddress2);
			AssertEquals("ConsigneeCity", header1.BO_ConsigneeCity);
			AssertEquals("Post Code", header1.BO_ConsigneePostCode);
			AssertEquals("Some Company", header1.BO_ConsigneeName);
			AssertEquals("AU", header1.BO_RN_NKConsigneeCountryCode);

			AssertEquals("ConsignorAddress1", header1.BO_ConsignorAddress1);
			AssertEquals("ConsignorAddress2", header1.BO_ConsignorAddress2);
			AssertEquals("ConsignorCity", header1.BO_ConsignorCity);
			AssertEquals("Post Code", header1.BO_ConsignorPostCode);
			AssertEquals("Some Company", header1.BO_ConsignorName);
			AssertEquals("AU", header1.BO_RN_NKConsignorCountryCode);

			var header1Detail1 = header1.Details.Cast<CusSeaManOBLDetail>().FirstOrDefault(d => d.BD_NoOfPacks == 55);
			var header1Detail2 = header1.Details.Cast<CusSeaManOBLDetail>().FirstOrDefault(d => d.PK != header1Detail1.PK);
			AssertNotNull(header1Detail1);
			AssertNotNull(header1Detail2);
			AssertEquals("detailed description", expectedDescription, header1Detail1.BD_GoodsDescription);
			AssertEquals(CMRQuantityUnits.Codes.Package, header1Detail1.BD_PackType);
			AssertEquals(23, (int)header1Detail1.BD_CargoVolume);
			AssertEquals(CMRQuantityUnits.Codes.CubicMetre, header1Detail1.BD_CargoVolumeUM);
			AssertEquals(CMRImportCargoTypes.Codes.FullContainerLoad, header1Detail1.BD_LineCargoType);
			AssertEquals(CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, header1Detail2.BD_LineCargoType);

			var header2Detail1 = header2.Details.Cast<CusSeaManOBLDetail>().FirstOrDefault(d => d.BD_LineCargoType == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills);
			var header2Detail2 = header2.Details.Cast<CusSeaManOBLDetail>().FirstOrDefault(d => d.BD_LineCargoType == CMRImportCargoTypes.Codes.LessThanContainerLoad);
			AssertNotNull(header2Detail1);
			AssertNotNull(header2Detail2);
			Assert(header2Detail1 != header2Detail2);

			AssertEquals(CMRImportCargoTypes.Codes.BreakBulk, header4.Details[0].BD_LineCargoType);
			AssertEquals(5, header4.Details[0].BD_NoOfPacks);
			AssertEquals("YC", header4.Details[0].BD_PackType);
			AssertEquals(35m, header4.Details[0].BD_CargoVolume);
			AssertEquals("CU", header4.Details[0].BD_CargoVolumeUM);
			AssertEquals(1234m, header4.Details[0].BD_GrossWeight);
			AssertEquals("KG", header4.Details[0].BD_GrossWeightUM);
			AssertEquals("imported cars", header4.Details[0].BD_GoodsDescription);
			AssertEquals(true, header4.Details[0].BD_HazardousIndicator);
			AssertEquals("MARKS6", header4.Details[0].BD_MarksAndNumbers);

			AssertContainsExactElementsInAnyOrder("manifest1.Arrivals",
				new string[] { "AUSYD", "AUCNS", "AUBNE" },
				Array.ConvertAll(manifest1.Arrivals.ToArray<CusSeaManArrivalPort>(), (a) => a.BA_RL_NKArrivalPort.ToString()));

			var arrival1AUBNE = FetchArrivalPort(manifest1, "AUBNE");
			var arrival1AUSYD = FetchArrivalPort(manifest1, "AUSYD");
			var arrival1AUCNS = FetchArrivalPort(manifest1, "AUCNS");

			AssertEquals("AUBNE", arrival1AUBNE.BA_RL_NKArrivalPort);
			AssertEquals(new ZDateTime(2006, 11, 15), arrival1AUBNE.BA_ArrivalPortETA);
			AssertEquals("B1", arrival1AUBNE.BA_BerthCode);
			AssertEquals(cToAddress.PK, arrival1AUBNE.BA_OA_CTOAddress);
			AssertEquals(new ZDateTime(2006, 11, 16), arrival1AUBNE.BA_ArrivalPortATA);

			AssertContainsExactElementsInAnyOrder("arrival1AUBNE.CargoLines",
				new string[] { "C21" },
				Array.ConvertAll(arrival1AUSYD.CargoLines.ToArray<CusSeaManOBLHeaderCargoLine>(), (l) => l.CargoIdentifier.ToString()));

			var line_C21 = FetchCargoLine(arrival1AUSYD, "C21");
			AssertEquals(0, line_C21.NumberOfPackages);
			AssertEquals("NZAKL", line_C21.BO_RL_NKLoadPort);
			AssertEquals("AUSYD", line_C21.BO_RL_NKDestinationPort);
			AssertEquals(ZString.Empty, line_C21.PackageType);
			AssertEquals("C21", line_C21.CargoIdentifier);
			AssertEquals(CMRImportCargoCodes.Codes.Empty, line_C21.BO_HeaderCargoType);
			AssertEquals(CMRCargoTypes.Codes.FullContainerLoad, line_C21.CargoType);

			AssertContainsExactElementsInAnyOrder("arrival1AUBNE.CargoLines",
				new string[] { "BBK-COASTAL", "CFCLCAB" },
				Array.ConvertAll(arrival1AUBNE.CargoLines.ToArray<CusSeaManOBLHeaderCargoLine>(), (l) => l.CargoIdentifier.ToString()));

			var line_BBK_COSTAL = FetchCargoLine(arrival1AUBNE, "BBK-COASTAL");
			AssertEquals(25, line_BBK_COSTAL.NumberOfPackages);
			AssertEquals("AUSYD", line_BBK_COSTAL.BO_RL_NKLoadPort);
			AssertEquals("AUBNE", line_BBK_COSTAL.BO_RL_NKDestinationPort);
			AssertEquals(CMRCargoTypes.Codes.BreakBulk, line_BBK_COSTAL.CargoType);
			AssertEquals("BBK-COASTAL", line_BBK_COSTAL.CargoIdentifier);
			AssertEquals(CMRPackageTypes.Codes.PalletLift, line_BBK_COSTAL.PackageType);
			AssertEquals(CMRImportCargoCodes.Codes.Cabotage, line_BBK_COSTAL.BO_HeaderCargoType);

			var line_CFCLCAB = FetchCargoLine(arrival1AUBNE, "CFCLCAB");
			AssertEquals(100, line_CFCLCAB.NumberOfPackages);
			AssertEquals("AUSYD", line_CFCLCAB.BO_RL_NKLoadPort);
			AssertEquals("AUBNE", line_CFCLCAB.BO_RL_NKDestinationPort);
			AssertEquals(CMRCargoTypes.Codes.FullContainerLoad, line_CFCLCAB.CargoType);
			AssertEquals("CFCLCAB", line_CFCLCAB.CargoIdentifier);
			AssertEquals(CMRPackageTypes.Codes.Package, line_CFCLCAB.PackageType);
			AssertEquals(CMRImportCargoCodes.Codes.Cabotage, line_CFCLCAB.BO_HeaderCargoType);
		}

		public void TestCreateLineForEmptyContainer() => CombineAssertions(() =>
		{
			var vessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory)[0];

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_E_DEP = new ZDateTime(2024, 1, 1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = new ZDateTime(2024, 1, 9);

			var bill = Factory.New<BillOfLading>();
			bill.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			bill.JS_HouseBill = "bill1";
			bill.JS_GoodsDescription = "foo";
			bill.JS_RL_NKOrigin = "NZAKL";
			bill.JS_RL_NKDestination = "AUSYD";

			var container1ForBill = bill.RealContainers.AddNew();
			container1ForBill.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container1ForBill.JC_ContainerCount = 3;
			container1ForBill.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1ForBill.JC_ContainerNum = "C11";
			container1ForBill.JC_IsEmptyContainer = true;
			container1ForBill.JC_IsShipperOwned = true;

			var container2ForBill = bill.RealContainers.AddNew();
			container2ForBill.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container2ForBill.JC_ContainerCount = 3;
			container2ForBill.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2ForBill.JC_ContainerNum = "C12";
			container2ForBill.JC_IsEmptyContainer = true;
			container2ForBill.JC_IsShipperOwned = false;
			Factory.Save();

			var holder = manager.CreateImportManifests();
			AssertEquals("Manifest count", 1, holder.Manifests.Count);

			var manifest = holder.Manifests[0].ImportManifest;
			AssertEquals("Cargo list count", 1, manifest.Arrivals[0].CargoLines.Count);
			AssertEquals("Container number in cargo list", "C12", manifest.Arrivals[0].CargoLines[0].CargoIdentifier);
			AssertEquals("Container details count", 1, manifest.OceanBills[0].Details.Count);
			AssertEquals("Container number in container details", "C11", manifest.OceanBills[0].Details[0].BD_ContainerNumber);
		});

		public void TestCreateImportManifestsForTranshipment()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HAMISHANDANDYSH";

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_E_DEP = new ZDateTime(2008, 12, 2);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NLAMS", "AUSYD");
			bill1.JS_HouseBill = "BILL1";
			bill1.JS_RL_NKOrigin = "NLAMS";
			bill1.JS_RL_NKDestination = "NZAKL";//transhipment

			bill1.JS_GoodsDescription = "bill1GoodDescription";
			bill1.JS_MarksAndNumbersShort = "Marks and Numbers";
			bill1.JS_OuterPacks = 22;
			bill1.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			bill1.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;

			bill1.ConsigneeDocumentaryAddress.E2_Address1 = "ConsigneeAddress1";
			bill1.ConsigneeDocumentaryAddress.E2_Address2 = "ConsigneeAddress2";
			bill1.ConsigneeDocumentaryAddress.E2_City = "ConsigneeCity";
			bill1.ConsigneeDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsigneeDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			bill1.ConsignorDocumentaryAddress.E2_Address1 = "ConsignorAddress1";
			bill1.ConsignorDocumentaryAddress.E2_Address2 = "ConsignorAddress2";
			bill1.ConsignorDocumentaryAddress.E2_City = "ConsignorCity";
			bill1.ConsignorDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsignorDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			//Bill 1, Container 1 - Hazardous Packline
			var bill1Container1 = bill1.RealContainers.AddNew();
			bill1Container1.JC_ContainerNum = "BILL1CONTAINER1";
			bill1Container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			bill1Container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();

			var holder = manager.CreateImportManifests();
			var collection = new ImportManifestItemCollection(holder.Manifests[0].ImportManifest, voyage.Sailings[0], true);

			AssertEquals(1, holder.Manifests.Count);

			var manifest = holder.Manifests[0].ImportManifest;
			AssertEquals("transhipment oceanbill should be created as an ocean bill", 1, manifest.OceanBills.Count);
		}

		public void TestCreateImportManifestsForTranshipmentNonFCL()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HAMISHANDANDYSH";

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_E_DEP = new ZDateTime(2008, 12, 2);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NLAMS", "AUSYD");
			bill1.JS_HouseBill = "BILL1";
			bill1.JS_RL_NKOrigin = "NLAMS";
			bill1.JS_RL_NKDestination = "NZAKL";//transhipment

			bill1.JS_GoodsDescription = "bill1GoodDescription";
			bill1.JS_MarksAndNumbersShort = "Marks and Numbers";
			bill1.JS_OuterPacks = 22;
			bill1.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			bill1.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;

			bill1.ConsigneeDocumentaryAddress.E2_Address1 = "ConsigneeAddress1";
			bill1.ConsigneeDocumentaryAddress.E2_Address2 = "ConsigneeAddress2";
			bill1.ConsigneeDocumentaryAddress.E2_City = "ConsigneeCity";
			bill1.ConsigneeDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsigneeDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			bill1.ConsignorDocumentaryAddress.E2_Address1 = "ConsignorAddress1";
			bill1.ConsignorDocumentaryAddress.E2_Address2 = "ConsignorAddress2";
			bill1.ConsignorDocumentaryAddress.E2_City = "ConsignorCity";
			bill1.ConsignorDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsignorDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			//Bill 1, Container 1 - Hazardous Packline
			var bill1Container1 = bill1.RealContainers.AddNew();
			bill1Container1.JC_ContainerNum = "BILL1CONTAINER1";
			bill1Container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			bill1Container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();

			var holder = manager.CreateImportManifests();
			var collection = new ImportManifestItemCollection(holder.Manifests[0].ImportManifest, voyage.Sailings[0], true);

			AssertEquals(1, holder.Manifests.Count);

			var manifest = holder.Manifests[0].ImportManifest;
			AssertEquals("transhipment oceanbill should be created as an ocean bill", 1, manifest.OceanBills.Count);
		}

		public void TestHazardousIndicator()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HAMISHANDANDYSH";

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_E_DEP = new ZDateTime(2008, 12, 2);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NLAMS", "AUSYD");
			bill1.JS_HouseBill = "BILL1";
			bill1.JS_RL_NKOrigin = "NLAMS";
			bill1.JS_RL_NKDestination = "AUSYD";
			bill1.JS_GoodsDescription = "bill1GoodDescription";
			bill1.JS_MarksAndNumbersShort = "Marks and Numbers";
			bill1.JS_OuterPacks = 22;
			bill1.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			bill1.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;

			bill1.ConsigneeDocumentaryAddress.E2_Address1 = "ConsigneeAddress1";
			bill1.ConsigneeDocumentaryAddress.E2_Address2 = "ConsigneeAddress2";
			bill1.ConsigneeDocumentaryAddress.E2_City = "ConsigneeCity";
			bill1.ConsigneeDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsigneeDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			bill1.ConsignorDocumentaryAddress.E2_Address1 = "ConsignorAddress1";
			bill1.ConsignorDocumentaryAddress.E2_Address2 = "ConsignorAddress2";
			bill1.ConsignorDocumentaryAddress.E2_City = "ConsignorCity";
			bill1.ConsignorDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsignorDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			//Bill 1, Container 1 - Hazardous Packline
			var bill1Container1 = bill1.RealContainers.AddNew();
			bill1Container1.JC_ContainerNum = "BILL1CONTAINER1";
			bill1Container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			bill1Container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			PackLine bill1Container1PackLine1 = bill1.OuterPackLines.AddNew();
			bill1Container1PackLine1.JL_PackageCount = 55;
			bill1Container1PackLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			bill1Container1PackLine1.JL_ActualVolume = 23;
			bill1Container1PackLine1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			bill1Container1PackLine1.JL_RH_NKCommodityCode = "HAZ";
			//bill1Container1PackLine1.JL_JC = bill1Container1.PK;
			bill1Container1PackLine1.Containers.Add(bill1Container1);

			//Bill 1, Container 2 - No Hazardous Contents
			var bill1Container2 = bill1.RealContainers.AddNew();
			bill1Container2.JC_ContainerNum = "BILL1CONTAINER2";
			bill1Container2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			bill1Container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			//Bill 1, Container 3 - Hazardous Container
			var bill1Container3 = bill1.RealContainers.AddNew();
			bill1Container3.JC_ContainerNum = "BILL1CONTAINER3";
			bill1Container3.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			bill1Container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			bill1Container3.JC_RH_NKContainerCommodityCode = "HAZ";

			Factory.Save();

			var holder = manager.CreateImportManifests();
			var collection = new ImportManifestItemCollection(holder.Manifests[0].ImportManifest, voyage.Sailings[0], true);

			collection.SelectAll(true);

			holder.Factory.Save();

			var manifests = new CusSeaManTranHeadCollection(Factory);
			manifests.Load();

			var manifest1 = FetchManifestForVesselAndVoyage(manifests, "HAMISHANDANDYSH", "001");

			AssertEquals("Bill1, Container1", true, GetContainerNumber(holder.Manifests[0].ImportManifest, bill1.JS_HouseBill, bill1Container1.JC_ContainerNum).BD_HazardousIndicator);
			AssertEquals("Bill1, Container2", false, GetContainerNumber(holder.Manifests[0].ImportManifest, bill1.JS_HouseBill, bill1Container2.JC_ContainerNum).BD_HazardousIndicator);
			AssertEquals("Bill1, Container3", true, GetContainerNumber(holder.Manifests[0].ImportManifest, bill1.JS_HouseBill, bill1Container3.JC_ContainerNum).BD_HazardousIndicator);
		}

		public void TestIgnoreWhenImportManifestsExist()
		{
			CreateAndAssertVoyageAndBillsOfLading();

			var manifest1 = Factory.New<CusSeaManTranHead>();
			manifest1.BT_RL_NKPortOfLastForeignPort = "USLAS";
			manifest1.BT_VesselName = "ADMIRALENGRACHT";
			manifest1.BT_VoyageNum = "002";

			var manifest2 = Factory.New<CusSeaManTranHead>();
			manifest2.BT_RL_NKPortOfLastForeignPort = "NZAKL";
			manifest2.BT_VesselName = "ADMIRALENGRACHT";
			manifest2.BT_VoyageNum = "002";

			var holder = manager.CreateImportManifests();
			holder.Factory.Save();

			var manifests = new CusSeaManTranHeadCollection(Factory);
			manifests.Load();
			AssertEquals(3, manifests.Count);
		}

		public void TestIsFirstArrivalCargoDischarge()
		{
			SetupVoyage("ADMIRALENGRACHT", "45S",
				new ZString[] { "NZAKL", "AUSYD", "AUBNE" },
				new DestinationPorts()
				{
					{ "AUSYD", new ZDateTime(2009, 08, 17) },
					{ "AUBNE", new ZDateTime(2009, 08, 10) },
					{ "AUADL", new ZDateTime(2009, 08, 12) }
				}
			);

			var bol1 = CreateShipment("NZAKL", "AUSYD");
			var bol2 = CreateShipment("NZAKL", "AUSYD");
			var bol3 = CreateShipment("NZAKL", "AUADL");
			var bol4 = CreateShipment("NZAKL", "AUADL");
			var bol5 = CreateShipment("NZAKL", "AUADL");

			Factory.Save();

			var holder = manager.CreateImportManifests();
			var collection = new ImportManifestItemCollection(holder.Manifests[0].ImportManifest, voyage.Sailings[0], true);

			AssertEquals(1, holder.Manifests.Count);

			collection.SelectAll(true);

			holder.Factory.Save();

			var manifests = new CusSeaManTranHeadCollection(Factory);
			manifests.Load();

			AssertEquals(1, manifests.Count);
			AssertEquals(true, manifests[0].IsInDatabase);

			var manifest1 = FetchManifestForVesselAndVoyage(manifests, "ADMIRALENGRACHT", "45S");

			AssertEquals(3, manifest1.Arrivals.Count);

			AssertEquals("AUSYD", manifest1.Arrivals[0].BA_RL_NKArrivalPort);
			AssertEquals(new ZDateTime(2009, 08, 17), manifest1.Arrivals[0].BA_ArrivalPortETA);
			AssertEquals("IsFirstArrival should be FALSE", false, manifest1.Arrivals[0].BA_IsFirstArrival);
			AssertEquals("CargoDischarge should be TRUE", true, manifest1.Arrivals[0].BA_DischargeIndicator);

			AssertEquals("AUBNE", manifest1.Arrivals[1].BA_RL_NKArrivalPort);
			AssertEquals(new ZDateTime(2009, 08, 10), manifest1.Arrivals[1].BA_ArrivalPortETA);
			AssertEquals("IsFirstArrival should be TRUE", true, manifest1.Arrivals[1].BA_IsFirstArrival);
			AssertEquals("CargoDischarge should be FALSE", false, manifest1.Arrivals[1].BA_DischargeIndicator);

			AssertEquals("AUADL", manifest1.Arrivals[2].BA_RL_NKArrivalPort);
			AssertEquals(new ZDateTime(2009, 08, 12), manifest1.Arrivals[2].BA_ArrivalPortETA);
			AssertEquals("IsFirstArrival should be FALSE", false, manifest1.Arrivals[2].BA_IsFirstArrival);
			AssertEquals("CargoDischarge should be TRUE", true, manifest1.Arrivals[2].BA_DischargeIndicator);
		}

		public void TestEmptyETA()
		{
			SetupVoyage("ADMIRALENGRACHT", "45S",
				new ZString[] { "NZAKL", "AUSYD", "AUBNE" },
				new DestinationPorts()
				{
					{ "AUSYD", new ZDateTime(2009, 08, 17) },
					{ "AUBNE", new ZDateTime(2009, 08, 10) },
					{ "AUADL", ZDateTime.Empty }
				}
			);

			var bol1 = CreateShipment("NZAKL", "AUSYD");
			var bol2 = CreateShipment("NZAKL", "AUSYD");
			var bol3 = CreateShipment("NZAKL", "AUADL");
			var bol4 = CreateShipment("NZAKL", "AUADL");
			var bol5 = CreateShipment("NZAKL", "AUADL");

			Factory.Save();

			var holder = manager.CreateImportManifests();
			var collection = new ImportManifestItemCollection(holder.Manifests[0].ImportManifest, voyage.Sailings[0], true);

			AssertEquals(1, holder.Manifests.Count);

			collection.SelectAll(true);

			holder.Factory.Save();

			var manifests = new CusSeaManTranHeadCollection(Factory);
			manifests.Load();

			AssertEquals(1, manifests.Count);
			AssertEquals(true, manifests[0].IsInDatabase);

			var manifest1 = FetchManifestForVesselAndVoyage(manifests, "ADMIRALENGRACHT", "45S");

			AssertContainsExactElementsInAnyOrder("manifest1.Arrivals",
				new string[] { "AUSYD", "AUADL", "AUBNE" },
				Array.ConvertAll(manifest1.Arrivals.ToArray<CusSeaManArrivalPort>(), (a) => a.BA_RL_NKArrivalPort.ToString()));

			var arrivalAUBNE = FetchArrivalPort(manifest1, "AUBNE");
			var arrivalAUSYD = FetchArrivalPort(manifest1, "AUSYD");
			var arrivalAUADL = FetchArrivalPort(manifest1, "AUADL");

			AssertEquals("AUSYD", arrivalAUSYD.BA_RL_NKArrivalPort);
			AssertEquals(new ZDateTime(2009, 08, 17), arrivalAUSYD.BA_ArrivalPortETA);
			AssertEquals("IsFirstArrival should be FALSE", false, arrivalAUSYD.BA_IsFirstArrival);
			AssertEquals("CargoDischarge should be TRUE", true, arrivalAUSYD.BA_DischargeIndicator);

			AssertEquals("AUBNE", arrivalAUBNE.BA_RL_NKArrivalPort);
			AssertEquals(new ZDateTime(2009, 08, 10), arrivalAUBNE.BA_ArrivalPortETA);
			AssertEquals("IsFirstArrival should be FALSE", false, arrivalAUBNE.BA_IsFirstArrival);
			AssertEquals("CargoDischarge should be FALSE", false, arrivalAUBNE.BA_DischargeIndicator);

			AssertEquals("AUADL", arrivalAUADL.BA_RL_NKArrivalPort);
			AssertEquals(ZDateTime.Empty, arrivalAUADL.BA_ArrivalPortETA);
			AssertEquals("IsFirstArrival should be FALSE", false, arrivalAUADL.BA_IsFirstArrival);
			AssertEquals("CargoDischarge should be TRUE", true, arrivalAUADL.BA_DischargeIndicator);
		}

		public void TestPerformLoadsUsingThePassedInFactoryNotTheVoyageFactory()
		{
			ZGuid voyagePK;
			{
				var createFactory = new BusinessObjectFactory();

				var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory)[0];

				var voyage = createFactory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "001";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				var sailing = voyage.Sailings[0];

				var bill = createFactory.New<BillOfLading>();
				bill.JS_JX = sailing.PK;

				var container1 = bill.RealContainers.AddNew();
				container1.JC_ContainerNum = "TEST4100013";
				container1.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				var booking = createFactory.New<AgencyBooking>();
				booking.JS_JX = sailing.PK;

				var container2 = booking.BookedContainers.AddNew();
				container2.JC_ContainerCount = 2;
				container2.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				createFactory.Save();

				voyagePK = voyage.PK;
			}

			{
				var sailingFactory = new BusinessObjectFactory();
				var generateFactory = new BusinessObjectFactory();

				var voyage = sailingFactory.Load<JobVoyage>(voyagePK);
				voyage.MarkAsNeedingValidationIncludingChildren();
				voyage.RunPreSaveValidation();
				sailingFactory.ResetDatabaseLoadCount();

				var wrapper = new CustomsJobVoyageWrapper(voyage);
				var creator = new ImportManifestFromSailingCreator(generateFactory, wrapper);

				var holder = creator.CreateImportManifests();

				AssertMaxDbHits("Should not have loaded anything new in the sailing factory", 0, sailingFactory);
				AssertEquals("manifest holder should be using the generate factory.", generateFactory, holder.Factory);
			}
		}

		[NUnit.Framework.TestDate(2015, 12, 9)]
		public void TestWeightOnContainerWhenCreateImportManifests()
		{
			SetupVoyage("ADMIRALENGRACHT", "45S",
				new ZString[] { "NZAKL", "AUSYD", "AUBNE" },
				new DestinationPorts()
				{
					{ "AUSYD", new ZDateTime(2015, 12, 9) },
					{ "AUBNE", new ZDateTime(2015, 12, 9) },
					{ "AUADL", ZDateTime.Empty }
				}
			);

			var shipment = CreateShipment("NZAKL", "AUSYD");
			var containerForShipment = shipment.RealContainers.AddNew();
			containerForShipment.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			containerForShipment.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			containerForShipment.JC_ContainerNum = "CONT1230549";
			containerForShipment.JC_TareWeight = 2281m;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 55;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_ActualVolume = 23;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 2000m;
			packLine.Containers.Add(containerForShipment);
			Factory.Save();

			var holder = manager.CreateImportManifests();
			var collection = new ImportManifestItemCollection(holder.Manifests[0].ImportManifest, voyage.Sailings[0], true);
			AssertEquals(1, holder.Manifests.Count);

			var manifest = holder.Manifests[0].ImportManifest;
			AssertEquals(1, manifest.OceanBills.Count);

			var detailsForBill = manifest.OceanBills[0].Details;
			AssertEquals(1, detailsForBill.Count);
			AssertEquals(2000m, detailsForBill[0].BD_GrossWeight);
		}

		#region Implementation

		CusSeaManOBLDetail GetContainerNumber(CusSeaManTranHead importManifest, string oceanBillId, string container)
		{
			for (var i = 0; i < importManifest.OceanBills.Count; i++)
			{
				if (StringComparer.OrdinalIgnoreCase.Compare(importManifest.OceanBills[i].BO_OceanBill, oceanBillId) == 0)
				{
					for (var j = 0; j < importManifest.OceanBills[i].Details.Count; j++)
					{
						if (StringComparer.OrdinalIgnoreCase.Compare(importManifest.OceanBills[i].Details[j].BD_ContainerNumber, container) == 0)
						{
							return importManifest.OceanBills[i].Details[j];
						}
					}
					return null;
				}
			}
			return null;
		}

		void CreateAndAssertVoyageAndBillsOfLading()
		{
			var vessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory)[0];

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_E_DEP = new ZDateTime(2006, 11, 1);
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKA";
			origin2.JA_E_DEP = new ZDateTime(2006, 11, 2);
			origin2.JA_A_DEP = new ZDateTime(2006, 11, 3);
			var origin3 = voyage.Origins.AddNew();
			origin3.JA_RL_NKPortOfLoading = "AUSYD";
			origin3.JA_E_DEP = new ZDateTime(2006, 11, 10);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = new ZDateTime(2006, 11, 9);
			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUCNS";
			destination2.JB_E_ARV = new ZDateTime(2006, 11, 12);
			var destination3 = voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "AUBNE";
			destination3.JB_E_ARV = new ZDateTime(2006, 11, 15);
			destination3.JB_A_ARV = new ZDateTime(2006, 11, 16);
			destination3.JB_Berth = "B1";
			destination3.JB_OA_ArrivalCTOAddress = cToAddress.PK;

			AssertEquals(8, voyage.Sailings.Count);
			var sailing1 = FetchSailingForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			AssertNotNull(sailing1);

			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			bill1.JS_HouseBill = "bill1";
			bill1.JS_GoodsDescription = "foo";
			bill1.DetailedGoodsDescriptionNoteText = ZString.Replicate('1', CusSeaManOBLDetailSchema.BD_GoodsDescription.MaxLength - 1) + "23333333";
			bill1.JS_MarksAndNumbersShort = "Marks and Numbers";
			bill1.JS_RL_NKOrigin = "SGSIN";
			bill1.JS_RL_NKDestination = "AUSYD";
			bill1.JS_OuterPacks = 22;
			bill1.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			bill1.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;

			bill1.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			bill1.ConsigneeDocumentaryAddress.E2_Address1 = "ConsigneeAddress1";
			bill1.ConsigneeDocumentaryAddress.E2_Address2 = "ConsigneeAddress2";
			bill1.ConsigneeDocumentaryAddress.E2_City = "ConsigneeCity";
			bill1.ConsigneeDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsigneeDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			bill1.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			bill1.ConsignorDocumentaryAddress.E2_Address1 = "ConsignorAddress1";
			bill1.ConsignorDocumentaryAddress.E2_Address2 = "ConsignorAddress2";
			bill1.ConsignorDocumentaryAddress.E2_City = "ConsignorCity";
			bill1.ConsignorDocumentaryAddress.E2_CompanyName = "Some Company";
			bill1.ConsignorDocumentaryAddress.E2_Postcode = "Post Code";
			bill1.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			var container1ForBill1 = bill1.RealContainers.AddNew();
			AssertEquals(1, bill1.RealContainers.Count);

			container1ForBill1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container1ForBill1.JC_ContainerCount = 3;
			container1ForBill1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1ForBill1.JC_ContainerNum = "C11";

			PackLine lineForBill1 = bill1.OuterPackLines.AddNew();
			lineForBill1.JL_PackageCount = 55;
			lineForBill1.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			lineForBill1.JL_ActualVolume = 23;
			lineForBill1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			lineForBill1.Containers.Add(container1ForBill1);

			var container2ForBill1 = bill1.RealContainers.AddNew();
			container2ForBill1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container2ForBill1.JC_ContainerNum = "C21";
			container2ForBill1.JC_IsEmptyContainer = true;

			var container3ForBill1 = bill1.RealContainers.AddNew();
			container3ForBill1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container3ForBill1.JC_ContainerCount = 3;
			container3ForBill1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container3ForBill1.JC_ContainerNum = "C31";

			AssertEquals(3, bill1.RealContainers.Count);
			AssertEquals(2, bill1.OuterPackLines.Count);

			var bill2 = Factory.New<BillOfLading>();
			bill2.JS_JX = ZGuid.Empty;
			bill2.JS_HouseBill = "BBK-COASTAL";
			bill2.JS_RL_NKDestination = "AUBNE";
			bill2.JS_RL_NKOrigin = "AUSYD";
			bill2.JS_GoodsDescription = "descr";
			bill2.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			bill2.JS_OuterPacks = 25;
			bill2.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			bill2.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;

			var transport2 = bill2.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_JX = FetchSailingPKForPorts(voyage.Sailings, "AUSYD", "AUBNE");

			var bill3 = Factory.New<BillOfLading>();
			bill3.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			bill3.JS_HouseBill = "bill3";
			bill3.JS_GoodsDescription = "haar";

			var container1ForBill3 = bill3.RealContainers.AddNew();
			container1ForBill3.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container1ForBill3.JC_ContainerCount = 30;
			container1ForBill3.JC_ContainerNum = "C13";

			PackLine lineForBill3 = bill3.OuterPackLines.AddNew();
			lineForBill3.JL_PackageCount = 10;
			lineForBill3.Containers.Add(container1ForBill3);

			AssertEquals(1, bill3.RealContainers.Count);
			AssertEquals(2, bill3.OuterPackLines.Count);

			var bill4 = Factory.New<BillOfLading>();
			bill4.JS_HouseBill = "Some Bill";
			bill4.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			bill4.JS_GoodsDescription = "fex";
			bill4.JS_MarksAndNumbersShort = "Marks and Numbers";
			bill4.JS_RL_NKOrigin = "SGSIN";
			bill4.JS_RL_NKDestination = "AUBNE";
			bill4.JS_OuterPacks = 19;
			bill4.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			bill4.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;

			AgencyShipmentContainer container1ForBill4 = bill4.RealContainers.AddNew();
			container1ForBill4.JC_ContainerNum = container3ForBill1.JC_ContainerNum;
			container1ForBill4.JC_RC = container3ForBill1.JC_RC;
			AgencyShipmentContainer container2ForBill4 = bill4.RealContainers.AddNew();
			container2ForBill4.JC_ContainerNum = container3ForBill1.JC_ContainerNum;
			container2ForBill4.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			AssertEquals(2, bill4.RealContainers.Count);

			var bill5 = Factory.New<BillOfLading>();
			bill5.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			bill5.JS_HouseBill = "RORO-TRANSHIP";
			bill5.JS_RL_NKDestination = "GBLON";
			bill5.JS_RL_NKOrigin = "NZAKL";
			bill5.JS_GoodsDescription = "descr";
			bill5.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			bill5.JS_OuterPacks = 3;
			bill5.JS_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			bill5.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;

			var bill6 = Factory.New<BillOfLading>();
			bill6.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			bill6.JS_HouseBill = "RORO-IMPORT";
			bill6.JS_RL_NKDestination = "AUSYD";
			bill6.JS_RL_NKOrigin = "NZAKL";
			bill6.JS_GoodsDescription = "imported cars";
			bill6.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			bill6.JS_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			bill6.JS_OuterPacks = 5;
			bill6.JS_ActualWeight = 1234m;
			bill6.JS_UnitOfWeight = "KG";
			bill6.JS_ActualVolume = 35m;
			bill6.JS_UnitOfVolume = "M3";
			bill6.JS_MarksAndNumbers = "MARKS6";
			PackLine lineForBill6 = bill6.OuterPackLines.AddNew();
			lineForBill6.JL_PackageCount = 0;
			lineForBill6.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			lineForBill6.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			bill6.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;

			var bill7 = Factory.New<BillOfLading>();
			bill7.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "AUSYD", "AUBNE");
			bill7.JS_HouseBill = "FCL-COASTAL";
			bill7.JS_RL_NKDestination = "AUBNE";
			bill7.JS_RL_NKOrigin = "AUSYD";
			bill7.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			bill7.JS_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			var containerForBill7 = bill7.RealContainers.AddNew();
			containerForBill7.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			containerForBill7.JC_ContainerCount = 1;
			containerForBill7.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			containerForBill7.JC_ContainerNum = "CFCLCAB";
			PackLine lineForBill7 = bill7.OuterPackLines.AddNew();
			lineForBill7.JL_PackageCount = 100;
			lineForBill7.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			lineForBill7.Containers.Add(containerForBill7);

			var bill8 = Factory.New<BillOfLading>();
			bill8.JS_JX = FetchSailingPKForPorts(voyage.Sailings, "NZAKL", "AUSYD");
			bill8.JS_HouseBill = "FCL-TRANSHIP";
			bill8.JS_RL_NKDestination = "GBLON";
			bill8.JS_RL_NKOrigin = "NZAKL";
			bill8.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			bill8.JS_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			var containerForBill8 = bill8.RealContainers.AddNew();
			containerForBill8.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			containerForBill8.JC_ContainerCount = 1;
			containerForBill8.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			containerForBill8.JC_ContainerNum = "CFCLEXP";
			PackLine lineForBill8 = bill8.OuterPackLines.AddNew();
			lineForBill8.JL_PackageCount = 102;
			lineForBill8.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			lineForBill8.Containers.Add(containerForBill8);

			Factory.Save();
		}

		CusSeaManTranHead FetchManifestForVesselAndVoyage(CusSeaManTranHeadCollection manifests, ZString vessel, ZString voyageNum)
		{
			foreach (var manifest in manifests.Cast<CusSeaManTranHead>())
			{
				if (manifest.BT_VoyageNum == voyageNum && manifest.BT_VesselName == vessel)
				{
					return manifest;
				}
			}

			var builder = new StringBuilder();
			builder.Append("Manifest not found for voyage number ");
			builder.Append(voyageNum);
			builder.Append(" - ");
			builder.AppendLine(vessel);

			foreach (var manifest in manifests.Cast<CusSeaManTranHead>())
			{
				builder.Append(" * ");
				builder.Append(manifest.BT_VoyageNum);
				builder.Append(" - ");
				builder.AppendLine(vessel);
			}

			Fail(builder.ToString());
			return null;
		}

		CusSeaManOBLHeader FetchOBLFromManifest(CusSeaManOBLHeaderCollection obls, ZString obl)
		{
			foreach (var header in obls.Cast<CusSeaManOBLHeader>())
			{
				if (header.BO_OceanBill == obl)
				{
					return header;
				}
			}

			return null;
		}

		static CusSeaManArrivalPort FetchArrivalPort(CusSeaManTranHead manifest1, string portName)
		{
			foreach (var port in manifest1.Arrivals.Cast<CusSeaManArrivalPort>())
			{
				if (port.BA_RL_NKArrivalPort == portName)
				{
					return port;
				}
			}

			return null;
		}

		static CusSeaManOBLHeaderCargoLine FetchCargoLine(CusSeaManArrivalPort arrival, string identifier)
		{
			foreach (var line in arrival.CargoLines.Cast<CusSeaManOBLHeaderCargoLine>())
			{
				if (line.CargoIdentifier == identifier)
				{
					return line;
				}
			}

			return null;
		}

		ZGuid FetchSailingPKForPorts(JobSailingCollection sailings, ZString loadPort, ZString dischargePort)
		{
			var sailing = FetchSailingForPorts(sailings, loadPort, dischargePort);
			if (sailing != null)
			{
				return sailing.PK;
			}

			return ZGuid.Empty;
		}

		JobSailing FetchSailingForPorts(JobSailingCollection sailings, ZString loadPort, ZString dischargePort)
		{
			foreach (var sailing in sailings.Cast<JobSailing>())
			{
				if (sailing.JX_JA_RL_NKPortOfLoading == loadPort && sailing.JX_JB_RL_NKPortOfDischarge == dischargePort)
				{
					return sailing;
				}
			}
			Fail(string.Format("Sailing not found for ports {0} and {1}.", loadPort, dischargePort));
			return null;
		}

		OrgAddress cToAddress
		{
			get { return _cToAddress ?? (_cToAddress = Factory.LoadTop1<OrgAddress>(new ZQuery())); }
		}
		OrgAddress _cToAddress;

		void SetupVoyage(ZString vessel, ZString voyageNumber, ZString[] origins, DestinationPorts destinations)
		{
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageNumber;

			foreach (var originPort in origins)
			{
				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = originPort;
			}

			foreach (var destinationPort in destinations.Keys)
			{
				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = destinationPort;
				destination.JB_E_ARV = destinations[destinationPort];
			}
		}

		AgencyShipment CreateShipment(ZString loadPort, ZString dischargePort)
		{
			AgencyShipment result = Factory.NewWithValidTestData<BillOfLading>();

			result.JS_JX = FetchSailingPKForPorts(voyage.Sailings, loadPort, dischargePort);

			return result;
		}

		class DestinationPorts : Dictionary<ZString, ZDateTime>
		{
		}

		protected override void SetUp()
		{
			base.SetUp();

			voyage = Factory.New<JobVoyage>();
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			manager = new ImportManifestFromSailingCreator(new BusinessObjectFactory(), voyageWrapper);
		}

		ImportManifestFromSailingCreator manager;
		JobVoyage voyage;
		CustomsJobVoyageWrapper voyageWrapper;

		#endregion
	}
}
