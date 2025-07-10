using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	public abstract class DocWhsDocketLineTest<TDocket, TLine, TWrapper> : DocumentWrapperTestCase
			where TDocket : WhsDocket
			where TLine : WhsDocketLine
			where TWrapper : DocWhsDocketLine
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocketLineWrapper };
		}

		public void TestProduct()
		{
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			part.OP_Desc = "123";
			AssertEquals("123", DocketLineWrapper.Product.Desc);
		}

		public void TestDGSubstance()
		{
			AssertEquals(null, DocketLineWrapper.DGSubstance);

			UNDGSubstance substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Code = "1234Z";
			AssertEquals(null, DocketLineWrapper.DGSubstance);

			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			part.UNDGs.AddNew().DI_DG = substance.PK;
			AssertEquals("1234Z", DocketLineWrapper.DGSubstance.UNNumberWithVariant);
		}

		#region TestLineNo

		public void TestLineNo()
		{
			TestLineNoCore();
		}

		protected virtual void TestLineNoCore()
		{
			DocketLine.WE_LineNo = 4;
			AssertEquals(4, DocketLineWrapper.LineNo);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			TestUnitsCore();
		}

		protected virtual void TestUnitsCore()
		{
			DocketLine.WE_TransactionQuantity = 9m;
			AssertEquals(9m, DocketLineWrapper.Units);
		}

		#endregion

		#region TestUnitsMet

		public void TestUnitsMet()
		{
			TestUnitsMetCore();
		}

		protected virtual void TestUnitsMetCore()
		{
			DocketLine.WE_TransactionQuantity = 10;
			AssertEquals(10m, DocketLineWrapper.UnitsMet);
		}

		#endregion

		#region TestUnitsBarcode

		public void TestUnitsBarcode()
		{
			AssertEquals("Precondition", DocketLine.WE_TransactionQuantity, 0.00m);
			TextBarcode barcode1 = new TextBarcode("0.00");
			AssertEquals(barcode1.TextAs128sFontString, DocketLineWrapper.UnitsBarcode);

			SetUnitsForBarcodeTest(1.97m);
			TextBarcode barcode2 = new TextBarcode("1.97");
			AssertEquals(barcode2.TextAs128sFontString, DocketLineWrapper.UnitsBarcode);
		}

		protected virtual void SetUnitsForBarcodeTest(ZDecimal units)
		{
			DocketLine.WE_TransactionQuantity = units;
		}

		#endregion

		#region TestUnitsMetBarcode

		public void TestUnitsMetBarcode()
		{
			TestUnitsMetBarcodeCore();
		}

		protected virtual void TestUnitsMetBarcodeCore()
		{
			var barcode1 = new TextBarcode("0.00");
			AssertEquals(barcode1.TextAs128sFontString, DocketLineWrapper.UnitsMetBarcode);

			DocketLine.WE_TransactionQuantity = 123.67m;
			var barcode2 = new TextBarcode("123.67");
			AssertEquals(barcode2.TextAs128sFontString, DocketLineWrapper.UnitsMetBarcode);
		}

		#endregion

		public void TestPackType()
		{
			DocketLine.WE_F3_NKPackType = "PLT";
			AssertEquals("PLT", DocketLineWrapper.PackType);
		}

		#region TestPackQty

		public void TestPackQty()
		{
			TestPackQtyCore();
		}

		protected virtual void TestPackQtyCore()
		{
			AssertEquals(0M, DocketLineWrapper.PackQty);
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			DocketLine.WE_TransactionQuantity = 30;
			AssertEquals(30M, DocketLineWrapper.PackQty);
		}

		#endregion

		#region TestPacks

		public void TestPacks()
		{
			TestPacksCore();
		}

		protected virtual void TestPacksCore()
		{
			AssertEquals(0M, DocketLineWrapper.PackQty);
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			DocketLine.WE_TransactionQuantity = 30;
			AssertEquals(30M, DocketLineWrapper.Packs);
		}

		#endregion

		#region TestExpiryDate

		public void TestExpiryDate()
		{
			var today = ZDate.Today;
			DocketLine.WE_ExpiryDate = today;
			AssertEquals(today, DocketLineWrapper.ExpiryDate);
		}

		#endregion

		#region TestPackingDate

		public void TestPackingDate()
		{
			var today = ZDate.Today;
			DocketLine.WE_PackingDate = today;
			AssertEquals(today, DocketLineWrapper.PackingDate);
		}

		#endregion

		public void TestLineComment()
		{
			DocketLine.WE_LineComment = "COMMENT123";
			AssertEquals("COMMENT123", DocketLineWrapper.LineComment);
		}

		#region TestProductCode

		public void TestProductCode()
		{
			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper PartNumber should be empty", ZString.Empty, DocketLineWrapper.ProductCode);

			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_PartNum = "PART";
			AssertEquals("DocWrapper PartNumber is incorrect", "PART", DocketLineWrapper.ProductCode);
		}

		#endregion

		#region TestProductBrandName

		public void TestProductBrandName()
		{
			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductBrandName should be empty", ZString.Empty, DocketLineWrapper.ProductBrandName);

			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_Brand = "TEST BRAND";
			AssertEquals("DocWrapper ProductBrandName is incorrect", "TEST BRAND", DocketLineWrapper.ProductBrandName);
		}

		#endregion

		#region TestProductModel

		public void TestProductModel()
		{
			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductModel should be empty", ZString.Empty, DocketLineWrapper.ProductModel);

			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_Model = "TEST MODEL";
			AssertEquals("DocWrapper ProductModel is incorrect", "TEST MODEL", DocketLineWrapper.ProductModel);
		}

		#endregion

		#region Test Custom Attributes

		public void TestCustomAttrib1()
		{
			DocketLine.WE_CustomAttrib1 = "ATTRIB1";
			AssertEquals("ATTRIB1", DocketLineWrapper.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			DocketLine.WE_CustomAttrib2 = "ATTRIB2";
			AssertEquals("ATTRIB2", DocketLineWrapper.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			DocketLine.WE_CustomAttrib3 = "ATTRIB3";
			AssertEquals("ATTRIB3", DocketLineWrapper.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			DocketLine.WE_CustomAttrib4 = "ATTRIB4";
			AssertEquals("ATTRIB4", DocketLineWrapper.CustomAttrib4);
		}

		public void TestCustomAttrib5()
		{
			DocketLine.WE_CustomAttrib5 = "ATTRIB5";
			AssertEquals("ATTRIB5", DocketLineWrapper.CustomAttrib5);
		}

		public void TestCustomAttrib6()
		{
			DocketLine.WE_CustomAttrib6 = "ATTRIB6";
			AssertEquals("ATTRIB6", DocketLineWrapper.CustomAttrib6);
		}

		public void TestCustomDate1()
		{
			DocketLine.WE_CustomDate1 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocketLineWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			DocketLine.WE_CustomDate2 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocketLineWrapper.CustomDate2);
		}

		public void TestCustomDate3()
		{
			DocketLine.WE_CustomDate3 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocketLineWrapper.CustomDate3);
		}

		public void TestCustomDate4()
		{
			DocketLine.WE_CustomDate4 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocketLineWrapper.CustomDate4);
		}

		public void TestCustomDate5()
		{
			DocketLine.WE_CustomDate5 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocketLineWrapper.CustomDate5);
		}

		public void TestCustomFlag1()
		{
			DocketLine.WE_CustomFlag1 = ZBool.True;
			AssertEquals(ZBool.True, DocketLineWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			DocketLine.WE_CustomFlag2 = ZBool.True;
			AssertEquals(ZBool.True, DocketLineWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			DocketLine.WE_CustomFlag3 = ZBool.True;
			AssertEquals(ZBool.True, DocketLineWrapper.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			DocketLine.WE_CustomFlag4 = ZBool.True;
			AssertEquals(ZBool.True, DocketLineWrapper.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			DocketLine.WE_CustomFlag5 = ZBool.True;
			AssertEquals(ZBool.True, DocketLineWrapper.CustomFlag5);
		}

		#endregion

		#region TestPartAttribute1Barcode

		public void TestPartAttribute1Barcode()
		{
			AssertEquals("", DocketLineWrapper.PartAttribute1Barcode);
			DocketLine.WE_PartAttrib1 = "PARTATTRIB1";
			TextBarcode barcode = new TextBarcode("PARTATTRIB1");
			AssertEquals(barcode.TextAs128sFontString, DocketLineWrapper.PartAttribute1Barcode);
		}

		#endregion

		#region TestPartAttributesWithLabel

		public void TestPartAttribute1WithLabel()
		{
			AssertEquals("DocWrapper PartAttribute1WithLabel should be empty", ZString.Empty, DocketLineWrapper.PartAttribute1WithLabel);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			DocketLine.Docket.WD_OH_Client = orgHeader.PK;
			DocketLine.WE_PartAttrib1 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel should be empty", ZString.Empty, DocketLineWrapper.PartAttribute1WithLabel);

			DocketLine.WE_PartAttrib1 = "TEST";
			DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "Attribute 1: TEST", DocketLineWrapper.PartAttribute1WithLabel);

			DocketLine.WE_PartAttrib1 = "TEST";
			DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "LABEL: TEST", DocketLineWrapper.PartAttribute1WithLabel);
		}

		public void TestPartAttribute2WithLabel()
		{
			AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, DocketLineWrapper.PartAttribute2WithLabel);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			DocketLine.Docket.WD_OH_Client = orgHeader.PK;
			DocketLine.WE_PartAttrib1 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, DocketLineWrapper.PartAttribute2WithLabel);

			DocketLine.WE_PartAttrib2 = "TEST2";
			DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel is correct", "Attribute 2: TEST2", DocketLineWrapper.PartAttribute2WithLabel);

			DocketLine.WE_PartAttrib2 = "TEST2";
			DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL2";
			AssertEquals("DocWrapper PartAttribute2WithLabel is correct", "LABEL2: TEST2", DocketLineWrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute3WithLabel()
		{
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocketLineWrapper.PartAttribute3WithLabel);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			DocketLine.Docket.WD_OH_Client = orgHeader.PK;
			DocketLine.WE_PartAttrib3 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocketLineWrapper.PartAttribute3WithLabel);

			DocketLine.WE_PartAttrib3 = "TEST3";
			DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel is correct", "Attribute 3: TEST3", DocketLineWrapper.PartAttribute3WithLabel);

			DocketLine.WE_PartAttrib3 = "TEST3";
			DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL3";
			AssertEquals("DocWrapper PartAttribute3WithLabel is correct", "LABEL3: TEST3", DocketLineWrapper.PartAttribute3WithLabel);
		}

		#endregion

		#region TestTrackedSerialWithLabel

		public void TestTrackedSerialWithLabel()
		{
			AssertEquals("DocWrapper TrackedSerialWithLabel  should be empty", ZString.Empty, DocketLineWrapper.TrackedSerialWithLabel);

			DocketLine.WE_SerialNumber = ZString.Empty;
			AssertEquals("DocWrapper TrackedSerialWithLabel  should be empty", ZString.Empty, DocketLineWrapper.TrackedSerialWithLabel);

			DocketLine.WE_SerialNumber = "TEST3";
			AssertEquals("DocWrapper TrackedSerialWithLabel  is correct", "Tracked Serial Number: TEST3", DocketLineWrapper.TrackedSerialWithLabel);
		}

		#endregion

		#region TestPartAttributesWithLabel_Translatable

		public void TestPartAttribute1WithLabel_Translatable()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			DocketLine.Docket.WD_OH_Client = orgHeader.PK;

			var miscServ = DocketLine.Docket.Client.MiscServ;
			DocketLine.WE_PartAttrib1 = "TEST";
			miscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1WithLabel in English", "LABEL: TEST", DocketLineWrapper.PartAttribute1WithLabel);

			var resKey1 = miscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey1, new ResourceStringData(resKey1, "标签"));
				AssertEquals("DocWrapper PartAttribute1WithLabel in Chinese", "标签: TEST", DocketLineWrapper.PartAttribute1WithLabel);
			}
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			DocketLine.Docket.WD_OH_Client = orgHeader.PK;

			var miscServ = DocketLine.Docket.Client.MiscServ;
			DocketLine.WE_PartAttrib2 = "TEST";
			miscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel in English", "LABEL: TEST", DocketLineWrapper.PartAttribute2WithLabel);

			var resKey2 = miscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey2, new ResourceStringData(resKey2, "标签"));
				AssertEquals("DocWrapper PartAttribute2WithLabel in Chinese", "标签: TEST", DocketLineWrapper.PartAttribute2WithLabel);
			}
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			DocketLine.Docket.WD_OH_Client = orgHeader.PK;

			var miscServ = DocketLine.Docket.Client.MiscServ;
			DocketLine.WE_PartAttrib3 = "TEST";
			miscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel in English", "LABEL: TEST", DocketLineWrapper.PartAttribute3WithLabel);

			var resKey3 = miscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey3, new ResourceStringData(resKey3, "标签"));
				AssertEquals("DocWrapper PartAttribute3WithLabel in Chinese", "标签: TEST", DocketLineWrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			AssertEquals("", DocketLineWrapper.PartAttribute1);

			DocketLine.WE_PartAttrib1 = "PARTATTRIB1";
			AssertEquals("PARTATTRIB1", DocketLineWrapper.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute2

		public void TestPartAttribute2()
		{
			AssertEquals("", DocketLineWrapper.PartAttribute2);

			DocketLine.WE_PartAttrib2 = "PARTATTRIB2";
			AssertEquals("PARTATTRIB2", DocketLineWrapper.PartAttribute2);
		}

		#endregion

		#region TestPartAttribute3

		public void TestPartAttribute3()
		{
			AssertEquals("", DocketLineWrapper.PartAttribute3);

			DocketLine.WE_PartAttrib3 = "PARTATTRIB3";
			AssertEquals("PARTATTRIB3", DocketLineWrapper.PartAttribute3);
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			AssertEquals("", DocketLineWrapper.TrackedSerialNumber);

			DocketLine.WE_SerialNumber = "SERIALNum";
			AssertEquals("SERIALNum", DocketLineWrapper.TrackedSerialNumber);
		}

		#endregion

		public void TestVolume()
		{
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			DocketLine.WE_TransactionQuantity = 10m;
			part.OP_Cubic = 10m;

			Docket.WD_TotalCubicUnit = "M3";
			AssertEquals(100m, DocketLineWrapper.Volume);
		}

		public void TestVolumeUQ()
		{
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			part.OP_CubicUQ = "M3";
			AssertEquals("M3", DocketLineWrapper.VolumeUQ);
		}

		public void TestWeight()
		{
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			DocketLine.WE_TransactionQuantity = 10m;
			part.OP_Weight = 10m;
			Docket.WD_TotalWeightUnit = "KG";
			AssertEquals(100m, DocketLineWrapper.Weight);
		}

		public void TestWeightUQ()
		{
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			part.OP_WeightUQ = "KG";
			AssertEquals("KG", DocketLineWrapper.WeightUQ);
		}

		public virtual void TestUnitsUQ()
		{
			AssertEquals("UNT", DocketLineWrapper.UnitsUQ);
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			part.OP_StockKeepingUnit = "PLT";
			AssertEquals("PLT", DocketLineWrapper.UnitsUQ);
		}

		public void TestTotalPallets()
		{
			Docket.WD_TotalPallets = 12;
			AssertEquals((ZShort)12, DocketLineWrapper.TotalPallets);
		}

		#region Implementation

		protected TWrapper DocketLineWrapper
		{
			get { return docketLineWrapper ?? (docketLineWrapper = CreateDocketLineWrapper(DocketLine)); }
		}

		protected TLine DocketLine
		{
			get { return docketLine ?? (docketLine = (TLine)Docket.Lines.AddNew()); }
		}

		protected TDocket Docket
		{
			get { return docket ?? (docket = Factory.New<TDocket>()); }
		}

		protected abstract TWrapper CreateDocketLineWrapper(TLine docketLine);

		TWrapper docketLineWrapper;
		TLine docketLine;
		TDocket docket;

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
