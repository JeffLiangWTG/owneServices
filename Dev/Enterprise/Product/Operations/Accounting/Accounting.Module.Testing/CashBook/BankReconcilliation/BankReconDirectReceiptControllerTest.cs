using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BankReconDirectReceiptController))]
	class BankReconDirectReceiptControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override BusinessObject ParentTransactionHeaderRow => DirectReceipt;

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BankReconDirectReceipt;
		}

		protected override string GetExpectedCantReverseMesaage
		{
			get { return "This transaction cannot be reversed because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing."; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			DirectReceipt = TestObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();
			DirectReceipt.AH_DateClearedInCashbook = ZDateTime.Today;
			Factory.Save();
		}

		TestObjectCreator testObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected BankReconDirectReceipt DirectReceipt;
	}
}
