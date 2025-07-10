using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowRelationshipsForm
	{
		#region Component Designer generated code

		ZPanel zPanel1;
		WorkflowRelationshipsUserControl workflowRelationshipsUserControl;
		new void InitializeComponent()
		{
			this.zPanel1 = new ZPanel();
			this.workflowRelationshipsUserControl = new WorkflowRelationshipsUserControl();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.zPanel1);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 735, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ProcessHeader);
			// 
			// zPanel1
			// 
			this.zPanel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.zPanel1.Controls.Add(this.workflowRelationshipsUserControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 735, true);
			this.zPanel1.TabIndex = 2;
			// 
			// workflowRelationshipsUserControl
			// 
			this.workflowRelationshipsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.workflowRelationshipsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.workflowRelationshipsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workflowRelationshipsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.workflowRelationshipsUserControl.Name = "workflowRelationshipsUserControl";
			this.workflowRelationshipsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 735, true);
			this.workflowRelationshipsUserControl.TabIndex = 0;
			// 
			// WorkflowRelationshipsForm
			// 
			this.AllowDrop = true;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("WorkflowRelationshipsForm|eb2c221a-a56e-42e0-9385-da247a4910f3", "Workflow Relationships");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 791, true);
			this.DataSourceType = typeof(ProcessHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 726, true);
			this.Name = "WorkflowRelationshipsForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
