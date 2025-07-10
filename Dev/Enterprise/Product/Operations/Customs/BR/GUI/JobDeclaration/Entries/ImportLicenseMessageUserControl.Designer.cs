namespace Enterprise.Customs.BR.GUI
{
	partial class ImportLicenseMessageUserControl
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
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a5e705ae-7c9d-4360-9e8e-91643ff87910", "Licenses");
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
			// TopVerticalSplitContainer
			// 
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
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
			// ImportLicenseMessageUserControl
			// 
			this.Name = "ImportLicenseMessageUserControl";
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

		internal Enterprise.ZArchitecture.ZTextBox TariffCodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
