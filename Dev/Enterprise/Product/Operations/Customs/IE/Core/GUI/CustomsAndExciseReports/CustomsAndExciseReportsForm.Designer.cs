using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	partial class CustomsAndExciseReportsForm
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
		private new void InitializeComponent()
		{
            this.RequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ReportTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.MessageNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.SaveButtonUserControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.RequestGroupBox.SuspendLayout();
            this.ReportTypeZDropEdit.SuspendLayout();
            this.DateZDateEdit.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 330, true);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			// 
			// MainTabPage
			// 
            this.MainTabPage.Controls.Add(this.RequestGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 308, true);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Res.GetData("30686633-BD83-4240-AA18-022DB1F2F0EA", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			this.MessagesTabPage.TabIndex = 3;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 308, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 308, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 330, true);
            // 
            // SaveButtonUserControl
            // 
            this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 6, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.CustomsAndExciseReportOutboundMessage);
            // 
            // RequestGroupBox
            // 
            this.RequestGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RequestGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("CustomsAndExciseReportsForm|RequestGroupBox", "Request");
            this.RequestGroupBox.Controls.Add(this.ReportTypeZDropEdit);
            this.RequestGroupBox.Controls.Add(this.DateZDateEdit);
            this.RequestGroupBox.Controls.Add(this.MessageNumberZTextBox);
            this.RequestGroupBox.Controls.Add(this.MessageStatusTextBox);
            this.RequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 9, true);
            this.RequestGroupBox.Name = "RequestGroupBox";
            this.RequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 136, true);
            this.RequestGroupBox.TabIndex = 0;
            this.RequestGroupBox.TabStop = false;
            // 
            // ReportTypeZDropEdit
            // 
            this.ReportTypeZDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReportTypeZDropEdit, "EM_MessageType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.CustomsAndExciseReportOutboundMessage)(null)).EM_MessageType)));
            this.ReportTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 35, true);
            this.ReportTypeZDropEdit.Name = "ReportTypeZDropEdit";
            this.ReportTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
            this.ReportTypeZDropEdit.TabIndex = 0;
            // 
            // DateZDateEdit
            // 
            this.DateZDateEdit.AllowDrop = true;
            this.DateZDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DateZDateEdit, "MessageDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.CustomsAndExciseReportOutboundMessage)(null)).MessageDate)));
            this.DateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 79, true);
            this.DateZDateEdit.Name = "DateZDateEdit";
            this.DateZDateEdit.TabIndex = 4;
            // 
            // MessageNumberZTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageNumberZTextBox, "EM_MessageNum");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CustomsAndExciseReportOutboundMessage)(null)).EM_MessageNum)));
            this.MessageNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 35, true);
            this.MessageNumberZTextBox.Name = "MessageNumberZTextBox";
            this.MessageNumberZTextBox.ReadOnly = true;
            this.MessageNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 17, true);
            this.MessageNumberZTextBox.TabIndex = 1;
            // 
            // MessageStatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "EM_Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CustomsAndExciseReportOutboundMessage)(null)).EM_Status)));
            this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 79, true);
            this.MessageStatusTextBox.Name = "MessageStatusTextBox";
            this.MessageStatusTextBox.ReadOnly = true;
            this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
            this.MessageStatusTextBox.TabIndex = 5;
			// 
			// CustomsAndExciseReportsForm
			// 
			this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("CustomsAndExciseReportsForm|66149197-E7C2-4EA1-9FA5-E0453C63E665", "Customs and Excise Reports");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 386, true);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 386, true);
			this.DataSourceType = typeof(Enterprise.Customs.IE.Business.CustomsAndExciseReportOutboundMessage);
            this.Name = "CustomsAndExciseReportsForm";
            this.ShouldSerializeTabPageMethods = false;
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.SaveButtonUserControl.ResumeLayout(true);
            this.SaveButtonUserControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.RequestGroupBox.ResumeLayout(false);
            this.RequestGroupBox.PerformLayout();
            this.ReportTypeZDropEdit.ResumeLayout(true);
            this.ReportTypeZDropEdit.PerformLayout();
            this.DateZDateEdit.ResumeLayout(true);
            this.DateZDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox RequestGroupBox;
		private ZArchitecture.ZTextBox MessageNumberZTextBox;
		private ZArchitecture.GUI.ZDropEdit ReportTypeZDropEdit;
		private ZArchitecture.ZTextBox MessageStatusTextBox;
		private ZArchitecture.GUI.ZDateEdit DateZDateEdit;
		Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl MessagesUserControl;
		private ZTabPage MessagesTabPage;
	}
}
