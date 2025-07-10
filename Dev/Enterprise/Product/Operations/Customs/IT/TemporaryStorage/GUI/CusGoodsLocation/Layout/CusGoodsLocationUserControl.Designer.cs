namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

partial class CusGoodsLocationUserControl
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
			this.AdditionalIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalIdentifierDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.TemporaryStorage.Business.CusGoodsLocation);
			// 
			// AdditionalIdentifierDropEdit
			// 
			this.AdditionalIdentifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalIdentifierDropEdit, "AdditionalIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.TemporaryStorage.Business.CusGoodsLocation)(null)).AdditionalIdentifier)));
			this.AdditionalIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalIdentifierDropEdit.Name = "AdditionalIdentifierDropEdit";
			this.AdditionalIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 26, true);
			this.AdditionalIdentifierDropEdit.TabIndex = 0;
			this.AdditionalIdentifierDropEdit.UseFullWidthForCodeBox = true;
			// 
			// CusGoodsLocationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AdditionalIdentifierDropEdit);
			this.Name = "CusGoodsLocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 210, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalIdentifierDropEdit.ResumeLayout(true);
			this.AdditionalIdentifierDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZDropEdit AdditionalIdentifierDropEdit;
}
