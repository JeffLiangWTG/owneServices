namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

partial class CustodianEORIBranchUserControl
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
		this.CustodianEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.CustodianBranchTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.CustodianEORITextBox.SuspendLayout();
		this.CustodianBranchTextBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader);
		// 
		// CustodianEORITextBox
		// 
		this.BindingSource.SetBindingMember(this.CustodianEORITextBox, "CusTempStorageRegLines.SRL_CustodianIdentifier");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustodianIdentifier)));
		this.CustodianEORITextBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("82466566-d8b4-474e-bd66-a3037062145c", "Custodian EORI/Branch");
		this.CustodianEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.CustodianEORITextBox.Name = "CustodianEORITextBox";
		this.CustodianEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
		this.CustodianEORITextBox.TabIndex = 0;
		// 
		// CustodianBranchTextBox
		// 
		this.BindingSource.SetBindingMember(this.CustodianBranchTextBox, "CusTempStorageRegLines.SRL_CustodianIdentifierBranchNo");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustodianIdentifierBranchNo)));
		this.CustodianBranchTextBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("1019924d-1391-40bc-b65e-2d4b39d6b176", "Branch");
		this.CustodianBranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 0, true);
		this.CustodianBranchTextBox.Name = "CustodianBranchTextBox";
		this.CustodianBranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 18, true);
		this.CustodianBranchTextBox.TabIndex = 1;
		// 
		// CustodianEORIBranchUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.CustodianEORITextBox);
		this.Controls.Add(this.CustodianBranchTextBox);
		this.Name = "CustodianEORIBranchUserControl";
		this.ShouldSerializeTabPageMethods = true;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 15, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.CustodianEORITextBox.ResumeLayout(true);
		this.CustodianEORITextBox.PerformLayout();
		this.CustodianBranchTextBox.ResumeLayout(true);
		this.CustodianBranchTextBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	internal ZArchitecture.ZTextBox CustodianEORITextBox;
	internal ZArchitecture.ZTextBox CustodianBranchTextBox;
}
