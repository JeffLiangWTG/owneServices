using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AgreedPlaceCode_UCC6()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoice = jobDeclaration.Invoices.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoice.ZG_AgreedPlaceCodeInfo);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			AssertNoMessageErrorContaining("No mandatory field validation when JZ_IncoTerm is 'XXX'", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_IncoTermPlace = "SYD";
			AssertNoMessageErrorContaining("No mandatory field validation when JZ_IncoTermPlace is not empty", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTermPlace = ZString.Empty;
			invoice.ZG_IncotermCountry = "LV";
			AssertNoMessageErrorContaining("No mandatory field validation when ZG_IncotermCountry is not empty", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.ZG_IncotermCountry = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoice.ZG_AgreedPlaceCodeInfo);
		}

		public void TestCheckZG_AgreedPlaceCode_NotUCC6()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var invoice = jobDeclaration.Invoices.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoice.ZG_AgreedPlaceCodeInfo);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			AssertHasMessageErrorContaining("Mandatory field validation when JZ_IncoTerm is 'XXX'", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertNoMessageErrorContaining("No mandatory field error when JZ_IncoTerm is not 'XXX'", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckZG_ValuationMethod()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				invoice.ZG_ValuationMethod = ZString.Empty;
				AssertNoMessageErrorContaining("UCC6, Import, Sub Style in C/F, ZG_ValuationMethod is not mandatory", invoice.ZG_ValuationMethodInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoice.ZG_ValuationMethodInfo, "X", "1");

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoice.ZG_ValuationMethodInfo, "X", "1");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoice.ZG_ValuationMethodInfo, "X", "1");

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoice.ZG_ValuationMethodInfo, "X", "1");

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoice.ZG_ValuationMethodInfo, "X", "1");
			}
		}
	}
}
