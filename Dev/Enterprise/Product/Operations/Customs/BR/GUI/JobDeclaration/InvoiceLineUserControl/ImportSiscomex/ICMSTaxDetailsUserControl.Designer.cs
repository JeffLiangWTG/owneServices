namespace Enterprise.Customs.BR.GUI
{
	partial class ICMSTaxDetailsUserControl
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
			this.ICMSGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ICMSFormulaExplanationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ICMSFormulaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ICMSTotalAmountReductionPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ICMSBaseValueReductionPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ICMSRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ICMSLegalBaseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ICMSTaxRegimeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ICMSGroupBox.SuspendLayout();
			this.ICMSFormulaDropEdit.SuspendLayout();
			this.ICMSLegalBaseDropEdit.SuspendLayout();
			this.ICMSTaxRegimeDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			//
			// ICMSGroupBox
			//
			this.ICMSGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("172a524e-6ef6-4a78-b66a-6a05ba6a484c", "ICMS");
			this.ICMSGroupBox.Controls.Add(this.ICMSFormulaExplanationButton);
			this.ICMSGroupBox.Controls.Add(this.ICMSFormulaDropEdit);
			this.ICMSGroupBox.Controls.Add(this.ICMSTotalAmountReductionPercentageCalcEdit);
			this.ICMSGroupBox.Controls.Add(this.ICMSBaseValueReductionPercentageCalcEdit);
			this.ICMSGroupBox.Controls.Add(this.ICMSRateCalcEdit);
			this.ICMSGroupBox.Controls.Add(this.ICMSLegalBaseDropEdit);
			this.ICMSGroupBox.Controls.Add(this.ICMSTaxRegimeDropEdit);
			this.ICMSGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ICMSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ICMSGroupBox.Name = "ICMSGroupBox";
			this.ICMSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 177, true);
			this.ICMSGroupBox.TabIndex = 1;
			this.ICMSGroupBox.TabStop = false;
			//
			// ICMSFormulaExplanationButton
			//
			this.ICMSFormulaExplanationButton.IsCaptionOverridden = true;
			this.ICMSFormulaExplanationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 92, true);
			this.ICMSFormulaExplanationButton.Name = "ICMSFormulaExplanationButton";
			this.ICMSFormulaExplanationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 20, true);
			this.ICMSFormulaExplanationButton.TabIndex = 6;
			this.ICMSFormulaExplanationButton.Text = "?";
			this.ICMSFormulaExplanationButton.ToolTipCaption = null;
			this.ICMSFormulaExplanationButton.UseVisualStyleBackColor = true;
			this.ICMSFormulaExplanationButton.Click += new System.EventHandler(this.ICMSFormulaExplanationButton_Click);
			//
			// ICMSFormulaDropEdit
			//
			this.ICMSFormulaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ICMSFormulaDropEdit, "JI_ICMSFormula");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ICMSFormula)));
			this.ICMSFormulaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 93, true);
			this.ICMSFormulaDropEdit.Name = "ICMSFormulaDropEdit";
			this.ICMSFormulaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.ICMSFormulaDropEdit.TabIndex = 5;
			//
			// ICMSTotalAmountReductionPercentageCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ICMSTotalAmountReductionPercentageCalcEdit, "JI_ICMSTotalAmountReductionPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ICMSTotalAmountReductionPercentage)));
			this.ICMSTotalAmountReductionPercentageCalcEdit.DecimalPlaces = 2;
			this.ICMSTotalAmountReductionPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 119, true);
			this.ICMSTotalAmountReductionPercentageCalcEdit.Name = "ICMSTotalAmountReductionPercentageCalcEdit";
			this.ICMSTotalAmountReductionPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ICMSTotalAmountReductionPercentageCalcEdit.TabIndex = 4;
			this.ICMSTotalAmountReductionPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ICMSBaseValueReductionPercentageCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ICMSBaseValueReductionPercentageCalcEdit, "JI_ICMSBaseValueReductionPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ICMSBaseValueReductionPercentage)));
			this.ICMSBaseValueReductionPercentageCalcEdit.DecimalPlaces = 2;
			this.ICMSBaseValueReductionPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 93, true);
			this.ICMSBaseValueReductionPercentageCalcEdit.Name = "ICMSBaseValueReductionPercentageCalcEdit";
			this.ICMSBaseValueReductionPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ICMSBaseValueReductionPercentageCalcEdit.TabIndex = 3;
			this.ICMSBaseValueReductionPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ICMSRateCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ICMSRateCalcEdit, "JI_ICMSRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ICMSRate)));
			this.ICMSRateCalcEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a8885598-1e1f-462f-8c76-7defaefb5dad", "Rate (%)");
			this.ICMSRateCalcEdit.DecimalPlaces = 2;
			this.ICMSRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 67, true);
			this.ICMSRateCalcEdit.Name = "ICMSRateCalcEdit";
			this.ICMSRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ICMSRateCalcEdit.TabIndex = 2;
			this.ICMSRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ICMSLegalBaseDropEdit
			//
			this.ICMSLegalBaseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ICMSLegalBaseDropEdit, "ICMSLegalBase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ICMSLegalBase)));
			this.ICMSLegalBaseDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("89133a68-4c85-4cf2-ad2a-5f3561316724", "Legal Base");
			this.ICMSLegalBaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 41, true);
			this.ICMSLegalBaseDropEdit.Name = "ICMSLegalBaseDropEdit";
			this.ICMSLegalBaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ICMSLegalBaseDropEdit.TabIndex = 1;
			//
			// ICMSTaxRegimeDropEdit
			//
			this.ICMSTaxRegimeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ICMSTaxRegimeDropEdit, "ICMSTaxRegime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ICMSTaxRegime)));
			this.ICMSTaxRegimeDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6cee3709-96bc-4bd6-bfdb-0467737bff54", "Tax Regime");
			this.ICMSTaxRegimeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 15, true);
			this.ICMSTaxRegimeDropEdit.Name = "ICMSTaxRegimeDropEdit";
			this.ICMSTaxRegimeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ICMSTaxRegimeDropEdit.TabIndex = 0;
			//
			// ICMSTaxDetailsUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ICMSGroupBox);
			this.Name = "ICMSTaxDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 177, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ICMSGroupBox.ResumeLayout(false);
			this.ICMSGroupBox.PerformLayout();
			this.ICMSFormulaDropEdit.ResumeLayout(true);
			this.ICMSFormulaDropEdit.PerformLayout();
			this.ICMSLegalBaseDropEdit.ResumeLayout(true);
			this.ICMSLegalBaseDropEdit.PerformLayout();
			this.ICMSTaxRegimeDropEdit.ResumeLayout(true);
			this.ICMSTaxRegimeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ICMSGroupBox;
		internal ZArchitecture.GUI.ZDropEdit ICMSLegalBaseDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ICMSTaxRegimeDropEdit;
		internal ZArchitecture.ZCalcEdit ICMSRateCalcEdit;
		internal ZArchitecture.ZCalcEdit ICMSBaseValueReductionPercentageCalcEdit;
		internal ZArchitecture.ZCalcEdit ICMSTotalAmountReductionPercentageCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit ICMSFormulaDropEdit;
		internal ZArchitecture.GUI.ZButton ICMSFormulaExplanationButton;
	}
}
