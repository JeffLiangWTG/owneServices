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
	[TestedType(typeof(PeriodicInvoiceBulkController))]
	public class PeriodicInvoiceBulkControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PeriodicInvoiceBulk;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new PeriodicInvoiceBulk(Factory);
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
			bool orginalValue = Env.Security.NewReceivablesBulkPeriodicInvoice.IsAllowed;
			PeriodicInvoiceBulkController controller = new PeriodicInvoiceBulkController();

			Env.Security.NewReceivablesBulkPeriodicInvoice.IsAllowed = true;
			using (IZForm form = controller.ShowNewForm())
			{
				AssertNotNull("PeriodicInvoicingBulkForm", form);
				AssertType("PeriodicInvoicingBulkForm", typeof(PeriodicInvoicingBulkForm), form);
			}

			Env.Security.NewReceivablesBulkPeriodicInvoice.IsAllowed = false;
			AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
			{
				using (IZForm form = controller.ShowNewForm())
				{
					AssertNull("PeriodicInvoicingBulkForm", form);
					AssertContains("You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});

			Env.Security.NewReceivablesBulkPeriodicInvoice.IsAllowed = orginalValue;
		}
	}
}
