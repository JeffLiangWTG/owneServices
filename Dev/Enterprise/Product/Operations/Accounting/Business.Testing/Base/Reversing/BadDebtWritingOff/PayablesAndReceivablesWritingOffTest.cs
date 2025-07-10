using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Reversing.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff.Testing
{
	class PayablesAndReceivablesWritingOffTest : PayablesAndReceivablesReversingTest
	{
		public void TestSetCancellationFlagOnTransactionsToReverse()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_IsCancelled = false;
			PayablesAndReceivablesWritingOff writingOff = new PayablesAndReceivablesWritingOff(invoice);
			writingOff.GenerateReverseTransactions_ForTestOnly();
			writingOff.SetCancellationFlagOnTransactionsToReverse_ForTestOnly();
			ARCreditNote creditNote = (ARCreditNote)writingOff.ReverseTransaction;
			Assert("IsCancelled must be false!", !invoice.AH_IsCancelled);
			Assert("IsCancelled must be false!", !creditNote.AH_IsCancelled);
		}

		public void TestGenerateReverseTransactions()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge = Factory.NewWithValidTestData<Charge>();
			job.Charges.Add(charge);
			job.PostAutoPopulation();

			ARInvoice invoice = Factory.New<ARInvoice>();
			TransactionLine line = invoice.Lines.AddNew();
			line.AL_LineType = "REV";
			invoice.AH_JH = job.PK;
			charge.JR_AL_ARLine = line.PK;

			Assert("Charge must be posted", job.Charges[0].JR_IsRevenuePosted);

			PayablesAndReceivablesWritingOff writingOff = new PayablesAndReceivablesWritingOff(invoice);
			writingOff.GenerateReverseTransactions_ForTestOnly();
			Assert("Charge must be posted", job.Charges[0].JR_IsRevenuePosted);
		}

		public override void TestCantReverseMatchedIPayablesAndReceivables()
		{
			Assert(true);
		}

		public override void TestErrorMessages()
		{
			ZString expectedErrorMessage = "This transaction cannot be written off because it has already been reversed or is a reversal of another transaction.";
			AssertEquals("The Error Message should be generated for writing off.", expectedErrorMessage, PayablesAndReceivablesWritingOff.AlreadyReversedErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be written off because it has been matched with other transactions.";
			AssertEquals("The Error Message should be generated for writing off.", expectedErrorMessage, PayablesAndReceivablesWritingOff.MatchedAndCantReverseErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be written off because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing.";
			AssertEquals("The Error Message should be generated for writing off.", expectedErrorMessage, PayablesAndReceivablesWritingOff.ClearedInCashBookErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be written off because it has been partially paid.";
			AssertEquals("The Error Message should be generated for writing off.", expectedErrorMessage, PayablesAndReceivablesWritingOff.CantWriteOffPartiallyPaidTransactionErrorMessage_ForTestOnly);
		}

		public void TestCantReverseIPayablesAndReceivables()
		{
			// Test DB will not have the StmData base data, which contains the default value for bad debt for the registry.
			// So add bad debt into the registry for this test
			AccGLHeader badDebtAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "3980.00.00"));
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badDebtAccount.PK.ToGuid());

			bool defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesJournal.IsAllowed;
			try
			{
				ARJournal journal = Factory.New<ARJournal>();
				PayablesAndReceivablesWritingOff writingOff = new PayablesAndReceivablesWritingOff(journal);
				Env.Security.BadDebtWriteOffReceivablesJournal.IsAllowed = false;
				Assert("Should never allow writing off journal without security rights.", !writingOff.CanReverseTransaction);
				ZString expectedError = writingOff.HaventSecuryRightsErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);

				Env.Security.BadDebtWriteOffReceivablesJournal.IsAllowed = true;

				Guid defaultRegistryValue = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				Assert("Should never allow writing off journal without bad debt acount in registry.", !writingOff.CanReverseTransaction);
				expectedError = writingOff.HaventBadDebtAccountInRegistryErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultRegistryValue);

				journal.AH_InvoiceAmount = 10;
				journal.AH_OSTaxAmount = 1;
				journal.AH_OutstandingAmount = 10;

				Assert("Should never allow writing off partly paid transaction.", !writingOff.CanReverseTransaction);
				expectedError = writingOff.CantWriteOffPartiallyPaidTransactionErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);

				journal.AH_OSTaxAmount = 0;

				Assert("Should allow writing off.", writingOff.CanReverseTransaction);

				journal.AH_LocalTaxAmountOtherTaxes = 1;
				Assert("Should never allow writing off partly paid transaction.", !writingOff.CanReverseTransaction);
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);
			}
			finally
			{
				defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesJournal.IsAllowed;
			}
		}

		#region Implementation

		protected override Type GetTestingClassType()
		{
			return typeof(PayablesAndReceivablesWritingOff);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIBadDebtWritingOffTransaction();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIBadDebtWritingOffTransaction();
		}

		protected PayablesAndReceivablesWritingOff PayablesAndReceivablesWritingOff
		{
			get { return (PayablesAndReceivablesWritingOff)Reversing; }
		}

		protected virtual TestIBadDebtWritingOffTransaction TestWritingOffTransaction
		{
			get { return (TestIBadDebtWritingOffTransaction)TestIReversingInstance; }
		}

		protected override void SetupReversingObject()
		{
			BadDebtWritingOffFactory writingOffFactory = new BadDebtWritingOffFactory();
			Reversing = writingOffFactory.NewWritingOff((IBadDebtWritingOff)TestIReversingInstance);
		}

		#endregion
	}
}
