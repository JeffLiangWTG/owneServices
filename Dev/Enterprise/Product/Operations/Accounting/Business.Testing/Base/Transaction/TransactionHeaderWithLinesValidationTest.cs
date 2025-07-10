using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class TransactionHeaderWithLinesValidationTest : TransactionHeaderValidationTest
	{
		public void TestValidateOSTotalAmount()
		{
			TransactionHeaderWithLines testTransaction = (TransactionHeaderWithLines)Factory.NewWithValidTestData(HeaderType);
			testTransaction.Lines.AddNew();
			testTransaction.Lines.AddNew();
			var line1 = testTransaction.Lines[0];
			var line2 = testTransaction.Lines[1];

			line1.AL_OSAmount = 10M;
			line2.AL_OSAmount = 6.5;

			testTransaction.AH_OSTotalAmount = 15M;

			TransactionHeaderWithLinesValidation validation = new TransactionHeaderWithLinesValidation(testTransaction);

			var ledgerTypesForARAP = new List<string>();
			ledgerTypesForARAP.Add(LedgerTypes.AccountsPayable);
			ledgerTypesForARAP.Add(LedgerTypes.AccountsReceivable);

			var transactionTypesToCheckForARAP = new List<string>();
			transactionTypesToCheckForARAP.Add(TransactionTypes.Invoice);
			transactionTypesToCheckForARAP.Add(TransactionTypes.CreditNote);
			transactionTypesToCheckForARAP.Add(TransactionTypes.AdjustmentNote);

			var transactionTypesToCheckForCashBook = new List<string>();
			transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectPayment);
			transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectReceipt);

			var isTransactionTypeToCheck = ((ledgerTypesForARAP.Contains(validation.Parent_ForTestOnly.AH_Ledger) && transactionTypesToCheckForARAP.Contains(validation.Parent_ForTestOnly.AH_TransactionType)) ||
			(validation.Parent_ForTestOnly.AH_Ledger == LedgerTypes.CashBook && transactionTypesToCheckForCashBook.Contains(validation.Parent_ForTestOnly.AH_TransactionType)));

			if (isTransactionTypeToCheck)
			{
				validation.ValidateAH_OSTotalAmount();
				AssertHasError(testTransaction.AH_OSTotalAmountInfo, "Transaction Line OS Amount Total does not match the Transaction Header OS Amount.");
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_OSTotalAmount_WithSourceReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Assert("PreCondition", invoice.IsSourceReferenceEnabled);
				Assert(!invoice.IsSourceReferenceUsed);

				invoice.Lines.AddNew();
				var line = invoice.Lines[0];
				line.AL_OSExTaxAmount = 10M;

				AssertEquals(10m, invoice.AH_OSTotalAmount);
				((TransactionHeaderWithLinesValidation)invoice.Validation).ValidateAH_OSTotalAmount();
				AssertNoErrors(invoice.AH_OSTotalAmountInfo);

				invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				Assert(invoice.IsSourceReferenceUsed);

				invoice.AH_OSTotalAmount = 30m;
				AssertEquals(30m, invoice.AH_OSTotalAmount);
				AssertNotEquals("When Source Reference is used, we do not check the synchronisation between line amounts and invoice total. Bad data can be saved",
					invoice.AH_OSExTaxAmount + invoice.AH_OSTaxAmount, invoice.AH_OSTotalAmount);

				((TransactionHeaderWithLinesValidation)invoice.Validation).ValidateAH_OSTotalAmount();
				AssertNoErrors("Transaction Line OS Amount Total does not match the Transaction Header OS Amount.", invoice.AH_OSTotalAmountInfo);
				AssertHasWarning(invoice.AH_OSTotalAmountInfo, "Transaction Line OS Amount Total does not match the Transaction Header OS Amount.");
			}
		}

		public override void TestCheckAH_GB_TaxBranch()
		{
			base.TestCheckAH_GB_TaxBranch();
			TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.TestOrganisation.CompanyData.SetAPTaxApplicable(true);

			var headerWithLines = Factory.NewWithValidTestData(HeaderType) as TransactionHeaderWithLines;
			headerWithLines.AH_OH = TestObjectCreator.TestOrganisation.PK;

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("PreCondition", false, headerWithLines.CanApplyTaxBranch);
			headerWithLines.Validation.ValidateAH_GB_TaxBranch();
			AssertNoErrors(headerWithLines.AH_GB_TaxBranchInfo);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			headerWithLines.AH_GB_TaxBranch = ZGuid.Empty;
			if (headerWithLines.CanApplyTaxBranch)
			{
				headerWithLines.Validation.ValidateAH_GB_TaxBranch();
				AssertHasError(headerWithLines.AH_GB_TaxBranchInfo, "Please enter a Tax Branch.");
			}
			else
			{
				headerWithLines.Validation.ValidateAH_GB_TaxBranch();
				AssertNoErrors(headerWithLines.AH_GB_TaxBranchInfo);
			}
		}

		public virtual void TestValidateOSTotalAmountExcludeOtherTaxes()
		{
			TransactionHeaderWithLines testTransaction = (TransactionHeaderWithLines)Factory.NewWithValidTestData(HeaderType);
			testTransaction.Lines.AddNew();
			testTransaction.Lines.AddNew();
			var line1 = testTransaction.Lines[0];
			var line2 = testTransaction.Lines[1];

			line1.AL_OSAmount = 10M;
			line2.AL_OSAmount = 6.5;

			testTransaction.AH_OSExTaxAmount = 9M;
			testTransaction.AH_OSTaxAmount = 6.5M;
			testTransaction.AH_OSTaxAmountOtherTaxes = 1M * Math.Sign(testTransaction.AH_OSTotal);
			AssertEquals("Precondition: AH_OSTotalAmount", 16.5m, testTransaction.AH_OSTotalAmount);

			TransactionHeaderWithLinesValidation validation = new TransactionHeaderWithLinesValidation(testTransaction);

			var ledgerTypesForARAP = new List<string>();
			ledgerTypesForARAP.Add(LedgerTypes.AccountsPayable);
			ledgerTypesForARAP.Add(LedgerTypes.AccountsReceivable);

			var transactionTypesToCheckForARAP = new List<string>();
			transactionTypesToCheckForARAP.Add(TransactionTypes.Invoice);
			transactionTypesToCheckForARAP.Add(TransactionTypes.CreditNote);
			transactionTypesToCheckForARAP.Add(TransactionTypes.AdjustmentNote);

			var transactionTypesToCheckForCashBook = new List<string>();
			transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectPayment);
			transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectReceipt);

			var isTransactionTypeToCheck = ((ledgerTypesForARAP.Contains(validation.Parent_ForTestOnly.AH_Ledger) && transactionTypesToCheckForARAP.Contains(validation.Parent_ForTestOnly.AH_TransactionType)) ||
			(validation.Parent_ForTestOnly.AH_Ledger == LedgerTypes.CashBook && transactionTypesToCheckForCashBook.Contains(validation.Parent_ForTestOnly.AH_TransactionType)));

			if (isTransactionTypeToCheck)
			{
				validation.ValidateAH_OSTotalAmount();
				AssertHasError(testTransaction.AH_OSTotalAmountInfo, "Transaction Line OS Amount Total does not match the Transaction Header OS Amount.");

				testTransaction.AH_OSExTaxAmount = 10M;
				validation.ValidateAH_OSTotalAmount();
				AssertEquals("Precondition: AH_OSTotalAmount", 17.5m, testTransaction.AH_OSTotalAmount);
				AssertNoErrors("No error with the Transaction Total since lines and the header have the same amount excluding Tax Transactions.", testTransaction.AH_OSTotalAmountInfo);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestValidateOSTotalAmountOnlyIfLineCurrenciesMatchHeaderCurrency()
		{
			TransactionHeaderWithLines testTransaction = (TransactionHeaderWithLines)Factory.NewWithValidTestData(HeaderType);
			testTransaction.AH_RX_NKTransactionCurrency = "EUR";
			testTransaction.AH_OH = TestObjectCreator.ABIGAS.PK;
			testTransaction.AH_OA_InvoiceAddressOverride = TestObjectCreator.ABIGAS.Addresses[0].PK;

			testTransaction.Lines.AddNew();
			testTransaction.Lines.AddNew();
			var line1 = testTransaction.Lines[0];
			var line2 = testTransaction.Lines[1];

			line1.AL_OSAmount = 10M;
			line1.AL_OSExTaxAmount = 10M;
			line1.AL_RX_NKTransactionCurrency = "EUR";
			line2.AL_OSAmount = 6.5;
			line2.AL_OSExTaxAmount = 6.5M;
			line2.AL_RX_NKTransactionCurrency = "USD";

			testTransaction.AH_OSTotalAmount = 15M;

			TransactionHeaderWithLinesValidation validation = new TransactionHeaderWithLinesValidation(testTransaction);

			var ledgerTypesForARAP = new List<string>();
			ledgerTypesForARAP.Add(LedgerTypes.AccountsPayable);
			ledgerTypesForARAP.Add(LedgerTypes.AccountsReceivable);

			var transactionTypesToCheckForARAP = new List<string>();
			transactionTypesToCheckForARAP.Add(TransactionTypes.Invoice);
			transactionTypesToCheckForARAP.Add(TransactionTypes.CreditNote);
			transactionTypesToCheckForARAP.Add(TransactionTypes.AdjustmentNote);

			var transactionTypesToCheckForCashBook = new List<string>();
			transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectPayment);
			transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectReceipt);

			var isTransactionTypeToCheck = ((ledgerTypesForARAP.Contains(validation.Parent_ForTestOnly.AH_Ledger) && transactionTypesToCheckForARAP.Contains(validation.Parent_ForTestOnly.AH_TransactionType)) ||
			(validation.Parent_ForTestOnly.AH_Ledger == LedgerTypes.CashBook && transactionTypesToCheckForCashBook.Contains(validation.Parent_ForTestOnly.AH_TransactionType)));

			if (isTransactionTypeToCheck)
			{
				validation.ValidateAH_OSTotalAmount();

				AssertNoErrors("No error with the Transaction Total since lines and the header uses different currencies", testTransaction.AH_OSTotalAmountInfo);
			}
			else
			{
				Assert(true);
			}
		}
	}
}
