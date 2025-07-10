using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.Module.Testing
{
	public class StaffCommissionAgreementsPlugInTest : TestCaseWithFactory
	{
		#region DisableStaffCommissionAgreements

		public void TestDisableStaffCommissionAgreementsMenuItem()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";

			using (var form = new GlbStaffForm(staff))
			{
				form.Show();

				IFileMenuItemsProvider formMenuItemsProvider = form;
				var disableStaffCommissionAgreementsMenuItem = formMenuItemsProvider.ActionsMenuItem.MenuItems.Cast<MenuItem>().First(x => x.Text == "Disable Staff Commission Agreements");
				disableStaffCommissionAgreementsMenuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Cannot disable commission agreements", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Text", "Staff does not have any commission agreements.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				var agreement = Factory.New<OrgCommissionAgreement>();
				var recipient = agreement.Recipients.AddNew();
				recipient.CAR_GS_NKStaff = "ADL";

				disableStaffCommissionAgreementsMenuItem.PerformClick();
				using (var lastFormShown = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(DisableStaffCommissionAgreementsForm), lastFormShown);
				}
			}
		}

		public void TestShowDisableStaffCommissionAgreementsFormOnDeactivate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_IsActive = true;

			Factory.Save();

			using (var form = new GlbStaffForm(staff))
			{
				form.Show();

				staff.GS_IsActive = false;
				using (var lastFormShown = ZFormModaliser.LastFormShownForTest)
				{
					AssertNull("Should not show form if staff does not have any commission agreements", lastFormShown);
				}

				var agreement = Factory.New<OrgCommissionAgreement>();
				var recipient = agreement.Recipients.AddNew();
				recipient.CAR_GS_NKStaff = "ADL";

				staff.GS_IsActive = true;
				using (var lastFormShown = ZFormModaliser.LastFormShownForTest)
				{
					AssertNull("Should only show form when deactivating staff (not activating)", lastFormShown);
				}

				staff.GS_IsActive = false;
				using (var lastFormShown = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(DisableStaffCommissionAgreementsForm), lastFormShown);
				}
			}
		}

		#endregion
	}
}
