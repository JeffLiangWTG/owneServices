using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CargoAttributeValidationUtilitiesTest : TestCaseWithFactory
	{
		public void TestValidateSelection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTUMF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "UME", tariff);
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			testItems.EntryInstruction.CEI_CIQRequires = true;
			var declaration = testItems.JobDeclaration;
			var invoiceLine = testItems.InvoiceLine;
			var testCollection = invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			var oldProductsWarnig = "The goods seems to be used mechanical and electrical products, '旧品' may need to be selected.";
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoWarningContaining(invoiceLine.CargoAttributesAsStringInfo, oldProductsWarnig);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_NameOfGoods = "旧";
			invoiceLine.JI_Tariff = "TESTUMF";
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoWarningContaining(invoiceLine.CargoAttributesAsStringInfo, oldProductsWarnig);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasWarningContaining(invoiceLine.CargoAttributesAsStringInfo, oldProductsWarnig);
			invoiceLine.JI_NameOfGoods = "other";
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoWarningContaining(invoiceLine.CargoAttributesAsStringInfo, oldProductsWarnig);
			invoiceLine.JI_NameOfGoods = "旧";
			invoiceLine.JI_Tariff = "other";
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoWarningContaining(invoiceLine.CargoAttributesAsStringInfo, oldProductsWarnig);
			testCollection.AddNew(CargoAttributeList.Codes._21);
			invoiceLine.JI_NameOfGoods = "旧";
			invoiceLine.JI_Tariff = "TESTUMF";
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoWarningContaining(invoiceLine.CargoAttributesAsStringInfo, oldProductsWarnig);
		}

		public void TestValidateRequiredCargoAttributeByDangerousGoods()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTUMF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "DGC", tariff);
			Factory.Save();
			var invoiceLine = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory).InvoiceLine;
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;
			var cargoAttributes = invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			var testCollection = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes).OptionCollection;
			var targetInfo = cargoAttributes.SelectedOptionsAsStringPropertyInfo;
			AssertNoMessageErrorContaining("Should not check DGC without DGC Tariff.", targetInfo, "seem to be dangerous chemical, please select one of");
			invoiceLine.JI_Tariff = "TESTUMF";
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining("Should check DGC with DGC Tariff.", targetInfo, "seem to be dangerous chemical, please select one of");
			testCollection[CargoAttributeList.Codes._11].Selected = true;
			testCollection.RefreshSelectionCollection();
			AssertHasMessageErrorContaining("None of 31,32,33 selected, should contain the message error.", targetInfo, "seem to be dangerous chemical, please select one of");
			testCollection[CargoAttributeList.Codes._31].Selected = true;
			testCollection.RefreshSelectionCollection();
			AssertNoMessageErrorContaining("31 selected, should NOT contain the message error.", targetInfo, "seem to be dangerous chemical, please select one of");
			testCollection[CargoAttributeList.Codes._32].Selected = true;
			testCollection.RefreshSelectionCollection();
			AssertNoMessageErrorContaining("32 selected, should NOT contain the message error.", targetInfo, "seem to be dangerous chemical, please select one of");
			testCollection[CargoAttributeList.Codes._33].Selected = true;
			testCollection.RefreshSelectionCollection();
			AssertNoMessageErrorContaining("33 selected, should NOT contain the message error.", targetInfo, "seem to be dangerous chemical, please select one of");
		}

		public void TestIsAnyDangerousGoodsAttributeSelected()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var testCollection = invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			testCollection.AddNew(CargoAttributeList.Codes._11);
			AssertEquals("None of 31,32,33 selected, IsAnyDangerousGoodsAttributeSelected should return false.", false, invoiceLine.CargoAttributes.IsAnyDangerousGoodsAttributeSelected());
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._11));
			testCollection.AddNew(CargoAttributeList.Codes._31);
			AssertEquals("31 selected, IsAnyDangerousGoodsAttributeSelected should return true.", true, invoiceLine.CargoAttributes.IsAnyDangerousGoodsAttributeSelected());
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._31));
			testCollection.AddNew(CargoAttributeList.Codes._32);
			AssertEquals("32 selected, IsAnyDangerousGoodsAttributeSelected should return true.", true, invoiceLine.CargoAttributes.IsAnyDangerousGoodsAttributeSelected());
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._32));
			testCollection.AddNew(CargoAttributeList.Codes._33);
			AssertEquals("33 selected, IsAnyDangerousGoodsAttributeSelected should return true.", true, invoiceLine.CargoAttributes.IsAnyDangerousGoodsAttributeSelected());
		}
	}
}
