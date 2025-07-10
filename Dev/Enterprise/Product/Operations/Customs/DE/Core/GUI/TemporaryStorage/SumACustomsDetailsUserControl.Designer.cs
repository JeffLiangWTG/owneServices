namespace Enterprise.Customs.DE.GUI
{
	partial class SumACustomsDetailsUserControl
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
			this.CustomsDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PreviousRefTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.PreviousRefNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NCTSFlagCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AdditionalInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PresentationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsDetailsGroupBox.SuspendLayout();
			this.EntryCustomsOfficeCodeFindBox.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.PreviousRefTypeDropEdit.SuspendLayout();
			this.PresentationDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// CustomsDetailsGroupBox
			// 
			this.CustomsDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("6b92a397-49a6-4163-931b-3b85bdb15bef", "Customs Details");
			this.CustomsDetailsGroupBox.Controls.Add(this.EntryCustomsOfficeCodeFindBox);
			this.CustomsDetailsGroupBox.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.CustomsDetailsGroupBox.Controls.Add(this.PreviousRefTypeDropEdit);
			this.CustomsDetailsGroupBox.Controls.Add(this.PreviousRefNumTextBox);
			this.CustomsDetailsGroupBox.Controls.Add(this.NCTSFlagCheckBox);
			this.CustomsDetailsGroupBox.Controls.Add(this.AdditionalInfoTextBox);
			this.CustomsDetailsGroupBox.Controls.Add(this.PresentationDateDateEdit);
			this.CustomsDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsDetailsGroupBox.Name = "CustomsDetailsGroupBox";
			this.CustomsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 237, true);
			this.CustomsDetailsGroupBox.TabIndex = 101;
			this.CustomsDetailsGroupBox.TabStop = false;
			// 
			// EntryCustomsOfficeCodeFindBox
			// 
			this.EntryCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryCustomsOfficeCodeFindBox, "SJH_CustomsOfficeOfEntryIntoEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_CustomsOfficeOfEntryIntoEU)));
			this.EntryCustomsOfficeCodeFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("55beff59-2a9e-48fa-b613-1ff1be4af31c", "Entry Customs Office");
			this.EntryCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 43, true);
			this.EntryCustomsOfficeCodeFindBox.Name = "EntryCustomsOfficeCodeFindBox";
			this.EntryCustomsOfficeCodeFindBox.ShouldResize = false;
			this.EntryCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 20, true);
			this.EntryCustomsOfficeCodeFindBox.TabIndex = 2;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "SJH_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 17, true);
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ShouldResize = false;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 20, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 1;
			// 
			// PreviousRefTypeDropEdit
			// 
			this.PreviousRefTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreviousRefTypeDropEdit, "SJH_PreviousReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_PreviousReferenceType)));
			this.PreviousRefTypeDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("9f9715ae-ab6c-47f1-9b0d-ded35847981a", "Previous Ref. Type");
			this.PreviousRefTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 179, true);
			this.PreviousRefTypeDropEdit.Name = "PreviousRefTypeDropEdit";
			this.PreviousRefTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PreviousRefTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 20, true);
			this.PreviousRefTypeDropEdit.TabIndex = 6;
			// 
			// PreviousRefNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousRefNumTextBox, "SJH_PreviousReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_PreviousReferenceNumber)));
			this.PreviousRefNumTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("6ceaae3a-0b19-4f9c-b2eb-238070ce7c35", "Previous Ref. Number");
			this.PreviousRefNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 204, true);
			this.PreviousRefNumTextBox.Name = "PreviousRefNumTextBox";
			this.PreviousRefNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 20, true);
			this.PreviousRefNumTextBox.TabIndex = 7;
			// 
			// NCTSFlagCheckBox
			// 
			this.NCTSFlagCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NCTSFlagCheckBox, "SJH_NCTSFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_NCTSFlag)));
			this.NCTSFlagCheckBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("bf9fe5b8-c68e-45d5-9da4-ddda0cbdd075", "NCTS-Flag");
			this.NCTSFlagCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NCTSFlagCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NCTSFlagCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 160, true);
			this.NCTSFlagCheckBox.Name = "NCTSFlagCheckBox";
			this.NCTSFlagCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.NCTSFlagCheckBox.TabIndex = 5;
			this.NCTSFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// AdditionalInfoTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalInfoTextBox, "SJH_AdditionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_AdditionalInformation)));
			this.AdditionalInfoTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("825a185d-ad2b-4d63-9ac7-6f08a36a9227", "Additional Information");
			this.AdditionalInfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 92, true);
			this.AdditionalInfoTextBox.Multiline = true;
			this.AdditionalInfoTextBox.Name = "AdditionalInfoTextBox";
			this.AdditionalInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 63, true);
			this.AdditionalInfoTextBox.TabIndex = 4;
			// 
			// PresentationDateDateEdit
			// 
			this.PresentationDateDateEdit.AllowDrop = true;
			this.PresentationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PresentationDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PresentationDateDateEdit, "SJH_PresentationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_PresentationDate)));
			this.PresentationDateDateEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b98aa1c2-4d88-42f3-b700-06ef1f1ddbb1", "Presentation Date");
			this.PresentationDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 68, true);
			this.PresentationDateDateEdit.Name = "PresentationDateDateEdit";
			this.PresentationDateDateEdit.TabIndex = 3;
			// 
			// SumACustomsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsDetailsGroupBox);
			this.Name = "SumACustomsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 237, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsDetailsGroupBox.ResumeLayout(false);
			this.CustomsDetailsGroupBox.PerformLayout();
			this.EntryCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.EntryCustomsOfficeCodeFindBox.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.PreviousRefTypeDropEdit.ResumeLayout(true);
			this.PreviousRefTypeDropEdit.PerformLayout();
			this.PresentationDateDateEdit.ResumeLayout(true);
			this.PresentationDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CustomsDetailsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox EntryCustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth PreviousRefTypeDropEdit;
		private ZArchitecture.ZTextBox PreviousRefNumTextBox;
		private ZArchitecture.GUI.ZCheckBox NCTSFlagCheckBox;
		private ZArchitecture.ZTextBox AdditionalInfoTextBox;
		private ZArchitecture.GUI.ZDateEdit PresentationDateDateEdit;
	}
}
