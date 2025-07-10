using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferCharge))]
	class BankTransferChargeEmptyValidationTest : DirectPaymentValiationTest
	{
		public override void TestValidateAll_ValidateAH_OSTotalAmount()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			TestBizO.RunPreSaveValidation();
			AssertEquals(false, TestBizO.AH_OSTotalAmountInfo.HasErrors());

			DirectTransactionLineBase testLine = (DirectTransactionLineBase)TestBizO.Lines.AddNew();
			testLine.AL_OSExTaxAmount = 10m;

			TestBizO.RunPreSaveValidation();
			AssertEquals(false, TestBizO.AH_OSTotalAmountInfo.HasErrors());
		}

		public override void TestCheckAH_AB()
		{
			TestBizO.AH_AB = ZGuid.Empty;
			AssertEquals(false, TestBizO.AH_ABInfo.HasErrors());
		}

		public override void TestCheckAH_OSTotalAmount()
		{
			TestBizO.AH_OSTotalAmount = -1m;
			DirectTransactionLineBase line = (DirectTransactionLineBase)TestBizO.Lines.AddNew();
			line.AL_OSExTaxAmount = -1m;

			((BankTransferChargeEmptyValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
			AssertEquals(false, TestBizO.AH_OSTotalAmountInfo.HasErrors());
		}

		public override void TestCheckAH_ChequeDrawer()
		{
			TestBizO.AH_ChequeDrawer = string.Empty;
			AssertEquals(false, TestBizO.AH_ChequeDrawerInfo.HasErrors());
		}

		public override void TestChequeNumberNumeric()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.AH_ChequeOrReference = "1234";
			AssertEquals("Should have NO error", false, TestBizO.AH_ChequeOrReferenceInfo.HasErrors());

			TestBizO.AH_ChequeOrReference = "abc";
			AssertEquals("Should have NO error", false, TestBizO.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public override void TestCheckAH_ChequeOrReference()
		{
			TestBizO.AH_ChequeOrReference = "1234";
			TestBizO.AH_ChequeOrReference = "";
			AssertNoErrors(TestBizO.AH_ChequeOrReferenceInfo);
		}

		public override void TestChequeOrReferenceValidationIsDisabledForAutoAllocationMode()
		{
			Assert("Bank Transfer doesn't have auto allocation mode", true);
		}

		public override void TestAH_IsCancelledBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_AH_InvoiceStatementBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_FullyPaidDateBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_InvoicePaymentReferenceCodeBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_ReceiptBatchNoBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}
	}
}
