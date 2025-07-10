using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusEntryInstructionValidaitonBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckInvoice_Currency_UniquenessForJobOrEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage = "All Invoices on an Entry Instruction must have the same '[22] Currency'.";

			var entryInstruction1Validation = new CusEntryInstructionValidationForInnerTest(entryInstruction);
			CombineAssertions("When currencies are empty", () =>
			{
				invoiceHeader1.JZ_RX_NKInvoice_Currency = "";
				invoiceHeader2.JZ_RX_NKInvoice_Currency = "";
				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = true;
				entryInstruction1Validation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is true", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = false;
				entryInstruction1Validation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is false", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("When one currency is blank and another is not", () =>
			{
				invoiceHeader1.JZ_RX_NKInvoice_Currency = "EUR";
				invoiceHeader2.JZ_RX_NKInvoice_Currency = "";
				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = true;
				entryInstruction1Validation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is true", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = false;
				entryInstruction1Validation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is false", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("When currencies are different", () =>
			{
				invoiceHeader1.JZ_RX_NKInvoice_Currency = "EUR";
				invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = true;
				entryInstruction1Validation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is true", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = false;
				entryInstruction1Validation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is false", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("When currencies are equal", () =>
			{
				invoiceHeader1.JZ_RX_NKInvoice_Currency = "EUR";
				invoiceHeader2.JZ_RX_NKInvoice_Currency = "EUR";
				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = true;
				entryInstruction1Validation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is true", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstruction1Validation.AllRelatedInvoicesMustHaveSameCurrencyOverriden = false;
				entryInstruction1Validation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameCurrencyOverriden is false", entryInstruction, jzRX_NKInvoice_Currency_UniquenessForJobOrEntryInstructionExpectedMessage);
			});
		}

		public void TestCheckIncoTermPlace_UniquenessForJobOrEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage = "All Invoices on an Entry Instruction must have the same 'Agreed Place'.";

			var entryInstructionValidation = new CusEntryInstructionValidationForInnerTest(entryInstruction);
			CombineAssertions("Empty JZ_IncoTermPlace", () =>
			{
				invoiceHeader1.JZ_IncoTermPlace = "";
				invoiceHeader2.JZ_IncoTermPlace = "";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is true", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is false", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("Same JZ_IncoTermPlace", () =>
			{
				invoiceHeader1.JZ_ValuationCode = "1";
				invoiceHeader2.JZ_ValuationCode = "1";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is true", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is false", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("1 Empty JZ_IncoTermPlace", () =>
			{
				invoiceHeader1.JZ_IncoTermPlace = "1";
				invoiceHeader2.JZ_IncoTermPlace = "";
				entryInstructionValidation.ValidateAll();
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is true", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is false", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("Different JZ_IncoTermPlace", () =>
			{
				invoiceHeader1.JZ_IncoTermPlace = "1";
				invoiceHeader2.JZ_IncoTermPlace = "2";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is true", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden is false", entryInstruction, jzIncoTermPlace_UniquenessForJobOrEntryInstructionExpectedMessage);
			});
		}

		public void TestCheckValuationCode_UniquenessForJobOrEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage = "All Invoices on an Entry Instruction must have the same '[24] Tran. Nature'.";

			var entryInstructionValidation = new CusEntryInstructionValidationForInnerTest(entryInstruction);
			CombineAssertions("Empty JZ_ValuationCode", () =>
			{
				invoiceHeader1.JZ_ValuationCode = "";
				invoiceHeader2.JZ_ValuationCode = "";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is true", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is false", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("1 Empty JZ_ValuationCode", () =>
			{
				invoiceHeader1.JZ_ValuationCode = "1";
				invoiceHeader2.JZ_ValuationCode = "";
				entryInstructionValidation.ValidateAll();
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is true", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is false", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("Different JZ_ValuationCode", () =>
			{
				invoiceHeader1.JZ_ValuationCode = "1";
				invoiceHeader2.JZ_ValuationCode = "2";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is true", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is false", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("Same JZ_ValuationCode", () =>
			{
				invoiceHeader1.JZ_ValuationCode = "1";
				invoiceHeader2.JZ_ValuationCode = "1";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = true;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is true", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameTransactionNatureOverriden = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameTransactionNatureOverriden is false", entryInstruction, jzValuationCode_UniquenessForJobOrEntryInstructionExpectedMessage);
			});
		}

		public void TestCheckIncoTerm_UniquenessForJobOrEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage = "All Invoices on an Entry Instruction must have the same '[20] Incoterm'.";

			var entryInstructionValidation = new CusEntryInstructionValidationForInnerTest(entryInstruction);
			CombineAssertions("Empty IncoTerm", () =>
			{
				invoiceHeader1.JZ_IncoTerm = "";
				invoiceHeader2.JZ_IncoTerm = "";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = true;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is true", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is false", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("1 Empty IncoTerm", () =>
			{
				invoiceHeader1.JZ_IncoTerm = "1";
				invoiceHeader2.JZ_IncoTerm = "";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = true;
				entryInstructionValidation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is true", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is false", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("Different IncoTerms", () =>
			{
				invoiceHeader1.JZ_IncoTerm = "1";
				invoiceHeader2.JZ_IncoTerm = "2";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = true;
				entryInstructionValidation.ValidateAll();
				AssertHasRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is true", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is false", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);
			});

			CombineAssertions("Same IncoTerms", () =>
			{
				invoiceHeader1.JZ_IncoTerm = "1";
				invoiceHeader2.JZ_IncoTerm = "1";
				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = true;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is true", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);

				entryInstructionValidation.AllRelatedInvoicesMustHaveSameIncotermOverridem = false;
				entryInstructionValidation.ValidateAll();
				AssertNoRowMessageError("AllRelatedInvoicesMustHaveSameIncoterm is false", entryInstruction, jzIncoTerm_UniquenessForJobOrEntryInstructionExpectedMessage);
			});
		}

		public void TestValidateAllInvoicesUseSame_Defaults()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var validation = new CusEntryInstructionValidationForInnerTest(instruction);
			CombineAssertions(() =>
			{
				AssertEquals("AllRelatedInvoicesMustHaveSameCurrency must be to false", false, validation.AllRelatedInvoicesMustHaveSameCurrencyExposed);
				AssertEquals("AllRelatedInvoicesMustHaveSameAgreedPlaceExposed must be to false", false, validation.AllRelatedInvoicesMustHaveSameAgreedPlaceExposed);
				AssertEquals("AllRelatedInvoicesMustHaveSameIncotermExposed must be to true", true, validation.AllRelatedInvoicesMustHaveSameIncotermExposed);
				AssertEquals("AllRelatedInvoicesMustHaveSameTransactionNatureExposed must be to true", true, validation.AllRelatedInvoicesMustHaveSameTransactionNatureExposed);
			});
		}

		class CusEntryInstructionValidationForInnerTest : CusEntryInstructionValidation
		{
			public CusEntryInstructionValidationForInnerTest(CusEntryInstruction parent) : base(parent)
			{
			}

			public ZBool AllRelatedInvoicesMustHaveSameCurrencyOverriden { get; set; }

			protected override ZBool AllRelatedInvoicesMustHaveSameCurrency => AllRelatedInvoicesMustHaveSameCurrencyOverriden;

			public ZBool AllRelatedInvoicesMustHaveSameCurrencyExposed => base.AllRelatedInvoicesMustHaveSameCurrency;

			public ZBool AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden { get; set; }

			protected override ZBool AllRelatedInvoicesMustHaveSameAgreedPlace => AllRelatedInvoicesMustHaveSameAgreedPlaceOverriden;

			public ZBool AllRelatedInvoicesMustHaveSameAgreedPlaceExposed => base.AllRelatedInvoicesMustHaveSameAgreedPlace;

			public ZBool AllRelatedInvoicesMustHaveSameIncotermOverridem { get; set; }

			protected override ZBool AllRelatedInvoicesMustHaveSameIncoterm => AllRelatedInvoicesMustHaveSameIncotermOverridem;

			public ZBool AllRelatedInvoicesMustHaveSameIncotermExposed => base.AllRelatedInvoicesMustHaveSameIncoterm;

			public ZBool AllRelatedInvoicesMustHaveSameTransactionNatureOverriden { get; set; }

			protected override ZBool AllRelatedInvoicesMustHaveSameTransactionNature => AllRelatedInvoicesMustHaveSameTransactionNatureOverriden;

			public ZBool AllRelatedInvoicesMustHaveSameTransactionNatureExposed => base.AllRelatedInvoicesMustHaveSameTransactionNature;
		}
	}
}
