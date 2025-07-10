namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRMessageSendingActionForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.BillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.VesselInformationUserControl = new Enterprise.Customs.JP.AFR.GUI.JPAFRVesselInformationUserControl();
			this.HasATDBeenSentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).BeginInit();
			this.BillsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.MainControlPanel.SuspendLayout();
			this.VesselInformationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 490, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.MessageSendingAction);
			// 
			// BillsGroupBox
			// 
			this.BillsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BillsGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("01f45fa1-d995-4a33-bb8a-33d013f0491c", "Bills");
			this.BillsGroupBox.Controls.Add(this.BillsGrid);
			this.BillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 285, true);
			this.BillsGroupBox.Name = "BillsGroupBox";
			this.BillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 162, true);
			this.BillsGroupBox.TabIndex = 4;
			this.BillsGroupBox.TabStop = false;
			// 
			// BillsGrid
			// 
			this.BillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillsGrid, "MessageSendingObjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_BillOfLadingNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_Send)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_ActionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_ReleaseStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_ReleaseStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_DeleteReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).MessageSendingObjects)).SyncRoot)).JPM_DeleteReasonText)));
			this.BillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JPM_BillOfLadingNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zCheckBoxColumnStyleInfo1.ColumnName = "JPM_Send";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "JPM_ActionCode";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JPM_ReleaseStatusDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "JPM_MessageStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "JPM_ReleaseStatus";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "JPM_MessageStatus";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zDropEditColumnStyleInfo2.ColumnName = "JPM_DeleteReasonCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JPM_DeleteReasonText";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.BillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGrid.GridId = "7f9ffd73-2f0b-43ac-a6a3-b5790d2b08bb";
			this.BillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillsGrid.LayoutKey = "BillsGrid";
			this.BillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BillsGrid.Name = "BillsGrid";
			this.BillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 143, true);
			this.BillsGrid.TabIndex = 5;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.cancelButton);
			this.BottomPanel.Controls.Add(this.SendButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 453, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 37, true);
			this.BottomPanel.TabIndex = 6;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("fe0dd80a-2301-4ba5-b188-3ff831b4613a", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 6, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("8d4020bd-23cd-4b95-83e7-1500c2113874", "&Send");
			this.SendButton.IsCaptionOverridden = false;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 6, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.SendButton.TabIndex = 7;
			this.SendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// MainControlPanel
			// 
			this.MainControlPanel.Controls.Add(this.VesselInformationUserControl);
			this.MainControlPanel.Controls.Add(this.HasATDBeenSentCheckBox);
			this.MainControlPanel.Controls.Add(this.DescriptionLabel);
			this.MainControlPanel.Controls.Add(this.BillsGroupBox);
			this.MainControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainControlPanel.Name = "MainControlPanel";
			this.MainControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 453, true);
			this.MainControlPanel.TabIndex = 1;
			// 
			// VesselInformationUserControl
			// 
			this.VesselInformationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselInformationUserControl, "Header");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).Header)));
			this.VesselInformationUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.VesselInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VesselInformationUserControl.Name = "VesselInformationUserControl";
			this.VesselInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 180, true);
			this.VesselInformationUserControl.TabIndex = 2;
			// 
			// HasATDBeenSentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HasATDBeenSentCheckBox, "HasATDBeenSent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).HasATDBeenSent)));
			this.HasATDBeenSentCheckBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("608ce943-d200-40c0-abb7-930ba10d4e27", "Has Carrier Sent The ATD Message?");
			this.HasATDBeenSentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasATDBeenSentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 186, true);
			this.HasATDBeenSentCheckBox.Name = "HasATDBeenSentCheckBox";
			this.HasATDBeenSentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 24, true);
			this.HasATDBeenSentCheckBox.TabIndex = 3;
			this.HasATDBeenSentCheckBox.UseVisualStyleBackColor = true;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DescriptionLabel.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("4B00E499-8546-42B5-842A-F11AED7EE72F", "In order to change the House Bill level information you have made in the Bill tab, you just need to select the corresponding action from below grid leaving the above sections unchanged. If you want to correct the Vessel Information or change the Master level information which will affect all the bills, please make the modification above and the grid below will be updated to corresponding message action.");
			this.DescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 213, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 69, true);
			this.DescriptionLabel.TabIndex = 5;
			// 
			// JPAFRMessageSendingActionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 514, true);
			this.Controls.Add(this.MainControlPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.MessageSendingAction);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 550, true);
			this.Name = "JPAFRMessageSendingActionForm";
			this.Text = "JPAFRMessageSendingActionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillsGroupBox.ResumeLayout(false);
			this.BillsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).EndInit();
			this.BillsGrid.ResumeLayout(false);
			this.BillsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MainControlPanel.ResumeLayout(false);
			this.MainControlPanel.PerformLayout();
			this.VesselInformationUserControl.ResumeLayout(true);
			this.VesselInformationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid BillsGrid;
		private ZArchitecture.GUI.ZGroupBox BillsGroupBox;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton SendButton;
		private JPAFRVesselInformationUserControl VesselInformationUserControl;
		private ZArchitecture.GUI.ZPanel MainControlPanel;
		private ZArchitecture.GUI.ZCheckBox HasATDBeenSentCheckBox;
		private ZArchitecture.ZLabel DescriptionLabel;
	}
}
