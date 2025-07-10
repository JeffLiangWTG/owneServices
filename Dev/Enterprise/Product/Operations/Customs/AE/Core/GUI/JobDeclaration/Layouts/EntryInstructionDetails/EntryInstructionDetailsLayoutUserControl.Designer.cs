using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AE.GUI;

partial class EntryInstructionDetailsLayoutUserControl
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
			this.TradeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationPurposeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationPurposeDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseUserControl = new Enterprise.Customs.AE.GUI.ToWarehouseUserControl();
			this.ToWarehouseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FromWarehouseUserControl = new Enterprise.Customs.AE.GUI.FromWarehouseUserControl();
			this.FromWarehouseLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TradeTypeDropEdit.SuspendLayout();
			this.DeclarationPurposeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Business.CusEntryInstruction);
			// 
			// TradeTypeDropEdit
			// 
			this.TradeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeTypeDropEdit, "CEI_TradeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(null)).CEI_TradeType)));
			this.TradeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 88, true);
			this.TradeTypeDropEdit.Name = "TradeTypeDropEdit";
			this.TradeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 18, true);
			this.TradeTypeDropEdit.TabIndex = 0;
			// 
			// DeclarationPurposeDropEdit
			// 
			this.DeclarationPurposeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationPurposeDropEdit, "CEI_DeclarationPurpose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(null)).CEI_DeclarationPurpose)));
			this.DeclarationPurposeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 16, true);
			this.DeclarationPurposeDropEdit.Name = "DeclarationPurposeDropEdit";
			this.DeclarationPurposeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.DeclarationPurposeDropEdit.TabIndex = 2;
			// 
			// DeclarationPurposeDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationPurposeDetailsTextBox, "CEI_DeclarationPurposeDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(null)).CEI_DeclarationPurposeDetails)));
			this.DeclarationPurposeDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 42, true);
			this.DeclarationPurposeDetailsTextBox.Name = "DeclarationPurposeDetailsTextBox";
			this.DeclarationPurposeDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.DeclarationPurposeDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeclarationPurposeDetailsTextBox.TabIndex = 3;
			// 
			// ToWarehouseUserControl
			// 
			this.ToWarehouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseUserControl, ".");
			this.ToWarehouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 48, true);
			this.ToWarehouseUserControl.Name = "ToWarehouseUserControl";
			this.ToWarehouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 22, true);
			this.ToWarehouseUserControl.TabIndex = 2;
			// 
			// ToWarehouseLabel
			// 
			this.ToWarehouseLabel.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("084DBE3F-E3D0-4501-A9F1-5779CF31B200", "To Warehouse");
			this.ToWarehouseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ToWarehouseLabel.IsFontBold = true;
			this.ToWarehouseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(846, 402, true);
			this.ToWarehouseLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.ToWarehouseLabel.Name = "ToWarehouseLabel";
			this.ToWarehouseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ToWarehouseLabel.TabIndex = 5;
			this.ToWarehouseLabel.UseMnemonic = false;
			// 
			// FromWarehouseUserControl
			// 
			this.FromWarehouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseUserControl, ".");
			this.FromWarehouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 68, true);
			this.FromWarehouseUserControl.Name = "FromWarehouseUserControl";
			this.FromWarehouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 22, true);
			this.FromWarehouseUserControl.TabIndex = 7;
			// 
			// FromWarehouseLabel
			// 
			this.FromWarehouseLabel.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("3DD8600D-EC57-4CBE-9DAC-BED22DA51618", "From Warehouse");
			this.FromWarehouseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FromWarehouseLabel.IsFontBold = true;
			this.FromWarehouseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 438, true);
			this.FromWarehouseLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.FromWarehouseLabel.Name = "FromWarehouseLabel";
			this.FromWarehouseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.FromWarehouseLabel.TabIndex = 6;
			this.FromWarehouseLabel.UseMnemonic = false;
			// 
			// EntryInstructionDetailsLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TradeTypeDropEdit);
			this.Controls.Add(this.DeclarationPurposeDropEdit);
			this.Controls.Add(this.DeclarationPurposeDetailsTextBox);
			this.Controls.Add(this.ToWarehouseLabel);
			this.Controls.Add(this.ToWarehouseUserControl);
			this.Controls.Add(this.FromWarehouseLabel);
			this.Controls.Add(this.FromWarehouseUserControl);
			this.Name = "EntryInstructionDetailsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 174, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TradeTypeDropEdit.ResumeLayout(true);
			this.TradeTypeDropEdit.PerformLayout();
			this.DeclarationPurposeDropEdit.ResumeLayout(true);
			this.DeclarationPurposeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal Enterprise.ZArchitecture.GUI.ZDropEdit TradeTypeDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit DeclarationPurposeDropEdit;
	internal Enterprise.ZArchitecture.ZTextBox DeclarationPurposeDetailsTextBox;
	internal FromWarehouseUserControl FromWarehouseUserControl;
	internal ToWarehouseUserControl ToWarehouseUserControl;
	internal ZLabel FromWarehouseLabel;
	internal ZLabel ToWarehouseLabel;
}
