namespace Enterprise.Customs.KR.GUI
{
	partial class EntryDetailsUserControl
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
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntrySubmittedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AcceptedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CustomsReviewDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TotalCustomsValueKRWCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalGrossWeightInKGCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPackagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomerOfficerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsRemarkTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationLoadingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTypeDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.EntryStatusDropEdit.SuspendLayout();
			this.EntrySubmittedDateEdit.SuspendLayout();
			this.AcceptedDateEdit.SuspendLayout();
			this.CustomsReviewDateEdit.SuspendLayout();
			this.DeclarationLoadingDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "CustomsEntryHeaders.FormattedEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedEntryNumber)));
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 11, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 17, true);
			this.EntryNumberTextBox.TabIndex = 1;
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "CustomsEntryHeaders.CH_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageType)));
			this.MessageTypeDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("59D09E2F-5E33-4B2A-BEEF-FD97064D25B1", "Message Type");
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 36, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 1;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 17, true);
			this.MessageTypeDropEdit.TabIndex = 2;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "CustomsEntryHeaders.CH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_Status)));
			this.MessageStatusDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("EB11E6C5-4546-4F19-810D-1BF0EA0095A5", "Message Status");
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 61, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.PreBoundMaxLength = 3;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 17, true);
			this.MessageStatusDropEdit.TabIndex = 3;
			// 
			// EntryStatusDropEdit
			// 
			this.EntryStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "CustomsEntryHeaders.CH_EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			this.EntryStatusDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("BAE3EB59-0ABF-4255-B212-3C3180C24EE3", "Entry Status");
			this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 86, true);
			this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
			this.EntryStatusDropEdit.PreBoundMaxLength = 3;
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 17, true);
			this.EntryStatusDropEdit.TabIndex = 4;
			// 
			// EntrySubmittedDateEdit
			// 
			this.EntrySubmittedDateEdit.AllowDrop = true;
			this.EntrySubmittedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EntrySubmittedDateEdit, "CustomsEntryHeaders.CH_EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntrySubmittedDate)));
			this.EntrySubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 110, true);
			this.EntrySubmittedDateEdit.Name = "EntrySubmittedDateEdit";
			this.EntrySubmittedDateEdit.TabIndex = 5;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CustomsEntryHeaders.FormattedRefNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedRefNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d95319ac-d4fc-475a-b373-1a60c9d61945", "Customs Reference Number");
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 135, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 17, true);
			this.ReferenceNumberTextBox.TabIndex = 6;
			// 
			// AcceptedDateEdit
			// 
			this.AcceptedDateEdit.AllowDrop = true;
			this.AcceptedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptedDateEdit, "CustomsEntryHeaders.AcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AcceptedDate)));
			this.AcceptedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 160, true);
			this.AcceptedDateEdit.Name = "AcceptedDateEdit";
			this.AcceptedDateEdit.TabIndex = 7;
			// 
			// CustomsReviewDateEdit
			// 
			this.CustomsReviewDateEdit.AllowDrop = true;
			this.CustomsReviewDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CustomsReviewDateEdit, "CustomsEntryHeaders.MostRecentCustomsReviewDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MostRecentCustomsReviewDate)));
			this.CustomsReviewDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 185, true);
			this.CustomsReviewDateEdit.Name = "CustomsReviewDateEdit";
			this.CustomsReviewDateEdit.TabIndex = 8;
			// 
			// TotalCustomsValueKRWCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCustomsValueKRWCalcEdit, "CustomsEntryHeaders.CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsValue)));
			this.TotalCustomsValueKRWCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 210, true);
			this.TotalCustomsValueKRWCalcEdit.Name = "TotalCustomsValueKRWCalcEdit";
			this.TotalCustomsValueKRWCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
			this.TotalCustomsValueKRWCalcEdit.TabIndex = 9;
			this.TotalCustomsValueKRWCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCustomsValueKRWCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalGrossWeightInKGCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossWeightInKGCalcEdit, "CustomsEntryHeaders.TotalGrossWeightInKG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalGrossWeightInKG)));
			this.TotalGrossWeightInKGCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 11, true);
			this.TotalGrossWeightInKGCalcEdit.Name = "TotalGrossWeightInKGCalcEdit";
			this.TotalGrossWeightInKGCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.TotalGrossWeightInKGCalcEdit.TabIndex = 10;
			this.TotalGrossWeightInKGCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalGrossWeightInKGCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalPackagesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackagesCalcEdit, "CustomsEntryHeaders.TotalPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalPackages)));
			this.TotalPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 36, true);
			this.TotalPackagesCalcEdit.Name = "TotalPackagesCalcEdit";
			this.TotalPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.TotalPackagesCalcEdit.TabIndex = 11;
			this.TotalPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalPackagesCalcEdit.TrackDisposedAccess = true;
			// 
			// CustomsOfficerTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomerOfficerTextBox, "CustomsEntryHeaders.ResponsibleCustomsOfficer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ResponsibleCustomsOfficer)));
			this.CustomerOfficerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 61, true);
			this.CustomerOfficerTextBox.Name = "CustomerOfficerTextBox";
			this.CustomerOfficerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.CustomerOfficerTextBox.TabIndex = 12;
			// 
			// CustomsRemarkTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsRemarkTextBox, "CustomsEntryHeaders.CH_CustomsMessageRemarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_CustomsMessageRemarks)));
			this.CustomsRemarkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 86, true);
			this.CustomsRemarkTextBox.Multiline = true;
			this.CustomsRemarkTextBox.Name = "CustomsRemarkTextBox";
			this.CustomsRemarkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 116, true);
			this.CustomsRemarkTextBox.TabIndex = 13;
			// 
			// DeclarationLoadingDateEdit
			// 
			this.DeclarationLoadingDateEdit.AllowDrop = true;
			this.DeclarationLoadingDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DeclarationLoadingDateEdit, "CustomsEntryHeaders.KR_ActualDateOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).KR_ActualDateOfLoading)));
			this.DeclarationLoadingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 210, true);
			this.DeclarationLoadingDateEdit.Name = "DeclarationLoadingDateEdit";
			this.DeclarationLoadingDateEdit.TabIndex = 14;
			// 
			// EntryDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DeclarationLoadingDateEdit);
			this.Controls.Add(this.CustomsRemarkTextBox);
			this.Controls.Add(this.CustomerOfficerTextBox);
			this.Controls.Add(this.TotalPackagesCalcEdit);
			this.Controls.Add(this.TotalGrossWeightInKGCalcEdit);
			this.Controls.Add(this.TotalCustomsValueKRWCalcEdit);
			this.Controls.Add(this.CustomsReviewDateEdit);
			this.Controls.Add(this.AcceptedDateEdit);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.EntrySubmittedDateEdit);
			this.Controls.Add(this.EntryStatusDropEdit);
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Controls.Add(this.MessageTypeDropEdit);
			this.Controls.Add(this.EntryNumberTextBox);
			this.Name = "EntryDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.EntryStatusDropEdit.ResumeLayout(true);
			this.EntryStatusDropEdit.PerformLayout();
			this.EntrySubmittedDateEdit.ResumeLayout(true);
			this.EntrySubmittedDateEdit.PerformLayout();
			this.AcceptedDateEdit.ResumeLayout(true);
			this.AcceptedDateEdit.PerformLayout();
			this.CustomsReviewDateEdit.ResumeLayout(true);
			this.CustomsReviewDateEdit.PerformLayout();
			this.DeclarationLoadingDateEdit.ResumeLayout(true);
			this.DeclarationLoadingDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox EntryNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		private ZArchitecture.GUI.ZDateEdit EntrySubmittedDateEdit;
		private ZArchitecture.ZTextBox ReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit AcceptedDateEdit;
		private ZArchitecture.GUI.ZDateEdit CustomsReviewDateEdit;
		private ZArchitecture.ZCalcEdit TotalCustomsValueKRWCalcEdit;
		private ZArchitecture.ZCalcEdit TotalGrossWeightInKGCalcEdit;
		private ZArchitecture.ZCalcEdit TotalPackagesCalcEdit;
		private ZArchitecture.ZTextBox CustomerOfficerTextBox;
		private ZArchitecture.ZTextBox CustomsRemarkTextBox;
		private ZArchitecture.GUI.ZDateEdit DeclarationLoadingDateEdit;
	}
}
