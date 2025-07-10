namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class InvoiceLineFiscalMarkUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FiscalMarkTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.FiscalMarkUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FiscalMarkTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine);
			// 
			// FiscalMarkTextBox
			// 
			this.FiscalMarkTextBox.AllowDrop = true;
			this.FiscalMarkTextBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("41bd9c40-6f43-4e18-befa-d48cf46af6c6", "Fiscal Mark");
			this.FiscalMarkTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FiscalMarkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.FiscalMarkTextBox.Name = "FiscalMarkTextBox";
			this.FiscalMarkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.FiscalMarkTextBox.TabIndex = 8;
			// 
			// FiscalMarkUsedCheckBox
			// 
			this.FiscalMarkUsedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FiscalMarkUsedCheckBox, "ZG_FiscalMarkUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_FiscalMarkUsed)));
			this.FiscalMarkUsedCheckBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("07cb6a51-60a5-4172-8c7b-79688228b5d8", "Used");
			this.FiscalMarkUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 4, true);
			this.FiscalMarkUsedCheckBox.Name = "FiscalMarkUsedCheckBox";
			this.FiscalMarkUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.FiscalMarkUsedCheckBox.TabIndex = 9;
			this.FiscalMarkUsedCheckBox.UseVisualStyleBackColor = true;
			// 
			// InvoiceLineFiscalMarkUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FiscalMarkUsedCheckBox);
			this.Controls.Add(this.FiscalMarkTextBox);
			this.Name = "InvoiceLineFiscalMarkUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FiscalMarkTextBox.ResumeLayout(true);
			this.FiscalMarkTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Customs.GUI.LongTextControl FiscalMarkTextBox;
		internal ZArchitecture.GUI.ZCheckBox FiscalMarkUsedCheckBox;
	}
}
