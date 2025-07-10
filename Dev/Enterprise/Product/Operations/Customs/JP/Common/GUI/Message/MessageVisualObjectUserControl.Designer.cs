using System;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.JP.Shared.GUI
{
	partial class MessageVisualObjectUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ObjectsPreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ObjectsNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ObjectsCurrentNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ObjectsPreNextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ObjectsNumberOfResultsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ObjectsPreNextPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MenuPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HeaderGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeleteItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ItemsPreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ItemsNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ItemsCurrentNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ItemsPreNextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ItemsNumberOfResultsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ObjectsPreNextPanel.SuspendLayout();
			this.MenuPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.HeaderGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HeaderGrid)).BeginInit();
			this.HeaderGrid.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Common.MessageVisualObjectParent);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("9D3BF2D1-6383-4190-A1AF-C67D72659A82", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(689, 8, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.ToolTipCaption = null;
			// 
			// ObjectsPreviousButton
			// 
			this.ObjectsPreviousButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectsPreviousButton.BackColor = System.Drawing.Color.Transparent;
			this.ObjectsPreviousButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ObjectsPreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.ObjectsPreviousButton.Name = "ObjectsPreviousButton";
			this.ObjectsPreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.ObjectsPreviousButton.TabIndex = 0;
			this.ObjectsPreviousButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ObjectsPreviousButton.ToolTipCaption = null;
			this.ObjectsPreviousButton.UseVisualStyleBackColor = false;
			// 
			// ObjectsNextButton
			// 
			this.ObjectsNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectsNextButton.BackColor = System.Drawing.Color.Transparent;
			this.ObjectsNextButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ObjectsNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 6, true);
			this.ObjectsNextButton.Name = "ObjectsNextButton";
			this.ObjectsNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.ObjectsNextButton.TabIndex = 4;
			this.ObjectsNextButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ObjectsNextButton.ToolTipCaption = null;
			this.ObjectsNextButton.UseVisualStyleBackColor = false;
			// 
			// ObjectsCurrentNumberCalcEdit
			// 
			this.ObjectsCurrentNumberCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ObjectsCurrentNumberCalcEdit, "CurrentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).CurrentNumber)));
			this.ObjectsCurrentNumberCalcEdit.DecimalPlaces = 0;
			this.ObjectsCurrentNumberCalcEdit.Decimals = 0;
			this.ObjectsCurrentNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 8, true);
			this.ObjectsCurrentNumberCalcEdit.Name = "ObjectsCurrentNumberCalcEdit";
			this.ObjectsCurrentNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.ObjectsCurrentNumberCalcEdit.TabIndex = 1;
			this.ObjectsCurrentNumberCalcEdit.Text = "10,00";
			this.ObjectsCurrentNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ObjectsCurrentNumberCalcEdit.TrackDisposedAccess = true;
			this.ObjectsCurrentNumberCalcEdit.AllowNegative = false;
			this.ObjectsCurrentNumberCalcEdit.CaptionResourceString = null;
			// 
			// ObjectsPreNextLabel
			// 
			this.ObjectsPreNextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectsPreNextLabel.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("ABD4411B-7391-4B2A-938F-0772EAEF12F8", "of");
			this.ObjectsPreNextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ObjectsPreNextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 8, true);
			this.ObjectsPreNextLabel.Name = "ObjectsPreNextLabel";
			this.ObjectsPreNextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.ObjectsPreNextLabel.TabIndex = 2;
			this.ObjectsPreNextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ObjectsPreNextLabel.UseMnemonic = false;
			// 
			// ObjectsNumberOfResultsCalcEdit
			// 
			this.ObjectsNumberOfResultsCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ObjectsNumberOfResultsCalcEdit, "NumberOfResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).NumberOfResults)));
			this.ObjectsNumberOfResultsCalcEdit.DecimalPlaces = 0;
			this.ObjectsNumberOfResultsCalcEdit.Decimals = 0;
			this.ObjectsNumberOfResultsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 8, true);
			this.ObjectsNumberOfResultsCalcEdit.Name = "ObjectsNumberOfResultsCalcEdit";
			this.ObjectsNumberOfResultsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.ObjectsNumberOfResultsCalcEdit.TabIndex = 3;
			this.ObjectsNumberOfResultsCalcEdit.Text = "10,000";
			this.ObjectsNumberOfResultsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ObjectsNumberOfResultsCalcEdit.TrackDisposedAccess = true;
			this.ObjectsNumberOfResultsCalcEdit.AllowNegative = false;
			this.ObjectsNumberOfResultsCalcEdit.CaptionResourceString = null;
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfirmButton.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("80794177-0754-43C5-8D5A-F12D80A0CFA9", "Confirm");
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 8, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ConfirmButton.TabIndex = 1;
			this.ConfirmButton.ToolTipCaption = null;
			// 
			// ObjectsPreNextPanel
			// 
			this.ObjectsPreNextPanel.BackColor = System.Drawing.Color.Transparent;
			this.ObjectsPreNextPanel.Controls.Add(this.ObjectsPreviousButton);
			this.ObjectsPreNextPanel.Controls.Add(this.ObjectsNumberOfResultsCalcEdit);
			this.ObjectsPreNextPanel.Controls.Add(this.ObjectsPreNextLabel);
			this.ObjectsPreNextPanel.Controls.Add(this.ObjectsNextButton);
			this.ObjectsPreNextPanel.Controls.Add(this.ObjectsCurrentNumberCalcEdit);
			this.ObjectsPreNextPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.ObjectsPreNextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ObjectsPreNextPanel.Name = "ObjectsPreNextPanel";
			this.ObjectsPreNextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 34, true);
			this.ObjectsPreNextPanel.TabIndex = 0;
			// 
			// MenuPanel
			// 
			this.MenuPanel.BackColor = System.Drawing.Color.Transparent;
			this.MenuPanel.Controls.Add(this.ObjectsPreNextPanel);
			this.MenuPanel.Controls.Add(this.CancelButton);
			this.MenuPanel.Controls.Add(this.ConfirmButton);
			this.MenuPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.MenuPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 516, true);
			this.MenuPanel.Name = "MenuPanel";
			this.MenuPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 34, true);
			this.MenuPanel.TabIndex = 20;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.HeaderGroupBox);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.ItemsGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 516, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(180);
			this.MainSplitContainer.SplitterWidth = 9;
			this.MainSplitContainer.TabIndex = 0;
			this.MainSplitContainer.TabStop = false;
			// 
			// HeaderGroupBox
			// 
			this.HeaderGroupBox.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("CC2C3056-1693-4E5A-8177-09EBB419FF43", "Header");
			this.HeaderGroupBox.Controls.Add(this.HeaderGrid);
			this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 180, true);
			this.HeaderGroupBox.TabIndex = 0;
			this.HeaderGroupBox.TabStop = false;
			// 
			// HeaderGrid
			// 
			this.HeaderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HeaderGrid, "VisualObjects.Header");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).No)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).ID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).JPName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).OriginalValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).OverrideValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).Header)).SyncRoot)).Instruction)));
			this.HeaderGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "No";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo6.ColumnName = "ID";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo7.ColumnName = "Name";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.ColumnName = "JPName";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.ColumnName = "OriginalValue";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.ColumnName = "OverrideValue";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "Length";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zMultiLineTextBoxColumnInfo2.ColumnName = "Instruction";
			zMultiLineTextBoxColumnInfo2.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.HeaderGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.HeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.HeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.HeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.HeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.HeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.HeaderGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.HeaderGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.HeaderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderGrid.GridId = "80AE91B8-178A-4CE2-A617-D68277968BD5";
			this.HeaderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HeaderGrid.LayoutKey = "HeaderGrid";
			this.HeaderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HeaderGrid.Name = "HeaderGrid";
			this.HeaderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 161, true);
			this.HeaderGrid.TabIndex = 2;
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("E4E56A8E-4D7B-4F66-96C2-DC9FFBDF26BB", "Items");
			this.ItemsGroupBox.Controls.Add(this.DeleteItemButton);
			this.ItemsGroupBox.Controls.Add(this.NewItemButton);
			this.ItemsGroupBox.Controls.Add(this.ItemsGrid);
			this.ItemsGroupBox.Controls.Add(this.ItemsPreviousButton);
			this.ItemsGroupBox.Controls.Add(this.ItemsNextButton);
			this.ItemsGroupBox.Controls.Add(this.ItemsCurrentNumberCalcEdit);
			this.ItemsGroupBox.Controls.Add(this.ItemsPreNextLabel);
			this.ItemsGroupBox.Controls.Add(this.ItemsNumberOfResultsCalcEdit);
			this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsGroupBox.Name = "ItemsGroupBox";
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 330, true);
			this.ItemsGroupBox.TabIndex = 0;
			this.ItemsGroupBox.TabStop = false;
			// 
			// BtnDeleteItem
			// 
			this.DeleteItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteItemButton.BackColor = System.Drawing.Color.Transparent;
			this.DeleteItemButton.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("65b541a0-305d-44dd-a114-14944073153d", "Delete");
			this.DeleteItemButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DeleteItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 17, true);
			this.DeleteItemButton.Name = "BtnDeleteItem";
			this.DeleteItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 23, true);
			this.DeleteItemButton.TabIndex = 23;
			this.DeleteItemButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DeleteItemButton.ToolTipCaption = null;
			this.DeleteItemButton.UseVisualStyleBackColor = false;
			// 
			// BtnNewItem
			// 
			this.NewItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.NewItemButton.BackColor = System.Drawing.Color.Transparent;
			this.NewItemButton.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("768c492b-62cd-4fce-88b9-d34a066e5e7f", "New");
			this.NewItemButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.NewItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 17, true);
			this.NewItemButton.Name = "BtnNewItem";
			this.NewItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.NewItemButton.TabIndex = 22;
			this.NewItemButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.NewItemButton.ToolTipCaption = null;
			this.NewItemButton.UseVisualStyleBackColor = false;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.ItemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ItemsGrid, "VisualObjects.CurrentItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).No)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).ID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).JPName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).OriginalValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).OverrideValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.EditableFieldBizObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentItems)).SyncRoot)).Instruction)));
			this.ItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "No";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo11.ColumnName = "ID";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo12.ColumnName = "Name";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo13.ColumnName = "JPName";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo14.ColumnName = "OriginalValue";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo15.ColumnName = "OverrideValue";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "Length";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zMultiLineTextBoxColumnInfo3.ColumnName = "Instruction";
			zMultiLineTextBoxColumnInfo3.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ItemsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.ItemsGrid.GridId = "0ADB7064-83B9-421A-BE13-AA1B19455228";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "ItemsGrid";
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 43, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 284, true);
			this.ItemsGrid.TabIndex = 5;
			// 
			// ItemsPreviousButton
			// 
			this.ItemsPreviousButton.BackColor = System.Drawing.Color.Transparent;
			this.ItemsPreviousButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ItemsPreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.ItemsPreviousButton.Name = "ItemsPreviousButton";
			this.ItemsPreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.ItemsPreviousButton.TabIndex = 0;
			this.ItemsPreviousButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ItemsPreviousButton.ToolTipCaption = null;
			this.ItemsPreviousButton.UseVisualStyleBackColor = false;
			// 
			// ItemsNextButton
			// 
			this.ItemsNextButton.BackColor = System.Drawing.Color.Transparent;
			this.ItemsNextButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ItemsNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 17, true);
			this.ItemsNextButton.Name = "ItemsNextButton";
			this.ItemsNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.ItemsNextButton.TabIndex = 1;
			this.ItemsNextButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ItemsNextButton.ToolTipCaption = null;
			this.ItemsNextButton.UseVisualStyleBackColor = false;
			// 
			// ItemsCurrentNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ItemsCurrentNumberCalcEdit, "VisualObjects.CurrentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).CurrentNumber)));
			this.ItemsCurrentNumberCalcEdit.DecimalPlaces = 0;
			this.ItemsCurrentNumberCalcEdit.Decimals = 0;
			this.ItemsCurrentNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 19, true);
			this.ItemsCurrentNumberCalcEdit.Name = "ItemsCurrentNumberCalcEdit";
			this.ItemsCurrentNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.ItemsCurrentNumberCalcEdit.TabIndex = 2;
			this.ItemsCurrentNumberCalcEdit.Text = "10,00";
			this.ItemsCurrentNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ItemsCurrentNumberCalcEdit.TrackDisposedAccess = true;
			this.ItemsCurrentNumberCalcEdit.AllowNegative = false;
			this.ItemsCurrentNumberCalcEdit.CaptionResourceString = null;
			// 
			// ItemsPreNextLabel
			// 
			this.ItemsPreNextLabel.CaptionResourceString = Enterprise.Customs.JP.Shared.GUI.Res.GetData("3A46C872-049A-43E6-BF43-E4877C1F7F70", "of");
			this.ItemsPreNextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ItemsPreNextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 19, true);
			this.ItemsPreNextLabel.Name = "ItemsPreNextLabel";
			this.ItemsPreNextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.ItemsPreNextLabel.TabIndex = 3;
			this.ItemsPreNextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ItemsPreNextLabel.UseMnemonic = false;
			// 
			// ItemsNumberOfResultsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ItemsNumberOfResultsCalcEdit, "VisualObjects.NumberOfResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.MessageVisualObject)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.MessageVisualObjectParent)(null)).VisualObjects)).SyncRoot)).NumberOfResults)));
			this.ItemsNumberOfResultsCalcEdit.DecimalPlaces = 0;
			this.ItemsNumberOfResultsCalcEdit.Decimals = 0;
			this.ItemsNumberOfResultsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.ItemsNumberOfResultsCalcEdit.Name = "ItemsNumberOfResultsCalcEdit";
			this.ItemsNumberOfResultsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.ItemsNumberOfResultsCalcEdit.TabIndex = 4;
			this.ItemsNumberOfResultsCalcEdit.Text = "10,000";
			this.ItemsNumberOfResultsCalcEdit.AllowNegative = false;
			this.ItemsNumberOfResultsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ItemsNumberOfResultsCalcEdit.TrackDisposedAccess = true;
			this.ItemsNumberOfResultsCalcEdit.CaptionResourceString = null;
			// 
			// MessageVisualObjectUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Controls.Add(this.MenuPanel);
			this.Name = "MessageVisualObjectUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 550, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ObjectsPreNextPanel.ResumeLayout(false);
			this.ObjectsPreNextPanel.PerformLayout();
			this.MenuPanel.ResumeLayout(false);
			this.MenuPanel.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HeaderGrid)).EndInit();
			this.HeaderGrid.ResumeLayout(false);
			this.HeaderGrid.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZButton ObjectsPreviousButton;
		ZButton ObjectsNextButton;
		ZCalcEdit ObjectsCurrentNumberCalcEdit;
		ZLabel ObjectsPreNextLabel;
		ZCalcEdit ObjectsNumberOfResultsCalcEdit;
		ZPanel ObjectsPreNextPanel;
		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		ZGroupBox HeaderGroupBox;
		ZGrid HeaderGrid;
		ZGroupBox ItemsGroupBox;
		ZGrid ItemsGrid;
		ZButton ItemsPreviousButton;
		ZButton ItemsNextButton;
		ZCalcEdit ItemsCurrentNumberCalcEdit;
		ZLabel ItemsPreNextLabel;
		ZCalcEdit ItemsNumberOfResultsCalcEdit;
		public ZButton ConfirmButton;
		public ZButton CancelButton;
		public ZPanel MenuPanel;
		ZButton DeleteItemButton;
		ZButton NewItemButton;
	}
}
