namespace Enterprise.Customs.ES.GUI;

partial class ImportSupportingDocumentsUserControl
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
		this.SupportingDocumentsFieldsControl.SuspendLayout();
		this.BottomPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
		this.SupportingDocumentsGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// SupportingDocumentsFieldsControl
		// 
		this.SupportingDocumentsFieldsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 260, true);
		this.SupportingDocumentsFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 260, true);
		// 
		// SupportingDocumentsSplitter
		// 
		this.SupportingDocumentsSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.SupportingDocumentsSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
		// 
		// BottomPanel
		// 
		this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 161, true);
		this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 260, true);
		this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 260, true);
		// 
		// gridSplitter
		// 
		this.gridSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.gridSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.gridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
		this.gridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 3, true);
		// 
		// SupportingDocumentsGrid
		// 
		this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 158, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
		// 
		// ImportSupportingDocumentsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Name = "ImportSupportingDocumentsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 382, true);
		this.SupportingDocumentsFieldsControl.ResumeLayout(true);
		this.SupportingDocumentsFieldsControl.PerformLayout();
		this.BottomPanel.ResumeLayout(false);
		this.BottomPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
		this.SupportingDocumentsGrid.ResumeLayout(false);
		this.SupportingDocumentsGrid.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion
}
