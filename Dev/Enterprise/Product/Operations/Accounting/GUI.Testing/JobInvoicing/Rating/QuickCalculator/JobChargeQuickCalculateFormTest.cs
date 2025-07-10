using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(JobChargeQuickCalculateForm))]
	public class JobChargeQuickCalculateFormTest : ZFormBasherTest
	{
		#region Payment Basis Amount

		#region Min > Rate

		public void TestPaymentBasisAmount_MinimumGreaterThanRate_Cost()
			=> AssertPaymentBasisAmount
				(
					quantity: 100m,
					costSell: CostSell.Cost,
					rate: 0.1m,
					minimum: 100m,
					expectedAmount: 100m,
					assertionMessage: "GIVEN min > rate THEN amount should be min"
				);

		public void TestPaymentBasisAmount_MinimumGreaterThanRate_Sell()
			=> AssertPaymentBasisAmount
				(
					quantity: 100m,
					costSell: CostSell.Revenue,
					rate: 0.1m,
					minimum: 100m,
					expectedAmount: 100m,
					assertionMessage: "GIVEN min > rate THEN amount should be min"
				);

		#endregion

		#region Min < Rate

		public void TestPaymentBasisAmount_MinimumLessThanRate_Cost()
			=> AssertPaymentBasisAmount
			(
				quantity: 100m,
				costSell: CostSell.Cost,
				rate: 0.1m,
				minimum: 1m,
				expectedAmount: 10m,
				assertionMessage: "GIVEN min < rate THEN amount should be rate"
			);

		public void TestPaymentBasisAmount_MinimumLessThanRate_Sell()
			=> AssertPaymentBasisAmount
			(
				quantity: 100m,
				costSell: CostSell.Revenue,
				rate: 0.1m,
				minimum: 1m,
				expectedAmount: 10m,
				assertionMessage: "GIVEN min < rate THEN amount should be rate"
			);

		#endregion

		#region Negative Rate

		public void TestPaymentBasisAmount_NegativeRate_Cost()
			=> AssertPaymentBasisAmount
			(
				quantity: 100m,
				costSell: CostSell.Cost,
				rate: -0.1m,
				minimum: null,
				expectedAmount: -10m,
				assertionMessage: "GIVEN min < rate THEN amount should be rate"
			);

		public void TestPaymentBasisAmount_NegativeRate_Sell()
			=> AssertPaymentBasisAmount
			(
				quantity: 100m,
				costSell: CostSell.Revenue,
				rate: -0.1m,
				minimum: null,
				expectedAmount: -10m,
				assertionMessage: "GIVEN min < rate THEN amount should be rate"
			);

		#endregion

		#region No Min, just rate

		public void TestPaymentBasisAmount_NoMin_Cost()
			=> AssertPaymentBasisAmount
			(
				quantity: 100m,
				costSell: CostSell.Cost,
				rate: 0.1m,
				minimum: null,
				expectedAmount: 10m,
				assertionMessage: "GIVEN no min THEN amount should be rate"
			);

		public void TestPaymentBasisAmount_NoMin_Sell()
			=> AssertPaymentBasisAmount
			(
				quantity: 100m,
				costSell: CostSell.Revenue,
				rate: 0.1m,
				minimum: null,
				expectedAmount: 10m,
				assertionMessage: "GIVEN no min THEN amount should be rate"
			);

		#endregion

		void AssertPaymentBasisAmount(decimal quantity, CostSell costSell, decimal rate, decimal? minimum, decimal expectedAmount, string assertionMessage)
		{
			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			var jobChargeQuickCalculateBusinessObject = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			using (var form = new JobChargeQuickCalculateForm(jobChargeQuickCalculateBusinessObject))
			{
				form.Show();

				jobChargeQuickCalculateBusinessObject.QuantityDescription = "Custom";
				jobChargeQuickCalculateBusinessObject.Quantity = quantity;

				switch (costSell)
				{
					case CostSell.Cost:
						jobChargeQuickCalculateBusinessObject.UpdateSell = false;
						jobChargeQuickCalculateBusinessObject.UpdateCost = true;
						jobChargeQuickCalculateBusinessObject.CostRate = rate;
						if (minimum != null)
						{
							jobChargeQuickCalculateBusinessObject.IsMinimum = true;
							jobChargeQuickCalculateBusinessObject.CostMinimum = minimum.Value;
						}

						form.AcceptButton.PerformClick();

						AssertEquals(assertionMessage, expectedAmount, charge.CostPaymentBases.Single().Amount);

						break;
					case CostSell.Revenue:
						jobChargeQuickCalculateBusinessObject.UpdateCost = false;
						jobChargeQuickCalculateBusinessObject.UpdateSell = true;
						jobChargeQuickCalculateBusinessObject.SellRate = rate;
						if (minimum != null)
						{
							jobChargeQuickCalculateBusinessObject.IsMinimum = true;
							jobChargeQuickCalculateBusinessObject.SellMinimum = minimum.Value;
						}

						form.AcceptButton.PerformClick();

						AssertEquals(assertionMessage, expectedAmount, charge.SellPaymentBases.Single().Amount);

						break;
				}
			}
		}

		#endregion

		public void TestOKButton_SetsResults()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (JobChargeQuickCalculateForm form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();

				quickCalculator.QuantityDescription = "Custom";
				quickCalculator.Quantity = 10m;

				quickCalculator.UpdateSell = true;
				quickCalculator.SellRate = 3m;

				quickCalculator.UpdateCost = true;
				quickCalculator.CostRate = 6m;

				form.AcceptButton.PerformClick();

				AssertEquals(30m, charge.JR_LocalSellAmt);
				AssertEquals(60m, charge.JR_LocalCostAmt);
			}
		}

		public void TestOKButton_ChargeIsDeleted()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();

				charge.Delete();

				AssertNoExceptionThrown(() => form.AcceptButton.PerformClick());
			}
		}

		public void TestCancelButton_NoChangesToJob()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_LocalSellAmt = 10m;
			charge.JR_LocalCostAmt = 10m;
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (JobChargeQuickCalculateForm form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();

				quickCalculator.QuantityDescription = "Custom";
				quickCalculator.Quantity = 10m;

				quickCalculator.UpdateSell = true;
				quickCalculator.SellRate = 3m;

				quickCalculator.UpdateCost = true;
				quickCalculator.CostRate = 6m;

				form.CancelButton.PerformClick();

				AssertEquals(10m, charge.JR_LocalSellAmt);
				AssertEquals(10m, charge.JR_LocalCostAmt);
			}
		}

		public void TestTotalDecimals()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_RX_NKCostCurrency = "JPY";
			charge.JR_RX_NKSellCurrency = "USD";
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (JobChargeQuickCalculateForm form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();
				AssertEquals(2, form.SellTotalCalcEdit.DecimalPlaces);
				AssertEquals(0, form.CostTotalCalcEdit.DecimalPlaces);
				AssertEquals(3, form.QuantityEdit.Decimals);
			}
		}

		public void TestContainerTypeMeasureShowsGridAndHidesOtherControls()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();
				quickCalculator.QuantityDescription = "Container Count";
				Assert(!form.RateCalcEditCost.Visible);
				Assert(!form.RateCalcEditSell.Visible);
				Assert(form.containersGrid.Visible);
				Assert(!form.CostMinimumCalcEdit.Visible);
				Assert(!form.SellMinimumCalcEdit.Visible);
				AssertEquals(0, form.QuantityEdit.DecimalPlaces);

				AssertEquals(form.CostTotalCalcEdit.Location, ControlDpiScalingHelper.NewScaledPoint(172, 165, true));
				AssertEquals(form.SellTotalCalcEdit.Location, ControlDpiScalingHelper.NewScaledPoint(490, 165, true));

				quickCalculator.QuantityDescription = "Chargeable";

				Assert(form.RateCalcEditCost.Visible);
				Assert(form.RateCalcEditSell.Visible);
				Assert(!form.containersGrid.Visible);
				Assert(form.CostMinimumCalcEdit.Visible);
				Assert(form.SellMinimumCalcEdit.Visible);
				AssertEquals(3, form.QuantityEdit.DecimalPlaces);

				AssertEquals(form.CostTotalCalcEdit.Location, ControlDpiScalingHelper.NewScaledPoint(384, 38, true));
				AssertEquals(form.SellTotalCalcEdit.Location, ControlDpiScalingHelper.NewScaledPoint(384, 61, true));
			}
		}

		public void TestTickUntickMinimumCheckbox()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();

				form.IsMinimumCheckBox.Checked = true;
				Assert(quickCalculator.IsMinimum);
				form.IsMinimumCheckBox.Checked = false;
				Assert(!quickCalculator.IsMinimum);

				quickCalculator.IsMinimum = true;
				Assert(form.IsMinimumCheckBox.Checked);
				Assert(!form.CostMinimumCalcEdit.ReadOnly);
				Assert(!form.SellMinimumCalcEdit.ReadOnly);
				Assert(form.CostTotalCalcEdit.ReadOnly);
				Assert(form.SellTotalCalcEdit.ReadOnly);

				quickCalculator.IsMinimum = false;
				Assert(!form.IsMinimumCheckBox.Checked);
				Assert(form.CostMinimumCalcEdit.ReadOnly);
				Assert(form.SellMinimumCalcEdit.ReadOnly);
				Assert(form.CostMinimumCalcEdit.Text == "0.00");
				Assert(form.SellMinimumCalcEdit.Text == "0.00");
				Assert(form.CostTotalCalcEdit.ReadOnly);
				Assert(form.SellTotalCalcEdit.ReadOnly);
			}
		}

		public void TestCurrencySetsDecimalPlacesOnForm()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Australia;
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.Jordan;

			var host = Factory.New<DummyAutoRating>();
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(host, charge);

			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();
				form.IsMinimumCheckBox.Checked = true;
				AssertEquals(3, form.SellTotalCalcEdit.Decimals);
				AssertEquals(3, form.SellMinimumCalcEdit.Decimals);

				AssertEquals(2, form.CostTotalCalcEdit.Decimals);
				AssertEquals(2, form.CostMinimumCalcEdit.Decimals);
			}

			charge.JR_RX_NKCostCurrency = "";
			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();
				form.IsMinimumCheckBox.Checked = true;
				AssertEquals("Should default remain at default decimal places if there is no currency", 3, form.CostTotalCalcEdit.Decimals);
				AssertEquals(3, form.CostMinimumCalcEdit.Decimals);
			}

			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.Japan;
			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();
				form.IsMinimumCheckBox.Checked = true;
				AssertEquals(0, form.SellTotalCalcEdit.Decimals);
				AssertEquals(0, form.SellMinimumCalcEdit.Decimals);
			}
		}

		public void TestAllControlsTabIndexShouleBeUnique_AndLessThanOKExceptCancel_IfControlTabStopIsTrue()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();

				var okButtonTabIndex = form.Controls.Find("ZButton1", false).FirstOrDefault().TabIndex;
				var cancelButtonTabIndex = form.Controls.Find("ZButton2", false).FirstOrDefault().TabIndex;

				var controlTabIndexes = new Dictionary<int, string>();
				foreach (var control in form.Controls.OfType<Control>().Where(x => x.TabStop))
				{
					var tabIndex = control.TabIndex;
					var controlName = control.Name;

					controlTabIndexes.TryGetValue(tabIndex, out var cachedControlName);
					AssertNull($"TabIndex {tabIndex} is being used in both controls {cachedControlName} and {controlName}", cachedControlName);

					controlTabIndexes.Add(tabIndex, controlName);
					if (tabIndex != okButtonTabIndex && tabIndex != cancelButtonTabIndex)
					{
						AssertLessThan($@"{controlName} should have tab index be less than cancel button tab index.<br />
										Please ensure the rule for any new control by:<br />
										- taking the current number from zButton1 then putting it to the new control<br />
										- and increasing the numbers for zButton1 and zButton2.", control.TabIndex, okButtonTabIndex);
					}
				}
			}
		}

		public void TestOKButtonTabIndexIsLessThanCancelButtonTabIndex()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			using (var form = new JobChargeQuickCalculateForm(quickCalculator))
			{
				form.Show();
				var okButtonTabIndex = form.Controls.Find("ZButton1", false).FirstOrDefault().TabIndex;
				var cancelButtonTabIndex = form.Controls.Find("ZButton2", false).FirstOrDefault().TabIndex;
				AssertLessThan("Ok button tabindex should be less than cancel button tabindex", okButtonTabIndex, cancelButtonTabIndex);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new JobChargeQuickCalculateForm(new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Factory.New<Charge>()));
		}

		#endregion
	}
}
