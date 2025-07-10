using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class TemporaryStorageForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.BillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UCC6TemporaryStorageBillControl = new Enterprise.Customs.EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillControl();
			this.UCC6TemporaryStorageBillsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ContainerTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UCC6TemporaryStorageContainerControl = new Enterprise.Customs.EU.TemporaryStorage.GUI.UCC6TemporaryStorageContainerControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillsTabPage.SuspendLayout();
			this.UCC6TemporaryStorageBillControl.SuspendLayout();
			this.ContainerTabPage.SuspendLayout();
			this.UCC6TemporaryStorageContainerControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ContainerTabPage);
			this.MainTabControl.Controls.Add(this.BillsTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1130, 600, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.BillsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContainerTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.MainTabPage.CaptionResourceString = Res.GetData("CF0C967F-C3FF-459C-9A8C-D6574C43D669", "Header");
			this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1130, 600, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 0, true);
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 32, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1130, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// BillsTabPage
			// 
			this.BillsTabPage.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.BillsTabPage.CaptionResourceString = Res.GetData("db24bb14-a435-4c0f-a1a0-326923ec3129", "Bills");
			this.BillsTabPage.Controls.Add(this.UCC6TemporaryStorageBillsLayoutPanel);
			this.BillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillsTabPage.Name = "BillsTabPage";
			this.BillsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			this.BillsTabPage.TabIndex = 4;
			// 
			// UCC6TemporaryStorageBillControl
			// 
			this.UCC6TemporaryStorageBillControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UCC6TemporaryStorageBillControl, ".");
			this.UCC6TemporaryStorageBillControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UCC6TemporaryStorageBillControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UCC6TemporaryStorageBillControl.Name = "UCC6TemporaryStorageBillControl";
			this.UCC6TemporaryStorageBillControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1116, 567, true);
			this.UCC6TemporaryStorageBillControl.TabIndex = 3;
			// 
			// UCC6TemporaryStorageBillsLayoutPanel
			// 
			this.UCC6TemporaryStorageBillsLayoutPanel.AllowDrop = true;
			this.UCC6TemporaryStorageBillsLayoutPanel.AutoScroll = true;
			this.UCC6TemporaryStorageBillsLayoutPanel.Controls.Add(this.UCC6TemporaryStorageBillControl);
			this.UCC6TemporaryStorageBillsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UCC6TemporaryStorageBillsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UCC6TemporaryStorageBillsLayoutPanel.Name = "UCC6TemporaryStorageBillsLayoutPanel";
			this.UCC6TemporaryStorageBillsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1116, 567, true);
			this.UCC6TemporaryStorageBillsLayoutPanel.TabIndex = 2;
			// 
			// ContainerTabPage
			// 
			this.ContainerTabPage.CaptionResourceString = Res.GetData("C8DEF5C4-E42A-4B38-81BF-5EBE98F5C3F1", "Containers");
			this.ContainerTabPage.Controls.Add(this.UCC6TemporaryStorageContainerControl);
			this.ContainerTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerTabPage.Name = "ContainerTabPage";
			this.ContainerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			this.ContainerTabPage.TabIndex = 7;
			// 
			// UCC6TemporaryStorageContainerControl
			// 
			this.UCC6TemporaryStorageContainerControl.AllowDrop = true;
			this.UCC6TemporaryStorageContainerControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.UCC6TemporaryStorageContainerControl, ".");
			this.UCC6TemporaryStorageContainerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UCC6TemporaryStorageContainerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UCC6TemporaryStorageContainerControl.Name = "UCC6TemporaryStorageContainerControl";
			this.UCC6TemporaryStorageContainerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1122, 573, true);
			this.UCC6TemporaryStorageContainerControl.TabIndex = 6;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Res.GetData("6D6D0244-4A4E-41BB-87E3-891D2438AF78", "Messages");
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
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 8;
			// 
			// TemporaryStorageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 656, true);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 695, true);
			this.Name = "TemporaryStorageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "TemporaryStorageForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillsTabPage.ResumeLayout(false);
			this.BillsTabPage.PerformLayout();
			this.UCC6TemporaryStorageBillControl.ResumeLayout(true);
			this.UCC6TemporaryStorageBillControl.PerformLayout();
			this.ContainerTabPage.ResumeLayout(false);
			this.ContainerTabPage.PerformLayout();
			this.UCC6TemporaryStorageContainerControl.ResumeLayout(true);
			this.UCC6TemporaryStorageContainerControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl MessagesUserControl;
		protected ZTabPage MessagesTabPage;
		DynamicLayoutPanel UCC6TemporaryStorageBillsLayoutPanel;
		ZTabPage BillsTabPage;
		UCC6TemporaryStorageBillControl UCC6TemporaryStorageBillControl;
		protected ZTabPage ContainerTabPage;
		UCC6TemporaryStorageContainerControl UCC6TemporaryStorageContainerControl;
		protected Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
