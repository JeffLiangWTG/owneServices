using System.Linq;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers.Testing
{
	sealed class DocJPAFRBillsTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocJPAFRBills.New(Bill, Factory);
		}

		public void TestPlaceOfDelivery()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.PlaceOfDelivery);
			Bill.JPB_RL_NKDelivery = "JPABA";
			AssertEquals("JPABA", testBillWrapper.PlaceOfDelivery);
			Bill.JPB_RL_NKDelivery = "AUSYD";
			AssertEquals("AUSYD", testBillWrapper.PlaceOfDelivery);
		}

		public void TestPlaceOfShipment()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.PlaceOfShipment);
			Bill.JPB_RL_NKFinalDestination = "JPABA";
			AssertEquals("JPABA", testBillWrapper.PlaceOfShipment);
			Bill.JPB_RL_NKFinalDestination = "AUSYD";
			AssertEquals("AUSYD", testBillWrapper.PlaceOfShipment);
		}

		public void TestConsignor()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals(@"
", testBillWrapper.Consignor);
			Bill.Consignor.E2_AddressOverride = true;
			Bill.Consignor.E2_Address1 = "Address 1";
			Bill.Consignor.E2_Address2 = "Address 2";
			Bill.Consignor.E2_City = "City";
			Bill.Consignor.E2_State = "State";
			Bill.Consignor.E2_RN_NKCountryCode = "AU";
			Bill.Consignor.E2_Phone = "Phone 1";
			AssertEquals(@"ADDRESS 1 ADDRESS 2 CITY STATE
Phone 1", testBillWrapper.Consignor);

			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "Consignor";
			testOrg.MainAddress.OA_Address1 = "New Address 1";
			testOrg.MainAddress.OA_Address2 = "New Address 2";
			testOrg.MainAddress.OA_City = "New City";
			testOrg.MainAddress.OA_State = "New State";
			testOrg.MainAddress.OA_RL_NKRelatedPortCode = "JPABA";
			testOrg.MainAddress.OA_Phone = "New Phone 1";
			Bill.Consignor.E2_OA_Address = testOrg.MainAddress.PK;
			Bill.Consignor.E2_AddressOverride = false;
			AssertEquals(@"CONSIGNOR NEW ADDRESS 1 NEW ADDRESS 2 NEW CITY, NEW STATE JAPAN
New Phone 1", testBillWrapper.Consignor);
		}

		public void TestConsignee()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals(@"
", testBillWrapper.Consignee);
			Bill.Consignee.E2_AddressOverride = true;
			Bill.Consignee.E2_Address1 = "Address 1";
			Bill.Consignee.E2_Address2 = "Address 2";
			Bill.Consignee.E2_City = "City";
			Bill.Consignee.E2_State = "State";
			Bill.Consignee.E2_RN_NKCountryCode = "AU";
			Bill.Consignee.E2_Phone = "Phone 1";
			AssertEquals(@"ADDRESS 1 ADDRESS 2 CITY STATE
Phone 1", testBillWrapper.Consignee);

			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "Consignee";
			testOrg.MainAddress.OA_Address1 = "New Address 1";
			testOrg.MainAddress.OA_Address2 = "New Address 2";
			testOrg.MainAddress.OA_City = "New City";
			testOrg.MainAddress.OA_State = "New State";
			testOrg.MainAddress.OA_RL_NKRelatedPortCode = "JPABA";
			testOrg.MainAddress.OA_Phone = "New Phone 1";
			Bill.Consignee.E2_OA_Address = testOrg.MainAddress.PK;
			Bill.Consignee.E2_AddressOverride = false;
			AssertEquals(@"CONSIGNEE NEW ADDRESS 1 NEW ADDRESS 2 NEW CITY, NEW STATE JAPAN
New Phone 1", testBillWrapper.Consignee);
		}

		public void TestNotifyParty()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals(@"
", testBillWrapper.NotifyParty);
			Bill.NotifyParty1.E2_AddressOverride = true;
			Bill.NotifyParty1.E2_Address1 = "Address 1";
			Bill.NotifyParty1.E2_Address2 = "Address 2";
			Bill.NotifyParty1.E2_City = "City";
			Bill.NotifyParty1.E2_State = "State";
			Bill.NotifyParty1.E2_RN_NKCountryCode = "AU";
			Bill.NotifyParty1.E2_Phone = "Phone 1";
			AssertEquals(@"ADDRESS 1 ADDRESS 2 CITY STATE
Phone 1", testBillWrapper.NotifyParty);

			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "NotifyParty";
			testOrg.MainAddress.OA_Address1 = "New Address 1";
			testOrg.MainAddress.OA_Address2 = "New Address 2";
			testOrg.MainAddress.OA_City = "New City";
			testOrg.MainAddress.OA_State = "New State";
			testOrg.MainAddress.OA_RL_NKRelatedPortCode = "JPABA";
			testOrg.MainAddress.OA_Phone = "New Phone 1";
			Bill.NotifyParty1.E2_OA_Address = testOrg.MainAddress.PK;
			Bill.NotifyParty1.E2_AddressOverride = false;
			AssertEquals(@"NOTIFYPARTY NEW ADDRESS 1 NEW ADDRESS 2 NEW CITY, NEW STATE JAPAN
New Phone 1", testBillWrapper.NotifyParty);
		}

		public void TestGoodsDescription()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.GoodsDescription);
			Bill.JPB_GoodsDescription = "test1";
			AssertEquals("test1", testBillWrapper.GoodsDescription);
			Bill.JPB_GoodsDescription = "test2";
			AssertEquals("test2", testBillWrapper.GoodsDescription);
		}

		public void TestMarksAndNumber()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.MarksAndNumber);
			Bill.JPB_MarksAndNumbers = "test1";
			AssertEquals("test1", testBillWrapper.MarksAndNumber);
			Bill.JPB_MarksAndNumbers = "test2";
			AssertEquals("test2", testBillWrapper.MarksAndNumber);
		}

		public void TestHSCode()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.HSCode);
			Bill.JPB_Tariff = "test1";
			AssertEquals("test1", testBillWrapper.HSCode);
			Bill.JPB_Tariff = "test2";
			AssertEquals("test2", testBillWrapper.HSCode);
		}

		public void TestHouseBillNumber()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.HouseBillNumber);
			Bill.JPB_BillNumber = "test1";
			AssertEquals("test1", testBillWrapper.HouseBillNumber);
			Bill.JPB_BillNumber = "test2";
			AssertEquals("test2", testBillWrapper.HouseBillNumber);
		}

		public void TestPackageInfo()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("0 ", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestQty = 100;
			Bill.JPB_ManifestUQ = "BA";
			AssertEquals("100 Barrel", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "BE";
			AssertEquals("100 Bundle", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "BG";
			AssertEquals("100 Bag", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "BK";
			AssertEquals("100 Basket", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "BL";
			AssertEquals("100 Bale, compressed", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "BN";
			AssertEquals("100 Bale, non-compressed", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "BR";
			AssertEquals("100 Bar", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "BX";
			AssertEquals("100 Box", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CA";
			AssertEquals("100 Can, rectangular", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CG";
			AssertEquals("100 Cage", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CH";
			AssertEquals("100 Chest", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CK";
			AssertEquals("100 Cask", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CL";
			AssertEquals("100 Coil", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CN";
			AssertEquals("100 Container", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CO";
			AssertEquals("100 Carboy, non-protected", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CP";
			AssertEquals("100 Carboy, protected", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CR";
			AssertEquals("100 Crate", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CS";
			AssertEquals("100 Case", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CT";
			AssertEquals("100 Carton", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CX";
			AssertEquals("100 Can, cylindrical", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "CY";
			AssertEquals("100 Cylinder", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "DJ";
			AssertEquals("100 Demijohn, non-protected", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "DP";
			AssertEquals("100 Demijohn, protected", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "DR";
			AssertEquals("100 Drum", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "FB";
			AssertEquals("100 Fibre Drum", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "FL";
			AssertEquals("100 Flask", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "FR";
			AssertEquals("100 Frame", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "FT";
			AssertEquals("100 Flexible Container", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "HD";
			AssertEquals("100 HEAD", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "HG";
			AssertEquals("100 Hogshead", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "IN";
			AssertEquals("100 Ingot", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "JR";
			AssertEquals("100 Jar", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "JG";
			AssertEquals("100 Jug", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "KG";
			AssertEquals("100 Keg", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "LG";
			AssertEquals("100 Log", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "LZ";
			AssertEquals("100 Logs, in bundle/bunch/truss", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "MT";
			AssertEquals("100 Mat", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "NE";
			AssertEquals("100 Unpacked or unpackaged", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "NT";
			AssertEquals("100 Net", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PA";
			AssertEquals("100 Packet", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PC";
			AssertEquals("100 Parcel", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PE";
			AssertEquals("100 Pen", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PG";
			AssertEquals("100 Plate", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PI";
			AssertEquals("100 Pipe", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PK";
			AssertEquals("100 Package", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PL";
			AssertEquals("100 Pail", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PP";
			AssertEquals("100 Pallet And Package", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PS";
			AssertEquals("100 Piece", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PU";
			AssertEquals("100 Tray", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PY";
			AssertEquals("100 Plates, in bundle/bunch/truss", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "PZ";
			AssertEquals("100 Planks or Pipes, in bundle/bunch/truss", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "RL";
			AssertEquals("100 Reel", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "RO";
			AssertEquals("100 Roll", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SA";
			AssertEquals("100 Sack", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SI";
			AssertEquals("100 Skid", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SF";
			AssertEquals("100 Set", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SK";
			AssertEquals("100 Skeleton Case", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SB";
			AssertEquals("100 Slab", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SS";
			AssertEquals("100 Steel Case", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "ST";
			AssertEquals("100 Sheet", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SV";
			AssertEquals("100 Steel Envelop", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "SZ";
			AssertEquals("100 Sheet,in bundle/bunch/truss", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "TB";
			AssertEquals("100 Tub", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "TI";
			AssertEquals("100 Tierce", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "TN";
			AssertEquals("100 Tin", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VA";
			AssertEquals("100 Vat", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VG";
			AssertEquals("100 Bulk, gas (at 1031mber and 15 Degree)", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VL";
			AssertEquals("100 Bulk, liquid", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VO";
			AssertEquals("100 Bulk, solid, large particles (nodules)", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VK";
			AssertEquals("100 Van-pack", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VQ";
			AssertEquals("100 Bulk, liquefied gas (at abnormal temperature/pressure)", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VR";
			AssertEquals("100 Bulk, solid, granular particles (grains)", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "VY";
			AssertEquals("100 Bulk, solid, fine particles (powders)", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "WD";
			AssertEquals("100 Wooden Drum", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestUQ = "ZZ";
			AssertEquals("100 Other", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestQty = 100;
			Bill.JPB_ManifestUQ = "";
			AssertEquals("100 ", testBillWrapper.PackageInfo);
			Bill.JPB_ManifestQty = 100;
			Bill.JPB_ManifestUQ = "VT";
			AssertEquals("100 VT", testBillWrapper.PackageInfo);
		}

		public void TestVolumeInfo()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("0 MTQ", testBillWrapper.VolumeInfo);
			Bill.JPB_Volume = 100;
			Bill.JPB_VolumeUQ = "M3";
			AssertEquals("100 MTQ", testBillWrapper.VolumeInfo);
			Bill.JPB_VolumeUQ = "CF";
			AssertEquals("100 FTQ", testBillWrapper.VolumeInfo);
			Bill.JPB_VolumeUQ = "BF";
			AssertEquals("100 BFT", testBillWrapper.VolumeInfo);
			Bill.JPB_VolumeUQ = "VT";
			AssertEquals("100 VT", testBillWrapper.VolumeInfo);
		}

		public void TestGrossWeightInfo()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("0 KGM", testBillWrapper.GrossWeightInfo);
			Bill.JPB_GrossWeight = 100;
			Bill.JPB_GrossWeightUQ = "KG";
			AssertEquals("100 KGM", testBillWrapper.GrossWeightInfo);
			Bill.JPB_GrossWeightUQ = "T";
			AssertEquals("100 TNE", testBillWrapper.GrossWeightInfo);
			Bill.JPB_GrossWeightUQ = "LB";
			AssertEquals("100 LBR", testBillWrapper.GrossWeightInfo);
			Bill.JPB_GrossWeightUQ = "VT";
			AssertEquals("100 VT", testBillWrapper.GrossWeightInfo);
		}

		public void TestDangerousGoodIMDG()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.DangerousGoodIMDG);
			Bill.JPB_DG_NKSubstance = "XXXX";
			AssertEquals("", testBillWrapper.DangerousGoodIMDG);
			Bill.JPB_DG_NKSubstance = "1100";
			AssertEquals("3", testBillWrapper.DangerousGoodIMDG);
			Bill.JPB_DG_NKSubstance = "2000";
			AssertEquals("4.1", testBillWrapper.DangerousGoodIMDG);
			Bill.Substance.DG_Class = "";
			AssertEquals("", testBillWrapper.DangerousGoodIMDG);
			Bill.Substance.DG_Class = "1.1A";
			AssertEquals("1.1A", testBillWrapper.DangerousGoodIMDG);
			Bill.Substance.DG_Class = "2.0";
			AssertEquals("2.0", testBillWrapper.DangerousGoodIMDG);
		}

		public void TestDangerousGoodUNDG()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			AssertEquals("", testBillWrapper.DangerousGoodUNDG);
			Bill.JPB_DG_NKSubstance = "";
			AssertEquals("", testBillWrapper.DangerousGoodUNDG);
			Bill.JPB_DG_NKSubstance = "1100";
			AssertEquals("1100", testBillWrapper.DangerousGoodUNDG);
			Bill.JPB_DG_NKSubstance = "2000";
			AssertEquals("2000", testBillWrapper.DangerousGoodUNDG);
		}

		public void TestContainers()
		{
			var testBillWrapper = GetNewDocumentWrapper() as DocJPAFRBills;
			var testCont11 = Bill.Containers.AddNew();
			testCont11.JPC_ContainerNum = "Cont12";
			var testCont12 = Bill.Containers.AddNew();
			testCont12.JPC_ContainerNum = "Cont11";
			var testCont13 = Bill.Containers.AddNew();
			testCont13.JPC_ContainerNum = "Cont12";
			var testCont14 = Bill.Containers.AddNew();
			var testCont15 = Bill.Containers.AddNew();

			AssertEquals(5, testBillWrapper.Containers.Count);
			AssertEquals(1, testBillWrapper.Containers.Count(cont => { var container = cont as DocJPAFRContainer; return container != null && container.ContainerSequence == 1 && container.ContainerNumber == "Cont12"; }));
			AssertEquals(1, testBillWrapper.Containers.Count(cont => { var container = cont as DocJPAFRContainer; return container != null && container.ContainerSequence == 2 && container.ContainerNumber == "Cont11"; }));
			AssertEquals(1, testBillWrapper.Containers.Count(cont => { var container = cont as DocJPAFRContainer; return container != null && container.ContainerSequence == 3 && container.ContainerNumber == "Cont12"; }));
			AssertEquals(1, testBillWrapper.Containers.Count(cont => { var container = cont as DocJPAFRContainer; return container != null && container.ContainerSequence == 4 && container.ContainerNumber == ""; }));
			AssertEquals(1, testBillWrapper.Containers.Count(cont => { var container = cont as DocJPAFRContainer; return container != null && container.ContainerSequence == 5 && container.ContainerNumber == ""; }));
		}

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Factory.New<JPAFRBills>()); }
		}
		JPAFRBills bill;
	}
}
