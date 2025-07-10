using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

class JobComInvoiceLineValidationTest : TestCaseWithFactory
{
	#region TestJI_Tariff
	public void TestCheckJI_Tariff() => CombineAssertions(() =>
	{
		invoiceLine.JI_PartNo = "NEWPART";
		invoiceLine.JI_Tariff = "";
		invoiceLine.JI_CC = ZGuid.Empty;
		invoiceLine.Validation.ValidateJI_Tariff();
		AssertHasWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
		invoiceLine.JI_Tariff = "8008";
		invoiceLine.Validation.ValidateJI_Tariff();
		AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
		var classification = Factory.New<CusClassification>();
		classification.CC_LookupCode = "NEWCODE";
		classification.CC_IsActive = true;
		invoiceLine.JI_CC = classification.PK;
		invoiceLine.JI_Tariff = ZString.Empty;
		invoiceLine.Validation.ValidateJI_Tariff();
		AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
		invoiceLine.JI_Tariff = "91011100";
		invoiceLine.Declaration.JE_MessageType = MessageSubTypeList.Codes.Transshipment;
		invoiceLine.JI_Tariff = "";
		AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
		invoiceLine.JI_Tariff = "9101";
		AssertNoMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffMayNotBeEmpty);
		AssertHasMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffShouldBe8DigitsLong);
		invoiceLine.JI_Tariff = "91011100";
		invoiceLine.Declaration.JE_MessageType = MessageSubTypeList.Codes.Import;
		((JobDeclaration)invoiceLine.Declaration).JE_TypeOfGoods = TypeOfGoodsList.Codes.HighValueAboveDeminimis;
		invoiceLine.JI_Tariff = "";
		AssertHasMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffMayNotBeEmpty);
		AssertNoMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffShouldBe8DigitsLong);
		invoiceLine.JI_Tariff = "910111";
		AssertNoMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffMayNotBeEmpty);
		AssertHasMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffShouldBe8DigitsLong);
		invoiceLine.JI_Tariff = "91011100";
		AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
		invoiceLine.JI_Tariff = "91.0111.00";
		AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
		invoiceLine.JI_Tariff = "1.0111.00";
		AssertNoMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffMayNotBeEmpty);
		AssertHasMessageError(invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MessageErrorTariffShouldBe8DigitsLong);
	});

	#endregion
	#region TestJI_CountryOfOrigin

	public void TestJI_CountryOfOrigin() => CombineAssertions(() =>
	{
		invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "";
		invoiceLine.JI_CountryOfOrigin = "";
		AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		invoiceLine.JI_CountryOfOrigin = "AU";
		AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = countryOfOrigin.RN_Code;
		AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		invoiceLine.JI_CountryOfOrigin = "";
		AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
	});

	#endregion

	public void TestCheckJI_NewUsed()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_NewUsedInfo, "X", "N");
	}

	#region Setup
	protected override void SetUp()
	{
		base.SetUp();
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
		JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		countryOfOrigin = Factory.New<RefCountry>();
		countryOfOrigin.RN_Code = "AU";
	}

	protected RefCountry countryOfOrigin;
	protected JobComInvoiceLine invoiceLine;
	#endregion
}
