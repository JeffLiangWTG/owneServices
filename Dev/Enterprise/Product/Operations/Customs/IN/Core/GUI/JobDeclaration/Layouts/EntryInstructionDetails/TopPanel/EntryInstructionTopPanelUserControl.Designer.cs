namespace Enterprise.Customs.IN.GUI;

partial class EntryInstructionTopPanelUserControl
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
			this.EntryInstructionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessageAndCustomsStatusWithOverridePanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionGrid)).BeginInit();
			this.EntryInstructionGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobDeclaration);
			// 
			// EntryInstructionGrid
			// 
			this.EntryInstructionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).CustomsEntryInstructions)));
			this.EntryInstructionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionGrid.GridId = "ad340f7f-fd6b-4196-998c-ee4c975da660";
			this.EntryInstructionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionGrid.Name = "EntryInstructionGrid";
			this.EntryInstructionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 176, true);
			this.EntryInstructionGrid.TabIndex = 0;
			// 
			// MessageAndCustomsStatusWithOverridePanel
			//
			this.BindingSource.SetBindingMember(this.MessageAndCustomsStatusWithOverridePanel, "CustomsEntryInstructions");
			this.MessageAndCustomsStatusWithOverridePanel.AllowDrop = true;
			this.MessageAndCustomsStatusWithOverridePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageAndCustomsStatusWithOverridePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageAndCustomsStatusWithOverridePanel.Name = "MessageAndCustomsStatusWithOverridePanel";
			this.MessageAndCustomsStatusWithOverridePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.MessageAndCustomsStatusWithOverridePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 176, true);
			this.MessageAndCustomsStatusWithOverridePanel.TabIndex = 0;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.EntryInstructionGrid);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.MessageAndCustomsStatusWithOverridePanel);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1070, 176, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(730);
			this.kSplitContainer1.SplitterWidth = 5;
			this.kSplitContainer1.TabIndex = 0;
			// 
			// EntryInstructionTopPanelUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Controls.Add(this.kSplitContainer1);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 7, 8, 7, true);
			this.Name = "EntryInstructionTopPanelUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1141, 176, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionGrid)).EndInit();
			this.EntryInstructionGrid.ResumeLayout(false);
			this.EntryInstructionGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private ZArchitecture.GUI.DynamicLayoutPanel MessageAndCustomsStatusWithOverridePanel;
	private ZArchitecture.ZGrid EntryInstructionGrid;
	private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
}
