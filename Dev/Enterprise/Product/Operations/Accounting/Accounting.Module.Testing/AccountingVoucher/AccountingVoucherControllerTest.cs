using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.Accounting.GUI.AccountingVoucherPrinting;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccountingVoucherController))]
	public class AccountingVoucherControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccountingVoucher;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new AccountingVoucherPrintWrapper();
		}

		public void TestGetForm()
		{
			using (IZForm testForm = TestController.GetForm_ForTestOnly(new AccountingVoucherPrintWrapper(Factory)))
			{
				AssertEquals(typeof(AccoutingVoucherPrintForm), testForm.GetType());
			}
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.AccountingVoucher, TestController.ID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AccountingVoucherPrintWrapper), TestController.TypeOfTopLevelBusinessObject);
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			AssertEquals(typeof(AccountingVoucherPrintWrapper), TestController.GetNewBusinessEntityInLocalFactory_ForTestOnly().GetType());
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.AccountingVoucher, TestController.CheckPointForNew_ForTestOnly);
		}

		public void TestDisplayModeForNew()
		{
			using (IZForm testForm = TestController.ShowNewForm())
			{
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
		}

		AccountingVoucherController fTestController;

		AccountingVoucherController TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new AccountingVoucherController();
				}
				return fTestController;
			}
		}
	}
}
