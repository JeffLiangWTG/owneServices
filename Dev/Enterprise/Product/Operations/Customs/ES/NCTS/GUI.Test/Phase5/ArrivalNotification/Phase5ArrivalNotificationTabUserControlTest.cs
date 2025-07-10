using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class Phase5ArrivalNotificationTabUserControlTest : TestCaseWithFactory
	{
		public void TestSummaryDeclarationGroupBox()
		{
			using (var userControl = new Phase5ArrivalNotificationTabUserControl())
			{
				var summaryDeclarationGroupBox = userControl.SummaryDeclarationGroupBox;
				var dynamicSummaryDeclarationLayoutPanel = userControl.DynamicSummaryDeclarationLayoutPanel;
				CombineAssertions(() =>
				{
					AssertEquals("SummaryDeclarationGroupBox Caption", "Summary Declaration", summaryDeclarationGroupBox.CaptionResourceString.Caption);
					AssertEquals("SummaryDeclarationGroupBox TabIndex", 3, summaryDeclarationGroupBox.TabIndex);
					AssertEquals("SummaryDeclarationGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left, summaryDeclarationGroupBox.Anchor);
					AssertEquals("DynamicSummaryDeclarationLayoutPanel is within SummaryDeclarationGroupBox", true, summaryDeclarationGroupBox.Controls.Contains(dynamicSummaryDeclarationLayoutPanel));
					AssertEquals("DynamicSummaryDeclarationLayoutPanel Dock", DockStyle.Fill, dynamicSummaryDeclarationLayoutPanel.Dock);
				});
			}
		}
	}
}
