namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

partial class DisposalEntitledTraderEORIBranchUserControl
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
		this.DisposalEntitledTraderEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.DisposalEntitledTraderBranchTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.DisposalEntitledTraderEORITextBox.SuspendLayout();
		this.DisposalEntitledTraderBranchTextBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader);
		// 
		// DisposalEntitledTraderEORITextBox
		// 
		this.BindingSource.SetBindingMember(this.DisposalEntitledTraderEORITextBox, "CusTempStorageRegLines.SRL_GoodsOwnerIdentifier");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsOwnerIdentifier)));
		this.DisposalEntitledTraderEORITextBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("e55ab440-f330-4d0d-8404-adf15665ebaa", "Disp. Ent. Trader EORI/Branch");
		this.DisposalEntitledTraderEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.DisposalEntitledTraderEORITextBox.Name = "DisposalEntitledTraderEORITextBox";
		this.DisposalEntitledTraderEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
		this.DisposalEntitledTraderEORITextBox.TabIndex = 0;
		// 
		// DisposalEntitledTraderBranchTextBox
		// 
		this.BindingSource.SetBindingMember(this.DisposalEntitledTraderBranchTextBox, "CusTempStorageRegLines.SRL_GoodsOwnerIdentifierBranchNo");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsOwnerIdentifierBranchNo)));
		this.DisposalEntitledTraderBranchTextBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("fdfa240b-8562-477f-bc5e-3f177ee8edd0", "Branch");
		this.DisposalEntitledTraderBranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 0, true);
		this.DisposalEntitledTraderBranchTextBox.Name = "DisposalEntitledTraderBranchTextBox";
		this.DisposalEntitledTraderBranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 18, true);
		this.DisposalEntitledTraderBranchTextBox.TabIndex = 1;
		// 
		// DisposalEntitledTraderEORIBranchUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.DisposalEntitledTraderEORITextBox);
		this.Controls.Add(this.DisposalEntitledTraderBranchTextBox);
		this.Name = "DisposalEntitledTraderEORIBranchUserControl";
		this.ShouldSerializeTabPageMethods = true;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 15, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.DisposalEntitledTraderBranchTextBox.ResumeLayout(true);
		this.DisposalEntitledTraderBranchTextBox.PerformLayout();
		this.DisposalEntitledTraderEORITextBox.ResumeLayout(true);
		this.DisposalEntitledTraderEORITextBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	internal ZArchitecture.ZTextBox DisposalEntitledTraderEORITextBox;
	internal ZArchitecture.ZTextBox DisposalEntitledTraderBranchTextBox;
}
