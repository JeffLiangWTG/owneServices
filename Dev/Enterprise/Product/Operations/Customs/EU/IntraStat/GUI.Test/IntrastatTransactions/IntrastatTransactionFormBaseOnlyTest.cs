using System.Windows.Forms;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.Customs.EU.Intrastat.GUI.Transactions;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(IntrastatTransactionForm))]
	sealed class IntrastatTransactionFormBaseOnlyTest : IntrastatTransactionFormAbstractTest<CusIntrastatHeader>
	{
		public void TestFormCaption()
		{
			using (var form = new IntrastatTransactionForm(transaction))
			{
				AssertEquals("Intrastat - Transaction", form.FormCaption);
			}
		}

		public void TestMinimumSize()
		{
			using (var form = new IntrastatTransactionForm(transaction))
			{
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true), form.MinimumSize);
			}
		}

		public void TestMainTabPage()
		{
			using (var form = new IntrastatTransactionForm(transaction))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = mainTabControl.GetTabPage("MainTabPage");

				CombineAssertions(() =>
				{
					var detailsTabUserControl = form.FindSingle<TransactionDetailsTabUserControl>("TransactionDetailsTabUserControl");

					AssertEquals("TransactionDetailsTabUserControl is within MainTabPage", true, mainTabPage.Contains(detailsTabUserControl));
					AssertEquals("TransactionDetailsTabUserControl docked to fill", DockStyle.Fill, detailsTabUserControl.Dock);
					AssertEquals("Caption", "Header", mainTabPage.CaptionResourceString.Caption);
				});
			}
		}

		public void TestLinesTabPage()
		{
			using (var form = new IntrastatTransactionForm(transaction))
			{
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var linesTabPage = mainTabControl.GetTabPage("LinesTabPage");

				CombineAssertions(() =>
				{
					var detailsTabUserControl = form.FindSingle<TransactionLineDetailsTabUserControl>("TransactionLineDetailsTabUserControl");

					AssertEquals("TransactionLineDetailsTabUserControl is within LinesTabPage", true, linesTabPage.Contains(detailsTabUserControl));
					AssertEquals("TransactionLineDetailsTabUserControl docked to fill", DockStyle.Fill, detailsTabUserControl.Dock);
					AssertEquals("Caption", "Lines", linesTabPage.CaptionResourceString.Caption);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			transaction = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();
		}

		CusIntrastatHeader transaction;
	}
}
