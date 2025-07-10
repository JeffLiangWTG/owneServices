using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class DeliveryTermsProviderTest : Customs.Business.Testing.DataProviderTestCase<DeliveryTermsProvider>
{
	public void TestIncotermCode()
	{
		invoiceHeader.JZ_IncoTerm = "CFR";
		AssertEquals("CFR", provider.IncotermCode);
	}

	public void TestUNLocode_IncotermplaceAsCountry()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "BE";
		AssertNullOrEmpty("Incoterm place is a country", provider.UNLocode);
	}

	public void TestUNLocode_IncotermplaceAsNotCountry()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "BEANR";
		AssertEquals("Incotermplace is not a country", "BEANR", provider.UNLocode);
	}

	public void TestLocation_IncotermplaceAsCountry()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "BE";
		invoiceHeader.JZ_IncoTermPlace = "Antwerpen";
		AssertEquals("Antwerpen", provider.Location);
	}

	public void TestLocation_IncotermplaceAsNotCountry()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "BEANR";
		invoiceHeader.JZ_IncoTermPlace = "Antwerpen";
		AssertNullOrEmpty(provider.Location);
	}

	public void TestCountry_IncotermplaceAsCountry()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "BE";
		AssertEquals("BE", provider.Country);
	}

	public void TestCountry_IncotermplaceAsNotCountry()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "BEANR";
		AssertNull(provider.Country);
	}

	public void TestText()
	{
		invoiceHeader.IncoTermsAgreedPlace = "A Place";
		AssertEquals("A Place", provider.Text);
	}

	protected override DeliveryTermsProvider GetProvider() => new DeliveryTermsProvider(invoiceHeader);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		provider = new DeliveryTermsProvider(invoiceHeader);
	}

	JobComInvoiceHeader invoiceHeader;
	JobDeclaration declaration;
	DeliveryTermsProvider provider;
}
