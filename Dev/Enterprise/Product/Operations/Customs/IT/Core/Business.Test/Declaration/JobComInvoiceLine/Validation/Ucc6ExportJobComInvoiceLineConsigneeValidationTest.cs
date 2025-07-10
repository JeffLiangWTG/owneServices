using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportJobComInvoiceLineConsigneeValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception", () => new Ucc6JobComInvoiceLineConsigneeValidation(null));
	}

	public void TestConsigneeValidation_WhenIsEmptyBothLineAndHeaderLevel()
	{
		const string expectedMessageError = "Consignee must be declared at header or line level.";

		var consignee = Factory.New<OrgHeader>();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine1 = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		var invoiceLine3 = invoice2.InvoiceLines.AddNew();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		invoiceLine3.JI_CEI = entryInstruction2.PK;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				invoiceLine1.JI_OA_ConsigneeAddress = ZGuid.Empty;
				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertHasMessageErrorContaining("When both Header and Line Consignees are empty", invoiceLine1.JI_OA_ConsigneeAddressInfo, expectedMessageError);

				invoiceLine2.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("When at least one Line Consignee is filled", invoiceLine1.JI_OA_ConsigneeAddressInfo, expectedMessageError);

				invoiceLine2.JI_OA_ConsigneeAddress = ZGuid.Empty;
				invoiceLine3.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertHasMessageErrorContaining("When both Header and Line Consignees related to the same instruction are empty", invoiceLine1.JI_OA_ConsigneeAddressInfo, expectedMessageError);

				invoiceLine2.JI_OA_ConsigneeAddress = ZGuid.Empty;
				invoiceLine1.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				AssertNoMessageErrorContaining("When Line Consignee is filled", invoiceLine1.JI_OA_ConsigneeAddressInfo, expectedMessageError);

				declaration.ImporterDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
				invoiceLine1.JI_OA_ConsigneeAddress = ZGuid.Empty;
				AssertNoMessageErrorContaining("When Header Consignee is filled", invoiceLine1.JI_OA_ConsigneeAddressInfo, expectedMessageError);

				var orphanInvoiceLine = Factory.New<JobComInvoiceLine>();
				orphanInvoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("When Line is orphan", invoiceLine1.JI_OA_ConsigneeAddressInfo, expectedMessageError);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine1.JI_OA_ConsigneeAddress = ZGuid.Empty;
			declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageErrorContaining("When both Header and Line Consignee are empty but is not UCC6", invoiceLine1.JI_OA_ConsigneeAddressInfo, expectedMessageError);
		}
	}

	public void TestConsigneeValidation_WhenIsEmptyButAtLeastInvoiceLineInTheSameInstructionHasConsigneePopulated()
	{
		var consignee = Factory.New<OrgHeader>();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine1 = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		var invoiceLine3 = invoice2.InvoiceLines.AddNew();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		invoiceLine3.JI_CEI = entryInstruction2.PK;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("When all Lines Consignees are empty", invoiceLine1.JI_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine3.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("When all entry instruction related consignees are empty", invoiceLine1.JI_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine2.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertHasMessageErrorContaining("When at least consignee is filled", invoiceLine1.JI_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ImporterDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
				invoiceLine1.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("When at least consignee is filled and Header consignee is filled too", invoiceLine1.JI_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				invoiceLine1.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				AssertNoMessageErrorContaining("When consignee is filled", invoiceLine1.JI_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);

				var orphanInvoiceLine = Factory.New<JobComInvoiceLine>();
				orphanInvoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
				AssertNoMessageErrorContaining("When Line is orphan", invoiceLine1.JI_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine1.JI_OA_ConsigneeAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining("When at least consignee is filled but is not UCC6", invoiceLine1.JI_OA_ConsigneeAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
	}

	JobDeclaration declaration;

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);
}
