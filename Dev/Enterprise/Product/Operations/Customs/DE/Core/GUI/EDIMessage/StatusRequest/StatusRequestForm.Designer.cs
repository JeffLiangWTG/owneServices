namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportStatusRequestForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.statusRequestTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.statusRequestTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.statusRequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.identificationFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.moduleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.roleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.mrnBox = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.statusRequestTabControl.SuspendLayout();
			this.statusRequestTabPage.SuspendLayout();
			this.statusRequestGroupBox.SuspendLayout();
			this.identificationFindBox.SuspendLayout();
			this.moduleDropEdit.SuspendLayout();
			this.roleDropEdit.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.StatusRequest);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c8046d18-5c6c-4907-97cc-a83fc0eef2df", "Send");
			this.SendButton.IsCaptionOverridden = false;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 3, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.SendButton.TabIndex = 0;
			this.SendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4887e59c-3330-4159-a399-8334e7056b7e", "Cancel");
			this.CancelButton2.IsCaptionOverridden = false;
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(680, 3, true);
			this.CancelButton2.Name = "CancelButton2";
			this.CancelButton2.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.CancelButton2.TabIndex = 1;
			this.CancelButton2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton2.ToolTipCaption = null;
			this.CancelButton2.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// statusRequestTabControl
			// 
			this.statusRequestTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.statusRequestTabControl.Controls.Add(this.statusRequestTabPage);
			this.statusRequestTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusRequestTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.statusRequestTabControl.Name = "statusRequestTabControl";
			this.statusRequestTabControl.SelectedIndex = 0;
			this.statusRequestTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 259, true);
			this.statusRequestTabControl.TabIndex = 0;
			// 
			// statusRequestTabPage
			// 
			this.statusRequestTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("7a47d2db-bf53-41bb-8417-b48fa1137840", "Request");
			this.statusRequestTabPage.Controls.Add(this.statusRequestGroupBox);
			this.statusRequestTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.statusRequestTabPage.Name = "statusRequestTabPage";
			this.statusRequestTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.statusRequestTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 232, true);
			this.statusRequestTabPage.TabIndex = 0;
			this.statusRequestTabPage.UseVisualStyleBackColor = true;
			// 
			// statusRequestGroupBox
			// 
			this.statusRequestGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c7ed46f5-49ee-4a6a-88c0-ec910698d287", "Status Request");
			this.statusRequestGroupBox.Controls.Add(this.identificationFindBox);
			this.statusRequestGroupBox.Controls.Add(this.moduleDropEdit);
			this.statusRequestGroupBox.Controls.Add(this.roleDropEdit);
			this.statusRequestGroupBox.Controls.Add(this.mrnBox);
			this.statusRequestGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusRequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.statusRequestGroupBox.Name = "statusRequestGroupBox";
			this.statusRequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(771, 226, true);
			this.statusRequestGroupBox.TabIndex = 2;
			this.statusRequestGroupBox.TabStop = false;
			// 
			// identificationFindBox
			// 
			this.identificationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.identificationFindBox, "Identification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.StatusRequest)(null)).Identification)));
			this.identificationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 86, true);
			this.identificationFindBox.Name = "identificationFindBox";
			this.identificationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.identificationFindBox.TabIndex = 2;
			// 
			// moduleDropEdit
			// 
			this.moduleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.moduleDropEdit, "Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.StatusRequest)(null)).Module)));
			this.moduleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 34, true);
			this.moduleDropEdit.Name = "moduleDropEdit";
			this.moduleDropEdit.ShouldResizeByMaxLength = true;
			this.moduleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.moduleDropEdit.TabIndex = 0;
			// 
			// roleDropEdit
			// 
			this.roleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.roleDropEdit, "Role");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.StatusRequest)(null)).Role)));
			this.roleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 112, true);
			this.roleDropEdit.Name = "roleDropEdit";
			this.roleDropEdit.ShouldResizeByMaxLength = true;
			this.roleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.roleDropEdit.TabIndex = 3;
			// 
			// mrnBox
			// 
			this.BindingSource.SetBindingMember(this.mrnBox, "MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.StatusRequest)(null)).MovementReferenceNumber)));
			this.mrnBox.CaptionResourceString = null;
			this.mrnBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 60, true);
			this.mrnBox.Name = "mrnBox";
			this.mrnBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.mrnBox.TabIndex = 1;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.CancelButton2);
			this.buttonPanel.Controls.Add(this.SendButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 259, true);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 31, true);
			this.buttonPanel.TabIndex = 4;
			// 
			// ExportStatusRequestForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 313, true);
			this.Controls.Add(this.statusRequestTabControl);
			this.Controls.Add(this.buttonPanel);
			this.DataSourceType = typeof(Enterprise.Customs.DE.Business.StatusRequest);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 350, true);
			this.Name = "ExportStatusRequestForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.buttonPanel, 0);
			this.Controls.SetChildIndex(this.statusRequestTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.statusRequestTabControl.ResumeLayout(false);
			this.statusRequestTabControl.PerformLayout();
			this.statusRequestTabPage.ResumeLayout(false);
			this.statusRequestTabPage.PerformLayout();
			this.statusRequestGroupBox.ResumeLayout(false);
			this.statusRequestGroupBox.PerformLayout();
			this.identificationFindBox.ResumeLayout(true);
			this.identificationFindBox.PerformLayout();
			this.moduleDropEdit.ResumeLayout(true);
			this.moduleDropEdit.PerformLayout();
			this.roleDropEdit.ResumeLayout(true);
			this.roleDropEdit.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelButton2;
		private Enterprise.ZArchitecture.GUI.ZTabControl statusRequestTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage statusRequestTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox statusRequestGroupBox;
		private Enterprise.ZArchitecture.ZTextBox mrnBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit roleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit moduleDropEdit;
		private System.ComponentModel.IContainer components;
		private ZArchitecture.GUI.ZPanel buttonPanel;
		private ZArchitecture.GUI.ZGuidFindBox identificationFindBox;
	}
}
