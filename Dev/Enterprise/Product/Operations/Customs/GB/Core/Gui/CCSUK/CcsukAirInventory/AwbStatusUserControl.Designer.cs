namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class AwbStatusUserControl
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
			this.Status = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Status2GrantedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Status1DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CustomsActionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LinkLabelEntry = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.PresenceOnNetworkStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsActionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsActionText = new Enterprise.ZArchitecture.ZTextBox();
			this.TemporaryStorageEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Status.SuspendLayout();
			this.Status1DateEdit.SuspendLayout();
			this.CustomsActionDateEdit.SuspendLayout();
			this.PresenceOnNetworkStatusDropEdit.SuspendLayout();
			this.CustomsActionCodeDropEdit.SuspendLayout();
			this.TemporaryStorageEndDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			// 
			// Status
			// 
			this.Status.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("af8934f1-8b49-4fbf-88d5-cf10254d3bf9", "Statuses");
			this.Status.Controls.Add(this.Status2GrantedCheckBox);
			this.Status.Controls.Add(this.Status1DateEdit);
			this.Status.Controls.Add(this.TemporaryStorageEndDateEdit);
			this.Status.Controls.Add(this.CustomsActionDateEdit);
			this.Status.Controls.Add(this.LinkLabelEntry);
			this.Status.Controls.Add(this.PresenceOnNetworkStatusDropEdit);
			this.Status.Controls.Add(this.CustomsActionCodeDropEdit);
			this.Status.Controls.Add(this.CustomsActionText);
			this.Status.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.Status.Name = "Status";
			this.Status.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 101, true);
			this.Status.TabIndex = 6;
			this.Status.TabStop = false;
			// 
			// Status2GrantedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.Status2GrantedCheckBox, "Status2Granted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Status2Granted)));
			this.Status2GrantedCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("60e5de56-5815-48e5-b15a-3a2c3468fb96", "Status 2");
			this.Status2GrantedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Status2GrantedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 84, true);
			this.Status2GrantedCheckBox.Name = "Status2GrantedCheckBox";
			this.Status2GrantedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 15, true);
			this.Status2GrantedCheckBox.TabIndex = 6;
			this.Status2GrantedCheckBox.UseVisualStyleBackColor = true;
			// 
			// Status1DateEdit
			// 
			this.Status1DateEdit.AllowDrop = true;
			this.Status1DateEdit.AutoCompleteMonthThreshold = 1;
			this.Status1DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.Status1DateEdit, "Status1Date");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Status1Date)));
			this.Status1DateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c6c2d69e-ed91-481a-b2d4-b1524e6adb28", "St. 1", "Status 1", "Status 1 Date", "Date of Status 1.  Date when NPR equaled NPX. ");
			this.Status1DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.Status1DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 64, true);
			this.Status1DateEdit.Name = "Status1DateEdit";
			this.Status1DateEdit.TabIndex = 5;
			this.Status1DateEdit.TabStop = false;
			// 
			// CustomsActionDateEdit
			// 
			this.CustomsActionDateEdit.AllowDrop = true;
			this.CustomsActionDateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomsActionDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomsActionDateEdit, "CustomsActionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).CustomsActionDate)));
			this.CustomsActionDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("AwbStatusUserControl|CustomsActionDateEdit", "Date", "CAC Date", "Customs Date", "Date of Customs Action.  The date that the present CAC was set. ");
			this.CustomsActionDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CustomsActionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 38, true);
			this.CustomsActionDateEdit.Name = "CustomsActionDateEdit";
			this.CustomsActionDateEdit.TabIndex = 2;
			this.CustomsActionDateEdit.TabStop = false;
			// 
			// LinkLabelEntry
			// 
			this.LinkLabelEntry.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LinkLabelEntry, "ChiefDeclarationUCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).ChiefDeclarationUCR)));
			this.LinkLabelEntry.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("AwbStatusUserControl|ChiefDeclarationUCR", "DUCR", "Declaration UCR", "UCR of CHIEF declaration", "Declaration unique consignment reference on CHIEF.");
			this.LinkLabelEntry.IsFontBold = false;
			this.LinkLabelEntry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 84, true);
			this.LinkLabelEntry.Name = "LinkLabelEntry";
			this.LinkLabelEntry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 13, true);
			this.LinkLabelEntry.TabIndex = 4;
			this.LinkLabelEntry.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelEntry_LinkClicked);
			// 
			// PresenceOnNetworkStatusDropEdit
			// 
			this.PresenceOnNetworkStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresenceOnNetworkStatusDropEdit, "PresenceOnNetworkStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).PresenceOnNetworkStatus)));
			this.PresenceOnNetworkStatusDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("AwbStatusUserControl|PresenceOnNetworkStatus", "Presence", "Presence Status", "Presence on Network", "Presence. An indication of the job\'s presence on the national network.");
			this.PresenceOnNetworkStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 61, true);
			this.PresenceOnNetworkStatusDropEdit.Name = "PresenceOnNetworkStatusDropEdit";
			this.PresenceOnNetworkStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.PresenceOnNetworkStatusDropEdit.TabIndex = 3;
			this.PresenceOnNetworkStatusDropEdit.TabStop = false;
			// 
			// CustomsActionCodeDropEdit
			// 
			this.CustomsActionCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsActionCodeDropEdit, "CustomsActionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).CustomsActionCode)));
			this.CustomsActionCodeDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("AwbStatusUserControl|CustomsActionCode", "CAC", "Customs Action", "Customs Action Code", "Customs Action Code. The current customs status of the AWB on CCSUK.");
			this.CustomsActionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 14, true);
			this.CustomsActionCodeDropEdit.Name = "CustomsActionCodeDropEdit";
			this.CustomsActionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.CustomsActionCodeDropEdit.TabIndex = 0;
			this.CustomsActionCodeDropEdit.TabStop = false;
			// 
			// CustomsActionText
			// 
			this.BindingSource.SetBindingMember(this.CustomsActionText, "LatestCustomsActionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).LatestCustomsActionText)));
			this.CustomsActionText.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("AwbStatusUserControl|LatestCustomsActionText", "CAT", "Customs Text", "Customs Action Text", "Customs Action Text. The latest customs text of the AWB on CCSUK.");
			this.CustomsActionText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 37, true);
			this.CustomsActionText.Multiline = true;
			this.CustomsActionText.Name = "CustomsActionText";
			this.CustomsActionText.ReadOnly = true;
			this.CustomsActionText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 21, true);
			this.CustomsActionText.TabIndex = 1;
			this.CustomsActionText.TabStop = false;
			// 
			// TemporaryStorageEndDateEdit
			// 
			this.TemporaryStorageEndDateEdit.AllowDrop = true;
			this.TemporaryStorageEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.TemporaryStorageEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TemporaryStorageEndDateEdit, "TemporaryStorageEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).TemporaryStorageEndDate)));
			this.TemporaryStorageEndDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("AwbStatusUserControl|TemporaryStorageEndDate", "Date", "Storage Date", "Temporary Storage End Date");
			this.TemporaryStorageEndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.TemporaryStorageEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 14, true);
			this.TemporaryStorageEndDateEdit.Name = "TemporaryStorageEndDateEdit";
			this.TemporaryStorageEndDateEdit.TabIndex = 2;
			this.TemporaryStorageEndDateEdit.TabStop = false;
			// 
			// AwbStatusUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Status);
			this.Name = "AwbStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Status.ResumeLayout(false);
			this.Status.PerformLayout();
			this.Status1DateEdit.ResumeLayout(true);
			this.Status1DateEdit.PerformLayout();
			this.CustomsActionDateEdit.ResumeLayout(true);
			this.CustomsActionDateEdit.PerformLayout();
			this.PresenceOnNetworkStatusDropEdit.ResumeLayout(true);
			this.PresenceOnNetworkStatusDropEdit.PerformLayout();
			this.CustomsActionCodeDropEdit.ResumeLayout(true);
			this.CustomsActionCodeDropEdit.PerformLayout();
			this.TemporaryStorageEndDateEdit.ResumeLayout(true);
			this.TemporaryStorageEndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox Status;
		private ZArchitecture.GUI.ZLinkLabel LinkLabelEntry;
		private ZArchitecture.GUI.ZDropEdit PresenceOnNetworkStatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit CustomsActionCodeDropEdit;

		private ZArchitecture.ZTextBox CustomsActionText;
		private ZArchitecture.GUI.ZDateEdit CustomsActionDateEdit;
		private ZArchitecture.GUI.ZDateEdit Status1DateEdit;
		private ZArchitecture.GUI.ZCheckBox Status2GrantedCheckBox;
		private ZArchitecture.GUI.ZDateEdit TemporaryStorageEndDateEdit;
	}
}
