using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BankReconDirectPaymentController))]
	class BankReconDirectPaymentControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override BusinessObject ParentTransactionHeaderRow => DirectPayment;

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BankReconDirectPayment;
		}

		protected override string GetExpectedCantReverseMesaage
		{
			get { return "This transaction cannot be reversed because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing."; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			DirectPayment = TestObjectCreator.CreateCashBookTransaction<BankReconDirectPayment>();
			DirectPayment.AH_DateClearedInCashbook = ZDateTime.Today;
			Factory.Save();
		}

		TestObjectCreator testObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected BankReconDirectPayment DirectPayment;
	}
}
