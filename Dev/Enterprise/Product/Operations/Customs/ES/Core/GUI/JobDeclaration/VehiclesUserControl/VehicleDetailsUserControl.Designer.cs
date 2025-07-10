namespace Enterprise.Customs.ES.GUI;

partial class VehicleDetailsUserControl
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
		this.VinTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.BrandTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.VinTextBox.SuspendLayout();
		this.BrandTextBox.SuspendLayout();
		this.ModelTextBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusVehicle);
		// 
		// VinTextBox
		// 
		this.BindingSource.SetBindingMember(this.VinTextBox, "CVH_VehicleIdentificationNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusVehicle)(null)).CVH_VehicleIdentificationNumber)));
		this.VinTextBox.CaptionResourceString = null;
		this.VinTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.VinTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 18, true);
		this.VinTextBox.Name = "VinTextBox";
		this.VinTextBox.ShouldEscapeAllSpecialCharacters = false;
		this.VinTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
		this.VinTextBox.TabIndex = 1;
		// 
		// BrandTextBox
		// 
		this.BindingSource.SetBindingMember(this.BrandTextBox, "CVH_BrandName");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusVehicle)(null)).CVH_BrandName)));
		this.BrandTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("A26D1200-B7F6-4692-A116-E0943A142B83", "Brand");
		this.BrandTextBox.CaptionResourceString = null;
		this.BrandTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.BrandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 47, true);
		this.BrandTextBox.Name = "BrandTextBox";
		this.BrandTextBox.ShouldEscapeAllSpecialCharacters = false;
		this.BrandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
		this.BrandTextBox.TabIndex = 2;
		// 
		// ModelTextBox
		// 
		this.BindingSource.SetBindingMember(this.ModelTextBox, "CVH_ModelName");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusVehicle)(null)).CVH_ModelName)));
		this.ModelTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("C06095D4-1C03-4EDE-AF4A-03D782D1FAE8", "Model");
		this.ModelTextBox.CaptionResourceString = null;
		this.ModelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 76, true);
		this.ModelTextBox.Name = "ModelTextBox";
		this.ModelTextBox.ShouldEscapeAllSpecialCharacters = false;
		this.ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
		this.ModelTextBox.TabIndex = 3;
		// 
		// VehicleDetailsUserControl
		//
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.VinTextBox);
		this.Controls.Add(this.BrandTextBox);
		this.Controls.Add(this.ModelTextBox);
		this.Name = "VehicleDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 389, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.VinTextBox.ResumeLayout(true);
		this.VinTextBox.PerformLayout();
		this.BrandTextBox.ResumeLayout(true);
		this.BrandTextBox.PerformLayout();
		this.ModelTextBox.ResumeLayout(true);
		this.ModelTextBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	public ZArchitecture.ZTextBox VinTextBox;
	public ZArchitecture.ZTextBox BrandTextBox;
	public ZArchitecture.ZTextBox ModelTextBox;
}
