using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	internal partial class BillOfLadingNumberCustomisationControl
	{
		void InitializeComponent()
		{
			CargoWise.Windows.UI.KSplitContainer descriptionSplitter;
			elementNameColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			detailColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			numberFountainColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBox descriptionTextBox;
			Enterprise.ZArchitecture.GUI.ZGroupBox codeElementsPositionGroupBox;
			Enterprise.ZArchitecture.ZCalcEdit maxGeneratedLengthBoundCalcEdit;
			CargoWise.Windows.UI.KPanel optionPanel;
			this.elementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.checkDigitAlgorithmDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.maxLengthDividerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.parentOptionPanel = new CargoWise.Windows.UI.KPanel();
			this.removeFountainPrefixCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.autoAllocateMasterBillNumbersToConsolsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.useShipmentSequenceNumberCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.codingOfHouseBillNumberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			descriptionSplitter = new CargoWise.Windows.UI.KSplitContainer();
			descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			codeElementsPositionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			maxGeneratedLengthBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			insertFieldButton = new ZButton();

			optionPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(descriptionSplitter)).BeginInit();
			descriptionSplitter.Panel1.SuspendLayout();
			descriptionSplitter.Panel2.SuspendLayout();
			descriptionSplitter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).BeginInit();
			codeElementsPositionGroupBox.SuspendLayout();
			optionPanel.SuspendLayout();
			this.parentOptionPanel.SuspendLayout();
			this.codingOfHouseBillNumberGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.BillOfLadingNumberCustomisation);
			// 
			// descriptionSplitter
			// 
			descriptionSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			descriptionSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			descriptionSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			descriptionSplitter.Name = "descriptionSplitter";
			descriptionSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// descriptionSplitter.Panel1
			// 
			descriptionSplitter.Panel1.Controls.Add(this.elementsGrid);
			// 
			// descriptionSplitter.Panel2
			// 
			descriptionSplitter.Panel2.Controls.Add(descriptionTextBox);
			descriptionSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 249, true);
			descriptionSplitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			descriptionSplitter.TabIndex = 5;
			// 
			// elementsGrid
			// 
			this.elementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.elementsGrid, "Elements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).ElementName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).Detail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).DetailType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).Fountain)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).CheckDigit)));
			this.elementsGrid.CaptionVisible = false;
			elementNameColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|1cf94170-40ba-4b24-ad1e-173dba3a1a84", "Element Name");
			elementNameColumnStyleInfo.ColumnName = "ElementName";
			elementNameColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|60fcca6c-8b31-45cf-b5ca-1e3817ad710d", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "Include";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|31a3fe19-c51b-4fdd-be2f-b0ef5b4baecc", "Order");
			zCalcEditColumnStyleInfo1.ColumnName = "Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(37);
			detailColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|e7893744-591b-4f23-ba41-f644994c284f", "Digit/Code");
			detailColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			detailColumnStyleInfo.ColumnName = "Detail";
			detailColumnStyleInfo.FieldTypeColumnName = "DetailType";
			detailColumnStyleInfo.BindToDecimalPlaces = "DecimalPlacesForBinding";
			detailColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			numberFountainColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|6f5e14ea-e80b-40ae-b2bd-29cb5760791b", "Fountain");
			numberFountainColumnStyleInfo.ColumnName = "Fountain";
			numberFountainColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(51);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|fa4fcbf6-fc52-43f5-8223-0c6d5985e9cd", "Check Digit");
			zCheckBoxColumnStyleInfo3.ColumnName = "CheckDigit";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.elementsGrid.ColumnStyles.Add(elementNameColumnStyleInfo);
			this.elementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(detailColumnStyleInfo);
			this.elementsGrid.ColumnStyles.Add(numberFountainColumnStyleInfo);
			this.elementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.elementsGrid.CopySelectedRowsAllowed = true;
			this.elementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementsGrid.GridId = "988fbfa5-8271-45f2-8467-a1c5886f6add";
			this.elementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.elementsGrid.LayoutKey = "panel1";
			this.elementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.elementsGrid.Name = "elementsGrid";
			this.elementsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.elementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 220, true);
			this.elementsGrid.TabIndex = 0;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(descriptionTextBox, "Elements.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).Elements)).SyncRoot)).Description)));
			descriptionTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5afffaec-b107-450e-ab30-0366502f208c", "Description");
			descriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			descriptionTextBox.Multiline = true;
			descriptionTextBox.Name = "descriptionTextBox";
			descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 25, true);
			descriptionTextBox.TabIndex = 1;
			// 
			// codeElementsPositionGroupBox
			// 
			codeElementsPositionGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|929ca15a-6656-4c6d-bb2e-4b76d39ba06e", "Code Elements");
			codeElementsPositionGroupBox.Controls.Add(descriptionSplitter);
			codeElementsPositionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			codeElementsPositionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			codeElementsPositionGroupBox.Name = "codeElementsPositionGroupBox";
			codeElementsPositionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 268, true);
			codeElementsPositionGroupBox.TabIndex = 2;
			codeElementsPositionGroupBox.TabStop = false;
			// 
			// maxGeneratedLengthBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(maxGeneratedLengthBoundCalcEdit, "MaxGeneratedLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).MaxGeneratedLength)));
			maxGeneratedLengthBoundCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|271b582a-a689-4e24-a4d3-f3cc5ff294c9", "Max possible length");
			maxGeneratedLengthBoundCalcEdit.DecimalPlaces = 0;
			maxGeneratedLengthBoundCalcEdit.Decimals = 0;
			maxGeneratedLengthBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 1, true);
			maxGeneratedLengthBoundCalcEdit.Name = "maxGeneratedLengthBoundCalcEdit";
			maxGeneratedLengthBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			maxGeneratedLengthBoundCalcEdit.TabIndex = 1;
			maxGeneratedLengthBoundCalcEdit.Text = "0";
			maxGeneratedLengthBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// optionPanel
			// 
			optionPanel.Controls.Add(this.checkDigitAlgorithmDropEdit);
			optionPanel.Controls.Add(maxGeneratedLengthBoundCalcEdit);
			optionPanel.Controls.Add(this.maxLengthDividerLabel);
			optionPanel.Dock = System.Windows.Forms.DockStyle.Top;
			optionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			optionPanel.Name = "optionPanel";
			optionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 24, true);
			optionPanel.TabIndex = 1;
			// 
			// checkDigitAlgorithmDropEdit
			// 
			this.checkDigitAlgorithmDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.checkDigitAlgorithmDropEdit, "CheckDigitAlgorithm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).CheckDigitAlgorithm)));
			this.checkDigitAlgorithmDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|62f8422d-e286-4153-89bd-dda0cd17e6c0", "Check Digit Algorithm");
			this.checkDigitAlgorithmDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 1, true);
			this.checkDigitAlgorithmDropEdit.Name = "checkDigitAlgorithmDropEdit";
			this.checkDigitAlgorithmDropEdit.ShowDescriptionBox = false;
			this.checkDigitAlgorithmDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.checkDigitAlgorithmDropEdit.TabIndex = 0;
			// 
			// maxLengthDividerLabel
			// 
			this.maxLengthDividerLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|9e7900e4-6f1c-4af7-ba97-abe42b40a03f", "/{2}");
			this.maxLengthDividerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 0, true);
			this.maxLengthDividerLabel.Name = "maxLengthDividerLabel";
			this.maxLengthDividerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.maxLengthDividerLabel.TabIndex = 2;
			// 
			// insertFieldButton
			// 
			this.insertFieldButton.Name = "insertFieldButton";
			this.insertFieldButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.insertFieldButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 23, true);
			this.insertFieldButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6ce4d9bf-db55-4492-812e-32938495a3d2", "Insert Field", "Displays a list of all available fields that can be included on the Air Waybill.\r\nYou can choose a field and it will automatically be inserted into the registry item value.");
			this.insertFieldButton.Enabled = true;
			this.insertFieldButton.TabIndex = 3;
			this.insertFieldButton.Visible = false;
			// 
			// parentOptionPanel
			// 
			this.parentOptionPanel.Controls.Add(this.removeFountainPrefixCheckBox);
			this.parentOptionPanel.Controls.Add(this.useShipmentSequenceNumberCheckBox);
			this.parentOptionPanel.Controls.Add(this.autoAllocateMasterBillNumbersToConsolsCheckBox);			
			this.parentOptionPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.parentOptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.parentOptionPanel.Name = "parentOptionPanel";
			this.parentOptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 24, true);
			this.parentOptionPanel.TabIndex = 0;
			// 
			// removeFountainPrefixCheckBox
			// 
			this.BindingSource.SetBindingMember(this.removeFountainPrefixCheckBox, "RemoveFountainPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).RemoveFountainPrefix)));
			this.removeFountainPrefixCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.removeFountainPrefixCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 0, true);
			this.removeFountainPrefixCheckBox.Name = "removeFountainPrefixCheckBox";
			this.removeFountainPrefixCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.removeFountainPrefixCheckBox.TabIndex = 1;
			this.removeFountainPrefixCheckBox.Text = "Remove the \'{1}\' Prefix";
			// 
			// autoAllocateMasterBillNumbersToConsolsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.autoAllocateMasterBillNumbersToConsolsCheckBox, "AutoAllocateMasterBillNumbersToConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).AutoAllocateMasterBillNumbersToConsols)));
			this.autoAllocateMasterBillNumbersToConsolsCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|833b5c64-6975-40ef-ae4c-6b8d88797a8b", "Auto Allocate Master Bill Number");
			this.autoAllocateMasterBillNumbersToConsolsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoAllocateMasterBillNumbersToConsolsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 0, true);
			this.autoAllocateMasterBillNumbersToConsolsCheckBox.Name = "autoAllocateMasterBillNumbersToConsolsCheckBox";
			this.autoAllocateMasterBillNumbersToConsolsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.autoAllocateMasterBillNumbersToConsolsCheckBox.TabIndex = 2;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.useShipmentSequenceNumberCheckBox, "UseShipmentSequenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.BillOfLadingNumberCustomisation)(null)).UseShipmentSequenceNumber)));
			this.useShipmentSequenceNumberCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("billOfLadingNumberCustomisationControl|117c883a-6db2-4475-8f25-617f45af9d91", "Use {3} Sequence Number");
			this.useShipmentSequenceNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useShipmentSequenceNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.useShipmentSequenceNumberCheckBox.Name = "zCheckBox1";
			this.useShipmentSequenceNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.useShipmentSequenceNumberCheckBox.TabIndex = 0;
			// 
			// codingOfHouseBillNumberGroupBox
			// 
			this.codingOfHouseBillNumberGroupBox.Controls.Add(optionPanel);
			this.codingOfHouseBillNumberGroupBox.Controls.Add(this.parentOptionPanel);
			this.codingOfHouseBillNumberGroupBox.Controls.Add(this.insertFieldButton);
			this.codingOfHouseBillNumberGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.codingOfHouseBillNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.codingOfHouseBillNumberGroupBox.Name = "codingOfHouseBillNumberGroupBox";
			this.codingOfHouseBillNumberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 68, true);
			this.codingOfHouseBillNumberGroupBox.TabIndex = 0;
			this.codingOfHouseBillNumberGroupBox.TabStop = false;
			this.codingOfHouseBillNumberGroupBox.Text = "Coding of {0}";
			// 
			// billOfLadingNumberCustomisationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(codeElementsPositionGroupBox);
			this.Controls.Add(this.codingOfHouseBillNumberGroupBox);
			this.Name = "billOfLadingNumberCustomisationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 336, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			descriptionSplitter.Panel1.ResumeLayout(false);
			descriptionSplitter.Panel2.ResumeLayout(false);
			descriptionSplitter.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(descriptionSplitter)).EndInit();
			descriptionSplitter.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).EndInit();
			codeElementsPositionGroupBox.ResumeLayout(false);
			optionPanel.ResumeLayout(false);
			optionPanel.PerformLayout();
			this.parentOptionPanel.ResumeLayout(false);
			this.codingOfHouseBillNumberGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		internal Enterprise.ZArchitecture.GUI.ZCheckBox removeFountainPrefixCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox autoAllocateMasterBillNumbersToConsolsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox codingOfHouseBillNumberGroupBox;
		private Enterprise.ZArchitecture.ZGrid elementsGrid;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox useShipmentSequenceNumberCheckBox;
		private CargoWise.Windows.UI.KPanel parentOptionPanel;
		private Enterprise.ZArchitecture.ZLabel maxLengthDividerLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit checkDigitAlgorithmDropEdit;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo elementNameColumnStyleInfo;
		private Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo detailColumnStyleInfo;
		Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo numberFountainColumnStyleInfo;
		private ZButton insertFieldButton;
	}
}
