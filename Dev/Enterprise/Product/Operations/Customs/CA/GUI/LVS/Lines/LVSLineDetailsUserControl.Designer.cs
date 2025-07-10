namespace Enterprise.Customs.CA.GUI
{
	partial class LVSLineDetailsUserControl
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
			this.DescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SIMADumpingDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Tariff99CodeFindBox = new Enterprise.Customs.GUI.TariffFindBox();
			this.TreatmentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsQty3CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsQty2CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ClassificationNumberFindBox = new Enterprise.Customs.GUI.TariffFindBox();
			this.CustomsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LinePriceCurrencyControl = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.TRSNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorityNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RemissionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.USStateOfExportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StateOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfOriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RemissionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DescriptionTextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.Tariff99CodeFindBox.SuspendLayout();
			this.TreatmentCodeDropEdit.SuspendLayout();
			this.CustomsQty3CalcDropEdit.SuspendLayout();
			this.CustomsQty2CalcDropEdit.SuspendLayout();
			this.ClassificationNumberFindBox.SuspendLayout();
			this.CustomsQuantityCalcDropEdit.SuspendLayout();
			this.LinePriceCurrencyControl.SuspendLayout();
			this.RemissionTypeDropEdit.SuspendLayout();
			this.USStateOfExportDropEdit.SuspendLayout();
			this.CountryOfExportCodeFindBox.SuspendLayout();
			this.StateOfOriginDropEdit.SuspendLayout();
			this.CountryOfOriginFindBox.SuspendLayout();
			this.RemissionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceLine);
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.AllowDrop = true;
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "JI_Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 183, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 21, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Top;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 180, true);
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.SIMADumpingDescriptionTextBox);
			this.SplitContainer.Panel1.Controls.Add(this.Tariff99CodeFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.TreatmentCodeDropEdit);
			this.SplitContainer.Panel1.Controls.Add(this.CustomsQty3CalcDropEdit);
			this.SplitContainer.Panel1.Controls.Add(this.CustomsQty2CalcDropEdit);
			this.SplitContainer.Panel1.Controls.Add(this.ClassificationNumberFindBox);
			this.SplitContainer.Panel1.Controls.Add(this.CustomsQuantityCalcDropEdit);
			this.SplitContainer.Panel1.Controls.Add(this.LinePriceCurrencyControl);
			this.SplitContainer.Panel1MinSize = 244;
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.TRSNumberTextBox);
			this.SplitContainer.Panel2.Controls.Add(this.AuthorityNumberTextBox);
			this.SplitContainer.Panel2.Controls.Add(this.RemissionDropEdit);
			this.SplitContainer.Panel2.Controls.Add(this.RemissionTypeDropEdit);
			this.SplitContainer.Panel2.Controls.Add(this.USStateOfExportDropEdit);
			this.SplitContainer.Panel2.Controls.Add(this.CountryOfExportCodeFindBox);
			this.SplitContainer.Panel2.Controls.Add(this.StateOfOriginDropEdit);
			this.SplitContainer.Panel2.Controls.Add(this.CountryOfOriginFindBox);
			this.SplitContainer.Panel2MinSize = 204;
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(244);
			this.SplitContainer.SplitterWidth = 2;
			this.SplitContainer.TabIndex = 0;
			// 
			// SIMADumpingDescriptionTextBox
			// 
			this.SIMADumpingDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SIMADumpingDescriptionTextBox, "CA_SIMADumpingDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_SIMADumpingDesc)));
			this.SIMADumpingDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 160, true);
			this.SIMADumpingDescriptionTextBox.Name = "SIMADumpingDescriptionTextBox";
			this.SIMADumpingDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.SIMADumpingDescriptionTextBox.TabIndex = 7;
			// 
			// Tariff99CodeFindBox
			// 
			this.Tariff99CodeFindBox.AllowDrop = true;
			this.Tariff99CodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Tariff99CodeFindBox, "CA_99TariffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_99TariffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Common.TariffPropertyInfo)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_99TariffCodeTariffInfo)));
			this.Tariff99CodeFindBox.BindToTariffPropertyInfo = "CA_99TariffCodeTariffInfo";
			this.Tariff99CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 137, true);
			this.Tariff99CodeFindBox.Name = "Tariff99CodeFindBox";
			this.Tariff99CodeFindBox.PreBoundMaxLength = 4;
			this.Tariff99CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.Tariff99CodeFindBox.TabIndex = 6;
			// 
			// TreatmentCodeDropEdit
			// 
			this.TreatmentCodeDropEdit.AllowDrop = true;
			this.TreatmentCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TreatmentCodeDropEdit, "CA_TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_TreatmentCode)));
			this.TreatmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 114, true);
			this.TreatmentCodeDropEdit.Name = "TreatmentCodeDropEdit";
			this.TreatmentCodeDropEdit.PreBoundMaxLength = 2;
			this.TreatmentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.TreatmentCodeDropEdit.TabIndex = 5;
			// 
			// CustomsQty3CalcDropEdit
			// 
			this.CustomsQty3CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQty3CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_CustomsThirdUnitQty)));
			this.CustomsQty3CalcDropEdit.BindToAmount = "JI_CustomsThirdQuantity";
			this.CustomsQty3CalcDropEdit.BindToUnit = "JI_CustomsThirdUnitQty";
			this.CustomsQty3CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 69, true);
			this.CustomsQty3CalcDropEdit.Name = "CustomsQty3CalcDropEdit";
			this.CustomsQty3CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.CustomsQty3CalcDropEdit.TabIndex = 3;
			this.CustomsQty3CalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsQty2CalcDropEdit
			// 
			this.CustomsQty2CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQty2CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_CustomsSecondUnitQty)));
			this.CustomsQty2CalcDropEdit.BindToAmount = "JI_CustomsSecondQuantity";
			this.CustomsQty2CalcDropEdit.BindToUnit = "JI_CustomsSecondUnitQty";
			this.CustomsQty2CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 46, true);
			this.CustomsQty2CalcDropEdit.Name = "CustomsQty2CalcDropEdit";
			this.CustomsQty2CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.CustomsQty2CalcDropEdit.TabIndex = 2;
			this.CustomsQty2CalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ClassificationNumberFindBox
			// 
			this.ClassificationNumberFindBox.AllowDrop = true;
			this.ClassificationNumberFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClassificationNumberFindBox, "JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Common.TariffPropertyInfo)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_FormattedTariffCodeTariffInfo)));
			this.ClassificationNumberFindBox.BindToTariffPropertyInfo = "JI_FormattedTariffCodeTariffInfo";
			this.ClassificationNumberFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSLineDetailsUserControl|fe3e4ea0-39af-477f-9ab0-0fb77667ba82", "HS", "Class. Tariff #", "Classification Tariff Code", "Must be a 10-digit Canadian National Customs Tariff code.");
			this.ClassificationNumberFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 1, true);
			this.ClassificationNumberFindBox.Name = "ClassificationNumberFindBox";
			this.ClassificationNumberFindBox.PreBoundMaxLength = 10;
			this.ClassificationNumberFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.ClassificationNumberFindBox.TabIndex = 0;
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_CustomsUnitQty)));
			this.CustomsQuantityCalcDropEdit.BindToAmount = "JI_CustomsQuantity";
			this.CustomsQuantityCalcDropEdit.BindToUnit = "JI_CustomsUnitQty";
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 23, true);
			this.CustomsQuantityCalcDropEdit.Name = "CustomsQuantityCalcDropEdit";
			this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 1;
			this.CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// LinePriceCurrencyControl
			// 
			this.LinePriceCurrencyControl.AllowDrop = true;
			this.LinePriceCurrencyControl.BindToAmount = "JI_LinePrice";
			this.LinePriceCurrencyControl.BindToUnit = "JI_RX_NKLinePriceCurr";
			this.LinePriceCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LinePriceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 92, true);
			this.LinePriceCurrencyControl.Name = "LinePriceCurrencyControl";
			this.LinePriceCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.LinePriceCurrencyControl.TabIndex = 4;
			// 
			// TRSNumberTextBox
			// 
			this.TRSNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TRSNumberTextBox, "CA_TRSNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_TRSNumber)));
			this.TRSNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 114, true);
			this.TRSNumberTextBox.Name = "TRSNumberTextBox";
			this.TRSNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TRSNumberTextBox.TabIndex = 5;
			// 
			// AuthorityNumberTextBox
			// 
			this.AuthorityNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AuthorityNumberTextBox, "CA_AuthorityNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_AuthorityNumber)));
			this.AuthorityNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 92, true);
			this.AuthorityNumberTextBox.Name = "AuthorityNumberTextBox";
			this.AuthorityNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AuthorityNumberTextBox.TabIndex = 4;
			// 
			// RemissionTypeDropEdit
			// 
			this.RemissionTypeDropEdit.AllowDrop = true;
			this.RemissionTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RemissionTypeDropEdit, "CA_RemissionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_RemissionType)));
			this.RemissionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 137, true);
			this.RemissionTypeDropEdit.Name = "RemissionTypeDropEdit";
			this.RemissionTypeDropEdit.PreBoundMaxLength = 2;
			this.RemissionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RemissionTypeDropEdit.TabIndex = 6;
			// 
			// USStateOfExportDropEdit
			// 
			this.USStateOfExportDropEdit.AllowDrop = true;
			this.USStateOfExportDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.USStateOfExportDropEdit, "CA_USStateOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_USStateOfExport)));
			this.USStateOfExportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 69, true);
			this.USStateOfExportDropEdit.Name = "USStateOfExportDropEdit";
			this.USStateOfExportDropEdit.PreBoundMaxLength = 2;
			this.USStateOfExportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.USStateOfExportDropEdit.TabIndex = 3;
			// 
			// CountryOfExportCodeFindBox
			// 
			this.CountryOfExportCodeFindBox.AllowDrop = true;
			this.CountryOfExportCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryOfExportCodeFindBox, "CA_RN_NKExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_RN_NKExport)));
			this.CountryOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 46, true);
			this.CountryOfExportCodeFindBox.Name = "CountryOfExportCodeFindBox";
			this.CountryOfExportCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CountryOfExportCodeFindBox.TabIndex = 2;
			// 
			// StateOfOriginDropEdit
			// 
			this.StateOfOriginDropEdit.AllowDrop = true;
			this.StateOfOriginDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StateOfOriginDropEdit, "JI_StateOrRegionOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_StateOrRegionOfOrigin)));
			this.StateOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 23, true);
			this.StateOfOriginDropEdit.Name = "StateOfOriginDropEdit";
			this.StateOfOriginDropEdit.PreBoundMaxLength = 2;
			this.StateOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.StateOfOriginDropEdit.TabIndex = 1;
			// 
			// CountryOfOriginFindBox
			// 
			this.CountryOfOriginFindBox.AllowDrop = true;
			this.CountryOfOriginFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryOfOriginFindBox, "JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_CountryOfOrigin)));
			this.CountryOfOriginFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSLineDetailsUserControl|1eabd67f-c2e5-4efa-908d-87c65c34b567", "Ctry/Rgn. of Origin", "Country/Region of Origin", "Default Country/Region of Origin", "The country/region where the goods are grown, produced or manufactured.");
			this.CountryOfOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 1, true);
			this.CountryOfOriginFindBox.Name = "CountryOfOriginFindBox";
			this.CountryOfOriginFindBox.PreBoundMaxLength = 2;
			this.CountryOfOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.CountryOfOriginFindBox.TabIndex = 0;
			// 
			// RemissionDropEdit
			// 
			this.RemissionDropEdit.AllowDrop = true;
			this.RemissionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RemissionDropEdit, "CA_CalculationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CalculationMethod)));
			this.RemissionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 160, true);
			this.RemissionDropEdit.Name = "RemissionDropEdit";
			this.RemissionDropEdit.PreBoundMaxLength = 2;
			this.RemissionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RemissionDropEdit.TabIndex = 7;
			// 
			// LVSLineDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Controls.Add(this.DescriptionTextBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 185, true);
			this.Name = "LVSLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 185, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionTextBox.ResumeLayout(true);
			this.DescriptionTextBox.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel1.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.Tariff99CodeFindBox.ResumeLayout(true);
			this.Tariff99CodeFindBox.PerformLayout();
			this.TreatmentCodeDropEdit.ResumeLayout(true);
			this.TreatmentCodeDropEdit.PerformLayout();
			this.CustomsQty3CalcDropEdit.ResumeLayout(true);
			this.CustomsQty3CalcDropEdit.PerformLayout();
			this.CustomsQty2CalcDropEdit.ResumeLayout(true);
			this.CustomsQty2CalcDropEdit.PerformLayout();
			this.ClassificationNumberFindBox.ResumeLayout(true);
			this.ClassificationNumberFindBox.PerformLayout();
			this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsQuantityCalcDropEdit.PerformLayout();
			this.LinePriceCurrencyControl.ResumeLayout(true);
			this.LinePriceCurrencyControl.PerformLayout();
			this.RemissionTypeDropEdit.ResumeLayout(true);
			this.RemissionTypeDropEdit.PerformLayout();
			this.USStateOfExportDropEdit.ResumeLayout(true);
			this.USStateOfExportDropEdit.PerformLayout();
			this.CountryOfExportCodeFindBox.ResumeLayout(true);
			this.CountryOfExportCodeFindBox.PerformLayout();
			this.StateOfOriginDropEdit.ResumeLayout(true);
			this.StateOfOriginDropEdit.PerformLayout();
			this.CountryOfOriginFindBox.ResumeLayout(true);
			this.CountryOfOriginFindBox.PerformLayout();
			this.RemissionDropEdit.ResumeLayout(true);
			this.RemissionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Customs.GUI.LongTextControl DescriptionTextBox;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.Customs.GUI.TariffFindBox Tariff99CodeFindBox;
		private ZArchitecture.GUI.ZDropEdit TreatmentCodeDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit CustomsQty3CalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit CustomsQty2CalcDropEdit;
		private Enterprise.Customs.GUI.TariffFindBox ClassificationNumberFindBox;
		private ZArchitecture.GUI.ZCalcDropEdit CustomsQuantityCalcDropEdit;
		private ZArchitecture.GUI.ZCalcFindBox LinePriceCurrencyControl;
		private ZArchitecture.ZTextBox TRSNumberTextBox;
		private ZArchitecture.ZTextBox AuthorityNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit USStateOfExportDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfExportCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit StateOfOriginDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfOriginFindBox;
		internal ZArchitecture.GUI.ZDropEdit RemissionTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox SIMADumpingDescriptionTextBox;
		internal ZArchitecture.GUI.ZDropEdit RemissionDropEdit;
	}
}
