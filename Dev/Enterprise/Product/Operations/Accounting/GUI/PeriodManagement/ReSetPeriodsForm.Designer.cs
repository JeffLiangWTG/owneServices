using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DataPurge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ReSetPeriodsForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ReSetPeriodsGroupBox = new ZGroupBox();
			this.extendLastFinancialYearButton = new ZButton();
			this.extendLastFinancialYearLabel = new ZLabel();
			this.deletePeriodsRangeButton = new ZButton();
			this.deletePeriodsFromSpecifiedDateLabel = new ZLabel();
			this.updateTaxGLMovementRecordsButton = new ZButton();
			this.updateTaxGLMovementRecordsLabel = new ZLabel();
			this.updateGeneralLedgerRecordsButton = new ZButton();
			this.updateGeneralLedgerRecordsLabel = new ZLabel();
			this.reAggregateButton = new ZButton();
			this.LogOutLabel = new ZLabel();
			this.reAggregateLabel = new ZLabel();
			this.CreatePeriodsButton = new ZButton();
			this.CreatePeriodsLabel = new ZLabel();
			this.DeletePeriodsButton = new ZButton();
			this.DeletePeriodsLabel = new ZLabel();
			this.BackUpLabel = new ZLabel();
			this.BackUpButton = new ZButton();
			this.bottomPanel = new ZPanel();
			this.CloseButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReSetPeriodsGroupBox.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 402, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 26, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(NewYearPeriodSettings);
			// 
			// ReSetPeriodsGroupBox
			// 
			this.ReSetPeriodsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|02eab025-f289-42f0-a325-3cb5c253f5ea", "Re-Set Periods Process");
			this.ReSetPeriodsGroupBox.Controls.Add(this.extendLastFinancialYearButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.extendLastFinancialYearLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.deletePeriodsRangeButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.deletePeriodsFromSpecifiedDateLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.updateTaxGLMovementRecordsButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.updateTaxGLMovementRecordsLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.updateGeneralLedgerRecordsButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.updateGeneralLedgerRecordsLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.reAggregateButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.LogOutLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.reAggregateLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.CreatePeriodsButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.CreatePeriodsLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.DeletePeriodsButton);
			this.ReSetPeriodsGroupBox.Controls.Add(this.DeletePeriodsLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.BackUpLabel);
			this.ReSetPeriodsGroupBox.Controls.Add(this.BackUpButton);
			this.ReSetPeriodsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReSetPeriodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ReSetPeriodsGroupBox.Name = "ReSetPeriodsGroupBox";
			this.ReSetPeriodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 367, true);
			this.ReSetPeriodsGroupBox.TabIndex = 0;
			this.ReSetPeriodsGroupBox.TabStop = false;
			// 
			// ExtendLastFinancialYearButton
			// 
			this.extendLastFinancialYearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("E5F663B0-B96E-4A81-B980-EBAAFB30CCB8", "Extend Last Financial Year");
			this.extendLastFinancialYearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 208, true);
			this.extendLastFinancialYearButton.Name = "ExtendLastFinancialYearButton";
			this.extendLastFinancialYearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 30, true);
			this.extendLastFinancialYearButton.TabIndex = 10;
			this.extendLastFinancialYearButton.ToolTipCaption = null;
			this.extendLastFinancialYearButton.Click += new EventHandler(this.ExtendLastFinancialYearButton_Click);
			// 
			// extendLastFinancialYearLabel
			// 
			this.extendLastFinancialYearLabel.AutoSize = true;
			this.extendLastFinancialYearLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|EA3CE9A4-BCE5-45C1-9D65-3011E3C4A186", "4b. Extend Last Financial Year");
			this.extendLastFinancialYearLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.extendLastFinancialYearLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 216, true);
			this.extendLastFinancialYearLabel.Name = "ExtendLastFinancialYearLabel";
			this.extendLastFinancialYearLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 14, true);
			this.extendLastFinancialYearLabel.TabIndex = 9;
			// 
			// DeletePeriodsFromButton
			// 
			this.deletePeriodsRangeButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|41B2F960-A979-4B4B-A2F0-C6127F1A8A1C", "Delete Periods From");
			this.deletePeriodsRangeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 132, true);
			this.deletePeriodsRangeButton.Name = "DeletePeriodsFromButton";
			this.deletePeriodsRangeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 30, true);
			this.deletePeriodsRangeButton.TabIndex = 6;
			this.deletePeriodsRangeButton.ToolTipCaption = null;
			this.deletePeriodsRangeButton.Click += new EventHandler(this.DeletePeriodsRangeButton_Click);
			// 
			// deletePeriodsFromSpecifiedDateLabel
			// 
			this.deletePeriodsFromSpecifiedDateLabel.AutoSize = true;
			this.deletePeriodsFromSpecifiedDateLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|A0FAC520-0FC3-4B28-9427-EAF295BA844A", "3b. Delete Periods From Specified Date");
			this.deletePeriodsFromSpecifiedDateLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.deletePeriodsFromSpecifiedDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 140, true);
			this.deletePeriodsFromSpecifiedDateLabel.Name = "DeletePeriodsFromLabel";
			this.deletePeriodsFromSpecifiedDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 14, true);
			this.deletePeriodsFromSpecifiedDateLabel.TabIndex = 5;
			// 
			// updateTaxGLMovementRecordsButton
			// 
			this.updateTaxGLMovementRecordsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("45e80cc1-e77a-4e5e-955b-8cb3176db46b", "Update Tax GL Movement Records");
			this.updateTaxGLMovementRecordsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 248, true);
			this.updateTaxGLMovementRecordsButton.Name = "updateTaxGLMovementRecordsButton";
			this.updateTaxGLMovementRecordsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 30, true);
			this.updateTaxGLMovementRecordsButton.TabIndex = 12;
			this.updateTaxGLMovementRecordsButton.ToolTipCaption = null;
			this.updateTaxGLMovementRecordsButton.Click += new EventHandler(this.updateTaxGLMovementRecordsButton_Click);
			// 
			// updateTaxGLMovementRecordsLabel
			// 
			this.updateTaxGLMovementRecordsLabel.AutoSize = true;
			this.updateTaxGLMovementRecordsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("78d7d823-b6bc-4e57-a35c-1fcd553506e7", "5. Set New Period on Tax GL Movement");
			this.updateTaxGLMovementRecordsLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.updateTaxGLMovementRecordsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 256, true);
			this.updateTaxGLMovementRecordsLabel.Name = "updateTaxGLMovementRecordsLabel";
			this.updateTaxGLMovementRecordsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 14, true);
			this.updateTaxGLMovementRecordsLabel.TabIndex = 11;
			// 
			// updateGeneralLedgerRecordsButton
			// 
			this.updateGeneralLedgerRecordsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("57F8EFE1-DE91-4964-BA74-ACCBBC01FFF1", "Update General Ledger Data Records");
			this.updateGeneralLedgerRecordsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 289, true);
			this.updateGeneralLedgerRecordsButton.Name = "updateGeneralLedgerRecordsButton";
			this.updateGeneralLedgerRecordsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 29, true);
			this.updateGeneralLedgerRecordsButton.TabIndex = 14;
			this.updateGeneralLedgerRecordsButton.ToolTipCaption = null;
			this.updateGeneralLedgerRecordsButton.Click += new EventHandler(this.updateGeneralLedgerRecordsButton_Click);
			// 
			// updateGeneralLedgerRecordsLabel
			// 
			this.updateGeneralLedgerRecordsLabel.AutoSize = true;
			this.updateGeneralLedgerRecordsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2158F74B-F743-4300-8ED1-63E90AB272A0", "6. Set New Period on GLD");
			this.updateGeneralLedgerRecordsLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.updateGeneralLedgerRecordsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 297, true);
			this.updateGeneralLedgerRecordsLabel.Name = "updateGeneralLedgerRecordsLabel";
			this.updateGeneralLedgerRecordsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 14, true);
			this.updateGeneralLedgerRecordsLabel.TabIndex = 13;
			this.updateGeneralLedgerRecordsLabel.UseMnemonic = false;
			// 
			// reAggregateButton
			// 
			this.reAggregateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|21fc6aec-415d-41bb-af71-b56a1ce1e8cb", "Re-Aggregate GL");
			this.reAggregateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 322, true);
			this.reAggregateButton.Name = "reAggregateButton";
			this.reAggregateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 30, true);
			this.reAggregateButton.TabIndex = 16;
			this.reAggregateButton.ToolTipCaption = null;
			this.reAggregateButton.Click += new EventHandler(this.reAggreateButton_Click);
			// 
			// LogOutLabel
			// 
			this.LogOutLabel.AutoSize = true;
			this.LogOutLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|402f19f3-905e-4a90-94a5-c6341589b2f1", "1. Logout All Users");
			this.LogOutLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LogOutLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 34, true);
			this.LogOutLabel.Name = "LogOutLabel";
			this.LogOutLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 14, true);
			this.LogOutLabel.TabIndex = 0;
			// 
			// reAggregateLabel
			// 
			this.reAggregateLabel.AutoSize = true;
			this.reAggregateLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|985d64a9-10ae-4822-88d2-a481796a3fa3", "7. Re-Aggregate GL");
			this.reAggregateLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.reAggregateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 330, true);
			this.reAggregateLabel.Name = "reAggregateLabel";
			this.reAggregateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 14, true);
			this.reAggregateLabel.TabIndex = 15;
			// 
			// CreatePeriodsButton
			// 
			this.CreatePeriodsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|f98bd28d-ba78-4756-bc2e-2fb05adf5ab2", "Create Periods");
			this.CreatePeriodsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 170, true);
			this.CreatePeriodsButton.Name = "CreatePeriodsButton";
			this.CreatePeriodsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 30, true);
			this.CreatePeriodsButton.TabIndex = 8;
			this.CreatePeriodsButton.ToolTipCaption = null;
			this.CreatePeriodsButton.Click += new EventHandler(this.CreatePeriodsButton_Click);
			// 
			// CreatePeriodsLabel
			// 
			this.CreatePeriodsLabel.AutoSize = true;
			this.CreatePeriodsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|8be07cda-913b-40ce-8668-e16d8d9f0a86", "4a. Create New Periods");
			this.CreatePeriodsLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CreatePeriodsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 178, true);
			this.CreatePeriodsLabel.Name = "CreatePeriodsLabel";
			this.CreatePeriodsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 14, true);
			this.CreatePeriodsLabel.TabIndex = 7;
			// 
			// DeletePeriodsButton
			// 
			this.DeletePeriodsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|35675e17-6c59-40c3-9ec0-d7a5f7d35f5c", "Delete Periods");
			this.DeletePeriodsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 95, true);
			this.DeletePeriodsButton.Name = "DeletePeriodsButton";
			this.DeletePeriodsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 30, true);
			this.DeletePeriodsButton.TabIndex = 4;
			this.DeletePeriodsButton.ToolTipCaption = null;
			this.DeletePeriodsButton.Click += new EventHandler(this.DeletePeriodsButton_Click);
			// 
			// DeletePeriodsLabel
			// 
			this.DeletePeriodsLabel.AutoSize = true;
			this.DeletePeriodsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|8b44e617-ab36-4016-98b0-08c5dca7734f", "3a. Delete All Existing Periods");
			this.DeletePeriodsLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DeletePeriodsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 103, true);
			this.DeletePeriodsLabel.Name = "DeletePeriodsLabel";
			this.DeletePeriodsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 14, true);
			this.DeletePeriodsLabel.TabIndex = 3;
			// 
			// BackUpLabel
			// 
			this.BackUpLabel.AutoSize = true;
			this.BackUpLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|889abd2b-2763-4736-9e20-9936884db80c", "2. Backup database");
			this.BackUpLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.BackUpLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 66, true);
			this.BackUpLabel.Name = "BackUpLabel";
			this.BackUpLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 14, true);
			this.BackUpLabel.TabIndex = 1;
			// 
			// BackUpButton
			// 
			this.BackUpButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|6668d171-7532-453d-9940-b8026875b3e9", "Open Backup Dialog");
			this.BackUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 58, true);
			this.BackUpButton.Name = "BackUpButton";
			this.BackUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 30, true);
			this.BackUpButton.TabIndex = 2;
			this.BackUpButton.ToolTipCaption = null;
			this.BackUpButton.Click += new EventHandler(this.BackUpButton_Click);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.CloseButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 369, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 33, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|28808f4c-81f0-4cf5-9b14-4e95ae8542bc", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 4, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 25, true);
			this.CloseButton.TabIndex = 0;
			this.CloseButton.ToolTipCaption = null;
			// 
			// ReSetPeriodsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReSetPeriodsForm|cc06c3c3-24d6-4c68-b4b5-a2a6c1204070", "Re-Set Periods");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 429, true);
			this.Controls.Add(this.ReSetPeriodsGroupBox);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(NewYearPeriodSettings);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.PeriodManagement.NewYearPeriodSettings";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ReSetPeriodsForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.ReSetPeriodsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReSetPeriodsGroupBox.ResumeLayout(false);
			this.ReSetPeriodsGroupBox.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}