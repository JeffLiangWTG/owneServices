using System;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin
{
	public class TaxUserControl : EU.GUI.PlugIn.TaxUserControl
	{
		public TaxUserControl()
		{
			AddColumnsToGrid();
			AddControls();
		}

		ZDropEdit taxRateOverrideDropEdit;
		ZDropEdit taxRateSuspensionDropEdit;

		void AddColumnsToGrid()
		{
			var rateDuty =
				new ZDropEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("GB.TaxUserControl.Rate", "Rate"),
					BindToList = "Data.Lookups.RateDutyList",
					ColumnName = "Data+G4_RateDuty"
				};

			var rateSuspension =
				new ZDropEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("GB.TaxUserControl.Suspension", "Suspension"),
					BindToList = "Data.Lookups.RateSuspensionList",
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = "Data+G4_RateSuspension"
				};

			var rateOverride =
				new ZDropEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("GB.TaxUserControl.Override", "Override"),
					BindToList = "Data.Lookups.RateOverrideList",
					ColumnName = "Data+G4_RateOverride"
				};

			TaxGrid.ColumnStyles.Remove(zTextBoxColumnStyleInfoForMethodOfCalculation);
			TaxGrid.ColumnStyles.Remove(zCalcEditColumnStyleInfoForCalcPercentage);

			TaxGrid.ColumnStyles.AddRange(new[] { rateDuty, rateSuspension, rateOverride });
			TaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfoForCalcPercentage);
		}

		void AddControls()
		{
			taxRateOverrideDropEdit = new ZDropEdit
			{
				AllowDrop = true,
				Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 65, true),
				Name = "TaxRateOverrideDropEdit",
				PreBoundMaxLength = 3,
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true),
				TabIndex = 11
			};

			BindingSource.SetBindingMember(taxRateOverrideDropEdit, "FilteredInvoiceLines.Taxes.Data.G4_RateOverride");

			taxRateSuspensionDropEdit = new ZDropEdit
			{
				AllowDrop = true,
				Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 39, true),
				Name = "TaxRateSuspensionDropEdit",
				PreBoundMaxLength = 1,
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true),
				TabIndex = 10
			};

			BindingSource.SetBindingMember(taxRateSuspensionDropEdit, "FilteredInvoiceLines.Taxes.Data.G4_RateSuspension");

			TaxRateGroupBox.Controls.Add(taxRateOverrideDropEdit);
			TaxRateGroupBox.Controls.Add(taxRateSuspensionDropEdit);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			WireHandlersFromBiz((JobDeclaration)JobDeclaration);
		}

		void Dec_OnAppCodeChanged(object sender, EventArgs e)
		{
			var jobDeclaration = sender as JobDeclaration;

			if (jobDeclaration != null)
			{
				TaxGroupBox.Text = jobDeclaration.TaxGroupCaption;
				taxRateOverrideDropEdit.Visible = false;
				taxRateSuspensionDropEdit.Visible = false;

				this.TaxRateDutyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 19, true);
				this.TaxMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 45, true);
				this.TaxMethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);

				TaxRateGroupBox.Controls.Remove(TaxRateDutyDropEdit);
				TaxGroupBox.Controls.Remove(TaxRateGroupBox);
				TaxGroupBox.Controls.Add(TaxRateDutyDropEdit);
			}
		}

		void WireHandlersFromBiz(JobDeclaration header)
		{
			if (header != null)
			{
				header.OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
				header.OnApplicationCodeChanged += Dec_OnAppCodeChanged;
				Dec_OnAppCodeChanged(header, null);
			}
		}

		protected override Type DeclarationType => typeof(JobDeclaration);
	}
}
