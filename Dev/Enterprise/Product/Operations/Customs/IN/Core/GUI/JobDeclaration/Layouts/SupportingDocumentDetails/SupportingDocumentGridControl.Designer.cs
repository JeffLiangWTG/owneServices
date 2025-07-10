namespace Enterprise.Customs.IN.GUI;

partial class SupportingDocumentGridControl
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
		this.SupportingDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentGrid)).BeginInit();
		this.SupportingDocumentGrid.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.SupportingDocumentCollection);
		// 
		// SupportingDocumentGrid
		// 
		this.SupportingDocumentGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.SupportingDocumentGrid, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IN.Business.SupportingDocumentCollection)(null)))));
		this.SupportingDocumentGrid.CaptionVisible = false;
		this.SupportingDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SupportingDocumentGrid.GridId = "D3E87D4E-E0F4-4B22-AAE4-6654839CBCD0";
		this.SupportingDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.SupportingDocumentGrid.LayoutKey = "SupportingDocumentGrid";
		this.SupportingDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.SupportingDocumentGrid.Name = "SupportingDocumentGrid";
		this.SupportingDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 175, true);
		this.SupportingDocumentGrid.TabIndex = 0;
		// 
		// SupportingDocumentGridControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.SupportingDocumentGrid);
		this.Name = "SupportingDocumentGridControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 175, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentGrid)).EndInit();
		this.SupportingDocumentGrid.ResumeLayout(false);
		this.SupportingDocumentGrid.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.ZGrid SupportingDocumentGrid;
}
