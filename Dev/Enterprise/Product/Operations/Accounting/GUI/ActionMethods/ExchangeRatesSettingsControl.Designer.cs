namespace Enterprise.Accounting.GUI
{
	partial class ExchangeRatesSettingsControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEdit deExchangeRatesSource;
			Enterprise.ZArchitecture.ZLabel lblExchangeRatesSource;
			deExchangeRatesSource = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			lblExchangeRatesSource = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ExchangeRatesSettings);
			// 
			// deExchangeRatesSource
			// 
			deExchangeRatesSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(deExchangeRatesSource, "ExchangeRatesSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ExchangeRatesSettings)(null)).ExchangeRatesSource)));
			deExchangeRatesSource.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 28, true);
			deExchangeRatesSource.Name = "deExchangeRatesSource";
			deExchangeRatesSource.PreBoundMaxLength = 3;
			deExchangeRatesSource.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			deExchangeRatesSource.TabIndex = 0;
			// 
			// lblExchangeRatesSource
			// 
			lblExchangeRatesSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			lblExchangeRatesSource.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			lblExchangeRatesSource.Name = "lblExchangeRatesSource";
			lblExchangeRatesSource.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 22, true);
			lblExchangeRatesSource.TabIndex = 1;
			lblExchangeRatesSource.Text = "Rates Source for Apply";
			// 
			// ExchangeRatesSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(lblExchangeRatesSource);
			this.Controls.Add(deExchangeRatesSource);
			this.Name = "ExchangeRatesSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 62, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}
	}
}
