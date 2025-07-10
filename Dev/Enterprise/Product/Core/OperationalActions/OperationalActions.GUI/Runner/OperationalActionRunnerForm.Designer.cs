namespace Enterprise.Services.OperationalActions.GUI
{
	partial class OperationalActionRunnerForm
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZPanel topPanel;
			Enterprise.ZArchitecture.GUI.ZCheckBox closeOnCompletionCheckBox;
			Enterprise.Services.OperationalActions.GUI.DocumentDeliveryControl documentsControl;
			this.RunOnSelectedRecordsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RunOnAllMatchingRecordsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.fieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.fieldsHost = new Enterprise.Services.OperationalActions.GUI.RunnerFieldControlHost();
			this.runnerTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.documentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.progressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.progressControl = new Enterprise.Services.OperationalActions.GUI.ActionLog();
			topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			closeOnCompletionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			documentsControl = new Enterprise.Services.OperationalActions.GUI.DocumentDeliveryControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			topPanel.SuspendLayout();
			this.fieldsTabPage.SuspendLayout();
			this.runnerTabControl.SuspendLayout();
			this.documentsTabPage.SuspendLayout();
			this.progressTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 349, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalActionRunner);
			// 
			// topPanel
			// 
			topPanel.Controls.Add(this.RunOnSelectedRecordsRadioButton);
			topPanel.Controls.Add(this.RunOnAllMatchingRecordsRadioButton);
			topPanel.Controls.Add(closeOnCompletionCheckBox);
			topPanel.Controls.Add(this.cancelButton);
			topPanel.Controls.Add(this.okButton);
			topPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 37, true);
			topPanel.TabIndex = 1;
			// 
			// RunOnSelectedRecordsRadioButton
			// 
			this.RunOnSelectedRecordsRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RunOnSelectedRecordsRadioButton, "RunOnSelectedRecords");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).RunOnSelectedRecords)));
			this.RunOnSelectedRecordsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RunOnSelectedRecordsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 12, true);
			this.RunOnSelectedRecordsRadioButton.Name = "RunOnSelectedRecordsRadioButton";
			this.RunOnSelectedRecordsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.RunOnSelectedRecordsRadioButton.TabIndex = 4;
			// 
			// RunOnAllRecordsRadioButton
			// 
			this.RunOnAllMatchingRecordsRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RunOnAllMatchingRecordsRadioButton, "RunOnAllMatchingRecords");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).RunOnAllMatchingRecords)));
			this.RunOnAllMatchingRecordsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RunOnAllMatchingRecordsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 12, true);
			this.RunOnAllMatchingRecordsRadioButton.Name = "RunOnAllMatchingRecordsRadioButton";
			this.RunOnAllMatchingRecordsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.RunOnAllMatchingRecordsRadioButton.TabIndex = 3;
			// 
			// closeOnCompletionCheckBox
			// 
			closeOnCompletionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(closeOnCompletionCheckBox, "CloseOnCompletion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).CloseOnCompletion)));
			closeOnCompletionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			closeOnCompletionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 12, true);
			closeOnCompletionCheckBox.Name = "closeOnCompletionCheckBox";
			closeOnCompletionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			closeOnCompletionCheckBox.TabIndex = 2;
			closeOnCompletionCheckBox.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("OperationalActionRunnerForm|81395e1d-d9cd-4e14-a677-85c0f76cd9f4", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 8, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("OperationalActionRunnerForm|54700b52-6472-45ce-9d68-3fcbaca1aeb1", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 8, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.okButton.TabIndex = 0;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// fieldsTabPage
			// 
			this.fieldsTabPage.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("OperationalActionRunnerForm|f1c53fbb-24c7-4dd2-80bd-3798a229f363", "Fields");
			this.fieldsTabPage.Controls.Add(this.fieldsHost);
			this.fieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.fieldsTabPage.Name = "fieldsTabPage";
			this.fieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.fieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 285, true);
			this.fieldsTabPage.TabIndex = 0;
			this.fieldsTabPage.UseVisualStyleBackColor = true;
			// 
			// fieldsHost
			// 
			this.fieldsHost.AutoScroll = true;
			this.fieldsHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fieldsHost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.fieldsHost.Name = "fieldsHost";
			this.fieldsHost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 279, true);
			this.fieldsHost.TabIndex = 0;
			// 
			// documentsControl
			// 
			this.BindingSource.SetBindingMember(documentsControl, ".");
			documentsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			documentsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			documentsControl.Name = "documentsControl";
			documentsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 279, true);
			documentsControl.TabIndex = 0;
			// 
			// runnerTabControl
			// 
			this.runnerTabControl.AccessibleDescription = "Action.DocumentPivots";
			this.runnerTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.runnerTabControl.Controls.Add(this.fieldsTabPage);
			this.runnerTabControl.Controls.Add(this.documentsTabPage);
			this.runnerTabControl.Controls.Add(this.progressTabPage);
			this.runnerTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.runnerTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.runnerTabControl.Name = "runnerTabControl";
			this.runnerTabControl.SelectedIndex = 0;
			this.runnerTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 312, true);
			this.runnerTabControl.TabIndex = 0;
			// 
			// documentsTabPage
			// 
			this.documentsTabPage.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("OperationalActionRunnerForm|9d961e6d-dbcf-4a37-b5d0-985f75af3e82", "Documents");
			this.documentsTabPage.Controls.Add(documentsControl);
			this.documentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.documentsTabPage.Name = "documentsTabPage";
			this.documentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.documentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 285, true);
			this.documentsTabPage.TabIndex = 1;
			// 
			// progressTabPage
			// 
			this.progressTabPage.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("OperationalActionRunnerForm|279b9ffe-1b72-40a7-a35c-58b63b726a16", "Progress");
			this.progressTabPage.Controls.Add(this.progressControl);
			this.progressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.progressTabPage.Name = "progressTabPage";
			this.progressTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.progressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 285, true);
			this.progressTabPage.TabIndex = 2;
			// 
			// progressControl
			// 
			this.progressControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.progressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.progressControl.Name = "progressControl";
			this.progressControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.progressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 279, true);
			this.progressControl.TabIndex = 0;
			// 
			// OperationalActionRunnerForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 373, true);
			this.Controls.Add(this.runnerTabControl);
			this.Controls.Add(topPanel);
			this.DataSourceAssemblyName = "Enterprise.Services.OperationalActions.Business";
			this.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalActionRunner);
			this.DataSourceTypeName = "Enterprise.Services.OperationalActions.Business.OperationalActionRunner";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 375, true);
			this.Name = "OperationalActionRunnerForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "OperationalActionRunner";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(topPanel, 0);
			this.Controls.SetChildIndex(this.runnerTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			this.fieldsTabPage.ResumeLayout(false);
			this.runnerTabControl.ResumeLayout(false);
			this.documentsTabPage.ResumeLayout(false);
			this.progressTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.GUI.ZTabControl runnerTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage documentsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage progressTabPage;
		private Enterprise.ZArchitecture.GUI.ZButton okButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton RunOnSelectedRecordsRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton RunOnAllMatchingRecordsRadioButton;
		internal ActionLog progressControl;
		private RunnerFieldControlHost fieldsHost;
		private Enterprise.ZArchitecture.GUI.ZTabPage fieldsTabPage;
	}
}
