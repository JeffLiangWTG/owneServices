using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5ArrivalMovementForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IncidentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.UnloadingRemarksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomFieldTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.IncidentsTabPage);
			this.MainTabControl.Controls.Add(this.UnloadingRemarksTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.CustomFieldTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.UnloadingRemarksTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.IncidentsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CustomFieldTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("75B54F3C-1936-43AF-BABE-5DBF298874D8", "Arrival Notification");
			this.MainTabPage.AutoScroll = true;
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// IncidentsTabPage
			// 
			this.IncidentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0CB57589-65F1-4DA8-9DFF-C92E9322E40B", "Incidents");
			this.IncidentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IncidentsTabPage.Name = "IncidentsTabPage";
			this.IncidentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IncidentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true);
			this.IncidentsTabPage.TabIndex = 6;
			this.IncidentsTabPage.UseVisualStyleBackColor = true;
			this.IncidentsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.IncidentsTabPage_InitializeTab));
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("FFE1E9A6-CA2A-4C1E-A598-9B59E51BE4B8", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true);
			this.MessagesTabPage.TabIndex = 6;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			this.MessagesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MessagesTabPage_InitializeTab));
			// 
			// NCAMovementCustomTabPage
			// 
			this.CustomFieldTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("196359E4-221B-4A71-843B-8D511DB97698", "Custom Fields");
			this.CustomFieldTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CustomFieldTabPage.UseVisualStyleBackColor = true;
			this.CustomFieldTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true);
			this.CustomFieldTabPage.TabIndex = 7;
			this.CustomFieldTabPage.Text = Res.GetString("06395372-A173-46CE-B4A2-2903C12A6E61", "Custom Fields");
			this.CustomFieldTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CustomFieldTabPage_InitializeTab));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.INctsBillCollection<Enterprise.Customs.EU.NCTS.Business.NctsBill>)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Bills)));
			// 
			// UnloadingRemarksTabPage
			// 
			this.UnloadingRemarksTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B252388F-768E-4C96-801A-1A13EB2194DD", "Unloading Remarks");
			this.UnloadingRemarksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnloadingRemarksTabPage.Name = "UnloadingRemarksTabPage";
			this.UnloadingRemarksTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnloadingRemarksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true);
			this.UnloadingRemarksTabPage.TabIndex = 4;
			this.UnloadingRemarksTabPage.UseVisualStyleBackColor = true;
			this.UnloadingRemarksTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.UnloadingRemarksTabPage_InitializeTab));
			// 
			// Phase5ArrivalMovementForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 686, true);
			this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "Phase5ArrivalMovementForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(nctsHeader.DefaultDataGroupingCode);
			this.ArrivalNotificationTabUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.MainTabPage.SuspendLayout();
			this.ArrivalNotificationTabUserControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.ArrivalNotificationTabUserControl);
			// 
			// ArrivalNotificationTabUserControl
			// 
			this.ArrivalNotificationTabUserControl.AllowDrop = true;
			this.ArrivalNotificationTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ArrivalNotificationTabUserControl, ".");
			this.ArrivalNotificationTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ArrivalNotificationTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ArrivalNotificationTabUserControl.Name = "ArrivalNotificationTabUserControl";
			this.ArrivalNotificationTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 592, true);
			this.ArrivalNotificationTabUserControl.TabIndex = 0;
			this.ArrivalNotificationTabUserControl.UserControlType = provider.ArrivalNotificationTabControlType;
			this.MainTabPage.PerformLayout();
			this.ArrivalNotificationTabUserControl.ResumeLayout(true);
			this.ArrivalNotificationTabUserControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		void UnloadingRemarksTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.UnloadingRemarksTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5UnloadingRemarksTabUserControl();
			this.UnloadingRemarksTabPage.SuspendLayout();
			this.UnloadingRemarksTabUserControl.SuspendLayout();
			this.UnloadingRemarksTabPage.Controls.Add(this.UnloadingRemarksTabUserControl);
			// 
			// UnloadingRemarksTabUserControl
			// 
			this.UnloadingRemarksTabUserControl.AllowDrop = true;
			this.UnloadingRemarksTabUserControl.AutoSize = true;
			this.UnloadingRemarksTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.UnloadingRemarksTabUserControl, ".");
			this.UnloadingRemarksTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingRemarksTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UnloadingRemarksTabUserControl.Name = "UnloadingRemarksTabUserControl";
			this.UnloadingRemarksTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 597, true);
			this.UnloadingRemarksTabUserControl.TabIndex = 0;
			this.UnloadingRemarksTabPage.PerformLayout();
			this.UnloadingRemarksTabUserControl.ResumeLayout(true);
			this.UnloadingRemarksTabUserControl.PerformLayout();
			this.UnloadingRemarksTabPage.ResumeLayout(true);

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
			this.BindingSource.SetBindingMember(this.MessagesTabDynamicUserControl, "Messages");
			this.MessagesTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesTabDynamicUserControl.Name = "MessagesTabDynamicUserControl";
			this.MessagesTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 597, true);
			this.MessagesTabDynamicUserControl.TabIndex = 0;
			this.MessagesTabDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.MessagesTabUserControl);
			this.MessagesTabPage.PerformLayout();
			this.MessagesTabDynamicUserControl.ResumeLayout(true);
			this.MessagesTabDynamicUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(true);

		}

		void IncidentsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.EventTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5EventTabUserControl();
			this.IncidentsTabPage.SuspendLayout();
			this.EventTabUserControl.SuspendLayout();
			this.IncidentsTabPage.Controls.Add(this.EventTabUserControl);
			// 
			// EventTabUserControl
			// 
			this.EventTabUserControl.AllowDrop = true;
			this.EventTabUserControl.AutoSize = true;
			this.EventTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.EventTabUserControl, "EnRouteIncidents");
			this.EventTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EventTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EventTabUserControl.Name = "EventTabUserControl";
			this.EventTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 597, true);
			this.EventTabUserControl.TabIndex = 0;
			this.IncidentsTabPage.PerformLayout();
			this.EventTabUserControl.ResumeLayout(true);
			this.EventTabUserControl.PerformLayout();
			this.IncidentsTabPage.ResumeLayout(true);

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
			this.ProcessTemplateCustomFieldsControl.AllowDrop = true;
			this.ProcessTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcessTemplateCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.BindingSource.SetBindingMember(this.ProcessTemplateCustomFieldsControl, "ArrivalMovementHeader");
			this.ProcessTemplateCustomFieldsControl.Name = "NCAProcessTemplateCustomFieldsControl";
			this.ProcessTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1201, 364, true);
			this.ProcessTemplateCustomFieldsControl.TabIndex = 0;
			this.ProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("F9213819-4188-446D-B9C1-1B58498C520A", "To make use of this tab, please setup NCTS - Arrival custom fields in Workflow Manager.");
			this.CustomFieldTabPage.PerformLayout();
			this.CustomFieldTabPage.ResumeLayout(true);
			this.ProcessTemplateCustomFieldsControl.PerformLayout();
			this.ProcessTemplateCustomFieldsControl.ResumeLayout(true);
		}

		#endregion

		internal Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage UnloadingRemarksTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage IncidentsTabPage;
		internal ZDynamicControlCreationUserControl ArrivalNotificationTabUserControl;
		internal Phase5UnloadingRemarksTabUserControl UnloadingRemarksTabUserControl;
		internal Phase5EventTabUserControl EventTabUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl MessagesTabDynamicUserControl;
		internal ZArchitecture.GUI.ZTabPage CustomFieldTabPage;
		internal ZArchitecture.GUI.ProcessTemplateCustomFieldsControl ProcessTemplateCustomFieldsControl;
	}
}
