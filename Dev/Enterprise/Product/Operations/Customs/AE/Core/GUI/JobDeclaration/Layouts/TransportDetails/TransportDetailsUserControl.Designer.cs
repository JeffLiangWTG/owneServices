using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI;

partial class TransportDetailsUserControl
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
		this.PlaceOfDischargeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.PlaceOfDischargeDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(JobDeclaration);
		// 
		// PlaceOfDischargeDropEdit
		// 
		this.BindingSource.SetBindingMember(this.PlaceOfDischargeDropEdit, "JE_PlaceOfDischarge");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).JE_PlaceOfDischarge)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).Lookups.PlaceOfDischargeList)));
		this.PlaceOfDischargeDropEdit.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("AEJobDeclarationUserControl|d8de130f-777e-4c5a-a31d-badc05612ac9", "Place Of Discharge");
		this.PlaceOfDischargeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
		this.PlaceOfDischargeDropEdit.Name = "PlaceOfDischargeDropEdit";
		this.PlaceOfDischargeDropEdit.PreBoundMaxLength = 1;
		this.PlaceOfDischargeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
		// 
		// TransportDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.PlaceOfDischargeDropEdit);
		this.Name = "TransportDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.PlaceOfDischargeDropEdit.ResumeLayout(true);
		this.PlaceOfDischargeDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	internal Enterprise.ZArchitecture.GUI.ZDropEdit PlaceOfDischargeDropEdit;
}
