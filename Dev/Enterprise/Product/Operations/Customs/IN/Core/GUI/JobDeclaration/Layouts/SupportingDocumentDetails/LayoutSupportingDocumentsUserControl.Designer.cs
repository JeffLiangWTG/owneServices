namespace Enterprise.Customs.IN.GUI;

partial class LayoutSupportingDocumentsUserControl
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
			this.SupportingDocumentGrid = new Enterprise.Customs.IN.GUI.SupportingDocumentGridControl();
			this.DetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.SupportingDocumentCollection);
			// 
			// SupportingDocumentGrid
			// 
			this.SupportingDocumentGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingDocumentGrid, ".");
			this.SupportingDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentGrid.Name = "SupportingDocumentGrid";
			this.SupportingDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 112, true);
			this.SupportingDocumentGrid.TabIndex = 0;
			// 
			// DetailsLayoutPanel
			// 
			this.DetailsLayoutPanel.AllowDrop = true;
			this.DetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsLayoutPanel.Name = "DetailsLayoutPanel";
			this.DetailsLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.DetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 316, true);
			this.DetailsLayoutPanel.TabIndex = 0;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.SupportingDocumentGrid);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.DetailsLayoutPanel);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 432, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(112);
			this.kSplitContainer1.TabIndex = 1;
			// 
			// LayoutSupportingDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.kSplitContainer1);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 7, 8, 7, true);
			this.Name = "LayoutSupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 432, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentGrid.ResumeLayout(true);
			this.SupportingDocumentGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private SupportingDocumentGridControl SupportingDocumentGrid;
	private ZArchitecture.GUI.DynamicLayoutPanel DetailsLayoutPanel;
	private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
}
