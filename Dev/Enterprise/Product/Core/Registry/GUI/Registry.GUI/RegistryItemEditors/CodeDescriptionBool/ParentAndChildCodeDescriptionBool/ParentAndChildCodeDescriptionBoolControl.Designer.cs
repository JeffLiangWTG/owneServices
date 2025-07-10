namespace Enterprise.Registry.GUI
{
	partial class ParentAndChildCodeDescriptionBoolControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ParentGroupBox;
		protected internal Enterprise.Registry.GUI.CodeDescriptionBoolControl ParentGrid;
		protected internal Enterprise.Registry.GUI.CodeDescriptionBoolControl ChildGrid;
		internal CargoWise.Windows.UI.KSplitter GridSplitter;
		internal Enterprise.ZArchitecture.GUI.ZPanel ChildGridPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ChildGroupBox;

		void InitializeComponent()
		{
			this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentGrid = new Enterprise.Registry.GUI.CodeDescriptionBoolControl();
			this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildGrid = new Enterprise.Registry.GUI.CodeDescriptionBoolControl();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ChildGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			this.ChildGroupBox.SuspendLayout();
			this.ChildGridPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionBoolCollection);
			// 
			// ParentGroupBox
			// 
			this.ParentGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ParentAndChildCodeDescriptionBoolControl|203683fb-d42c-4bb6-a697-019568b765ca", "Parent");
			this.ParentGroupBox.Controls.Add(this.ParentGrid);
			this.ParentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ParentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentGroupBox.Name = "ParentGroupBox";
			this.ParentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 176, true);
			this.ParentGroupBox.TabIndex = 0;
			this.ParentGroupBox.TabStop = false;
			// 
			// ParentGrid
			// 
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.ReadOnly = false;
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 157, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// ChildGroupBox
			// 
			this.ChildGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ParentAndChildCodeDescriptionBoolControl|b3e39d29-a4bf-4dc7-b4ed-cbc28fe9a8f5", "Child");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 197, true);
			this.ChildGroupBox.TabIndex = 1;
			this.ChildGroupBox.TabStop = false;
			// 
			// ChildGrid
			// 
			this.BindingSource.SetBindingMember(this.ChildGrid, "ChildList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.CodeDescriptionBoolCollection)(((Enterprise.Registry.Business.ParentCodeDescriptionBool)(null)).ChildList)));
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.ReadOnly = false;
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 178, true);
			this.ChildGrid.TabIndex = 0;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 3, true);
			this.GridSplitter.TabIndex = 2;
			this.GridSplitter.TabStop = false;
			// 
			// ChildGridPanel
			// 
			this.ChildGridPanel.Controls.Add(this.ChildGroupBox);
			this.ChildGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.ChildGridPanel.Name = "ChildGridPanel";
			this.ChildGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 197, true);
			this.ChildGridPanel.TabIndex = 3;
			// 
			// ParentAndChildCodeDescriptionBoolControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGridPanel);
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "ParentAndChildCodeDescriptionBoolControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			this.ChildGroupBox.ResumeLayout(false);
			this.ChildGridPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
