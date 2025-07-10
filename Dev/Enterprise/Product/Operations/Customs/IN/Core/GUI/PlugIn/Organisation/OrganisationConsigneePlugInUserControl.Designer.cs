namespace Enterprise.Customs.IN.GUI;

partial class OrganisationConsigneePlugInUserControl
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
		this.ImportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.ImportTypeDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.INOrgImpAddInfo);
		// 
		// ImportTypeDropEdit
		// 
		this.ImportTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ImportTypeDropEdit, "ZO_TypeOfImporter");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.INOrgImpAddInfo)(null)).ZO_TypeOfImporter)));
		this.ImportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 18, true);
		this.ImportTypeDropEdit.Name = "ImportTypeDropEdit";
		this.ImportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 15, true);
		this.ImportTypeDropEdit.TabIndex = 0;
		// 
		// OrganisationConsignorPlugInUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.ImportTypeDropEdit);
		this.Name = "OrganisationConsigneePlugInUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 123, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ImportTypeDropEdit.ResumeLayout(true);
		this.ImportTypeDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private ZArchitecture.GUI.ZDropEdit ImportTypeDropEdit;
}
