using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferChargeLine))]
	class BankTransferChargeLineTest : DirectPaymentLineTest
	{
		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(BankTransferCharge);
		}

		protected override Base.Transaction.TransactionLine CreateNewLine()
		{
			MasterHeader = (BankTransferCharge)Factory.New(MasterHeaderType);
			return MasterHeader.Lines[0];
		}

		public override void TestCalculateOSAmountsWhenZeroGST()
		{
			Assert("Test does not apply: this BusinessObject is never reloaded after saving.", true);
		}

		protected override void CoreTestForTestOverseasAmountsAreCalculatedCorrectly(bool companyIsReciprocal, RefCurrency currency, ZDecimal initialExchangeRate, ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount)
		{
			Assert("Test does not apply: this BusinessObject is never reloaded after saving.", true);
		}

		public override void TestOnSavingDateDifference()
		{
			Assert("Test does not apply: this BusinessObject is never reloaded after saving.", true);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BankTransferCharge testBankTransferCharge = factory.New<BankTransferCharge>();
			testBankTransferCharge.EnableFinanceCharge = true;
			BankTransferChargeLine testLine = (BankTransferChargeLine)testBankTransferCharge.Lines[0];

			return testLine;
		}

		protected override bool AcceptAL_AG
		{
			get { return false; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			BankTransferCharge testBankTransferCharge = Factory.New<BankTransferCharge>();
			testBankTransferCharge.EnableFinanceCharge = true;
			Line = (BankTransferChargeLine)testBankTransferCharge.Lines[0];
		}

		public override void TestTransactionLineFetchHints()
		{
			Assert(true);
		}

		public new void TestGSTandEDUSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestLineAH_OSExTaxAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineExTaxAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineLocalEDUAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineLocalGSTAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineLocalQSTAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineOSEDUAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineOSQSTAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineOSTaxAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestLineOSTotalAmountUpdatesHeaderTotal()
		{
			Assert("Not applicable", true);
		}
		public new void TestOTOSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}
		public new void TestQCTSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}
		public new void TestGSTandQSTSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}
		public new void TestGSTandRETSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}
		public new void TestVATandSPVSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestVATandSPVSplittingOnLoad_AL_GSTVATExtraIsPersistent()
		{
			Assert("Not applicable", true);
		}

		public void TestTransactionHeaderNotCreateNewObject()
		{
			var bankTransferCharge = Factory.New<BankTransferCharge>();
			var bizosBefore = Factory.GetBizOsForPK(bankTransferCharge.PK.ToGuid());
			AssertEquals(1, bizosBefore.Length);
			var pk = bankTransferCharge.Lines[0].TransactionHeader.PK;
			var bizosAfter = Factory.GetBizOsForPK(bankTransferCharge.PK.ToGuid());
			AssertContainsExactElementsInAnyOrder("We should not create a new AccTransactionHeader while accessing BankTransferChargeLine.TransactionHeader", bizosBefore, bizosAfter);
		}

		protected override bool IsExpectMultiSubAccountsSupported => false;
	}
}
