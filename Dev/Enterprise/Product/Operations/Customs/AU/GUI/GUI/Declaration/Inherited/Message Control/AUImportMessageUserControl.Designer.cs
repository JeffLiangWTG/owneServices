namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class AUImportMessageUserControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.EntryAdviceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryAdviceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.TariffCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
			this.EntriesBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
			this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
			this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
			this.MainHorizontalSplitContainer.SuspendLayout();
			this.EntryLinesMessagesTabControl.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
			this.EntryLineGrid.SuspendLayout();
			this.ExtendedInfoGroupBox.SuspendLayout();
			this.MessageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.Panel2.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 95, true);
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 76, true);
			// 
			// MainHorizontalSplitContainer
			// 
			this.MainHorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 592, true);
			this.MainHorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(95);
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 494, true);
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 467, true);
			// 
			// EntryLineGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Line Status";
			zTextBoxColumnStyleInfo1.ColumnName = "LineSubmissionStatusDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Tariff";
			zTextBoxColumnStyleInfo2.ColumnName = "CL_AdValoremTariff";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Description";
			zTextBoxColumnStyleInfo3.ColumnName = "CL_Description";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Refund Reason";
			zTextBoxColumnStyleInfo4.ColumnName = "RefundReasonCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Total Duty Tax Advised In Last Clearance Message";
			zCalcEditColumnStyleInfo1.ColumnName = "TotalDutyTaxAdvisedInLastClearanceMessage";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Current Total Duty Tax";
			zCalcEditColumnStyleInfo2.ColumnName = "CurrentTotalDutyTax";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Duty Percent";
			zCalcEditColumnStyleInfo3.ColumnName = "CL_DutyPercent";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Flat Rate Description";
			zTextBoxColumnStyleInfo5.ColumnName = "FlatRateDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Customs Value";
			zCalcEditColumnStyleInfo4.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 344, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeLabel);
			this.ExtendedInfoGroupBox.Controls.Add(this.zTextBox1);
			this.ExtendedInfoGroupBox.Controls.Add(this.zLabel1);
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 347, true);
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 117, true);
			this.ExtendedInfoGroupBox.Visible = true;
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 433, true);
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.TopVerticalSplitContainer.IsSplitterFixed = true;
			// 
			// TopVerticalSplitContainer.Panel2
			// 
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.EntryAdviceTextBox);
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.EntryAdviceLabel);
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.MessageStatusTextBox);
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.MessageStatusLabel);
			this.TopVerticalSplitContainer.Panel2Collapsed = false;
			this.TopVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 95, true);
			this.TopVerticalSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(736);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 427, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// EntryAdviceTextBox
			// 
			this.EntryAdviceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EntryAdviceTextBox, "CustomsEntryHeaders.ImportEntryAdvice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ImportEntryAdvice)));
			this.EntryAdviceTextBox.CaptionResourceString = null;
			this.EntryAdviceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EntryAdviceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 31, true);
			this.EntryAdviceTextBox.Name = "EntryAdviceTextBox";
			this.EntryAdviceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.EntryAdviceTextBox.TabIndex = 3;
			// 
			// EntryAdviceLabel
			// 
			this.EntryAdviceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EntryAdviceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 31, true);
			this.EntryAdviceLabel.Name = "EntryAdviceLabel";
			this.EntryAdviceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
			this.EntryAdviceLabel.TabIndex = 2;
			this.EntryAdviceLabel.Text = "Entry Advice :";
			// 
			// MessageStatusTextBox
			// 
			this.MessageStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MessageStatusTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "CustomsEntryHeaders.MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MessageStatusDescription)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.MessageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 7, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.MessageStatusTextBox.TabIndex = 1;
			// 
			// MessageStatusLabel
			// 
			this.MessageStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 7, true);
			this.MessageStatusLabel.Name = "MessageStatusLabel";
			this.MessageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.MessageStatusLabel.TabIndex = 0;
			this.MessageStatusLabel.Text = "Message Status :";
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 39, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 70, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Description:";
			// 
			// TariffCodeLabel
			// 
			this.TariffCodeLabel.AutoSize = true;
			this.TariffCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TariffCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TariffCodeLabel.Name = "TariffCodeLabel";
			this.TariffCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.TariffCodeLabel.TabIndex = 0;
			this.TariffCodeLabel.Text = "Tariff Code:";
			// 
			// TariffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = null;
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 13, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.TariffCodeTextBox.TabIndex = 1;
			// 
			// AUImportMessageUserControl
			// 
			this.Name = "AUImportMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 592, true);
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
			this.EntriesBoundGrid.ResumeLayout(false);
			this.EntriesBoundGrid.PerformLayout();
			this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
			this.MainHorizontalSplitContainer.ResumeLayout(false);
			this.MainHorizontalSplitContainer.PerformLayout();
			this.EntryLinesMessagesTabControl.ResumeLayout(false);
			this.EntryLinesMessagesTabControl.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
			this.EntryLineGrid.ResumeLayout(false);
			this.EntryLineGrid.PerformLayout();
			this.ExtendedInfoGroupBox.ResumeLayout(false);
			this.ExtendedInfoGroupBox.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
			this.TopVerticalSplitContainer.Panel2.ResumeLayout(false);
			this.TopVerticalSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal Enterprise.ZArchitecture.ZTextBox EntryAdviceTextBox;
		private Enterprise.ZArchitecture.ZLabel EntryAdviceLabel;
		protected internal Enterprise.ZArchitecture.ZTextBox MessageStatusTextBox;
		private Enterprise.ZArchitecture.ZLabel MessageStatusLabel;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZTextBox TariffCodeTextBox;
		private Enterprise.ZArchitecture.ZLabel TariffCodeLabel;
	}
}
