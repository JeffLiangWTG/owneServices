using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.GUI.CashBook.BankReconciliation;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BankReconcilliationController))]
	public class BankReconcilliationControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BankReconcilliation;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new BankReconciliation(Factory);
		}

		public void TestGetForm()
		{
			using (IZForm testForm = TestController.GetForm_ForTestOnly(new BankReconciliation(Factory)))
			{
				AssertEquals(typeof(BankReconcilationForm), testForm.GetType());
			}
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.BankReconcilliation, TestController.ID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(BankReconciliation), TestController.TypeOfTopLevelBusinessObject);
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			AssertEquals(typeof(BankReconciliation), TestController.GetNewBusinessEntityInLocalFactory_ForTestOnly().GetType());
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.BankReconciliation, TestController.CheckPointForNew_ForTestOnly);
		}

		BankReconcilliationController fTestController;

		BankReconcilliationController TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new BankReconcilliationController();
				}
				return fTestController;
			}
		}
	}
}
