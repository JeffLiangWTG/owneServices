using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class LPCOForm
	{
		#region Component Designer generated code

		private new void InitializeComponent()
		{
			this.LPCOUserControl = new Enterprise.Customs.BR.GUI.LPCOUserControl();
			this.MessageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageStatusPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageTabUserControl = new Enterprise.Customs.BR.GUI.MessagesTabUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MessageStatusDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MessageStatusPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTabPage.SuspendLayout();
			this.MessageTabUserControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.MessageTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 568, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.LPCOUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 570, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusLPCOHeader);
			// 
			// LPCOUserControl
			// 
			this.LPCOUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOUserControl, ".");
			this.LPCOUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCOUserControl.Name = "LPCOUserControl";
			this.LPCOUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 603, true);
			this.LPCOUserControl.TabIndex = 0;
			//
			// MessageTabPage
			//
			this.MessageTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("04F7A9F9-B9F8-48C8-ADC7-36FB67C44732", "Messages");
			this.MessageTabPage.Controls.Add(this.MessageTabUserControl);
			this.MessageTabPage.Controls.Add(this.MessageStatusPanel);
			this.MessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTabPage.Name = "MessageTabPage";
			this.MessageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
			this.MessageTabPage.TabIndex = 0;
			// 
			// MessageStatusPanel
			// 
			this.MessageStatusPanel.Controls.Add(this.MessageStatusDropEdit);
			this.MessageStatusPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageStatusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageStatusPanel.Name = "MessageStatusPanel";
			this.MessageStatusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 40, true);
			this.MessageStatusPanel.TabIndex = 1;
			//
			// MessageTabUserControl
			//
			this.MessageTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTabUserControl, "Messages");
			this.MessageTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTabUserControl.Name = "MessageTabUserControl";
			this.MessageTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 488, true);
			this.MessageTabUserControl.TabIndex = 0;
			// 
			// MessageStatusDropEdit
			//
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "CPH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusLPCOHeader)(null)).CPH_MessageStatus)));
			this.MessageStatusDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 9, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.ShowDescriptionBox = true;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.MessageStatusDropEdit.TabIndex = 0;
			//
			// WorkflowTabPage
			//
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
			this.WorkflowTabPage.TabIndex = 0;
			//
			// LPCOForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1136, 623, true);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "LPCOForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.LPCOUserControl.ResumeLayout(true);
			this.LPCOUserControl.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.MessageStatusPanel.ResumeLayout(false);
			this.MessageStatusPanel.PerformLayout();
			this.MessageTabUserControl.ResumeLayout(true);
			this.MessageTabUserControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		LPCOUserControl LPCOUserControl;
		internal Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MessageTabPage;
		internal Enterprise.Customs.BR.GUI.MessagesTabUserControl MessageTabUserControl;
		private ZPanel MessageStatusPanel;
		public ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
	}
}
