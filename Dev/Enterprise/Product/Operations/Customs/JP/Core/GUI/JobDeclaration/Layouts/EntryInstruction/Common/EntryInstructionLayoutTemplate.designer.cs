using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class EntryInstructionLayoutTemplate
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

		void InitializeComponent()
		{
			this.ValueTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TradeTypePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TradeTypeFirstCharDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TradeTypeSecondCharDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TradeTypeThirdCharDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsInspectionCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightzCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CargoQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalDeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TradeTypePanel.SuspendLayout();
			this.TradeTypeFirstCharDropEdit.SuspendLayout();
			this.TradeTypeSecondCharDropEdit.SuspendLayout();
			this.TradeTypeThirdCharDropEdit.SuspendLayout();
			this.ValueTypeDropEdit.SuspendLayout();
			this.WeightzCalcDropEdit.SuspendLayout();
			this.CargoQuantityCalcDropEdit.SuspendLayout();
			this.CustomsWeightCalcDropEdit.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.AdditionalDeclarationTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// ValueTypeDropEdit
			// 
			this.ValueTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueTypeDropEdit, "CEI_ValueType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_ValueType)));
			this.ValueTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 41, true);
			this.ValueTypeDropEdit.Name = "ValueTypeDropEdit";
			this.ValueTypeDropEdit.PreBoundMaxLength = 3;
			this.ValueTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.ValueTypeDropEdit.TabIndex = 1;
			//
			// 
			// CustomsInspectionCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsInspectionCodeTextBox, "CEI_CustomsInspectionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CustomsInspectionCode)));
			this.CustomsInspectionCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.CustomsInspectionCodeTextBox.Name = "CustomsInspectionCodeTextBox";
			this.CustomsInspectionCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.CustomsInspectionCodeTextBox.TabIndex = 1;
			// 
			// TradeTypePanel
			// 
			this.TradeTypePanel.Controls.Add(this.TradeTypeFirstCharDropEdit);
			this.TradeTypePanel.Controls.Add(this.TradeTypeSecondCharDropEdit);
			this.TradeTypePanel.Controls.Add(this.TradeTypeThirdCharDropEdit);
			this.TradeTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 21, true);
			this.TradeTypePanel.Name = "TradeTypePanel";
			this.TradeTypePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.TradeTypePanel.TabIndex = 0;
			// 
			// TradeTypeFirstCharDropEdit
			// 
			this.TradeTypeFirstCharDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeTypeFirstCharDropEdit, "TradeTypeFirstChar");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).TradeTypeFirstChar)));
			this.TradeTypeFirstCharDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TradeTypeFirstCharDropEdit.Name = "TradeTypeFirstCharDropEdit";
			this.TradeTypeFirstCharDropEdit.PreBoundMaxLength = 1;
			this.TradeTypeFirstCharDropEdit.ShowDescriptionBox = false;
			this.TradeTypeFirstCharDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 16, true);
			this.TradeTypeFirstCharDropEdit.TabIndex = 0;
			// 
			// TradeTypeSecondCharDropEdit
			// 
			this.TradeTypeSecondCharDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeTypeSecondCharDropEdit, "TradeTypeSecondChar");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).TradeTypeSecondChar)));
			this.TradeTypeSecondCharDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 0, true);
			this.TradeTypeSecondCharDropEdit.Name = "TradeTypeSecondCharDropEdit";
			this.TradeTypeSecondCharDropEdit.PreBoundMaxLength = 1;
			this.TradeTypeSecondCharDropEdit.ShowDescriptionBox = false;
			this.TradeTypeSecondCharDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 16, true);
			this.TradeTypeSecondCharDropEdit.TabIndex = 1;
			// 
			// TradeTypeThirdCharDropEdit
			// 
			this.TradeTypeThirdCharDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeTypeThirdCharDropEdit, "TradeTypeThirdChar");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).TradeTypeThirdChar)));
			this.TradeTypeThirdCharDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 0, true);
			this.TradeTypeThirdCharDropEdit.Name = "TradeTypeThirdCharDropEdit";
			this.TradeTypeThirdCharDropEdit.PreBoundMaxLength = 1;
			this.TradeTypeThirdCharDropEdit.ShowDescriptionBox = false;
			this.TradeTypeThirdCharDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 16, true);
			this.TradeTypeThirdCharDropEdit.TabIndex = 2;
			// 
			// WeightzCalcDropEdit
			// 
			this.WeightzCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightzCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_GrossWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).Lookups.GrossWeightUnitList)));
			this.WeightzCalcDropEdit.BindToAmount = "CEI_GrossWeight";
			this.WeightzCalcDropEdit.BindToList = "Lookups.GrossWeightUnitList";
			this.WeightzCalcDropEdit.BindToUnit = "CEI_GrossWeightUnit";
			this.WeightzCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
			this.WeightzCalcDropEdit.Name = "WeightzCalcDropEdit";
			this.WeightzCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 16, true);
			this.WeightzCalcDropEdit.TabIndex = 2;
			this.WeightzCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CargoQuantityCalcDropEdit
			// 
			this.CargoQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CargoQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CargoQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration.Lookups.PackingUnitTypesList)));
			this.CargoQuantityCalcDropEdit.BindToAmount = "CEI_CargoQuantity";
			this.CargoQuantityCalcDropEdit.BindToList = "JobDeclaration.Lookups.PackingUnitTypesList";
			this.CargoQuantityCalcDropEdit.BindToUnit = "CEI_CargoQuantityUnit";
			this.CargoQuantityCalcDropEdit.Decimals = 0;
			this.CargoQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136, true);
			this.CargoQuantityCalcDropEdit.MaxValue = 99999999;
			this.CargoQuantityCalcDropEdit.Name = "CargoQuantityCalcDropEdit";
			this.CargoQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 16, true);
			this.CargoQuantityCalcDropEdit.TabIndex = 3;
			this.CargoQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ContainerCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainerCountCalcEdit, "CEI_ContainerCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_ContainerCount)));
			this.ContainerCountCalcEdit.DecimalPlaces = 0;
			this.ContainerCountCalcEdit.Decimals = 0;
			this.ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 44, true);
			this.ContainerCountCalcEdit.Name = "ContainerCountCalcEdit";
			this.ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 16, true);
			this.ContainerCountCalcEdit.TabIndex = 5;
			this.ContainerCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ContainerCountCalcEdit.TrackDisposedAccess = true;
			// 
			// CustomsWeightCalcDropEdit
			// 
			this.CustomsWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CustomsWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CustomsWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).Lookups.CustomsWeightUnitConditionList)));
			this.CustomsWeightCalcDropEdit.BindToAmount = "CEI_CustomsWeight";
			this.CustomsWeightCalcDropEdit.BindToList = "Lookups.CustomsWeightUnitConditionList";
			this.CustomsWeightCalcDropEdit.BindToUnit = "CEI_CustomsWeightUnit";
			this.CustomsWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 62, true);
			this.CustomsWeightCalcDropEdit.Name = "CustomsWeightCalcDropEdit";
			this.CustomsWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 16, true);
			this.CustomsWeightCalcDropEdit.TabIndex = 7;
			this.CustomsWeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// DeclarationTypeDropEdit
			// 
			this.DeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "CEI_Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_Style)));
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
			this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
			this.DeclarationTypeDropEdit.PreBoundMaxLength = 1;
			this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DeclarationTypeDropEdit.TabIndex = 7;
			// 
			// AdditionalDeclarationTypeDropEdit
			// 
			this.AdditionalDeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalDeclarationTypeDropEdit, "CEI_SubStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_SubStyle)));
			this.AdditionalDeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 376, true);
			this.AdditionalDeclarationTypeDropEdit.Name = "AdditionalDeclarationTypeDropEdit";
			this.AdditionalDeclarationTypeDropEdit.PreBoundMaxLength = 1;
			this.AdditionalDeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AdditionalDeclarationTypeDropEdit.TabIndex = 8;
			// 
			// EntryInstructionLayoutTemplate
			// 
			this.Controls.Add(this.CustomsWeightCalcDropEdit);
			this.Controls.Add(this.ContainerCountCalcEdit);
			this.Controls.Add(this.TradeTypePanel);
			this.Controls.Add(this.CustomsInspectionCodeTextBox);
			this.Controls.Add(this.WeightzCalcDropEdit);
			this.Controls.Add(this.CargoQuantityCalcDropEdit);
			this.Controls.Add(this.DeclarationTypeDropEdit);
			this.Controls.Add(this.AdditionalDeclarationTypeDropEdit);
			this.Controls.Add(this.ValueTypeDropEdit);
			this.Name = "EntryInstructionLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TradeTypePanel.ResumeLayout(false);
			this.TradeTypePanel.PerformLayout();
			this.TradeTypeFirstCharDropEdit.ResumeLayout(true);
			this.TradeTypeFirstCharDropEdit.PerformLayout();
			this.TradeTypeSecondCharDropEdit.ResumeLayout(true);
			this.TradeTypeSecondCharDropEdit.PerformLayout();
			this.TradeTypeThirdCharDropEdit.ResumeLayout(true);
			this.TradeTypeThirdCharDropEdit.PerformLayout();
			this.WeightzCalcDropEdit.ResumeLayout(true);
			this.WeightzCalcDropEdit.PerformLayout();
			this.ValueTypeDropEdit.ResumeLayout(true);
			this.ValueTypeDropEdit.PerformLayout();
			this.CargoQuantityCalcDropEdit.ResumeLayout(true);
			this.CargoQuantityCalcDropEdit.PerformLayout();
			this.CustomsWeightCalcDropEdit.ResumeLayout(true);
			this.CustomsWeightCalcDropEdit.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.AdditionalDeclarationTypeDropEdit.ResumeLayout(true);
			this.AdditionalDeclarationTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZPanel TradeTypePanel;
		ZDropEdit TradeTypeFirstCharDropEdit;
		ZDropEdit TradeTypeSecondCharDropEdit;
		ZDropEdit TradeTypeThirdCharDropEdit;
		ZTextBox CustomsInspectionCodeTextBox;
		ZCalcDropEdit WeightzCalcDropEdit;
		ZCalcDropEdit CargoQuantityCalcDropEdit;
		ZCalcEdit ContainerCountCalcEdit;
		ZCalcDropEdit CustomsWeightCalcDropEdit;
		ZArchitecture.GUI.ZDropEdit ValueTypeDropEdit;
		ZDropEdit DeclarationTypeDropEdit;
		ZDropEdit AdditionalDeclarationTypeDropEdit;
	}
}
