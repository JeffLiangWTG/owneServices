using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI;

partial class ShipmentDetailsUserControl
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
		this.TypeOfGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.OperationalStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.TypeOfGoodsDropEdit.SuspendLayout();
		this.OperationalStatusDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(JobDeclaration);
		// 
		// TypeOfGoodsDropEdit
		// 
		this.BindingSource.SetBindingMember(this.TypeOfGoodsDropEdit, "JE_TypeOfGoods");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).JE_TypeOfGoods)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).Lookups.TypeOfGoodsList)));
		this.TypeOfGoodsDropEdit.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("AEJobDeclarationUserControl|d7aac746-33da-4344-9c5c-ff9e051eb28d", "Type Of Goods");
		this.TypeOfGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 184, true);
		this.TypeOfGoodsDropEdit.Name = "TypeOfGoodsDropEdit";
		this.TypeOfGoodsDropEdit.PreBoundMaxLength = 1;
		this.TypeOfGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
		this.TypeOfGoodsDropEdit.TabIndex = 9;
		//
		// OperationalStatusDropEdit
		// 
		this.BindingSource.SetBindingMember(this.OperationalStatusDropEdit, "JE_OperationalStatus");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).JE_OperationalStatus)));
		this.OperationalStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 281, true);
		this.OperationalStatusDropEdit.Name = "OperationalStatusDropEdit";
		this.OperationalStatusDropEdit.PreBoundMaxLength = 3;
		this.OperationalStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
		this.OperationalStatusDropEdit.TabIndex = 27;
		// 
		// ShipmentDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.TypeOfGoodsDropEdit);
		this.Controls.Add(this.OperationalStatusDropEdit);
		this.Name = "ShipmentDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.TypeOfGoodsDropEdit.ResumeLayout(true);
		this.TypeOfGoodsDropEdit.PerformLayout();
		this.OperationalStatusDropEdit.ResumeLayout(true);
		this.OperationalStatusDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}
	#endregion

	internal Enterprise.ZArchitecture.GUI.ZDropEdit TypeOfGoodsDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit OperationalStatusDropEdit;
}
