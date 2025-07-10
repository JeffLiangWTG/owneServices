namespace Enterprise.Customs.CN.GUI
{
	public partial class CustomsEntriesAndEntryLinesUserControl
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
			this.components = new System.ComponentModel.Container();
			this.EntryHeaderTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HtmlFormatEntryDataWebBrowser = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.DutiesAndTaxesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TotalDutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalGSTVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalExciseAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalAntiDumpingAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCountervailingAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
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
			this.MessageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryHeaderTabPage.SuspendLayout();
			this.DutiesAndTaxesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Controls.Add(this.DutiesAndTaxesGroupBox);
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 142, true);
			this.EntriesGroupBox.Controls.SetChildIndex(this.DutiesAndTaxesGroupBox, 0);
			this.EntriesGroupBox.Controls.SetChildIndex(this.EntriesBoundGrid, 0);
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.ReadOnly = false;
			this.EntriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 123, true);
			// 
			// MainHorizontalSplitContainer
			// 
			this.MainHorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 634, true);
			this.MainHorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(142);
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Controls.Add(this.EntryHeaderTabPage);
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 489, true);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryHeaderTabPage, 0);
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1139, 462, true);
			// 
			// EntryLineGrid
			// 
			this.BindingSource.SetBindingMember(this.EntryLineGrid, "CustomsEntryHeaders.MergedLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).CL_LineNumber)));
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1133, 289, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 292, true);
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1133, 167, true);
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 398, true);
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 142, true);
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(753);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 392, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobDeclaration);
			// 
			// EntryHeaderTabPage
			// 
			this.EntryHeaderTabPage.AutoScroll = true;
			this.EntryHeaderTabPage.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("AD7D4B17-6D51-4481-ACFF-9E8D3235098D", "Entry Details");
			this.EntryHeaderTabPage.Controls.Add(this.HtmlFormatEntryDataWebBrowser);
			this.EntryHeaderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryHeaderTabPage.Name = "EntryHeaderTabPage";
			this.EntryHeaderTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryHeaderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 398, true);
			this.EntryHeaderTabPage.TabIndex = 2;
			// 
			// HtmlFormatEntryDataWebBrowser
			// 
			this.HtmlFormatEntryDataWebBrowser.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlFormatEntryDataWebBrowser, "CustomsEntryHeaders.HtmlFormatEntryData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).HtmlFormatEntryData)));
			this.HtmlFormatEntryDataWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlFormatEntryDataWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HtmlFormatEntryDataWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.HtmlFormatEntryDataWebBrowser.Name = "HtmlFormatEntryDataWebBrowser";
			this.HtmlFormatEntryDataWebBrowser.ScriptErrorsSuppressed = true;
			this.HtmlFormatEntryDataWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 392, true);
			this.HtmlFormatEntryDataWebBrowser.TabIndex = 2;
			// 
			// DutiesAndTaxesGroupBox
			// 
			this.DutiesAndTaxesGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("338de958-281d-4f15-90aa-1ff5b921ed32", "Duties and Taxes");
			this.DutiesAndTaxesGroupBox.Controls.Add(this.TotalDutyAmountCalcEdit);
			this.DutiesAndTaxesGroupBox.Controls.Add(this.TotalGSTVATAmountCalcEdit);
			this.DutiesAndTaxesGroupBox.Controls.Add(this.TotalExciseAmountCalcEdit);
			this.DutiesAndTaxesGroupBox.Controls.Add(this.TotalAntiDumpingAmountCalcEdit);
			this.DutiesAndTaxesGroupBox.Controls.Add(this.TotalCountervailingAmountCalcEdit);
			this.DutiesAndTaxesGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.DutiesAndTaxesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(917, 16, true);
			this.DutiesAndTaxesGroupBox.Name = "DutiesAndTaxesGroupBox";
			this.DutiesAndTaxesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 123, true);
			this.DutiesAndTaxesGroupBox.TabIndex = 1;
			this.DutiesAndTaxesGroupBox.TabStop = false;
			// 
			// TotalDutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalDutyAmountCalcEdit, "CustomsEntryHeaders.TotalDutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalDutyAmount)));
			this.TotalDutyAmountCalcEdit.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("cdc0d87b-6a5f-459a-9e62-0c62bf382df0", "Total Duty");
			this.TotalDutyAmountCalcEdit.DecimalPlaces = 2;
			this.TotalDutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 20, true);
			this.TotalDutyAmountCalcEdit.Name = "TotalDutyAmountCalcEdit";
			this.TotalDutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.TotalDutyAmountCalcEdit.TabIndex = 0;
			this.TotalDutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalGSTVATAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGSTVATAmountCalcEdit, "CustomsEntryHeaders.TotalGSTVATAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalGSTVATAmount)));
			this.TotalGSTVATAmountCalcEdit.CaptionResourceString = null;
			this.TotalGSTVATAmountCalcEdit.DecimalPlaces = 2;
			this.TotalGSTVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 43, true);
			this.TotalGSTVATAmountCalcEdit.Name = "TotalGSTVATAmountCalcEdit";
			this.TotalGSTVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.TotalGSTVATAmountCalcEdit.TabIndex = 1;
			this.TotalGSTVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalExciseAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalExciseAmountCalcEdit, "CustomsEntryHeaders.TotalExciseAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalExciseAmount)));
			this.TotalExciseAmountCalcEdit.CaptionResourceString = null;
			this.TotalExciseAmountCalcEdit.DecimalPlaces = 2;
			this.TotalExciseAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 66, true);
			this.TotalExciseAmountCalcEdit.Name = "TotalExciseAmountCalcEdit";
			this.TotalExciseAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.TotalExciseAmountCalcEdit.TabIndex = 2;
			this.TotalExciseAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAntiDumpingAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalAntiDumpingAmountCalcEdit, "CustomsEntryHeaders.TotalAntiDumpingAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalAntiDumpingAmount)));
			this.TotalAntiDumpingAmountCalcEdit.CaptionResourceString = null;
			this.TotalAntiDumpingAmountCalcEdit.DecimalPlaces = 2;
			this.TotalAntiDumpingAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 89, true);
			this.TotalAntiDumpingAmountCalcEdit.Name = "TotalAntiDumpingAmountCalcEdit";
			this.TotalAntiDumpingAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.TotalAntiDumpingAmountCalcEdit.TabIndex = 3;
			this.TotalAntiDumpingAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalCountervailingAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCountervailingAmountCalcEdit, "CustomsEntryHeaders.TotalCountervailingAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalCountervailingAmount)));
			this.TotalCountervailingAmountCalcEdit.CaptionResourceString = null;
			this.TotalCountervailingAmountCalcEdit.DecimalPlaces = 2;
			this.TotalCountervailingAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 112, true);
			this.TotalCountervailingAmountCalcEdit.Name = "TotalCountervailingAmountCalcEdit";
			this.TotalCountervailingAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.TotalCountervailingAmountCalcEdit.TabIndex = 4;
			this.TotalCountervailingAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomsEntriesAndEntryLinesUserControl
			// 
			this.Name = "CustomsEntriesAndEntryLinesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 634, true);
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
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryHeaderTabPage.ResumeLayout(false);
			this.EntryHeaderTabPage.PerformLayout();
			this.DutiesAndTaxesGroupBox.ResumeLayout(false);
			this.DutiesAndTaxesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage EntryHeaderTabPage;
		private Enterprise.Messaging.GUI.HtmlInterpretationBox HtmlFormatEntryDataWebBrowser;
		private ZArchitecture.GUI.ZGroupBox DutiesAndTaxesGroupBox;
		protected ZArchitecture.ZCalcEdit TotalDutyAmountCalcEdit;
		protected ZArchitecture.ZCalcEdit TotalGSTVATAmountCalcEdit;
		protected ZArchitecture.ZCalcEdit TotalExciseAmountCalcEdit;
		protected ZArchitecture.ZCalcEdit TotalAntiDumpingAmountCalcEdit;
		protected ZArchitecture.ZCalcEdit TotalCountervailingAmountCalcEdit;

	}
}
