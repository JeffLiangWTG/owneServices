using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class AUCMRMessageUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.EntriesEntryLinesSplitter = new CargoWise.Windows.UI.KSplitter();
			this.EntryHeaderInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SubjectToRedLineLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ATDCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ATDCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomsPaymentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsPaymentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EntryAdviceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryAdviceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffCodeLabel = new Enterprise.ZArchitecture.ZLabel();
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
			this.EntryHeaderInfoGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 135, true);
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.ReadOnly = false;
			this.EntriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 116, true);
			// 
			// MainHorizontalSplitContainer
			// 
			this.MainHorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 664, true);
			this.MainHorizontalSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.MainHorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(135);
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 526, true);
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 499, true);
			// 
			// EntryLineGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Line Status";
			zTextBoxColumnStyleInfo1.ColumnName = "LineSubmissionStatusDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.Caption = "Consolidated Entry Line No.";
			zCalcEditColumnStyleInfo7.ColumnName = "ZA_AggregateEntryLineNumber";
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.Caption = "Tariff";
			zTextBoxColumnStyleInfo2.ColumnName = "CL_AdValoremTariff";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Description";
			zTextBoxColumnStyleInfo3.ColumnName = "EffectiveDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "EntryLineAddInfo.Lookups.ZA_RRC_List";
			zDropEditColumnStyleInfo1.Caption = "Refund Reason";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "RefundReasonCode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMessage|3560e308-f851-4c46-a24d-675afd84cfb4", "Refund");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Total Duty Tax Advised In Last Clearance Message";
			zCalcEditColumnStyleInfo1.ColumnName = "TotalDutyTaxAdvisedInLastClearanceMessage";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMessage|3560e308-f851-4c46-a24d-675afd84cfb4", "Refund");
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Current Total Duty Tax";
			zCalcEditColumnStyleInfo2.ColumnName = "CurrentTotalDutyTax";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMessage|3560e308-f851-4c46-a24d-675afd84cfb4", "Refund");
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Duty Percent";
			zCalcEditColumnStyleInfo3.ColumnName = "CL_DutyPercent";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Flat Rate Description";
			zTextBoxColumnStyleInfo4.ColumnName = "FlatRateDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Customs Value";
			zCalcEditColumnStyleInfo4.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "No Quarantine Concern";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsNonAQISAEPLine";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Security Amount";
			zCalcEditColumnStyleInfo5.ColumnName = "SecurityConcessionAmount";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "Security Uncollected";
			zCalcEditColumnStyleInfo6.ColumnName = "SecurityLiabilityAmount";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EntryLineGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 376, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeLabel);
			this.ExtendedInfoGroupBox.Controls.Add(this.zTextBox1);
			this.ExtendedInfoGroupBox.Controls.Add(this.zLabel1);
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 379, true);
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 117, true);
			this.ExtendedInfoGroupBox.Visible = true;
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 499, true);
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.TopVerticalSplitContainer.IsSplitterFixed = true;
			// 
			// TopVerticalSplitContainer.Panel2
			// 
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.EntryHeaderInfoGroupBox);
			this.TopVerticalSplitContainer.Panel2Collapsed = false;
			this.TopVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 135, true);
			this.TopVerticalSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 493, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// EntriesEntryLinesSplitter
			// 
			this.EntriesEntryLinesSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntriesEntryLinesSplitter.DoNotSaveSplitterLayout = false;
			this.EntriesEntryLinesSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.EntriesEntryLinesSplitter.Name = "EntriesEntryLinesSplitter";
			this.EntriesEntryLinesSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 3, true);
			this.EntriesEntryLinesSplitter.TabIndex = 10;
			this.EntriesEntryLinesSplitter.TabStop = false;
			// 
			// EntryHeaderInfoGroupBox
			// 
			this.EntryHeaderInfoGroupBox.Controls.Add(this.SubjectToRedLineLabel);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.ATDCodeTextBox);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.ATDCodeLabel);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.CustomsPaymentTextBox);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.CustomsPaymentLabel);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.EntryAdviceTextBox);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.EntryAdviceLabel);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.EntryHeaderInfoGroupBox.Controls.Add(this.MessageStatusLabel);
			this.EntryHeaderInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryHeaderInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryHeaderInfoGroupBox.Name = "EntryHeaderInfoGroupBox";
			this.EntryHeaderInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 135, true);
			this.EntryHeaderInfoGroupBox.TabIndex = 0;
			this.EntryHeaderInfoGroupBox.TabStop = false;
			this.EntryHeaderInfoGroupBox.Text = "Entry Header Information";
			// 
			// SubjectToRedLineLabel
			// 
			this.SubjectToRedLineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubjectToRedLineLabel, "CustomsEntryHeaders.SubjectToRedLineProcessing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubjectToRedLineProcessing)));
			this.SubjectToRedLineLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SubjectToRedLineLabel.ForeColor = System.Drawing.Color.Red;
			this.SubjectToRedLineLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 64, true);
			this.SubjectToRedLineLabel.Name = "SubjectToRedLineLabel";
			this.SubjectToRedLineLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
			this.SubjectToRedLineLabel.TabIndex = 8;
			// 
			// ATDCodeTextBox
			// 
			this.ATDCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ATDCodeTextBox, "CustomsEntryHeaders.ATDSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ATDSecurityCode)));
			this.ATDCodeTextBox.CaptionResourceString = null;
			this.ATDCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ATDCodeTextBox, false);
			this.ATDCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 43, true);
			this.ATDCodeTextBox.Name = "ATDCodeTextBox";
			this.ATDCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.ATDCodeTextBox.TabIndex = 5;
			// 
			// ATDCodeLabel
			// 
			this.ATDCodeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ATDCodeLabel.AutoSize = true;
			this.ATDCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ATDCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 46, true);
			this.ATDCodeLabel.Name = "ATDCodeLabel";
			this.ATDCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.ATDCodeLabel.TabIndex = 4;
			this.ATDCodeLabel.Text = "ATD Code:";
			// 
			// CustomsPaymentTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsPaymentTextBox, "CustomsEntryHeaders.PaymentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).PaymentStatus)));
			this.CustomsPaymentTextBox.CaptionResourceString = null;
			this.CustomsPaymentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsPaymentTextBox, false);
			this.CustomsPaymentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 64, true);
			this.CustomsPaymentTextBox.Name = "CustomsPaymentTextBox";
			this.CustomsPaymentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.CustomsPaymentTextBox.TabIndex = 7;
			// 
			// CustomsPaymentLabel
			// 
			this.CustomsPaymentLabel.AutoSize = true;
			this.CustomsPaymentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomsPaymentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 66, true);
			this.CustomsPaymentLabel.Name = "CustomsPaymentLabel";
			this.CustomsPaymentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.CustomsPaymentLabel.TabIndex = 6;
			this.CustomsPaymentLabel.Text = "Customs Pay:";
			// 
			// EntryAdviceTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryAdviceTextBox, "CustomsEntryHeaders.ImportEntryAdvice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ImportEntryAdvice)));
			this.EntryAdviceTextBox.CaptionResourceString = null;
			this.EntryAdviceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EntryAdviceTextBox, false);
			this.EntryAdviceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 41, true);
			this.EntryAdviceTextBox.Name = "EntryAdviceTextBox";
			this.EntryAdviceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.EntryAdviceTextBox.TabIndex = 3;
			// 
			// EntryAdviceLabel
			// 
			this.EntryAdviceLabel.AutoSize = true;
			this.EntryAdviceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EntryAdviceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 44, true);
			this.EntryAdviceLabel.Name = "EntryAdviceLabel";
			this.EntryAdviceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.EntryAdviceLabel.TabIndex = 2;
			this.EntryAdviceLabel.Text = "Entry Advice :";
			// 
			// MessageStatusTextBox
			// 
			this.MessageStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "CustomsEntryHeaders.MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MessageStatusDescription)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.MessageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageStatusTextBox, false);
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 19, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 20, true);
			this.MessageStatusTextBox.TabIndex = 1;
			// 
			// MessageStatusLabel
			// 
			this.MessageStatusLabel.AutoSize = true;
			this.MessageStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.MessageStatusLabel.Name = "MessageStatusLabel";
			this.MessageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 13, true);
			this.MessageStatusLabel.TabIndex = 0;
			this.MessageStatusLabel.Text = "Message Status :";
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.zTextBox1.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 41, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 70, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 44, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Description:";
			// 
			// TariffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TariffCodeTextBox, false);
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 15, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.TariffCodeTextBox.TabIndex = 1;
			// 
			// TariffCodeLabel
			// 
			this.TariffCodeLabel.AutoSize = true;
			this.TariffCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TariffCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.TariffCodeLabel.Name = "TariffCodeLabel";
			this.TariffCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.TariffCodeLabel.TabIndex = 0;
			this.TariffCodeLabel.Text = "Tariff Code:";
			// 
			// AUCMRMessageUserControl
			// 
			this.Name = "AUCMRMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 664, true);
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
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryHeaderInfoGroupBox.ResumeLayout(false);
			this.EntryHeaderInfoGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitter EntriesEntryLinesSplitter;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EntryHeaderInfoGroupBox;
		private ZLabel SubjectToRedLineLabel;
		protected internal ZTextBox ATDCodeTextBox;
		internal ZLabel ATDCodeLabel;
		protected internal ZTextBox CustomsPaymentTextBox;
		internal ZLabel CustomsPaymentLabel;
		protected internal ZTextBox EntryAdviceTextBox;
		internal ZLabel EntryAdviceLabel;
		protected internal ZTextBox MessageStatusTextBox;
		internal ZLabel MessageStatusLabel;
		private ZTextBox zTextBox1;
		private ZLabel zLabel1;
		private ZTextBox TariffCodeTextBox;
		private ZLabel TariffCodeLabel;
	}
}
