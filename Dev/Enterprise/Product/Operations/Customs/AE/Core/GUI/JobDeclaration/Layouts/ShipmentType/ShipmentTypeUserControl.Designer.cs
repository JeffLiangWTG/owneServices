using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI;

partial class ShipmentTypeUserControl
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
		this.ExitPointDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.ClearanceLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.ExitPointDropEdit.SuspendLayout();
		this.ClearanceLocationDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(JobDeclaration);
		// 
		// ExitPointDropEdit
		//
		this.ExitPointDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ExitPointDropEdit, "JE_ExitPoint");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).JE_ExitPoint)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).Lookups.ExitPointList)));
		this.ExitPointDropEdit.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("AEJobDeclarationUserControl|fe43d322-df89-4c01-b170-01d7c0311167", "Exit Point");
		this.ExitPointDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 88, true);
		this.ExitPointDropEdit.Name = "ExitPointDropEdit";
		this.ExitPointDropEdit.PreBoundMaxLength = 3;
		this.ExitPointDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
		this.ExitPointDropEdit.TabIndex = 7;
		// 
		// ClearanceLocationDropEdit
		//
		this.ClearanceLocationDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ClearanceLocationDropEdit, "JE_ClearanceLocation");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).JE_ClearanceLocation)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).Lookups.ClearanceLocationList)));
		this.ClearanceLocationDropEdit.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("AEJobDeclarationUserControl|7dbb1813-1774-42c5-8e36-28f4084e8273", "Clear. Loc.", "Clearance Loc.", "Clearance Location", "");
		this.ClearanceLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 112, true);
		this.ClearanceLocationDropEdit.Name = "ClearanceLocationDropEdit";
		this.ClearanceLocationDropEdit.PreBoundMaxLength = 3;
		this.ClearanceLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
		this.ClearanceLocationDropEdit.TabIndex = 11;
		// 
		// ShipmentTypeLayoutUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.ExitPointDropEdit);
		this.Controls.Add(this.ClearanceLocationDropEdit);
		this.Name = "ShipmentTypeLayoutUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ExitPointDropEdit.ResumeLayout(true);
		this.ExitPointDropEdit.PerformLayout();
		this.ClearanceLocationDropEdit.ResumeLayout(true);
		this.ClearanceLocationDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	internal Enterprise.ZArchitecture.GUI.ZDropEdit ExitPointDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit ClearanceLocationDropEdit;
}
