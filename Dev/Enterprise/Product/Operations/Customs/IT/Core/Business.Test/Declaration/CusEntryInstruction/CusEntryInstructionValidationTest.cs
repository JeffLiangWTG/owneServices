using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstructionValidation))]
sealed class CusEntryInstructionValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
{
	public void TestCheckCEI_SubStyle_ValidateRuleR0677_ExportUCC6()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryInstructionValidation = entryInstruction.Validation;
		var expectedErrorMessage = "[R0677] A 'SDE' code must be present in Entry instruction > authorizations > type";
		var cclAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		entryInstruction.CEI_SubStyle = "C";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";

			CombineAssertions(() =>
			{
				entryInstructionValidation.ValidateCEI_SubStyle();
				AssertHasMessageErrorContaining("When no Authorization has been provided, error message is expected", entryInstruction.CEI_SubStyleInfo, expectedErrorMessage);

				cclAuthorizationUsage.AGC_Code = "ARG";
				entryInstructionValidation.ValidateCEI_SubStyle();
				AssertHasMessageErrorContaining("When no Authorization of type SDE has been provided, error message is expected", entryInstruction.CEI_SubStyleInfo, expectedErrorMessage);

				cclAuthorizationUsage.AGC_Code = "SDE";
				entryInstructionValidation.ValidateCEI_SubStyle();
				AssertNoMessageErrorContaining("When Authorization of type SDE has been provided, no error message is expected", entryInstruction.CEI_SubStyleInfo, expectedErrorMessage);

				entryInstruction.CEI_SubStyle = "X";
				cclAuthorizationUsage.AGC_Code = "QQQ";
				entryInstructionValidation.ValidateCEI_SubStyle();
				AssertNoMessageErrorContaining("If SubStyle is not C or F, no error message is expected whatever the value of AGC_Code", entryInstruction.CEI_SubStyleInfo, expectedErrorMessage);
			});
		}
	}

	public void TestCheckCEI_SubStyle_ValidateRuleB1905()
	{
		const string expectedMessage = "[B1905] For Sub Style X or Y a Previous Document must be filled in Entry Instructions.";
		declaration.JE_MessageType = "EXP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When Ucc6 and AESTP ON", () =>
				{
					AssertEquals("[Pre]: Previous documents in Entry instruction", 0, entryInstruction.PreviousDocuments.Count);
					entryInstruction.CEI_SubStyle = "A";
					AssertNoMessageError("For SubStyle = A and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);

					entryInstruction.CEI_SubStyle = "D";
					AssertNoMessageError("For SubStyle = D and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);

					entryInstruction.CEI_SubStyle = "X";
					AssertHasMessageError("For SubStyle = X and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);

					entryInstruction.CEI_SubStyle = "Y";
					AssertHasMessageError("For SubStyle = Y and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);

					entryInstruction.CEI_SubStyle = "Z";
					AssertNoMessageError("For SubStyle = Z and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);

					entryInstruction.PreviousDocuments.AddNew();
					entryInstruction.CEI_SubStyle = "X";
					AssertNoMessageError("For SubStyle = X and Previous document added", entryInstruction.CEI_SubStyleInfo, expectedMessage);

					entryInstruction.CEI_SubStyle = "Y";
					AssertNoMessageError("For SubStyle = Y and Previous document added", entryInstruction.CEI_SubStyleInfo, expectedMessage);
				});
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				CombineAssertions("When Ucc6 and AESTP OFF", () =>
				{
					entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
					entryInstruction.CEI_SubStyle = "X";
					AssertNoMessageError("For SubStyle = X and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);

					entryInstruction.CEI_SubStyle = "Y";
					AssertNoMessageError("For SubStyle = Y and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);
				});
			}
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			CombineAssertions("For Non Ucc6", () =>
			{
				entryInstruction.CEI_SubStyle = "X";
				AssertNoMessageError("For SubStyle = X and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);

				entryInstruction.CEI_SubStyle = "Y";
				AssertNoMessageError("For SubStyle = Y and no Previous documents", entryInstruction.CEI_SubStyleInfo, expectedMessage);
			});
		}
	}

	public void TestAllRelatedInvoicesMustHaveSameTransactionNature()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var validation = new JCusEntryInstructionValidationForTest(entryInstruction);

		declaration.JE_MessageType = "EXP";
		AssertEquals("For non UCC6 declaration AllRelatedInvoicesMustHaveSameTransactionNature is true", true, validation.AllRelatedInvoicesMustHaveSameTransactionNatureExposed);

		declaration.JE_MessageType = "IMP";
		AssertEquals("For IMP UCC6 declaration AllRelatedInvoicesMustHaveSameTransactionNature is true", true, validation.AllRelatedInvoicesMustHaveSameTransactionNatureExposed);

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			AssertEquals("For EXP UCC6 declaration AllRelatedInvoicesMustHaveSameTransactionNature is false", false, validation.AllRelatedInvoicesMustHaveSameTransactionNatureExposed);
		}
	}

	public void TestCheckCEI_Style_DeclarationTypeMustBeTheSame_WhenIsNotUcc6Declaration()
	{
		const string expectedMessage = "The Declaration Type must be the same for every Entry Instruction";

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			entryInstruction1.CEI_Style = "B";
			entryInstruction2.CEI_Style = "B";
			entryInstruction1.Validation.ValidateCEI_Style();
			entryInstruction2.Validation.ValidateCEI_Style();
			AssertNoErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);
			AssertNoErrorContaining(entryInstruction2.CEI_StyleInfo, expectedMessage);

			entryInstruction1.CEI_Style = "A";
			entryInstruction2.CEI_Style = "B";
			entryInstruction1.Validation.ValidateCEI_Style();
			entryInstruction2.Validation.ValidateCEI_Style();
			AssertHasErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);
			AssertHasErrorContaining(entryInstruction2.CEI_StyleInfo, expectedMessage);
		}
	}

	public void TestCheckCEI_Style_DeclarationTypeMustBeTheSame_WhenIsUcc6Declaration()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "11", "11", "111", "One", Common.EU.EUJobMessageTypeList.Codes.Export, group: "A,B");

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			entryInstruction1.CEI_Style = "A";
			entryInstruction2.CEI_Style = "B";
			entryInstruction1.Validation.ValidateCEI_Style();
			entryInstruction2.Validation.ValidateCEI_Style();
			AssertNoErrors(entryInstruction1.CEI_StyleInfo);
			AssertNoErrors(entryInstruction2.CEI_StyleInfo);
		}
	}

	public void TestCheckCEI_Style_FallbackDeclarationTypeCanOnlyBeSubmittedInPaperForm()
	{
		const string expectedMessage = "Declaration type DSE cannot be submitted to Customs with a file interchange, but only in paper form";

		entryInstruction1.CEI_Style = "";
		AssertNoWarningContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);

		entryInstruction1.CEI_Style = "COL";
		AssertNoWarningContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);

		entryInstruction1.CEI_Style = "DSE";
		AssertHasWarningContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);
	}

	public void TestCheckCEI_Style_PreliminaryDeclarationMustHaveStyleCod()
	{
		const string expectedMessage = "A declaration with Sub-Style = D must have Type = COD";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("For EXP not UCC6", () =>
			{
				AssertEquals("Message Type", "EXP", declaration.JE_MessageType);
				entryInstruction1.CEI_SubStyle = "D";
				entryInstruction1.CEI_Style = "COL";
				AssertHasMessageErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);

				entryInstruction1.CEI_Style = "COD";
				AssertNoMessageErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);

				entryInstruction1.CEI_SubStyle = "";
				entryInstruction1.CEI_Style = "COL";
				AssertNoMessageErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			entryInstruction1.CEI_SubStyle = "D";
			entryInstruction1.CEI_Style = "COL";
			AssertNoMessageErrorContaining("For EXP UCC6", entryInstruction1.CEI_StyleInfo, expectedMessage);
		}

		CombineAssertions("For IMP", () =>
		{
			declaration.JE_MessageType = "IMP";
			entryInstruction1.CEI_SubStyle = "D";
			entryInstruction1.CEI_Style = "H1";
			AssertNoMessageErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);
		});
	}

	public void TestCheckCEI_Style_MandatoryValidation()
	{
		const string expectedMessage = "Please enter a Declaration Type";

		entryInstruction1.CEI_Style = "";
		entryInstruction1.Validation.ValidateCEI_Style();
		AssertHasErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);

		entryInstruction1.CEI_Style = "A";
		AssertNoErrorContaining(entryInstruction1.CEI_StyleInfo, expectedMessage);
	}

	public void TestCheckCEI_StyleListValidation()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "11", "11", "111", "One", Common.EU.EUJobMessageTypeList.Codes.Import, group: "H1,H2,H3,H4,H5,I1,I2");

		declaration.JE_MessageType = "IMP";

		CombineAssertions(() =>
		{
			entryInstruction1.CEI_Style = "XX";
			AssertHasErrorContaining(entryInstruction1.CEI_StyleInfo, ListValidation.InvalidCodeError);

			entryInstruction1.CEI_Style = "H1";
			AssertNoErrorContaining(entryInstruction1.CEI_StyleInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckCEI_DateForDuty()
	{
		entryInstruction1.CEI_DateForDuty = ZDate.Empty;
		AssertHasMessageErrorContaining(entryInstruction1.CEI_DateForDutyInfo, MandatoryValidation.YouHaveNotEntered);

		var messageErrorExpected = FormattableString.Invariant($"The Acceptance Date should be {ZDate.Today.ToShortDateString()}");
		entryInstruction1.CEI_DateForDuty = ZDate.Today.AddDays(-1);
		AssertHasMessageErrorContaining(entryInstruction1.CEI_DateForDutyInfo, messageErrorExpected);

		entryInstruction1.CEI_DateForDuty = ZDate.Today;
		AssertNoMessageErrorContaining(entryInstruction1.CEI_DateForDutyInfo, messageErrorExpected);

		var acceptanceDateMustBeEmptyMessageError = "For Sub-Style = D the Acceptance Date must be empty";

		entryInstruction1.CEI_SubStyle = "D";
		AssertEquals("[PRE-CONDITION] IsPreliminaryDeclarationUnderCodeA", true, entryInstruction1.IsPreliminaryDeclarationUnderCodeA);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("For EXP not UCC6", () =>
			{
				entryInstruction1.CEI_DateForDuty = ZDate.Today;
				AssertHasMessageErrorContaining(entryInstruction1.CEI_DateForDutyInfo, acceptanceDateMustBeEmptyMessageError);

				entryInstruction1.CEI_DateForDuty = ZDate.Empty;
				AssertNoMessageErrorContaining(entryInstruction1.CEI_DateForDutyInfo, acceptanceDateMustBeEmptyMessageError);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			entryInstruction1.CEI_DateForDuty = ZDate.Today;
			AssertNoMessageErrorContaining("For EXP UCC6", entryInstruction1.CEI_DateForDutyInfo, acceptanceDateMustBeEmptyMessageError);

			entryInstruction1.CEI_DateForDuty = ZDate.Empty;
			AssertHasMessageErrorContaining(entryInstruction1.CEI_DateForDutyInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestCheckIncoterm()
	{
		SetUpRefData();

		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
		entryInstruction1.Validation.ValidateAll();
		const string NoIncotermFound = "No Incoterm [20] found in Invoices linked to this Entry Instruction";
		const string DifferentIncoTermsForEntryInstructionInvoices = "Invoices linked to this Entry Instruction have different Incoterms [20]";

		AssertHasMessageError(entryInstruction1.IncotermInfo, NoIncotermFound);
		AssertNoMessageError(entryInstruction1.IncotermInfo, DifferentIncoTermsForEntryInstructionInvoices);

		invoice1.JZ_IncoTerm = "XXX";
		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.IncotermInfo, NoIncotermFound);
		AssertNoMessageError(entryInstruction1.IncotermInfo, DifferentIncoTermsForEntryInstructionInvoices);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_IncoTerm = "XXX";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.IncotermInfo, NoIncotermFound);
		AssertNoMessageError(entryInstruction1.IncotermInfo, DifferentIncoTermsForEntryInstructionInvoices);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_IncoTerm = "";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.IncotermInfo, NoIncotermFound);
		AssertHasMessageError(entryInstruction1.IncotermInfo, DifferentIncoTermsForEntryInstructionInvoices);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_IncoTerm = "ZZZ";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.IncotermInfo, NoIncotermFound);
		AssertHasMessageError(entryInstruction1.IncotermInfo, DifferentIncoTermsForEntryInstructionInvoices);

		entryInstruction1.CEI_Procedure = "71";
		invoice2.JZ_IncoTerm = "ZZZ";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.IncotermInfo, NoIncotermFound);
		AssertNoMessageError(entryInstruction1.IncotermInfo, DifferentIncoTermsForEntryInstructionInvoices);
	}

	public void TestCheckValuationCode()
	{
		SetUpRefData();

		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
		entryInstruction1.Validation.ValidateAll();
		const string NoTransactionNatureFound = "No Transaction Nature [24] found in Invoices linked to this Entry Instruction";
		const string AllInvoicesMustHaveSameTransactionNature = "All Invoices on an Entry Instruction must have the same '[24] Tran. Nature'.";

		AssertHasMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllInvoicesMustHaveSameTransactionNature);

		invoice1.JZ_ValuationCode = "XX";
		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllInvoicesMustHaveSameTransactionNature);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_ValuationCode = "XX";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllInvoicesMustHaveSameTransactionNature);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_ValuationCode = "";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
		AssertHasMessageError(entryInstruction1.ValuationCodeInfo, AllInvoicesMustHaveSameTransactionNature);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_ValuationCode = "ZZ";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
		AssertHasMessageError(entryInstruction1.ValuationCodeInfo, AllInvoicesMustHaveSameTransactionNature);

		entryInstruction1.CEI_Procedure = "71";
		invoice2.JZ_ValuationCode = "ZZ";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
		AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllInvoicesMustHaveSameTransactionNature);
	}

	public void TestCheckValuationCode_UCC6()
	{
		SetUpRefData();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
			declaration.JE_MessageType = "EXP";
			entryInstruction1.Validation.ValidateAll();
			const string NoTransactionNatureFound = "No Transaction Nature [24] found in Invoices linked to this Entry Instruction";
			const string AllEntryInstructionInvoicesMustHaveSameTransactionNature = "All Invoices on an Entry Instruction must have the same '[24] Tran. Nature'.";

			AssertHasMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);
			declaration.JE_MessageType = "IMP";
			entryInstruction1.Validation.ValidateAll();
			AssertHasMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);

			declaration.JE_MessageType = "EXP";
			invoice1.JZ_ValuationCode = "XX";
			invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
			AssertNoMessageErrorForMultipleNatureOfTransaction();

			declaration.JE_MessageType = "EXP";
			invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
			invoice2.JZ_ValuationCode = "XX";
			AssertNoMessageErrorForMultipleNatureOfTransaction();

			declaration.JE_MessageType = "EXP";
			invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
			invoice2.JZ_ValuationCode = "";
			entryInstruction1.Validation.ValidateAll();
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);
			declaration.JE_MessageType = "IMP";
			entryInstruction1.Validation.ValidateAll();
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
			AssertHasMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);

			declaration.JE_MessageType = "EXP";
			invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
			invoice2.JZ_ValuationCode = "ZZ";
			entryInstruction1.Validation.ValidateAll();
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);
			declaration.JE_MessageType = "IMP";
			entryInstruction1.Validation.ValidateAll();
			AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
			AssertHasMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);

			declaration.JE_MessageType = "EXP";
			entryInstruction1.CEI_Procedure = "71";
			invoice2.JZ_ValuationCode = "ZZ";
			AssertNoMessageErrorForMultipleNatureOfTransaction();

			void AssertNoMessageErrorForMultipleNatureOfTransaction()
			{
				invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
				entryInstruction1.Validation.ValidateAll();
				AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
				AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);
				declaration.JE_MessageType = "IMP";
				entryInstruction1.Validation.ValidateAll();
				AssertNoMessageError(entryInstruction1.ValuationCodeInfo, NoTransactionNatureFound);
				AssertNoMessageError(entryInstruction1.ValuationCodeInfo, AllEntryInstructionInvoicesMustHaveSameTransactionNature);
			}
		}
	}

	public void TestCheckCurrency()
	{
		SetUpRefData();

		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
		entryInstruction1.Validation.ValidateAll();
		const string NoCurrencyInEntryInstructionInvoices = "No Currency [22] found in Invoices linked to this Entry Instruction";
		const string EntryInstructionInvoicesHaveDifferentCurrencies = "Invoices linked to this Entry Instruction have different Currency [22]";

		AssertHasMessageError(entryInstruction1.CurrencyInfo, NoCurrencyInEntryInstructionInvoices);
		AssertNoMessageError(entryInstruction1.CurrencyInfo, EntryInstructionInvoicesHaveDifferentCurrencies);

		invoice1.JZ_RX_NKInvoice_Currency = "XX";
		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.CurrencyInfo, NoCurrencyInEntryInstructionInvoices);
		AssertNoMessageError(entryInstruction1.CurrencyInfo, EntryInstructionInvoicesHaveDifferentCurrencies);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "XX";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.CurrencyInfo, NoCurrencyInEntryInstructionInvoices);
		AssertNoMessageError(entryInstruction1.CurrencyInfo, EntryInstructionInvoicesHaveDifferentCurrencies);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.CurrencyInfo, NoCurrencyInEntryInstructionInvoices);
		AssertHasMessageError(entryInstruction1.CurrencyInfo, EntryInstructionInvoicesHaveDifferentCurrencies);

		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "ZZ";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.CurrencyInfo, NoCurrencyInEntryInstructionInvoices);
		AssertHasMessageError(entryInstruction1.CurrencyInfo, EntryInstructionInvoicesHaveDifferentCurrencies);

		entryInstruction1.CEI_Procedure = "71";
		invoice2.JZ_RX_NKInvoice_Currency = "ZZ";
		entryInstruction1.Validation.ValidateAll();
		AssertNoMessageError(entryInstruction1.CurrencyInfo, NoCurrencyInEntryInstructionInvoices);
		AssertNoMessageError(entryInstruction1.CurrencyInfo, EntryInstructionInvoicesHaveDifferentCurrencies);
	}

	public void TestCheckCEI_OA_Warehouse()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure("IT", "IM", "40", "", "", "Procedure1", "IMP", outOfWarehouse: false);
		helper.CreateRefCusProcedure("IT", "IM", "71", "", "", "Procedure1", "IMP", outOfWarehouse: true);

		var warehouseOrgHeader2 = Factory.New<OrgHeader>();
		warehouseOrgHeader2.OH_Code = "ORG2";
		var warehouseWithValidCCP = Factory.New<OrgAddress>();
		warehouseWithValidCCP.Address1 = "2 Street";
		warehouseWithValidCCP.OA_OH = warehouseOrgHeader2.PK;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLineIntoWarehouseN = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineIntoWarehouseN.JI_Procedure = "40";
		invoiceLineIntoWarehouseN.JI_CEI = ZGuid.Empty;

		var invoiceLineIntoWarehouseY = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineIntoWarehouseY.JI_Procedure = "71";
		invoiceLineIntoWarehouseY.JI_CEI = ZGuid.Empty;

		Factory.Save();

		string requiredCustomsWarehouseMessage = "At least one invoice line uses a CPC that requires that [49] From Warehouse is set. Please select a value";

		entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
		AssertNoMessageErrorContaining(entryInstruction.CEI_OA_WarehouseInfo, requiredCustomsWarehouseMessage);

		invoiceLineIntoWarehouseN.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
		AssertNoMessageErrorContaining(entryInstruction.CEI_OA_WarehouseInfo, requiredCustomsWarehouseMessage);

		invoiceLineIntoWarehouseY.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
		AssertHasMessageErrorContaining(entryInstruction.CEI_OA_WarehouseInfo, requiredCustomsWarehouseMessage);

		entryInstruction.CEI_OA_Warehouse = warehouseWithValidCCP.PK;
		AssertNoMessageErrorContaining(entryInstruction.CEI_OA_WarehouseInfo, requiredCustomsWarehouseMessage);
	}

	public void TestCheckCEI_OA_Warehouse2()
	{
		SetUpRefData();

		var warehouseOrgHeader2 = Factory.New<OrgHeader>();
		warehouseOrgHeader2.OH_Code = "ORG2";
		var warehouseWithValidCCP = Factory.New<OrgAddress>();
		warehouseWithValidCCP.Address1 = "2 Street";
		warehouseWithValidCCP.OA_OH = warehouseOrgHeader2.PK;
		var warehouseCustomsCode = warehouseOrgHeader2.CustomsCodes.AddNew();
		warehouseCustomsCode.OK_CodeType = "CCP";
		warehouseCustomsCode.OK_RN_NKCodeCountry = "IT";
		warehouseCustomsCode.OK_CustomsRegNo = "A123456ZB";
		warehouseCustomsCode.OK_OA_PremisesAddress = warehouseWithValidCCP.PK;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLineIntoWarehouseN = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineIntoWarehouseN.JI_Procedure = "40";
		invoiceLineIntoWarehouseN.JI_CEI = ZGuid.Empty;
		var invoiceLineIntoWarehouseY = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineIntoWarehouseY.JI_Procedure = "71";
		invoiceLineIntoWarehouseY.JI_CEI = ZGuid.Empty;

		Factory.Save();

		var requiredCustomsWarehouseMessage = "At least one invoice line uses a CPC that requires that [49] To Warehouse is set. Please select a value";
		entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertNoMessageErrorContaining(entryInstruction.CEI_OA_Warehouse2Info, requiredCustomsWarehouseMessage);
		invoiceLineIntoWarehouseN.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertNoMessageErrorContaining(entryInstruction.CEI_OA_Warehouse2Info, requiredCustomsWarehouseMessage);
		invoiceLineIntoWarehouseY.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertHasMessageErrorContaining(entryInstruction.CEI_OA_Warehouse2Info, requiredCustomsWarehouseMessage);
		entryInstruction.CEI_OA_Warehouse2 = warehouseWithValidCCP.PK;
		AssertNoMessageErrorContaining(entryInstruction.CEI_OA_Warehouse2Info, requiredCustomsWarehouseMessage);
		declaration.JE_MessageType = "EXP";
		entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertNoMessageErrorContaining(entryInstruction.CEI_OA_Warehouse2Info, requiredCustomsWarehouseMessage);
	}

	public void TestCheckCEI_OA_Warehouse2_NoError_WithOnlyIntoVATWarehouseProcedure()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var warehouse = CreateNewWarehouse();
		var cusProcedure1 = helper.CreateRefCusProcedure("IT", "IM", "45", "", "", "Procedure1", "IMP");
		var cusProcedure2 = helper.CreateRefCusProcedure("IT", "IM", "45", "", "", "Procedure2", "IMP");
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		entryInstruction1.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
		invoiceLineInvoice1.JI_Procedure = "45";
		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			cusProcedure1.ZZ6_IntoWarehouse = "N";
			cusProcedure1.ZZ6_IntoVATWarehouse = "Y";
			cusProcedure2.ZZ6_IntoWarehouse = "N";
			cusProcedure2.ZZ6_IntoVATWarehouse = "N";

			entryInstruction1.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("No Invoice Line have ZZ6_IntoWarehouse 'Y' and atleast one has ZZ6_IntoVATWarehouse 'Y'", entryInstruction1.CEI_OA_Warehouse2Info, ValidationCaptions.EntryInstruction.NoAuthorisationConfiguredMessage);

			cusProcedure1.ZZ6_IntoVATWarehouse = "N";
			entryInstruction1.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError("No Invoice Line have ZZ6_IntoWarehouse 'Y' and No Invoice Line has ZZ6_IntoVATWarehouse 'Y'", entryInstruction1.CEI_OA_Warehouse2Info, ValidationCaptions.EntryInstruction.NoAuthorisationConfiguredMessage);

			cusProcedure1.ZZ6_IntoWarehouse = "Y";
			cusProcedure2.ZZ6_IntoVATWarehouse = "Y";
			entryInstruction1.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError("One Invoice Line have ZZ6_IntoWarehouse 'Y' and atleast one has ZZ6_IntoVATWarehouse 'Y'", entryInstruction1.CEI_OA_Warehouse2Info, ValidationCaptions.EntryInstruction.NoAuthorisationConfiguredMessage);
		}
	}

	public void TestCheckCEI_OA_Warehouse2_ToWarehouseShouldNotBeEmptyIfHasInvoiceLinesThatRequireIt_IntoVATWarehouseProcedure()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var cusProcedure = helper.CreateRefCusProcedure("IT", "IM", "72", "", "", "Procedure1", "IMP");
		cusProcedure.ZZ6_IntoVATWarehouse = "Y";

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			entryInstruction1.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("No IntoVATWarehouse Procedure", entryInstruction1.CEI_OA_Warehouse2Info, ValidationCaptions.EntryInstruction.ToWarehouseShouldNotBeEmptyIfHasInvoiceLinesThatRequireIt);

			invoiceLineInvoice1.JI_Procedure = "72";
			invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;
			AssertEquals("Precondition", true, entryInstruction1.HasIntoVATWarehouseProcedure);
			entryInstruction1.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError("Has IntoVATWarehouse Procedure", entryInstruction1.CEI_OA_Warehouse2Info, ValidationCaptions.EntryInstruction.ToWarehouseShouldNotBeEmptyIfHasInvoiceLinesThatRequireIt);
		});
	}

	public void TestCheckCEI_OA_Warehouse2_ToWarehouseShouldBeVATFiscalWarehouse()
	{
		var universalReferenceHelper = new UniversalReferenceTestDataHelper(Factory);
		var cusProcedure = universalReferenceHelper.CreateRefCusProcedure("IT", "IM", "10", "", "", "Procedure1", "IMP");
		cusProcedure.ZZ6_IntoVATWarehouse = "N";

		var cusProcedureVAT = universalReferenceHelper.CreateRefCusProcedure("IT", "IM", "20", "", "", "Procedure1", "IMP");
		cusProcedureVAT.ZZ6_IntoVATWarehouse = "Y";

		var whsHelper = new WhsDataTestHelper(Factory);
		declaration.JE_OH_Importer = whsHelper.Importer.PK;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		invoiceLineInvoice2.JI_CEI = entryInstruction1.PK;

		entryInstruction1.CEI_OA_Warehouse2 = ZGuid.Empty;

		CombineAssertions("To Warehouse is empty", () =>
		{
			Assert("Procedure ZZ6_IntoVATWarehouse is N", "10", false);
			Assert("Procedure ZZ6_IntoVATWarehouse is Y", "20", false);
		});

		entryInstruction1.CEI_OA_Warehouse2 = whsHelper.WhsWarehouse.WW_OA_WarehouseAddress;

		CombineAssertions("To Warehouse isn't VAT Fiscal", () =>
		{
			Assert("Procedure ZZ6_IntoVATWarehouse is N", "10", false);
			Assert("Procedure ZZ6_IntoVATWarehouse is Y", "20", true);
		});

		var area = (IWhsArea)whsHelper.WhsWarehouse.Areas.AddNew();
		area.WA_AreaType = "VAT";

		CombineAssertions("To Warehouse is VAT Fiscal", () =>
		{
			Assert("Procedure ZZ6_IntoVATWarehouse is N", "10", false);
			Assert("Procedure ZZ6_IntoVATWarehouse is Y", "20", false);
		});

		void Assert(string message, string procedureCode, bool warningExpected)
		{
			invoiceLineInvoice2.JI_Procedure = procedureCode;
			entryInstruction1.Validation.ValidateCEI_OA_Warehouse2();

			const string warning = "There is no Warehouse of type VAT Fiscal for the selected Organization and Address";

			if (warningExpected)
			{
				AssertHasWarning(message, entryInstruction1.CEI_OA_Warehouse2Info, warning);
			}
			else
			{
				AssertNoWarning(message, entryInstruction1.CEI_OA_Warehouse2Info, warning);
			}
		}
	}

	public void TestCEI_OA_WarehouseRequiredForUcc6Export()
	{
		const string expectedErrorMessage = "At least one invoice line uses a CPC that requires that [49] From Warehouse is set. Please select a value";

		SetUpRefData();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLineWithOutOfWarehouseY = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineWithOutOfWarehouseY.JI_Procedure = "44";
		invoiceLineWithOutOfWarehouseY.JI_CEI = ZGuid.Empty;

		var invoiceLineWithOutOfWarehouseN = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineWithOutOfWarehouseN.JI_Procedure = "61";
		invoiceLineWithOutOfWarehouseN.JI_CEI = ZGuid.Empty;

		var warehouse = CreateNewWarehouse();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageErrorContaining("No warehouse and no linking between Entry Instruction and Invoice Line", entryInstruction.CEI_OA_WarehouseInfo, expectedErrorMessage);

			invoiceLineWithOutOfWarehouseY.JI_CEI = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertHasMessageErrorContaining("Invoice With CPC having OutOfWarehouse=Y and empty warehouse value", entryInstruction.CEI_OA_WarehouseInfo, expectedErrorMessage);

			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageErrorContaining("Invoice With CPC having OutOfWarehouse=Y and Non empty warehouse value", entryInstruction.CEI_OA_WarehouseInfo, expectedErrorMessage);

			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			invoiceLineWithOutOfWarehouseY.JI_CEI = ZGuid.Empty;
			invoiceLineWithOutOfWarehouseN.JI_CEI = entryInstruction.PK;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageErrorContaining("Invoice With CPC having OutOfWarehouse=N and an empty warehouse value", entryInstruction.CEI_OA_WarehouseInfo, expectedErrorMessage);
		}
	}

	public void TestCEI_OA_Warehouse2RequiredForUcc6Export()
	{
		const string expectedErrorMessage = "At least one invoice line uses a CPC that requires that [49] To Warehouse is set. Please select a value";

		SetUpRefData();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLineWithIntoWarehouseY = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineWithIntoWarehouseY.JI_Procedure = "63";
		invoiceLineWithIntoWarehouseY.JI_CEI = ZGuid.Empty;

		var invoiceLineWithIntoWarehouseN = invoiceHeader.InvoiceLines.AddNew();
		invoiceLineWithIntoWarehouseN.JI_Procedure = "01";
		invoiceLineWithIntoWarehouseN.JI_CEI = ZGuid.Empty;

		var warehouse = CreateNewWarehouse();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageErrorContaining("No warehouse and no linking between Entry Instruction and Invoice Line", entryInstruction.CEI_OA_Warehouse2Info, expectedErrorMessage);

			invoiceLineWithIntoWarehouseY.JI_CEI = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageErrorContaining("Invoice With CPC having IntoWarehouse=Y and empty warehouse value", entryInstruction.CEI_OA_Warehouse2Info, expectedErrorMessage);

			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageErrorContaining("Invoice With CPC having IntoWarehouse=Y and Non empty warehouse value", entryInstruction.CEI_OA_Warehouse2Info, expectedErrorMessage);

			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			invoiceLineWithIntoWarehouseY.JI_CEI = ZGuid.Empty;
			invoiceLineWithIntoWarehouseN.JI_CEI = entryInstruction.PK;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageErrorContaining("Invoice With CPC having IntoWarehouse=N and an empty warehouse value", entryInstruction.CEI_OA_Warehouse2Info, expectedErrorMessage);
		}
	}

	public void TestValidateFinancialAndBankingDataLine()
	{
		int maximumLength = 40;
		var expectedWarningMessage = ValidationCaptions.EntryInstruction.FinancialAndBankingDataLineExceedMaxLength(maximumLength);
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		entryInstruction.FinancialAndBankingDataLine1 = "FINANCIALANDBANKINGDATALINE1";
		AssertNoWarningContaining($"No warning expected when FinancialAndBankingDataLine1 value length is less than {maximumLength}", entryInstruction.FinancialAndBankingDataLine1Info, expectedWarningMessage);

		entryInstruction.FinancialAndBankingDataLine1 = "FINANCIALANDBANKINGDATALINE1 OTHER INFORMATIONS";
		AssertHasWarningContaining($"Warning expected when FinancialAndBankingDataLine1 value length is greater than {maximumLength}", entryInstruction.FinancialAndBankingDataLine1Info, expectedWarningMessage);

		entryInstruction.FinancialAndBankingDataLine2 = "FINANCIALANDBANKINGDATALINE2";
		AssertNoWarningContaining($"No warning expected when FinancialAndBankingDataLine1 value length is less than {maximumLength}", entryInstruction.FinancialAndBankingDataLine2Info, expectedWarningMessage);

		entryInstruction.FinancialAndBankingDataLine2 = "FINANCIALANDBANKINGDATALINE1 OTHER INFORMATIONS";
		AssertHasWarningContaining($"Warning expected when FinancialAndBankingDataLine1 value length is greater than {maximumLength}", entryInstruction.FinancialAndBankingDataLine2Info, expectedWarningMessage);
	}

	public void TestCheckWarehouseIDFor27_CannotFindAuthorisationForWarehouse_HasIntoWarehouseProcedure()
	{
		SetUpRefData();

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		invoiceLineInvoice1.JI_Procedure = "71";
		invoiceLineInvoice1.JI_CEI = entryInstruction1.PK;

		var warehouseOrgHeaderNoCCP = Factory.New<OrgHeader>();
		warehouseOrgHeaderNoCCP.OH_Code = "ORGH1";
		entryInstruction1.CEI_OA_Warehouse2 = warehouseOrgHeaderNoCCP.MainAddress.PK;
		entryInstruction1.Validation.ValidateAll();
		AssertNoWarningContaining("When Warehouse does not have a CCP Code, No warning expected", entryInstruction1.WarehouseIDFor27Info, ValidationCaptions.EntryInstruction.CannotFindAuthorisationForWarehouse);

		var warehouseOrgHeaderWithCCP = Factory.New<OrgHeader>();
		warehouseOrgHeaderWithCCP.OH_Code = "ORGH2";
		var ccpCode = warehouseOrgHeaderWithCCP.CustomsCodes.AddNew();
		ccpCode.OK_CodeType = "CCP";
		ccpCode.OK_CustomsRegNo = "C123456XIT";
		ccpCode.OK_OA_PremisesAddress = warehouseOrgHeaderWithCCP.MainAddress.PK;

		entryInstruction1.CEI_OA_Warehouse2 = warehouseOrgHeaderWithCCP.MainAddress.PK;
		AssertHasWarningContaining("When Warehouse does have a CCP Code and no authorisations with same loc rule have been inserted, Warning expected", entryInstruction1.WarehouseIDFor27Info, ValidationCaptions.EntryInstruction.CannotFindAuthorisationForWarehouse);

		var authorisationHeader = Factory.New<CusAuthorisationHeader>();
		authorisationHeader.CPH_Type = "CWP";
		authorisationHeader.CPH_Number = "12345X";
		authorisationHeader.CPH_OH_PermitHolder = warehouseOrgHeaderWithCCP.PK;

		var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = "LOC";
		authorisationRule.CPR_ValueFrom = "123456X";

		Factory.Save();
		entryInstruction1.Validation.ValidateAll();
		AssertNoWarningContaining("When Warehouse does have a CCP Code and authorisations with same loc rule have been inserted, No Warning expected", entryInstruction1.WarehouseIDFor27Info, ValidationCaptions.EntryInstruction.CannotFindAuthorisationForWarehouse);
	}

	public void TestCheckWarehouseIDFor27_CannotFindAuthorisationForWarehouse_NoIntoWarehouseProcedure()
	{
		var warehouseOrgHeaderWithCCP = Factory.New<OrgHeader>();
		var ccpCode = warehouseOrgHeaderWithCCP.CustomsCodes.AddNew();
		ccpCode.OK_CodeType = "CCP";
		ccpCode.OK_CustomsRegNo = "C123456XIT";
		ccpCode.OK_OA_PremisesAddress = warehouseOrgHeaderWithCCP.MainAddress.PK;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			entryInstruction1.CEI_OA_Warehouse2 = warehouseOrgHeaderWithCCP.MainAddress.PK;
			AssertEquals("Precondition", false, entryInstruction1.HasIntoWarehouseProcedureOnAnyInvoiceLine);
			entryInstruction1.Validation.ValidateAll();
			AssertEquals("No IntoWarehouse Procedure", false, entryInstruction1.WarehouseIDFor27Info.Notifications.Contains(ValidationCaptions.EntryInstruction.CannotFindAuthorisationForWarehouse));
		});
	}

	public void TestCheckCEI_SubStyle_MandatoryValidation()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_Style = "H1";
		entryInstruction.CEI_SubStyle = ZString.Empty;
		AssertHasMessageErrorContaining(entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Style = "H5";
		entryInstruction.Validation.ValidateCEI_SubStyle();
		AssertNoMessageErrorContaining(entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Style = "COD";
		entryInstruction.CEI_SubStyle = ZString.Empty;
		AssertHasMessageErrorContaining(entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Style = "H1";
		entryInstruction.CEI_SubStyle = "A";
		AssertNoMessageErrorContaining(entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Style = "H5";
		entryInstruction.Validation.ValidateCEI_SubStyle();
		AssertNoMessageErrorContaining(entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.CEI_Style = "COD";
		entryInstruction.Validation.ValidateCEI_SubStyle();
		AssertNoMessageErrorContaining(entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCEI_SubStyle_ListValidation()
	{
		var expectedMessage = ListValidation.InvalidCodeMessageError.ToString();
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_Style = "H1";
		entryInstruction.CEI_SubStyle = "X";
		AssertNoMessageErrorContaining("When Declaration type H1, with valid Substyle X", entryInstruction.CEI_SubStyleInfo, expectedMessage);

		entryInstruction.CEI_SubStyle = "C";
		AssertHasMessageErrorContaining("When Declaration type H1, with invalid Substyle C", entryInstruction.CEI_SubStyleInfo, expectedMessage);

		entryInstruction.CEI_Style = "H2";
		entryInstruction.CEI_SubStyle = "A";
		AssertNoMessageErrorContaining("When Declaration type H2, with valid Substyle A", entryInstruction.CEI_SubStyleInfo, expectedMessage);

		entryInstruction.CEI_SubStyle = "E";
		AssertHasMessageErrorContaining("When Declaration type H2, with invalid Substyle E", entryInstruction.CEI_SubStyleInfo, expectedMessage);

		entryInstruction.CEI_Style = "I1";
		entryInstruction.CEI_SubStyle = "B";
		AssertNoMessageErrorContaining("When Declaration type I1, with valid Substyle B", entryInstruction.CEI_SubStyleInfo, expectedMessage);

		entryInstruction.CEI_SubStyle = "A";
		AssertHasMessageErrorContaining("When Declaration type I1, with invalid Substyle A", entryInstruction.CEI_SubStyleInfo, expectedMessage);

		entryInstruction.CEI_Style = "I2";
		AssertHasMessageErrorContaining("When Declaration type I2 dont need SubStyle, still entered", entryInstruction.CEI_SubStyleInfo, expectedMessage);
	}

	public void TestCheck_UCC6_ExportSpecificCEI_SubStyleListValidation()
	{
		var expectedMessage = ListValidation.InvalidCodeMessageError.ToString();
		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_Style = "B1";
		AssertADXYZSubStyles_UCC6_Export(entryInstruction, expectedMessage);

		entryInstruction.CEI_Style = "C2";
		entryInstruction.CEI_SubStyle = string.Empty;
		AssertNoMessageErrorContaining("When Declaration type C2, empty value does not trigger validation error", entryInstruction.CEI_SubStyleInfo, expectedMessage);
		entryInstruction.CEI_SubStyle = "W";
		AssertHasMessageErrorContaining("When Declaration type C2, whatever value is not allowed", entryInstruction.CEI_SubStyleInfo, ValidationCaptions.EntryInstruction.FieldMustBeEmptyForThisKindOfDeclaration);
	}

	public void TestCheckCEI_StyleAndJE_EntryStyleForImport()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var errorMessageForIM = ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType("IM");
		var errorMessageForCO = ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType("CO");

		CombineAssertions(() =>
		{
			SetDataAndAssertMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
			SetDataAndAssertMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
			SetDataAndAssertMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3, entryStyle: "XO", errorMessage: ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType("XO"), entryInstruction: entryInstruction1);
			SetDataAndAssertMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
			SetDataAndAssertMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5, entryStyle: "IM", errorMessage: errorMessageForIM, entryInstruction: entryInstruction1);
			SetDataAndAssertMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.DichiarazioneImportazioneSemplificataI1, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);

			SetDataAndAssertNoMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1, entryStyle: "IM", errorMessage: errorMessageForIM, entryInstruction: entryInstruction1);
			SetDataAndAssertNoMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2, entryStyle: "IM", errorMessage: errorMessageForIM, entryInstruction: entryInstruction1);
			SetDataAndAssertNoMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3, entryStyle: "IM", errorMessage: errorMessageForIM, entryInstruction: entryInstruction1);
			SetDataAndAssertNoMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4, entryStyle: "IM", errorMessage: errorMessageForIM, entryInstruction: entryInstruction1);
			SetDataAndAssertNoMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
			SetDataAndAssertNoMessageError(ceiStyle: ImportUCC6DeclarationTypeList.Codes.DichiarazioneImportazioneSemplificataI1, entryStyle: "IM", errorMessage: errorMessageForIM, entryInstruction: entryInstruction1);
			SetDataAndAssertNoMessageError(ceiStyle: "XX", entryStyle: "IM", errorMessage: ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType("IM"), entryInstruction: entryInstruction1);
		});
	}

	public void TestCheckCEI_StyleAndJE_EntryStyleForUcc6Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var errorMessageForEX = ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType("EX");
		var errorMessageForCO = ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType("CO");
		var errorMessageForXO = ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType("XO");

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("UCC6, EXP", () =>
			{
				SetDataAndAssertMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
				SetDataAndAssertMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
				SetDataAndAssertMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4, entryStyle: "EX", errorMessage: errorMessageForEX, entryInstruction: entryInstruction1);
				SetDataAndAssertMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1, entryStyle: "XO", errorMessage: errorMessageForXO, entryInstruction: entryInstruction1);

				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1, entryStyle: "EX", errorMessage: errorMessageForEX, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2, entryStyle: "EX", errorMessage: errorMessageForEX, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1, entryStyle: "EX", errorMessage: errorMessageForEX, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2, entryStyle: "EX", errorMessage: errorMessageForEX, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2, entryStyle: "XO", errorMessage: errorMessageForXO, entryInstruction: entryInstruction1);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions("Non-UCC6, EXP", () =>
			{
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2, entryStyle: "CO", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4, entryStyle: "EX", errorMessage: errorMessageForCO, entryInstruction: entryInstruction1);
				SetDataAndAssertNoMessageError(ceiStyle: ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1, entryStyle: "XO", errorMessage: errorMessageForXO, entryInstruction: entryInstruction1);
			});
		}
	}

	void AssertADXYZSubStyles_UCC6_Export(CusEntryInstruction entryInstruction, string expectedMessage)
	{
		CombineAssertions($"When CEI_Style is {entryInstruction.CEI_Style}", () =>
		{
			AssertNoErrorWhenSubStyleIs("A", entryInstruction, expectedMessage);
			AssertNoErrorWhenSubStyleIs("D", entryInstruction, expectedMessage);
			AssertNoErrorWhenSubStyleIs("X", entryInstruction, expectedMessage);
			AssertNoErrorWhenSubStyleIs("Y", entryInstruction, expectedMessage);
			AssertNoErrorWhenSubStyleIs("Z", entryInstruction, expectedMessage);

			AssertHasErrorWhenSubStyleIs("W", entryInstruction, expectedMessage);
			AssertHasErrorWhenSubStyleIs(string.Empty, entryInstruction, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheck_ExportSpecificCEI_SubStyleListValidation()
	{
		var expectedMessage = ListValidation.InvalidCodeMessageError.ToString();
		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_Style = "COD";
		AssertADXYZSubStyles_Export(entryInstruction, expectedMessage);
		entryInstruction.CEI_Style = "COL";
		AssertADXYZSubStyles_Export(entryInstruction, expectedMessage);
		entryInstruction.CEI_Style = "DSE";
		AssertADXYZSubStyles_Export(entryInstruction, expectedMessage);
	}

	public void TestSameDeliveryTermOnAllInvoices()
	{
		var invoice1 = declaration.Invoices.AddNew();
		var line1 = invoice1.InvoiceLines.AddNew();
		line1.JI_CEI = entryInstruction1.PK;

		var invoice2 = declaration.Invoices.AddNew();
		var line2 = invoice2.InvoiceLines.AddNew();
		line2.JI_CEI = entryInstruction1.PK;

		var validation = new CusEntryInstructionValidation(entryInstruction1);
		var errorMessage = ValidationCaptions.EntryInstruction.InvoicesLinkedToThiEntryInstructionMustHaveSameDeliveryTerm;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoice1.JZ_AdditionalTerms = "TERM1";
			invoice2.JZ_AdditionalTerms = "TERM2";
			validation.ValidateDeliveryTermForLinkedInvoiceHeaders();
			AssertHasRowError("[UCC6, EXP] Different Delivery Terms", entryInstruction1, errorMessage);

			entryInstruction1.ClearAllNotifications();
			invoice1.JZ_AdditionalTerms = "TERM1";
			invoice2.JZ_AdditionalTerms = "TERM1";
			validation.ValidateDeliveryTermForLinkedInvoiceHeaders();
			AssertNoRowError("[UCC6, EXP] Same Delivery Terms", entryInstruction1, errorMessage);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice1.JZ_AdditionalTerms = "TERM1";
			invoice2.JZ_AdditionalTerms = "TERM2";
			validation.ValidateDeliveryTermForLinkedInvoiceHeaders();
			AssertNoRowError("[UCC6, IMP] Different Delivery Terms", entryInstruction1, errorMessage);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoice1.JZ_AdditionalTerms = "TERM1";
			invoice2.JZ_AdditionalTerms = "TERM2";
			validation.ValidateDeliveryTermForLinkedInvoiceHeaders();
			AssertNoRowError("[NON-UCC6, EXP] Different Delivery Terms", entryInstruction1, errorMessage);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			validation.ValidateDeliveryTermForLinkedInvoiceHeaders();
			AssertNoRowError("[NON-UCC6, IMP] Different Delivery Terms", entryInstruction1, errorMessage);
		}
	}

	public void TestCEI_OA_Warehouse_MultipleAuthorisationType()
	{
		SetupAuthorizationAndAssertMultipleAuthorizationForWarehouse(entryInstruction1,
			setWarehouseAction: (i, id) => i.CEI_OA_Warehouse = id,
			validateAction: (i) => i.Validation.ValidateCEI_OA_Warehouse(),
			targetPropertyInfo: entryInstruction1.CEI_OA_WarehouseInfo);
	}

	public void TestCEI_OA_Warehouse2_MultipleAuthorisationType()
	{
		SetupAuthorizationAndAssertMultipleAuthorizationForWarehouse(entryInstruction1,
			setWarehouseAction: (i, id) => i.CEI_OA_Warehouse2 = id,
			validateAction: (i) => i.Validation.ValidateCEI_OA_Warehouse2(),
			targetPropertyInfo: entryInstruction1.CEI_OA_Warehouse2Info);
	}

	public void TestCEI_OA_Warehouse_NoAuthorizationExists()
	{
		SetupAuthorizationAndAssertNoAuthorizationForWarehouse(entryInstruction1,
			setWarehouseAction: (i, id) => i.CEI_OA_Warehouse = id,
			validateAction: (i) => i.Validation.ValidateCEI_OA_Warehouse(),
			targetPropertyInfo: entryInstruction1.CEI_OA_WarehouseInfo);
	}

	public void TestCEI_OA_Warehouse2_NoAuthorizationExists()
	{
		SetupAuthorizationAndAssertNoAuthorizationForWarehouse(entryInstruction1,
			setWarehouseAction: (i, id) => i.CEI_OA_Warehouse2 = id,
			validateAction: (i) => i.Validation.ValidateCEI_OA_Warehouse2(),
			targetPropertyInfo: entryInstruction1.CEI_OA_Warehouse2Info);
	}

	public void TestAllRelatedInvoicesMustHaveSameCurrency()
	{
		var cusInstruction = Factory.New<CusEntryInstruction>();
		var validation = new JCusEntryInstructionValidationForTest(cusInstruction);
		Assert("AllRelatedInvoicesMustHaveSameCurrency must be to true", validation.AllRelatedInvoicesMustHaveSameCurrencyExposed);
	}

	public void TestCheckCEI_Procedure()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure("IT", "IM", "71", "", "", "71 DESCRIPTION", "IMP");
		helper.CreateRefCusProcedure("IT", "EX", "10", "", "", "10 DESCRIPTION", "EXP");

		entryInstruction1.Validation.ValidateCEI_Procedure();
		AssertHasErrorContaining(entryInstruction1.CEI_ProcedureInfo, MandatoryValidation.MustBeEntered);

		entryInstruction1.CEI_Procedure = "A";
		AssertNoErrorContaining(entryInstruction1.CEI_ProcedureInfo, MandatoryValidation.MustBeEntered);

		declaration.JE_MessageType = "IMP";
		entryInstruction1.CEI_Procedure = "71";
		AssertNoMessageErrorContaining(entryInstruction1.CEI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

		entryInstruction1.CEI_Procedure = "10";
		AssertHasMessageErrorContaining(entryInstruction1.CEI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_MessageType = "EXP";
		entryInstruction1.CEI_Procedure = "";
		AssertNoMessageErrorContaining(entryInstruction1.CEI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

		entryInstruction1.CEI_Procedure = "10";
		AssertNoMessageErrorContaining(entryInstruction1.CEI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

		entryInstruction1.CEI_Procedure = "71";
		AssertHasMessageErrorContaining(entryInstruction1.CEI_ProcedureInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckCEI_Procedure_WhenUCC6Export_B2DeclarationAndProcedureCodeIs21Or22ShouldRequireOPO()
	{
		declaration.MessageVersion = "XML";
		AssertEquals("[PRE-CONDITION] EXP UCC6", expected: true, declaration.IsUCC6AndIsExport);
		AssertEntityValidation(entryInstruction1)
			.WhenProperty(x => x.CEI_Style, Is.EqualTo("B2"))
			.WhenProperty(x => x.CEI_Procedure, Is.EqualToAnyOf("21", "22"))
			.ShouldCheckThat(x => x.CEI_ProcedureInfo, Has.MessageErrorContaining(
				message: "For this Declaration Type and Procedure code, an Authorization of type 'OPO' must be present"));

		var entryInstructionAuthorization = entryInstruction1.CusAuthorizationUsages.AddNew();
		entryInstructionAuthorization.AGC_Code = "OPO";
		AssertEntityValidation(entryInstruction1)
			.WhenProperty(x => x.CEI_Style, Is.EqualTo("B2"))
			.WhenProperty(x => x.CEI_Procedure, Is.EqualToAnyOf("21", "22"))
			.ShouldCheckThat(x => x.CEI_ProcedureInfo, Has.NoMessageErrorContaining(
				message: "For this Declaration Type and Procedure code, an Authorization of type 'OPO' must be present"));
	}

	void SetupAuthorizationAndAssertMultipleAuthorizationForWarehouse(CusEntryInstruction entryInstruction
		, Action<CusEntryInstruction, ZGuid> setWarehouseAction
		, Action<CusEntryInstruction> validateAction
		, ZPropertyInfo targetPropertyInfo)
	{
		var errorMessage = ValidationCaptions.EntryInstruction.MultipleAuthorisationFoundMessage;

		AssertMultipleAuthorizationForWarehouseForUcc6MessageType(EUJobMessageTypeList.Codes.Import);
		AssertMultipleAuthorizationForWarehouseForUcc6MessageType(EUJobMessageTypeList.Codes.Export);

		AssertMultipleAuthorizationForWarehouseForNonUcc6MessageType(EUJobMessageTypeList.Codes.Export);

		void AssertMultipleAuthorizationForWarehouseForUcc6MessageType(string messageType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions($"For MessageType = {messageType}", () =>
				{
					declaration.JE_MessageType = messageType;
					var warehouse = Factory.NewWithValidTestData<OrgHeader>();
					SetupAuthHeaderAndWarehouseAddress("CW11111", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, warehouse);
					SetupAuthHeaderAndWarehouseAddress("CW21111", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, warehouse);

					setWarehouseAction(entryInstruction, warehouse.MainAddress.PK);
					validateAction(entryInstruction);
					AssertHasMessageErrorContaining("More than one Authorization Found", targetPropertyInfo, errorMessage);

					var warehouse2 = Factory.NewWithValidTestData<OrgHeader>();
					SetupAuthHeaderAndWarehouseAddress("CW183", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, warehouse2);
					setWarehouseAction(entryInstruction1, warehouse2.MainAddress.PK);
					validateAction(entryInstruction);
					AssertNoMessageErrorContaining("Single Authorization configured for the warehouse. No Validation for Multiple Authorization", targetPropertyInfo, errorMessage);

					var warehouse3 = Factory.NewWithValidTestData<OrgHeader>();
					setWarehouseAction(entryInstruction, warehouse3.MainAddress.PK);
					validateAction(entryInstruction);
					AssertNoMessageErrorContaining("No Authorisations configured for the warehouse. No Validation for Multiple Authorization", targetPropertyInfo, errorMessage);

					setWarehouseAction(entryInstruction, ZGuid.Empty);
					validateAction(entryInstruction);
					AssertNoMessageErrorContaining("Empty WarehouseID. No Validation for Multiple Authorization", targetPropertyInfo, errorMessage);
				});
			}
		}

		void AssertMultipleAuthorizationForWarehouseForNonUcc6MessageType(string messageType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				CombineAssertions($"For MessageType = {messageType}", () =>
				{
					declaration.JE_MessageType = messageType;
					var warehouse = Factory.NewWithValidTestData<OrgHeader>();
					SetupAuthHeaderAndWarehouseAddress("CW11111", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, warehouse);
					SetupAuthHeaderAndWarehouseAddress("CW21111", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, warehouse);

					setWarehouseAction(entryInstruction, warehouse.MainAddress.PK);
					validateAction(entryInstruction);
					AssertNoMessageErrorContaining("More than one Authorization, No Validation for Multiple Authorization.", targetPropertyInfo, errorMessage);
				});
			}
		}
	}

	void SetupAuthorizationAndAssertNoAuthorizationForWarehouse(CusEntryInstruction entryInstruction
		, Action<CusEntryInstruction, ZGuid> setWarehouseAction
		, Action<CusEntryInstruction> validateAction
		, ZPropertyInfo targetPropertyInfo)
	{
		var errorMessage = ValidationCaptions.EntryInstruction.NoAuthorisationConfiguredMessage;
		AssertNoAuthorizationForWarehouseForUcc6MessageType(EUJobMessageTypeList.Codes.Import);
		AssertNoAuthorizationForWarehouseForUcc6MessageType(EUJobMessageTypeList.Codes.Export);

		AssertNoAuthorizationForWarehouseForNonUcc6MessageType(EUJobMessageTypeList.Codes.Export);

		void AssertNoAuthorizationForWarehouseForUcc6MessageType(string messageType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions($"For MessageType = {messageType}", () => { });
				declaration.JE_MessageType = messageType;
				var warehouse = Factory.NewWithValidTestData<OrgHeader>();
				setWarehouseAction(entryInstruction, warehouse.MainAddress.PK);
				validateAction(entryInstruction);
				AssertHasMessageErrorContaining("No Authorisation Configured for the warehouse", targetPropertyInfo, errorMessage);

				ClearAuthorizationCache(entryInstruction, warehouse);
				SetupAuthHeaderAndWarehouseAddress("CW11111", CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer, warehouse);
				SetupAuthHeaderAndWarehouseAddress("CW21111", CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, warehouse);
				validateAction(entryInstruction);
				AssertHasMessageErrorContaining("Authorisations other than CW1, CW2, CWP are Configured for the warehouse", targetPropertyInfo, errorMessage);

				ClearAuthorizationCache(entryInstruction, warehouse);
				SetupAuthHeaderAndWarehouseAddress("CW183", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, warehouse);
				validateAction(entryInstruction);
				AssertNoMessageErrorContaining("Single Authorization configured for the warehouse. No Validation for No Authorization", targetPropertyInfo, errorMessage);

				ClearAuthorizationCache(entryInstruction, warehouse);
				setWarehouseAction(entryInstruction, ZGuid.Empty);
				validateAction(entryInstruction);
				AssertNoMessageErrorContaining("Empty WarehouseID. No Validation for No Authorization", targetPropertyInfo, errorMessage);
			}
		}

		void AssertNoAuthorizationForWarehouseForNonUcc6MessageType(string messageType)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				CombineAssertions($"For MessageType = {messageType}", () =>
				{
					declaration.JE_MessageType = messageType;
					var warehouse = Factory.NewWithValidTestData<OrgHeader>();
					setWarehouseAction(entryInstruction, warehouse.MainAddress.PK);
					validateAction(entryInstruction);
					AssertNoMessageErrorContaining("No Validation for No Authorization", targetPropertyInfo, errorMessage);
				});
			}
		}
	}

	void ClearAuthorizationCache(CusEntryInstruction entryInstruction, OrgHeader warehouse)
	{
		entryInstruction.Factory.ClearCachedValue<CusAuthorisationHeader[]>($"IT_GetAuthorisationHeaders|{ZDateTime.Today.ToISO8601ShortDateString()}|{warehouse.MainAddress.PK.ToStringKey()}");
	}

	void SetupAuthHeaderAndWarehouseAddress(string referenceNumber, string authHeaderType, OrgHeader warehouse)
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Type = authHeaderType;

		var owner = Factory.NewWithValidTestData<OrgHeader>();
		authorizationHeader.CPH_OH_PermitHolder = owner.PK;
		authorizationHeader.CPH_Number = referenceNumber;

		authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;
	}

	void AssertADXYZSubStyles_Export(CusEntryInstruction entryInstruction, string expectedMessage)
	{
		CombineAssertions($"When CEI_Style is {entryInstruction.CEI_Style}", () =>
		{
			AssertNoErrorWhenSubStyleIs("A", entryInstruction, expectedMessage);
			AssertNoErrorWhenSubStyleIs("D", entryInstruction, expectedMessage);
			AssertHasErrorWhenSubStyleIs("X", entryInstruction, expectedMessage);
			AssertHasErrorWhenSubStyleIs("Y", entryInstruction, expectedMessage);
			AssertHasErrorWhenSubStyleIs("Z", entryInstruction, expectedMessage);
			AssertHasErrorWhenSubStyleIs("W", entryInstruction, expectedMessage);
		});
	}

	void AssertHasErrorWhenSubStyleIs(string subStyle, CusEntryInstruction entryInstruction, string expectedMessage)
	{
		entryInstruction.CEI_SubStyle = subStyle;
		entryInstruction.Validation.ValidateCEI_SubStyle();
		AssertHasMessageErrorContaining($"Substyle {subStyle} is not allowed", entryInstruction.CEI_SubStyleInfo, expectedMessage);
	}

	void AssertNoErrorWhenSubStyleIs(string subStyle, CusEntryInstruction entryInstruction, string expectedMessage)
	{
		entryInstruction.CEI_SubStyle = subStyle;
		entryInstruction.Validation.ValidateCEI_SubStyle();
		AssertNoMessageErrorContaining($"Substyle {subStyle} is allowed", entryInstruction.CEI_SubStyleInfo, expectedMessage);
	}

	void SetDataAndAssertMessageError(ZString ceiStyle, ZString entryStyle, string errorMessage, CusEntryInstruction entryInstruction)
	{
		var ceiStylePropertyInfo = entryInstruction.CEI_StyleInfo;
		SetCeiStyleAndEntryStyle(ceiStyle, entryStyle, entryInstruction);
		entryInstruction.Validation.ValidateCEI_Style();
		AssertHasMessageErrorContaining($"CEI_Style={ceiStyle}, JE_EntryStyle={entryStyle}, Expected Error", ceiStylePropertyInfo, errorMessage);
	}

	void SetDataAndAssertNoMessageError(ZString ceiStyle, ZString entryStyle, string errorMessage, CusEntryInstruction entryInstruction)
	{
		var ceiStylePropertyInfo = entryInstruction.CEI_StyleInfo;
		SetCeiStyleAndEntryStyle(ceiStyle, entryStyle, entryInstruction);
		entryInstruction.Validation.ValidateCEI_Style();
		AssertNoMessageErrorContaining($"CEI_Style={ceiStyle}, JE_EntryStyle={entryStyle}, No error", ceiStylePropertyInfo, errorMessage);
	}

	void SetCeiStyleAndEntryStyle(ZString ceiStyle, ZString entryStyle, CusEntryInstruction entryInstruction)
	{
		declaration.JE_EntryStyle = entryStyle;
		entryInstruction.CEI_Style = ceiStyle;
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure("IT", "IM", "40", "", "", "Procedure1", "IMP", intoWarehouse: false);
		helper.CreateRefCusProcedure("IT", "IM", "71", "", "", "Procedure1", "IMP", intoWarehouse: true);

		helper.CreateRefCusProcedure("IT", "EX", "01", "", "", "Procedure3", "EXP", intoWarehouse: false);
		helper.CreateRefCusProcedure("IT", "EX", "63", "", "", "Procedure4", "EXP", intoWarehouse: true);
		helper.CreateRefCusProcedure("IT", "EX", "61", "", "", "Procedure61", "EXP", outOfWarehouse: false);
		helper.CreateRefCusProcedure("IT", "EX", "44", "", "", "Procedure44", "EXP", outOfWarehouse: true);
	}

	OrgHeader CreateNewWarehouse()
	{
		var warehouseOrgHeader = Factory.New<OrgHeader>();
		warehouseOrgHeader.OH_Code = "ORG1";

		var owner = Factory.New<OrgHeader>();
		owner.OH_Code = "ORG2";

		var warehouseAddress = Factory.New<OrgAddress>();
		warehouseAddress.Address1 = "Street 2";
		warehouseAddress.OA_OH = owner.PK;
		return warehouseOrgHeader;
	}

	IDisposable TemporarilySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	protected override void SetUp()
	{
		base.SetUp();

		var today = ZDate.Today;
		declaration = Factory.New<JobDeclaration>();
		entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_DateForDuty = today;
		entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_DateForDuty = today;
		invoice1 = declaration.Invoices.AddNew();
		invoice2 = declaration.Invoices.AddNew();
		invoiceLineInvoice1 = invoice1.InvoiceLines.AddNew();
		invoiceLineInvoice2 = invoice2.InvoiceLines.AddNew();
		invoiceLineInvoice1.JI_CEI = ZGuid.Empty;
		invoiceLineInvoice2.JI_CEI = ZGuid.Empty;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction1;
	CusEntryInstruction entryInstruction2;
	JobComInvoiceHeader invoice1;
	JobComInvoiceHeader invoice2;
	JobComInvoiceLine invoiceLineInvoice1;
	JobComInvoiceLine invoiceLineInvoice2;

	class JCusEntryInstructionValidationForTest : CusEntryInstructionValidation
	{
		public JCusEntryInstructionValidationForTest(CusEntryInstruction instruction) : base(instruction)
		{
		}

		public ZBool AllRelatedInvoicesMustHaveSameCurrencyExposed => base.AllRelatedInvoicesMustHaveSameCurrency;

		public ZBool AllRelatedInvoicesMustHaveSameTransactionNatureExposed => base.AllRelatedInvoicesMustHaveSameTransactionNature;
	}
}
