using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BankCurrencyAdjustmentController))]
	class BankCurrencyAdjustmentControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BankCurrencyAdjustment;
		}

		public void TestSecurityCheckPoints()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			AssertEquals("CheckPointForView", Env.Security.ViewCashBookBankCurrencyAdjustment, Controller.GetCheckPointForView(header));
			AssertEquals("CheckPointForNew", Env.Security.NewCashBookBankCurrencyAdjustment, Controller.GetCheckPointForNew(header));
			AssertEquals("CheckPointForEdit", Env.Security.ViewCashBookBankCurrencyAdjustment, Controller.GetCheckPointForEdit(header));
			AssertEquals("CheckPointForDelete", Env.Security.ReverseCashBookBankCurrencyAdjustment, Controller.GetCheckPointForDelete(header));
		}

		public override void TestNewForm()
		{
			using (var form = Controller.ShowNewForm())
			{
				AssertEquals(typeof(NewCashBookExchangeDiffForm), form.GetType());
			}
		}

		public override void TestViewForm()
		{
			using (var form = Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals(typeof(CashBookExchangeDiffForm), form.GetType());
			}
		}

		public void TestModuleID()
		{
			var controller = new BankCurrencyAdjustmentController();
			AssertEquals("ModuleID of BankCurrencyAdjustmentController is CashbookTransaction", ModuleIDs.CashbookTransaction, controller.ModuleID);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject bO = Factory.New(typeof(CashbookExchangeDiff));
			Factory.Save();
			return bO;
		}
	}
}
