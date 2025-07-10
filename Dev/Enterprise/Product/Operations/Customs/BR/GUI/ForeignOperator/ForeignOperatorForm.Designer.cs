using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class ForeignOperatorForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTabUserControl = new Enterprise.Customs.BR.GUI.MessagesTabUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.ForeignOperatorUserControl = new Enterprise.Customs.BR.GUI.ForeignOperatorUserControl();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesTabPage.SuspendLayout();
			this.MessageTabUserControl.SuspendLayout();
			this.ForeignOperatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusBRForeignOperator);
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 555, true);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(ForeignOperatorUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 528, true);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 1;
			this.WorkflowTabPage.TabRelevant = true;
			//
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 528, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 528, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 555, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(546, 6, true);
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 23, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 24, true);
			//
			// MessagesTabPage
			//
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6E681515-4E5A-4F30-897F-8F5C81A8DA59", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessageTabUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 465, true);
			this.MessagesTabPage.TabIndex = 3;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageTabUserControl
			//
			this.MessageTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTabUserControl, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.BR.Business.CusGoodsCatalog)(null)).Messages)));
			this.MessageTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 56, true);
			this.MessageTabUserControl.Name = "MessageTabUserControl";
			this.MessageTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 459, true);
			this.MessageTabUserControl.TabIndex = 0;
			// 
			// ForeignOperatorUserControl
			//
			this.ForeignOperatorUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForeignOperatorUserControl, ".");
			this.ForeignOperatorUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForeignOperatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 56, true);
			this.ForeignOperatorUserControl.Name = "ForeignOperatorUserControl";
			this.ForeignOperatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 459, true);
			this.ForeignOperatorUserControl.TabIndex = 0;
			// 
			// ForeignOperatorForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 611, true);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusBRForeignOperator);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 500, true);
			this.Name = "ForeignOperatorForm";
			this.ShouldSerializeTabPageMethods = false;
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ForeignOperatorUserControl.ResumeLayout(true);
			this.ForeignOperatorUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessageTabUserControl.ResumeLayout(true);
			this.MessageTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ForeignOperatorUserControl ForeignOperatorUserControl;
		internal ZTabPage MessagesTabPage;
		internal MessagesTabUserControl MessageTabUserControl;
		public Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
