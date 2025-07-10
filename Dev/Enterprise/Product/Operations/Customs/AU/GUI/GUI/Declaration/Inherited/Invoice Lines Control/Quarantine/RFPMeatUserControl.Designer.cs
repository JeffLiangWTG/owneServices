using System.Drawing.Text;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPMeatUserControl
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

		private void InitializeComponent()
		{
			this.MeatGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HalalIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AdditionalProductsCodeFindBox = new Enterprise.Customs.GUI.CodeFindBoxWithMultiSelect();
			this.DominantProductCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.UngradedProductCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LabelApprovalIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QL_LabelApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_BeefVealWeightAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_ChemicalLeanPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MeatGroupBox.SuspendLayout();
			this.AdditionalProductsCodeFindBox.SuspendLayout();
			this.DominantProductCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			// 
			// MeatGroupBox
			// 
			this.MeatGroupBox.Controls.Add(this.HalalIndicatorCheckBox);
			this.MeatGroupBox.Controls.Add(this.AdditionalProductsCodeFindBox);
			this.MeatGroupBox.Controls.Add(this.DominantProductCodeFindBox);
			this.MeatGroupBox.Controls.Add(this.UngradedProductCheckBox);
			this.MeatGroupBox.Controls.Add(this.LabelApprovalIndicatorCheckBox);
			this.MeatGroupBox.Controls.Add(this.QL_LabelApprovalNumberTextBox);
			this.MeatGroupBox.Controls.Add(this.QL_BeefVealWeightAmountCalcEdit);
			this.MeatGroupBox.Controls.Add(this.QL_ChemicalLeanPercentageCalcEdit);
			this.MeatGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.MeatGroupBox.Name = "MeatGroupBox";
			this.MeatGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 206, true);
			this.MeatGroupBox.TabIndex = 0;
			this.MeatGroupBox.TabStop = false;
			this.MeatGroupBox.Text = "Meat";
			// 
			// HalalIndicatorCheckBox
			// 
			this.HalalIndicatorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HalalIndicatorCheckBox, "QuarantineExDocLine+QL_HalalProductIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_HalalProductIndicator)));
			this.HalalIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HalalIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HalalIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(441, 61, true);
			this.HalalIndicatorCheckBox.Name = "HalalIndicatorCheckBox";
			this.HalalIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.HalalIndicatorCheckBox.TabIndex = 28;
			this.HalalIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// AdditionalProductsCodeFindBox
			// 
			this.AdditionalProductsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalProductsCodeFindBox, "QuarantineExDocLine+QL_AdditionalProducts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_AdditionalProducts)));
			this.AdditionalProductsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 107, true);
			this.AdditionalProductsCodeFindBox.Name = "AdditionalProductsCodeFindBox";
			this.AdditionalProductsCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AdditionalProductsCodeFindBox.ParentType = null;
			this.AdditionalProductsCodeFindBox.ShowDescriptionBox = false;
			this.AdditionalProductsCodeFindBox.ShowNewFormWhenEmpty = false;
			this.AdditionalProductsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.AdditionalProductsCodeFindBox.TabIndex = 30;
			// 
			// DominantProductCodeFindBox
			// 
			this.DominantProductCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DominantProductCodeFindBox, "QuarantineExDocLine+QL_DominantProduct");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_DominantProduct)));
			this.DominantProductCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 81, true);
			this.DominantProductCodeFindBox.Name = "DominantProductCodeFindBox";
			this.DominantProductCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DominantProductCodeFindBox.ParentType = null;
			this.DominantProductCodeFindBox.ShowDescriptionBox = false;
			this.DominantProductCodeFindBox.ShowNewFormWhenEmpty = false;
			this.DominantProductCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.DominantProductCodeFindBox.TabIndex = 29;
			// 
			// UngradedProductCheckBox
			// 
			this.UngradedProductCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UngradedProductCheckBox, "QuarantineExDocLine+QL_UngradedProductIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_UngradedProductIndicator)));
			this.UngradedProductCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UngradedProductCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UngradedProductCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 61, true);
			this.UngradedProductCheckBox.Name = "UngradedProductCheckBox";
			this.UngradedProductCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UngradedProductCheckBox.TabIndex = 27;
			this.UngradedProductCheckBox.UseVisualStyleBackColor = true;
			// 
			// LabelApprovalIndicatorCheckBox
			// 
			this.LabelApprovalIndicatorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LabelApprovalIndicatorCheckBox, "QuarantineExDocLine+QL_LabelApprovalIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_LabelApprovalIndicator)));
			this.LabelApprovalIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LabelApprovalIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelApprovalIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 61, true);
			this.LabelApprovalIndicatorCheckBox.Name = "LabelApprovalIndicatorCheckBox";
			this.LabelApprovalIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.LabelApprovalIndicatorCheckBox.TabIndex = 26;
			this.LabelApprovalIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// QL_LabelApprovalNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.QL_LabelApprovalNumberTextBox, "QuarantineExDocLine+QL_LabelApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_LabelApprovalNumber)));
			this.QL_LabelApprovalNumberTextBox.CaptionResourceString = null;
			this.QL_LabelApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 37, true);
			this.QL_LabelApprovalNumberTextBox.Name = "QL_LabelApprovalNumberTextBox";
			this.QL_LabelApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.QL_LabelApprovalNumberTextBox.TabIndex = 25;
			// 
			// QL_BeefVealWeightAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_BeefVealWeightAmountCalcEdit, "QuarantineExDocLine+QL_BeefVealWeightAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_BeefVealWeightAmount)));
			this.QL_BeefVealWeightAmountCalcEdit.CaptionResourceString = null;
			this.QL_BeefVealWeightAmountCalcEdit.DecimalPlaces = 2;
			this.QL_BeefVealWeightAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(441, 13, true);
			this.QL_BeefVealWeightAmountCalcEdit.Name = "QL_BeefVealWeightAmountCalcEdit";
			this.QL_BeefVealWeightAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.QL_BeefVealWeightAmountCalcEdit.TabIndex = 3;
			this.QL_BeefVealWeightAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_ChemicalLeanPercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_ChemicalLeanPercentageCalcEdit, "QuarantineExDocLine+QL_ChemicalLeanPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ChemicalLeanPercentage)));
			this.QL_ChemicalLeanPercentageCalcEdit.CaptionResourceString = null;
			this.QL_ChemicalLeanPercentageCalcEdit.DecimalPlaces = 0;
			this.QL_ChemicalLeanPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 13, true);
			this.QL_ChemicalLeanPercentageCalcEdit.Name = "QL_ChemicalLeanPercentageCalcEdit";
			this.QL_ChemicalLeanPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.QL_ChemicalLeanPercentageCalcEdit.TabIndex = 1;
			this.QL_ChemicalLeanPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RFPMeatUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MeatGroupBox);
			this.Name = "RFPMeatUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 246, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MeatGroupBox.ResumeLayout(false);
			this.MeatGroupBox.PerformLayout();
			this.AdditionalProductsCodeFindBox.ResumeLayout(true);
			this.AdditionalProductsCodeFindBox.PerformLayout();
			this.DominantProductCodeFindBox.ResumeLayout(true);
			this.DominantProductCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZGroupBox MeatGroupBox;
		private ZCalcEdit QL_ChemicalLeanPercentageCalcEdit;
		private ZCalcEdit QL_BeefVealWeightAmountCalcEdit;
		private ZCheckBox HalalIndicatorCheckBox;
		private CodeFindBoxWithMultiSelect AdditionalProductsCodeFindBox;
		private ZCodeFindBox DominantProductCodeFindBox;
		private ZCheckBox LabelApprovalIndicatorCheckBox;
		private ZCheckBox UngradedProductCheckBox;
		private ZTextBox QL_LabelApprovalNumberTextBox;
	}
}
