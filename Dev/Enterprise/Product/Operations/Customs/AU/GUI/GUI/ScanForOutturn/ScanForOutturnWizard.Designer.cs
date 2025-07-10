using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	partial class ScanForOutturnWizard
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WizardTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.UnderbondSelectionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ShipmentSelectionTapPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ScanningManagementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SurplusConsignmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReportZeroLandedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NextStepButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.WizardTabControl.SuspendLayout();
			this.ButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 338, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.WizardTabControl);
			this.MainPanel.Controls.Add(this.ButtonPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 362, true);
			this.MainPanel.TabIndex = 3;
			// 
			// WizardTabControl
			// 
			this.WizardTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.WizardTabControl.Controls.Add(this.UnderbondSelectionTabPage);
			this.WizardTabControl.Controls.Add(this.ShipmentSelectionTapPage);
			this.WizardTabControl.Controls.Add(this.ScanningManagementTabPage);
			this.WizardTabControl.Controls.Add(this.SurplusConsignmentsTabPage);
			this.WizardTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WizardTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WizardTabControl.Name = "WizardTabControl";
			this.WizardTabControl.SelectedIndex = 0;
			this.WizardTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 298, true);
			this.WizardTabControl.TabIndex = 4;
			// 
			// UnderbondSelectionTabPage
			// 
			this.UnderbondSelectionTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("000cbc3a-424f-4979-81ee-c523cabf09e0", "Select Underbond", "Underbond Selection");
			this.UnderbondSelectionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnderbondSelectionTabPage.Name = "UnderbondSelectionTabPage";
			this.UnderbondSelectionTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnderbondSelectionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 271, true);
			this.UnderbondSelectionTabPage.TabIndex = 2;
			this.UnderbondSelectionTabPage.Text = "Select Underbond";
			this.UnderbondSelectionTabPage.UseVisualStyleBackColor = true;
			this.UnderbondSelectionTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.UnderbondSelectionTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).FlightNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).ArivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).ContainerNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).PiecesManifested)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).OriginID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).ResponsiblePartyID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).ModeOfMove)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).MovementReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).UnderbondStatusText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).OutturnStatusText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.UnderbondSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).UnderbondSelectorLineCollection)).SyncRoot)).DestinationID)));
			// 
			// ShipmentSelectionTapPage
			// 
			this.ShipmentSelectionTapPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("4f3cf6ba-df3e-403f-bd23-9ecd764a0814", "Select Shipments", "Shipment Selection");
			this.ShipmentSelectionTapPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipmentSelectionTapPage.Name = "ShipmentSelectionTapPage";
			this.ShipmentSelectionTapPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ShipmentSelectionTapPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 271, true);
			this.ShipmentSelectionTapPage.TabIndex = 0;
			this.ShipmentSelectionTapPage.Text = "Select Shipments";
			this.ShipmentSelectionTapPage.UseVisualStyleBackColor = true;
			this.ShipmentSelectionTapPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ShipmentSelectionTapPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).ShipmentSelectorLineCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ShipmentSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).ShipmentSelectorLineCollection)).SyncRoot)).Shipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ShipmentSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).ShipmentSelectorLineCollection)).SyncRoot)).Consignee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.ShipmentSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).ShipmentSelectorLineCollection)).SyncRoot)).IncludeInScan)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ShipmentSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).ShipmentSelectorLineCollection)).SyncRoot)).OutturnStatusText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ShipmentSelectorLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).ShipmentSelectorLineCollection)).SyncRoot)).UnderbondStatusText)));
			// 
			// ScanningManagementTabPage
			// 
			this.ScanningManagementTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("809a0b42-afc2-4ac4-bb5a-393e86025494", "Scanning", "Scanning Management Options");
			this.ScanningManagementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ScanningManagementTabPage.Name = "ScanningManagementTabPage";
			this.ScanningManagementTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ScanningManagementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 271, true);
			this.ScanningManagementTabPage.TabIndex = 1;
			this.ScanningManagementTabPage.Text = "Scanning";
			this.ScanningManagementTabPage.UseVisualStyleBackColor = true;
			this.ScanningManagementTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ScanningManagementTabPage_InitializeTab));
			// 
			// SurplusConsignmentsTabPage
			// 
			this.SurplusConsignmentsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9f697ea1-9f81-4396-b5f5-10798a7f0c1e", "Surplus Consignments");
			this.SurplusConsignmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SurplusConsignmentsTabPage.Name = "SurplusConsignmentsTabPage";
			this.SurplusConsignmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SurplusConsignmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 271, true);
			this.SurplusConsignmentsTabPage.TabIndex = 3;
			this.SurplusConsignmentsTabPage.Text = "Surplus Consignments";
			this.SurplusConsignmentsTabPage.UseVisualStyleBackColor = true;
			this.SurplusConsignmentsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SurplusConsignmentsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).SurplusOutturnCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SurplusOutturnLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).SurplusOutturnCollection)).SyncRoot)).ConsignmentRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SurplusOutturnLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).SurplusOutturnCollection)).SyncRoot)).Shipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SurplusOutturnLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).SurplusOutturnCollection)).SyncRoot)).ShipmentList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SurplusOutturnLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource)(null)).SurplusOutturnCollection)).SyncRoot)).Description)));
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.ReportZeroLandedButton);
			this.ButtonPanel.Controls.Add(this.NextStepButton);
			this.ButtonPanel.Controls.Add(this.CancelButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 298, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 64, true);
			this.ButtonPanel.TabIndex = 3;
			// 
			// ReportZeroLandedButton
			// 
			this.ReportZeroLandedButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("89c56e29-e13f-454c-b69c-83b3101d945a", "Zero land selected shipments");
			this.ReportZeroLandedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.ReportZeroLandedButton.Name = "ReportZeroLandedButton";
			this.ReportZeroLandedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 23, true);
			this.ReportZeroLandedButton.TabIndex = 6;
			this.ReportZeroLandedButton.Text = "Zero land selected shipments";
			this.ReportZeroLandedButton.UseVisualStyleBackColor = true;
			// 
			// NextStepButton
			// 
			this.NextStepButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NextStepButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("e1690840-7fc3-4184-b48a-8216f75dd7d9", "Next Step");
			this.NextStepButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 6, true);
			this.NextStepButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 30, true);
			this.NextStepButton.Name = "NextStepButton";
			this.NextStepButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 30, true);
			this.NextStepButton.TabIndex = 0;
			this.NextStepButton.Text = "Next Step";
			this.NextStepButton.UseVisualStyleBackColor = true;
			this.NextStepButton.Click += new System.EventHandler(this.NextStepButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("3ef8c3c0-7471-4c08-b459-bc42608ed963", "Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 5, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 30, true);
			this.CancelButton.TabIndex = 1;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ScanForOutturnWizard
			// 
			this.AcceptButton = this.NextStepButton;
			this.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("e39fe2a8-c9c2-4665-ae44-951e3d1b293b", "Outturn Scanning");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 362, true);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ScanWizardDataSource);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			this.Name = "ScanForOutturnWizard";
			this.ShouldSerializeTabPageMethods = true;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Outturn Scanning";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.WizardTabControl.ResumeLayout(false);
			this.WizardTabControl.PerformLayout();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected virtual void ShipmentSelectionTapPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ShipmentSelectionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ShipmentSelectionTapPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentSelectionGrid)).BeginInit();
			this.ShipmentSelectionGrid.SuspendLayout();
			this.ShipmentSelectionTapPage.Controls.Add(this.ShipmentSelectionGrid);
			// 
			// ShipmentSelectionGrid
			// 
			this.ShipmentSelectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ShipmentSelectionGrid, "ShipmentSelectorLineCollection");
			this.ShipmentSelectionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("6bdca62b-fbb3-40b4-816d-ee75618dfa53", "Shipment");
			zTextBoxColumnStyleInfo14.ColumnName = "Shipment";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ddb995fa-7257-4095-a7b3-a4cfc10f1a48", "Consignee");
			zTextBoxColumnStyleInfo15.ColumnName = "Consignee";
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("d80bbae9-7fb3-4ee0-8ad6-8aa36da05140", "Include In Scan");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInScan";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ee9fb128-5cd3-4be9-87b0-03280ba171a9", "Outturn Status");
			zTextBoxColumnStyleInfo16.ColumnName = "OutturnStatusText";
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("6351169c-a5ec-47fa-87f9-c9908cb86a8d", "Underbond Status");
			zTextBoxColumnStyleInfo17.ColumnName = "UnderbondStatusText";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ShipmentSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ShipmentSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ShipmentSelectionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ShipmentSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ShipmentSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ShipmentSelectionGrid.CopySelectedRowsAllowed = true;
			this.ShipmentSelectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentSelectionGrid.GridId = "2c6eee39-ce6b-4abf-9429-c5c9260a15bc";
			this.ShipmentSelectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentSelectionGrid.LayoutKey = "zGrid1";
			this.ShipmentSelectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShipmentSelectionGrid.Name = "ShipmentSelectionGrid";
			this.ShipmentSelectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 265, true);
			this.ShipmentSelectionGrid.TabIndex = 5;
			this.ShipmentSelectionTapPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentSelectionGrid)).EndInit();
			this.ShipmentSelectionGrid.ResumeLayout(false);
			this.ShipmentSelectionGrid.PerformLayout();
			this.ShipmentSelectionTapPage.ResumeLayout(true);

		}

		private void ScanningManagementTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WriteFileToScanButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LoadFileFromScanButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DirectScanningButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileWritingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FileLoadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ParcelsScannedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zTabPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ScanningManagementTabPage.SuspendLayout();
			this.ScanningManagementTabPage.Controls.Add(this.ParcelsScannedLabel);
			this.ScanningManagementTabPage.Controls.Add(this.FileLoadingLabel);
			this.ScanningManagementTabPage.Controls.Add(this.FileWritingLabel);
			this.ScanningManagementTabPage.Controls.Add(this.DirectScanningButton);
			this.ScanningManagementTabPage.Controls.Add(this.LoadFileFromScanButton);
			this.ScanningManagementTabPage.Controls.Add(this.WriteFileToScanButton);
			this.ScanningManagementTabPage.Controls.Add(this.zTabPanel);
			// 
			// WriteFileToScanButton
			// 
			this.WriteFileToScanButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("93e2f90b-7bbd-4f6a-b39a-6b0855dd796a", "Write File To Scan Equipment");
			this.WriteFileToScanButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 18, true);
			this.WriteFileToScanButton.Name = "WriteFileToScanButton";
			this.WriteFileToScanButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 45, true);
			this.WriteFileToScanButton.TabIndex = 0;
			this.WriteFileToScanButton.Text = "Write File To Scan Equipment";
			this.WriteFileToScanButton.UseVisualStyleBackColor = true;
			this.WriteFileToScanButton.Click += new System.EventHandler(this.WriteFileToScanButton_Click);
			// 
			// LoadFileFromScanButton
			// 
			this.LoadFileFromScanButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9440fc50-f27f-40af-9dde-60b8ed883642", "Load File From Scan Equipment");
			this.LoadFileFromScanButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 85, true);
			this.LoadFileFromScanButton.Name = "LoadFileFromScanButton";
			this.LoadFileFromScanButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 45, true);
			this.LoadFileFromScanButton.TabIndex = 1;
			this.LoadFileFromScanButton.Text = "Load File From Scan Equipment";
			this.LoadFileFromScanButton.UseVisualStyleBackColor = true;
			this.LoadFileFromScanButton.Click += new System.EventHandler(this.LoadFileFromScanButton_Click);
			// 
			// DirectScanningButton
			// 
			this.DirectScanningButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("91e07a85-1351-473c-95c5-65b7dae25ad0", "Commence Direct Scanning");
			this.DirectScanningButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 149, true);
			this.DirectScanningButton.Name = "DirectScanningButton";
			this.DirectScanningButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 45, true);
			this.DirectScanningButton.TabIndex = 2;
			this.DirectScanningButton.Text = "Commence Direct Scanning";
			this.DirectScanningButton.UseVisualStyleBackColor = true;
			this.DirectScanningButton.Click += new System.EventHandler(this.DirectScanningButton_Click);
			// 
			// FileWritingLabel
			// 
			this.FileWritingLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("0a0dcffd-23fd-4965-aefb-ef6a47ba7932", "File Not Written");
			this.FileWritingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 22, true);
			this.FileWritingLabel.Name = "FileWritingLabel";
			this.FileWritingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 37, true);
			this.FileWritingLabel.TabIndex = 3;
			this.FileWritingLabel.Text = "File Not Written";
			// 
			// FileLoadingLabel
			// 
			this.FileLoadingLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ec285228-9f9e-4c39-b6d8-b48957898c64", "File Not Loaded");
			this.FileLoadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 89, true);
			this.FileLoadingLabel.Name = "FileLoadingLabel";
			this.FileLoadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 37, true);
			this.FileLoadingLabel.TabIndex = 4;
			this.FileLoadingLabel.Text = "File Not Loaded";
			// 
			// ParcelsScannedLabel
			// 
			this.ParcelsScannedLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("063d2f62-e9d7-479b-94eb-23afe541ddd4", "Parcels Scanned so far: 0");
			this.ParcelsScannedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 153, true);
			this.ParcelsScannedLabel.Name = "ParcelsScannedLabel";
			this.ParcelsScannedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 37, true);
			this.ParcelsScannedLabel.TabIndex = 5;
			this.ParcelsScannedLabel.Text = "Parcels Scanned so far: 0";
			// 
			// zTabPanel
			// 
			this.zTabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zTabPanel.Name = "zTabPanel";
			this.zTabPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 265, true);
			this.zTabPanel.TabIndex = 4;
			this.ScanningManagementTabPage.PerformLayout();
			this.ScanningManagementTabPage.ResumeLayout(true);

		}

		protected virtual void UnderbondSelectionTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UnderbondSelectionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.UnderbondSelectionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UnderbondSelectionTabPage.SuspendLayout();
			this.UnderbondSelectionPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnderbondSelectionGrid)).BeginInit();
			this.UnderbondSelectionGrid.SuspendLayout();
			this.UnderbondSelectionTabPage.Controls.Add(this.UnderbondSelectionPanel);
			// 
			// UnderbondSelectionPanel
			// 
			this.UnderbondSelectionPanel.Controls.Add(this.UnderbondSelectionGrid);
			this.UnderbondSelectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondSelectionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UnderbondSelectionPanel.Name = "UnderbondSelectionPanel";
			this.UnderbondSelectionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 265, true);
			this.UnderbondSelectionPanel.TabIndex = 0;
			// 
			// UnderbondSelectionGrid
			// 
			this.UnderbondSelectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnderbondSelectionGrid, "UnderbondSelectorLineCollection");
			this.UnderbondSelectionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("53adf00d-2db5-449b-91b3-3cf072ea7265", "Reference");
			zTextBoxColumnStyleInfo1.ColumnName = "Reference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ef80aea7-68d6-4e92-9098-41f7b2b9fa9a", "Flight Number");
			zTextBoxColumnStyleInfo2.ColumnName = "FlightNumber";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("d145e5c1-9939-4c64-b546-66cbb54bb83a", "Arrival Date");
			zDateEditColumnStyleInfo1.ColumnName = "ArivalDate";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("c93d8ac6-2d87-49ed-bdaf-db0f11975fb5", "Container Number");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "ContainerNo";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("7d1e732d-1acd-4970-a3a0-3a15ac1d0f7e", "Pieces Manifested");
			zCalcEditColumnStyleInfo1.ColumnName = "PiecesManifested";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("0b941bdf-0532-4bcb-9ef8-33d9bfe1b0a7", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "Status";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("8257a050-f445-4b4c-ba89-a41a724a6fe7", "Message Status");
			zTextBoxColumnStyleInfo5.ColumnName = "MessageStatus";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("56afc934-dbb6-49e2-89fc-9491ffd25e9a", "Origin Premise ID");
			zTextBoxColumnStyleInfo6.ColumnName = "OriginID";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ec9e1a02-2389-4dba-be7d-647cccd6fb27", "Responsible Party ID");
			zTextBoxColumnStyleInfo7.ColumnName = "ResponsiblePartyID";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("44d36850-53b0-4b62-9aed-36d51efb00cf", "Mode of Move");
			zTextBoxColumnStyleInfo8.ColumnName = "ModeOfMove";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("260dea01-807e-4b2c-b43c-71833c8b5da0", "Movement Reason");
			zTextBoxColumnStyleInfo9.ColumnName = "MovementReason";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("417dccd0-daf3-4616-be99-c07ff8bf15c2", "Package Type");
			zTextBoxColumnStyleInfo10.ColumnName = "PackageType";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("d0ca0ce9-795e-44ed-9322-5236be513a83", "Underbond Status");
			zTextBoxColumnStyleInfo11.ColumnName = "UnderbondStatusText";
			zTextBoxColumnStyleInfo11.IsMandatory = true;
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("0476fa4c-5a01-473a-9685-4fc15b302d7c", "Outturn Status");
			zTextBoxColumnStyleInfo12.ColumnName = "OutturnStatusText";
			zTextBoxColumnStyleInfo12.IsMandatory = true;
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("c8c4021d-5ba0-4286-ba83-c6297168d851", "Destination Premise ID");
			zTextBoxColumnStyleInfo13.ColumnName = "DestinationID";
			zTextBoxColumnStyleInfo13.IsMandatory = true;
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(122);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.UnderbondSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.UnderbondSelectionGrid.CopySelectedRowsAllowed = true;
			this.UnderbondSelectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondSelectionGrid.GridId = "3ef7cc8d-1761-4973-867f-fc470eb1219b";
			this.UnderbondSelectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnderbondSelectionGrid.IsWholeRowSelectedOnClick = true;
			this.UnderbondSelectionGrid.LayoutKey = "zGrid1";
			this.UnderbondSelectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnderbondSelectionGrid.Name = "UnderbondSelectionGrid";
			this.UnderbondSelectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 265, true);
			this.UnderbondSelectionGrid.TabIndex = 0;
			this.UnderbondSelectionTabPage.PerformLayout();
			this.UnderbondSelectionPanel.ResumeLayout(false);
			this.UnderbondSelectionPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnderbondSelectionGrid)).EndInit();
			this.UnderbondSelectionGrid.ResumeLayout(false);
			this.UnderbondSelectionGrid.PerformLayout();
			this.UnderbondSelectionTabPage.ResumeLayout(true);

		}

		private ZPanel MainPanel;
		protected ZTabControl WizardTabControl;
		protected ZTabPage ShipmentSelectionTapPage;
		protected ZTabPage ScanningManagementTabPage;
		new ZButton CancelButton;
		protected ZButton NextStepButton;
		private ZPanel zTabPanel;
		protected ZTabPage UnderbondSelectionTabPage;
		private ZPanel UnderbondSelectionPanel;
		protected ZButton ReportZeroLandedButton;
		private ZPanel ButtonPanel;

		#endregion
		private System.ComponentModel.IContainer components;
		protected ZTabPage SurplusConsignmentsTabPage;
		private ZArchitecture.ZGrid SurplusConsignmentsGrid;
		protected ZButton DirectScanningButton;
		protected ZArchitecture.ZGrid ShipmentSelectionGrid;
		protected ZButton WriteFileToScanButton;
		protected ZButton LoadFileFromScanButton;
		protected ZArchitecture.ZLabel ParcelsScannedLabel;
		protected ZArchitecture.ZLabel FileLoadingLabel;
		protected ZArchitecture.ZLabel FileWritingLabel;
		protected ZArchitecture.ZGrid UnderbondSelectionGrid;
	}
}
