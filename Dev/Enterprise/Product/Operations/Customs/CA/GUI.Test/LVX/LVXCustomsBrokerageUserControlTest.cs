using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVXCustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestRemovingRedundantTabs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var lvxJobsControl = (LVXCustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				Assert(lvxJobsControl.InvoiceLinesTabPage.IsDisposed);
				Assert(lvxJobsControl.InvoicesTabPage.IsDisposed);
				Assert(lvxJobsControl.PackingTabPage.IsDisposed);
				Assert(lvxJobsControl.InvoiceGroupingTabPage.IsDisposed);
				Assert(lvxJobsControl.MiscOptionsTabPage.IsDisposed);
				Assert(lvxJobsControl.ContainerTabPage.IsDisposed);
				Assert(lvxJobsControl.MessagesTabPage.IsDisposed);
			}
		}

		public void TestHideGroupChargesGridForLVXJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var detailsTabControl = (ZTemplateTabControl)form.Controls.Find("DetailsTabControl", true)[0];
				var chargesTabPage = (ZTabPage)form.Controls.Find("ChargesTabPage", true)[0];
				detailsTabControl.SelectTab(chargesTabPage);
				var chargesSplitContainer = (KSplitContainer)form.Controls.Find("ChargesSplitContainer", true)[0];

				Assert("Group Charges grid should be hiden", chargesSplitContainer.Panel2Collapsed);
			}
		}

		public void TestRunMergeOnJobDeclarationSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.LVXInvoiceHeader.InvoiceLines.AddNew().JI_LinePrice = 10m;
			declaration.CA_RequiresMerge = true;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Assert("Merge was done", declaration.IsMergeDone);
				Assert("Merge was done", !declaration.CA_RequiresMerge);
			}
		}
	}
}
