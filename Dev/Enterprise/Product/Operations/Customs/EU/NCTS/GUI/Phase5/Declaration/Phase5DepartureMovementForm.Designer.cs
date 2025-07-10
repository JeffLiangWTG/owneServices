using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DepartureMovementForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ServicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.TransportAndPackagingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MiscTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MovementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TotalNumberOfPackagesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalGrossMassInKilogramsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalGrossMassInKilogramsUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomFieldTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ServicesTabPage);
			this.MainTabControl.Controls.Add(this.MovementsTabPage);
			this.MainTabControl.Controls.Add(this.TransportAndPackagingTabPage);
			this.MainTabControl.Controls.Add(this.HouseConsignmentsTabPage);
			this.MainTabControl.Controls.Add(this.MiscTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.CustomFieldTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 733, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MiscTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.HouseConsignmentsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.TransportAndPackagingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MovementsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ServicesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CustomFieldTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.AutoScroll = true;
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			// 
			// TotalNumberOfPackagesTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalNumberOfPackagesTextBox, "TotalNumberOfPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).TotalNumberOfPackages)));
			this.TotalNumberOfPackagesTextBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("8BBF88C1-85CC-4AD1-8331-00C2C057E878", "Total Packs");
			this.TotalNumberOfPackagesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 7, true);
			this.TotalNumberOfPackagesTextBox.Name = "TotalNumberOfPackagesTextBox";
			this.TotalNumberOfPackagesTextBox.ReadOnly = true;
			this.TotalNumberOfPackagesTextBox.TextAlign = HorizontalAlignment.Right;
			this.TotalNumberOfPackagesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			// 
			// TotalGrossMassInKilogramsTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossMassInKilogramsCalcEdit, "MovementHeader.TotalGrossMassInKilograms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.TotalGrossMassInKilograms)));
            this.TotalGrossMassInKilogramsCalcEdit.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("CB4ADC05-4E0A-45F9-9FD1-FB30D6FD2546", "Total Gross Weight");
			this.TotalGrossMassInKilogramsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 7, true);
			this.TotalGrossMassInKilogramsCalcEdit.DecimalPlaces = 6;
			this.TotalGrossMassInKilogramsCalcEdit.Name = "TotalGrossMassInKilogramsCalcEdit ";
			this.TotalGrossMassInKilogramsCalcEdit.ReadOnly = true;
			this.TotalGrossMassInKilogramsCalcEdit.TextAlign = HorizontalAlignment.Right;
			this.TotalGrossMassInKilogramsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			// 
			// TotalGrossMassInKilogramsUnitLabel
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalGrossMassInKilogramsUnitLabel, false);
			this.TotalGrossMassInKilogramsUnitLabel.Location = ControlDpiScalingHelper.NewScaledPoint(505, 4, true);
			this.TotalGrossMassInKilogramsUnitLabel.Name = "TotalGrossMassInKilogramsUnitLabel";
			this.TotalGrossMassInKilogramsUnitLabel.Size = ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalGrossMassInKilogramsUnitLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("AC83345C-22B4-45FA-A2D4-8CAF11A13861", "KG");
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			//
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.TotalNumberOfPackagesTextBox);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.TotalGrossMassInKilogramsCalcEdit);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.TotalGrossMassInKilogramsUnitLabel);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 1, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 733, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// ServicesTabPage
			// 
			this.ServicesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("C6EC8B10-9B4D-4A9C-A3E6-0A55AF24BD73", "Services");
			this.ServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServicesTabPage.Name = "ServicesTabPage";
			this.ServicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.ServicesTabPage.TabIndex = 1;
			this.ServicesTabPage.UseVisualStyleBackColor = true;
			this.ServicesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ServicesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.IHaveServices)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)))));
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("FFE1E9A6-CA2A-4C1E-A598-9B59E51BE4B8", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.MessagesTabPage.TabIndex = 5;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			this.MessagesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MessagesTabPage_InitializeTab));
			// 
			// NCTMovementCustomTabPage
			// 
			this.CustomFieldTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("9F0B275D-DE3A-4465-9196-69D769BD9213", "Custom Fields");
			this.CustomFieldTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CustomFieldTabPage.UseVisualStyleBackColor = true;
			this.CustomFieldTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.CustomFieldTabPage.TabIndex = 6;
			this.CustomFieldTabPage.Text = Res.GetString("ACB8E840-FC94-48A1-B03E-1931CCFFDB94", "Custom Fields");
			this.CustomFieldTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CustomFieldTabPage_InitializeTab));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 7;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// TransportAndPackagingTabPage
			// 
			this.TransportAndPackagingTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("01A24909-3199-4595-9C4B-180C35A44924", "Transport && Containers");
			this.TransportAndPackagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransportAndPackagingTabPage.Name = "TransportAndPackagingTabPage";
			this.TransportAndPackagingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TransportAndPackagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.TransportAndPackagingTabPage.TabIndex = 2;
			this.TransportAndPackagingTabPage.UseVisualStyleBackColor = true;
			// 
			// HouseConsignmentsTabPage
			// 
			this.HouseConsignmentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7401029D-44C5-45AB-AB53-D3DED54B1A8C", "House Consignments");
			this.HouseConsignmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentsTabPage.Name = "HouseConsignmentsTabPage";
			this.HouseConsignmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseConsignmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.HouseConsignmentsTabPage.TabIndex = 3;
			this.HouseConsignmentsTabPage.UseVisualStyleBackColor = true;
			// 
			// MiscTabPage
			// 
			this.MiscTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5BA7669C-DC6D-45B7-A71B-771A0FD61F73", "Misc.");
			this.MiscTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MiscTabPage.Name = "MiscTabPage";
			this.MiscTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MiscTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.MiscTabPage.TabIndex = 4;
			this.MiscTabPage.UseVisualStyleBackColor = true;
			this.MiscTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MiscTabPage_InitializeTab));
			// 
			// MovementsTabPage
			// 
			this.MovementsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("e1096628-472d-4e1d-8c53-a3c98eb37fda", "Movements");
			this.MovementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MovementsTabPage.Name = "MovementsTabPage";
			this.MovementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.MovementsTabPage.TabIndex = 8;
			this.MovementsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MovementsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.INctsDepartureMovementHeaderCollection<Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader>)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DepartureMovementHeaders)));
			// 
			// Phase5DepartureMovementForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 789, true);
			this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "Phase5DepartureMovementForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.ResumeLayout(false);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void ServicesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ServicesTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5DeclarationServicesTabUserControl();
			this.ServicesTabPage.SuspendLayout();
			this.ServicesTabUserControl.SuspendLayout();
			this.ServicesTabPage.Controls.Add(this.ServicesTabUserControl);
			// 
			// ServicesTabUserControl
			// 
			this.ServicesTabUserControl.AllowDrop = true;
			this.ServicesTabUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ServicesTabUserControl, ".");
			this.ServicesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ServicesTabUserControl.Name = "ServicesTabUserControl";
			this.ServicesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 700, true);
			this.ServicesTabUserControl.TabIndex = 0;
			this.ServicesTabPage.PerformLayout();
			this.ServicesTabUserControl.ResumeLayout(true);
			this.ServicesTabUserControl.PerformLayout();
			this.ServicesTabPage.ResumeLayout(true);

		}
		private void MovementsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DepartureMovementsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.DepartureMovementsTabUserControl();
			this.MovementsTabPage.SuspendLayout();
			this.DepartureMovementsTabUserControl.SuspendLayout();
			this.MovementsTabPage.Controls.Add(this.DepartureMovementsTabUserControl);
			// 
			// DepartureMovementsTabUserControl
			// 
			this.DepartureMovementsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureMovementsTabUserControl, "DepartureMovementHeaders");
			this.DepartureMovementsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DepartureMovementsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DepartureMovementsTabUserControl.Name = "DepartureMovementsTabUserControl";
			this.DepartureMovementsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 706, true);
			this.DepartureMovementsTabUserControl.TabIndex = 2;
			this.MovementsTabPage.PerformLayout();
			this.DepartureMovementsTabUserControl.ResumeLayout(true);
			this.DepartureMovementsTabUserControl.PerformLayout();
			this.MovementsTabPage.ResumeLayout(true);

		}

		void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		void WorkflowTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);

		}

		void MessagesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MessagesTabDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesTabDynamicUserControl.SuspendLayout();
			this.MessagesTabPage.Controls.Add(this.MessagesTabDynamicUserControl);
			// 
			// MessagesTabDynamicUserControl
			// 
			this.MessagesTabDynamicUserControl.AllowDrop = true;
			this.MessagesTabDynamicUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(MessagesTabDynamicUserControl, "MovementHeader.MessagesForDisplay");
			this.MessagesTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesTabDynamicUserControl.Name = "MessagesTabDynamicUserControl";
			this.MessagesTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 700, true);
			this.MessagesTabDynamicUserControl.TabIndex = 0;
			this.MessagesTabDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.MessagesTabUserControl);
			this.MessagesTabPage.PerformLayout();
			this.MessagesTabDynamicUserControl.ResumeLayout(true);
			this.MessagesTabDynamicUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(true);

		}

		void MiscTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MiscTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5DeclarationMiscTabUserControl();
			this.MiscTabPage.SuspendLayout();
			this.MiscTabUserControl.SuspendLayout();
			this.MiscTabPage.Controls.Add(this.MiscTabUserControl);
			// 
			// MiscTabUserControl
			// 
			this.MiscTabUserControl.AllowDrop = true;
			this.MiscTabUserControl.AutoSize = true;
			this.MiscTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MiscTabUserControl, ".");
			this.MiscTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiscTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MiscTabUserControl.Name = "MiscTabUserControl";
			this.MiscTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 700, true);
			this.MiscTabUserControl.TabIndex = 0;
			this.MiscTabPage.PerformLayout();
			this.MiscTabUserControl.ResumeLayout(true);
			this.MiscTabUserControl.PerformLayout();
			this.MiscTabPage.ResumeLayout(true);

		}

		void CustomFieldTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.ProcessTemplateCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.CustomFieldTabPage.SuspendLayout();
			this.ProcessTemplateCustomFieldsControl.SuspendLayout();
			this.CustomFieldTabPage.Controls.Add(this.ProcessTemplateCustomFieldsControl);
			// 
			// ProcessTemplateCustomFieldsControl
			//
			this.ProcessTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BindingSource.SetBindingMember(this.ProcessTemplateCustomFieldsControl, ".");
			this.ProcessTemplateCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ProcessTemplateCustomFieldsControl.Name = "NCTProcessTemplateCustomFieldsControl";
			this.ProcessTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1201, 364, true);
			this.ProcessTemplateCustomFieldsControl.TabIndex = 0;
			this.ProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("836FA6B3-A35D-481C-9DFE-5E2617B36FD3", "To make use of this tab, please setup NCTS - New Computerized Transit System (Europe) custom fields in Workflow Manager.");
			this.CustomFieldTabPage.PerformLayout();
			this.ProcessTemplateCustomFieldsControl.ResumeLayout(true);
			this.ProcessTemplateCustomFieldsControl.PerformLayout();
			this.CustomFieldTabPage.ResumeLayout(true);

		}

		#endregion

		internal Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage TransportAndPackagingTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage HouseConsignmentsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MiscTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl MessagesTabDynamicUserControl;
		internal ZArchitecture.GUI.ZTabPage ServicesTabPage;
		internal ZArchitecture.GUI.ZTabPage MovementsTabPage;
		internal DepartureMovementsTabUserControl DepartureMovementsTabUserControl;
		internal Phase5TransportAndPackagingTabUserControl TransportAndPackagingTabUserControl;
		internal Phase5DeclarationMiscTabUserControl MiscTabUserControl;
		internal Phase5DeclarationServicesTabUserControl ServicesTabUserControl;
		private ZArchitecture.ZTextBox TotalNumberOfPackagesTextBox;
		private ZArchitecture.ZCalcEdit TotalGrossMassInKilogramsCalcEdit;
		private ZArchitecture.ZLabel TotalGrossMassInKilogramsUnitLabel;
		internal ZArchitecture.GUI.ZTabPage CustomFieldTabPage;
		internal ZArchitecture.GUI.ProcessTemplateCustomFieldsControl ProcessTemplateCustomFieldsControl;
	}
}
