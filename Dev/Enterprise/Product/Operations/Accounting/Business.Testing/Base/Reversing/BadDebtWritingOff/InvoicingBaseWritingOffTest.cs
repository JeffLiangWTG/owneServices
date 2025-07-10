using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff.Testing
{
	class InvoicingBaseWritingOffTest : PayablesAndReceivablesWritingOffTest
	{
		public void TestCantReverseInvoicingBase()
		{
			// Test DB will not have the StmData base data, which contains the default value for bad debt for the registry.
			// So add bad debt into the registry for this test
			AccGLHeader badDebtAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "3980.00.00"));
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badDebtAccount.PK.ToGuid());

			bool defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed;
			try
			{
				ARInvoice invoice = Factory.New<ARInvoice>();
				InvoicingBaseWritingOff writingOff = new InvoicingBaseWritingOff(invoice);
				Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed = false;
				Assert("Should never allow writing off invoice without security rights.", !writingOff.CanReverseTransaction);
				ZString expectedError = writingOff.HaventSecuryRightsErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);

				Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed = true;

				Guid defaultRegistryValue = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				Assert("Should never allow writing off invoice without bad debt acount in registry.", !writingOff.CanReverseTransaction);
				expectedError = writingOff.HaventBadDebtAccountInRegistryErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultRegistryValue);

				invoice.AH_InvoiceAmount = 10;
				invoice.AH_OSTaxAmount = 1;
				invoice.AH_OutstandingAmount = 10;

				Assert("Should never allow writing off partly paid transaction.", !writingOff.CanReverseTransaction);
				expectedError = writingOff.CantWriteOffPartiallyPaidTransactionErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);
			}
			finally
			{
				defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed;
			}

			defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesCreditNote.IsAllowed;
			try
			{
				ARCreditNote creditNote = Factory.New<ARCreditNote>();
				InvoicingBaseWritingOff writingOff = new InvoicingBaseWritingOff(creditNote);
				Env.Security.BadDebtWriteOffReceivablesCreditNote.IsAllowed = false;
				Assert("Should never allow writing off credit note without security rights.", !writingOff.CanReverseTransaction);
				ZString expectedError = writingOff.HaventSecuryRightsErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);

				Env.Security.BadDebtWriteOffReceivablesCreditNote.IsAllowed = true;

				Guid defaultRegistryValue = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				Assert("Should never allow writing off credit note without bad debt acount in registry.", !writingOff.CanReverseTransaction);
				expectedError = writingOff.HaventBadDebtAccountInRegistryErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultRegistryValue);

				creditNote.AH_InvoiceAmount = 10;
				creditNote.AH_OSTaxAmount = 1;
				creditNote.AH_OutstandingAmount = 10;

				Assert("Should never allow writing off partly paid transaction.", !writingOff.CanReverseTransaction);
				expectedError = writingOff.CantWriteOffPartiallyPaidTransactionErrorMessage_ForTestOnly;
				AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);
			}
			finally
			{
				defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesCreditNote.IsAllowed;
			}
		}

		public void TestCantReverseInvoicingBase_ARCreditNoteDisabled()
		{
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AccGLHeader badDebtAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "3980.00.00"));
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badDebtAccount.PK.ToGuid());

			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			var creditNote = (ARCreditNote)TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);

			InvoicingBaseWritingOff writingOff = new InvoicingBaseWritingOff(invoice);
			Assert("Should not allow writing off as AR Credit Note is disabled", !writingOff.CanReverseTransaction);
			var expectedError = "Write Off As Bad Debt is not permitted. Receivables Invoice Transactions cannot be reversed. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Reversal of Invoice Transactions.";
			AssertEquals("Can't reverse error", expectedError, writingOff.CantReverseErrorMessage);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert("Should allow writing off as Reversing of AR Credit Note is permitted", writingOff.CanReverseTransaction);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			writingOff = new InvoicingBaseWritingOff(creditNote);
			Assert("Should allow writing off as Reversing of AR Credit Note is permitted, but Reversing of AR Invoice is not permitted", writingOff.CanReverseTransaction);
		}

		#region Implementation

		protected override Type GetTestingClassType()
		{
			return typeof(InvoicingBaseWritingOff);
		}

		protected InvoicingBaseWritingOff InvoicingBaseWritingOff
		{
			get { return (InvoicingBaseWritingOff)Reversing; }
		}

		protected override void SetupReversingObject()
		{
			Reversing = new InvoicingBaseWritingOff((IBadDebtWritingOff)TestIReversingInstance);
		}

		#endregion
	}
}
