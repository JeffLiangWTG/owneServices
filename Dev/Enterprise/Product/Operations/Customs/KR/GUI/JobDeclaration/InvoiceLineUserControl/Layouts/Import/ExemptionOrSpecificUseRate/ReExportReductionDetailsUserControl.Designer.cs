using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class ReExportReductionDetailsUserControl
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
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DestCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.EstimateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.DestCountryCodeFindBox.SuspendLayout();
            this.EstimateDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "FilteredInvoiceLines.JI_ScheduledReExportCustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ScheduledReExportCustomsOffice)));
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 9, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsOfficeCodeFindBox.ShowDescriptionBox = false;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 38, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 0;
            // 
            // DestCountryCodeFindBox
            // 
            this.DestCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DestCountryCodeFindBox, "FilteredInvoiceLines.JI_RN_NKReExportDestinationCountry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RN_NKReExportDestinationCountry)));
            this.DestCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 9, true);
            this.DestCountryCodeFindBox.Name = "DestCountryCodeFindBox";
            this.DestCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DestCountryCodeFindBox.ParentType = null;
            this.DestCountryCodeFindBox.PreBoundMaxLength = 2;
            this.DestCountryCodeFindBox.ShowDescriptionBox = false;
            this.DestCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 38, true);
            this.DestCountryCodeFindBox.TabIndex = 1;
            // 
            // EstimateDateEdit
            // 
            this.EstimateDateEdit.AllowDrop = true;
            this.EstimateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EstimateDateEdit, "FilteredInvoiceLines.JI_ScheduledReExportDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ScheduledReExportDate)));
            this.EstimateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 53, true);
            this.EstimateDateEdit.Name = "EstimateDateEdit";
            this.EstimateDateEdit.TabIndex = 2;
            // 
            // ReExportReductionDetailsUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.EstimateDateEdit);
            this.Controls.Add(this.DestCountryCodeFindBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Name = "ReExportReductionDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 81, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.DestCountryCodeFindBox.ResumeLayout(true);
            this.DestCountryCodeFindBox.PerformLayout();
            this.EstimateDateEdit.ResumeLayout(true);
            this.EstimateDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZCodeFindBox CustomsOfficeCodeFindBox;
		internal ZCodeFindBox DestCountryCodeFindBox;
		internal ZDateEdit EstimateDateEdit;
	}
}
