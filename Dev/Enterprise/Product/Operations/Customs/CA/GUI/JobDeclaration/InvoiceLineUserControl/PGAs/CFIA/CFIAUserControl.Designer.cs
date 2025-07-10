namespace Enterprise.Customs.CA.GUI
{
	partial class CFIAUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DeliveryAddressUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.AirsExtensionCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirsEndUseCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AirsMicellaneousCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SourceCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SourceStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CFIARegNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LpcoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.AIRSGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AIRSToolLinkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryAddressUserControl.SuspendLayout();
			this.AirsEndUseCodeFindBox.SuspendLayout();
			this.AirsMicellaneousCodeFindBox.SuspendLayout();
			this.SourceCountryCodeFindBox.SuspendLayout();
			this.SourceStateDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CFIARegNumbersGrid)).BeginInit();
			this.CFIARegNumbersGrid.SuspendLayout();
			this.LpcoGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.AIRSGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CFIAPGAHeader);
			// 
			// DeliveryAddressUserControl
			// 
			this.DeliveryAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryAddressUserControl, "InvoiceLine.JI_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).InvoiceLine.JI_OA_ConsigneeAddress)));
			this.DeliveryAddressUserControl.BindToOrgList = "InvoiceLine.Lookups.Consignees";
			this.DeliveryAddressUserControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("EA9382D9-70FD-4D21-9908-8A8CAE100111", "Delivery Location");
			this.DeliveryAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 97, true);
			this.DeliveryAddressUserControl.Name = "DeliveryAddressUserControl";
			this.DeliveryAddressUserControl.PopupCaption = "";
			this.DeliveryAddressUserControl.ReadOnly = false;
			this.DeliveryAddressUserControl.ShowAddress = false;
			this.DeliveryAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DeliveryAddressUserControl.TabIndex = 9;
			// 
			// AirsExtensionCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirsExtensionCodeBoundTextBox, "CA_AIRSExtensionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).CA_AIRSExtensionCode)));
			this.AirsExtensionCodeBoundTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("E565228E-F9AD-4632-B301-E92836D6FAAF", "AIRS Extension Code");
			this.AirsExtensionCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 71, true);
			this.AirsExtensionCodeBoundTextBox.Name = "AirsExtensionCodeBoundTextBox";
			this.AirsExtensionCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.AirsExtensionCodeBoundTextBox.TabIndex = 7;
			// 
			// AirsEndUseCodeFindBox
			// 
			this.AirsEndUseCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirsEndUseCodeFindBox, "CA_AIRSEndUse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).CA_AIRSEndUse)));
			this.AirsEndUseCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DA835807-237A-48D6-8DBF-3B508B2B9E50", "AIRS End Use");
			this.AirsEndUseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 45, true);
			this.AirsEndUseCodeFindBox.Name = "AirsEndUseCodeFindBox";
			this.AirsEndUseCodeFindBox.PreBoundMaxLength = 3;
			this.AirsEndUseCodeFindBox.ShouldResize = true;
			this.AirsEndUseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.AirsEndUseCodeFindBox.TabIndex = 3;
			// 
			// AirsMicellaneousCodeFindBox
			// 
			this.AirsMicellaneousCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirsMicellaneousCodeFindBox, "CA_AIRSMiscellaneous");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).CA_AIRSMiscellaneous)));
			this.AirsMicellaneousCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("D30DDA57-3EAA-4F9A-B272-E36F722F3301", "AIRS Miscellaneous");
			this.AirsMicellaneousCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(613, 71, true);
			this.AirsMicellaneousCodeFindBox.Name = "AirsMicellaneousCodeFindBox";
			this.AirsMicellaneousCodeFindBox.PreBoundMaxLength = 3;
			this.AirsMicellaneousCodeFindBox.ShouldResize = true;
			this.AirsMicellaneousCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.AirsMicellaneousCodeFindBox.TabIndex = 8;
			// 
			// SourceCountryCodeFindBox
			// 
			this.SourceCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SourceCountryCodeFindBox, "RN_NKCountryOfSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).RN_NKCountryOfSource)));
			this.SourceCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("79BB54D2-E44A-4C89-93BD-79AF94107757", "Country/Region Of Source");
			this.SourceCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 19, true);
			this.SourceCountryCodeFindBox.Name = "SourceCountryCodeFindBox";
			this.SourceCountryCodeFindBox.PreBoundMaxLength = 3;
			this.SourceCountryCodeFindBox.ShouldResize = true;
			this.SourceCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.SourceCountryCodeFindBox.TabIndex = 1;
			// 
			// SourceStateDropEdit
			// 
			this.SourceStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SourceStateDropEdit, "RW_NKSourceState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).RW_NKSourceState)));
			this.SourceStateDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("57F652FF-98E1-4226-A144-6E719A4CB3CF", "State of Source");
			this.SourceStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(613, 19, true);
			this.SourceStateDropEdit.Name = "SourceStateDropEdit";
			this.SourceStateDropEdit.PreBoundMaxLength = 3;
			this.SourceStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.SourceStateDropEdit.TabIndex = 2;
			// 
			// CFIARegNumbersGrid
			// 
			this.CFIARegNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CFIARegNumbersGrid, "AIRSRegistrationNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).AIRSRegistrationNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.AIRSRegistrationNumber)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).AIRSRegistrationNumbers)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.AIRSRegistrationNumber)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).AIRSRegistrationNumbers)).SyncRoot)).CY_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.AIRSRegistrationNumber)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).AIRSRegistrationNumbers)).SyncRoot)).CY_Data)));
			this.CFIARegNumbersGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ef59fd4a-fba9-4c46-a872-e6c425141dfe", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CFIAUserCountrol|3828F888-7999-418B-91D1-FC5C11C01DEF", "Registration Number");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CY_Data";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CFIARegNumbersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CFIARegNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CFIARegNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CFIARegNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CFIARegNumbersGrid.GridId = "A323535C-BCC1-42E5-96A5-D6E397293214";
			this.CFIARegNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CFIARegNumbersGrid.LayoutKey = "CFIARegNumbersGrid";
			this.CFIARegNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CFIARegNumbersGrid.Name = "CFIARegNumbersGrid";
			this.CFIARegNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 777, true);
			this.CFIARegNumbersGrid.TabIndex = 0;
			// 
			// LpcoGroupBox
			// 
			this.LpcoGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3FE74E2F-59A3-4293-90C8-F636ED5CC5F5", "LPCOs");
			this.LpcoGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LpcoGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.LpcoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.LpcoGroupBox.Name = "LpcoGroupBox";
			this.LpcoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 110, true);
			this.LpcoGroupBox.TabIndex = 1;
			this.LpcoGroupBox.TabStop = false;
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.CFIAPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 777, true);
			this.LPCOGridUserControl.TabIndex = 11;
			// 
			// AIRSGroupBox
			// 
			this.AIRSGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("C3EA2D00-6FE0-4CC0-92F5-EDDDA17AE9B9", "AIRS Registration Type/Number");
			this.AIRSGroupBox.Controls.Add(this.CFIARegNumbersGrid);
			this.AIRSGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AIRSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 165, true);
			this.AIRSGroupBox.Name = "AIRSGroupBox";
			this.AIRSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 110, true);
			this.AIRSGroupBox.TabIndex = 2;
			this.AIRSGroupBox.TabStop = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.AIRSToolLinkButton);
			this.DetailsGroupBox.Controls.Add(this.SourceStateDropEdit);
			this.DetailsGroupBox.Controls.Add(this.SourceCountryCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.AirsMicellaneousCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.AirsEndUseCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.AirsExtensionCodeBoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.DeliveryAddressUserControl);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 152, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// AIRSToolLinkButton
			// 
			this.AIRSToolLinkButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("beefb1d2-6579-4d42-abec-a18ad6b1bd46", "AIRS Tool Link");
			this.AIRSToolLinkButton.ToolTipCaption = null;
			this.AIRSToolLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 45, true);
			this.AIRSToolLinkButton.Name = "AIRSToolLinkButton";
			this.AIRSToolLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AIRSToolLinkButton.TabIndex = 4;
			this.AIRSToolLinkButton.Click += new System.EventHandler(this.AIRSToolLinkButton_Click);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.AIRSGroupBox);
			this.BottomPanel.Controls.Add(this.LpcoGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 200, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// CFIAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "CFIAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 796, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryAddressUserControl.ResumeLayout(true);
			this.DeliveryAddressUserControl.PerformLayout();
			this.AirsEndUseCodeFindBox.ResumeLayout(true);
			this.AirsEndUseCodeFindBox.PerformLayout();
			this.AirsMicellaneousCodeFindBox.ResumeLayout(true);
			this.AirsMicellaneousCodeFindBox.PerformLayout();
			this.SourceCountryCodeFindBox.ResumeLayout(true);
			this.SourceCountryCodeFindBox.PerformLayout();
			this.SourceStateDropEdit.ResumeLayout(true);
			this.SourceStateDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CFIARegNumbersGrid)).EndInit();
			this.CFIARegNumbersGrid.ResumeLayout(false);
			this.CFIARegNumbersGrid.PerformLayout();
			this.LpcoGroupBox.ResumeLayout(false);
			this.LpcoGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.AIRSGroupBox.ResumeLayout(false);
			this.AIRSGroupBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZAddressControl DeliveryAddressUserControl;
		protected internal ZArchitecture.ZTextBox AirsExtensionCodeBoundTextBox;
		private ZArchitecture.GUI.ZCodeFindBox AirsEndUseCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox AirsMicellaneousCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox SourceCountryCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit SourceStateDropEdit;
		internal Enterprise.ZArchitecture.ZGrid CFIARegNumbersGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LpcoGroupBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AIRSGroupBox;
		internal LPCOGridUserControl LPCOGridUserControl;
		private ZArchitecture.GUI.ZButton AIRSToolLinkButton;
		internal ZArchitecture.GUI.ZPanel BottomPanel;
	}
}
