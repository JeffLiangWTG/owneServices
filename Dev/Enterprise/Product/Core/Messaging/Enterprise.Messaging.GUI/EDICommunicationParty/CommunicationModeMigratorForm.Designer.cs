using CargoWiseOne.ResourceStrings;

namespace Enterprise.Messaging.GUI
{
	partial class CommunicationModeMigratorForm
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
            this.components = new System.ComponentModel.Container();
            this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.closeOnCompletionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.fieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.progressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.progressControl = new Enterprise.Messaging.GUI.ActionLog();
            this.runnerTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.topPanel.SuspendLayout();
            this.fieldsTabPage.SuspendLayout();
            this.zGuidFindBox1.SuspendLayout();
            this.progressTabPage.SuspendLayout();
            this.progressControl.SuspendLayout();
            this.runnerTabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 24, true);
            this.MainStatusBar.TabIndex = 2;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UniversalData.ManualDataExport);
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.closeOnCompletionCheckBox);
            this.topPanel.Controls.Add(this.cancelButton);
            this.topPanel.Controls.Add(this.okButton);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 37, true);
            this.topPanel.TabIndex = 1;
            // 
            // closeOnCompletionCheckBox
            // 
            this.closeOnCompletionCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.closeOnCompletionCheckBox, "CloseOnCompletion");
            this.closeOnCompletionCheckBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("A4356CA2-81A6-47CD-B862-497B09F9217E", "Close on Completion");
            this.closeOnCompletionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 12, true);
            this.closeOnCompletionCheckBox.Name = "closeOnCompletionCheckBox";
            this.closeOnCompletionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
            this.closeOnCompletionCheckBox.TabIndex = 2;
            this.closeOnCompletionCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0CAE94EC-B51F-4A5F-B378-67C5878C497D", "Cancel");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 8, true);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.ToolTipCaption = null;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("AFDA5DC7-AFC3-491B-BA89-AB1976F103CA", "OK");
            this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 8, true);
            this.okButton.Name = "okButton";
            this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.okButton.TabIndex = 0;
            this.okButton.ToolTipCaption = null;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.SendAndCloseButton_Click);
            // 
            // fieldsTabPage
            // 
            this.fieldsTabPage.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("D318862C-627C-4A61-BF48-BF0DB98A226E", "Fields");
            this.fieldsTabPage.Controls.Add(this.zGuidFindBox1);
            this.fieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
            this.fieldsTabPage.Name = "fieldsTabPage";
            this.fieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.fieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 314, true);
            this.fieldsTabPage.TabIndex = 0;
            // 
            // zGuidFindBox1
            // 
            this.zGuidFindBox1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zGuidFindBox1, "ECP_PK");
            this.zGuidFindBox1.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("D4FA8347-8067-43F0-8B70-709E7A7731A3", "EDI Client", "This field will be applied even if it is empty.");
            this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 14, true);
            this.zGuidFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Messaging.EDICommunicationParty;
            this.zGuidFindBox1.Name = "zGuidFindBox1";
            this.zGuidFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zGuidFindBox1.ParentType = null;
            this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.zGuidFindBox1.TabIndex = 0;
            // 
            // progressTabPage
            // 
            this.progressTabPage.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("CC7C00C1-ED04-49DD-AD7D-E1E885D6144F", "Progress");
            this.progressTabPage.Controls.Add(this.progressControl);
            this.progressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
            this.progressTabPage.Name = "progressTabPage";
            this.progressTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.progressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 314, true);
            this.progressTabPage.TabIndex = 1;
            // 
            // progressControl
            // 
            this.progressControl.AllowDrop = true;
            this.progressControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.progressControl.Name = "progressControl";
            this.progressControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.progressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 309, true);
            this.progressControl.TabIndex = 0;
            // 
            // runnerTabControl
            // 
            this.runnerTabControl.AccessibleDescription = "Action.DocumentPivots";
            this.runnerTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.runnerTabControl.Controls.Add(this.fieldsTabPage);
            this.runnerTabControl.Controls.Add(this.progressTabPage);
            this.runnerTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.runnerTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.runnerTabControl.Name = "runnerTabControl";
            this.runnerTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 336, true);
            this.runnerTabControl.TabIndex = 0;
            // 
            // CommunicationModeMigratorForm
            // 
            this.CancelButton = this.cancelButton;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 373, true);
            this.Controls.Add(this.runnerTabControl);
            this.Controls.Add(this.topPanel);
            this.DataSourceAssemblyName = "Enterprise.Services.OperationalActions.Business";
            this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UniversalData.ManualDataExport);
            this.DataSourceTypeName = "Enterprise.Services.OperationalActions.Business.OperationalActionRunner";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 375, true);
            this.Name = "CommunicationModeMigratorForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "Run Update Organizations EDI Client";
            this.Controls.SetChildIndex(this.topPanel, 0);
            this.Controls.SetChildIndex(this.runnerTabControl, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.fieldsTabPage.ResumeLayout(false);
            this.fieldsTabPage.PerformLayout();
            this.zGuidFindBox1.ResumeLayout(true);
            this.zGuidFindBox1.PerformLayout();
            this.progressTabPage.ResumeLayout(false);
            this.progressTabPage.PerformLayout();
            this.progressControl.ResumeLayout(true);
            this.progressControl.PerformLayout();
            this.runnerTabControl.ResumeLayout(false);
            this.runnerTabControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZPanel topPanel;
		private ZArchitecture.GUI.ZCheckBox closeOnCompletionCheckBox;
		private ZArchitecture.GUI.ZTabPage progressTabPage;
		private ZArchitecture.GUI.ZTabPage fieldsTabPage;
		private ZArchitecture.GUI.ZTabControl runnerTabControl;
		private ZArchitecture.GUI.ZGuidFindBox zGuidFindBox1;
		internal ActionLog progressControl;
	}
}
