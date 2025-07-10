using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class InvalidStagePopupForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.warnningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.stageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.groupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.broadcastCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.stageGrid)).BeginInit();
			this.stageGrid.SuspendLayout();
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 428, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup);
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 1;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.messageLabel, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.stageGrid, 0, 1);
			this.tableLayoutPanel.Controls.Add(this.groupBox, 0, 2);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 3;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24F));
			this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 428, true);
			this.tableLayoutPanel.TabIndex = 2;
			// 
			// messageLabel
			// 
			this.messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 128, true);
			this.messageLabel.TabIndex = 3;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.messageLabel.UseMnemonic = false;
			// 
			// stageGrid
			// 
			this.stageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.stageGrid, "LatestStages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).DescriptionOnGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).ControlIncidents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).IncidentCompleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).GroupCompleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).TriggerOn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).CascadeCriticality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LatestStages)).SyncRoot)).CascadeProductDetails)));
			this.stageGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "DescriptionOnGroup";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "ControlIncidents";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "IncidentCompleted";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "GroupCompleted";
			zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "TriggerOn";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "CascadeCriticality";
			zCheckBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.ColumnName = "CascadeProductDetails";
			zCheckBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.stageGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.stageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.stageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.stageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.stageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.stageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.stageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.stageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.stageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.stageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stageGrid.GridId = "06cd73b4-41c1-4e52-858c-58e10ae128dd";
			this.stageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.stageGrid.IsWholeRowSelectedOnClick = true;
			this.stageGrid.LayoutKey = "stageGrid";
			this.stageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 129, true);
			this.stageGrid.Name = "stageGrid";
			this.stageGrid.ReadOnly = true;
			this.stageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 193, true);
			this.stageGrid.TabIndex = 4;
			this.stageGrid.SelectedRowsChangedInMouseDown += stageGrid_SelectedRowsChangedInMouseDown;
			this.stageGrid.KeyUp += stageGrid_KeyUp;
			// 
			// groupBox
			// 
			this.groupBox.Controls.Add(this.broadcastCheckbox);
			this.groupBox.Controls.Add(this.warnningLabel);
			this.groupBox.Controls.Add(this.okButton);
			this.groupBox.Controls.Add(this.cancelButton);
			this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 326, true);
			this.groupBox.Name = "groupBox";
			this.groupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 100, true);
			this.groupBox.TabIndex = 5;
			this.groupBox.TabStop = false;
			// 
			// broadcastCheckbox
			// 
			this.broadcastCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.broadcastCheckbox, "BroadcastFromPopupForm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).BroadcastFromPopupForm)));
			this.broadcastCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 10, true);
			this.broadcastCheckbox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.broadcastCheckbox.Name = "broadcastCheckbox";
			this.broadcastCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 25, true);
			this.broadcastCheckbox.TabIndex = 5;
			this.broadcastCheckbox.Text = "Send Opening Broadcast";
			this.broadcastCheckbox.UseVisualStyleBackColor = true;
			// 
			// warnningLabel
			// 
			this.warnningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.warnningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.warnningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 15, true);
			this.warnningLabel.Name = "warnningLabel";
			this.warnningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 25, true);
			this.warnningLabel.TabIndex = 6;
			this.warnningLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.warnningLabel.UseMnemonic = false;
			this.warnningLabel.Text = "Warning: No published broadcast message.";
			this.warnningLabel.Visible = false;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("InvalidStagePopupForm|c6706aae-68d6-4772-af35-8cfb48c2cc34", "OK");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.IsCaptionOverridden = true;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 57, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.ToolTipCaption = null;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("InvalidStagePopupForm|388cfa55-44aa-46bc-a23c-c39134acab6f", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = true;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 57, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ToolTipCaption = null;
			// 
			// InvalidStagePopupForm
			//
			this.AcceptButton = this.okButton;
			this.CancelButton = this.cancelButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 452, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 452, true);
			this.Controls.Add(this.tableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup);
			this.Name = "InvalidStagePopupForm";
			this.Text = "Invalid Stage";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.stageGrid)).EndInit();
			this.stageGrid.ResumeLayout(false);
			this.stageGrid.PerformLayout();
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		
		#endregion
		protected KTableLayoutPanel tableLayoutPanel;
		protected ZGrid stageGrid;
		protected ZLabel messageLabel;
		protected ZLabel warnningLabel;
		protected ZCheckBox broadcastCheckbox;
		protected ZGroupBox groupBox;
		protected ZButton okButton;
		protected ZButton cancelButton;
	}
}
