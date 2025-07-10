using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class ImportInvoiceLineTemplate
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
			components = new System.ComponentModel.Container();
			this.DutyRateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcedureTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StorageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.CertificateOfOriginPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PreferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OriginCertifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificateOfOriginCertifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StorageTypeDropEdit.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.CertificateOfOriginPanel.SuspendLayout();
			this.PreferenceDropEdit.SuspendLayout();
			this.OriginCertifierDropEdit.SuspendLayout();
			this.CertificateOfOriginCertifierDropEdit.SuspendLayout();
			this.SuspendLayout();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			this.ResumeLayout(false);
			this.PerformLayout();
			// 
			// TariffFindBox
			//
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(null)).JI_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.PreBoundMaxLength = 30;
			this.TariffFindBox.ShouldResize = true;
			this.TariffFindBox.ShowDescriptionBox = true;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 15, true);
			this.TariffFindBox.TabIndex = 3;
			// 
			// DutyRateTextBox
			// 
			this.BindingSource.SetBindingMember(this.DutyRateTextBox, "JI_DutyRateFormula");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DutyRateFormula)));
			this.DutyRateTextBox.CaptionResourceString = null;
			this.DutyRateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 127, true);
			this.DutyRateTextBox.Name = "DutyRateTextBox";
			this.DutyRateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 15, true);
			this.DutyRateTextBox.TabIndex = 3;
			// 
			// ProcedureTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProcedureTextBox, "JI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Procedure)));
			this.ProcedureTextBox.CaptionResourceString = null;
			this.ProcedureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 105, true);
			this.ProcedureTextBox.Name = "ProcedureTextBox";
			this.ProcedureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.ProcedureTextBox.TabIndex = 4;
			// 
			// StorageTypeDropEdit
			// 
			this.StorageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StorageTypeDropEdit, "JI_StorageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_StorageType)));
			this.StorageTypeDropEdit.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("JPImportInvoiceLineUserControl|JI_StorageType", "Storage Type");
			this.StorageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 83, true);
			this.StorageTypeDropEdit.Name = "StorageTypeDropEdit";
			this.StorageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 15, true);
			this.StorageTypeDropEdit.TabIndex = 6;
			// 
			// CertificateOfOriginPanel
			// 
			this.CertificateOfOriginPanel.Controls.Add(this.PreferenceDropEdit);
			this.CertificateOfOriginPanel.Controls.Add(this.OriginCertifierDropEdit);
			this.CertificateOfOriginPanel.Controls.Add(this.CertificateOfOriginCertifierDropEdit);
			this.CertificateOfOriginPanel.Name = "CertificateOfOriginPanel";
			this.CertificateOfOriginPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			// 
			// PreferenceDropEdit
			// 
			this.PreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceDropEdit, "JI_Calc_Preference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Calc_Preference)));
			this.PreferenceDropEdit.Name = "PreferenceDropEdit";
			this.PreferenceDropEdit.PreBoundMaxLength = 2;
			this.PreferenceDropEdit.ShowDescriptionBox = false;
			// 
			// OriginCertifierDropEdit
			// 
			this.OriginCertifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCertifierDropEdit, "JI_Calc_OriginCertifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Calc_OriginCertifier)));
			this.OriginCertifierDropEdit.Name = "OriginCertifierDropEdit";
			this.OriginCertifierDropEdit.CaptionResourceString = null;
			this.OriginCertifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 0, true);
			this.OriginCertifierDropEdit.PreBoundMaxLength = 1;
			this.OriginCertifierDropEdit.ShowDescriptionBox = false;
			// 
			// CertificateOfOriginCertifierDropEdit
			// 
			this.CertificateOfOriginCertifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateOfOriginCertifierDropEdit, "JI_Calc_CertificateOfOriginCertifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Calc_CertificateOfOriginCertifier)));
			this.CertificateOfOriginCertifierDropEdit.Name = "CertificateOfOriginCertifierDropEdit";
			this.CertificateOfOriginCertifierDropEdit.PreBoundMaxLength = 1;
			this.CertificateOfOriginCertifierDropEdit.ShowDescriptionBox = false;
			this.CertificateOfOriginCertifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 0, true);
			this.CertificateOfOriginCertifierDropEdit.CaptionResourceString = null;
			// 
			// ImportInvoiceLineTemplate
			// 
			this.Controls.Add(this.DutyRateTextBox);
			this.Controls.Add(this.ProcedureTextBox);
			this.Controls.Add(this.StorageTypeDropEdit);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.CertificateOfOriginPanel);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.StorageTypeDropEdit.ResumeLayout(true);
			this.StorageTypeDropEdit.PerformLayout();
			this.CertificateOfOriginPanel.ResumeLayout(false);
			this.CertificateOfOriginPanel.PerformLayout();
			this.PreferenceDropEdit.ResumeLayout(true);
			this.PreferenceDropEdit.PerformLayout();
			this.OriginCertifierDropEdit.ResumeLayout(true);
			this.OriginCertifierDropEdit.PerformLayout();
			this.CertificateOfOriginCertifierDropEdit.ResumeLayout(true);
			this.CertificateOfOriginCertifierDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public Enterprise.Customs.Universal.GUI.TariffFindBox TariffFindBox;
		public ZArchitecture.ZTextBox DutyRateTextBox;
		public ZArchitecture.ZTextBox ProcedureTextBox;
		public ZArchitecture.GUI.ZDropEdit StorageTypeDropEdit;
		public ZPanel CertificateOfOriginPanel;
		ZDropEdit PreferenceDropEdit;
		ZDropEdit OriginCertifierDropEdit;
		ZDropEdit CertificateOfOriginCertifierDropEdit;
	}
}
