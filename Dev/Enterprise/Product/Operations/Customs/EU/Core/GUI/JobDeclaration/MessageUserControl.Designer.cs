namespace Enterprise.Customs.EU.GUI
{
	partial class MessageUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AmendmentReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AmendmentReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NewEntryDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryDetailsUserControl = new EntryDetailsUserControl();
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
			this.NewEntryDetailsTabPage.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 65, true);
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Controls.Add(this.NewEntryDetailsTabPage);
			// 
			// NewEntryDetailsTabPage
			// 
			this.NewEntryDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("75D174E1-0625-4734-899D-42518A62B792", "Entry");
			this.NewEntryDetailsTabPage.Controls.Add(this.EntryDetailsUserControl);
			this.NewEntryDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewEntryDetailsTabPage.Name = "NewEntryDetailsTabPage";
			this.NewEntryDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 457, true);
			this.NewEntryDetailsTabPage.TabIndex = 4;
			//
			// EntryDetailsUserControl
			//
			this.EntryDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryDetailsUserControl.Name = "EntryDetailsUserControl";
			this.EntryDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 281, true);
			this.EntryDetailsUserControl.TabIndex = 0;
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.Size = this.EntriesBoundGrid.Size;
			this.EntriesBoundGrid.TabIndex = 1;
			// 
			// MainHorizontalSplitContainer
			// 
			// 
			// EntryLineGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7278E8B0-DD27-42B5-8ED5-33427B3A91E8", "Line Status");
			zTextBoxColumnStyleInfo1.ColumnName = "LineSubmissionStatusDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("B46C97AC-89F4-45DE-8C58-5DDDD24F9A1E", "Tariff");
			zTextBoxColumnStyleInfo2.ColumnName = "FormattedTariff";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9AD3697F-ACA2-48C0-A2C7-F10422E8195A", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "EffectiveDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("699F7CB1-04CA-48E3-A2FA-C468F568966B", "Total Duty Tax");
			zCalcEditColumnStyleInfo1.ColumnName = "DutyAmount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("67136D17-68D2-494B-BBCA-3CC242776FFF", "Duty Percent");
			zCalcEditColumnStyleInfo2.ColumnName = "CL_DutyPercent";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("24947F4C-68D2-4269-84FB-240D96C929ED", "VAT Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "GSTVATAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("58CB798F-A7C3-4595-97F2-943F79B7225A", "VAT Def.");
			zCalcEditColumnStyleInfo4.ColumnName = "GSTVATDeferred";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("956EB803-70A7-49D3-8806-312FFD99D853", "Customs Value");
			zCalcEditColumnStyleInfo5.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("B70079CE-2EF2-4C2D-95F0-4F2237B62944", "Statistical Value");
			zCalcEditColumnStyleInfo6.ColumnName = "CL_StatisticalValue";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 333, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ExtendedInfoGroupBox.Visible = true;
			// 
			// TopVerticalSplitContainer
			// 
			// 
			// TopVerticalSplitContainer.Panel2
			// 
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.AmendmentReasonLabel);
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.AmendmentReasonTextBox);
			this.TopVerticalSplitContainer.Panel2Collapsed = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// TariffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c6f55bc1-26b1-4ef3-b2c9-2e83b75f2b1a", "Tariff Code:");
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 13, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.TariffCodeTextBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c6f55bc1-26b1-4ef3-b2c9-2e83b75f3c2b", "Description:");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 37, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 72, true);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// AmendmentReasonTextBox
			// 
			this.AmendmentReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AmendmentReasonTextBox, "CustomsEntryHeaders.CH_CustomsMessageRemarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_CustomsMessageRemarks)));
			this.AmendmentReasonTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AmendmentReasonTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AmendmentReasonTextBox, false);
			this.AmendmentReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.AmendmentReasonTextBox.Multiline = true;
			this.AmendmentReasonTextBox.Name = "AmendmentReasonTextBox";
			this.AmendmentReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 40, true);
			this.AmendmentReasonTextBox.TabIndex = 1;
			// 
			// AmendmentReasonLabel
			// 
			this.AmendmentReasonLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("424D331A-9421-4E31-A153-B5146DCB96BF", "Reason For Amendment / Invalidation");
			this.AmendmentReasonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AmendmentReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.AmendmentReasonLabel.Name = "AmendmentReasonLabel";
			this.AmendmentReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 13, true);
			this.AmendmentReasonLabel.TabIndex = 0;
			// 
			// MessageUserControl
			// 
			this.Name = "MessageUserControl";
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
			this.NewEntryDetailsTabPage.ResumeLayout(false);
			this.NewEntryDetailsTabPage.PerformLayout();
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

		Enterprise.ZArchitecture.ZTextBox TariffCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		protected Enterprise.ZArchitecture.ZTextBox AmendmentReasonTextBox;
		protected Enterprise.ZArchitecture.ZLabel AmendmentReasonLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage NewEntryDetailsTabPage;
		protected EntryDetailsUserControl EntryDetailsUserControl;
	}
}
