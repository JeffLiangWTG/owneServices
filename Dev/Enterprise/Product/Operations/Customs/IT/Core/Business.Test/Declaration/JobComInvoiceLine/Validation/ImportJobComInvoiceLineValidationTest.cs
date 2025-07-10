using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportJobComInvoiceLineValidationTest : CommonJobComInvoiceLineValidationTest
{
	public void TestCheckJI_DescriptionMaximumLengthValidationWithUCC6()
	{
		var expectedMessageError = "Goods Description exceeds the maximum allowed length in the declaration message (512 characters). Excess characters will be truncated.";

		invoiceLine.JI_Description = new string('0', 511);
		AssertNoWarningContaining("When Description length less than 512", invoiceLine.JI_DescriptionInfo, expectedMessageError);

		invoiceLine.JI_Description = new string('0', 512);
		AssertNoWarningContaining("When Description length equal to 512", invoiceLine.JI_DescriptionInfo, expectedMessageError);

		invoiceLine.JI_Description = new string('0', 513);
		AssertHasWarningContaining("When Description length more than 512", invoiceLine.JI_DescriptionInfo, expectedMessageError);
	}

	public void TestCheckJI_PrimaryPreference()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure("IT", "IM", "40", "", "", "Procedure1", "IMP", intoWarehouse: false);
		helper.CreateRefCusProcedure("IT", "IM", "71", "", "", "Procedure1", "IMP", intoWarehouse: true);

		entryInstruction.CEI_Procedure = "71";
		invoiceLine.JI_PrimaryPreference = "";
		AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Procedure = "40";
		invoiceLine.JI_PrimaryPreference = "";
		AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Procedure = "40";
		invoiceLine.JI_PrimaryPreference = "100";
		AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJI_ValuationCode()
	{
		invoiceLine.JI_ValuationCode = "2";
		AssertNoMessageErrorContaining("When ValuationCode entered and valid", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
		AssertNoMessageErrorContaining("When ValuationCode entered and valid", invoiceLine.JI_ValuationCodeInfo, ListValidation.InvalidCodeMessageError.ToString());

		invoiceLine.JI_ValuationCode = "";
		AssertHasMessageErrorContaining("When ValuationCode Empty", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine.JI_ValuationCode = "9";
		AssertHasMessageErrorContaining("When ValuationCode is invalid", invoiceLine.JI_ValuationCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckCustomsQuantityAndNetWeightEquality()
	{
		const string expectedWarningMessage = "Customs Quantity is usually equal to Net Weight";

		CombineAssertions("CustomsQuantity rounding and unit conversion checks", () =>
		{
			invoiceLine.JI_NetWeight = 1.124;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1.123566m;
			AssertNoWarning("Should not warn: Rounded CustomsQuantity (1.123566 → 1.124) == NetWeight (1.124)", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);

			invoiceLine.JI_CustomsQuantity = 1.123456m;
			AssertHasWarning("Should warn: Rounded CustomsQuantity (1.123456 → 1.123) != NetWeight (1.124)", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);

			invoiceLine.JI_NetWeight = 1124;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine.JI_CustomsQuantity = 1.124000m;
			AssertNoWarning("Should not warn: NetWeight in grams (1124g = 1.124kg) == CustomsQuantity (1.124)", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);

			invoiceLine.JI_CustomsQuantity = 1.123000m;
			AssertHasWarning("Should warn: NetWeight in grams (1124g = 1.124kg) != CustomsQuantity (1.123)", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);
		});
	}

	public void TestCheckJI_Weight()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CheckInvoiceLineMandatoryField(invoiceLine.JI_WeightInfo);
	}

	public void TestCheckJI_OA_ExporterAddress()
	{
		var exporter = Factory.NewWithValidTestData<OrgHeader>();
		var propertyInfo = invoiceLine.JI_OA_ExporterAddressInfo;
		invoiceLine.JI_OA_ExporterAddress = exporter.MainAddress.PK;

		var mainAddress = exporter.MainAddress;
		declaration.JE_MessageType = "IMP";

		var expectedAddressWarningMessage = "Consignor Address is longer than 70 characters, it will be truncated in the message.";
		mainAddress.OA_Address1 = "".PadRight(35, 'A');
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertNoWarningContaining(propertyInfo, expectedAddressWarningMessage);

		mainAddress.OA_Address2 = "".PadRight(36, 'A');
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertHasWarningContaining(propertyInfo, expectedAddressWarningMessage);

		var expectedCityWarningMessage = "Consignor City is longer than 35 characters, it will be truncated in the message.";
		mainAddress.OA_City = "Milan";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertNoWarningContaining(propertyInfo, expectedCityWarningMessage);

		mainAddress.OA_City = "Llanfairpwllgwyngyllgogerychwyrndrobwllllantysilio";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertHasWarningContaining(propertyInfo, expectedCityWarningMessage);

		exporter.OH_FullName = "".PadRight(75, 'A');
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		var expectedCompanyNameWarningMessageForImport = "Consignor Company Name is longer than 70 characters, it will be truncated in the message.";
		AssertHasWarningContaining(propertyInfo, expectedCompanyNameWarningMessageForImport);

		exporter.OH_FullName = "SHORT";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertNoWarningContaining(propertyInfo, expectedCompanyNameWarningMessageForImport);

		mainAddress.OA_PostCode = "1234567890";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		var expectedPostCodeWarningMessageForImport = "Consignor Postcode is longer than 9 characters, it will be truncated in the message.";
		AssertHasWarningContaining(propertyInfo, expectedPostCodeWarningMessageForImport);

		mainAddress.OA_PostCode = "20154";
		invoiceLine.Validation.ValidateJI_OA_ExporterAddress();
		AssertNoWarningContaining(propertyInfo, expectedPostCodeWarningMessageForImport);
	}

	public void TestTraderJobDocAddressValidationCaptions()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertTraderJobDocAddressValidationCaptions(invoiceLine.SellerDocAddress, invoiceLine.SellerDocAddress.OrganisationPKInfo, "Seller");
		AssertTraderJobDocAddressValidationCaptions(invoiceLine.BuyerDocAddress, invoiceLine.BuyerDocAddress.OrganisationPKInfo, "Buyer");
	}

	public void TestCheckJI_Tariff_WhenCountryOfDispatchIsTurkey()
	{
		DutyCalculatorStrategyTurkeyNonImpositionOfCustomsDutiesTest
			.SetupRatesAndTariff(Factory);
		Factory.Save();

		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_CountryOfOrigin = "ZA";

		const string expectedMessageError = "There is no applicable Duty rate for the Tariff '4016999190' and Country Of Dispatch 'TR' in combination with other data entered on the form.\r\n" +
			"Valid Duty rates exist for Preference = '400'";

		invoiceLine.JI_PrimaryPreference = "";
		invoiceLine.JI_Tariff = "4016999190";
		AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, expectedMessageError);

		invoiceLine.JI_PrimaryPreference = "400";
		invoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, expectedMessageError);

		invoiceLine.JI_PrimaryPreference = "400";
		invoiceLine.JI_Tariff = "9023453200";
		AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, expectedMessageError);
	}

	public void TestCheckJI_PrimaryPreference_WhenPreferenceIs400()
	{
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_CountryOfOrigin = "ZA";
		invoiceLine.JI_PrimaryPreference = "400";

		const string expectedMessageError = "Preference cannot be '400' if Goods Origin is not 'TR' and there is no Supporting Document with code 'N018'";

		AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageError);

		entryInstruction.SupportingDocuments.AddNew().CSI_Code = "N018";

		invoiceLine.Validation.ValidateJI_PrimaryPreference();
		AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageError);
	}

	void AssertTraderJobDocAddressValidationCaptions(JobDocAddress invoiceLineTrader, ZPropertyInfo propertyInfo, string expectedTraderName)
	{
		var expectedMessage = $"{expectedTraderName} City is longer than 35 characters, it will be truncated in the message.";

		var trader = Factory.New<OrgHeader>();
		var traderAddress = trader.Addresses.AddNew();
		traderAddress.City = "1234567890123456789012345678901234567890";
		invoiceLineTrader.OrganisationPK = trader.PK;
		invoiceLineTrader.E2_OA_Address = traderAddress.PK;
		invoiceLineTrader.Validation.ValidateAll();

		AssertHasWarningContaining(propertyInfo, expectedMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceLine invoiceLine;
}
