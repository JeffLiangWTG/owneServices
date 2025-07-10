namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class SplitConsignmentUserControl
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
		void InitializeComponent()
		{
			this.AwbNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SplitReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NPXTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NPRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CatTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CacDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HandlingInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChiefDucrLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.Status1DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CustomsActionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AgentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PresenceOnNetworkStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TemporaryStorageEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CacDropEdit1.SuspendLayout();
			this.Status1DateEdit.SuspendLayout();
			this.CustomsActionDateEdit.SuspendLayout();
			this.AgentCodeFindBox.SuspendLayout();
			this.TemporaryStorageEndDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment);
			// 
			// AwbNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.AwbNumberTextBox, "AWB.ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).AWB.ReferenceNumber)));
			this.AwbNumberTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|05f0bad7-154b-4a6b-9255-8a2e1a193e4c", "AWB Number");
			this.AwbNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 3, true);
			this.AwbNumberTextBox.Name = "AwbNumberTextBox";
			this.AwbNumberTextBox.ReadOnly = true;
			this.AwbNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.AwbNumberTextBox.TabIndex = 0;
			this.AwbNumberTextBox.TabStop = false;
			// 
			// SplitReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SplitReferenceTextBox, "SplitReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).SplitReference)));
			this.SplitReferenceTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|35da44a8-5e23-4448-8437-9a461c8b3f19", "Split Reference");
			this.SplitReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 31, true);
			this.SplitReferenceTextBox.Name = "SplitReferenceTextBox";
			this.SplitReferenceTextBox.ReadOnly = true;
			this.SplitReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.SplitReferenceTextBox.TabIndex = 1;
			this.SplitReferenceTextBox.TabStop = false;
			// 
			// WeightUnitTextBox
			// 
			this.BindingSource.SetBindingMember(this.WeightUnitTextBox, "WeightCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).WeightCode)));
			this.WeightUnitTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|d5bce87b-5872-4a6c-a2bf-8e78de8047f3", "Weight Unit");
			this.WeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 59, true);
			this.WeightUnitTextBox.Name = "WeightUnitTextBox";
			this.WeightUnitTextBox.ReadOnly = true;
			this.WeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.WeightUnitTextBox.TabIndex = 4;
			this.WeightUnitTextBox.TabStop = false;
			// 
			// WeightTextBox
			// 
			this.BindingSource.SetBindingMember(this.WeightTextBox, "Weight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).Weight)));
			this.WeightTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|45444626-7dc1-437d-94e3-4362f21e6627", "Weight");
			this.WeightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 59, true);
			this.WeightTextBox.Name = "WeightTextBox";
			this.WeightTextBox.ReadOnly = true;
			this.WeightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.WeightTextBox.TabIndex = 3;
			this.WeightTextBox.TabStop = false;
			// 
			// NPXTextBox
			// 
			this.BindingSource.SetBindingMember(this.NPXTextBox, "NumberOfPiecesExpected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).NumberOfPiecesExpected)));
			this.NPXTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|a569a2f6-4ecf-4f33-8265-901654567093", "NPX");
			this.NPXTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 87, true);
			this.NPXTextBox.Name = "NPXTextBox";
			this.NPXTextBox.ReadOnly = true;
			this.NPXTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.NPXTextBox.TabIndex = 5;
			this.NPXTextBox.TabStop = false;
			// 
			// NPRTextBox
			// 
			this.BindingSource.SetBindingMember(this.NPRTextBox, "NumberOfPiecesReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).NumberOfPiecesReceived)));
			this.NPRTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|2f9bf855-a012-49b6-89f8-82621e6e49f4", "NPR");
			this.NPRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 87, true);
			this.NPRTextBox.Name = "NPRTextBox";
			this.NPRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.NPRTextBox.TabIndex = 6;
			NPRTextBox.ReadOnly = true; // Do not allow this line to get removed by the designer. It is vital. 
			// 
			// CatTextBox
			// 
			this.BindingSource.SetBindingMember(this.CatTextBox, "LatestCustomsActionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).LatestCustomsActionText)));
			this.CatTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|5a9a5465-c168-4504-b1d9-e95549e98579", "CAT", "Customs Action Text.");
			this.CatTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 143, true);
			this.CatTextBox.Name = "CatTextBox";
			this.CatTextBox.ReadOnly = true;
			this.CatTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.CatTextBox.TabIndex = 9;
			this.CatTextBox.TabStop = false;
			// 
			// CacDropEdit1
			// 
			this.CacDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CacDropEdit1, "CustomsActionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).CustomsActionCode)));
			this.CacDropEdit1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|91f6a2cf-f90c-41cf-bafe-e6d63beb6ce0", "CAC", "Customs Action Code.");
			this.CacDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 115, true);
			this.CacDropEdit1.Name = "CacDropEdit1";
			this.CacDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.CacDropEdit1.TabIndex = 8;
			this.CacDropEdit1.TabStop = false;
			// 
			// HandlingInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.HandlingInformationTextBox, "HandlingInformationForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).HandlingInformationForBinding)));
			this.HandlingInformationTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|28203a23-8d6c-438d-8636-e369d97a78ab", "Handling", "Handling Information", "Handling Information (from shed).");
			this.HandlingInformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HandlingInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 202, true);
			this.HandlingInformationTextBox.Name = "HandlingInformationTextBox";
			this.HandlingInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.HandlingInformationTextBox.TabIndex = 12;
			// 
			// ChiefDucrLinkLabel
			// 
			this.ChiefDucrLinkLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChiefDucrLinkLabel, "ChiefDeclarationUCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).ChiefDeclarationUCR)));
			this.ChiefDucrLinkLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentUserControl|ChiefDeclarationUCR", "DUCR", "CHIEF DUCR", "CHIEF Declaration UCR", "CHIEF Declaration Unique Consignment Reference");
			this.ChiefDucrLinkLabel.IsFontBold = false;
			this.ChiefDucrLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 255, true);
			this.ChiefDucrLinkLabel.Name = "ChiefDucrLinkLabel";
			this.ChiefDucrLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 13, true);
			this.ChiefDucrLinkLabel.TabIndex = 14;
			this.ChiefDucrLinkLabel.Text = "0GB000000000000-XXXXXXXXXXXXXXXXXXXXXX";
			this.ChiefDucrLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChiefDucrLinkLabel_LinkClicked);
			// 
			// Status1DateEdit
			// 
			this.Status1DateEdit.AllowDrop = true;
			this.Status1DateEdit.AutoCompleteMonthThreshold = 1;
			this.Status1DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.Status1DateEdit, "Status1Date");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).Status1Date)));
			this.Status1DateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("82d4d8e2-e51b-4806-a175-5087773507d9", "Status 1");
			this.Status1DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.Status1DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 87, true);
			this.Status1DateEdit.Name = "Status1DateEdit";
			this.Status1DateEdit.TabIndex = 7;
			this.Status1DateEdit.TabStop = false;
			// 
			// CustomsActionDateEdit
			// 
			this.CustomsActionDateEdit.AllowDrop = true;
			this.CustomsActionDateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomsActionDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomsActionDateEdit, "CustomsActionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).CustomsActionDate)));
			this.CustomsActionDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("828d1da1-0e22-431d-8bba-97f18b0d74b6", "Customs Date");
			this.CustomsActionDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CustomsActionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 169, true);
			this.CustomsActionDateEdit.Name = "CustomsActionDateEdit";
			this.CustomsActionDateEdit.TabIndex = 10;
			this.CustomsActionDateEdit.TabStop = false;
			// 
			// AgentCodeFindBox
			// 
			this.AgentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgentCodeFindBox, "AgentBadge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).AgentBadge)));
			this.AgentCodeFindBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirportsAndPartiesControl|804741a2-5869-4b74-90f4-210dfcd79928", "Agt.", "Agent", "Nominated Agent", "Current Nominated Agent.");
			this.AgentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 31, true);
			this.AgentCodeFindBox.Name = "AgentCodeFindBox";
			this.AgentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.AgentCodeFindBox.TabIndex = 2;
			// 
			// PresenceOnNetworkStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.PresenceOnNetworkStatusTextBox, "PresenceOnNetworkStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).PresenceOnNetworkStatus)));
			this.PresenceOnNetworkStatusTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("39df0394-a9bf-4417-9259-69493560d4cd", "Presence");
			this.PresenceOnNetworkStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PresenceOnNetworkStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 169, true);
			this.PresenceOnNetworkStatusTextBox.Name = "PresenceOnNetworkStatusTextBox";
			this.PresenceOnNetworkStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PresenceOnNetworkStatusTextBox.TabIndex = 11;
			// 
			// TemporaryStorageEndDateEdit
			// 
			this.TemporaryStorageEndDateEdit.AllowDrop = true;
			this.TemporaryStorageEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.TemporaryStorageEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TemporaryStorageEndDateEdit, "TemporaryStorageEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(null)).TemporaryStorageEndDate)));
			this.TemporaryStorageEndDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2c2df67c-2062-4ad8-ba78-532287a0ae1e", "Storage", "Storage Date", "Temporary Storage End Date");
			this.TemporaryStorageEndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.TemporaryStorageEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 232, true);
			this.TemporaryStorageEndDateEdit.Name = "TemporaryStorageEndDateEdit";
			this.TemporaryStorageEndDateEdit.TabIndex = 13;
			this.TemporaryStorageEndDateEdit.TabStop = false;
			// 
			// SplitConsignmentUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TemporaryStorageEndDateEdit);
			this.Controls.Add(this.PresenceOnNetworkStatusTextBox);
			this.Controls.Add(this.AgentCodeFindBox);
			this.Controls.Add(this.CustomsActionDateEdit);
			this.Controls.Add(this.Status1DateEdit);
			this.Controls.Add(this.ChiefDucrLinkLabel);
			this.Controls.Add(this.HandlingInformationTextBox);
			this.Controls.Add(this.CacDropEdit1);
			this.Controls.Add(this.CatTextBox);
			this.Controls.Add(this.NPRTextBox);
			this.Controls.Add(this.NPXTextBox);
			this.Controls.Add(this.WeightTextBox);
			this.Controls.Add(this.WeightUnitTextBox);
			this.Controls.Add(this.SplitReferenceTextBox);
			this.Controls.Add(this.AwbNumberTextBox);
			this.Name = "SplitConsignmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 290, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CacDropEdit1.ResumeLayout(true);
			this.CacDropEdit1.PerformLayout();
			this.Status1DateEdit.ResumeLayout(true);
			this.Status1DateEdit.PerformLayout();
			this.CustomsActionDateEdit.ResumeLayout(true);
			this.CustomsActionDateEdit.PerformLayout();
			this.AgentCodeFindBox.ResumeLayout(true);
			this.AgentCodeFindBox.PerformLayout();
			this.TemporaryStorageEndDateEdit.ResumeLayout(true);
			this.TemporaryStorageEndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox AwbNumberTextBox;
		private ZArchitecture.ZTextBox SplitReferenceTextBox;
		private ZArchitecture.ZTextBox WeightUnitTextBox;
		private ZArchitecture.ZTextBox WeightTextBox;
		private ZArchitecture.ZTextBox NPXTextBox;
		private ZArchitecture.ZTextBox NPRTextBox;
		private ZArchitecture.ZTextBox CatTextBox;
		private ZArchitecture.GUI.ZDropEdit CacDropEdit1;
		private ZArchitecture.ZTextBox HandlingInformationTextBox;
		private ZArchitecture.GUI.ZLinkLabel ChiefDucrLinkLabel;
		private ZArchitecture.GUI.ZDateEdit Status1DateEdit;
		private ZArchitecture.GUI.ZDateEdit CustomsActionDateEdit;
		private ZArchitecture.GUI.ZDropEdit AgentCodeFindBox;
		private ZArchitecture.ZTextBox PresenceOnNetworkStatusTextBox;
		private ZArchitecture.GUI.ZDateEdit TemporaryStorageEndDateEdit;
	}
}
