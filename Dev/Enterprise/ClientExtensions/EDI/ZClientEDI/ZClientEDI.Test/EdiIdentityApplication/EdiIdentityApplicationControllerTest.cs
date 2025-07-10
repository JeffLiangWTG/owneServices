using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityApplication.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplication.Module.Testing
{
	[TestedType(typeof(EdiIdentityApplicationController))]
	public class EdiIdentityApplicationControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.EdiIdentityApplication;
		}

		public void TestGetForm()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			Factory.Save();

			using var form = Controller.ShowEditForm(application);
			AssertEquals(typeof(EdiIdentityApplicationForm), form.GetType());

			var customerApp = Factory.New<EdiIdentityApplication>();
			Factory.Save();

			using var customerForm = Controller.ShowEditForm(customerApp);
			AssertEquals(typeof(EdiIdentityCustomerApplicationForm), customerForm.GetType());
		}

		public void TestShowNewCustomerApplicationForm()
		{
			var controller = (EdiIdentityApplicationController)Controller;
			using var form = controller.ShowNewCustomerApplicationForm();
			AssertEquals(typeof(EdiIdentityCustomerApplicationForm), form.GetType());
		}
	}
}
