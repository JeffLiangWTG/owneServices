using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class CertificateLayoutTemplate
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CommonControlNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AnimalQuarantineCertificateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PlantProtectionCertificateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FoodHygieneCertificateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommercialValueTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TradeControlOrderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OtherLawsAndRegulationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OtherLawsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ApprovalCertificateInfosGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApprovalCertificateInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AnimalQuarantineCertificateTypeDropEdit.SuspendLayout();
			this.PlantProtectionCertificateTypeDropEdit.SuspendLayout();
			this.FoodHygieneCertificateTypeDropEdit.SuspendLayout();
			this.CommercialValueTypeDropEdit.SuspendLayout();
			this.TradeControlOrderDropEdit.SuspendLayout();
			this.OtherLawsAndRegulationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherLawsGrid)).BeginInit();
			this.OtherLawsGrid.SuspendLayout();
			this.ApprovalCertificateInfosGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApprovalCertificateInfoGrid)).BeginInit();
			this.ApprovalCertificateInfoGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// CommonControlNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommonControlNumberTextBox, "CEI_CommonControlNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CommonControlNumber)));
			this.CommonControlNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.CommonControlNumberTextBox.Name = "CommonControlNumberTextBox";
			this.CommonControlNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 16, true);
			this.CommonControlNumberTextBox.TabIndex = 0;
			// 
			// AnimalQuarantineCertificateTypeDropEdit
			// 
			this.AnimalQuarantineCertificateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AnimalQuarantineCertificateTypeDropEdit, "CEI_AnimalQuarantineCertificateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_AnimalQuarantineCertificateType)));
			this.AnimalQuarantineCertificateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
			this.AnimalQuarantineCertificateTypeDropEdit.Name = "AnimalQuarantineCertificateTypeDropEdit";
			this.AnimalQuarantineCertificateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 16, true);
			this.AnimalQuarantineCertificateTypeDropEdit.TabIndex = 4;
			// 
			// PlantProtectionCertificateTypeDropEdit
			// 
			this.PlantProtectionCertificateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlantProtectionCertificateTypeDropEdit, "CEI_PlantProtectionCertificateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_PlantProtectionCertificateType)));
			this.PlantProtectionCertificateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 0, true);
			this.PlantProtectionCertificateTypeDropEdit.Name = "PlantProtectionCertificateTypeDropEdit";
			this.PlantProtectionCertificateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 16, true);
			this.PlantProtectionCertificateTypeDropEdit.TabIndex = 5;
			// 
			// FoodHygieneCertificateTypeDropEdit
			// 
			this.FoodHygieneCertificateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FoodHygieneCertificateTypeDropEdit, "CEI_FoodHygieneCertificateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_FoodHygieneCertificateType)));
			this.FoodHygieneCertificateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 29, true);
			this.FoodHygieneCertificateTypeDropEdit.Name = "FoodHygieneCertificateTypeDropEdit";
			this.FoodHygieneCertificateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 16, true);
			this.FoodHygieneCertificateTypeDropEdit.TabIndex = 6;
			// 
			// CommercialValueTypeDropEdit
			// 
			this.CommercialValueTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommercialValueTypeDropEdit, "CEI_CommercialValueType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CommercialValueType)));
			this.CommercialValueTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 55, true);
			this.CommercialValueTypeDropEdit.Name = "CommercialValueTypeDropEdit";
			this.CommercialValueTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.CommercialValueTypeDropEdit.TabIndex = 7;
			// 
			// TradeControlOrderDropEdit
			// 
			this.TradeControlOrderDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeControlOrderDropEdit, "CEI_TradeControlOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_TradeControlOrder)));
			this.TradeControlOrderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 133, true);
			this.TradeControlOrderDropEdit.Name = "TradeControlOrderDropEdit";
			this.TradeControlOrderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.TradeControlOrderDropEdit.TabIndex = 10;
			// 
			// OtherLawsAndRegulationsGroupBox
			// 
			this.OtherLawsAndRegulationsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("49BC20C7-EF6E-43ED-BDBB-C41E9969A86D", "Other Laws and Regulations");
			this.OtherLawsAndRegulationsGroupBox.Controls.Add(this.OtherLawsGrid);
			this.OtherLawsAndRegulationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(680, 200, true);
			this.OtherLawsAndRegulationsGroupBox.Name = "OtherLawsAndRegulationsGroupBox";
			this.OtherLawsAndRegulationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 130, true);
			this.OtherLawsAndRegulationsGroupBox.TabIndex = 0;
			this.OtherLawsAndRegulationsGroupBox.TabStop = false;
			// 
			// OtherLawsGrid
			// 
			this.OtherLawsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OtherLawsGrid, "OtherLaws");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).OtherLaws)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.CusOtherLawReference)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).OtherLaws)).SyncRoot)).CFR_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.CusOtherLawReference)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).OtherLaws)).SyncRoot)).CodeDescription)));
			this.OtherLawsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CFR_Reference";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CodeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.OtherLawsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OtherLawsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OtherLawsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherLawsGrid.GridId = "74E7B5EB-A4CD-40E8-96AE-0650EB7FF59E";
			this.OtherLawsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OtherLawsGrid.LayoutKey = "OtherLawsGrid";
			this.OtherLawsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 24, true);
			this.OtherLawsGrid.Name = "OtherLawsGrid";
			this.OtherLawsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 97, true);
			this.OtherLawsGrid.TabIndex = 5;
			// 
			// ApprovalCertificateInfosGroupBox
			// 
			this.ApprovalCertificateInfosGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("843aabe0-b140-46b4-bc1c-f7130b25e8af", "Approval Certificate");
			this.ApprovalCertificateInfosGroupBox.Controls.Add(this.ApprovalCertificateInfoGrid);
			this.ApprovalCertificateInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 220, true);
			this.ApprovalCertificateInfosGroupBox.Name = "ApprovalCertificateInfosGroupBox";
			this.ApprovalCertificateInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 187, true);
			this.ApprovalCertificateInfosGroupBox.TabIndex = 3;
			this.ApprovalCertificateInfosGroupBox.TabStop = false;
			// 
			// ApprovalCertificateInfoGrid
			// 
			this.ApprovalCertificateInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApprovalCertificateInfoGrid, "ApprovalCertificateInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).ApprovalCertificateInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.ApprovalCertificateInfo)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).ApprovalCertificateInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.ApprovalCertificateInfo)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).ApprovalCertificateInfos)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.ApprovalCertificateInfo)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).ApprovalCertificateInfos)).SyncRoot)).ReferenceNumberDescription)));
			this.ApprovalCertificateInfoGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ReferenceNumberDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.ApprovalCertificateInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ApprovalCertificateInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ApprovalCertificateInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApprovalCertificateInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApprovalCertificateInfoGrid.GridId = "9EC56504-D49E-4B87-9B14-F3F51330866F";
			this.ApprovalCertificateInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApprovalCertificateInfoGrid.LayoutKey = "ApprovalCertificateInfoGrid";
			this.ApprovalCertificateInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ApprovalCertificateInfoGrid.Name = "ApprovalCertificateInfoGrid";
			this.ApprovalCertificateInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 454, true);
			this.ApprovalCertificateInfoGrid.TabIndex = 1;
			// 
			// CertificateLayoutTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CommonControlNumberTextBox);
			this.Controls.Add(this.AnimalQuarantineCertificateTypeDropEdit);
			this.Controls.Add(this.PlantProtectionCertificateTypeDropEdit);
			this.Controls.Add(this.FoodHygieneCertificateTypeDropEdit);
			this.Controls.Add(this.CommercialValueTypeDropEdit);
			this.Controls.Add(this.TradeControlOrderDropEdit);
			this.Controls.Add(this.OtherLawsAndRegulationsGroupBox);
			this.Controls.Add(this.ApprovalCertificateInfosGroupBox);
			this.Name = "CertificateLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AnimalQuarantineCertificateTypeDropEdit.ResumeLayout(true);
			this.AnimalQuarantineCertificateTypeDropEdit.PerformLayout();
			this.PlantProtectionCertificateTypeDropEdit.ResumeLayout(true);
			this.PlantProtectionCertificateTypeDropEdit.PerformLayout();
			this.FoodHygieneCertificateTypeDropEdit.ResumeLayout(true);
			this.FoodHygieneCertificateTypeDropEdit.PerformLayout();
			this.CommercialValueTypeDropEdit.ResumeLayout(true);
			this.CommercialValueTypeDropEdit.PerformLayout();
			this.TradeControlOrderDropEdit.ResumeLayout(true);
			this.TradeControlOrderDropEdit.PerformLayout();
			this.OtherLawsAndRegulationsGroupBox.ResumeLayout(false);
			this.OtherLawsAndRegulationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherLawsGrid)).EndInit();
			this.OtherLawsGrid.ResumeLayout(false);
			this.OtherLawsGrid.PerformLayout();
			this.ApprovalCertificateInfosGroupBox.ResumeLayout(false);
			this.ApprovalCertificateInfosGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApprovalCertificateInfoGrid)).EndInit();
			this.ApprovalCertificateInfoGrid.ResumeLayout(false);
			this.ApprovalCertificateInfoGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZGrid OtherLawsGrid;
		ZGroupBox OtherLawsAndRegulationsGroupBox;
		ZTextBox CommonControlNumberTextBox;
		ZDropEdit FoodHygieneCertificateTypeDropEdit;
		ZDropEdit PlantProtectionCertificateTypeDropEdit;
		ZDropEdit AnimalQuarantineCertificateTypeDropEdit;
		ZGroupBox ApprovalCertificateInfosGroupBox;
		ZGrid ApprovalCertificateInfoGrid;
		ZDropEdit TradeControlOrderDropEdit;
		ZDropEdit CommercialValueTypeDropEdit;
		#endregion
	}
}
