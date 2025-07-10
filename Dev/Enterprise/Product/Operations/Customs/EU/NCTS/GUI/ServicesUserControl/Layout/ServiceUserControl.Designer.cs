namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ServiceUserControl
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
			this.MeasurementBasisDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContractorCodeFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.NotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DurationTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.ServiceCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SubLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ServiceLocationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CompletedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BookedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ServiceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateAndCurrencyCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MeasurementBasisDropEdit.SuspendLayout();
			this.ContractorCodeFindBox.SuspendLayout();
			this.ServiceLocationAddressControl.SuspendLayout();
			this.CompletedDateEdit.SuspendLayout();
			this.BookedDateEdit.SuspendLayout();
			this.ServiceTypeDropEdit.SuspendLayout();
			this.RateAndCurrencyCalcFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IHaveServices);
			// 
			// MeasurementBasisDropEdit
			// 
			this.MeasurementBasisDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MeasurementBasisDropEdit, "Services.ES_MeasurementBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_MeasurementBasis)));
			this.MeasurementBasisDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MeasurementBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 212, true);
			this.MeasurementBasisDropEdit.Name = "MeasurementBasisDropEdit";
			this.MeasurementBasisDropEdit.PreBoundMaxLength = 11;
			this.MeasurementBasisDropEdit.ShowDescriptionBox = false;
			this.MeasurementBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.MeasurementBasisDropEdit.TabIndex = 22;
			// 
			// ContractorCodeFindBox
			// 
			this.ContractorCodeFindBox.AllowDrop = true;
			this.ContractorCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContractorCodeFindBox, "Services.ES_OH_Contractor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			this.ContractorCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 58, true);
			this.ContractorCodeFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ContractorCodeFindBox.Name = "ContractorCodeFindBox";
			this.ContractorCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ContractorCodeFindBox.ParentType = null;
			this.ContractorCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 20, true);
			this.ContractorCodeFindBox.TabIndex = 14;
			// 
			// NotesTextBox
			// 
			this.NotesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NotesTextBox, "Services.ES_ServiceNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			this.NotesTextBox.CaptionResourceString = null;
			this.NotesTextBox.IsDynamicMultiline = true;
			this.NotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 236, true);
			this.NotesTextBox.Multiline = true;
			this.NotesTextBox.Name = "NotesTextBox";
			this.NotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 19, true);
			this.NotesTextBox.TabIndex = 23;
			// 
			// ReferenceTextBox
			// 
			this.ReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "Services.ES_References");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_References)));
			this.ReferenceTextBox.CaptionResourceString = null;
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(900, 122, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ReferenceTextBox.TabIndex = 24;
			// 
			// DurationTimeEdit
			// 
			this.DurationTimeEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DurationTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DurationTimeEdit, "Services.ES_Duration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Duration)));
			this.DurationTimeEdit.CaptionResourceString = null;
			this.DurationTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(960, 80, true);
			this.DurationTimeEdit.Name = "DurationTimeEdit";
			this.DurationTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.DurationTimeEdit.TabIndex = 20;
			// 
			// ServiceCountCalcEdit
			// 
			this.ServiceCountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceCountCalcEdit, "Services.ES_ServiceCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			this.ServiceCountCalcEdit.CaptionResourceString = null;
			this.ServiceCountCalcEdit.DecimalPlaces = 2;
			this.ServiceCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(960, 59, true);
			this.ServiceCountCalcEdit.Name = "ServiceCountCalcEdit";
			this.ServiceCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.ServiceCountCalcEdit.TabIndex = 18;
			this.ServiceCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SubLocationTextBox
			// 
			this.SubLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubLocationTextBox, "Services.ES_SubLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_SubLocation)));
			this.SubLocationTextBox.CaptionResourceString = null;
			this.SubLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 110, true);
			this.SubLocationTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SubLocationTextBox.Name = "SubLocationTextBox";
			this.SubLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SubLocationTextBox.TabIndex = 17;
			// 
			// ServiceLocationAddressControl
			// 
			this.ServiceLocationAddressControl.AllowDrop = true;
			this.ServiceLocationAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceLocationAddressControl, "Services.ES_OA_Location");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_OA_Location)));
			this.ServiceLocationAddressControl.BindToOrgList = "Services.Lookups.ServiceProvider";
			this.ServiceLocationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 84, true);
			this.ServiceLocationAddressControl.Name = "ServiceLocationAddressControl";
			this.ServiceLocationAddressControl.PopupCaption = null;
			this.ServiceLocationAddressControl.ShowAddress = false;
			this.ServiceLocationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ServiceLocationAddressControl.TabIndex = 15;
			// 
			// CompletedDateEdit
			// 
			this.CompletedDateEdit.AllowDrop = true;
			this.CompletedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CompletedDateEdit, "Services.ES_Completed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Completed)));
			this.CompletedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 162, true);
			this.CompletedDateEdit.Name = "CompletedDateEdit";
			this.CompletedDateEdit.TabIndex = 19;
			// 
			// BookedDateEdit
			// 
			this.BookedDateEdit.AllowDrop = true;
			this.BookedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.BookedDateEdit, "Services.ES_Booked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Booked)));
			this.BookedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 136, true);
			this.BookedDateEdit.Name = "BookedDateEdit";
			this.BookedDateEdit.TabIndex = 16;
			// 
			// ServiceTypeDropEdit
			// 
			this.ServiceTypeDropEdit.AllowDrop = true;
			this.ServiceTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceTypeDropEdit, "Services.ES_ServiceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			this.ServiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 32, true);
			this.ServiceTypeDropEdit.Name = "ServiceTypeDropEdit";
			this.ServiceTypeDropEdit.PreBoundMaxLength = 3;
			this.ServiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 20, true);
			this.ServiceTypeDropEdit.TabIndex = 13;
			// 
			// RateAndCurrencyCalcFindBox
			// 
			this.RateAndCurrencyCalcFindBox.AllowDrop = true;
			this.RateAndCurrencyCalcFindBox.BindToAmount = "Services.ES_ServiceRate";
			this.RateAndCurrencyCalcFindBox.BindToUnit = "Services.ES_RX_NKServiceRateCurrency";
			this.RateAndCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.RateAndCurrencyCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 188, true);
			this.RateAndCurrencyCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.RateAndCurrencyCalcFindBox.Name = "RateAndCurrencyCalcFindBox";
			this.RateAndCurrencyCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			this.RateAndCurrencyCalcFindBox.TabIndex = 21;
			// 
			// ServiceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MeasurementBasisDropEdit);
			this.Controls.Add(this.ContractorCodeFindBox);
			this.Controls.Add(this.NotesTextBox);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.DurationTimeEdit);
			this.Controls.Add(this.ServiceCountCalcEdit);
			this.Controls.Add(this.SubLocationTextBox);
			this.Controls.Add(this.ServiceLocationAddressControl);
			this.Controls.Add(this.CompletedDateEdit);
			this.Controls.Add(this.BookedDateEdit);
			this.Controls.Add(this.ServiceTypeDropEdit);
			this.Controls.Add(this.RateAndCurrencyCalcFindBox);
			this.Name = "ServiceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 362, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MeasurementBasisDropEdit.ResumeLayout(true);
			this.MeasurementBasisDropEdit.PerformLayout();
			this.ContractorCodeFindBox.ResumeLayout(true);
			this.ContractorCodeFindBox.PerformLayout();
			this.ServiceLocationAddressControl.ResumeLayout(true);
			this.ServiceLocationAddressControl.PerformLayout();
			this.CompletedDateEdit.ResumeLayout(true);
			this.CompletedDateEdit.PerformLayout();
			this.BookedDateEdit.ResumeLayout(true);
			this.BookedDateEdit.PerformLayout();
			this.ServiceTypeDropEdit.ResumeLayout(true);
			this.ServiceTypeDropEdit.PerformLayout();
			this.RateAndCurrencyCalcFindBox.ResumeLayout(true);
			this.RateAndCurrencyCalcFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit MeasurementBasisDropEdit;
		internal MasterFiles.GUI.ZOrganisationFindBox ContractorCodeFindBox;
		internal ZArchitecture.ZTextBox NotesTextBox;
		internal ZArchitecture.ZTextBox ReferenceTextBox;
		internal ZArchitecture.GUI.ZTimeEdit DurationTimeEdit;
		internal ZArchitecture.ZCalcEdit ServiceCountCalcEdit;
		internal ZArchitecture.ZTextBox SubLocationTextBox;
		internal ZArchitecture.GUI.ZAddressControl ServiceLocationAddressControl;
		internal ZArchitecture.GUI.ZDateEdit CompletedDateEdit;
		internal ZArchitecture.GUI.ZDateEdit BookedDateEdit;
		internal ZArchitecture.GUI.ZDropEdit ServiceTypeDropEdit;
		internal ZArchitecture.GUI.ZCalcFindBox RateAndCurrencyCalcFindBox;
	}
}
