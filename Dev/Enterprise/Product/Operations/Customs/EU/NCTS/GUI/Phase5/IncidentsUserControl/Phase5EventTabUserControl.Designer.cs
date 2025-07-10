using CargoWise.Windows.UI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5EventTabUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.IncidentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncidentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContainersAndSealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersAndSealsUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5ArrivalIncidentsContainersAndSealsUserControl();
			this.IncidentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncidentDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncidentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IncidentsSplitContainer)).BeginInit();
			this.IncidentsSplitContainer.Panel2.SuspendLayout();
			this.IncidentsSplitContainer.SuspendLayout();
			this.ContainersAndSealsGroupBox.SuspendLayout();
			this.ContainersAndSealsUserControl.SuspendLayout();
			this.IncidentDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.EnRouteIncidentCollection);
			// 
			// IncidentGroupBox
			// 
			this.IncidentGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("a572f352-984e-40ad-bf4b-68944af20636", "Incidents");
			this.IncidentGroupBox.Controls.Add(this.IncidentsSplitContainer);
			this.IncidentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.IncidentGroupBox.Name = "IncidentGroupBox";
			this.IncidentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1339, 348, true);
			this.IncidentGroupBox.TabIndex = 1;
			this.IncidentGroupBox.TabStop = false;
			// 
			// IncidentsSplitContainer
			// 
			this.IncidentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.IncidentsSplitContainer.Name = "IncidentsSplitContainer";
			this.IncidentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.IncidentsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40);
			// 
			// IncidentsSplitContainer.Panel2
			// 
			this.IncidentsSplitContainer.Panel2.Controls.Add(this.ContainersAndSealsGroupBox);
			this.IncidentsSplitContainer.Panel2.Controls.Add(this.IncidentDetailsGroupBox);
			this.IncidentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1333, 314, true);
			this.IncidentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(65);
			this.IncidentsSplitContainer.SplitterWidth = 1;
			this.IncidentsSplitContainer.TabIndex = 0;
			this.IncidentsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(250);
			// 
			// ContainersAndSealsGroupBox
			// 
			this.ContainersAndSealsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("6A7EF788-E376-496A-AA0F-AF5A2B194EF4", "Containers/Equipment and Seals");
			this.ContainersAndSealsGroupBox.Controls.Add(this.ContainersAndSealsUserControl);
			this.ContainersAndSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(826, 0, true);
			this.ContainersAndSealsGroupBox.Name = "ContainersAndSealsGroupBox";
			this.ContainersAndSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 248, true);
			this.ContainersAndSealsGroupBox.TabIndex = 1;
			this.ContainersAndSealsGroupBox.TabStop = false;
			// 
			// ContainersAndSealsUserControl
			// 
			this.ContainersAndSealsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainersAndSealsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)))));
			this.ContainersAndSealsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.ContainersAndSealsUserControl.Name = "ContainersAndSealsUserControl";
			this.ContainersAndSealsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 214, true);
			this.ContainersAndSealsUserControl.TabIndex = 0;
			// 
			// IncidentDetailsGroupBox
			// 
			this.IncidentDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("9c44c717-62fe-40e0-bd5c-557df22c7cdb", "Incident Details");
			this.IncidentDetailsGroupBox.Controls.Add(this.IncidentDetailsDynamicLayoutPanel);
			this.IncidentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.IncidentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncidentDetailsGroupBox.Name = "IncidentDetailsGroupBox";
			this.IncidentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 248, true);
			this.IncidentDetailsGroupBox.TabIndex = 0;
			this.IncidentDetailsGroupBox.TabStop = false;
			// 
			// IncidentDetailsDynamicLayoutPanel
			// 
			this.IncidentDetailsDynamicLayoutPanel.AllowDrop = true;
			this.IncidentDetailsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.IncidentDetailsDynamicLayoutPanel.Name = "IncidentDetailsDynamicLayoutPanel";
			this.IncidentDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 214, true);
			this.IncidentDetailsDynamicLayoutPanel.TabIndex = 0;
			// 
			// Phase5EventTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncidentGroupBox);
			this.Name = "Phase5EventTabUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1339, 353, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncidentGroupBox.ResumeLayout(false);
			this.IncidentGroupBox.PerformLayout();
			this.IncidentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.IncidentsSplitContainer)).EndInit();
			this.IncidentsSplitContainer.ResumeLayout(false);
			this.IncidentsSplitContainer.PerformLayout();
			this.ContainersAndSealsGroupBox.ResumeLayout(false);
			this.ContainersAndSealsGroupBox.PerformLayout();
			this.ContainersAndSealsUserControl.ResumeLayout(true);
			this.ContainersAndSealsUserControl.PerformLayout();
			this.IncidentDetailsGroupBox.ResumeLayout(false);
			this.IncidentDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox IncidentGroupBox;
		internal KSplitContainer IncidentsSplitContainer;
		internal ZArchitecture.GUI.ZGroupBox IncidentDetailsGroupBox;
		internal ZArchitecture.GUI.ZGroupBox ContainersAndSealsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel IncidentDetailsDynamicLayoutPanel;
		internal Phase5ArrivalIncidentsContainersAndSealsUserControl ContainersAndSealsUserControl;
	}
}
