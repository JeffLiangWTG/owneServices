using System;

namespace Enterprise.Customs.CA.GUI
{
	partial class CADCorrectionMessageSendingForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendingActionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendingActionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.notificationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.sendOption = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.discardOption = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendingActionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SendingActionsGrid)).BeginInit();
			this.SendingActionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 305, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B4D348CB-CC36-49F6-90A0-E27F9C1ACA63", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 269, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 9;
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3D9D116D-5B17-483C-8EBD-A88B3FBB75BE", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 269, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 8;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// SendingActionsGroupBox
			//
			this.SendingActionsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DA4CDE5A-FF81-453B-A65D-47652267AC1D", "Amendment Details");
			this.SendingActionsGroupBox.Controls.Add(this.SendingActionsGrid);
			this.SendingActionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 62, true);
			this.SendingActionsGroupBox.Name = "SendingActionsGroupBox";
			this.SendingActionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 178, true);
			this.SendingActionsGroupBox.TabIndex = 0;
			this.SendingActionsGroupBox.TabStop = false;
			// 
			// SendingActionsGrid
			// 
			this.SendingActionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SendingActionsGrid, "SendingActions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)).SyncRoot)).InvoiceSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)).SyncRoot)).EntryLineSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)).SyncRoot)).ReasonCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)).SyncRoot)).AppealsProgramCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendingActions)).SyncRoot)).CSI_Description)));
			this.SendingActionsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "EntryLineSequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.MaxValue = 99999;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "InvoiceSequence";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.MaxValue = 99999;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "InvoiceLineSequence";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.MaxValue = 99999;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("991EE3AA-2796-4B7B-AF9B-14865026861E", "Reason Code");
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6B71ADD1-2991-4E61-901E-7C1A03C64DA0", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ReasonCodeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("991EE3AA-2796-4B7B-AF9B-14865026861E", "Reason Code");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("940E6F62-23FF-4D10-825F-A08E45224643", "Appeals Program Code");
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3CF3BBE1-87F7-472D-97E0-DCEC1088C945", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "AppealsProgramCodeDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("940E6F62-23FF-4D10-825F-A08E45224643", "Appeals Program Code");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "CSI_Description";
			zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 350;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			this.SendingActionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SendingActionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SendingActionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SendingActionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SendingActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SendingActionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SendingActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SendingActionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);

			this.SendingActionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SendingActionsGrid.GridId = "4c2cd689-cc9f-4539-b145-40d963c0a807";
			this.SendingActionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SendingActionsGrid.LayoutKey = "SendingActionsGrid";
			this.SendingActionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SendingActionsGrid.Name = "SendingActionsGrid";
			this.SendingActionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 159, true);
			this.SendingActionsGrid.TabIndex = 0;
			// 
			// notificationLabel
			//
			this.notificationLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5EF6A5C5-F4DE-4E9B-BE14-3745A12CE0E3", "The system has detected that changes made to the declaration will require an amendment to be sent to Customs. Please select an option below and \'OK\'. Alternatively select \'Cancel\' which cancels the process of amendment.");
			this.notificationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.notificationLabel.IsFontBold = true;
			this.notificationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 1, true);
			this.notificationLabel.Name = "notificationLabel";
			this.notificationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 31, true);
			this.notificationLabel.TabIndex = 5;
			this.notificationLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// sendOption
			// 
			this.sendOption.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.sendOption, "SendMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SendMessage)));
			this.sendOption.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("C30656E3-5105-42B1-9AE6-C695BC7CE5C7", "Send an amendment message NOW.");
			this.sendOption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sendOption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 35, true);
			this.sendOption.Name = "sendOption";
			this.sendOption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 20, true);
			this.sendOption.TabIndex = 6;
			this.sendOption.TabStop = true;
			this.sendOption.UseVisualStyleBackColor = true;
			// 
			// discardOption
			// 
			this.discardOption.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.discardOption, "SaveWithoutSendMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper)(null)).SaveWithoutSendMessage)));
			this.discardOption.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("A7B8A741-4931-413C-978B-6A3CBDEF4E52", "Save WITHOUT sending amendment. Changes do NOT affect Customs Entry.");
			this.discardOption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.discardOption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 243, true);
			this.discardOption.Name = "discardOption";
			this.discardOption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 20, true);
			this.discardOption.TabIndex = 7;
			this.discardOption.TabStop = true;
			this.discardOption.UseVisualStyleBackColor = true;
			// 
			// CADCorrectionMessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1B438A7C-DF0F-48A8-B2D0-1BE0157F2330", "Saving Options");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 329, true);
			this.Controls.Add(this.discardOption);
			this.Controls.Add(this.sendOption);
			this.Controls.Add(this.notificationLabel);
			this.Controls.Add(this.SendingActionsGroupBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.CA.Business";
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper);
			this.DataSourceTypeName = "Enterprise.Customs.CA.Business.CADCorrectionMessageSendingActionWrapper";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 368, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 368, true);
			this.Name = "CADCorrectionMessageSendingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.SendingActionsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.notificationLabel, 0);
			this.Controls.SetChildIndex(this.sendOption, 0);
			this.Controls.SetChildIndex(this.discardOption, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendingActionsGroupBox.ResumeLayout(false);
			this.SendingActionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SendingActionsGrid)).EndInit();
			this.SendingActionsGrid.ResumeLayout(false);
			this.SendingActionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SendingActionsGroupBox;
		internal Enterprise.ZArchitecture.ZGrid SendingActionsGrid;
		private Enterprise.ZArchitecture.ZLabel notificationLabel;
		private Enterprise.ZArchitecture.GUI.ZRadioButton sendOption;
		private Enterprise.ZArchitecture.GUI.ZRadioButton discardOption;
	}
}
