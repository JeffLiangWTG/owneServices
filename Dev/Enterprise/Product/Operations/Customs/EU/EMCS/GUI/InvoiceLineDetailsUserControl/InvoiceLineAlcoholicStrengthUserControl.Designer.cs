namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class InvoiceLineAlcoholicStrengthUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AlcoholicUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AlcoholicStrengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine);
			// 
			// AlcoholicUnitLabel
			// 
			this.AlcoholicUnitLabel.AutoSize = true;
			this.AlcoholicUnitLabel.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("779aa10c-637c-4751-8813-abfa20ee8d57", "%");
			this.AlcoholicUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AlcoholicUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 3, true);
			this.AlcoholicUnitLabel.Name = "AlcoholicUnitLabel";
			this.AlcoholicUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.AlcoholicUnitLabel.TabIndex = 15;
			this.AlcoholicUnitLabel.Text = "%";
			// 
			// AlcoholicStrengthCalcEdit
			// 
			this.AlcoholicStrengthCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlcoholicStrengthCalcEdit, "ZG_AlcoholicStrength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_AlcoholicStrength)));
			this.AlcoholicStrengthCalcEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("416780a2-97a8-44ba-87e0-eb8802457f89", "Alcoholic Strength");
			this.AlcoholicStrengthCalcEdit.DecimalPlaces = 2;
			this.AlcoholicStrengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.AlcoholicStrengthCalcEdit.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.AlcoholicStrengthCalcEdit.Name = "AlcoholicStrengthCalcEdit";
			this.AlcoholicStrengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.AlcoholicStrengthCalcEdit.TabIndex = 14;
			this.AlcoholicStrengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceLineAlcoholicStrengthUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AlcoholicUnitLabel);
			this.Controls.Add(this.AlcoholicStrengthCalcEdit);
			this.Name = "InvoiceLineAlcoholicStrengthUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel AlcoholicUnitLabel;
		internal ZArchitecture.ZCalcEdit AlcoholicStrengthCalcEdit;
	}
}
