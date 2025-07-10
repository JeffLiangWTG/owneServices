namespace Enterprise.Customs.IN.GUI;

partial class DutyFreeImportAuthorizationUserControl
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
		this.ExportItemDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
		this.ImportItemDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
		this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
		this.ExportItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.ImportItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ExportItemDetailsGrid)).BeginInit();
		this.ExportItemDetailsGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.ImportItemDetailsGrid)).BeginInit();
		this.ImportItemDetailsGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
		this.kSplitContainer1.Panel1.SuspendLayout();
		this.kSplitContainer1.Panel2.SuspendLayout();
		this.kSplitContainer1.SuspendLayout();
		this.ExportItemDetailsGroupBox.SuspendLayout();
		this.ImportItemDetailsGroupBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.DfiaExportItemDetail);
		// 
		// ExportItemDetailsGrid
		// 
		this.ExportItemDetailsGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.ExportItemDetailsGrid, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IN.Business.DfiaExportItemDetail)(null)))));
		this.ExportItemDetailsGrid.CaptionVisible = false;
		this.ExportItemDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ExportItemDetailsGrid.GridId = "000b5744-7725-412a-adba-f843a09b913f";
		this.ExportItemDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.ExportItemDetailsGrid.LayoutKey = "ExportItemDetailsGrid";
		this.ExportItemDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
		this.ExportItemDetailsGrid.Name = "ExportItemDetailsGrid";
		this.ExportItemDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 335, true);
		this.ExportItemDetailsGrid.TabIndex = 0;
		// 
		// ImportItemDetailsGrid
		// 
		this.ImportItemDetailsGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.ImportItemDetailsGrid, "DfiaImportItemDetails");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IN.Business.DfiaExportItemDetail)(null)).DfiaImportItemDetails)));
		this.ImportItemDetailsGrid.CaptionVisible = false;
		this.ImportItemDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ImportItemDetailsGrid.GridId = "7dc83207-88a6-48e1-b8d9-d894c654fa4d";
		this.ImportItemDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.ImportItemDetailsGrid.LayoutKey = "zGrid1";
		this.ImportItemDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
		this.ImportItemDetailsGrid.Name = "ImportItemDetailsGrid";
		this.ImportItemDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 335, true);
		this.ImportItemDetailsGrid.TabIndex = 1;
		// 
		// kSplitContainer1
		// 
		this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.kSplitContainer1.Name = "kSplitContainer1";
		// 
		// kSplitContainer1.Panel1
		// 
		this.kSplitContainer1.Panel1.Controls.Add(this.ExportItemDetailsGroupBox);
		// 
		// kSplitContainer1.Panel2
		// 
		this.kSplitContainer1.Panel2.Controls.Add(this.ImportItemDetailsGroupBox);
		this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 354, true);
		this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
		this.kSplitContainer1.TabIndex = 2;
		// 
		// ExportItemDetailsGroupBox
		// 
		this.ExportItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("a0f25b22-6523-48ad-8e6c-15b9dc75e60e", "Export Item Details");
		this.ExportItemDetailsGroupBox.Controls.Add(this.ExportItemDetailsGrid);
		this.ExportItemDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ExportItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.ExportItemDetailsGroupBox.Name = "ExportItemDetailsGroupBox";
		this.ExportItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 354, true);
		this.ExportItemDetailsGroupBox.TabIndex = 1;
		this.ExportItemDetailsGroupBox.TabStop = false;
		// 
		// ImportItemDetailsGroupBox
		// 
		this.ImportItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("f7c3dc8a-ea65-48f2-8962-5192d968e904", "Import Item Details");
		this.ImportItemDetailsGroupBox.Controls.Add(this.ImportItemDetailsGrid);
		this.ImportItemDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ImportItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.ImportItemDetailsGroupBox.Name = "ImportItemDetailsGroupBox";
		this.ImportItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 354, true);
		this.ImportItemDetailsGroupBox.TabIndex = 2;
		this.ImportItemDetailsGroupBox.TabStop = false;
		// 
		// DutyFreeImportAuthorizationUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.kSplitContainer1);
		this.Name = "DutyFreeImportAuthorizationUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 354, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ExportItemDetailsGrid)).EndInit();
		this.ExportItemDetailsGrid.ResumeLayout(false);
		this.ExportItemDetailsGrid.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.ImportItemDetailsGrid)).EndInit();
		this.ImportItemDetailsGrid.ResumeLayout(false);
		this.ImportItemDetailsGrid.PerformLayout();
		this.kSplitContainer1.Panel1.ResumeLayout(false);
		this.kSplitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
		this.kSplitContainer1.ResumeLayout(false);
		this.kSplitContainer1.PerformLayout();
		this.ExportItemDetailsGroupBox.ResumeLayout(false);
		this.ExportItemDetailsGroupBox.PerformLayout();
		this.ImportItemDetailsGroupBox.ResumeLayout(false);
		this.ImportItemDetailsGroupBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private ZArchitecture.ZGrid ExportItemDetailsGrid;
	private ZArchitecture.ZGrid ImportItemDetailsGrid;
	private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
	private ZArchitecture.GUI.ZGroupBox ExportItemDetailsGroupBox;
	private ZArchitecture.GUI.ZGroupBox ImportItemDetailsGroupBox;
}
