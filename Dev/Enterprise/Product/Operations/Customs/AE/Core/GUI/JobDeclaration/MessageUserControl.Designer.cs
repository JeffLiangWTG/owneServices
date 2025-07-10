namespace Enterprise.Customs.AE.GUI
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
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
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.Size = this.EntriesBoundGrid.Size;
			// 
			// MainHorizontalSplitContainer
			// 
			// 
			// EntryLineGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("7B04B6BC-C062-4481-8A4C-FD2D46375402", "Line Status");
			zTextBoxColumnStyleInfo1.ColumnName = "LineSubmissionStatusDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("9B7F4D21-E5A8-4C38-B10D-F8ED64A73458", "Tariff");
			zTextBoxColumnStyleInfo2.ColumnName = "CL_AdValoremTariff";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString= Res.GetData("FB0DCDBA-329C-405D-8D17-B651F93C6E63", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "EffectiveDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("6AA890E9-D3E5-4FBE-8127-06C72E7817B3", "Total Duty Tax");
			zCalcEditColumnStyleInfo1.ColumnName = "DutyAmount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Res.GetData("259A86A2-88B1-4291-BB35-407DD1B2F1A8", "Duty Percent");
			zCalcEditColumnStyleInfo2.ColumnName = "CL_DutyPercent";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Res.GetData("B6396655-29FA-4F11-A25C-09FDA075DB5A", "Customs Value");
			zCalcEditColumnStyleInfo3.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 333, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeLabel);
			this.ExtendedInfoGroupBox.Controls.Add(this.zTextBox1);
			this.ExtendedInfoGroupBox.Controls.Add(this.zLabel1);
			this.ExtendedInfoGroupBox.Visible = true;
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Business.JobDeclaration);
			// 
			// TariffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TariffCodeTextBox, false);
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 13, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.TariffCodeTextBox.TabIndex = 1;
			// 
			// TariffCodeLabel
			// 
			this.TariffCodeLabel.AutoSize = true;
			this.TariffCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TariffCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TariffCodeLabel.Name = "TariffCodeLabel";
			this.TariffCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.TariffCodeLabel.TabIndex = 0;
			this.TariffCodeLabel.CaptionResourceString = Res.GetData("B3437AB7-DF8E-426F-8E3A-A2CACA8820AB", "Tariff Code:");
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.zTextBox1.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 39, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 70, true);
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
			this.zLabel1.CaptionResourceString = Res.GetData("7B951682-2124-4C19-BAC3-327A104D613B", "Description:");
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

		private Enterprise.ZArchitecture.ZTextBox TariffCodeTextBox;
		private Enterprise.ZArchitecture.ZLabel TariffCodeLabel;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
	}
}
