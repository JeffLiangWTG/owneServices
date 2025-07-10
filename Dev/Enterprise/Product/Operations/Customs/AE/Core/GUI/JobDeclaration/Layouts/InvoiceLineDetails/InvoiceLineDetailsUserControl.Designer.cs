namespace Enterprise.Customs.AE.GUI;

partial class InvoiceLineDetailsUserControl
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
		this.GoodsConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.GoodsConditionDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Business.JobComInvoiceLine);
		// 
		// GoodsConditionDropEdit
		// 
		this.GoodsConditionDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.GoodsConditionDropEdit, "JI_NewUsed");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.JobComInvoiceLine)(null)).JI_NewUsed)));
		this.GoodsConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 23, true);
		this.GoodsConditionDropEdit.Name = "GoodsConditionDropEdit";
		this.GoodsConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
		this.GoodsConditionDropEdit.TabIndex = 0;
		// 
		// InvoiceLineDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Controls.Add(this.GoodsConditionDropEdit);
		this.Name = "InvoiceLineDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 100, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.GoodsConditionDropEdit.ResumeLayout(true);
		this.GoodsConditionDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZDropEdit GoodsConditionDropEdit;
}
