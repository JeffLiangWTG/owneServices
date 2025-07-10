namespace Enterprise.Customs.BR.GUI
{
	partial class EntryLineAdditionalDataUserControl
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ExtendedInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportLicenseNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExtendInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TaxOrFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLineDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExtendedInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExtendedInfoGroupBox.SuspendLayout();
			this.ExtendInfoTabControl.SuspendLayout();
			this.TaxOrFeeTabPage.SuspendLayout();
			this.EntryLineDutyAndTaxGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).BeginInit();
			this.EntryLineDutyAndTaxGrid.SuspendLayout();
			this.ExtendedInfoTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("48899717-DB4D-4F25-B854-336B6323CB88", "Extended Information");
			this.ExtendedInfoGroupBox.Controls.Add(this.ImportLicenseNumberTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ExtendedInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendedInfoGroupBox.Name = "ExtendedInfoGroupBox";
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
			this.ExtendedInfoGroupBox.TabIndex = 2;
			this.ExtendedInfoGroupBox.TabStop = false;
			// 
			// ImportLicenseNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportLicenseNumberTextBox, "FormalEntryHeaders.AllEntryLines.ImportLicenseNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ImportLicenseNumber)));
			this.ImportLicenseNumberTextBox.CaptionResourceString = null;
			this.ImportLicenseNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 17, true);
			this.ImportLicenseNumberTextBox.Name = "ImportLicenseNumberTextBox";
			this.ImportLicenseNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.ImportLicenseNumberTextBox.TabIndex = 2;
			// 
			// TariffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "FormalEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("897B8D25-2BF6-427E-AE33-44A597B9AAE9", "Tariff Code:");
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.TariffCodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FormalEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("794C82D0-2F13-4EA9-A227-C9647348C60A", "Description:");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 41, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 137, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// ExtendInfoTabControl
			// 
			this.ExtendInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ExtendInfoTabControl.Controls.Add(this.TaxOrFeeTabPage);
			this.ExtendInfoTabControl.Controls.Add(this.ExtendedInfoTabPage);
			this.ExtendInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendInfoTabControl.Name = "ExtendInfoTabControl";
			this.ExtendInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
			this.ExtendInfoTabControl.TabIndex = 15;
			// 
			// TaxOrFeeTabPage
			// 
			this.TaxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("7FD032DD-51D8-43A0-974E-5278A29E2844", "Tax Or Fee");
			this.TaxOrFeeTabPage.Controls.Add(this.EntryLineDutyAndTaxGroupBox);
			this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxOrFeeTabPage.Name = "TaxOrFeeTabPage";
			this.TaxOrFeeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
			this.TaxOrFeeTabPage.TabIndex = 1;
			this.TaxOrFeeTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryLineDutyAndTaxGroupBox
			// 
			this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("81AFC011-D573-42EE-8B02-85142A27698E", "Duty And Tax");
			this.EntryLineDutyAndTaxGroupBox.Controls.Add(this.EntryLineDutyAndTaxGrid);
			this.EntryLineDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineDutyAndTaxGroupBox, true);
			this.EntryLineDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLineDutyAndTaxGroupBox.Name = "EntryLineDutyAndTaxGroupBox";
			this.EntryLineDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 178, true);
			this.EntryLineDutyAndTaxGroupBox.TabIndex = 4;
			this.EntryLineDutyAndTaxGroupBox.TabStop = false;
			// 
			// EntryLineDutyAndTaxGrid
			// 
			this.EntryLineDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineDutyAndTaxGrid, "FormalEntryHeaders.AllEntryLines.Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FormalEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_MethodOfPayment)));
			this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("5AAD9837-C67C-4781-9D96-2E88966CC5FF", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.BR.GUI.Res.GetData("3DE2AEC1-517E-400C-AF0E-E7F2B4D8670B", "Charge Type");
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("B44A4E36-1E14-430B-9AFA-7F06A7B86F5B", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.BR.GUI.Res.GetData("3DE2AEC1-517E-400C-AF0E-E7F2B4D8670B", "Charge Type");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("688C8865-58F6-41DE-84F5-ED4C25CC68A3", "Base Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("199C2855-4955-45CB-9C1D-89424AFCB502", "Method of Calculation");
			zDropEditColumnStyleInfo2.ColumnName = "CF_MethodOfCalculation";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("2FC2786E-FD48-4166-877E-4DC1611D0A4A", "Tax Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("FF502BB9-1358-4375-848E-8E03817F7FC9", "Total Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("E98659FB-8035-46EB-AEA6-879A3D0B1525", "Method of Payment");
			zTextBoxColumnStyleInfo2.ColumnName = "CF_MethodOfPayment";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineDutyAndTaxGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
			this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.ReadOnly = true;
			this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 159, true);
			this.EntryLineDutyAndTaxGrid.TabIndex = 0;
			// 
			// ExtendedInfoTabPage
			// 
			this.ExtendedInfoTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("A7EED764-8C01-4D7D-B536-C10BC4113FA6", "Extended Information");
			this.ExtendedInfoTabPage.Controls.Add(this.ExtendedInfoGroupBox);
			this.ExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ExtendedInfoTabPage.Name = "ExtendedInfoTabPage";
			this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
			this.ExtendedInfoTabPage.TabIndex = 2;
			// 
			// EntryLineAdditionalDataUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExtendInfoTabControl);
			this.Name = "EntryLineAdditionalDataUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExtendedInfoGroupBox.ResumeLayout(false);
			this.ExtendedInfoGroupBox.PerformLayout();
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.TaxOrFeeTabPage.ResumeLayout(false);
			this.TaxOrFeeTabPage.PerformLayout();
			this.EntryLineDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineDutyAndTaxGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).EndInit();
			this.EntryLineDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineDutyAndTaxGrid.PerformLayout();
			this.ExtendedInfoTabPage.ResumeLayout(false);
			this.ExtendedInfoTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox TariffCodeTextBox;
		internal ZArchitecture.ZTextBox DescriptionTextBox;
		internal ZArchitecture.GUI.ZTemplateTabControl ExtendInfoTabControl;
		internal ZArchitecture.GUI.ZTabPage ExtendedInfoTabPage;
		internal ZArchitecture.GUI.ZTabPage TaxOrFeeTabPage;
		internal ZArchitecture.GUI.ZGroupBox ExtendedInfoGroupBox;
		internal ZArchitecture.GUI.ZGroupBox EntryLineDutyAndTaxGroupBox;
		internal ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
		internal ZArchitecture.ZTextBox ImportLicenseNumberTextBox;
	}
}
