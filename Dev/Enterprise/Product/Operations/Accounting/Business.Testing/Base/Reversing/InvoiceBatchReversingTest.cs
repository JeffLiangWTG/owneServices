using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class InvoiceBatchReversingTest : ReversingBaseTest
	{
		public void TestCancelMatchedInvoice()
		{
			InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			batchHeader.AH_IsCancelled = ZBool.True;
			ARInvoice aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice.AH_OSExTaxAmount = 120m;
			aRInvoice.AH_OutstandingAmount = 0m;
			batchHeader.Line.Add(aRInvoice);

			Reversing = ReversingFactory.NewReversing(batchHeader);
			AssertEquals("CanReverseTransaction when transaction is related to claim", false, InvoiceBatchReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when transaction is related to claim", InvoiceBatchReversing.CantReverseBatchIfSomeTransactionIsAlreadyMatchedErrorMessage_ForTestOnly, InvoiceBatchReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public override void TestTransactionReversedEmailSentOnSaving()
		{
			SetupStaffMemberEmailAddress();
			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, StaffGroupPK.ToGuid());

			TestIReversingInstance.Factory.Save();

			AssertEquals("Should not have sent email because reversing transactions were not setup correctly",
				0, Env.OutgoingMailManager.EmailsCreated.Count);

			TestIReversingInstance.SetReverseTransactionToBeGenerated(TestReversingIReversingInstance);
			TestIReversingInstance.Factory.Save();

			AssertEquals("Should have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected override Type GetTestingClassType()
		{
			return typeof(InvoiceBatchReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = Factory.NewWithValidTestData<InvoiceBatchHeaderForReversingTest>();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = Factory.NewWithValidTestData<InvoiceBatchHeaderForReversingTest>();
		}

		InvoiceBatchReversing InvoiceBatchReversing
		{
			get { return (InvoiceBatchReversing)Reversing; }
		}

		class InvoiceBatchHeaderForReversingTest : InvoiceBatchHeader, IReversingForTests
		{
			public InvoiceBatchHeaderForReversingTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IReversingForTests Members

			public void SetIsClearedInCashbook(bool value)
			{
				AH_DateClearedInCashbook = value ? ZDateTime.Now : ZDateTime.Empty;
			}

			public void SetIsReversed(bool value)
			{
				AH_IsCancelled = value;
			}

			public void SetReverseTransactionToBeGenerated(IReversing reverseTransaction)
			{
				fReverseTransaction = (TransactionHeader)reverseTransaction;
			}

			#endregion

			#region ITransactionForTests Members

			public void SetFactory(BusinessObjectFactory factory)
			{
			}

			public ZDateTime FullyPaidDate
			{
				get
				{
					return AH_FullyPaidDate;
				}
				set
				{
					AH_FullyPaidDate = value;
				}
			}

			#endregion
		}
	}
}
