namespace Enterprise.Customs.GB.GUI.Plugin
{
	partial class GBFECChallengeUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.FECChallengesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewValueDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OriginalValueDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NewValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OriginalValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FieldTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ObjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewValueCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OriginalValueCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ConfirmCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FECChallengesGrid)).BeginInit();
			this.FECChallengesGrid.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.NewValueDropEdit.SuspendLayout();
			this.OriginalValueDropEdit.SuspendLayout();
			this.NewValueCodeFindBox.SuspendLayout();
			this.OriginalValueCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Declaration.CusEntryHeader);
			// 
			// FECChallengesGrid
			// 
			this.FECChallengesGrid.AllowNavigation = false;
			this.FECChallengesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FECChallengesGrid, "FECChallenges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).RelatedObjectName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).RelatedObjectFieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).CY_IsOverridden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).NewValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).NewValueFieldType)));
			this.FECChallengesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ea3b026c-9495-4baf-a5bb-6529063a5f5d", "Object");
			zTextBoxColumnStyleInfo1.ColumnName = "RelatedObjectName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("84b20238-c73d-4015-a8a1-1bcc7aaf7824", "Field");
			zTextBoxColumnStyleInfo2.ColumnName = "RelatedObjectFieldName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("81d12b63-da5c-411c-a5ba-63ffcdfdd1ae", "Old Value");
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("45970513-5377-45e5-b721-a208befcae7c", "Confirm?");
			zCheckBoxColumnStyleInfo1.ColumnName = "CY_IsOverridden";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("b93083f1-8530-447b-aae5-55913c5c1d16", "New Value");
			zMultiControlColumnStyleInfo1.ColumnName = "NewValue";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "NewValueFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.FECChallengesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FECChallengesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FECChallengesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FECChallengesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FECChallengesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.FECChallengesGrid.GridId = "31620fa1-6881-4a47-a0d7-14fb8a7f97f8";
			this.FECChallengesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FECChallengesGrid.LayoutKey = "zGrid1";
			this.FECChallengesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FECChallengesGrid.Name = "FECChallengesGrid";
			this.FECChallengesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 160, true);
			this.FECChallengesGrid.TabIndex = 0;
			// 
			// zGroupBox1
			//
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("47de8e28-5b54-411d-978e-cebfba9ea0ce", "FEC Challenges");
			this.zGroupBox1.Controls.Add(this.NewValueDropEdit);
			this.zGroupBox1.Controls.Add(this.OriginalValueDropEdit);
			this.zGroupBox1.Controls.Add(this.NewValueCalcEdit);
			this.zGroupBox1.Controls.Add(this.OriginalValueCalcEdit);
			this.zGroupBox1.Controls.Add(this.FieldTextBox);
			this.zGroupBox1.Controls.Add(this.ObjectTextBox);
			this.zGroupBox1.Controls.Add(this.NewValueCodeFindBox);
			this.zGroupBox1.Controls.Add(this.OriginalValueCodeFindBox);
			this.zGroupBox1.Controls.Add(this.ConfirmCheckBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 161, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 130, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// NewValueDropEdit
			// 
			this.NewValueDropEdit.AllowDrop = true;
			this.NewValueDropEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.NewValueDropEdit, "FECChallenges.NewValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).NewValue)));
			this.NewValueDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("334e01d2-563f-44f4-a7e2-9b71e8e7e738", "New Value");
			this.NewValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 96, true);
			this.NewValueDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.NewValueDropEdit.Name = "NewValueDropEdit";
			this.NewValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.NewValueDropEdit.TabIndex = 4;
			// 
			// OriginalValueDropEdit
			// 
			this.OriginalValueDropEdit.AllowDrop = true;
			this.OriginalValueDropEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.OriginalValueDropEdit, "FECChallenges.CY_Data");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).CY_Data)));
			this.OriginalValueDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ff8bfe0f-3a1a-4b09-a1c8-68e3e845832c", "Original Value");
			this.OriginalValueDropEdit.Enabled = false;
			this.OriginalValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 70, true);
			this.OriginalValueDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.OriginalValueDropEdit.Name = "OriginalValueDropEdit";
			this.OriginalValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.OriginalValueDropEdit.TabIndex = 2;
			// 
			// NewValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NewValueCalcEdit, "FECChallenges.NewValue_Decimal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).NewValue_Decimal)));
			this.NewValueCalcEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("dc0df33f-11b5-4a62-8e37-0f496324de09", "New Value");
			this.NewValueCalcEdit.DecimalPlaces = 2;
			this.NewValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 96, true);
			this.NewValueCalcEdit.Name = "NewValueCalcEdit";
			this.NewValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NewValueCalcEdit.TabIndex = 4;
			this.NewValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OriginalValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OriginalValueCalcEdit, "FECChallenges.CY_Data_Decimal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).CY_Data_Decimal)));
			this.OriginalValueCalcEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7a9bec35-ee3b-445a-930a-a804f858a211", "Original Value");
			this.OriginalValueCalcEdit.DecimalPlaces = 2;
			this.OriginalValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 70, true);
			this.OriginalValueCalcEdit.Name = "OriginalValueCalcEdit";
			this.OriginalValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OriginalValueCalcEdit.TabIndex = 2;
			this.OriginalValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FieldTextBox
			// 
			this.BindingSource.SetBindingMember(this.FieldTextBox, "FECChallenges.RelatedObjectFieldName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).RelatedObjectFieldName)));
			this.FieldTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("bcf8eac0-cab6-45fd-87f8-fef6a1ac8e33", "Field");
			this.FieldTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FieldTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 45, true);
			this.FieldTextBox.Name = "FieldTextBox";
			this.FieldTextBox.ReadOnly = true;
			this.FieldTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.FieldTextBox.TabIndex = 1;
			// 
			// ObjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.ObjectTextBox, "FECChallenges.RelatedObjectName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).RelatedObjectName)));
			this.ObjectTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("45e933c8-9ad1-4ad1-b7ca-ea79725e0096", "Object");
			this.ObjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ObjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
			this.ObjectTextBox.Name = "ObjectTextBox";
			this.ObjectTextBox.ReadOnly = true;
			this.ObjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.ObjectTextBox.TabIndex = 0;
			// 
			// NewValueCodeFindBox
			// 
			this.NewValueCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewValueCodeFindBox, "FECChallenges.NewValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).NewValue)));
			this.NewValueCodeFindBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5782ede2-4274-4315-8c0a-d985fb151368", "New Value");
			this.NewValueCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 97, true);
			this.NewValueCodeFindBox.Name = "NewValueCodeFindBox";
			this.NewValueCodeFindBox.ShouldResize = false;
			this.NewValueCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NewValueCodeFindBox.TabIndex = 4;
			// 
			// OriginalValueCodeFindBox
			// 
			this.OriginalValueCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginalValueCodeFindBox, "FECChallenges.CY_Data");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).CY_Data)));
			this.OriginalValueCodeFindBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("96dca906-dd93-4a94-9f02-9801e5fdebaa", "Original Value");
			this.OriginalValueCodeFindBox.Enabled = false;
			this.OriginalValueCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 71, true);
			this.OriginalValueCodeFindBox.Name = "OriginalValueCodeFindBox";
			this.OriginalValueCodeFindBox.ShouldResize = false;
			this.OriginalValueCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OriginalValueCodeFindBox.TabIndex = 2;
			// 
			// ConfirmCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ConfirmCheckBox, "FECChallenges.CY_IsOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.FECChallenge)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(null)).FECChallenges)).SyncRoot)).CY_IsOverridden)));
			this.ConfirmCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2ad5128c-66fb-4271-a10d-cdee7093a92b", "Confirm?");
			this.ConfirmCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConfirmCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 69, true);
			this.ConfirmCheckBox.Name = "ConfirmCheckBox";
			this.ConfirmCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.ConfirmCheckBox.TabIndex = 3;
			this.ConfirmCheckBox.UseVisualStyleBackColor = true;
			// 
			// GBFECChallengeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.FECChallengesGrid);
			this.Name = "GBFECChallengeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 291, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FECChallengesGrid)).EndInit();
			this.FECChallengesGrid.ResumeLayout(false);
			this.FECChallengesGrid.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.NewValueDropEdit.ResumeLayout(true);
			this.NewValueDropEdit.PerformLayout();
			this.OriginalValueDropEdit.ResumeLayout(true);
			this.OriginalValueDropEdit.PerformLayout();
			this.NewValueCodeFindBox.ResumeLayout(true);
			this.NewValueCodeFindBox.PerformLayout();
			this.OriginalValueCodeFindBox.ResumeLayout(true);
			this.OriginalValueCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid FECChallengesGrid;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZTextBox FieldTextBox;
		private ZArchitecture.ZTextBox ObjectTextBox;
		private ZArchitecture.GUI.ZCodeFindBox OriginalValueCodeFindBox;
		private ZArchitecture.GUI.ZCheckBox ConfirmCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox NewValueCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit NewValueDropEdit;
		private ZArchitecture.GUI.ZDropEdit OriginalValueDropEdit;
		private ZArchitecture.ZCalcEdit NewValueCalcEdit;
		private ZArchitecture.ZCalcEdit OriginalValueCalcEdit;
	}
}
