using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	sealed class TransactionDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(CusIntrastatHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestOrganisationDetails()
		{
			var organisationDetailsPanel = userControl.DynamicOrganisationDetailsPanel;
			CombineAssertions(() =>
			{
				AssertEquals("TraderDetailsGroupBox Dock", DockStyle.Left, organisationDetailsPanel.Dock);
				AssertEquals("TraderDetailsGroupBox TabIndex", 0, organisationDetailsPanel.TabIndex);
			});
		}

		public void TestTransactionDetailsGroupBox()
		{
			var transactionDetailsGroupBox = userControl.TransactionDetailsGroupBox;
			var dynamicTransactionDetailsPanel = userControl.DynamicTransactionDetailsPanel;
			CombineAssertions(() =>
			{
				AssertEquals("TransactionDetailsGroupBox Caption", "Transaction Details", transactionDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("TransactionDetailsGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom, transactionDetailsGroupBox.Anchor);
				AssertEquals("TransactionDetailsGroupBox TabIndex", 1, transactionDetailsGroupBox.TabIndex);
				AssertEquals("DynamicTransactionDetailsPanel is within TransactionDetailsGroupBox", true, transactionDetailsGroupBox.Controls.Contains(dynamicTransactionDetailsPanel));
				AssertEquals("DynamicTransactionDetailsPanel Dock", DockStyle.Fill, dynamicTransactionDetailsPanel.Dock);
			});
		}

		public void TestOrganisationDetailsPanelLayout()
		{
			var transaction = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();
			userControl.SetDataBinding(transaction, "");
			var dynamicTraderDetailsPanel = userControl.DynamicOrganisationDetailsPanel;
			DynamicLayoutPanelTest.AssertControlsOrder(dynamicTraderDetailsPanel, nameof(OrganisationDetailsControlBag.SupplierOrganisationControl),
				nameof(OrganisationDetailsControlBag.ConsigneeOrganisationControl));
		}

		public void TestTransactionDetailsPanelLayout()
		{
			var transaction = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();

			userControl.SetDataBinding(transaction, "");
			var transactionDetailsPanel = userControl.DynamicTransactionDetailsPanel;
			DynamicLayoutPanelTest.AssertControlsOrder(transactionDetailsPanel,
				nameof(TransactionDetailsControlBag.SupplierNameTextBox),
				nameof(TransactionDetailsControlBag.SupplierVATTextBox),
				nameof(TransactionDetailsControlBag.ConsigneeNameTextBox),
				nameof(TransactionDetailsControlBag.ConsigneeVATTextBox),
				nameof(TransactionDetailsControlBag.CountryOfSupplyDropEdit),
				nameof(TransactionDetailsControlBag.CountryOfReceiptDropEdit),
				nameof(TransactionDetailsControlBag.TransactionDateEdit),
				nameof(TransactionDetailsControlBag.NatureOfTransactionDropEdit),
				nameof(TransactionDetailsControlBag.ModeOfTransportDropEdit),
				nameof(TransactionDetailsControlBag.TradersReferenceTextBox),
				nameof(TransactionDetailsControlBag.IncoTermDropEdit));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new TransactionDetailsTabUserControl();
		}
		TransactionDetailsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
