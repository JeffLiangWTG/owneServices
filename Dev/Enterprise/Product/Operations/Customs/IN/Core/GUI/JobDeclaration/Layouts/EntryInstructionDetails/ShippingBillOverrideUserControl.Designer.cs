using System.ComponentModel;

namespace Enterprise.Customs.IN.GUI;

partial class ShippingBillOverrideUserControl
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private IContainer components = null;

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
		this.ShippingBillGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.SBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.SBDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
		this.OverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.ShippingBillGroupBox.SuspendLayout();
		this.SBDateDateEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.CusEntryInstruction);
		// 
		// ShippingBillGroupBox
		// 
		this.ShippingBillGroupBox.BackColor = System.Drawing.Color.Transparent;
		this.ShippingBillGroupBox.Controls.Add(this.SBNumberTextBox);
		this.ShippingBillGroupBox.Controls.Add(this.SBDateDateEdit);
		this.ShippingBillGroupBox.Controls.Add(this.OverrideCheckBox);
		this.ShippingBillGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShippingBillGroupBox, false);
		this.ShippingBillGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.ShippingBillGroupBox.Name = "ShippingBillGroupBox";
		this.ShippingBillGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 49, true);
		this.ShippingBillGroupBox.TabIndex = 0;
		this.ShippingBillGroupBox.TabStop = false;
		// 
		// SBNumberTextBox
		// 
		this.BindingSource.SetBindingMember(this.SBNumberTextBox, "ShippingBillNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).ShippingBillNumber)));
		this.SBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 19, true);
		this.SBNumberTextBox.Name = "SBNumberTextBox";
		this.SBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
		this.SBNumberTextBox.TabIndex = 2;
		// 
		// SBDateDateEdit
		// 
		this.SBDateDateEdit.AllowDrop = true;
		this.SBDateDateEdit.AutoCompleteMonthThreshold = 1;
		this.BindingSource.SetBindingMember(this.SBDateDateEdit, "ShippingBillDate");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).ShippingBillDate)));
		this.SBDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 19, true);
		this.SBDateDateEdit.Name = "SBDateDateEdit";
		this.SBDateDateEdit.TabIndex = 3;
		// 
		// OverrideCheckBox
		// 
		this.OverrideCheckBox.AutoSize = true;
		this.OverrideCheckBox.BackColor = System.Drawing.SystemColors.Control;
		this.BindingSource.SetBindingMember(this.OverrideCheckBox, "ShippingBillNumberOverride");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).ShippingBillNumberOverride)));
		this.OverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 0, true);
		this.OverrideCheckBox.Name = "OverrideCheckBox";
		this.OverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
		this.OverrideCheckBox.TabIndex = 1;
		this.OverrideCheckBox.UseVisualStyleBackColor = false;
		// 
		// ShippingBillOverrideUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.SystemColors.Control;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.ShippingBillGroupBox);
		this.Name = "ShippingBillOverrideUserControl";
		this.ShouldSerializeTabPageMethods = false;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 49, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ShippingBillGroupBox.ResumeLayout(false);
		this.ShippingBillGroupBox.PerformLayout();
		this.SBDateDateEdit.ResumeLayout(true);
		this.SBDateDateEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	internal ZArchitecture.GUI.ZGroupBox ShippingBillGroupBox;
	internal ZArchitecture.GUI.ZCheckBox OverrideCheckBox;
	internal ZArchitecture.ZTextBox SBNumberTextBox;
	internal ZArchitecture.GUI.ZDateEdit  SBDateDateEdit;

	#endregion
}

