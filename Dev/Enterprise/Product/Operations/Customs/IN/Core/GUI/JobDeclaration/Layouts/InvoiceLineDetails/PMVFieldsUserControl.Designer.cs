namespace Enterprise.Customs.IN.GUI
{
	partial class PMVFieldsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ValuationMarkupCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PMVCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobComInvoiceLine);
			// 
			// ValuationMarkupCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValuationMarkupCalcEdit, "JI_ValuationMarkup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_ValuationMarkup)));
			this.ValuationMarkupCalcEdit.DecimalPlaces = 2;
			this.ValuationMarkupCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValuationMarkupCalcEdit.MaxValue = new decimal(new int[] {
            99999,
            0,
            0,
            131072});
			this.ValuationMarkupCalcEdit.Name = "ValuationMarkupCalcEdit";
			this.ValuationMarkupCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 22, true);
			this.ValuationMarkupCalcEdit.TabIndex = 0;
			this.ValuationMarkupCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ValuationMarkupCalcEdit.TrackDisposedAccess = true;
			// 
			// PMVCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PMVCalcEdit, "JI_PMV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_PMV)));
			this.PMVCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PMVCalcEdit, false);
			this.PMVCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 0, true);
			this.PMVCalcEdit.MaxValue = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            131072});
			this.PMVCalcEdit.Name = "PMVCalcEdit";
			this.PMVCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 22, true);
			this.PMVCalcEdit.TabIndex = 2;
			this.PMVCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PMVCalcEdit.TrackDisposedAccess = true;
			// 
			// PercentageLabel
			// 
			this.PercentageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PercentageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 0, true);
			this.PercentageLabel.Name = "PercentageLabel";
			this.PercentageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 22, true);
			this.PercentageLabel.TabIndex = 1;
			this.PercentageLabel.Text = "%";
			this.PercentageLabel.UseMnemonic = false;
			// 
			// PMVFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PercentageLabel);
			this.Controls.Add(this.PMVCalcEdit);
			this.Controls.Add(this.ValuationMarkupCalcEdit);
			this.Name = "PMVFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit ValuationMarkupCalcEdit;
		internal ZArchitecture.ZCalcEdit PMVCalcEdit;
		internal ZArchitecture.ZLabel PercentageLabel;
	}
}
