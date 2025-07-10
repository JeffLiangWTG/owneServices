using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class CusCAeMHItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.eMHItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.eMHItemSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HSCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DGSubtanceLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.DGDetailsLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.DGSubtanceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.IsDangerouseInBulkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.eMHItemsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.eMHItemSplitContainer)).BeginInit();
			this.eMHItemSplitContainer.Panel1.SuspendLayout();
			this.eMHItemSplitContainer.Panel2.SuspendLayout();
			this.eMHItemSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusCAeMHItemCollection);
			// 
			// eMHItemsGrid
			// 
			this.eMHItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.eMHItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_QuantityUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_Marks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_HSCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_IsDangerousInBulk)));
			this.eMHItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8d7ae736-b747-491a-828e-5d0184831e0c", "Quantity");
			zCalcEditColumnStyleInfo1.ColumnName = "BX_Quantity";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ae3be0c5-f45e-4cf3-a2b7-36e4b22a91f8", "UQ");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "BX_QuantityUQ";
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9568d68d-6bfe-429b-b97b-59c8fb58cb47", "Description");
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "BX_Description";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8dcaedf5-bb3a-405b-b368-9f1ed77a0de8", "Marks & Nos");
			zMultiLineTextBoxColumnInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo2.ColumnName = "BX_Marks";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2e7b8b48-7c07-47e1-9473-1b941b9c0783", "HS Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BX_HSCode";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a1be6943-dd25-43f5-a3d8-dfa877c81723", "Dangerous?", "Is Dangerous In Bulk?", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "BX_IsDangerousInBulk";
			this.eMHItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.eMHItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.eMHItemsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.eMHItemsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.eMHItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.eMHItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.eMHItemsGrid.CopySelectedRowsAllowed = true;
			this.eMHItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eMHItemsGrid.GridId = "55ce5ee8-e3fe-4929-a8a6-f265dd3c43a1";
			this.eMHItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.eMHItemsGrid.LayoutKey = "eMHItemsGrid";
			this.eMHItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eMHItemsGrid.Name = "eMHItemsGrid";
			this.eMHItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 106, true);
			this.eMHItemsGrid.TabIndex = 0;
			// 
			// eMHItemSplitContainer
			// 
			this.eMHItemSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eMHItemSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.eMHItemSplitContainer.IsSplitterFixed = true;
			this.eMHItemSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eMHItemSplitContainer.Name = "eMHItemSplitContainer";
			this.eMHItemSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// eMHItemSplitContainer.Panel1
			// 
			this.eMHItemSplitContainer.Panel1.Controls.Add(this.eMHItemsGrid);
			// 
			// eMHItemSplitContainer.Panel2
			// 
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.IsDangerouseInBulkCheckBox);
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.HSCodeTextBox);
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.DGSubtanceLinkLabel);
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.DGDetailsLinkLabel);
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.DGSubtanceGuidFindBox);
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.MarksTextBox);
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.DescriptionTextBox);
			this.eMHItemSplitContainer.Panel2.Controls.Add(this.QuantityCalcDropEdit);
			this.eMHItemSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 240, true);
			this.eMHItemSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			this.eMHItemSplitContainer.TabIndex = 0;
			// 
			// HSCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.HSCodeTextBox, "BX_HSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_HSCode)));
			this.HSCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ecb1327e-00d7-4223-8c25-e25c53e58bea", "HS Code");
			this.HSCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 3, true);
			this.HSCodeTextBox.Name = "HSCodeTextBox";
			this.HSCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.HSCodeTextBox.TabIndex = 1;
			// 
			// DGSubtanceLinkLabel
			// 
			this.DGSubtanceLinkLabel.AutoSize = true;
			this.DGSubtanceLinkLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9a910219-1d2e-4ce0-a13f-541cf82da669", "Dangerous Goods Details");
			this.DGSubtanceLinkLabel.IsFontBold = false;
			this.DGSubtanceLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 31, true);
			this.DGSubtanceLinkLabel.Name = "DGSubtanceLinkLabel";
			this.DGSubtanceLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 13, true);
			this.DGSubtanceLinkLabel.TabIndex = 3;
			// 
			// DGDetailsLinkLabel
			// 
			this.DGDetailsLinkLabel.AutoSize = true;
			this.DGDetailsLinkLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8F0689D7-D5D6-49E2-BD2E-A59728CCA1ED", "Dangerous Goods Details");
			this.DGDetailsLinkLabel.IsFontBold = false;
			this.DGDetailsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 31, true);
			this.DGDetailsLinkLabel.Name = "DGDetailsLinkLabel";
			this.DGDetailsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 13, true);
			this.DGDetailsLinkLabel.TabIndex = 3;
			this.DGDetailsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DGManagementLinkLabel_LinkClicked);
			// 
			// DGSubtanceGuidFindBox
			// 
			this.DGSubtanceGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGSubtanceGuidFindBox, "FirstUNDG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).FirstUNDG)));
			this.DGSubtanceGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e352a4ad-c686-4a3a-855a-2031e3ad6545", "DG Subs");
			this.DGSubtanceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 29, true);
			this.DGSubtanceGuidFindBox.Name = "DGSubtanceGuidFindBox";
			this.DGSubtanceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.DGSubtanceGuidFindBox.TabIndex = 2;
			// 
			// MarksTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksTextBox, "BX_Marks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_Marks)));
			this.MarksTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5c879c3e-bb52-456c-ac2e-8c9d4c858bf1", "Marks & Numbers");
			this.MarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 55, true);
			this.MarksTextBox.Multiline = true;
			this.MarksTextBox.Name = "MarksTextBox";
			this.MarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 73, true);
			this.MarksTextBox.TabIndex = 5;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "BX_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_Description)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("36b64cfe-c3c6-4423-a7df-2c6cbfa50c75", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 55, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 73, true);
			this.DescriptionTextBox.TabIndex = 4;
			// 
			// QuantityCalcDropEdit
			// 
			this.QuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_QuantityUQ)));
			this.QuantityCalcDropEdit.BindToAmount = "BX_Quantity";
			this.QuantityCalcDropEdit.BindToUnit = "BX_QuantityUQ";
			this.QuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8ad3997a-5572-44f0-9a44-e2bc4d66a336", "Quantity");
			this.QuantityCalcDropEdit.Decimals = 0;
			this.QuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 3, true);
			this.QuantityCalcDropEdit.Name = "QuantityCalcDropEdit";
			this.QuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.QuantityCalcDropEdit.TabIndex = 0;
			// 
			// IsDangerouseInBulkCheckBox
			// 
			this.IsDangerouseInBulkCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsDangerouseInBulkCheckBox, "BX_IsDangerousInBulk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHItem)(null)).BX_IsDangerousInBulk)));
			this.IsDangerouseInBulkCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f03a021f-90a1-40db-a013-916b51e36053", "Is Dangerous In Bulk?");
			this.IsDangerouseInBulkCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsDangerouseInBulkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDangerouseInBulkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 31, true);
			this.IsDangerouseInBulkCheckBox.Name = "IsDangerouseInBulkCheckBox";
			this.IsDangerouseInBulkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
			this.IsDangerouseInBulkCheckBox.TabIndex = 6;
			this.IsDangerouseInBulkCheckBox.UseVisualStyleBackColor = true;
			// 
			// CusCAeMHItemUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.eMHItemSplitContainer);
			this.Name = "CusCAeMHItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.eMHItemsGrid)).EndInit();
			this.eMHItemSplitContainer.Panel1.ResumeLayout(false);
			this.eMHItemSplitContainer.Panel2.ResumeLayout(false);
			this.eMHItemSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.eMHItemSplitContainer)).EndInit();
			this.eMHItemSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid eMHItemsGrid;
		private CargoWise.Windows.UI.KSplitContainer eMHItemSplitContainer;
		private ZArchitecture.ZTextBox MarksTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit QuantityCalcDropEdit;
		private ZArchitecture.GUI.ZLinkLabel DGSubtanceLinkLabel;
		private ZArchitecture.GUI.ZLinkLabel DGDetailsLinkLabel;
		private ZArchitecture.GUI.ZGuidFindBox DGSubtanceGuidFindBox;
		private ZArchitecture.ZTextBox HSCodeTextBox;
		private ZArchitecture.GUI.ZCheckBox IsDangerouseInBulkCheckBox;
	}
}
