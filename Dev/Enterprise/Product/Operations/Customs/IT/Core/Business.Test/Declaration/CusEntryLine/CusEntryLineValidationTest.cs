using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using EUBusinessTesting = Enterprise.Customs.EU.Business.Declaration.Testing;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineValidationTest : EUBusinessTesting.CusEntryLineValidationTest
{
	public void TestInvoiceHeadersAreConsideredOnlyOnceInDocumentsCountValidateRule1407()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			entryLine1 = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine1.PK;

			using (TemporallySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When Declaration EXP UCC6 and Transition Period is ON", () =>
				{
					AddSupportingDocument(invoiceHeader1, 50);
					entryLine1.Validation.ValidateAll();
					AssertNoRowMessageError("Invoice Header appears in two invoice lines but it is not considered twice, so only 50 documents are counted and no error expected", entryLine1, expectedE1407MessageError);
				});
			}
		}
	}

	public void TestCheckSupportingAndAdditionalDocumentsCountValidateRule1407_Export()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			using (TemporallySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When Declaration EXP UCC6 and Transition Period is ON", () =>
				{
					AddSupportingDocument(invoiceLine1, 50);
					entryLine1.Validation.ValidateAll();
					AssertNoRowMessageError("Only 50 documents on invoice line, no error is expected", entryLine1, expectedE1407MessageError);
					AddSupportingDocument(invoiceLine2, 50);
					entryLine1.Validation.ValidateAll();
					AssertHasRowMessageError("100 documents on two invoice lines, error is expected", entryLine1, expectedE1407MessageError);
					entryLine2.Validation.ValidateAll();
					AssertNoRowMessageError("no documents at all in entry line 2, no error is expected", entryLine2, expectedE1407MessageError);

					invoiceLine2.SupportingDocuments.RemoveAndDeleteAll();
					AddSupportingDocument(invoiceHeader1, 50);
					entryLine1.Validation.ValidateAll();
					AssertHasRowMessageError("100 documents on invoice line and first header, error is expected", entryLine1, expectedE1407MessageError);

					invoiceHeader1.SupportingDocuments.RemoveAndDeleteAll();
					AddSupportingDocument(invoiceHeader2, 50);
					entryLine1.Validation.ValidateAll();
					AssertHasRowMessageError("100 documents on invoice line and second header, error is expected", entryLine1, expectedE1407MessageError);

					invoiceHeader2.SupportingDocuments.RemoveAndDeleteAll();
					AddAdditionalInfosToInvoiceLine(invoiceLine1, 50, "XXX");
					entryLine1.Validation.ValidateAll();
					AssertNoRowMessageError("50 documents + 50 additional infos not of type REF or TRA, no error is expected", entryLine1, expectedE1407MessageError);

					invoiceHeader2.SupportingDocuments.RemoveAndDeleteAll();
					AddAdditionalInfosToInvoiceLine(invoiceLine1, 50, "REF");
					entryLine1.Validation.ValidateAll();
					AssertHasRowMessageError("50 documents + 50 additional infos of  type REF, so error is expected", entryLine1, expectedE1407MessageError);
				});
			}

			using (TemporallySetTransitionPeriod(isActive: false))
			{
				CombineAssertions("When Declaration EXP UCC6 and Transition Period is OFF", () =>
				{
					RemoveAndDeleteAllDocumentsFromAllProviders();

					AddSupportingDocument(invoiceLine1, 50);
					AddSupportingDocument(invoiceLine2, 50);
					entryLine1.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period is OFF, even with exceeding number of documents, no error is expected", entryLine1, expectedE1407MessageError);
				});
			}
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			invoiceLine1.SupportingDocuments.RemoveAndDeleteAll();
			invoiceLine1.AdditionalInfos.RemoveAndDeleteAll();

			AddSupportingDocument(invoiceLine1, 50);
			AddAdditionalInfosToInvoiceLine(invoiceLine2, 50, "REF");

			entryLine1.Validation.ValidateAll();
			AssertNoRowMessageError("In non UCC6 Export declaration, even when cumulative number of supporting and additional documents is greater than 99, error is not expected", entryLine1, expectedE1407MessageError);
		}
	}

	public void TestCheckSupportingAndAdditionalDocumentsCountValidateRule1407_Import()
	{
		declaration.JE_MessageType = "IMP";

		using (TemporallySetTransitionPeriod(isActive: true))
		{
			RemoveAndDeleteAllDocumentsFromAllProviders();

			AddSupportingDocument(invoiceLine1, 50);
			AddSupportingDocument(invoiceLine2, 50);
			entryLine1.Validation.ValidateAll();
			AssertNoRowMessageError("In IMP declaration, when Transition Period is ON, even with exceeding number of documents, no error is expected", entryLine1, expectedE1407MessageError);
		}

		using (TemporallySetTransitionPeriod(isActive: false))
		{
			CombineAssertions("When Declaration EXP UCC6 and Transition Period is OFF", () =>
			{
				RemoveAndDeleteAllDocumentsFromAllProviders();

				AddSupportingDocument(invoiceLine1, 50);
				AddSupportingDocument(invoiceLine2, 50);
				entryLine1.Validation.ValidateAll();
				AssertNoRowMessageError("In IMP declaration, when Transition Period is OFF, even with exceeding number of documents, no error is expected", entryLine1, expectedE1407MessageError);
			});
		}
	}

	IDisposable TemporallySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	void AddSupportingDocument(ISupportingDocumentsProvider supportingDocumentsProvider, int numberOfDocuments)
	{
		for (var i = 0; i < numberOfDocuments; i++)
		{
			supportingDocumentsProvider.SupportingDocuments.AddNew();
		}
	}

	void AddAdditionalInfosToInvoiceLine(JobComInvoiceLine invoiceLine, int numberOfDocuments, string subType)
	{
		for (var i = 0; i < numberOfDocuments; i++)
		{
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = subType;
		}
	}

	void RemoveAndDeleteAllDocumentsFromAllProviders()
	{
		RemoveAndDeleteAllDocumentsFromInvoiceLineAndHeader(invoiceLine1);
		RemoveAndDeleteAllDocumentsFromInvoiceLineAndHeader(invoiceLine2);
		RemoveAndDeleteAllDocumentsFromInvoiceLineAndHeader(invoiceLine3);

		void RemoveAndDeleteAllDocumentsFromInvoiceLineAndHeader(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
			invoiceLine.AdditionalInfos.RemoveAndDeleteAll();
			invoiceLine.InvoiceHeader.SupportingDocuments.RemoveAndDeleteAll();
			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAndDeleteAll();
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();

		invoiceHeader2 = declaration.Invoices.AddNew();
		invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

		invoiceHeader3 = declaration.Invoices.AddNew();
		invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		invoiceLine3.JI_CEI = entryInstruction2.PK;

		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
		entryLine1 = entryHeader1.MergedLines.AddNew();

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
		entryLine2 = entryHeader2.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine1.PK;
		invoiceLine3.JI_CL = entryLine2.PK;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader1;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceHeader invoiceHeader2;
	JobComInvoiceLine invoiceLine2;
	JobComInvoiceHeader invoiceHeader3;
	JobComInvoiceLine invoiceLine3;

	CusEntryLine entryLine1;
	CusEntryLine entryLine2;

	const string expectedE1407MessageError = "[E1407] The maximum cumulative number of Supporting Documents, Transport Document and Additional Reference must not exceed 99.\r\nPlease check, in both Invoice lines and Invoice headers bound to this Entry line, the Codes indicated in Supporting Documents and Additional Documents (of Kind 'TRA' and 'REF')";
}
