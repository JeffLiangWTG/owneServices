namespace Enterprise.Customs.BR.GUI
{
	partial class InvoiceLineDetailsAdditionalInfoUserControl
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
			this.TemporaryExportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BRIntendedTerm = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BRDigitalServiceDossiers = new Enterprise.ZArchitecture.ZTextBox();
			this.BR_AgentCommissionCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BRJustificationExportTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_FinancedValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OtherGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FinancingTransactionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TemporaryExportGroupBox.SuspendLayout();
			this.OtherGroupBox.SuspendLayout();
			this.FinancingTransactionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// TemporaryExportGroupBox
			// 
			this.TemporaryExportGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("26CAB769-25B5-40C1-8AFD-296343A92EE2", "Temporary Export");
			this.TemporaryExportGroupBox.Controls.Add(this.BRIntendedTerm);
			this.TemporaryExportGroupBox.Controls.Add(this.BRDigitalServiceDossiers);
			this.TemporaryExportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TemporaryExportGroupBox.Name = "TemporaryExportGroupBox";
			this.TemporaryExportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 73, true);
			this.TemporaryExportGroupBox.TabIndex = 0;
			this.TemporaryExportGroupBox.TabStop = false;
			// 
			// BRIntendedTerm
			// 
			this.BRIntendedTerm.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BRIntendedTerm, "FilteredInvoiceLines.JI_IntendedTermDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IntendedTermDays)));
			this.BRIntendedTerm.CaptionResourceString = null;
			this.BRIntendedTerm.DecimalPlaces = 2;
			this.BRIntendedTerm.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 22, true);
			this.BRIntendedTerm.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.BRIntendedTerm.Name = "BRIntendedTerm";
			this.BRIntendedTerm.ShouldEscapeAllSpecialCharacters = false;
			this.BRIntendedTerm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.BRIntendedTerm.TabIndex = 1;
			this.BRIntendedTerm.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BRDigitalServiceDossiers
			// 
			this.BRDigitalServiceDossiers.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BRDigitalServiceDossiers, "FilteredInvoiceLines.JI_DigitalServiceDossier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DigitalServiceDossier)));
			this.BRDigitalServiceDossiers.CaptionResourceString = null;
			this.BRDigitalServiceDossiers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 45, true);
			this.BRDigitalServiceDossiers.Name = "BRDigitalServiceDossiers";
			this.BRDigitalServiceDossiers.ShouldEscapeAllSpecialCharacters = false;
			this.BRDigitalServiceDossiers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.BRDigitalServiceDossiers.TabIndex = 2;
			// 
			// BR_AgentCommissionCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BR_AgentCommissionCalcEdit, "FilteredInvoiceLines.JI_AgentCommissionPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AgentCommissionPercentage)));
			this.BR_AgentCommissionCalcEdit.CaptionResourceString = null;
			this.BR_AgentCommissionCalcEdit.DecimalPlaces = 2;
			this.BR_AgentCommissionCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 19, true);
			this.BR_AgentCommissionCalcEdit.Name = "BR_AgentCommissionCalcEdit";
			this.BR_AgentCommissionCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.BR_AgentCommissionCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.BR_AgentCommissionCalcEdit.TabIndex = 6;
			this.BR_AgentCommissionCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BRJustificationExportTextBox
			// 
			this.BindingSource.SetBindingMember(this.BRJustificationExportTextBox, "FilteredInvoiceLines.JI_ExportJustificationInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ExportJustificationInfo)));
			this.BRJustificationExportTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6bb8bce2-a906-4726-97b8-bb18acad008b", "Justification for the reported values (VMLE, net weight or quantity in the unit of statistic measure) that are outside statistical tolerance range");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.BRJustificationExportTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.BRJustificationExportTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 67, true);
			this.BRJustificationExportTextBox.Name = "BRJustificationExportTextBox";
			this.BRJustificationExportTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.BRJustificationExportTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 20, true);
			this.BRJustificationExportTextBox.TabIndex = 7;
			// 
			// JI_FinancedValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_FinancedValueCalcEdit, "FilteredInvoiceLines.JI_FinancedValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FinancedValue)));
			this.JI_FinancedValueCalcEdit.CaptionResourceString = null;
			this.JI_FinancedValueCalcEdit.DecimalPlaces = 2;
			this.JI_FinancedValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 21, true);
			this.JI_FinancedValueCalcEdit.Name = "JI_FinancedValueCalcEdit";
			this.JI_FinancedValueCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.JI_FinancedValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.JI_FinancedValueCalcEdit.TabIndex = 4;
			this.JI_FinancedValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OtherGroupBox
			// 
			this.OtherGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0B45F447-D5C3-4504-AA2F-F19F736D329E", "Other");
			this.OtherGroupBox.Controls.Add(this.BRJustificationExportTextBox);
			this.OtherGroupBox.Controls.Add(this.BR_AgentCommissionCalcEdit);
			this.OtherGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 133, true);
			this.OtherGroupBox.Name = "OtherGroupBox";
			this.OtherGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 95, true);
			this.OtherGroupBox.TabIndex = 5;
			this.OtherGroupBox.TabStop = false;
			// 
			// FinancingTransactionGroupBox
			// 
			this.FinancingTransactionGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("BE4FC19F-565B-4D6F-8C5D-5495CE4DA94D", "Financing Transaction");
			this.FinancingTransactionGroupBox.Controls.Add(this.JI_FinancedValueCalcEdit);
			this.FinancingTransactionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 79, true);
			this.FinancingTransactionGroupBox.Name = "FinancingTransactionGroupBox";
			this.FinancingTransactionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 49, true);
			this.FinancingTransactionGroupBox.TabIndex = 3;
			this.FinancingTransactionGroupBox.TabStop = false;
			// 
			// InvoiceLineDetailsAdditionalInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FinancingTransactionGroupBox);
			this.Controls.Add(this.OtherGroupBox);
			this.Controls.Add(this.TemporaryExportGroupBox);
			this.Name = "InvoiceLineDetailsAdditionalInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 252, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TemporaryExportGroupBox.ResumeLayout(false);
			this.TemporaryExportGroupBox.PerformLayout();
			this.OtherGroupBox.ResumeLayout(false);
			this.OtherGroupBox.PerformLayout();
			this.FinancingTransactionGroupBox.ResumeLayout(false);
			this.FinancingTransactionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox TemporaryExportGroupBox;
		private ZArchitecture.ZCalcEdit JI_FinancedValueCalcEdit;
		private ZArchitecture.ZCalcEdit BRIntendedTerm;
		private ZArchitecture.ZTextBox BRDigitalServiceDossiers;
		internal ZArchitecture.ZTextBox BRJustificationExportTextBox;
		private ZArchitecture.ZCalcEdit BR_AgentCommissionCalcEdit;
		private ZArchitecture.GUI.ZGroupBox OtherGroupBox;
		private ZArchitecture.GUI.ZGroupBox FinancingTransactionGroupBox;
	}
}
