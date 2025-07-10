using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceHeaderValidationTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderValidationTest
{
	public void TestSupplierCountryCodeNotDifferentFromDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_RL_NKOrigin = "IT";
		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.OH_RL_NKClosestPort = "ITTAR";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;

		CombineAssertions("Country codes in Declaration and Suppliers", () =>
		{
			AssertNoWarning(invoiceHeader1.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);

			orgHeader1.OH_RL_NKClosestPort = "AUSYD";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_RL_NKClosestPort = "AUSYD";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_OH_Supplier = orgHeader2.PK;
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			invoiceHeader1.Validation.ValidateJZ_OH_Supplier();
			AssertHasWarning(invoiceHeader1.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);
			AssertHasWarning(invoiceHeader2.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);

			orgHeader1.OH_RL_NKClosestPort = "ITTAR";
			orgHeader2.OH_RL_NKClosestPort = "ITTAR";

			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_RL_NKClosestPort = "ITTAR";
			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_OH_Supplier = orgHeader3.PK;
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;

			invoiceHeader1.Validation.ValidateJZ_OH_Supplier();
			invoiceHeader2.Validation.ValidateJZ_OH_Supplier();
			AssertNoWarning(invoiceHeader1.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);
			AssertNoWarning(invoiceHeader2.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);
			AssertNoWarning(invoiceHeader3.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);
		});
	}

	public void TestCheckJZ_IncoTermPlace()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD", intoWarehouse: true);
		helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IFD", intoWarehouse: false);
		var unloco = Factory.New<RefUNLOCO>();
		unloco.RL_Code = "UNLOC";
		Factory.Save();

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		invoiceHeader.JZ_IncoTermPlace = "PLACE";
		AssertNoMessageErrorContaining(invoiceHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
		AssertHasMessageErrorContaining(invoiceHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		invoiceHeader.ZG_AgreedPlaceCode = "UNLOC";
		invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
		AssertNoMessageErrorContaining(invoiceHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

		jobDeclaration.JE_MessageType = "EXP";
		invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
		AssertNoMessageErrorContaining(invoiceHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

		var message = "Invoices linked to the same Entry Instruction have different [20.2] Agreed Place";
		jobDeclaration.JE_MessageType = "IMP";
		var invoiceHeader2 = jobDeclaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		invoiceHeader2.JZ_IncoTermPlace = "PLACE2";
		AssertHasMessageErrorContaining(invoiceHeader2.JZ_IncoTermPlaceInfo, message);
		invoiceHeader.JZ_IncoTermPlace = "PLACE1";
		AssertHasMessageErrorContaining(invoiceHeader.JZ_IncoTermPlaceInfo, message);
		invoiceHeader2.JZ_IncoTermPlace = "PLACE1";
		AssertNoMessageErrorContaining(invoiceHeader2.JZ_IncoTermPlaceInfo, message);

		invoiceHeader.JZ_IncoTermPlace = "PLACE1";
		invoiceHeader2.ZG_AgreedPlaceCode = "ITMIL";
		invoiceHeader2.JZ_IncoTermPlace = "PLACE2";
		AssertNoMessageErrorContaining(invoiceHeader2.JZ_IncoTermPlaceInfo, message);
	}

	public void TestCheckJZ_ValuationCode_MandatoryValidation()
	{
		new ITUniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure40And71ForCurrentCountry();

		var declaration = Factory.New<JobDeclaration>();
		var entryInstructionWarehouse = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionWarehouse.CEI_Procedure = "71";
		var entryInstructionNonWarehouse = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionNonWarehouse.CEI_Procedure = "40";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstructionWarehouse.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstructionWarehouse.PK;
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstructionWarehouse.PK;

		invoice.JZ_ValuationCode = "";
		AssertNoMessageErrorContaining("All lines are linked to warehouse procedure entry instructions", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine1.JI_CEI = entryInstructionNonWarehouse.PK;
		invoice.JZ_ValuationCode = "";
		AssertHasMessageErrorContaining("NOT all lines are linked to warehouse procedure entry instructions", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoice.JZ_ValuationCode = "11";
		AssertNoMessageErrorContaining("NOT all lines are linked to warehouse procedure entry instructions, but value is entered", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine1.JI_CEI = ZGuid.Empty;
		invoiceLine2.JI_CEI = ZGuid.Empty;
		invoiceLine3.JI_CEI = ZGuid.Empty;
		invoice.JZ_ValuationCode = "";
		AssertNoMessageErrorContaining("There are no lines linked to any entry instruction", invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJZ_IncoTerm()
	{
		var expectedMessageError = "This code is not valid for Italian Customs.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();

		invoice.JZ_IncoTerm = "";
		AssertNoMessageErrorContaining("Incoterm is not set, no error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "CFR";
		AssertNoMessageErrorContaining("The incoterm entered is valid, no error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "FCA";
		AssertNoMessageErrorContaining("The incoterm entered is valid, no error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "FC1";
		AssertHasMessageErrorContaining("The incoterm entered is invalid for Italian customs, error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "FC2";
		AssertHasMessageErrorContaining("The incoterm entered is invalid for Italian customs, error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		declaration.JE_MessageType = "EXP";

		invoice.JZ_IncoTerm = "";
		AssertNoMessageErrorContaining("Incoterm is not set, no error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "CFR";
		AssertNoMessageErrorContaining("The incoterm entered is valid, no error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "FCA";
		AssertNoMessageErrorContaining("The incoterm entered is valid, no error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "FC1";
		AssertHasMessageErrorContaining("The incoterm entered is invalid for Italian customs, error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);

		invoice.JZ_IncoTerm = "FC2";
		AssertHasMessageErrorContaining("The incoterm entered is invalid for Italian customs, error message expected", invoice.JZ_IncoTermInfo, expectedMessageError);
	}

	public void TestCheckJZ_IncoTermPlaceMandatoryValidationBasedOnMessageTypeAndIncoTerm()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();

		CombineAssertions("JE_MessageType = 'EXP'", () =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			invoice.JZ_IncoTerm = "";
			invoice.JZ_IncoTermPlace = "";
			AssertNoMessageErrorContaining("Both fields empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_IncoTermPlace = "";
			AssertHasMessageErrorContaining("JZ_IncoTermPlace empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_IncoTermPlace = "1";
			AssertNoMessageErrorContaining("Both fields filled", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("JE_MessageType NOT IN 'EXP'", () =>
		{
			declaration.JE_MessageType = "XXX";
			invoice.JZ_IncoTerm = "";
			invoice.JZ_IncoTermPlace = "";
			AssertNoMessageErrorContaining("Both fields empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_IncoTermPlace = "";
			AssertNoMessageErrorContaining("JZ_IncoTermPlace empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_IncoTermPlace = "1";
			AssertNoMessageErrorContaining("Both fields filled", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJZ_IncoTermPlaceMandatoryValidationForExportUcc6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "";
			invoice.JZ_IncoTermPlace = "";
			AssertNoMessageErrorContaining("When 'INCO Term' is not XXX and 'incoterm Place code' is empty and ‘[20.2] Place’ is empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.ZG_AgreedPlaceCode = "IT";
			invoice.JZ_IncoTermPlace = "";
			AssertHasMessageErrorContaining("When 'INCO Term' is not XXX and 'incoterm Place code' Length is 2 and ‘[20.2] Place’ is empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTermPlace = "123";
			AssertNoMessageErrorContaining("When 'INCO Term' is not XXX and 'incoterm Place code' Length is 2 and ‘[20.2] Place’ is not empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTerm = "XXX";
			invoice.JZ_IncoTermPlace = "";
			AssertHasMessageErrorContaining("When 'INCO Term' is XXX and 'incoterm Place code' Length is 2 and ‘[20.2] Place’ is empty", invoice.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestCheckJZ_RX_NKInvoice_CurrencyMandatoryForImportDeclaration()
	{
		const string expectedMessageError = "Please enter a Currency";
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();

		CombineAssertions("JE_MessageType = 'IMP'", () =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			AssertNoMessageErrorContaining("InvoiceAmount = 100, InvoiceCurrency = 'EUR' ", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);

			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertHasMessageErrorContaining("InvoiceAmount = 100, InvoiceCurrency = ''", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);

			invoice.JZ_InvoiceAmount = ZDecimal.Zero;
			AssertHasMessageErrorContaining("InvoiceAmount = 0, InvoiceCurrency = ''", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);

			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			AssertNoMessageErrorContaining("InvoiceAmount = 0, InvoiceCurrency = 'EUR''", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);
		});

		CombineAssertions("JE_MessageType = 'EXP'", () =>
		{
			declaration.JE_MessageType = "XXX";
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			AssertNoMessageError("InvoiceAmount = 100, InvoiceCurrency = 'EUR' ", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);

			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertHasMessageError("InvoiceAmount = 100, InvoiceCurrency = ''", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);

			invoice.JZ_InvoiceAmount = ZDecimal.Zero;
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertNoMessageError("InvoiceAmount = 0, InvoiceCurrency = ''", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);

			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			AssertNoMessageError("InvoiceAmount = 0, InvoiceCurrency = 'EUR''", invoice.JZ_RX_NKInvoice_CurrencyInfo, expectedMessageError);
		});
	}

	public void TestCheckJZ_AdditionalTermsEntryInstructionHasConsistentDeliveryTerms()
	{
		const string messageError = "Invoices linked to the same Entry Instruction have different Delivery Terms";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoiceHeader1.JZ_IncoTerm = "XXX";
			invoiceHeader2.JZ_IncoTerm = "XXX";
			invoiceHeader1.JZ_AdditionalTerms = "ABC";
			invoiceHeader2.JZ_AdditionalTerms = "DEF";
			invoiceHeader1.Validation.ValidateJZ_AdditionalTerms();
			invoiceHeader2.Validation.ValidateJZ_AdditionalTerms();
			AssertHasMessageErrorContaining(invoiceHeader1.JZ_AdditionalTermsInfo, messageError);
			AssertHasMessageErrorContaining(invoiceHeader2.JZ_AdditionalTermsInfo, messageError);

			invoiceHeader1.JZ_AdditionalTerms = "ABC";
			invoiceHeader2.JZ_AdditionalTerms = "ABC";
			invoiceHeader1.Validation.ValidateJZ_AdditionalTerms();
			invoiceHeader2.Validation.ValidateJZ_AdditionalTerms();
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_AdditionalTermsInfo, messageError);
			AssertNoMessageErrorContaining(invoiceHeader2.JZ_AdditionalTermsInfo, messageError);
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			invoiceHeader1.JZ_IncoTerm = "XXX";
			invoiceHeader2.JZ_IncoTerm = "XXX";
			invoiceHeader1.JZ_AdditionalTerms = "123";
			invoiceHeader2.JZ_AdditionalTerms = "456";
			invoiceHeader1.Validation.ValidateJZ_AdditionalTerms();
			invoiceHeader2.Validation.ValidateJZ_AdditionalTerms();
			AssertNoMessageErrors(invoiceHeader1.JZ_AdditionalTermsInfo);
			AssertNoMessageErrors(invoiceHeader2.JZ_AdditionalTermsInfo);
		}
	}

	public void TestCheckJZ_AdditionalTermsMandatoryValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceHeader = declaration.Invoices.AddNew();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoiceHeader.JZ_IncoTerm = "XXX";
			invoiceHeader.JZ_AdditionalTerms = "";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_AdditionalTermsInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_AdditionalTerms = "ABC";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_AdditionalTermsInfo, MandatoryValidation.YouHaveNotEntered);
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			invoiceHeader.JZ_IncoTerm = "XXX";
			invoiceHeader.JZ_AdditionalTerms = "";
			AssertNoMessageErrors(invoiceHeader.JZ_AdditionalTermsInfo);
		}
	}

	public void TestCheckJZ_ValuationCode_MustHaveSameTransaction_DuringTransitionPeriod()
	{
		const string expectedMessageError = "[E1301] During the transition period, which is active now, all invoices linked to the same entry must have the same [24] Transaction Nature.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceHeader3 = declaration.Invoices.AddNew();

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		invoiceHeader1.InvoiceLines.AddNew().JI_CEI = entryInstruction1.PK;
		invoiceHeader2.InvoiceLines.AddNew().JI_CEI = entryInstruction1.PK;
		invoiceHeader3.InvoiceLines.AddNew().JI_CEI = entryInstruction2.PK;

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			using (TemporarilySetFunctionalitySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When Declaration EXP UCC6 and Transition Period is ON", () =>
				{
					invoiceHeader1.JZ_ValuationCode = "11";
					invoiceHeader2.JZ_ValuationCode = "11";
					AssertNoMessageErrorContaining(invoiceHeader2.JZ_ValuationCodeInfo, expectedMessageError);

					invoiceHeader2.JZ_ValuationCode = "12";
					AssertHasMessageErrorContaining(invoiceHeader2.JZ_ValuationCodeInfo, expectedMessageError);

					invoiceHeader3.JZ_ValuationCode = "13";
					AssertNoMessageErrorContaining(invoiceHeader3.JZ_ValuationCodeInfo, expectedMessageError);
				});
			}

			using (TemporarilySetFunctionalitySetTransitionPeriod(isActive: false))
			{
				invoiceHeader2.JZ_ValuationCode = "12";
				AssertNoMessageErrorContaining("When is EXP UCC6 but not in Transition Period", invoiceHeader2.JZ_ValuationCodeInfo, expectedMessageError);
			}
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			invoiceHeader2.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining("When is not UCC6 and not in Transition Period", invoiceHeader2.JZ_ValuationCodeInfo, expectedMessageError);
		}
	}

	public void TestShouldCheckMissingPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceHeader = declaration.Invoices.AddNew();
		var validation = new JobComInvoiceHeaderValidationForTest(invoiceHeader);

		CombineAssertions("ShouldCheckMissingPreviousDocuments", () =>
		{
			AssertEquals("For Import", true, validation.ShouldCheckMissingPreviousDocumentsExposed);

			declaration.JE_MessageType = "EXP";
			AssertEquals("For Non Ucc6 Export", true, validation.ShouldCheckMissingPreviousDocumentsExposed);

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("For Ucc6 Export", false, validation.ShouldCheckMissingPreviousDocumentsExposed);
			}
		});
	}

	public void TestJZ_IncoTermPlace_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			CombineAssertions("When 'INCO Term' is XXX and 'incoterm Place code' length is 5", () =>
			{
				invoice.JZ_IncoTermPlace = "Place1";
				invoice.ZG_AgreedPlaceCode = "ITVCE";
				AssertEquals(true, invoice.JZ_IncoTermPlaceInfo.ReadOnly);
				AssertEquals(ZString.Empty, invoice.JZ_IncoTermPlace);
			});
			CombineAssertions("When 'INCO Term' is XXX and 'incoterm Place code' length is 2", () =>
			{
				invoice.ZG_AgreedPlaceCode = "";
				invoice.JZ_IncoTermPlace = "Place1";
				invoice.ZG_AgreedPlaceCode = "IT";
				AssertEquals(false, invoice.JZ_IncoTermPlaceInfo.ReadOnly);
				AssertEquals("Place1", invoice.JZ_IncoTermPlace);
			});
		}
	}

	public void TestCheckShouldValidateNeedAtLeastOneInvoiceSupportingDocumentFlag()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var jobComInvHeaderValidator = new JobComInvoiceHeaderValidationForTest(invoiceHeader);
		AssertEquals("ShouldValidateNeedAtLeastOneInvoiceSupportingDocument flag should be true", true, jobComInvHeaderValidator.ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentExposed);
	}

	public void TestCheckJZ_OA_SupplierAddress_SupplierAndAddressMismatch_IsNotPrompted()
	{
		invoiceHeader.SupplierOrgPK = ZGuid.NewZGuid();
		invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
		invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
		AssertNoNotifications(
			"'Supplier organization will not be saved because no address is selected' should not be prompted as Supplier is actually persisted in JZ_OH_Supplier",
			invoiceHeader.JZ_OA_SupplierAddressInfo);
	}

	IDisposable TemporarilySetFunctionalitySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

	class JobComInvoiceHeaderValidationForTest : JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidationForTest(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		public ZBool ShouldCheckMissingPreviousDocumentsExposed => ShouldCheckMissingPreviousDocuments;

		public bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentExposed => ShouldValidateNeedAtLeastOneInvoiceSupportingDocument;
	}
}
