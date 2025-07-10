namespace Enterprise.Customs.BR.GUI
{
	partial class MessageUserControlSubmitionTypeBLT
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
			this.SiscomexUsageFeeUserControl = new Enterprise.Customs.BR.GUI.SiscomexUsageFeeUserControl();
			this.SiscomexUsageFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
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
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SiscomexUsageFeeUserControl.SuspendLayout();
			this.SiscomexUsageFeeTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// EntriesGroupBox
			//
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 65, true);
			//
			// EntriesBoundGrid
			//
			this.BindingSource.SetBindingMember(this.EntriesBoundGrid, "FormalEntryHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).CH_BGMReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).CH_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).CH_MessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).PackagesCount)));
			this.EntriesBoundGrid.Size = this.EntriesBoundGrid.Size;
			this.EntriesBoundGrid.TabIndex = 1;
			//
			// EntryLineGrid
			//
			this.BindingSource.SetBindingMember(this.EntryLineGrid, "FormalEntryHeaders.AllEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_LineNumber)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6e823221-f0de-443e-8db0-7ba50be1647d", "Line Status");
			zTextBoxColumnStyleInfo1.ColumnName = "LineSubmissionStatusDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("fc7b2f80-d693-4483-8a17-5a9b46377f18", "Tariff");
			zTextBoxColumnStyleInfo2.ColumnName = "FormattedTariff";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("4b45d9bd-2045-4576-98ff-dca2aa26c537", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "EffectiveDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("649880c3-2abd-45cd-8d71-7dd5ad13e880", "Customs Value");
			zCalcEditColumnStyleInfo1.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 333, true);
			//
			// ExtendedInfoGroupBox
			//
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ExtendedInfoGroupBox.Visible = true;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			//
			// EntryLinesMessagesTabControl
			//
			this.EntryLinesMessagesTabControl.Controls.Add(this.SiscomexUsageFeeTabPage);
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 489, true);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.SiscomexUsageFeeTabPage, 0);
			//
			// SiscomexUsageFeeUserControl
			//
			this.SiscomexUsageFeeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SiscomexUsageFeeUserControl, "FormalEntryHeaders.SiscomexUsageFees");
			this.SiscomexUsageFeeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SiscomexUsageFeeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SiscomexUsageFeeUserControl.Name = "SiscomexUsageFeeUserControl";
			this.SiscomexUsageFeeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 456, true);
			this.SiscomexUsageFeeUserControl.TabIndex = 0;
			//
			// SiscomexUsageFeeTabPage
			//
			this.SiscomexUsageFeeTabPage.AutoScroll = true;
			this.SiscomexUsageFeeTabPage.Controls.Add(this.SiscomexUsageFeeUserControl);
			this.SiscomexUsageFeeTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("9BB61A68-35B0-4A9F-8B36-040BE467D5CD", "SISCOMEX Usage Fee");
			this.SiscomexUsageFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SiscomexUsageFeeTabPage.Name = "SiscomexUsageFeeTabPage";
			this.SiscomexUsageFeeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SiscomexUsageFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 398, true);
			this.SiscomexUsageFeeTabPage.TabIndex = 2;
			//
			// TariffCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "FormalEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("c19114a6-dc28-4a29-8377-b57b40f08d80", "Tariff Code:");
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 13, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.TariffCodeTextBox.TabIndex = 1;
			//
			// DescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FormalEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("83e14399-2fd9-4a96-8805-3736bd1c1c9a", "Description:");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 37, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 72, true);
			this.DescriptionTextBox.TabIndex = 3;
			//
			// MessageUserControlSubmitionTypeBLT
			//
			this.Name = "MessageUserControlSubmitionTypeBLT";
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
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SiscomexUsageFeeUserControl.ResumeLayout(true);
			this.SiscomexUsageFeeUserControl.PerformLayout();
			this.SiscomexUsageFeeTabPage.ResumeLayout(false);
			this.SiscomexUsageFeeTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTabPage SiscomexUsageFeeTabPage;
		internal SiscomexUsageFeeUserControl SiscomexUsageFeeUserControl;
		Enterprise.ZArchitecture.ZTextBox TariffCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
