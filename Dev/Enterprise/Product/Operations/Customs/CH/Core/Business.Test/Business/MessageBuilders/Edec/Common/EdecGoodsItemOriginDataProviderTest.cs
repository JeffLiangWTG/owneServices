using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecGoodsItemOriginDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null", EdecGoodsItemOriginDataProvider.New(null));
			AssertNotNull("Not Null", EdecGoodsItemOriginDataProvider.New(entryLine));
		});
	}

	public void TestOriginCountry()
	{
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Switzerland;

		var messageBuilder = EdecGoodsItemOriginDataProvider.New(entryLine);
		AssertEquals(nameof(messageBuilder.OriginCountry), Core.Constants.CountryCodes.Switzerland, messageBuilder.OriginCountry);
	}

	public void TestPreference()
	{
		var messageBuilder = EdecGoodsItemOriginDataProvider.New(entryLine);
		AssertEquals("When empty, preference should be: ", false, messageBuilder.Preference);

		invoiceLine.JI_PrimaryPreference = "XX";
		messageBuilder = EdecGoodsItemOriginDataProvider.New(entryLine);
		AssertEquals("When JI_PrimaryPreference != (NT or PR), preference should be: ", false, messageBuilder.Preference);

		invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
		messageBuilder = EdecGoodsItemOriginDataProvider.New(entryLine);
		AssertEquals("When JI_PrimaryPreference = NT, preference should be: ", false, messageBuilder.Preference);

		invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
		messageBuilder = EdecGoodsItemOriginDataProvider.New(entryLine);
		AssertEquals("When JI_PrimaryPreference = PR), preference should be: ", true, messageBuilder.Preference);
	}

	public void TestPreferenceConfirmation()
	{
		RefCusCodeTestHelper.CreateOriginDocumentCodes(Factory);

		var messageBuilder = EdecGoodsItemOriginDataProvider.New(entryLine);

		CombineAssertions(() =>
		{
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			AssertEquals("Preferential Tariff", false, messageBuilder.PreferenceConfirmation);

			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			AssertEquals("Normal Tariff, but no document", false, messageBuilder.PreferenceConfirmation);

			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			supportingDocument.CSI_Code = RefCusCodeTestHelper.NoOriginDocument;
			AssertEquals("Normal Tariff, but no origin document", false, messageBuilder.PreferenceConfirmation);

			supportingDocument.CSI_Code = RefCusCodeTestHelper.OriginDocument;
			AssertEquals("Normal Tariff, but with origin document", true, messageBuilder.PreferenceConfirmation);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
}
