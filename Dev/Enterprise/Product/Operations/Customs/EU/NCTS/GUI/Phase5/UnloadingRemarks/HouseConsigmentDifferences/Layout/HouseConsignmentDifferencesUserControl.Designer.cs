using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentDifferencesUserControl
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
			this.SequenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecurityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HouseConsignmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnloadedStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DeclaredValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnloadedValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GrossWeightUnloadedCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ArrivalTransportInfosUserControl = new Enterprise.Customs.EU.NCTS.GUI.ArrivalTransportInfosUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnloadedStateDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.ConsignorDocAddressControl.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.GrossWeightUnloadedCalcDropEdit.SuspendLayout();
			this.ArrivalTransportInfosUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsBill);
			// 
			// SequenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SequenceNumberTextBox, "MovementDetail+B9_SeqNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).MovementDetail.B9_SeqNo)));
			this.SequenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 13, true);
			this.SequenceNumberTextBox.Name = "SequenceNumberTextBox";
			this.SequenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 35, true);
			this.SequenceNumberTextBox.TabIndex = 0;
			// 
			// SecurityCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SecurityCheckBox, "B0_SecurityIndicatorFromExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_SecurityIndicatorFromExport)));
			this.SecurityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 39, true);
			this.SecurityCheckBox.Name = "SecurityCheckBox";
			this.SecurityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.SecurityCheckBox.TabIndex = 1;
			this.SecurityCheckBox.UseVisualStyleBackColor = true;
			// 
			// HouseConsignmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.HouseConsignmentTextBox, "B0_ReferenceID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_ReferenceID)));
			this.HouseConsignmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 71, true);
			this.HouseConsignmentTextBox.Name = "HouseConsignmentTextBox";
			this.HouseConsignmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 35, true);
			this.HouseConsignmentTextBox.TabIndex = 2;
			// 
			// UnloadedStateDropEdit
			// 
			this.UnloadedStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedStateDropEdit, "MovementDetail+B9_UnloadedState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).MovementDetail.B9_UnloadedState)));
			this.UnloadedStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 98, true);
			this.UnloadedStateDropEdit.Name = "UnloadedStateDropEdit";
			this.UnloadedStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 35, true);
			this.UnloadedStateDropEdit.TabIndex = 3;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).Lookups.WeightUnitList)));
			this.GrossWeightCalcDropEdit.BindToAmount = "B0_Weight";
			this.GrossWeightCalcDropEdit.BindToList = "Lookups.WeightUnitList";
			this.GrossWeightCalcDropEdit.BindToUnit = "B0_WeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 125, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 35, true);
			this.GrossWeightCalcDropEdit.TabIndex = 4;
			// 
			// ConsignorDocAddressControl
			// 
			this.ConsignorDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ConsignorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorDocAddressControl, "MovementDetail.ConsignorDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).MovementDetail.ConsignorDocAddress)));
			this.ConsignorDocAddressControl.BindToOrganisations = "Lookups.Consignors";
			this.ConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("11A0AB3C-E7F2-40E1-88A7-ADECFE32ED6F", "Consignor");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsignorDocAddressControl, false);
			this.ConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 224, true);
			this.ConsignorDocAddressControl.Name = "ConsignorDocAddressControl";
			this.ConsignorDocAddressControl.ReadOnly = false;
			this.ConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsignorDocAddressControl.TabIndex = 7;
			this.ConsignorDocAddressControl.ValidationJustForced = false;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "MovementDetail.ConsigneeDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).MovementDetail.ConsigneeDocAddress)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignees";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("C4AD4AFE-065F-48CF-9EC3-37ADD15F9959", "Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeDocAddressControl, false);
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 224, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 8;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// DeclaredValueLabel
			// 
			this.DeclaredValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeclaredValueLabel, false);
			this.DeclaredValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 445, true);
			this.DeclaredValueLabel.Name = "DeclaredValueLabel";
			this.DeclaredValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DeclaredValueLabel.TabIndex = 9;
			this.DeclaredValueLabel.UseMnemonic = false;
			// 
			// UnloadedValueLabel
			// 
			this.UnloadedValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d294ad59-dce5-493f-9f9b-2bfb50720a44", "Unloaded Value");
			this.UnloadedValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnloadedValueLabel, false);
			this.UnloadedValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 445, true);
			this.UnloadedValueLabel.Name = "UnloadedValueLabel";
			this.UnloadedValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.UnloadedValueLabel.TabIndex = 10;
			this.UnloadedValueLabel.UseMnemonic = false;
			// 
			// GrossWeightUnloadedCalcDropEdit
			// 
			this.GrossWeightUnloadedCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightUnloadedCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_GrossWeightUnloaded)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CusInBondMoveDetail)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).MovementDetail.DifferenceMoveDetailCollection)).SyncRoot)).DifferenceWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).Lookups.WeightUnitList)));
			this.GrossWeightUnloadedCalcDropEdit.BindToAmount = "B0_GrossWeightUnloaded";
			this.GrossWeightUnloadedCalcDropEdit.BindToList = "Lookups.WeightUnitList";
			this.GrossWeightUnloadedCalcDropEdit.BindToUnit = "MovementDetail.DifferenceMoveDetailCollection.DifferenceWeightUnit";
			this.GrossWeightUnloadedCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 155, true);
			this.GrossWeightUnloadedCalcDropEdit.Name = "GrossWeightUnloadedCalcDropEdit";
			this.GrossWeightUnloadedCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 35, true);
			this.GrossWeightUnloadedCalcDropEdit.TabIndex = 12;
			// 
			// ArrivalTransportInfosUserControl
			// 
			this.ArrivalTransportInfosUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalTransportInfosUserControl, "ArrivalTransportInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.IArrivalCusTransportMeansCollection<Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).ArrivalTransportInfos)));
			this.ArrivalTransportInfosUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 630, true);
			this.ArrivalTransportInfosUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ArrivalTransportInfosUserControl.Name = "ArrivalTransportInfosUserControl";
			this.ArrivalTransportInfosUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 121, true);
			this.ArrivalTransportInfosUserControl.TabIndex = 19;
			// 
			// HouseConsignmentDifferencesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ArrivalTransportInfosUserControl);
			this.Controls.Add(this.GrossWeightUnloadedCalcDropEdit);
			this.Controls.Add(this.UnloadedValueLabel);
			this.Controls.Add(this.DeclaredValueLabel);
			this.Controls.Add(this.ConsigneeDocAddressControl);
			this.Controls.Add(this.ConsignorDocAddressControl);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.UnloadedStateDropEdit);
			this.Controls.Add(this.HouseConsignmentTextBox);
			this.Controls.Add(this.SecurityCheckBox);
			this.Controls.Add(this.SequenceNumberTextBox);
			this.Name = "HouseConsignmentDifferencesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 791, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnloadedStateDropEdit.ResumeLayout(true);
			this.UnloadedStateDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.ConsignorDocAddressControl.ResumeLayout(true);
			this.ConsignorDocAddressControl.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.GrossWeightUnloadedCalcDropEdit.ResumeLayout(true);
			this.GrossWeightUnloadedCalcDropEdit.PerformLayout();
			this.ArrivalTransportInfosUserControl.ResumeLayout(true);
			this.ArrivalTransportInfosUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox SequenceNumberTextBox;
		internal ZArchitecture.GUI.ZCheckBox SecurityCheckBox;
		internal ZArchitecture.ZTextBox HouseConsignmentTextBox;
		internal ZArchitecture.GUI.ZDropEdit UnloadedStateDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal MasterFiles.GUI.ZDocAddressControl ConsignorDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ConsigneeDocAddressControl;
		internal ZArchitecture.ZLabel DeclaredValueLabel;
		internal ZArchitecture.ZLabel UnloadedValueLabel;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightUnloadedCalcDropEdit;
		internal ArrivalTransportInfosUserControl ArrivalTransportInfosUserControl;
	}
}
