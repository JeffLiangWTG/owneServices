using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class FinanceDataProviderTest : TestCaseWithFactory
{
	public void TestNew() => CombineAssertions(() =>
		{
			AssertNull("InvoiceHeader==null", FinanceDataProvider.New(null));
			AssertNotNull("InvoiceHeader != null", FinanceDataProvider.New(Invoice));
		});

	public void TestProvider() => CombineAssertions(() =>
	{
		Invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		var financeDataProvider = FinanceDataProvider.New(Invoice);

		AssertEquals("FreeOnBoard FOB", Core.Constants.IncoTerms.FreeOnBoard, financeDataProvider.Incoterms);
		AssertNull("VatNumber null", financeDataProvider.VatNumber);
		AssertNull("CustomsInvoiceRecipient null", financeDataProvider.CustomsInvoiceRecipient);
	});

	public void TestVatNumber() => CombineAssertions(() =>
	{
		const string customsVatNo = "CHE-123.456.789";
		const string customsBidNo = "123456789";

		var financeDataProvider = FinanceDataProvider.New(Invoice);

		AssertNull("VatNumber null", financeDataProvider.VatNumber);

		Invoice.JobDeclaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertNull("VatNumber null", financeDataProvider.VatNumber);

		Invoice.JobDeclaration.Supplier.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, customsBidNo);
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertNull("BID entered", financeDataProvider.VatNumber);

		Invoice.JobDeclaration.Supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, ZString.Empty);
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertNull("VatNumber empty", financeDataProvider.VatNumber);

		Invoice.JobDeclaration.Supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, customsVatNo, Core.Constants.CountryCodes.Switzerland);
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertEquals("VatNumber", customsVatNo, financeDataProvider.VatNumber);
	});

	public void TestIncoterms() => CombineAssertions(() =>
	{
		var financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertEquals("incoterm empty", string.Empty, financeDataProvider.Incoterms);

		Invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertEquals("FreeOnBoard FOB", Core.Constants.IncoTerms.FreeOnBoard, financeDataProvider.Incoterms);

		Invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeCarrierSeller;
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertEquals("FreeOnBoard FC1 -> FCA", Core.Constants.IncoTerms.FreeCarrier, financeDataProvider.Incoterms);

		Invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeCarrierBuyer;
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertEquals("FreeOnBoard FC2 -> FCA", Core.Constants.IncoTerms.FreeCarrier, financeDataProvider.Incoterms);
	});

	public void TestCustomsInvoiceRecipient() => CombineAssertions(() =>
	{
		var orgSupplier = Factory.New<OrgHeader>();
		Invoice.JobDeclaration.JE_OH_Supplier = orgSupplier.PK;

		orgSupplier.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "BID001", Core.Constants.CountryCodes.Andorra);
		var financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertNull("No CH BID", financeDataProvider.CustomsInvoiceRecipient);

		orgSupplier.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "BID002", Core.Constants.CountryCodes.Switzerland);
		financeDataProvider = FinanceDataProvider.New(Invoice);
		AssertEquals("CH BID", "BID002", financeDataProvider.CustomsInvoiceRecipient);
	});

	public void TestCustomsInvoiceReferenceNumber()
	{
		AssertNull("CustomsInvoiceReferenceNumber not available", FinanceDataProvider.New(Invoice).CustomsInvoiceReferenceNumber);
	}

	public void TestImmediatePaymentRequest()
	{
		Assert("ImmediatePaymentRequest is false", !FinanceDataProvider.New(Invoice).ImmediatePaymentRequest);
	}

	public void TestVatInvoiceRecipient()
	{
		AssertNull("VatInvoiceRecipient not available", FinanceDataProvider.New(Invoice).VatInvoiceRecipient);
	}

	public void TestVatInvoiceReferenceNumber()
	{
		AssertNull("VatInvoiceReferenceNumber not available", FinanceDataProvider.New(Invoice).VatInvoiceReferenceNumber);
	}

	JobComInvoiceHeader CreateInvoice()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		return declaration.Invoices.AddNew();
	}

	JobComInvoiceHeader Invoice => invoice ?? (invoice = CreateInvoice());
	JobComInvoiceHeader invoice;
}
