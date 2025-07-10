using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChequeTransactionController))]
	public class ChequeTransactionControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestViewForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestEditForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestTemplateCopyForm()
		{
			Assert("Currently not support", true);
		}

		public void TestChequeTransactionController()
		{
			var controller = new ChequeTransactionController();
			Controller = controller;

			AssertEquals(ModuleIDs.ChequeTransaction, controller.ModuleID);
			Assert(controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.ChequeTransactionHeader;
	}
}
