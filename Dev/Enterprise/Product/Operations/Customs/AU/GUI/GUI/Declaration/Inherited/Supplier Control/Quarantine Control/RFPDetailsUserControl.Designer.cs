namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.RecommendationLettersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RecommendationLettersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PrintLocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QH_CertificateRequiredLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_CertificateRequiredLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QH_PrintLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_OH_PrintLocationOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TransportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QH_PackDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StorageTemperatureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.orLabel = new Enterprise.ZArchitecture.ZLabel();
			this.QH_MaximumTemperatureCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QH_MinimumTemperatureCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QH_AbsoluteTemperatureCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QH_TemperatureUMDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_RL_NKBorderInspectionPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QH_RN_NKOriginCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HealthCertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QuotaTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_AQISRegionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipStoresCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AMLCQuotaYearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AMLCQuotaCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SplitPackerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SplitMarksCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SplitContainerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QH_CertificatePrintIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HeaderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QH_ExemptionCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_CustomsConsigneeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProductUseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GetCustomsEDNCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QH_ProduceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneeAgentNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RecommendationLettersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecommendationLettersGrid)).BeginInit();
			this.RecommendationLettersGrid.SuspendLayout();
			this.PrintLocationGroupBox.SuspendLayout();
			this.QH_CertificateRequiredLocationCodeFindBox.SuspendLayout();
			this.QH_PrintLocationDropEdit.SuspendLayout();
			this.QH_OH_PrintLocationOrganisationGuidFindBox.SuspendLayout();
			this.TransportGroupBox.SuspendLayout();
			this.QH_PackDateDateEdit.SuspendLayout();
			this.StorageTemperatureGroupBox.SuspendLayout();
			this.QH_TemperatureUMDropEdit.SuspendLayout();
			this.QH_RL_NKBorderInspectionPortCodeFindBox.SuspendLayout();
			this.QH_RN_NKOriginCountryCodeFindBox.SuspendLayout();
			this.HealthCertificateGroupBox.SuspendLayout();
			this.QH_AQISRegionCodeFindBox.SuspendLayout();
			this.QH_CertificatePrintIndicatorDropEdit.SuspendLayout();
			this.HeaderDetailsGroupBox.SuspendLayout();
			this.ProductUseDropEdit.SuspendLayout();
			this.QH_ProduceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// RecommendationLettersGroupBox
			// 
			this.RecommendationLettersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RecommendationLettersGroupBox.Controls.Add(this.RecommendationLettersGrid);
			this.RecommendationLettersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(959, 5, true);
			this.RecommendationLettersGroupBox.Name = "RecommendationLettersGroupBox";
			this.RecommendationLettersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 265, true);
			this.RecommendationLettersGroupBox.TabIndex = 9;
			this.RecommendationLettersGroupBox.TabStop = false;
			this.RecommendationLettersGroupBox.Text = "Recommendation Letter Details";
			// 
			// RecommendationLettersGrid
			// 
			this.RecommendationLettersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RecommendationLettersGrid, "QuarantineExDocHeader+RecommendationLetters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.RecommendationLetters)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.RecommendationLetter)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.RecommendationLetters)).SyncRoot)).ZA_LetterNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.RecommendationLetter)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.RecommendationLetters)).SyncRoot)).ZA_LetterDate)));
			this.RecommendationLettersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ZA_LetterNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "ZA_LetterDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RecommendationLettersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RecommendationLettersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RecommendationLettersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecommendationLettersGrid.GridId = "4E9B1F0C-F0E4-479B-9CE2-06E07DCDDBE0";
			this.RecommendationLettersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecommendationLettersGrid.LayoutKey = "RecommendationLettersGrid";
			this.RecommendationLettersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RecommendationLettersGrid.Name = "RecommendationLettersGrid";
			this.RecommendationLettersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 246, true);
			this.RecommendationLettersGrid.TabIndex = 0;
			// 
			// PrintLocationGroupBox
			// 
			this.PrintLocationGroupBox.Controls.Add(this.QH_CertificateRequiredLocationTextBox);
			this.PrintLocationGroupBox.Controls.Add(this.QH_CertificateRequiredLocationCodeFindBox);
			this.PrintLocationGroupBox.Controls.Add(this.QH_PrintLocationDropEdit);
			this.PrintLocationGroupBox.Controls.Add(this.QH_OH_PrintLocationOrganisationGuidFindBox);
			this.PrintLocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 171, true);
			this.PrintLocationGroupBox.Name = "PrintLocationGroupBox";
			this.PrintLocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 99, true);
			this.PrintLocationGroupBox.TabIndex = 7;
			this.PrintLocationGroupBox.TabStop = false;
			this.PrintLocationGroupBox.Text = "Print Location";
			// 
			// QH_CertificateRequiredLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_CertificateRequiredLocationTextBox, "QuarantineExDocHeader+QH_CertificateRequiredLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_CertificateRequiredLocation)));
			this.QH_CertificateRequiredLocationTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_CertificateRequiredLocationTextBox, false);
			this.QH_CertificateRequiredLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 26, true);
			this.QH_CertificateRequiredLocationTextBox.Name = "QH_CertificateRequiredLocationTextBox";
			this.QH_CertificateRequiredLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.QH_CertificateRequiredLocationTextBox.TabIndex = 1;
			// 
			// QH_CertificateRequiredLocationCodeFindBox
			// 
			this.QH_CertificateRequiredLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_CertificateRequiredLocationCodeFindBox, "QuarantineExDocHeader+QH_CertificateRequiredLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_CertificateRequiredLocation)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_CertificateRequiredLocationCodeFindBox, false);
			this.QH_CertificateRequiredLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 26, true);
			this.QH_CertificateRequiredLocationCodeFindBox.Name = "QH_CertificateRequiredLocationCodeFindBox";
			this.QH_CertificateRequiredLocationCodeFindBox.ShouldResize = true;
			this.QH_CertificateRequiredLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.QH_CertificateRequiredLocationCodeFindBox.TabIndex = 1;
			// 
			// QH_PrintLocationDropEdit
			// 
			this.QH_PrintLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_PrintLocationDropEdit, "QuarantineExDocHeader+QH_PrintLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_PrintLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.LocationWithAqisPlace)));
			this.QH_PrintLocationDropEdit.BindToList = "QuarantineExDocHeader+Lookups+LocationWithAqisPlace";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_PrintLocationDropEdit, false);
			this.QH_PrintLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 26, true);
			this.QH_PrintLocationDropEdit.Name = "QH_PrintLocationDropEdit";
			this.QH_PrintLocationDropEdit.ShouldResizeByMaxLength = true;
			this.QH_PrintLocationDropEdit.ShowDescriptionBox = false;
			this.QH_PrintLocationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_PrintLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.QH_PrintLocationDropEdit.TabIndex = 0;
			// 
			// QH_OH_PrintLocationOrganisationGuidFindBox
			// 
			this.QH_OH_PrintLocationOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_OH_PrintLocationOrganisationGuidFindBox, "QuarantineExDocHeader+QH_OH_PrintLocationOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_OH_PrintLocationOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.EDIUser)));
			this.QH_OH_PrintLocationOrganisationGuidFindBox.BindToList = "QuarantineExDocHeader+Lookups+EDIUser";
			this.QH_OH_PrintLocationOrganisationGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.QH_OH_PrintLocationOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 26, true);
			this.QH_OH_PrintLocationOrganisationGuidFindBox.Name = "QH_OH_PrintLocationOrganisationGuidFindBox";
			this.QH_OH_PrintLocationOrganisationGuidFindBox.ShouldResize = true;
			this.QH_OH_PrintLocationOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.QH_OH_PrintLocationOrganisationGuidFindBox.TabIndex = 4;
			// 
			// TransportGroupBox
			// 
			this.TransportGroupBox.Controls.Add(this.QH_PackDateDateEdit);
			this.TransportGroupBox.Controls.Add(this.StorageTemperatureGroupBox);
			this.TransportGroupBox.Controls.Add(this.QH_RL_NKBorderInspectionPortCodeFindBox);
			this.TransportGroupBox.Controls.Add(this.QH_RN_NKOriginCountryCodeFindBox);
			this.TransportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 5, true);
			this.TransportGroupBox.Name = "TransportGroupBox";
			this.TransportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 265, true);
			this.TransportGroupBox.TabIndex = 8;
			this.TransportGroupBox.TabStop = false;
			this.TransportGroupBox.Text = "Transport Details";
			// 
			// QH_PackDateDateEdit
			// 
			this.QH_PackDateDateEdit.AllowDrop = true;
			this.QH_PackDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.QH_PackDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QH_PackDateDateEdit, "QuarantineExDocHeader+QH_PackDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_PackDate)));
			this.QH_PackDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 87, true);
			this.QH_PackDateDateEdit.Name = "QH_PackDateDateEdit";
			this.QH_PackDateDateEdit.TabIndex = 5;
			// 
			// StorageTemperatureGroupBox
			// 
			this.StorageTemperatureGroupBox.Controls.Add(this.orLabel);
			this.StorageTemperatureGroupBox.Controls.Add(this.QH_MaximumTemperatureCalcEdit);
			this.StorageTemperatureGroupBox.Controls.Add(this.QH_MinimumTemperatureCalcEdit);
			this.StorageTemperatureGroupBox.Controls.Add(this.QH_AbsoluteTemperatureCalcEdit);
			this.StorageTemperatureGroupBox.Controls.Add(this.QH_TemperatureUMDropEdit);
			this.StorageTemperatureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 129, true);
			this.StorageTemperatureGroupBox.Name = "StorageTemperatureGroupBox";
			this.StorageTemperatureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 118, true);
			this.StorageTemperatureGroupBox.TabIndex = 6;
			this.StorageTemperatureGroupBox.TabStop = false;
			this.StorageTemperatureGroupBox.Text = "Storage Temperature";
			// 
			// orLabel
			// 
			this.orLabel.AutoSize = true;
			this.orLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.orLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 46, true);
			this.orLabel.Name = "orLabel";
			this.orLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 13, true);
			this.orLabel.TabIndex = 2;
			this.orLabel.Text = "or";
			// 
			// QH_MaximumTemperatureCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QH_MaximumTemperatureCalcEdit, "QuarantineExDocHeader+QH_MaximumTemperature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_MaximumTemperature)));
			this.QH_MaximumTemperatureCalcEdit.CaptionResourceString = null;
			this.QH_MaximumTemperatureCalcEdit.DecimalPlaces = 2;
			this.QH_MaximumTemperatureCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 63, true);
			this.QH_MaximumTemperatureCalcEdit.Name = "QH_MaximumTemperatureCalcEdit";
			this.QH_MaximumTemperatureCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.QH_MaximumTemperatureCalcEdit.TabIndex = 6;
			this.QH_MaximumTemperatureCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QH_MinimumTemperatureCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QH_MinimumTemperatureCalcEdit, "QuarantineExDocHeader+QH_MinimumTemperature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_MinimumTemperature)));
			this.QH_MinimumTemperatureCalcEdit.CaptionResourceString = null;
			this.QH_MinimumTemperatureCalcEdit.DecimalPlaces = 2;
			this.QH_MinimumTemperatureCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 62, true);
			this.QH_MinimumTemperatureCalcEdit.Name = "QH_MinimumTemperatureCalcEdit";
			this.QH_MinimumTemperatureCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.QH_MinimumTemperatureCalcEdit.TabIndex = 4;
			this.QH_MinimumTemperatureCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QH_AbsoluteTemperatureCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QH_AbsoluteTemperatureCalcEdit, "QuarantineExDocHeader+QH_AbsoluteTemperature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AbsoluteTemperature)));
			this.QH_AbsoluteTemperatureCalcEdit.CaptionResourceString = null;
			this.QH_AbsoluteTemperatureCalcEdit.DecimalPlaces = 2;
			this.QH_AbsoluteTemperatureCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 23, true);
			this.QH_AbsoluteTemperatureCalcEdit.Name = "QH_AbsoluteTemperatureCalcEdit";
			this.QH_AbsoluteTemperatureCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.QH_AbsoluteTemperatureCalcEdit.TabIndex = 1;
			this.QH_AbsoluteTemperatureCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QH_TemperatureUMDropEdit
			// 
			this.QH_TemperatureUMDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_TemperatureUMDropEdit, "QuarantineExDocHeader+QH_TemperatureUM");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_TemperatureUM)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.TemperatureUnit)));
			this.QH_TemperatureUMDropEdit.BindToList = "QuarantineExDocHeader+Lookups+TemperatureUnit";
			this.QH_TemperatureUMDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 91, true);
			this.QH_TemperatureUMDropEdit.Name = "QH_TemperatureUMDropEdit";
			this.QH_TemperatureUMDropEdit.ShouldResizeByMaxLength = true;
			this.QH_TemperatureUMDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.QH_TemperatureUMDropEdit.TabIndex = 8;
			// 
			// QH_RL_NKBorderInspectionPortCodeFindBox
			// 
			this.QH_RL_NKBorderInspectionPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_RL_NKBorderInspectionPortCodeFindBox, "QuarantineExDocHeader+QH_RL_NKBorderInspectionPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_RL_NKBorderInspectionPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.BorderInspectionPorts)));
			this.QH_RL_NKBorderInspectionPortCodeFindBox.BindToList = "QuarantineExDocHeader+Lookups+BorderInspectionPorts";
			this.QH_RL_NKBorderInspectionPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 54, true);
			this.QH_RL_NKBorderInspectionPortCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.QH_RL_NKBorderInspectionPortCodeFindBox.Name = "QH_RL_NKBorderInspectionPortCodeFindBox";
			this.QH_RL_NKBorderInspectionPortCodeFindBox.ShouldResize = true;
			this.QH_RL_NKBorderInspectionPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.QH_RL_NKBorderInspectionPortCodeFindBox.TabIndex = 3;
			// 
			// QH_RN_NKOriginCountryCodeFindBox
			// 
			this.QH_RN_NKOriginCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_RN_NKOriginCountryCodeFindBox, "QuarantineExDocHeader+QH_RN_NKOriginCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_RN_NKOriginCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.OriginCountries)));
			this.QH_RN_NKOriginCountryCodeFindBox.BindToList = "QuarantineExDocHeader+Lookups+OriginCountries";
			this.QH_RN_NKOriginCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 23, true);
			this.QH_RN_NKOriginCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.QH_RN_NKOriginCountryCodeFindBox.Name = "QH_RN_NKOriginCountryCodeFindBox";
			this.QH_RN_NKOriginCountryCodeFindBox.ShouldResize = true;
			this.QH_RN_NKOriginCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.QH_RN_NKOriginCountryCodeFindBox.TabIndex = 1;
			// 
			// HealthCertificateGroupBox
			// 
			this.HealthCertificateGroupBox.Controls.Add(this.QuotaTypeTextBox);
			this.HealthCertificateGroupBox.Controls.Add(this.QH_AQISRegionCodeFindBox);
			this.HealthCertificateGroupBox.Controls.Add(this.ShipStoresCheckBox);
			this.HealthCertificateGroupBox.Controls.Add(this.AMLCQuotaYearTextBox);
			this.HealthCertificateGroupBox.Controls.Add(this.AMLCQuotaCheckBox);
			this.HealthCertificateGroupBox.Controls.Add(this.SplitPackerCheckBox);
			this.HealthCertificateGroupBox.Controls.Add(this.SplitMarksCheckBox);
			this.HealthCertificateGroupBox.Controls.Add(this.SplitContainerCheckBox);
			this.HealthCertificateGroupBox.Controls.Add(this.QH_CertificatePrintIndicatorDropEdit);
			this.HealthCertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 5, true);
			this.HealthCertificateGroupBox.Name = "HealthCertificateGroupBox";
			this.HealthCertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 160, true);
			this.HealthCertificateGroupBox.TabIndex = 6;
			this.HealthCertificateGroupBox.TabStop = false;
			this.HealthCertificateGroupBox.Text = "Health Certificate Details";
			// 
			// QuotaTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.QuotaTypeTextBox, "QuarantineExDocHeader+QH_QuotaType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_QuotaType)));
			this.QuotaTypeTextBox.CaptionResourceString = null;
			this.QuotaTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 137, true);
			this.QuotaTypeTextBox.Name = "QuotaTypeTextBox";
			this.QuotaTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.QuotaTypeTextBox.TabIndex = 10;
			// 
			// QH_AQISRegionCodeFindBox
			// 
			this.QH_AQISRegionCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_AQISRegionCodeFindBox, "QuarantineExDocHeader+QH_AQISRegion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AQISRegion)));
			this.QH_AQISRegionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 46, true);
			this.QH_AQISRegionCodeFindBox.Name = "QH_AQISRegionCodeFindBox";
			this.QH_AQISRegionCodeFindBox.ShouldResize = true;
			this.QH_AQISRegionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.QH_AQISRegionCodeFindBox.TabIndex = 3;
			// 
			// ShipStoresCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ShipStoresCheckBox, "QuarantineExDocHeader+QH_ShipsStores");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ShipsStores)));
			this.ShipStoresCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShipStoresCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShipStoresCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 114, true);
			this.ShipStoresCheckBox.Name = "ShipStoresCheckBox";
			this.ShipStoresCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.ShipStoresCheckBox.TabIndex = 8;
			this.ShipStoresCheckBox.Text = "Ship Stores:";
			this.ShipStoresCheckBox.UseVisualStyleBackColor = true;
			// 
			// AMLCQuotaYearTextBox
			// 
			this.BindingSource.SetBindingMember(this.AMLCQuotaYearTextBox, "QuarantineExDocHeader+QH_AMLCQuotaYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AMLCQuotaYear)));
			this.AMLCQuotaYearTextBox.CaptionResourceString = null;
			this.AMLCQuotaYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 113, true);
			this.AMLCQuotaYearTextBox.Name = "AMLCQuotaYearTextBox";
			this.AMLCQuotaYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.AMLCQuotaYearTextBox.TabIndex = 9;
			// 
			// AMLCQuotaCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AMLCQuotaCheckBox, "QuarantineExDocHeader+QH_AMLCQuota");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AMLCQuota)));
			this.AMLCQuotaCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AMLCQuotaCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AMLCQuotaCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 92, true);
			this.AMLCQuotaCheckBox.Name = "AMLCQuotaCheckBox";
			this.AMLCQuotaCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.AMLCQuotaCheckBox.TabIndex = 7;
			this.AMLCQuotaCheckBox.UseVisualStyleBackColor = true;
			// 
			// SplitPackerCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SplitPackerCheckBox, "QuarantineExDocHeader+QH_SplitHealthCertByPacker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_SplitHealthCertByPacker)));
			this.SplitPackerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SplitPackerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SplitPackerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 69, true);
			this.SplitPackerCheckBox.Name = "SplitPackerCheckBox";
			this.SplitPackerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.SplitPackerCheckBox.TabIndex = 5;
			this.SplitPackerCheckBox.Text = "Split by Packer:";
			this.SplitPackerCheckBox.UseVisualStyleBackColor = true;
			// 
			// SplitMarksCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SplitMarksCheckBox, "QuarantineExDocHeader+QH_SplitHealthCertByMarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_SplitHealthCertByMarks)));
			this.SplitMarksCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SplitMarksCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SplitMarksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 92, true);
			this.SplitMarksCheckBox.Name = "SplitMarksCheckBox";
			this.SplitMarksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.SplitMarksCheckBox.TabIndex = 6;
			this.SplitMarksCheckBox.Text = "Split by Marks:";
			this.SplitMarksCheckBox.UseVisualStyleBackColor = true;
			// 
			// SplitContainerCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SplitContainerCheckBox, "QuarantineExDocHeader+QH_SplitHealthCertByContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_SplitHealthCertByContainer)));
			this.SplitContainerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SplitContainerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SplitContainerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 69, true);
			this.SplitContainerCheckBox.Name = "SplitContainerCheckBox";
			this.SplitContainerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.SplitContainerCheckBox.TabIndex = 4;
			this.SplitContainerCheckBox.Text = "Split by Container:";
			this.SplitContainerCheckBox.UseVisualStyleBackColor = true;
			// 
			// QH_CertificatePrintIndicatorDropEdit
			// 
			this.QH_CertificatePrintIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_CertificatePrintIndicatorDropEdit, "QuarantineExDocHeader+QH_CertificatePrintIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_CertificatePrintIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.CertificatePrintCode)));
			this.QH_CertificatePrintIndicatorDropEdit.BindToList = "QuarantineExDocHeader+Lookups+CertificatePrintCode";
			this.QH_CertificatePrintIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 23, true);
			this.QH_CertificatePrintIndicatorDropEdit.Name = "QH_CertificatePrintIndicatorDropEdit";
			this.QH_CertificatePrintIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.QH_CertificatePrintIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.QH_CertificatePrintIndicatorDropEdit.TabIndex = 1;
			// 
			// HeaderDetailsGroupBox
			// 
			this.HeaderDetailsGroupBox.Controls.Add(this.ConsigneeAgentNameTextBox);
			this.HeaderDetailsGroupBox.Controls.Add(this.QH_ExemptionCodeTextBox);
			this.HeaderDetailsGroupBox.Controls.Add(this.QH_CustomsConsigneeNameTextBox);
			this.HeaderDetailsGroupBox.Controls.Add(this.ProductUseDropEdit);
			this.HeaderDetailsGroupBox.Controls.Add(this.GetCustomsEDNCheckBox);
			this.HeaderDetailsGroupBox.Controls.Add(this.QH_ProduceTypeDropEdit);
			this.HeaderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.HeaderDetailsGroupBox.Name = "HeaderDetailsGroupBox";
			this.HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 266, true);
			this.HeaderDetailsGroupBox.TabIndex = 5;
			this.HeaderDetailsGroupBox.TabStop = false;
			this.HeaderDetailsGroupBox.Text = "Header Details";
			// 
			// QH_ExemptionCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_ExemptionCodeTextBox, "QuarantineExDocHeader+QH_ExemptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ExemptionCode)));
			this.QH_ExemptionCodeTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("2e2733cf-4406-4817-b215-39adaf937ebd", "Exemption Code");
			this.QH_ExemptionCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 151, true);
			this.QH_ExemptionCodeTextBox.Multiline = true;
			this.QH_ExemptionCodeTextBox.Name = "QH_ExemptionCodeTextBox";
			this.QH_ExemptionCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 44, true);
			this.QH_ExemptionCodeTextBox.TabIndex = 4;
			// 
			// QH_CustomsConsigneeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_CustomsConsigneeNameTextBox, "QuarantineExDocHeader+QH_CustomsConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_CustomsConsigneeName)));
			this.QH_CustomsConsigneeNameTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("20689411-5a75-4e20-ab14-35c136ba8b30", "Customs Consignee");
			this.QH_CustomsConsigneeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 98, true);
			this.QH_CustomsConsigneeNameTextBox.Multiline = true;
			this.QH_CustomsConsigneeNameTextBox.Name = "QH_CustomsConsigneeNameTextBox";
			this.QH_CustomsConsigneeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 46, true);
			this.QH_CustomsConsigneeNameTextBox.TabIndex = 3;
			// 
			// ProductUseDropEdit
			// 
			this.ProductUseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductUseDropEdit, "QuarantineExDocHeader+QH_ProductUseIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ProductUseIndicator)));
			this.ProductUseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 49, true);
			this.ProductUseDropEdit.Name = "ProductUseDropEdit";
			this.ProductUseDropEdit.PreBoundMaxLength = 3;
			this.ProductUseDropEdit.ShouldResizeByMaxLength = true;
			this.ProductUseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.ProductUseDropEdit.TabIndex = 1;
			// 
			// GetCustomsEDNCheckBox
			// 
			this.GetCustomsEDNCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GetCustomsEDNCheckBox, "QuarantineExDocHeader+QH_ObtainExportCustomsPermit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ObtainExportCustomsPermit)));
			this.GetCustomsEDNCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("4f541b5e-ea67-4698-bdf4-42f1f983fae7", "Get Customs EDN");
			this.GetCustomsEDNCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.GetCustomsEDNCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GetCustomsEDNCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 75, true);
			this.GetCustomsEDNCheckBox.Name = "GetCustomsEDNCheckBox";
			this.GetCustomsEDNCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.GetCustomsEDNCheckBox.TabIndex = 2;
			this.GetCustomsEDNCheckBox.Text = "Get Customs EDN";
			this.GetCustomsEDNCheckBox.UseVisualStyleBackColor = true;
			// 
			// QH_ProduceTypeDropEdit
			// 
			this.QH_ProduceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_ProduceTypeDropEdit, "QuarantineExDocHeader+QH_ProduceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ProduceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.ProduceType)));
			this.QH_ProduceTypeDropEdit.BindToList = "QuarantineExDocHeader+Lookups+ProduceType";
			this.QH_ProduceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 23, true);
			this.QH_ProduceTypeDropEdit.Name = "QH_ProduceTypeDropEdit";
			this.QH_ProduceTypeDropEdit.PreBoundMaxLength = 3;
			this.QH_ProduceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.QH_ProduceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.QH_ProduceTypeDropEdit.TabIndex = 0;
			// 
			// ConsigneeAgentNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeAgentNameTextBox, "QuarantineExDocHeader+QH_ConsigneeAgentName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ConsigneeAgentName)));
			this.ConsigneeAgentNameTextBox.CaptionResourceString = null;
			this.ConsigneeAgentNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 203, true);
			this.ConsigneeAgentNameTextBox.Multiline = true;
			this.ConsigneeAgentNameTextBox.Name = "ConsigneeAgentNameTextBox";
			this.ConsigneeAgentNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 44, true);
			this.ConsigneeAgentNameTextBox.TabIndex = 5;
			// 
			// RFPDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RecommendationLettersGroupBox);
			this.Controls.Add(this.PrintLocationGroupBox);
			this.Controls.Add(this.TransportGroupBox);
			this.Controls.Add(this.HealthCertificateGroupBox);
			this.Controls.Add(this.HeaderDetailsGroupBox);
			this.Name = "RFPDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1171, 275, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RecommendationLettersGroupBox.ResumeLayout(false);
			this.RecommendationLettersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecommendationLettersGrid)).EndInit();
			this.RecommendationLettersGrid.ResumeLayout(false);
			this.RecommendationLettersGrid.PerformLayout();
			this.PrintLocationGroupBox.ResumeLayout(false);
			this.PrintLocationGroupBox.PerformLayout();
			this.QH_CertificateRequiredLocationCodeFindBox.ResumeLayout(true);
			this.QH_CertificateRequiredLocationCodeFindBox.PerformLayout();
			this.QH_PrintLocationDropEdit.ResumeLayout(true);
			this.QH_PrintLocationDropEdit.PerformLayout();
			this.QH_OH_PrintLocationOrganisationGuidFindBox.ResumeLayout(true);
			this.QH_OH_PrintLocationOrganisationGuidFindBox.PerformLayout();
			this.TransportGroupBox.ResumeLayout(false);
			this.TransportGroupBox.PerformLayout();
			this.QH_PackDateDateEdit.ResumeLayout(true);
			this.QH_PackDateDateEdit.PerformLayout();
			this.StorageTemperatureGroupBox.ResumeLayout(false);
			this.StorageTemperatureGroupBox.PerformLayout();
			this.QH_TemperatureUMDropEdit.ResumeLayout(true);
			this.QH_TemperatureUMDropEdit.PerformLayout();
			this.QH_RL_NKBorderInspectionPortCodeFindBox.ResumeLayout(true);
			this.QH_RL_NKBorderInspectionPortCodeFindBox.PerformLayout();
			this.QH_RN_NKOriginCountryCodeFindBox.ResumeLayout(true);
			this.QH_RN_NKOriginCountryCodeFindBox.PerformLayout();
			this.HealthCertificateGroupBox.ResumeLayout(false);
			this.HealthCertificateGroupBox.PerformLayout();
			this.QH_AQISRegionCodeFindBox.ResumeLayout(true);
			this.QH_AQISRegionCodeFindBox.PerformLayout();
			this.QH_CertificatePrintIndicatorDropEdit.ResumeLayout(true);
			this.QH_CertificatePrintIndicatorDropEdit.PerformLayout();
			this.HeaderDetailsGroupBox.ResumeLayout(false);
			this.HeaderDetailsGroupBox.PerformLayout();
			this.ProductUseDropEdit.ResumeLayout(true);
			this.ProductUseDropEdit.PerformLayout();
			this.QH_ProduceTypeDropEdit.ResumeLayout(true);
			this.QH_ProduceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox RecommendationLettersGroupBox;
		private ZArchitecture.ZGrid RecommendationLettersGrid;
		private ZArchitecture.GUI.ZGroupBox PrintLocationGroupBox;
		private ZArchitecture.ZTextBox QH_CertificateRequiredLocationTextBox;
		private ZArchitecture.GUI.ZCodeFindBox QH_CertificateRequiredLocationCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit QH_PrintLocationDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox QH_OH_PrintLocationOrganisationGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox TransportGroupBox;
		private ZArchitecture.GUI.ZDateEdit QH_PackDateDateEdit;
		private ZArchitecture.GUI.ZGroupBox StorageTemperatureGroupBox;
		private ZArchitecture.ZLabel orLabel;
		private ZArchitecture.ZCalcEdit QH_MaximumTemperatureCalcEdit;
		private ZArchitecture.ZCalcEdit QH_MinimumTemperatureCalcEdit;
		private ZArchitecture.ZCalcEdit QH_AbsoluteTemperatureCalcEdit;
		private ZArchitecture.GUI.ZDropEdit QH_TemperatureUMDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox QH_RL_NKBorderInspectionPortCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox QH_RN_NKOriginCountryCodeFindBox;
		private ZArchitecture.GUI.ZGroupBox HealthCertificateGroupBox;
		private ZArchitecture.ZTextBox QuotaTypeTextBox;
		private ZArchitecture.GUI.ZCodeFindBox QH_AQISRegionCodeFindBox;
		private ZArchitecture.GUI.ZCheckBox ShipStoresCheckBox;
		private ZArchitecture.ZTextBox AMLCQuotaYearTextBox;
		private ZArchitecture.GUI.ZCheckBox AMLCQuotaCheckBox;
		private ZArchitecture.GUI.ZCheckBox SplitPackerCheckBox;
		private ZArchitecture.GUI.ZCheckBox SplitMarksCheckBox;
		private ZArchitecture.GUI.ZCheckBox SplitContainerCheckBox;
		private ZArchitecture.GUI.ZDropEdit QH_CertificatePrintIndicatorDropEdit;
		private ZArchitecture.GUI.ZGroupBox HeaderDetailsGroupBox;
		private ZArchitecture.ZTextBox QH_ExemptionCodeTextBox;
		private ZArchitecture.ZTextBox QH_CustomsConsigneeNameTextBox;
		private ZArchitecture.GUI.ZDropEdit ProductUseDropEdit;
		private ZArchitecture.GUI.ZCheckBox GetCustomsEDNCheckBox;
		private ZArchitecture.GUI.ZDropEdit QH_ProduceTypeDropEdit;
		private ZArchitecture.ZTextBox ConsigneeAgentNameTextBox;
	}
}
