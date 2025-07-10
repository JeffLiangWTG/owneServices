using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(MatchEPaymentRecipientsBulkController))]
	class MatchEPaymentRecipientsBulkControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new MatchEPaymentRecipientsBulkController();
			AssertNull(controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(MatchEPaymentRecipientsBulk);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MatchEPaymentRecipientsBulk;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new MatchEPaymentRecipientsBulk(Factory);
		}

		public override void TestDeleteForm()
		{
			Assert("Not supported", true);
		}

		public override void TestEditForm()
		{
			Assert("Not supported", true);
		}

		public override void TestViewForm()
		{
			Assert("Not supported", true);
		}

		public override void TestNewForm()
		{
			using (var form = Controller.ShowNewForm())
			{
				AssertEquals(typeof(MatchEPaymentRecipientsBulkForm), form.GetType());
			}
		}
	}
}
