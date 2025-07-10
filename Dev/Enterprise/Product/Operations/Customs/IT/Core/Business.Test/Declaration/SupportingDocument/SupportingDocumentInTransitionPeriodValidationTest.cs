using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SupportingDocumentInTransitionPeriodValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber_DeclarationLevel_WhenUCC6IsOnAndTransitionPeriodIsOn()
	{
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnAndTransitionPeriodIsOn(declarationSupportingDocument);
	}

	public void TestCheckCSI_ReferenceNumber_DeclarationLevel_WhenUCC6IsOnButTransitionPeriodIsOff()
	{
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnButTransitionPeriodIsOff(declarationSupportingDocument);
	}

	public void TestCheckCSI_ReferenceNumber_InvoiceHeaderLevel_WhenUCC6IsOnAndTransitionPeriodIsOn()
	{
		var invoiceHeaderSupportingDocument = invoiceHeader.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnAndTransitionPeriodIsOn(invoiceHeaderSupportingDocument);
	}

	public void TestCheckCSI_ReferenceNumber_InvoiceHeaderLevel_WhenUCC6IsOnButTransitionPeriodIsOff()
	{
		var invoiceHeaderSupportingDocument = invoiceHeader.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnButTransitionPeriodIsOff(invoiceHeaderSupportingDocument);
	}

	public void TestCheckCSI_ReferenceNumber_EntryInstructionLevel_WhenUCC6IsOnAndTransitionPeriodIsOn()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryInstructionSupportingDocument = entryInstruction.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnAndTransitionPeriodIsOn(entryInstructionSupportingDocument);
	}

	public void TestCheckCSI_ReferenceNumber_EntryInstructionLevel_WhenUCC6IsOnButTransitionPeriodIsOff()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryInstructionSupportingDocument = entryInstruction.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnButTransitionPeriodIsOff(entryInstructionSupportingDocument);
	}

	public void TestCheckCSI_ReferenceNumber_InvoiceLineLevel_WhenUCC6IsOnAndTransitionPeriodIsOn()
	{
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnAndTransitionPeriodIsOn(supportingDocument);
	}

	public void TestCheckCSI_ReferenceNumber_InvoiceLineLevel_WhenUCC6IsOnButTransitionPeriodIsOff()
	{
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		CheckSupportingDocumentWhenUCC6IsOnButTransitionPeriodIsOff(supportingDocument);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
	}

	JobComInvoiceLine invoiceLine;

	JobComInvoiceHeader invoiceHeader;

	JobDeclaration declaration;

	IDisposable TemporarilySetFunctionalitySetAESTransitionPeriod(bool isAESTransitionPeriod)
		=> Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, isAESTransitionPeriod);

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	void CheckSupportingDocumentWhenUCC6IsOnButTransitionPeriodIsOff(SupportingDocument supportingDocument)
	{
		const string expectedMessageError70 = "Field exceeds the maximum allowed length in the declaration message (70 characters).";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		using (TemporarilySetFunctionalitySetAESTransitionPeriod(false))
		{
			CombineAssertions("When Declaration UCC6 is ON, But AESTransitionPeriod is OFF", () =>
			{
				supportingDocument.CSI_ReferenceNumber = "A0001";
				AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError70);

				supportingDocument.CSI_ReferenceNumber = "LessThan70ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
				AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError70);

				supportingDocument.CSI_ReferenceNumber = "EqualTo70charsABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRST";
				AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError70);

				supportingDocument.CSI_ReferenceNumber = "MoreThan70charactersABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOP";
				AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError70);
			});
		}
	}

	void CheckSupportingDocumentWhenUCC6IsOnAndTransitionPeriodIsOn(SupportingDocument supportingDocument)
	{
		const string expectedMessageError35 = "Field exceeds the maximum allowed length in the declaration message (35 characters).";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		using (TemporarilySetFunctionalitySetAESTransitionPeriod(true))
		{
			CombineAssertions("When Declaration is UCC6 ON and AESTP is ON", () =>
			{
				supportingDocument.CSI_ReferenceNumber = "A0001";
				AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError35);

				supportingDocument.CSI_ReferenceNumber = "LessThan35ABCDEFGHIJKLMNOPQRSTUVWX";
				AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError35);

				supportingDocument.CSI_ReferenceNumber = "EqualTo35charsABCDEFGHIJKLMNOPQRSTU";
				AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError35);

				supportingDocument.CSI_ReferenceNumber = "MoreThan35charactersABCDEFGHIJKLMNOP";
				AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError35);
			});
		}
	}
}
