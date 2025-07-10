using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class WarehouseDocketLineWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region Properties

		#region TestHoldCode

		protected override void TestHoldCodeCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocketLineWrapper.HoldCode);
			DocketLine.WE_WHC_NKCurrentInventoryHeldCode = "HELD";
			AssertEquals("HELD", DocketLineWrapper.HoldCode);
		}

		#endregion

		#region TestHoldReason

		protected override void TestHoldReasonCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocketLineWrapper.HoldReason);
			DocketLine.WE_CurrentHoldReason = "ABC";
			AssertEquals("ABC", DocketLineWrapper.HoldReason);
		}

		#endregion

		#region TestPartAttribute2Name

		protected override void TestPartAttribute2NameCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocketLineWrapper.PartAttribute2Name);
			Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			Docket.Client.MiscServ.OM_IMPartAttrib2Name = "Batch";
			AssertEquals("Batch", DocketLineWrapper.PartAttribute2Name);
		}

		#endregion

		#region TestPartAttribute3Name

		protected override void TestPartAttribute3NameCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocketLineWrapper.PartAttribute2Name);
			Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			Docket.Client.MiscServ.OM_IMPartAttrib3Name = "Batch";
			AssertEquals("Batch", DocketLineWrapper.PartAttribute3Name);
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			TestTrackedSerialNumberCore();
		}

		protected virtual void TestTrackedSerialNumberCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocketLineWrapper.TrackedSerialNumber);
			DocketLine.WE_SerialNumber = "FRT";
			AssertEquals("FRT", DocketLineWrapper.TrackedSerialNumber);
		}

		#endregion

		#region TestTrackedSerialWithLabel

		public void TestTrackedSerialWithLabel()
		{
			TestTrackedSerialWithLabelCore();
		}

		protected virtual void TestTrackedSerialWithLabelCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocketLineWrapper.TrackedSerialWithLabel);
			DocketLine.WE_SerialNumber = "FRT";
			AssertEquals("Tracked Serial Number: FRT", DocketLineWrapper.TrackedSerialWithLabel);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_Width = 10m;
			DocketLine.WE_OP = part.PK;
			AssertEquals(part, DocketLineWrapper.Product.WrappedObject);
			AssertEquals(10m, DocketLineWrapper.Product.Product.OP_Width);
		}

		#endregion

		#region TestLocationString

		public void TestLocationString()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket();
			docket.WD_WW_Whs = data.Whs1.PK;

			var docketLine = docket.Lines.AddNew();
			if (docketLine is WhsTransferLine transferLine)
			{
				transferLine.TransferFromLocationString = "A-1";
			}

			docketLine.LocationString = "A-2";
			AssertLocationString(docketLine);
		}

		protected virtual void AssertLocationString(WhsDocketLine line)
		{
			AssertEquals(line.LocationString, GetNewWarehouseLineWrapper(line).LocationString);
		}

		#endregion

		#region TestLocationString2

		public void TestLocationString2()
		{
			var docket = GetNewDocket();
			var docketLine = docket.Lines.AddNew();
			if (docketLine is WhsTransferLine transferLine)
			{
				transferLine.TransferFromLocationString = "ABC";
			}

			docketLine.LocationString = "XYZ";
			AssertLocationString2(docketLine);
		}

		protected virtual void AssertLocationString2(WhsDocketLine line)
		{
			AssertEquals("", GetNewWarehouseLineWrapper(line).LocationString2);
		}

		#endregion

		#region TestCustomsSecondQuantity

		public void TestCustomsSecondQuantity()
		{
			AssertEquals("Precondition", 0.0m, DocketLineWrapper.CustomsSecondQuantity);
			GetCustomsData().WB_CustomsSecondQuantity = 10.2m;
			AssertEquals(10.2M, DocketLineWrapper.CustomsSecondQuantity);
		}

		#endregion

		#region TestCustomsSecondUnitQty

		public void TestCustomsSecondUnitQty()
		{
			AssertEquals("Precondition", ZString.Empty, DocketLineWrapper.CustomsSecondUnitQty);
			GetCustomsData().WB_CustomsSecondUnitQty = "GRM";
			AssertEquals("GRM", DocketLineWrapper.CustomsSecondUnitQty);
		}

		#endregion

		#region TestCustomsThirdQuantity

		public void TestCustomsThirdQuantity()
		{
			AssertEquals("Precondition", 0.0m, DocketLineWrapper.CustomsThirdQuantity);
			GetCustomsData().WB_CustomsThirdQuantity = 11.2m;
			AssertEquals(11.2M, DocketLineWrapper.CustomsThirdQuantity);
		}

		#endregion

		#region TestCustomsThirdUnitQty

		public void TestCustomsThirdUnitQty()
		{
			AssertEquals("Precondition", ZString.Empty, DocketLineWrapper.CustomsThirdUnitQty);
			GetCustomsData().WB_CustomsThirdUnitQty = "KG";
			AssertEquals("KG", DocketLineWrapper.CustomsThirdUnitQty);
		}

		#endregion

		#region TestManufacturerAddress

		public void TestManufacturerAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org1.MainAddress;
			address1.OA_Email = "a1@a1.com";

			GetCustomsData().WB_OA_ManufacturerAddress = address1.PK;
			AssertEquals(address1.PK, DocketLineWrapper.ManufacturerAddress.WrappedObjectPK);
			AssertEquals("a1@a1.com", DocketLineWrapper.ManufacturerAddress.Email);
		}

		#endregion

		#region TestTariff

		public void TestTariff()
		{
			AssertEquals("Precondition", ZString.Empty, DocketLineWrapper.Tariff);
			GetCustomsData().WB_Tariff = "ABC";
			AssertEquals("ABC", DocketLineWrapper.Tariff);
		}

		#endregion

		#region TestPrimaryPreference

		public void TestPrimaryPreference()
		{
			AssertEquals("Precondition", ZString.Empty, DocketLineWrapper.PrimaryPreference);
			GetCustomsData().WB_PrimaryPreference = "PRT";
			AssertEquals("PRT", DocketLineWrapper.PrimaryPreference);
		}

		#endregion

		#region TestTranslatable

		public void TestPartAttribute1Name_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			Docket.WD_OH_Client = client.PK;
			client.MiscServ.OM_IMPartAttrib1Name = "Batch";
			AssertEquals("PartAttribute1Name in English.", "Batch", DocketLineWrapper.PartAttribute1Name);

			var resKey = client.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "Batch").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "批号"));
				AssertEquals("PartAttribute1Name in Chinese.", "批号", DocketLineWrapper.PartAttribute1Name);
			}
		}

		public void TestPartAttribute2Name_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			Docket.WD_OH_Client = client.PK;
			client.MiscServ.OM_IMPartAttrib2Name = "Batch";
			AssertEquals("PartAttribute2Name in English.", "Batch", DocketLineWrapper.PartAttribute2Name);

			var resKey = client.MiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "Batch").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "批号"));
				AssertEquals("PartAttribute2Name in Chinese.", "批号", DocketLineWrapper.PartAttribute2Name);
			}
		}

		public void TestPartAttribute3Name_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			Docket.WD_OH_Client = client.PK;
			client.MiscServ.OM_IMPartAttrib3Name = "Batch";
			AssertEquals("PartAttribute3Name in English.", "Batch", DocketLineWrapper.PartAttribute3Name);

			var resKey = client.MiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "Batch").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "批号"));
				AssertEquals("PartAttribute3Name in Chinese.", "批号", DocketLineWrapper.PartAttribute3Name);
			}
		}

		public void TestPartAttribute1NameWithLabel_Translatable()
		{
			TestPartAttribute1NameWithLabel_TranslatableCore();
		}

		public virtual void TestPartAttribute1NameWithLabel_TranslatableCore()
		{
			var client = Factory.New<OrgHeader>();
			DocketLine.Docket.WD_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			DocketLine.WE_PartAttrib1 = "TEST";
			miscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("PartAttribute1WithLabel in English", "LABEL: TEST", DocketLineWrapper.PartAttribute1WithLabel);

			var resKey = miscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute1WithLabel in Chinese", "标签: TEST", DocketLineWrapper.PartAttribute1WithLabel);
			}
		}

		public void TestPartAttribute2NameWithLabel_Translatable()
		{
			TestPartAttribute2NameWithLabel_TranslatableCore();
		}

		public virtual void TestPartAttribute2NameWithLabel_TranslatableCore()
		{
			var client = Factory.New<OrgHeader>();
			DocketLine.Docket.WD_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			DocketLine.WE_PartAttrib2 = "TEST";
			miscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("PartAttribute2WithLabel in English", "LABEL: TEST", DocketLineWrapper.PartAttribute2WithLabel);

			var resKey = miscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute2WithLabel in Chinese", "标签: TEST", DocketLineWrapper.PartAttribute2WithLabel);
			}
		}

		public void TestPartAttribute3NameWithLabel_Translatable()
		{
			TestPartAttribute3NameWithLabel_TranslatableCore();
		}

		public virtual void TestPartAttribute3NameWithLabel_TranslatableCore()
		{
			var client = Factory.New<OrgHeader>();
			DocketLine.Docket.WD_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			DocketLine.WE_PartAttrib3 = "TEST";
			miscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("PartAttribute3WithLabel in English", "LABEL: TEST", DocketLineWrapper.PartAttribute3WithLabel);

			var resKey = miscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute3WithLabel in Chinese", "标签: TEST", DocketLineWrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override void SetAttribute1Core() => DocketLine.WE_PartAttrib1 = "AT1";

		protected override void SetAttribute2Core() => DocketLine.WE_PartAttrib2 = "AT2";

		protected override void SetAttribute3Core() => DocketLine.WE_PartAttrib3 = "AT3";

		protected override void SetExpiryDateCore() => DocketLine.WE_ExpiryDate = ZDate.Today.AddDays(7);

		protected override void SetPackingDateCore() => DocketLine.WE_PackingDate = ZDate.Today;

		protected override void SetSerialNumberCore() => DocketLine.WE_SerialNumber = "SNX";

		protected override WarehouseGenericLineWrapper SetProductDGCore()
		{
			var part = Factory.New<OrgSupplierPart>();
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_UNNO = "9001";
			undg.DG_Variant = "a";
			part.UNDGs.AddNew().DI_DG = undg.PK;
			DocketLine.WE_OP = part.PK;
			return GetNewWarehouseLineWrapper(DocketLine);
		}

		#endregion

		#region TestLineNo

		public void TestLineNo()
		{
			TestLineNoCore();
		}

		protected virtual void TestLineNoCore()
		{
			var docketLine = GetNewDocket().Lines.AddNew();
			docketLine.WE_LineNo = 2;

			var docketLineWrapper = (WarehouseDocketLineWrapper)GetNewWarehouseLineWrapper(docketLine);
			AssertEquals(nameof(docketLineWrapper.LineNo), "00002", docketLineWrapper.LineNo);
		}

		#endregion

		#endregion

		#region Business Objects For Testing

		protected WhsDocket Docket
		{
			get { return docket ?? (docket = GetNewDocket()); }
		}
		WhsDocket docket;
		protected abstract WhsDocket GetNewDocket();

		protected WhsDocketLine DocketLine
		{
			get { return docketLine ?? (docketLine = Docket.Lines.AddNew()); }
		}
		WhsDocketLine docketLine;

		protected override BusinessObject GetNewBusinessObject()
		{
			return DocketLine;
		}

		protected virtual IWhsBondedWarehouseAttribute GetCustomsData()
		{
			return DocketLine.CustomsData;
		}

		protected WarehouseDocketLineWrapper DocketLineWrapper
		{
			get { return docketLineWrapper ?? (docketLineWrapper = (WarehouseDocketLineWrapper)GetNewWarehouseLineWrapper(DocketLine)); }
		}
		WarehouseDocketLineWrapper docketLineWrapper;

		#endregion
	}
}
