using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class ExportTermsOfDeliveryWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		AssertExceptionThrown<ArgumentNullException>("When entry and invoice are null", () => ExportTermsOfDeliveryWrapper.NewOrNull(null, null));
		AssertExceptionThrown<ArgumentNullException>("When entry not null and invoice null", () => ExportTermsOfDeliveryWrapper.NewOrNull(entryHeader, null));
		AssertExceptionThrown<ArgumentNullException>("When entry null and invoice not null", () => ExportTermsOfDeliveryWrapper.NewOrNull(null, invoiceHeader));
		AssertNull(nameof(TermsOfDeliveryWrapper.NewOrNull), ExportTermsOfDeliveryWrapper.NewOrNull(entryHeader, invoiceHeader));
	}

	public void TestNewOrNullWithSubStyle()
	{
		invoiceHeader.JZ_IncoTerm = "CIM";
		CombineAssertions(() =>
		{
			var termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
			AssertNotNull("When no SubStyle", termOfDeliveryWrapper);

			entryInstruction.CEI_SubStyle = "B";
			termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
			AssertNull("When SubStyle = B", termOfDeliveryWrapper);

			entryInstruction.CEI_SubStyle = "C";
			termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
			AssertNull("When SubStyle = C", termOfDeliveryWrapper);

			entryInstruction.CEI_SubStyle = "E";
			termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
			AssertNull("When SubStyle = E", termOfDeliveryWrapper);

			entryInstruction.CEI_SubStyle = "F";
			termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
			AssertNull("When SubStyle = F", termOfDeliveryWrapper);

			entryInstruction.CEI_SubStyle = "A";
			termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
			AssertNotNull("When SubStyle = A", termOfDeliveryWrapper);

			entryInstruction.CEI_SubStyle = "D";
			termOfDeliveryWrapper = GetNewTermOfDeliveryWrapper();
			AssertNotNull("When SubStyle = D", termOfDeliveryWrapper);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		invoiceHeader = declaration.Invoices.AddNew();
	}

	JobComInvoiceHeader invoiceHeader;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;

	ITermsOfDelivery GetNewTermOfDeliveryWrapper() => ExportTermsOfDeliveryWrapper.NewOrNull(entryHeader, invoiceHeader);
}
