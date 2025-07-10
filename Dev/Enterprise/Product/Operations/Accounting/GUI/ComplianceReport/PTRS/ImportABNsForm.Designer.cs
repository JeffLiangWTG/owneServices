using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.PaymentTimesReportingScheme;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.PTRS
{
	public partial class ImportABNsForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.ImportFromFileButton = new ZButton();
			this.CloseButton = new ZButton();
			this.ProgressTextBox = new WhiteTextBox();
			this.SubmitButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 527, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 26, true);
			this.MainStatusBar.SizingGrip = false;
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
			this.BindingSource.DataSourceType = typeof(Enterprise.DataTransfer.Business.DataImporterBusinessObject);
			// 
			// ImportFromFileButton
			// 
			this.ImportFromFileButton.AutoSize = true;
			this.ImportFromFileButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportABNsForm|6B2395DC-2526-458B-ACF2-FA86017D4276", "Import ABNs");
			this.ImportFromFileButton.IsCaptionOverridden = false;
			this.ImportFromFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 8, true);
			this.ImportFromFileButton.Name = "ImportFromFileButton";
			this.ImportFromFileButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportFromFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 26, true);
			this.ImportFromFileButton.TabIndex = 0;
			this.ImportFromFileButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ImportFromFileButton.ToolTipCaption = null;
			this.ImportFromFileButton.Click += new EventHandler(this.ImportFromFile_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportABNsForm|DF4A711B-D410-48BE-85A6-187DE216D2BB", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(602, 8, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 26, true);
			this.CloseButton.TabIndex = 12;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 37, true);
			this.ProgressTextBox.Multiline = true;
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 484, true);
			this.ProgressTextBox.TabIndex = 5;
			// 
			// SubmitButton
			// 
			this.SubmitButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SubmitButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportABNsForm|8A11F7FF-44CB-4623-A78D-023C72137C5F", "Submit");
			this.SubmitButton.IsCaptionOverridden = false;
			this.SubmitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 8, true);
			this.SubmitButton.Name = "SubmitButton";
			this.SubmitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SubmitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 26, true);
			this.SubmitButton.TabIndex = 13;
			this.SubmitButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SubmitButton.ToolTipCaption = null;
			this.SubmitButton.Enabled = false;
			this.SubmitButton.Click += new EventHandler(this.SubmitButton_Click);
			// 
			// ImportABNsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportABNsForm|5440F932-3EE9-43EE-99F0-6198D0DEC5DA", "Accounting ABN Small Business List Import");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Controls.Add(this.SubmitButton);
			this.Controls.Add(this.ProgressTextBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ImportFromFileButton);
			this.DataSourceAssemblyName = "Enterprise.DataTransfer";
			this.DataSourceType = typeof(Enterprise.DataTransfer.Business.DataImporterBusinessObject);
			this.DataSourceTypeName = "Enterprise.DataTransfer.Business.DataImporterBusinessObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ImportABNsForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ImportFromFileButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ProgressTextBox, 0);
			this.Controls.SetChildIndex(this.SubmitButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}