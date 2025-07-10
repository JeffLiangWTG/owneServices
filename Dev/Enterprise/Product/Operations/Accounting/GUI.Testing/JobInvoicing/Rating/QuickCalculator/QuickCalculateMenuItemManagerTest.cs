using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class QuickCalculateMenuItemManagerTest : TestCaseWithFactory
	{
		public void TestQuickCalculateForJobConsolCost_CurrencyDoestExistInRefCurrency_ThenShouldShowMessageAndNotSendingErrorReporter()
		{
			TestQuickCalculateForJobConsolCost(currencyCode: "???", expectJobChargeQuickCalculateForm: false);
		}

		public void TestQuickCalculateForJobConsolCost_CurrencyExistInRefCurrency_ThenShouldShowQuickCalculateForm()
		{
			TestQuickCalculateForJobConsolCost(currencyCode: "USD", expectJobChargeQuickCalculateForm: true);
		}

		void TestQuickCalculateForJobConsolCost(string currencyCode, bool expectJobChargeQuickCalculateForm)
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var apportionmentListing = new ApportionmentListing(Factory, forwardingConsol);

			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_RX_NKCurrency = currencyCode;

			using (var consolidationsForm = new ZForm(apportionmentListing))
			using (var costSummaryGrid = new ZGrid())
			{
				costSummaryGrid.BindTo = "CostsCollection";
				costSummaryGrid.Columns.AddTextColumn("E6_Description", 100);
				consolidationsForm.Controls.Add(costSummaryGrid);
				consolidationsForm.Show();
				costSummaryGrid.SetDataBinding(apportionmentListing, "CostsCollection");

				new QuickCalculateMenuItemManager(costSummaryGrid, forwardingConsol).AddMenuItem();
				var quickCalculateMenuItem = costSummaryGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull("'Quick Calculate' menu item should exist", quickCalculateMenuItem);

				costSummaryGrid.CurrentRowIndex = 0;
				costSummaryGrid.Select(0);
				quickCalculateMenuItem.PerformClick();

				var jobChargeQuickCalculateForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;
				if (expectJobChargeQuickCalculateForm)
				{
					AssertNotNull(jobChargeQuickCalculateForm);
				}
				else
				{
					AssertNull(jobChargeQuickCalculateForm);
					AssertEquals("Please enter Cost Currency before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestQuickCalculateForJobConsolCost_RatingBehaviourSpot_ThenShouldShowQuickCalculateFormWithCostRate()
		{
			QuickCalculateForJobConsolCost_RatingBehaviour_ShowQuickCalculateFormWithCostRateIfSpot(true);
		}

		public void TestQuickCalculateForJobConsolCost_RatingBehaviourDefault_ThenShouldShowQuickCalculateFormWithNoCostRate()
		{
			QuickCalculateForJobConsolCost_RatingBehaviour_ShowQuickCalculateFormWithCostRateIfSpot(false);
		}

		void QuickCalculateForJobConsolCost_RatingBehaviour_ShowQuickCalculateFormWithCostRateIfSpot(bool isSpotRate)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 10;

			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_RX_NKCurrency = "AUD";
			jobConsolCost.E6_RatingBehaviour = isSpotRate ? "SPT" : "";
			jobConsolCost.E6_AC_ChargeCode = Factory.New<AccChargeCode>().PK;
			var paymentBasis = jobConsolCost.PaymentBases.AddNew();
			paymentBasis.PBS_PerUnitRate = 500;
			paymentBasis.PBS_RateUnit = "KG";
			paymentBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);
			Factory.Save();

			using (var consolidationsForm = new ZForm(apportionmentListing))
			using (var costSummaryGrid = new ZGrid())
			{
				costSummaryGrid.BindTo = "CostsCollection";
				costSummaryGrid.Columns.AddTextColumn("E6_Description", 100);
				consolidationsForm.Controls.Add(costSummaryGrid);
				consolidationsForm.Show();
				costSummaryGrid.SetDataBinding(apportionmentListing, "CostsCollection");

				new QuickCalculateMenuItemManager(costSummaryGrid, consol).AddMenuItem();
				var quickCalculateMenuItem = costSummaryGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");

				quickCalculateMenuItem.PerformClick();

				var jobChargeQuickCalculateForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;
				var jobChargeQuickCalculateObject = jobChargeQuickCalculateForm.BusinessEntity as JobChargeQuickCalculateBusinessObject;
				jobChargeQuickCalculateObject.QuantityDescription = "Chargeable";
				AssertEquals("Quantity shown from chargeable", 10, (int)jobChargeQuickCalculateObject.Quantity);
				AssertEquals("Cost rate shown from job consol cost", isSpotRate ? 500 : 0, (int)jobChargeQuickCalculateObject.CostRate);
				AssertEquals("Total cost calculated", isSpotRate ? 5000 : 0, (int)jobChargeQuickCalculateObject.CostTotal);

				AssertEquals("Sell rate defaults to 0", 0, (int)jobChargeQuickCalculateObject.SellRate);
				AssertEquals("Sell total unmodified", 0, (int)jobChargeQuickCalculateObject.SellTotal);
			}
		}

		public void TestQuickCalculateForJobConsolCost_RatingBehaviourSpot_ThenShouldShowQuickCalculateFormWithContainerCostRate()
		{
			QuickCalculateForJobConsolCost_RatingBehaviour_ShowQuickCalculateFormWithContainerCostRateIfSpot(true);
		}

		public void TestQuickCalculateForJobConsolCost_RatingBehaviourDefault_ThenShouldShowQuickCalculateFormWithNoContainerCostRate()
		{
			QuickCalculateForJobConsolCost_RatingBehaviour_ShowQuickCalculateFormWithContainerCostRateIfSpot(false);
		}

		void QuickCalculateForJobConsolCost_RatingBehaviour_ShowQuickCalculateFormWithContainerCostRateIfSpot(bool isSpotRate)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 10;

			consol.AddContainer("20GP", "GEN", 2);
			consol.AddContainer("40GP", "GEN", 1);

			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_RX_NKCurrency = "AUD";
			jobConsolCost.E6_RatingBehaviour = isSpotRate ? "SPT" : "";
			jobConsolCost.E6_AC_ChargeCode = Factory.New<AccChargeCode>().PK;
			var paymentBasis = jobConsolCost.PaymentBases.AddNew();
			paymentBasis.PBS_PerUnitRate = 2500;
			paymentBasis.PBS_ChargeableUnit = "20GP";
			paymentBasis.PBS_RateUnit = "CN";
			paymentBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);
			paymentBasis = jobConsolCost.PaymentBases.AddNew();
			paymentBasis.PBS_PerUnitRate = 4000;
			paymentBasis.PBS_ChargeableUnit = "40GP";
			paymentBasis.PBS_RateUnit = "CN";
			paymentBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);
			Factory.Save();

			using (var consolidationsForm = new ZForm(apportionmentListing))
			using (var costSummaryGrid = new ZGrid())
			{
				costSummaryGrid.BindTo = "CostsCollection";
				costSummaryGrid.Columns.AddTextColumn("E6_Description", 100);
				consolidationsForm.Controls.Add(costSummaryGrid);
				consolidationsForm.Show();
				costSummaryGrid.SetDataBinding(apportionmentListing, "CostsCollection");

				new QuickCalculateMenuItemManager(costSummaryGrid, consol).AddMenuItem();
				var quickCalculateMenuItem = costSummaryGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				quickCalculateMenuItem.PerformClick();

				var jobChargeQuickCalculateForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;
				var jobChargeQuickCalculateObject = jobChargeQuickCalculateForm.BusinessEntity as JobChargeQuickCalculateBusinessObject;

				jobChargeQuickCalculateObject.QuantityDescription = "Chargeable";
				AssertEquals("Cost rate should not be populated", 0, (int)jobChargeQuickCalculateObject.CostRate);

				jobChargeQuickCalculateObject.QuantityDescription = "Container Count";
				var containers = jobChargeQuickCalculateObject.Containers.Cast<ContainerCalculationData>();

				var container1 = containers.Single(x => x.ContainerType == "20GP");
				AssertEquals("Cost rate shown from payment basis", isSpotRate ? 2500 : 0, (int)container1.Cost);
				AssertEquals("Container count correct", 2, (int)container1.QuantitySelectedContainers);

				var container2 = containers.Single(x => x.ContainerType == "40GP");
				AssertEquals("Cost rate shown from payment basis", isSpotRate ? 4000 : 0, (int)container2.Cost);
				AssertEquals("Container count correct", 1, (int)container2.QuantitySelectedContainers);

				AssertEquals("Total cost calculated", isSpotRate ? 9000 : 0, (int)jobChargeQuickCalculateObject.CostTotal);
			}
		}

		public void TestQuickCalculate_NoAutoRating()
		{
			Factory.NewJobForTesting<Job>();
			using (var grid = new ZGrid())
			{
				new QuickCalculateMenuItemManager(grid, null).AddMenuItem();
				MenuItem quickCalculateMenuItem = grid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertEquals(Shortcut.Ctrl0, quickCalculateMenuItem.Shortcut);
				AssertEquals(true, quickCalculateMenuItem.ShowShortcut);

				AssertNotNull(quickCalculateMenuItem);

				quickCalculateMenuItem.PerformClick();
				AssertEquals("You can only perform Quick Calculations for jobs that support Auto Rating.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuickCalculator_MenuItem_DisabledWhenGridIsReadOnly()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			using (var form = new ZForm(job))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Charges";
				grid.Columns.AddTextColumn("JR_Desc", 100);
				form.Controls.Add(grid);
				grid.SetDataBinding(job, "Charges");
				grid.ReadOnly = true;

				new QuickCalculateMenuItemManager(grid, parent).AddMenuItem();
				form.Show();

				var quickCalculateMenuItem = grid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				grid.ContextMenu.DoPopup();
				AssertEquals("When grid is readonly, we should disable Quick Calculator", expected: false, quickCalculateMenuItem.Enabled);
			}
		}

		public void TestQuickCalculate_NoRowSelected()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;

			using (var grid = new ZGrid())
			{
				new QuickCalculateMenuItemManager(grid, parent).AddMenuItem();
				MenuItem quickCalculateMenuItem = grid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull(quickCalculateMenuItem);

				quickCalculateMenuItem.PerformClick();
				AssertEquals("Please click on a row before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuickCalculate_RowSelected()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			using (var form = new ZForm(job))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Charges";
				grid.Columns.AddTextColumn("JR_Desc", 100);
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(job, "Charges");

				new QuickCalculateMenuItemManager(grid, parent).AddMenuItem();

				MenuItem quickCalculateMenuItem = grid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull(quickCalculateMenuItem);

				grid.Select(0);
				quickCalculateMenuItem.PerformClick();

				AssertEquals(typeof(JobChargeQuickCalculateForm), ZFormModaliser.ActiveForm.GetType());
				((IZForm)ZFormModaliser.ActiveForm).Dispose();
			}
		}

		public void TestQuickCalculate_NoJobChargeCurrency()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;

			var chargeA = job.Charges.AddNew();
			chargeA.JR_AC = Env.Registry.FreightChargeCode;
			chargeA.JR_RX_NKCostCurrency = string.Empty;

			var chargeB = job.Charges.AddNew();
			chargeB.JR_AC = Env.Registry.FreightChargeCode;
			chargeB.JR_RX_NKSellCurrency = string.Empty;

			var chargeC = job.Charges.AddNew();
			chargeC.JR_AC = Env.Registry.FreightChargeCode;
			chargeC.JR_RX_NKCostCurrency = string.Empty;
			chargeC.JR_RX_NKSellCurrency = string.Empty;

			var chargeC1 = job.Charges.AddNew();
			chargeC1.JR_AC = Env.Registry.FreightChargeCode;
			chargeC1.JR_RX_NKSellCurrency = "SSS";
			chargeC1.JR_RX_NKCostCurrency = "PPP";

			var disbursementChargeCode = Factory.New<AccChargeCode>();
			disbursementChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;

			var chargeD = job.Charges.AddNew();
			chargeD.JR_AC = disbursementChargeCode.PK;
			chargeD.JR_RX_NKSellCurrency = string.Empty;

			var chargeE = job.Charges.AddNew();
			chargeE.JR_AC = disbursementChargeCode.PK;
			chargeE.JR_RX_NKCostCurrency = string.Empty;

			var revenueChargeCode = Factory.New<AccChargeCode>();
			revenueChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			var chargeF = job.Charges.AddNew();
			chargeF.JR_AC = revenueChargeCode.PK;
			chargeF.JR_RX_NKCostCurrency = string.Empty;

			var chargeG = job.Charges.AddNew();
			chargeG.JR_AC = revenueChargeCode.PK;
			chargeG.JR_RX_NKSellCurrency = string.Empty;

			using (var form = new ZForm(job))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Charges";
				grid.Columns.AddTextColumn("JR_Desc", 100);
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(job, "Charges");

				new QuickCalculateMenuItemManager(grid, parent).AddMenuItem();
				MenuItem quickCalculateMenuItem = grid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull(quickCalculateMenuItem);

				grid.CurrentRowIndex = 0;
				grid.Select(0);
				quickCalculateMenuItem.PerformClick();
				AssertEquals("Please enter Cost Currency before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);

				grid.CurrentRowIndex = 1;
				grid.Select(1);
				quickCalculateMenuItem.PerformClick();
				AssertEquals("Please enter Sell Currency before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);

				grid.CurrentRowIndex = 2;
				grid.Select(2);
				quickCalculateMenuItem.PerformClick();
				AssertEquals("Please enter Cost and Sell Currencies before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);

				grid.CurrentRowIndex = 3;
				grid.Select(3);
				quickCalculateMenuItem.PerformClick();
				AssertEquals("Please enter Cost and Sell Currencies before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);

				#region Sell currency should not be checked for Disbursment Charges but Cost currency should be.

				UnitTestUserNotification.Instance.ClearMessages();
				grid.CurrentRowIndex = 4;
				grid.Select(4);
				quickCalculateMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var qcForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;
				AssertNotNull(qcForm);

				grid.CurrentRowIndex = 5;
				grid.Select(5);
				quickCalculateMenuItem.PerformClick();
				AssertEquals("Please enter Cost Currency before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);

				#endregion

				#region Cost currency should not be checked for Revenue Charges but Sell currency should be.

				UnitTestUserNotification.Instance.ClearMessages();
				grid.CurrentRowIndex = 6;
				grid.Select(6);
				quickCalculateMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				qcForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;
				AssertNotNull(qcForm);

				grid.CurrentRowIndex = 7;
				grid.Select(7);
				quickCalculateMenuItem.PerformClick();
				AssertEquals("Please enter Sell Currency before performing Quick Calculation.", UnitTestUserNotification.Instance.LastMessage.Text);

				#endregion
			}
		}

		public void TestQuickCalculate_CalculatesCostNotRevenue()
		{
			var commonWorkSheet = Factory.New<CommonWorkSheet>();
			var apportionmentListing = new ApportionmentListing(Factory, commonWorkSheet);

			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_RX_NKCurrency = "USD";

			using (var consolidationsForm = new ZForm(apportionmentListing))
			using (var costSummaryGrid = new ZGrid())
			{
				costSummaryGrid.BindTo = "CostsCollection";
				costSummaryGrid.Columns.AddTextColumn("E6_Description", 100);
				consolidationsForm.Controls.Add(costSummaryGrid);
				consolidationsForm.Show();
				costSummaryGrid.SetDataBinding(apportionmentListing, "CostsCollection");

				new QuickCalculateMenuItemManager(costSummaryGrid, commonWorkSheet).AddMenuItem();
				var quickCalculateMenuItem = costSummaryGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull("'Quick Calculate' menu item should exist", quickCalculateMenuItem);

				costSummaryGrid.CurrentRowIndex = 0;
				costSummaryGrid.Select(0);
				AssertNoExceptionThrown("Incorrectly asking for revenue, rather than cost, be calculated", () => {
					quickCalculateMenuItem.PerformClick();
				});

				var jobChargeQuickCalculateForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;
				AssertNotNull(jobChargeQuickCalculateForm);
			}
		}

		public void TestQuickCalculateMenuItemEnableDisable()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;

			var revenueChargeCode = Factory.New<AccChargeCode>();
			revenueChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			var chargeA = job.Charges.AddNew();
			chargeA.JR_AC = revenueChargeCode.PK;

			var commentChargeCode = Factory.New<AccChargeCode>();
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			var chargeB = job.Charges.AddNew();
			chargeB.JR_AC = commentChargeCode.PK;

			var parentBizo = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(job))
			using (var grid = new ZGridForTest())
			{
				grid.BindTo = "Charges";
				grid.Columns.AddTextColumn("JR_Desc", 100);
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(job, "Charges");

				new QuickCalculateMenuItemManager(grid, parent).AddMenuItem();

				ShowPopupMenuForRow(0);
				var quickCalculateMenuItem = grid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull(quickCalculateMenuItem);
				AssertEquals(true, quickCalculateMenuItem.Visible);
				AssertEquals("Quick calculate context menu item should be enabled", true, quickCalculateMenuItem.Enabled);

				ShowPopupMenuForRow(2);
				AssertNotNull(quickCalculateMenuItem);
				AssertEquals(true, quickCalculateMenuItem.Visible);
				AssertEquals("Quick calculate context menu item should be disabled", false, quickCalculateMenuItem.Enabled);

				void ShowPopupMenuForRow(int rowIndex)
				{
					grid.CurrentRowIndex = rowIndex;
					grid.Select(rowIndex);
					grid.RowUnderMouseForTest = rowIndex;
					grid.ContextMenu.ShowPopupMenu();
				}
			}
		}

		public void TestQuickCalculate_ShortcutWorks()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;

			var revenueChargeCode = Factory.New<AccChargeCode>();
			revenueChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			var chargeA = job.Charges.AddNew();
			chargeA.JR_AC = revenueChargeCode.PK;

			var commentChargeCode = Factory.New<AccChargeCode>();
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			var chargeB = job.Charges.AddNew();
			chargeB.JR_AC = commentChargeCode.PK;

			using (var form = new ZForm(job))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Charges";
				grid.Columns.AddTextColumn("JR_Desc", 100);
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(job, "Charges");

				new QuickCalculateMenuItemManager(grid, parent).AddMenuItem();

				grid.CurrentRowIndex = 0;
				grid.Select(0);
				grid.SetCurrentHitTestForTest(0, 0);
				KeySender.PostKeyDown(grid, (Keys.Control | Keys.D0));
				Application.DoEvents();

				AssertEquals(typeof(JobChargeQuickCalculateForm), ZFormModaliser.ActiveForm.GetType());
				((IZForm)ZFormModaliser.ActiveForm).Dispose();

				KeySender.PostKeyDown(grid, (Keys.Control | Keys.NumPad0));
				Application.DoEvents();

				AssertEquals(typeof(JobChargeQuickCalculateForm), ZFormModaliser.ActiveForm.GetType());
				((IZForm)ZFormModaliser.ActiveForm).Dispose();
			}
		}

		public void TestQuickCalculate_ShortcutWorksWhenMouseIsNotOnSelectedRow()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			job.PlugInData = parent;

			var revenueChargeCode = Factory.New<AccChargeCode>();
			revenueChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			var chargeA = job.Charges.AddNew();
			chargeA.JR_AC = revenueChargeCode.PK;

			using (var form = new ZForm(job))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Charges";
				grid.Columns.AddTextColumn("JR_Desc", 100);
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(job, "Charges");

				new QuickCalculateMenuItemManager(grid, parent).AddMenuItem();

				grid.CurrentRowIndex = 0;
				grid.Select(0);
				grid.MousePositionForTesting = new Point(0, 0);
				KeySender.PostKeyDown(grid, (Keys.Control | Keys.D0));
				Application.DoEvents();

				AssertEquals(typeof(JobChargeQuickCalculateForm), ZFormModaliser.ActiveForm.GetType());
				((IZForm)ZFormModaliser.ActiveForm).Dispose();

				KeySender.PostKeyDown(grid, (Keys.Control | Keys.NumPad0));
				Application.DoEvents();

				AssertEquals(typeof(JobChargeQuickCalculateForm), ZFormModaliser.ActiveForm.GetType());
				((IZForm)ZFormModaliser.ActiveForm).Dispose();
			}
		}

		public void TestQuickCalculate_WrongUnitOfVolume()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 10;
			shipment.JS_UnitOfVolume = "CM";

			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var jobConsolCost = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost.E6_RX_NKCurrency = "AUD";
			jobConsolCost.E6_RatingBehaviour = "SPT";
			jobConsolCost.E6_AC_ChargeCode = Factory.New<AccChargeCode>().PK;
			Factory.Save();

			using (var consolidationsForm = new ZForm(apportionmentListing))
			using (var costSummaryGrid = new ZGrid())
			{
				costSummaryGrid.BindTo = "CostsCollection";
				costSummaryGrid.Columns.AddTextColumn("E6_Description", 100);
				consolidationsForm.Controls.Add(costSummaryGrid);
				consolidationsForm.Show();
				costSummaryGrid.SetDataBinding(apportionmentListing, "CostsCollection");

				new QuickCalculateMenuItemManager(costSummaryGrid, consol).AddMenuItem();
				var quickCalculateMenuItem = costSummaryGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				quickCalculateMenuItem.PerformClick();
				AssertEquals("Invalid unit of volume: 'CM'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGivenConsolWithCharges_WhenRunQuickCalculationInChargePercentageForNonExistingChargeCode_ThenValidateChargeCodePKShouldFail()
		{
			var creator = new TestObjectCreator(Factory);

			var forwardingConsol = Factory.New<ForwardingConsol>();
			var apportionmentListing = new ApportionmentListing(Factory, forwardingConsol);

			var jobConsolCost1 = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost1.E6_AC_ChargeCode = creator.CC1.PK;

			var jobConsolCost2 = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost2.E6_AC_ChargeCode = creator.CC2.PK;

			using (var consolidationsForm = new ZForm(apportionmentListing))
			using (var costSummaryGrid = new ZGrid())
			{
				costSummaryGrid.BindTo = "CostsCollection";
				costSummaryGrid.Columns.AddTextColumn("E6_Description", 100);
				consolidationsForm.Controls.Add(costSummaryGrid);
				consolidationsForm.Show();
				costSummaryGrid.SetDataBinding(apportionmentListing, "CostsCollection");

				new QuickCalculateMenuItemManager(costSummaryGrid, forwardingConsol).AddMenuItem();
				var quickCalculateMenuItem = costSummaryGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull("'Quick Calculate' menu item should exist", quickCalculateMenuItem);

				costSummaryGrid.CurrentRowIndex = 0;
				costSummaryGrid.Select(0);
				quickCalculateMenuItem.PerformClick();

				var jobChargeQuickCalculateForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;

				var jobChargeQuickCalculateObject = jobChargeQuickCalculateForm.BusinessEntity as JobChargeQuickCalculateBusinessObject;
				jobChargeQuickCalculateObject.QuantityDescription = "Charge Percentage";
				jobChargeQuickCalculateObject.ChargeCodePK = creator.CC3.PK;

				jobChargeQuickCalculateObject.ValidateChargeCodePK();
				AssertHasError(jobChargeQuickCalculateObject.ChargeCodePKInfo, "Charge Code ZZCC3 does not exist on the job. You can only enter a charge code that already has a charge on the job.");
			}
		}

		public void TestGivenConsolWithCharges_WhenRunQuickCalculationInChargePercentage_ThenCostAmountCouldBeCalculated()
		{
			var creator = new TestObjectCreator(Factory);

			var forwardingConsol = Factory.New<ForwardingConsol>();
			var apportionmentListing = new ApportionmentListing(Factory, forwardingConsol);

			var jobConsolCost1 = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost1.E6_AC_ChargeCode = creator.CC1.PK;
			jobConsolCost1.E6_OSCostAmount = 200m;

			var jobConsolCost2 = apportionmentListing.CostsCollection.TryAddNew();
			jobConsolCost2.E6_AC_ChargeCode = creator.CC2.PK;
			jobConsolCost2.E6_OSCostAmount = 300m;

			using (var consolidationsForm = new ZForm(apportionmentListing))
			using (var costSummaryGrid = new ZGrid())
			{
				costSummaryGrid.BindTo = "CostsCollection";
				costSummaryGrid.Columns.AddTextColumn("E6_Description", 100);
				consolidationsForm.Controls.Add(costSummaryGrid);
				consolidationsForm.Show();
				costSummaryGrid.SetDataBinding(apportionmentListing, "CostsCollection");

				new QuickCalculateMenuItemManager(costSummaryGrid, forwardingConsol).AddMenuItem();
				var quickCalculateMenuItem = costSummaryGrid.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull("'Quick Calculate' menu item should exist", quickCalculateMenuItem);

				costSummaryGrid.CurrentRowIndex = 0;
				costSummaryGrid.Select(0);
				quickCalculateMenuItem.PerformClick();

				var jobChargeQuickCalculateForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;

				var jobChargeQuickCalculateObject = jobChargeQuickCalculateForm.BusinessEntity as JobChargeQuickCalculateBusinessObject;
				jobChargeQuickCalculateObject.QuantityDescription = "Charge Percentage";
				jobChargeQuickCalculateObject.Quantity = 50m;
				jobChargeQuickCalculateObject.ChargeCodePK = creator.CC1.PK;

				jobChargeQuickCalculateForm.AcceptButton.PerformClick();
				AssertEquals(100m, jobConsolCost1.E6_OSCostAmount);
			}
		}

		class ZGridForTest : ZGrid
		{
			public int RowUnderMouseForTest { get; set; }

			protected override int RowUnderMouse()
			{
				return RowUnderMouseForTest;
			}
		}
	}
}
