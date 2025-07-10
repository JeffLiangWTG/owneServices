using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.Customs.EU.Intrastat.GUI.Transactions;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	sealed class TransactionLineDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(ICusIntrastatLineCollection<CusIntrastatLine>), userControl.BindingSource.DataSourceType);
		}

		public void TestTransactionLinesSplitContainer()
		{
			var splitContainer = userControl.TransactionLinesSplitContainer;
			AssertEquals("Orientation", Orientation.Horizontal, splitContainer.Orientation);

			AssertEquals(175, splitContainer.Panel1MinSize);
			AssertEquals(175, splitContainer.Panel2MinSize);
			AssertEquals("Dock", DockStyle.Fill, splitContainer.Dock);
		}

		public void TestTransactionLineTabControl()
		{
			var tabControl = userControl.TransactionLineTabControl;
			CombineAssertions(() =>
			{
				AssertEquals("TransactionLineTabControl is within TransactionLinesSplitContainer.Panel2", true, userControl.TransactionLinesSplitContainer.Panel2.Contains(tabControl));
				AssertEquals("Dock", DockStyle.Fill, tabControl.Dock);
			});
		}

		public void TestTransactionLineDetailsTabPage()
		{
			var tabPage = userControl.TransactionLineDetailsTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("TransactionLineDetailsTabPage is within TransactionLineTabControl", true, userControl.TransactionLineTabControl.Contains(tabPage));
				AssertEquals("Details", "Details", tabPage.CaptionResourceString.Caption);
			});
		}

		public void TestTransactionLineDetailsPanelLayout()
		{
			var transactionLine = IntrastatTestDataHelper.New(Factory).NewCusIntrastatLineWithValidData();

			using (var form = new IntrastatTransactionForm(transactionLine.Header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.LinesTabPage);
				var control = form.TransactionLineDetailsTabUserControl;
				var transactionLineDetailsPanel = control.DynamicTransactionLineDetailsPanel;
				DynamicLayoutPanelTest.AssertControlsOrder(transactionLineDetailsPanel,
					nameof(TransactionLineDetailsControlBag.DescriptionOfGoodsTextBox),
					nameof(TransactionLineDetailsControlBag.InvoiceValueDropEdit),
					nameof(TransactionLineDetailsControlBag.TariffFindBox),
					nameof(TransactionLineDetailsControlBag.StatisticalValueDropEdit),
					nameof(TransactionLineDetailsControlBag.CountryOfOriginDropEdit),
					nameof(TransactionLineDetailsControlBag.MassDropEdit),
					nameof(TransactionLineDetailsControlBag.RegionDropEdit),
					nameof(TransactionLineDetailsControlBag.SupplementaryUnitsCalcDropEdit));
			}
		}

		public void TestTransactionLinesGridUserControl()
		{
			var transaction = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();

			userControl.SetDataBinding(transaction, nameof(CusIntrastatHeader.CusIntrastatLines));

			var gridUserControl = userControl.FindSingle<TransactionLinesGridUserControl>();

			AssertEquals("TransactionLinesGridUserControl is within TransactionLinesSplitContainer.Panel1", true, userControl.TransactionLinesSplitContainer.Panel1.Contains(gridUserControl));
			AssertEquals("Dock", DockStyle.Fill, gridUserControl.Dock);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new TransactionLineDetailsTabUserControl();
		}
		TransactionLineDetailsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
