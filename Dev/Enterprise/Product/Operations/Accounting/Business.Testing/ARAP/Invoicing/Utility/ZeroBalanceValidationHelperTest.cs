using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	public class ZeroBalanceValidationHelperTest : TestCaseWithFactory
	{
		#region IsZeroOSTotalAmountInvalid

		public void TestIsZeroOSTotalAmountInvalid_NonAR()
		{
			AssertZeroBalanceOSTotalAmount<APInvoice>(isRegistryOn: false, OldInvalid, Valid, OldInvalid, Valid, Valid, Valid);
			AssertZeroBalanceOSTotalAmount<APInvoice>(isRegistryOn: true, OldInvalid, Valid, OldInvalid, Valid, Valid, Valid);
		}

		public void TestIsZeroOSTotalAmountInvalid_ARInvoice()
		{
			AssertZeroBalanceOSTotalAmount<ARInvoice>(isRegistryOn: false, OldInvalid, NewInvalid, OldInvalid, NewInvalid, NewInvalid, NewInvalid);
			AssertZeroBalanceOSTotalAmount<ARInvoice>(isRegistryOn: true, OldInvalid, Valid, OldInvalid, Valid, Valid, Valid);
		}

		public void TestIsZeroOSTotalAmountInvalid_ARCreditNote()
		{
			AssertZeroBalanceOSTotalAmount<ARCreditNote>(isRegistryOn: false, OldInvalid, NewInvalid, OldInvalid, NewInvalid, NewInvalid, NewInvalid);
			AssertZeroBalanceOSTotalAmount<ARCreditNote>(isRegistryOn: true, OldInvalid, Valid, OldInvalid, Valid, Valid, Valid);
		}

		public void TestIsZeroOSTotalAmountInvalid_ARAdjustmentNote()
		{
			AssertZeroBalanceOSTotalAmount<ARAdjustmentNote>(isRegistryOn: false, OldInvalid, NewInvalid, OldInvalid, NewInvalid, NewInvalid, NewInvalid);
			AssertZeroBalanceOSTotalAmount<ARAdjustmentNote>(isRegistryOn: true, OldInvalid, Valid, OldInvalid, Valid, Valid, Valid);
		}

		void AssertZeroBalanceOSTotalAmount<T>(bool isRegistryOn, params (bool IsValid, string ErrorMsg)[] expectedValues) where T : InvoicingBase
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryOn);

			ConvertInvoicingBaseType<T>(out string ledger, out string transactionType);
			var transactionCreator = new TransactionCreator();
			int index = 0;

			var invoice = (T)transactionCreator.CreateTransaction(Factory, ledger, transactionType, numberOfLines: 0);
			invoice.AH_OSTotalAmount = 0m;
			AssertIsValidAndErrorMessage("No lines");

			invoice = (T)transactionCreator.CreateTransaction(Factory, ledger, transactionType, numberOfLines: 1);
			invoice.AH_OSTotalAmount = 0m;
			invoice.Lines[0].AL_AC = TestObjectCreator.CommentChargeCode.PK;
			AssertIsValidAndErrorMessage("1 comment charge line");

			invoice = (T)transactionCreator.CreateTransaction(Factory, ledger, transactionType, numberOfLines: 1);
			invoice.AH_OSTotalAmount = 0m;
			invoice.Lines[0].AL_AC = TestObjectCreator.FRT.PK;
			AssertIsValidAndErrorMessage("1 non-comment charge line");

			invoice = (T)transactionCreator.CreateTransaction(Factory, ledger, transactionType, numberOfLines: 2);
			invoice.AH_OSTotalAmount = 0m;
			var lines = invoice.Lines.Cast<InvoicingLineBase>().ToList();
			lines[0].AL_AC = testObjectCreator.CommentChargeCode.PK;
			lines[1].AL_AC = testObjectCreator.FRT.PK;
			AssertIsValidAndErrorMessage("1 comment charge line, 1 non-comment");

			invoice = (T)transactionCreator.CreateTransaction(Factory, ledger, transactionType, numberOfLines: 2);
			invoice.AH_OSTotalAmount = 0m;
			lines = invoice.Lines.Cast<InvoicingLineBase>().ToList();
			lines[0].AL_AC = testObjectCreator.CommentChargeCode.PK;
			lines[1].AL_AC = testObjectCreator.CommentChargeCode.PK;
			AssertIsValidAndErrorMessage("2 comment charge lines");

			invoice = (T)transactionCreator.CreateTransaction(Factory, ledger, transactionType, numberOfLines: 2);
			invoice.AH_OSTotalAmount = 0m;
			lines = invoice.Lines.Cast<InvoicingLineBase>().ToList();
			lines[0].AL_AC = testObjectCreator.FRT.PK;
			lines[1].AL_AC = testObjectCreator.FRT.PK;
			AssertIsValidAndErrorMessage("2 non-comment charge lines");

			void AssertIsValidAndErrorMessage(string assertionMsg)
			{
				var fullAssertionMsg = $"{ledger} {transactionType}, registry value {isRegistryOn}, {assertionMsg}";
				var expectedValue = expectedValues[index++];

				AssertEquals(fullAssertionMsg, !expectedValue.IsValid, ZeroBalanceValidationHelper.IsZeroOSTotalAmountInvalid(invoice, out string actualErrorMsg));
				AssertEquals(fullAssertionMsg, expectedValue.ErrorMsg, actualErrorMsg);
			}
		}

		void ConvertInvoicingBaseType<T>(out string ledger, out string transactionType)
		{
			if (typeof(T) == typeof(APInvoice))
			{
				ledger = LedgerTypes.AccountsPayable;
				transactionType = TransactionTypes.Invoice;
			}
			else if (typeof(T) == typeof(ARInvoice))
			{
				ledger = LedgerTypes.AccountsReceivable;
				transactionType = TransactionTypes.Invoice;
			}
			else if (typeof(T) == typeof(ARCreditNote))
			{
				ledger = LedgerTypes.AccountsReceivable;
				transactionType = TransactionTypes.CreditNote;
			}
			else if (typeof(T) == typeof(ARAdjustmentNote))
			{
				ledger = LedgerTypes.AccountsReceivable;
				transactionType = TransactionTypes.AdjustmentNote;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		(bool IsValid, string ErrorMsg) Valid => (true, null);
		(bool IsValid, string ErrorMsg) OldInvalid => (false, "The sum of the transaction lines should not equal zero.");
		(bool IsValid, string ErrorMsg) NewInvalid => (false, "Transaction Total cannot be zero. This is controlled by the registry: Accounting -> Receivable Defaults -> Default Settings -> Allow Posting of Zero Value AR Invoices");

		#endregion

		#region IsZeroOSTotalAmountInvalidForPeriodicInvoice

		public void TestIsZeroOSTotalAmountInvalidForPeriodicInvoice()
		{
			var result = ZeroBalanceValidationHelper.IsZeroOSTotalAmountInvalidForPeriodicInvoice(out string errorMessage);

			AssertEquals("Should return false by default.", false, result);
			AssertEquals("Should have no errorMessage.", null, errorMessage);

			using(AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				result = ZeroBalanceValidationHelper.IsZeroOSTotalAmountInvalidForPeriodicInvoice(out errorMessage);

				AssertEquals("Should return true.", true, result);
				AssertEquals("Should have expected errorMessage.", "Total Amount cannot be 0. Please select transactions in order to generate Periodic Invoice. This is controlled by the registry: Accounting -> Receivable Defaults -> Default Settings -> Allow Posting of Zero Value AR Invoices.", errorMessage);
			}
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
