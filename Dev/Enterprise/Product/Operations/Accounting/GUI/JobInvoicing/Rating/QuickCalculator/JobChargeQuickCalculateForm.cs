using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobChargeQuickCalculateForm : ZChildForm
	{
		public JobChargeQuickCalculateForm(JobChargeQuickCalculateBusinessObject quickCalculate)
			: base(quickCalculate)
		{
			var sellCurrency = quickCalculate.SellCurrency;
			if (sellCurrency != null)
			{
				SellTotalCalcEdit.Decimals = sellCurrency.Decimals;
				SellMinimumCalcEdit.Decimals = sellCurrency.Decimals;
			}

			var costCurrency = quickCalculate.CostCurrency;
			if (costCurrency != null)
			{
				CostTotalCalcEdit.Decimals = costCurrency.Decimals;
				CostMinimumCalcEdit.Decimals = costCurrency.Decimals;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			RearrangeAndSetControls();
			new ContainerCountMenuItemManager(containersGrid).AddMenuItem();
		}

		public override string FormVerb => string.Empty;

		#region Binding

		public new JobChargeQuickCalculateBusinessObject DataSource
		{
			get { return (JobChargeQuickCalculateBusinessObject)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				RateCalcEditCost.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding((NoResString)"Caption"); // Hard-coded constant
				RateCalcEditSell.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding((NoResString)"Caption"); // Hard-coded constant
				DataSource.QuantityDescriptionInfo.ValueChanged -= QuantityDescriptionInfo_ValueChanged;
				ChargeCodeFindBox.DataBindings.RemoveBinding("IsVisibleForBinding");
				RateCalcEditCost.DataBindings.RemoveBinding("IsVisibleForBinding");
				RateCalcEditSell.DataBindings.RemoveBinding("IsVisibleForBinding");
				CostMinimumCalcEdit.DataBindings.RemoveBinding("IsVisibleForBinding");
				SellMinimumCalcEdit.DataBindings.RemoveBinding("IsVisibleForBinding");
				IsMinimumCheckBox.DataBindings.RemoveBinding("IsVisibleForBinding");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				RateCalcEditCost.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, "RateLabelText"));
				RateCalcEditSell.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, "RateLabelText"));
				ChargeCodeFindBox.DataBindings.Add(new KBinding("IsVisibleForBinding", DataSource, "IsPercentageCharge"));
				IsMinimumCheckBox.DataBindings.Add(new KBinding("IsVisibleForBinding", DataSource, "IsMinimumVisible"));
				DataSource.QuantityDescriptionInfo.ValueChanged += QuantityDescriptionInfo_ValueChanged;
			}
		}

		void QuantityDescriptionInfo_ValueChanged(object sender, EventArgs e)
		{
			RearrangeAndSetControls();
		}

		void RearrangeAndSetControls()
		{
			if (DataSource == null)
			{
				return;
			}

			bool usesContainerCountMeasure = DataSource.UsesContainerCountMeasure;
			containersGrid.Visible = usesContainerCountMeasure;
			RateCalcEditCost.Visible = !usesContainerCountMeasure;
			RateCalcEditSell.Visible = !usesContainerCountMeasure;
			CostMinimumCalcEdit.Visible = !usesContainerCountMeasure;
			SellMinimumCalcEdit.Visible = !usesContainerCountMeasure;

			this.SuspendLayout();

			var index = usesContainerCountMeasure ? 1 : 0;
			CostTotalCalcEdit.Location = calcEditLocations[index][this.CostTotalCalcEdit.Name];
			CostTotalCalcEdit.Size = calcEditSizes[index][this.CostTotalCalcEdit.Name];
			SellTotalCalcEdit.Location = calcEditLocations[index][this.SellTotalCalcEdit.Name];
			SellTotalCalcEdit.Size = calcEditSizes[index][this.SellTotalCalcEdit.Name];
			Size = calcEditSizes[index]["FORM"];

			QuantityEdit.Decimals = usesContainerCountMeasure ? 0 : 3;

			this.ResumeLayout(true);
			this.PerformLayout();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Close

		void OKButton_Click(object sender, EventArgs e)
		{
			DataSource.RunPreSaveValidation();
			if (DataSource.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DataSource.SetCalculationResults();
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Constants of sizes and locations

		readonly Dictionary<string, System.Drawing.Size>[] calcEditSizes =
		{
			new Dictionary<string, System.Drawing.Size>()
			{
				{ "RateCalcEditCost", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true) },
				{ "RateCalcEditSell", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true) },
				{ "CostMinimumCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true) },
				{ "SellMinimumCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true) },
				{ "CostTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true) },
				{ "SellTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true) },
				{ "FORM", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 200, true) },
			},
			new Dictionary<string, System.Drawing.Size>()
			{
				{ "CostTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true) },
				{ "SellTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true) },
				{ "FORM", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 280, true) },
			},
		};

		readonly Dictionary<string, System.Drawing.Point>[] calcEditLocations =
		{
			new Dictionary<string, System.Drawing.Point>
			{
				{ "RateCalcEditCost", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 38, true) },
				{ "RateCalcEditSell", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 61, true) },
				{ "CostMinimumCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 38, true) },
				{ "SellMinimumCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 61, true) },
				{ "CostTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 38, true) },
				{ "SellTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 61, true) },
			},
			new Dictionary<string, System.Drawing.Point>
			{
				{ "CostTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 165, true) },
				{ "SellTotalCalcEdit", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 165, true) },
			},
		};

		#endregion
	}
}

