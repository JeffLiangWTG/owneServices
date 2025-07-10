namespace Enterprise.Customs.IN.GUI;

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
			this.RBIWaiverNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RBIWaiverDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PackagesQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LoosePackagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalContainerZIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.ShippingBillOverrideUserControl = new Enterprise.Customs.IN.GUI.ShippingBillOverrideUserControl();
			this.TotalGrossWeightAndNetWeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WeightUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MessageAndCustomsStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RBIWaiverDateEdit.SuspendLayout();
			this.PackagesQtyCalcDropEdit.SuspendLayout();
			this.LoosePackagesCalcDropEdit.SuspendLayout();
			this.ShippingBillOverrideUserControl.SuspendLayout();
			this.TotalGrossWeightAndNetWeightGroupBox.SuspendLayout();
			this.WeightUQDropEdit.SuspendLayout();
			this.MessageAndCustomsStatusGroupBox.SuspendLayout();
			this.CustomsStatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.CusEntryInstruction);
			// 
			// RBIWaiverNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RBIWaiverNumberTextBox, "RBIWaiverNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).RBIWaiverNumber)));
			this.RBIWaiverNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 14, true);
			this.RBIWaiverNumberTextBox.Name = "RBIWaiverNumberTextBox";
			this.RBIWaiverNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.RBIWaiverNumberTextBox.TabIndex = 0;
			// 
			// RBIWaiverDateEdit
			// 
			this.RBIWaiverDateEdit.AllowDrop = true;
			this.RBIWaiverDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RBIWaiverDateEdit, "RBIWaiverDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).RBIWaiverDate)));
			this.RBIWaiverDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 46, true);
			this.RBIWaiverDateEdit.Name = "RBIWaiverDateEdit";
			this.RBIWaiverDateEdit.TabIndex = 1;
			// 
			// PackagesQtyCalcDropEdit
			// 
			this.PackagesQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagesQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).CEI_NumberOfPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).NumberOfPackagesUQ)));
			this.PackagesQtyCalcDropEdit.BindToAmount = "CEI_NumberOfPackages";
			this.PackagesQtyCalcDropEdit.BindToUnit = "NumberOfPackagesUQ";
			this.PackagesQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 103, true);
			this.PackagesQtyCalcDropEdit.MaxValue = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.PackagesQtyCalcDropEdit.Name = "PackagesQtyCalcDropEdit";
			this.PackagesQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.PackagesQtyCalcDropEdit.TabIndex = 2;
			this.PackagesQtyCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// LoosePackagesCalcDropEdit
			// 
			this.LoosePackagesCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoosePackagesCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).CEI_LoosePackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).LoosePackagesUQ)));
			this.LoosePackagesCalcDropEdit.BindToAmount = "CEI_LoosePackages";
			this.LoosePackagesCalcDropEdit.BindToUnit = "LoosePackagesUQ";
			this.LoosePackagesCalcDropEdit.Decimals = 0;
			this.LoosePackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 134, true);
			this.LoosePackagesCalcDropEdit.MaxValue = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.LoosePackagesCalcDropEdit.Name = "LoosePackagesCalcDropEdit";
			this.LoosePackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.LoosePackagesCalcDropEdit.TabIndex = 3;
			this.LoosePackagesCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TotalContainerZIntEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalContainerZIntEdit, "CEI_TotalContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).CEI_TotalContainer)));
			this.TotalContainerZIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 170, true);
			this.TotalContainerZIntEdit.Name = "TotalContainerZIntEdit";
			this.TotalContainerZIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.TotalContainerZIntEdit.TabIndex = 2;
			this.TotalContainerZIntEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShippingBillOverrideUserControl
			// 
			this.ShippingBillOverrideUserControl.AllowDrop = true;
			this.ShippingBillOverrideUserControl.AutoSize = true;
			this.ShippingBillOverrideUserControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ShippingBillOverrideUserControl, ".");
			this.ShippingBillOverrideUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 459, true);
			this.ShippingBillOverrideUserControl.Name = "ShippingBillOverrideUserControl";
			this.ShippingBillOverrideUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 57, true);
			this.ShippingBillOverrideUserControl.TabIndex = 0;
			// 
			// TotalGrossWeightAndNetWeightGroupBox
			// 
			this.TotalGrossWeightAndNetWeightGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("8cf3c70a-23e8-47cb-bf15-d8cbe1bf8a77", "Total Gross Weight && Net Weight");
			this.TotalGrossWeightAndNetWeightGroupBox.Controls.Add(this.WeightUQDropEdit);
			this.TotalGrossWeightAndNetWeightGroupBox.Controls.Add(this.GrossWeightCalcEdit);
			this.TotalGrossWeightAndNetWeightGroupBox.Controls.Add(this.NetWeightCalcEdit);
			this.TotalGrossWeightAndNetWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 213, true);
			this.TotalGrossWeightAndNetWeightGroupBox.Name = "TotalGrossWeightAndNetWeightGroupBox";
			this.TotalGrossWeightAndNetWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 109, true);
			this.TotalGrossWeightAndNetWeightGroupBox.TabIndex = 0;
			this.TotalGrossWeightAndNetWeightGroupBox.TabStop = false;
			// 
			// WeightUQDropEdit
			// 
			this.WeightUQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightUQDropEdit, "CEI_WeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).CEI_WeightUQ)));
			this.WeightUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 28, true);
			this.WeightUQDropEdit.Name = "WeightUQDropEdit";
			this.WeightUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.WeightUQDropEdit.TabIndex = 0;
			// 
			// GrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit, "GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).GrossWeight)));
			this.GrossWeightCalcEdit.DecimalPlaces = 3;
			this.GrossWeightCalcEdit.Decimals = 3;
			this.GrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 52, true);
			this.GrossWeightCalcEdit.Name = "GrossWeightCalcEdit";
			this.GrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.GrossWeightCalcEdit.TabIndex = 2;
			this.GrossWeightCalcEdit.Text = "0.000";
			this.GrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GrossWeightCalcEdit.TrackDisposedAccess = true;
			// 
			// NetWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NetWeightCalcEdit, "NetWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).NetWeight)));
			this.NetWeightCalcEdit.DecimalPlaces = 3;
			this.NetWeightCalcEdit.Decimals = 3;
			this.NetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 76, true);
			this.NetWeightCalcEdit.Name = "NetWeightCalcEdit";
			this.NetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.NetWeightCalcEdit.TabIndex = 3;
			this.NetWeightCalcEdit.Text = "0.000";
			this.NetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NetWeightCalcEdit.TrackDisposedAccess = true;
			// 
			// MessageAndCustomsStatusGroupBox
			// 
			this.MessageAndCustomsStatusGroupBox.BackColor = System.Drawing.Color.Transparent;
			this.MessageAndCustomsStatusGroupBox.Controls.Add(this.CustomsStatusDropEdit);
			this.MessageAndCustomsStatusGroupBox.Controls.Add(this.MessageStatusDropEdit);
			this.MessageAndCustomsStatusGroupBox.Controls.Add(this.OverrideCheckBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageAndCustomsStatusGroupBox, false);
			this.MessageAndCustomsStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 354, true);
			this.MessageAndCustomsStatusGroupBox.Name = "MessageAndCustomsStatusGroupBox";
			this.MessageAndCustomsStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 74, true);
			this.MessageAndCustomsStatusGroupBox.TabIndex = 0;
			this.MessageAndCustomsStatusGroupBox.TabStop = false;
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).CustomsStatus)));
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 45, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.PreBoundMaxLength = 3;
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustomsStatusDropEdit.TabIndex = 3;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).MessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.PreBoundMaxLength = 3;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.MessageStatusDropEdit.TabIndex = 2;
			// 
			// OverrideCheckBox
			// 
			this.OverrideCheckBox.AutoSize = true;
			this.OverrideCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.OverrideCheckBox, "StatusOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Business.CusEntryInstruction)(null)).StatusOverride)));
			this.OverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 0, true);
			this.OverrideCheckBox.Name = "OverrideCheckBox";
			this.OverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.OverrideCheckBox.TabIndex = 1;
			this.OverrideCheckBox.UseVisualStyleBackColor = false;
			// 
			// EntryInstructionDetailsLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TotalGrossWeightAndNetWeightGroupBox);
			this.Controls.Add(this.RBIWaiverDateEdit);
			this.Controls.Add(this.RBIWaiverNumberTextBox);
			this.Controls.Add(this.PackagesQtyCalcDropEdit);
			this.Controls.Add(this.LoosePackagesCalcDropEdit);
			this.Controls.Add(this.TotalContainerZIntEdit);
			this.Controls.Add(this.ShippingBillOverrideUserControl);
			this.Controls.Add(this.MessageAndCustomsStatusGroupBox);
			this.Name = "EntryInstructionDetailsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 526, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RBIWaiverDateEdit.ResumeLayout(true);
			this.RBIWaiverDateEdit.PerformLayout();
			this.PackagesQtyCalcDropEdit.ResumeLayout(true);
			this.PackagesQtyCalcDropEdit.PerformLayout();
			this.LoosePackagesCalcDropEdit.ResumeLayout(true);
			this.LoosePackagesCalcDropEdit.PerformLayout();
			this.ShippingBillOverrideUserControl.ResumeLayout(true);
			this.ShippingBillOverrideUserControl.PerformLayout();
			this.TotalGrossWeightAndNetWeightGroupBox.ResumeLayout(false);
			this.TotalGrossWeightAndNetWeightGroupBox.PerformLayout();
			this.WeightUQDropEdit.ResumeLayout(true);
			this.WeightUQDropEdit.PerformLayout();
			this.MessageAndCustomsStatusGroupBox.ResumeLayout(false);
			this.MessageAndCustomsStatusGroupBox.PerformLayout();
			this.CustomsStatusDropEdit.ResumeLayout(true);
			this.CustomsStatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.ZTextBox RBIWaiverNumberTextBox;
	internal ZArchitecture.GUI.ZDateEdit RBIWaiverDateEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit PackagesQtyCalcDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit LoosePackagesCalcDropEdit;
	internal ZArchitecture.GUI.ZIntEdit TotalContainerZIntEdit;
	internal Enterprise.Customs.IN.GUI.ShippingBillOverrideUserControl ShippingBillOverrideUserControl;

	internal ZArchitecture.GUI.ZGroupBox TotalGrossWeightAndNetWeightGroupBox;
	internal ZArchitecture.GUI.ZDropEdit WeightUQDropEdit;
	internal ZArchitecture.ZCalcEdit NetWeightCalcEdit;
	internal ZArchitecture.ZCalcEdit GrossWeightCalcEdit;
	internal ZArchitecture.GUI.ZGroupBox MessageAndCustomsStatusGroupBox;
	internal ZArchitecture.GUI.ZCheckBox OverrideCheckBox;
	internal ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
	internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
}
