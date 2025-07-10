using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobManagement.Testing
{
	[TestedType(typeof(JobInvoicingForm))]
	public class JobInvoicingFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			return new JobInvoicingForm(job);
		}

		public void TestFormCaption()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			using (var testForm = new JobInvoicingForm(job))
			{
				testForm.Show();

				AssertEquals("Should be true", "Shipment S000001 Job Invoicing", testForm.FormCaption);
			}
		}

		public void TestActionMenuItemInvisible()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			using (var testForm = new JobInvoicingForm(job))
			{
				testForm.Show();

				var menuItem = testForm.Menu.MenuItems.FindByName("ActionsMenuItem", false);
				Assert("Should be invisible", menuItem == null || !menuItem.Visible);
			}
		}

		public void TestRecentItemIsRemoved()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			using (var testForm = new JobInvoicingForm(job))
			{
				testForm.ControllerID = ControllerIDs.JobInvoicingForm;
				testForm.Show();

				var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(testForm);
				AssertEquals("Job billing should not be saved to recent items from the beginning.", false, RecentItemManager.Instance.IsInRecentItems(linkWrapper.ModuleName, linkWrapper));

				testForm.Close();
				AssertEquals("Job billing should not be in recent items.", false, RecentItemManager.Instance.IsInRecentItems(linkWrapper.ModuleName, linkWrapper));
			}
		}
	}
}
