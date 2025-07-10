using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	[TestedType(typeof(JobConsolCostingForm))]
	public class JobConsolCostingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObjectWithNavigationProvider>();
			Factory.Save();

			return new JobConsolCostingForm(dummyBizo);
		}

		public void TestFormCaption()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObjectWithNavigationProvider>();
			Factory.Save();

			using (var form = new JobConsolCostingForm(dummyBizo))
			{
				form.Show();
				AssertEquals("Should be true", "DummyBizo Job Consol Costing", form.FormCaption);
			}
		}

		public void TestDisabledAction()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObjectWithNavigationProvider>();
			Factory.Save();

			using (var form = new JobConsolCostingForm(dummyBizo))
			{
				form.Show();
				var menuItems = form.Menu.MenuItems;

				CombineAssertions(() =>
				{
					AssertNotNull("This form should have a menu of File.", menuItems.FindByText("File", true));
					AssertNotNull("This form should have a menu of Edit.", menuItems.FindByText("Edit", true));
					AssertNotNull("This form should have a menu of Costing.", menuItems.FindByText("Costing", true));
					AssertNotNull("This form should have a menu of Help.", menuItems.FindByText("Help", true));
					AssertNull("This form should not have a menu of Action.", menuItems.FindByText("Action", true));
				});
			}
		}

		public void TestDisabledMenuItems()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObjectWithNavigationProvider>();
			Factory.Save();

			using (var form = new JobConsolCostingForm(dummyBizo))
			{
				form.Show();
				var menuItems = form.Menu.MenuItems;

				CombineAssertions("The Job Consol Costing form should not have the following menu items.", () =>
				{
					AssertNull(menuItems.FindByText(Core.Constants.MenuNameConstants.PostOverseasAgentCharges, true));
					AssertNull(menuItems.FindByText(Core.Constants.MenuNameConstants.ApportionRevenueToShipments, true));
					AssertNull(menuItems.FindByText(Core.Constants.MenuNameConstants.PreviewInvoices, true));
					AssertNull(menuItems.FindByText(Core.Constants.MenuNameConstants.ImportAPInvoicesIssuedByOtherGroupCompanies, true));
					AssertNull(menuItems.FindByText(Core.Constants.MenuNameConstants.RedefaultJobBillingExchangeRate, true));
					AssertNull(menuItems.FindByText(Core.Constants.MenuNameConstants.ResetUnpostedLinesTaxDefault, true));
				});
			}
		}

		public void TestRemoveTabPages()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObjectWithNavigationProvider>();
			Factory.Save();

			using (var form = new JobConsolCostingForm(dummyBizo))
			{
				form.Show();

				var tabPages = form.FindAll<ZTabPage>();

				CombineAssertions("The Job Consol Costing form should not have the following tab controls.", () =>
				{
					AssertEquals(true, tabPages.Any(t => t.Text == "Consol Costing"));
					AssertEquals(false, tabPages.Any(t => t.Text == "Details"));
					AssertEquals(false, tabPages.Any(t => t.Text == "Logs"));
				});
			}
		}

		public void TestHideControlAndUnavailableColumn()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObjectWithNavigationProvider>();
			Factory.Save();

			using (var form = new JobConsolCostingForm(dummyBizo))
			{
				form.Show();

				var checkBox = form.Find(control => control is ZCheckBox && control.Text.Equals("Include on Collect Invoice")).First();
				AssertEquals("This form should hide the checkBox of Include on Collect Invoice.", false, checkBox.Visible);

				var isForCollectInvoice = form.Find(control => control is DataGridTextBox && control.Name.Equals(JobConsolCost.Schema.E6_IsForCollectInvoice));
				AssertEquals("This form can't find the column of E6_IsForCollectInvoice.", 0, isForCollectInvoice.Count());
			}
		}
	}
}
