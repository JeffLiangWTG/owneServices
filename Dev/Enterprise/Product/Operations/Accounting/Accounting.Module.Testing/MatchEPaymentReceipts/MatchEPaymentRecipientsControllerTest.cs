using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(MatchEPaymentRecipientsController))]
	class MatchEPaymentRecipientsControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new MatchEPaymentRecipientsController();
			AssertNull(controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(MatchEPaymentRecipients);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MatchEPaymentRecipients;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			return new MatchEPaymentRecipients(Factory, accountDetails);
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
				AssertEquals(typeof(MatchEPaymentRecipientsForm), form.GetType());
			}
		}
	}
}
