namespace Enterprise.Customs.EU.GUI
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.DutyAndTaxDetails = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ExtendedInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExtendInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TaxOrFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExtendedInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLineSupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TariffCodePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExtendedInfoGroupBox.SuspendLayout();
			this.ExtendInfoTabControl.SuspendLayout();
			this.TaxOrFeeTabPage.SuspendLayout();
			this.ExtendedInfoTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).BeginInit();
			this.EntryLineSupportingDocumentsGrid.SuspendLayout();
			this.TariffCodePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// DutyAndTaxDetails
			// 
			this.DutyAndTaxDetails.AllowDrop = true;
			this.DutyAndTaxDetails.BindingMember = "CustomsEntryHeaders.AllEntryLines";
			this.DutyAndTaxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DutyAndTaxDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DutyAndTaxDetails.Name = "DutyAndTaxDetails";
			this.DutyAndTaxDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 287, true);
			this.DutyAndTaxDetails.TabIndex = 0;
			// 
			// ExtendedInfoGroupBox
			//
			this.ExtendedInfoGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("72B3499D-9B8B-48BC-BF38-49607A45DF16", "Extended Information");
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodePanel);
			this.ExtendedInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendedInfoGroupBox.Name = "ExtendedInfoGroupBox";
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 293, true);
			this.ExtendedInfoGroupBox.TabIndex = 2;
			this.ExtendedInfoGroupBox.TabStop = false;
			// 
			// ExtendInfoTabControl
			// 
			this.ExtendInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ExtendInfoTabControl.Controls.Add(this.TaxOrFeeTabPage);
			this.ExtendInfoTabControl.Controls.Add(this.ExtendedInfoTabPage);
			this.ExtendInfoTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.ExtendInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendInfoTabControl.Name = "ExtendInfoTabControl";
			this.ExtendInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 270, true);
			this.ExtendInfoTabControl.TabIndex = 15;
			// 
			// TaxOrFeeTabPage
			//
			this.TaxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("FB3FB307-EE5B-4C35-AF5E-C77D759CFE14", "Tax Or Fee");
			this.TaxOrFeeTabPage.Controls.Add(this.DutyAndTaxDetails);
			this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxOrFeeTabPage.Name = "TaxOrFeeTabPage";
			this.TaxOrFeeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 293, true);
			this.TaxOrFeeTabPage.TabIndex = 1;
			this.TaxOrFeeTabPage.UseVisualStyleBackColor = true;
			// 
			// ExtendedInfoTabPage
			//
			this.ExtendedInfoTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("D6435F67-8656-4C9F-A4FE-9D58E6F154A9", "Extended Information");
			this.ExtendedInfoTabPage.Controls.Add(this.ExtendedInfoGroupBox);
			this.ExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ExtendedInfoTabPage.Name = "ExtendedInfoTabPage";
			this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 293, true);
			this.ExtendedInfoTabPage.TabIndex = 2;
			// 
			// SupportingDocumentsTabPage
			//
			this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A451BF06-4C6B-4BC1-BDCF-5B6EF51E3446", "[44] Supporting Documents");
			this.SupportingDocumentsTabPage.Controls.Add(this.EntryLineSupportingDocumentsGrid);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
			this.SupportingDocumentsTabPage.TabIndex = 1;
			this.SupportingDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryLineSupportingDocumentsGrid
			// 
			this.EntryLineSupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineSupportingDocumentsGrid, "CustomsEntryHeaders.AllEntryLines.ReadOnlySupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_CodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ReadOnlySupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			this.EntryLineSupportingDocumentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_CodeDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_Status";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "CSI_UnitOfQuantity";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity2";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "CSI_UnitOfQuantity2";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "CSI_RX_NKCurrency";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "CSI_DateOfExpiry";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryLineSupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.EntryLineSupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineSupportingDocumentsGrid.GridId = "cacbd336-b28d-4730-95cc-0751f7bb394e";
			this.EntryLineSupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineSupportingDocumentsGrid.LayoutKey = "EntryLineSupportingDocumentsGrid";
			this.EntryLineSupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLineSupportingDocumentsGrid.Name = "EntryLineSupportingDocumentsGrid";
			this.EntryLineSupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 237, true);
			this.EntryLineSupportingDocumentsGrid.TabIndex = 0;
			// 
			// TariffCodePanel
			// 
			this.TariffCodePanel.Controls.Add(this.TariffCodeTextBox);
			this.TariffCodePanel.Controls.Add(this.DescriptionTextBox);
			this.TariffCodePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.TariffCodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TariffCodePanel.Name = "TariffCodePanel";
			this.TariffCodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 224, true);
			this.TariffCodePanel.TabIndex = 2;
			//
			// TariffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9CAF67E5-322A-410E-806F-5DB1B831FBEE", "Tariff Code:");
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.TariffCodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4FFB7922-F631-4DA8-A7EE-A7358AFC032C", "Description:");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 41, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 137, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// EntryLineAdditionalDataUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExtendInfoTabControl);
			this.Name = "EntryLineAdditionalDataUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExtendedInfoGroupBox.ResumeLayout(false);
			this.ExtendedInfoGroupBox.PerformLayout();
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.TaxOrFeeTabPage.ResumeLayout(false);
			this.TaxOrFeeTabPage.PerformLayout();
			this.ExtendedInfoTabPage.ResumeLayout(false);
			this.ExtendedInfoTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).EndInit();
			this.EntryLineSupportingDocumentsGrid.ResumeLayout(false);
			this.EntryLineSupportingDocumentsGrid.PerformLayout();
			this.TariffCodePanel.ResumeLayout(false);
			this.TariffCodePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZTemplateTabControl ExtendInfoTabControl;
		public ZArchitecture.GUI.ZTabPage ExtendedInfoTabPage;
		public ZArchitecture.GUI.ZTabPage TaxOrFeeTabPage;
		public ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		protected ZArchitecture.GUI.ZGroupBox ExtendedInfoGroupBox;
		public ZArchitecture.GUI.ZDynamicControlCreationUserControl DutyAndTaxDetails;
		public ZArchitecture.ZGrid EntryLineSupportingDocumentsGrid;
		private ZArchitecture.GUI.ZPanel TariffCodePanel;
		private ZArchitecture.ZTextBox TariffCodeTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
