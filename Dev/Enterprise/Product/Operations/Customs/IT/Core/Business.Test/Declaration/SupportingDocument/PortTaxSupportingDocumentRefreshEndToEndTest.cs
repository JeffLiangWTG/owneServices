using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PortTaxSupportingDocumentRefreshEndToEndTest : TestCaseWithFactory
{
	public void TestRefreshOnMessageTypeChange()
	{
		declaration.JE_MessageType = "";
		declaration.JE_RL_NKPortOfLoading = "USLAX";
		invoiceLine1.ZG_PortTaxRate = "A4";
		invoiceLine2.ZG_PortTaxRate = "A4";

		CombineAssertions("PRE-CONDITIONS", () =>
		{
			AssertEquals($"{nameof(invoiceLine1)} Supporting Documents Count", 0, invoiceLine1.SupportingDocuments.Count);
			AssertEquals($"{nameof(invoiceLine2)} Supporting Documents Count", 0, invoiceLine2.SupportingDocuments.Count);
		});

		declaration.JE_MessageType = "EXP";

		CombineAssertions("POST-CONDITIONS", () =>
		{
			Assert39YYSupportingDocumentAdded(nameof(invoiceLine1), invoiceLine1.SupportingDocuments, "--USLAX");
			Assert39YYSupportingDocumentAdded(nameof(invoiceLine2), invoiceLine2.SupportingDocuments, "--USLAX");
		});
	}

	public void TestRefreshOnPortOfLoadingChange()
	{
		declaration.JE_MessageType = "EXP";
		invoiceLine1.ZG_PortTaxRate = "A4";
		invoiceLine2.ZG_PortTaxRate = "A4";

		CombineAssertions("PRE-CONDITIONS", () =>
		{
			AssertEquals($"{nameof(invoiceLine1)} Supporting Documents Count", 0, invoiceLine1.SupportingDocuments.Count);
			AssertEquals($"{nameof(invoiceLine2)} Supporting Documents Count", 0, invoiceLine2.SupportingDocuments.Count);
		});

		declaration.JE_RL_NKPortOfLoading = "USLAX";

		CombineAssertions("POST-CONDITIONS", () =>
		{
			Assert39YYSupportingDocumentAdded(nameof(invoiceLine1), invoiceLine1.SupportingDocuments, "--USLAX");
			Assert39YYSupportingDocumentAdded(nameof(invoiceLine2), invoiceLine2.SupportingDocuments, "--USLAX");
		});
	}

	public void TestRefreshOnPortOfArrivalChange()
	{
		declaration.JE_MessageType = "IMP";
		invoiceLine1.ZG_PortTaxRate = "A3";
		invoiceLine2.ZG_PortTaxRate = "A3";

		CombineAssertions("PRE-CONDITIONS", () =>
		{
			AssertEquals($"{nameof(invoiceLine1)} Supporting Documents Count", 0, invoiceLine1.SupportingDocuments.Count);
			AssertEquals($"{nameof(invoiceLine2)} Supporting Documents Count", 0, invoiceLine2.SupportingDocuments.Count);
		});

		declaration.JE_RL_NKPortOfArrival = "ITVCE";

		CombineAssertions("POST-CONDITIONS", () =>
		{
			Assert39YYSupportingDocumentAdded(nameof(invoiceLine1), invoiceLine1.SupportingDocuments, "--ITVCE");
			Assert39YYSupportingDocumentAdded(nameof(invoiceLine2), invoiceLine2.SupportingDocuments, "--ITVCE");
		});
	}

	public void TestRefreshOnPortTaxRateChange()
	{
		declaration.JE_MessageType = "IMP";
		declaration.JE_RL_NKPortOfArrival = "ITVCE";

		CombineAssertions("PRE-CONDITIONS", () =>
		{
			AssertEquals($"{nameof(invoiceLine1)} Supporting Documents Count", 0, invoiceLine1.SupportingDocuments.Count);
			AssertEquals($"{nameof(invoiceLine2)} Supporting Documents Count", 0, invoiceLine2.SupportingDocuments.Count);
		});

		invoiceLine1.ZG_PortTaxRate = "A3";

		CombineAssertions("POST-CONDITIONS", () =>
		{
			Assert39YYSupportingDocumentAdded(nameof(invoiceLine1), invoiceLine1.SupportingDocuments, "--ITVCE");
			AssertEquals($"{nameof(invoiceLine2)} Supporting Documents Count", 0, invoiceLine2.SupportingDocuments.Count);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = "SEA";
		invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;

	void Assert39YYSupportingDocumentAdded(string assertionMessagePrefix, SupportingDocumentCollection supportingDocuments, string expectedReferenceNumber)
	{
		AssertEquals(assertionMessagePrefix + " Supporting Documents Count", 1, supportingDocuments.Count);

		var portTaxSupportingDocument = supportingDocuments
			.Cast<SupportingDocument>()
			.SingleOrDefault(x => x.CSI_Code == "39YY" && x.CSI_ReferenceNumber == expectedReferenceNumber);
		AssertNotNull(assertionMessagePrefix + " Port Tax Supporting Document", portTaxSupportingDocument);
	}
}
