using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZExchangeRateControl
	{
		internal ZExchangeRateFindBox CurrencyFindBox;
		internal ZExchangeRateCalcEdit RateCalcEdit;

		private void InitializeComponent()
		{
			this.CurrencyFindBox = new ZExchangeRateFindBox();
			this.RateCalcEdit = new ZExchangeRateCalcEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CurrencyFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// CurrencyFindBox
			// 
			this.CurrencyFindBox.AllowDrop = true;
			this.CurrencyFindBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.CurrencyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CurrencyFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.CurrencyFindBox.Name = "CurrencyFindBox";
			this.CurrencyFindBox.PreBoundMaxLength = 3;
			this.CurrencyFindBox.ShowDescriptionBox = false;
			this.CurrencyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 18, true);
			this.CurrencyFindBox.TabIndex = 0;
			// 
			// RateCalcEdit
			// 
			this.RateCalcEdit.DecimalPlaces = 2;
			this.RateCalcEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 0, true);
			this.RateCalcEdit.Name = "RateCalcEdit";
			this.RateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 18, true);
			this.RateCalcEdit.TabIndex = 1;
			this.RateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZExchangeRateControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RateCalcEdit);
			this.Controls.Add(this.CurrencyFindBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.Name = "ZExchangeRateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.CurrencyFindBox.ResumeLayout(true);
			this.CurrencyFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
