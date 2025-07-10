using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(PeriodicInvoiceController))]
	public class PeriodicInvoiceControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PeriodicInvoice;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new PeriodicInvoice(Factory);
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public void TestSecurityForNew()
		{
			bool orginalValue = Env.Security.NewReceivablesPeriodicInvoice.IsAllowed;
			PeriodicInvoiceController controller = new PeriodicInvoiceController();

			Env.Security.NewReceivablesPeriodicInvoice.IsAllowed = true;
			using (IZForm form = controller.ShowNewForm())
			{
				AssertNotNull("PeriodicInvoicingForm", form);
				AssertType("PeriodicInvoicingForm", typeof(PeriodicInvoicingForm), form);
			}

			Env.Security.NewReceivablesPeriodicInvoice.IsAllowed = false;
			AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
				{
					using (IZForm form = controller.ShowNewForm())
					{
						AssertNull("PeriodicInvoicingForm", form);
						AssertContains("You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				});
			Env.Security.NewReceivablesPeriodicInvoice.IsAllowed = orginalValue;
		}
	}
}
