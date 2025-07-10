namespace Enterprise.Customs.IN.GUI;

partial class OrganisationConsignorPlugInUserControl
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
		this.ExportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.ExportTypeDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.INOrgImpAddInfo);
		// 
		// ExportTypeDropEdit
		// 
		this.ExportTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ExportTypeDropEdit, "ZO_TypeOfExporter");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.INOrgImpAddInfo)(null)).ZO_TypeOfExporter)));
		this.ExportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 18, true);
		this.ExportTypeDropEdit.Name = "ExportTypeDropEdit";
		this.ExportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
		this.ExportTypeDropEdit.TabIndex = 0;
		// 
		// OrganisationConsignorPlugInUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.ExportTypeDropEdit);
		this.Name = "OrganisationConsignorPlugInUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 123, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ExportTypeDropEdit.ResumeLayout(true);
		this.ExportTypeDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private ZArchitecture.GUI.ZDropEdit ExportTypeDropEdit;
}
