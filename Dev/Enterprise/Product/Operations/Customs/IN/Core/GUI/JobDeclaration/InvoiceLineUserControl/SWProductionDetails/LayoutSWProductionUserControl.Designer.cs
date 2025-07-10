using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IN.GUI;

partial class LayoutSWProductionUserControl
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
		this.SWProductionGrid = new Enterprise.ZArchitecture.ZGrid();
		this.SWProductionDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
		this.ksplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.SWProductionGrid)).BeginInit();
		this.SWProductionGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.ksplitContainer1)).BeginInit();
		this.ksplitContainer1.Panel1.SuspendLayout();
		this.ksplitContainer1.Panel2.SuspendLayout();
		this.ksplitContainer1.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.SWProduction);
		// 
		// SWProductionGrid
		// 
		this.SWProductionGrid.AllowDrop = true;
		this.SWProductionGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.SWProductionGrid, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IN.Business.SWProduction)(null)))));
		this.SWProductionGrid.CaptionVisible = false;
		this.SWProductionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SWProductionGrid.GridId = "088ADE76-B6DB-4EAB-9CFB-97E3EE287F0F";
		this.SWProductionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.SWProductionGrid.LayoutKey = "SWProductionGrid";
		this.SWProductionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.SWProductionGrid.Name = "SWProductionGrid";
		this.SWProductionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 43, true);
		this.SWProductionGrid.TabIndex = 0;
		// 
		// SWProductionDetailsPanel
		// 
		this.SWProductionDetailsPanel.AllowDrop = true;
		this.SWProductionDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SWProductionDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.SWProductionDetailsPanel.Name = "SWProductionDetailsPanel";
		this.SWProductionDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
		this.SWProductionDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 151, true);
		this.SWProductionDetailsPanel.TabIndex = 0;
		// 
		// ksplitContainer1
		// 
		this.ksplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ksplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.ksplitContainer1.Name = "ksplitContainer1";
		this.ksplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
		// 
		// ksplitContainer1.Panel1
		// 
		this.ksplitContainer1.Panel1.Controls.Add(this.SWProductionGrid);
		// 
		// ksplitContainer1.Panel2
		// 
		this.ksplitContainer1.Panel2.Controls.Add(this.SWProductionDetailsPanel);
		this.ksplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 198, true);
		this.ksplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(43);
		this.ksplitContainer1.TabIndex = 0;
		// 
		// LayoutSWProductionUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Controls.Add(this.ksplitContainer1);
		this.CaptionRenderingEnabled = true;
		this.Name = "LayoutSWProductionUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 198, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.SWProductionGrid)).EndInit();
		this.SWProductionGrid.ResumeLayout(false);
		this.SWProductionGrid.PerformLayout();
		this.ksplitContainer1.Panel1.ResumeLayout(false);
		this.ksplitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.ksplitContainer1)).EndInit();
		this.ksplitContainer1.ResumeLayout(false);
		this.ksplitContainer1.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal CargoWise.Windows.UI.KSplitContainer ksplitContainer1;
	internal ZArchitecture.ZGrid SWProductionGrid;
	internal ZArchitecture.GUI.DynamicLayoutPanel SWProductionDetailsPanel;
}
