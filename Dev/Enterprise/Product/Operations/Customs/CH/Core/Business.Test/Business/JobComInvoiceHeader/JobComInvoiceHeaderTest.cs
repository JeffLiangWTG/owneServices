using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public override void TestLocalCurrencyCodeCoreOverride()
	{
		AssertEquals("LocalCurrencyCode", Core.Constants.CurrencyCodes.Switzerland, Factory.New<JobComInvoiceHeader>().LocalCurrencyCode);
	}

	public void TestPreviousDocumentCollection() => CombineAssertions("PreviousDocumentCollection", () =>
	{
		var header = Factory.New<JobDeclaration>().Invoices.AddNew();

		AssertNotNull("JobComInvoiceHeader.PreviousDocuments must not be null", header.PreviousDocuments);
		AssertType<PreviousDocumentCollection>("Mismatching Type", header.PreviousDocuments);
		AssertEquals("Wrong count", 0, header.PreviousDocuments.Count);
		header.PreviousDocuments.AddNew();
		AssertEquals("Wrong count", 1, header.PreviousDocuments.Count);
	});

	public void TestTransportDocumentCollection() => CombineAssertions("PreviousDocumentCollection", () =>
	{
		InvoiceHeader.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertType<TransportDocument>(InvoiceHeader.TransportDocuments.AddNew());

		Factory.Save();
		AssertEquals("TransportDocument saved/loaded for EXP", 1,
			new BusinessObjectFactory().Load<JobComInvoiceHeader>(InvoiceHeader.PK).TransportDocuments.Count);

		InvoiceHeader.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Factory.Save();
		AssertEquals("TransportDocuments deleted for EDA", 0, InvoiceHeader.TransportDocuments.Count);

		AssertType<TransportDocument>(InvoiceHeader.TransportDocuments.AddNew());
		InvoiceHeader.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Factory.Save();
		AssertEquals("TransportDocuments deleted for IMP", 0, InvoiceHeader.TransportDocuments.Count);
	});

	public void TestSpecialMentions() => CombineAssertions(() =>
	{
		SpecialMentionsTestHelper.TestSpecialMentions(InvoiceHeader.SpecialMentionsInfo);
	});

	public void TestCountOfSpecialMentionsLines() => CombineAssertions(() =>
	{
		InvoiceHeader.SpecialMentions = "Line1\nLine2\n";
		AssertEquals(2, InvoiceHeader.CountOfSpecialMentionsLines);
		InvoiceHeader.SpecialMentions = "Line1\nLine2\nLine3\n";
		AssertEquals(3, InvoiceHeader.CountOfSpecialMentionsLines);
	});

	public void TestSupportingDocuments() => AssertType<SupportingDocumentCollection>(InvoiceHeader.SupportingDocuments);

	public void TestChargeType() => AssertType<InvoiceCharge>(InvoiceHeader.Charges.AddNew());

	public void TestIsGSPCertificateRequired() => CombineAssertions(() =>
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);
		ISupportingDocumentParent supportingDocumentParent = InvoiceHeader;

		AssertEquals("No lines", false, supportingDocumentParent.IsGSPCertificateRequired);

		var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
		invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		invoiceLine1.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;
		invoiceLine2.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;
		AssertEquals("No line requires GSP Certificate", false, supportingDocumentParent.IsGSPCertificateRequired);

		invoiceLine2.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
		AssertEquals("At least one line requires GSP Certificate", true, supportingDocumentParent.IsGSPCertificateRequired);
	});

	public void TestUCR() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(InvoiceHeader.JZ_UCRInfo, caption: "Ref. No./UCR");
		AssertEquals("MaxLength", 35, InvoiceHeader.JZ_UCRInfo.MaxLength);
	});

	public void TestIsOrdinaryInvoice() => CombineAssertions(() =>
	{
		var entryInstruction1 = InvoiceHeader.JobDeclaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = InvoiceHeader.JobDeclaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_Style = ZString.Empty;
		entryInstruction2.CEI_Style = ZString.Empty;
		var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		AssertEquals("CEI_Style is empty", false, invoiceHeader.HasOrdinaryInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("CEI_Style InvoiceLine 1 is not ordinary", true, invoiceHeader.HasOrdinaryInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("CEI_Style InvoiceLine 2 is not ordinary", true, invoiceHeader.HasOrdinaryInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("CEI_Style InvoiceLine 1 or 2 is not ordinary", true, invoiceHeader.HasOrdinaryInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("CEI_Style InvoiceLIne 1 or 2 is ordinary", false, invoiceHeader.HasOrdinaryInvoiceLines);
	});

	public void TestIsSimplifiedInvoice() => CombineAssertions(() =>
	{
		var entryInstruction1 = InvoiceHeader.JobDeclaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = InvoiceHeader.JobDeclaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_Style = ZString.Empty;
		entryInstruction2.CEI_Style = ZString.Empty;
		var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		AssertEquals("CEI_Style is empty", false, invoiceHeader.HasSimplifiedInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("CEI_Style InvoiceLine 2 is not simplified", true, invoiceHeader.HasSimplifiedInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("CEI_Style InvoiceLine 1 is not simplified", true, invoiceHeader.HasSimplifiedInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("CEI_Style InvoiceLine 1 or 2 is simplified", false, invoiceHeader.HasSimplifiedInvoiceLines);

		entryInstruction1.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		entryInstruction2.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("CEI_Style InvoiceLine 1 or 2 is not simplified", true, invoiceHeader.HasSimplifiedInvoiceLines);
	});

	public void TestTotalWeightInKG() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100;
		invoiceLine1.JI_WeightUQ = "KG";
		var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 100;
		invoiceLine2.JI_WeightUQ = "KG";

		AssertEquals("TotalWeightInKG", 200m, InvoiceHeader.TotalWeightInKG);

		invoiceLine1.JI_Weight = 150000;
		invoiceLine1.JI_WeightUQ = "G";

		invoiceLine2.JI_Weight = 0.15;
		invoiceLine2.JI_WeightUQ = "T";

		AssertEquals("TotalWeightInKG - invoice lines in T/G", 300m, InvoiceHeader.TotalWeightInKG);
	});

	public void TestTotalNetWeightInKG() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_NetWeight = 10;
		invoiceLine1.JI_NetWeightUQ = "KG";
		var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_NetWeight = 10;
		invoiceLine2.JI_NetWeightUQ = "KG";

		AssertEquals("TotalNetWeightInKG", 20m, InvoiceHeader.TotalNetWeightInKG);

		invoiceLine1.JI_NetWeight = 0.015;
		invoiceLine1.JI_NetWeightUQ = "T";
		invoiceLine2.JI_NetWeight = 15000;
		invoiceLine2.JI_NetWeightUQ = "G";

		AssertEquals("TotalNetWeightInKG - invoice lines in T/G", 30m, InvoiceHeader.TotalNetWeightInKG);
	});

	protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	new JobDeclaration declaration;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	new JobComInvoiceHeader invoiceHeader;
}
