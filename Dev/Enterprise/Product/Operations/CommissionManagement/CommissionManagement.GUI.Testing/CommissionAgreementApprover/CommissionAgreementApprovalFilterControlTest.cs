using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI.Testing;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionAgreementApprovalFilterControlTest : TestCaseWithFactory
	{
		public void TestShouldPerformSearch()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement1.CA0_LastApprovedDateUtc = ZDateTime.Empty;

			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement2.CA0_LastApprovedDateUtc = ZDateTime.BrettsBirthday;

			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement3.CA0_LastApprovedDateUtc = ZDateTime.UtcNow;

			var agreement4 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement4.CA0_LastApprovedDateUtc = ZDateTime.Empty;

			var wizard = new CommissionAgreementApprovalWizard(Factory);

			using (var filterControl = new CommissionAgreementApprovalFilterControl(wizard, new CommissionAgreementApprovalFilterBusinessObject()))
			{
				filterControl.FirePerformSearch();

				var items = wizard.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>();
				AssertCollectionContains(items, item => item.CommissionAgreement.PK == agreement1.PK || item.CommissionAgreement.PK == agreement4.PK);
				AssertCollectionNotContains(items, item => item.CommissionAgreement.PK == agreement2.PK || item.CommissionAgreement.PK == agreement3.PK);
			}
		}

		public void TestHandleSearchPerformedEvent()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			var performSearchEventTriggered = false;

			using (var filterControl = new CommissionAgreementApprovalFilterControl(wizard, new CommissionAgreementApprovalFilterBusinessObject()))
			{
				filterControl.SearchPerformed += (o, e) => performSearchEventTriggered = true;
				filterControl.FirePerformSearch();

				AssertEquals("Search Performed Event Triggered", true, performSearchEventTriggered);
			}
		}

		public void TestMenuItemsVisibility()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			wizard.CommissionAgreementApprovalItemCollection.AddNew();

			using (var form = new ZForm())
			using (var filterControl = new CommissionAgreementApprovalFilterControl(wizard, new CommissionAgreementApprovalFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.FilteredGrid.ContextMenu.OnPopup_ForTest();

				var menuItems = filterControl.Grid.ContextMenu.MenuItems.Cast<MenuItem>().ToList();
				Assert("Export All Columns To Excel menu item should be visible.", menuItems.First(mi => mi.Text == "Export All Columns To Excel").Visible);
				Assert("Export Visible Columns To Excel menu item should be visible.", menuItems.First(mi => mi.Text == "Export Visible Columns To Excel").Visible);
				Assert("Mass Update Data menu item should be visible.", menuItems.First(mi => mi.Text == "&Mass Update Data...").Visible);
			}
		}
	}
}
