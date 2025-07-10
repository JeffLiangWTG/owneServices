using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ComponentRelationshipForm
	{
		new void InitializeComponent()
		{
			this.ComponentRelationshipControl = new ComponentRelationshipControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ComponentRelationshipControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 407, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ComponentRelationshipControl);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 385, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 385, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 385, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 407, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ComponentRelationship);
			// 
			// ComponentRelationshipControl
			// 
			this.ComponentRelationshipControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComponentRelationshipControl, ".");
			this.ComponentRelationshipControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentRelationshipControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentRelationshipControl.Name = "ComponentRelationshipControl";
			this.ComponentRelationshipControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 385, true);
			this.ComponentRelationshipControl.TabIndex = 0;
			// 
			// ComponentRelationshipForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("04cb9519-eb9e-46ac-a534-5909f37dea7a", "Component Relationship");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 463, true);
			this.DataSourceType = typeof(ComponentRelationship);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Name = "ComponentRelationshipForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.ComponentRelationshipControl.ResumeLayout(true);
			this.ComponentRelationshipControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
