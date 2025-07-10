using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ItemTabControl))]
	public class EUH7ItemTabControlTest : TestCaseWithFactory
	{
		public void TestEUH7ItemAdditionalTabPageVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var itemDetailsUserControl = itemTabpage.FindSingle<EUH7ItemTabControl>();

				CombineAssertions("All additional tabs should be visible", () =>
				{
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_EUH7ItemPacksUserControl", true).FirstOrDefault()).TabVisible);
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_SupportingDocumentsUserControl", true).FirstOrDefault()).TabVisible);
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_AdditionalDocumentsUserControl", true).FirstOrDefault()).TabVisible);
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_PreviousDocumentsUserControl", true).FirstOrDefault()).TabVisible);
				});
			}

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var billsAndPacksTabControl = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var itemTabpage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_EUH7ItemUserControl");
				billsAndPacksTabControl.SelectedTab = itemTabpage;

				var itemDetailsUserControl = itemTabpage.FindSingle<EUH7ItemTabControl>();

				CombineAssertions("All additional tabs should be visible", () =>
				{
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_EUH7ItemPacksUserControl", true).FirstOrDefault()).TabVisible);
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_SupportingDocumentsUserControl", true).FirstOrDefault()).TabVisible);
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_AdditionalDocumentsUserControl", true).FirstOrDefault()).TabVisible);
					AssertEquals(true, ((ZTabPage)itemDetailsUserControl.Controls.Find("ItemDetailsTabControl_TabPage_PreviousDocumentsUserControl", true).FirstOrDefault()).TabVisible);
				});
			}
		}
	}
}
