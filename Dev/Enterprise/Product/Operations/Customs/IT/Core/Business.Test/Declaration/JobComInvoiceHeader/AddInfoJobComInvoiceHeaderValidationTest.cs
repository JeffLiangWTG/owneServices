using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_AgreedPlaceCode()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD", intoWarehouse: true);
		helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IFD", intoWarehouse: false);
		Factory.Save();

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
		AssertHasMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceHeader.ZG_AgreedPlaceCode = "X";
		AssertNoMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

		jobDeclaration.JE_MessageType = "EXP";
		entryInstruction.CEI_Procedure = "22";
		invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
		AssertNoMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

		var message = "Invoices linked to the same Entry Instruction have different [20.3] Agreed Place Code";
		jobDeclaration.JE_MessageType = "IMP";
		var invoiceHeader2 = jobDeclaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		invoiceHeader2.ZG_AgreedPlaceCode = "2";
		AssertHasMessageErrorContaining(invoiceHeader2.ZG_AgreedPlaceCodeInfo, message);
		invoiceHeader.ZG_AgreedPlaceCode = "1";
		AssertHasMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, message);
		invoiceHeader2.ZG_AgreedPlaceCode = "1";
		AssertNoMessageErrorContaining(invoiceHeader2.ZG_AgreedPlaceCodeInfo, message);
	}

	public void TestCheckZG_AgreedPlaceCode_ListValidation()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		invoiceHeader.ZG_AgreedPlaceCode = "X";
		AssertHasMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);

		invoiceHeader.ZG_AgreedPlaceCode = "ITVNC";
		AssertNoMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);

		jobDeclaration.JE_MessageType = "EXP";
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.ZG_AgreedPlaceCode = "X";
			AssertHasMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader.ZG_AgreedPlaceCode = "ITVNC";
			AssertNoMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}

	public void TestCheckZG_AgreedPlaceCodeEntryFreightCost()
	{
		var messageError = "Invoice Freight Charges are required for the used INCO Term Place Code. Add needed charges to the invoice or the invoice line";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MergeBy = "TRF";
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		invoice.InvoiceLines.AddNew();
		invoice.InvoiceLines.AddNew();

		invoice.ZG_AgreedPlaceCode = "2";
		invoice.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(invoice, messageError);

		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_FreightAdjustment = ZDecimal.Zero;

		invoice.ZG_AgreedPlaceCode = "";
		entryHeader.CH_FreightAdjustment = 0;
		invoice.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice.ZG_AgreedPlaceCodeInfo, messageError);

		invoice.ZG_AgreedPlaceCode = "ES";
		invoice.Validation.ValidateAll();
		AssertHasMessageErrorContaining(invoice.ZG_AgreedPlaceCodeInfo, messageError);

		invoice.ZG_AgreedPlaceCode = "IT";
		invoice.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice.ZG_AgreedPlaceCodeInfo, messageError);

		entryHeader.CH_FreightAdjustment = 1.5;
		invoice.ZG_AgreedPlaceCode = "ES";
		invoice.Validation.ValidateAll();
		AssertNoMessageErrorContaining(invoice.ZG_AgreedPlaceCodeInfo, messageError);
	}

	public void TestCheckZG_AgreedPlaceCodeMandatoryValidationForExportUcc6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "";
			AssertHasMessageErrorContaining("When 'INCO Term' is not XXX and 'incoterm Place code' is empty", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.ZG_AgreedPlaceCode = "123";
			AssertNoMessageErrorContaining("When 'INCO Term' is not XXX and 'incoterm Place code' is not empty", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = "XXX";
			invoice.ZG_AgreedPlaceCode = "";
			AssertHasMessageErrorContaining("When 'INCO Term' is XXX and 'incoterm Place code' is empty", invoice.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestCheckZG_AgreedPlaceCodeMandatoryValidationForExportNonUcc6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "";
			AssertNoMessageErrors(invoice.ZG_AgreedPlaceCodeInfo);
		}
	}

	public void TestCheckZG_AgreedPlaceCodeMandatoryValidationForNonExportNonUcc6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "XXX";
		var invoice = declaration.Invoices.AddNew();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "";
			AssertNoMessageErrors(invoice.ZG_AgreedPlaceCodeInfo);
		}
	}

	public void TestCheckZG_AgreedPlaceCodeCannotBeIt()
	{
		var expectedMessageError = "For the chosen INCO Term, INCO Term Place Code cannot be IT.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;

		CombineAssertions(() =>
		{
			invoiceHeader.ZG_AgreedPlaceCode = "ITGOA";
			AssertHasMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, expectedMessageError);

			invoiceHeader.ZG_AgreedPlaceCode = "ITXXX";
			AssertHasMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, expectedMessageError);

			invoiceHeader.ZG_AgreedPlaceCode = "ESBCN";
			AssertNoMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, expectedMessageError);
		});

		invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
		invoiceHeader.ZG_AgreedPlaceCode = "ITGOA";
		AssertNoMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, expectedMessageError);

		declaration.JE_MessageType = "EXP";
		invoiceHeader.ZG_AgreedPlaceCode = "ITXXX";
		AssertNoMessageErrorContaining(invoiceHeader.ZG_AgreedPlaceCodeInfo, expectedMessageError);
	}
}
