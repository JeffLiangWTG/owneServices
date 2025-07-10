using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	partial class ApplicationActiveLoggerForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.applicationLoggerGuidBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.productTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.nameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.environmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.licenceGuidBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.activeUntilDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.applicationLoggerGuidBox.SuspendLayout();
			this.nameTextBox.SuspendLayout();
			this.environmentTextBox.SuspendLayout();
			this.licenceGuidBox.SuspendLayout();
			this.productTextBox.SuspendLayout();
			this.activeUntilDateTimeOffsetEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 530, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.environmentTextBox);
			this.MainTabPage.Controls.Add(this.applicationLoggerGuidBox);
			this.MainTabPage.Controls.Add(this.productTextBox);
			this.MainTabPage.Controls.Add(this.nameTextBox);
			this.MainTabPage.Controls.Add(this.licenceGuidBox);
			this.MainTabPage.Controls.Add(this.activeUntilDateTimeOffsetEdit);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 11, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 11, true);
			this.NotesTabPage.TabVisible = false;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(894, 507, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 530, true);
			// 

			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger);
			// 
			// applicationLoggerGuidBox
			// 
			this.applicationLoggerGuidBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.applicationLoggerGuidBox, "AAL_ALG_ApplicationLogger");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).AAL_ALG_ApplicationLogger)));
			this.applicationLoggerGuidBox.CaptionResourceString = ZClientEDI.Res.GetData("7C7316A8-94EA-4591-92F2-489F201542B4", "Application Logger");
			this.applicationLoggerGuidBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 16, true);
			this.applicationLoggerGuidBox.Name = "applicationLoggerGuidBox";
			this.applicationLoggerGuidBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.applicationLoggerGuidBox.ParentType = null;
			this.applicationLoggerGuidBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.applicationLoggerGuidBox.TabIndex = 0;
			// 
			// productTextBox
			// 
			this.BindingSource.SetBindingMember(this.productTextBox, "ApplicationLoggerProduct");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).ApplicationLoggerProduct)));
			this.productTextBox.CaptionResourceString = ZClientEDI.Res.GetData("476B96C2-61CC-422B-A6A9-5F2D7BFA0274", "Product");
			this.productTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 42, true);
			this.productTextBox.Name = "productTextBox";
			this.productTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.productTextBox.TabIndex = 1;
			this.productTextBox.ReadOnly = true;
			// 
			// nameTextBox
			// 
			this.BindingSource.SetBindingMember(this.nameTextBox, "ApplicationLoggerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).ApplicationLoggerName)));
			this.nameTextBox.CaptionResourceString = ZClientEDI.Res.GetData("61868988-A387-44F3-8573-D60A2AB701DF", "Name");
			this.nameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 68, true);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.nameTextBox.TabIndex = 2;
			this.nameTextBox.ReadOnly = true;
			// 
			// licenceGuidBox
			// 
			this.licenceGuidBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.licenceGuidBox, "LicenceGuid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).LicenceGuid)));
			this.licenceGuidBox.CaptionResourceString = ZClientEDI.Res.GetData("7C7316A8-94EA-4591-92F2-489F201542B4", "Licence");
			this.licenceGuidBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 94, true);
			this.licenceGuidBox.Name = "licenceGuidBox";
			this.licenceGuidBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.licenceGuidBox.ParentType = null;
			this.licenceGuidBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.licenceGuidBox.TabIndex = 3;
			// 
			// environmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.environmentTextBox, "AAL_Environment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).AAL_Environment)));
			this.environmentTextBox.CaptionResourceString = ZClientEDI.Res.GetData("0DCED6FA-6393-4384-9E7C-A855AD16B683", "Environment");
			this.environmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 120, true);
			this.environmentTextBox.ReadOnly = true;
			this.environmentTextBox.Name = "environmentTextBox";
			this.environmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.environmentTextBox.TabIndex = 4;
			// 
			// activeUntilDateTimeOffsetEdit
			// 
			this.activeUntilDateTimeOffsetEdit.AllowDrop = true;
			this.activeUntilDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.activeUntilDateTimeOffsetEdit, "AAL_ActiveUntil");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).AAL_ActiveUntil)));
			this.activeUntilDateTimeOffsetEdit.CaptionResourceString = ZClientEDI.Res.GetData("637AE936-DC7B-4623-A4D1-BB2DCA3FE82A", "Active Until");
			this.activeUntilDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.activeUntilDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 146, true);
			this.activeUntilDateTimeOffsetEdit.Name = "activeUntilDateTimeOffsetEdit";
			this.activeUntilDateTimeOffsetEdit.TabIndex = 5;
			// 
			// ApplicationActiveLoggerForm
			// 
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 586, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger);
			this.DataSourceTypeName = "Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 90, true);
			this.Name = "ApplicationActiveLoggerForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.applicationLoggerGuidBox.ResumeLayout(true);
			this.applicationLoggerGuidBox.PerformLayout();
			this.productTextBox.ResumeLayout(true);
			this.productTextBox.PerformLayout();
			this.nameTextBox.ResumeLayout(true);
			this.nameTextBox.PerformLayout();
			this.licenceGuidBox.ResumeLayout(true);
			this.licenceGuidBox.PerformLayout();
			this.environmentTextBox.ResumeLayout(true);
			this.environmentTextBox.PerformLayout();
			this.activeUntilDateTimeOffsetEdit.ResumeLayout(true);
			this.activeUntilDateTimeOffsetEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZGuidFindBox applicationLoggerGuidBox;
		ZTextBox productTextBox;
		ZTextBox nameTextBox;
		ZTextBox environmentTextBox;
		ZGuidFindBox licenceGuidBox;
		ZDateTimeOffsetEdit activeUntilDateTimeOffsetEdit;

		#endregion
	}
}
