using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class AUOrgSupplierPartFormCustomsControl
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
			if (disposing && components != null)
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ExportClassGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportTariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.UniversalTariffImportFindBox();
			this.ImportTariffFindBoxAUCClass = new Enterprise.Customs.AU.Declaration.GUI.AUCClassFindBox();
			this.ExportTariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox();
			this.ExportTariffFindBoxAHECC = new Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox();
			this.LastAuditDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AuditedByCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ClassificationFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImportClassGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportClassificationAddInfoStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LkUpInstrumentCode = new Enterprise.ZArchitecture.ZTextBox();
			this.LkUpInstrumentType = new Enterprise.ZArchitecture.ZTextBox();
			this.LkUpTreatmentCode = new Enterprise.ZArchitecture.ZTextBox();
			this.InstrumentNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OP_CI_AddInfoBoundAddInfoCMRControl = new Enterprise.Customs.AU.Declaration.GUI.AddInfoControlOptionalCMR();
			this.InstrumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TreatmentCodeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CMRDefaultCPDecAnswersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.fAQISUserControl = new Enterprise.Customs.AU.Declaration.GUI.AQISUserControl();
			this.HTIQuarantineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HTEQuarantineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.fHTEAQISUserControl = new Enterprise.Customs.AU.Declaration.GUI.HTEAQISUserControl();
			this.HTIPermitsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ICSPermitsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ICSPermitsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.detailsPanel.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.CI_UsageCommentTextBox.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).BeginInit();
			this.AttributesLeftSplitContainer.Panel1.SuspendLayout();
			this.AttributesLeftSplitContainer.Panel2.SuspendLayout();
			this.AttributesLeftSplitContainer.SuspendLayout();
			this.Attributes1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).BeginInit();
			this.Attributes1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).BeginInit();
			this.AttributesRightSplitContainer.Panel1.SuspendLayout();
			this.AttributesRightSplitContainer.Panel2.SuspendLayout();
			this.AttributesRightSplitContainer.SuspendLayout();
			this.Attributes2GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).BeginInit();
			this.Attributes2Grid.SuspendLayout();
			this.Attributes3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).BeginInit();
			this.Attributes3Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportClassGroupBox.SuspendLayout();
			this.ImportTariffFindBox.SuspendLayout();
			this.ImportTariffFindBoxAUCClass.SuspendLayout();
			this.ExportTariffFindBox.SuspendLayout();
			this.ExportTariffFindBoxAHECC.SuspendLayout();
			this.LastAuditDateDateEdit.SuspendLayout();
			this.AuditedByCodeFindBox.SuspendLayout();
			this.ClassificationFindBox.SuspendLayout();
			this.ImportClassGroupBox.SuspendLayout();
			this.InstrumentNumberCodeFindBox.SuspendLayout();
			this.OP_CI_AddInfoBoundAddInfoCMRControl.SuspendLayout();
			this.InstrumentTypeDropEdit.SuspendLayout();
			this.TreatmentCodeBoundDropEdit.SuspendLayout();
			this.fAQISUserControl.SuspendLayout();
			this.HTIQuarantineTabPage.SuspendLayout();
			this.HTEQuarantineTabPage.SuspendLayout();
			this.fHTEAQISUserControl.SuspendLayout();
			this.HTIPermitsTabPage.SuspendLayout();
			this.ICSPermitsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ICSPermitsGrid)).BeginInit();
			this.ICSPermitsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// detailsPanel
			// 
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 292, true);
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Controls.Add(this.HTIPermitsTabPage);
			this.DetailTabControl.Controls.Add(this.HTIQuarantineTabPage);
			this.DetailTabControl.Controls.Add(this.HTEQuarantineTabPage);
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 252, true);
			this.DetailTabControl.TabIndex = 0;
			this.DetailTabControl.Controls.SetChildIndex(this.HTEQuarantineTabPage, 0);
			this.DetailTabControl.Controls.SetChildIndex(this.HTIQuarantineTabPage, 0);
			this.DetailTabControl.Controls.SetChildIndex(this.HTIPermitsTabPage, 0);
			this.DetailTabControl.Controls.SetChildIndex(this.AttributesTabPage, 0);
			this.DetailTabControl.Controls.SetChildIndex(this.DetailsTabPage, 0);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.ExportClassGroupBox);
			this.DetailsTabPage.Controls.Add(this.ImportClassGroupBox);
			this.DetailsTabPage.Controls.Add(this.CMRDefaultCPDecAnswersButton);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 225, true);
			this.DetailsTabPage.Controls.SetChildIndex(this.CMRDefaultCPDecAnswersButton, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.CI_UsageCommentTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.ImportClassGroupBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.ExportClassGroupBox, 0);
			// 
			// CI_UsageCommentTextBox
			// 
			this.CI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 20, true);
			this.CI_UsageCommentTextBox.TabIndex = 0;
			// 
			// AttributesTabPage
			// 
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 225, true);
			// 
			// AttributesLeftSplitContainer
			// 
			this.AttributesLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 219, true);
			this.AttributesLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(365);
			// 
			// Attributes1GroupBox
			// 
			this.Attributes1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 219, true);
			// 
			// Attributes1Grid
			// 
			this.Attributes1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 200, true);
			// 
			// AttributesRightSplitContainer
			// 
			this.AttributesRightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 219, true);
			this.AttributesRightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(268);
			// 
			// Attributes2GroupBox
			// 
			this.Attributes2GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 219, true);
			// 
			// Attributes2Grid
			// 
			this.Attributes2Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 200, true);
			// 
			// Attributes3GroupBox
			// 
			this.Attributes3GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 219, true);
			// 
			// Attributes3Grid
			// 
			this.Attributes3Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 200, true);
			// 
			// PivotGrid
			// 
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 90, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart);
			// 
			// ExportClassGroupBox
			// 
			this.ExportClassGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportClassGroupBox.Controls.Add(this.ImportTariffFindBox);
			this.ExportClassGroupBox.Controls.Add(this.ImportTariffFindBoxAUCClass);
			this.ExportClassGroupBox.Controls.Add(this.ExportTariffFindBox);
			this.ExportClassGroupBox.Controls.Add(this.ExportTariffFindBoxAHECC);
			this.ExportClassGroupBox.Controls.Add(this.LastAuditDateDateEdit);
			this.ExportClassGroupBox.Controls.Add(this.AuditedByCodeFindBox);
			this.ExportClassGroupBox.Controls.Add(this.ClassificationFindBox);
			this.ExportClassGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 74, true);
			this.ExportClassGroupBox.Name = "ExportClassGroupBox";
			this.ExportClassGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 72, true);
			this.ExportClassGroupBox.TabIndex = 1;
			this.ExportClassGroupBox.TabStop = false;
			this.ExportClassGroupBox.Text = "General Classification";
			// 
			// ClassificationDescriptionTextBox
			//
			this.ClassificationDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 36, true);
			this.ClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 20, true);
			// 
			// ImportTariffFindBox
			// 
			this.ImportTariffFindBox.AllowDrop = true;
			this.ImportTariffFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ImportTariffFindBox, "PivotsForBinding.CI_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNum)));
			this.ImportTariffFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("C4E43E10-F09F-4A7F-97FB-3FD4B1EA11EF", "Tariff");
			this.ImportTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.ImportTariffFindBox.Name = "ImportTariffFindBox";
			this.ImportTariffFindBox.PreBoundMaxLength = 10;
			this.ImportTariffFindBox.ShouldResize = true;
			this.ImportTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.ImportTariffFindBox.TabIndex = 1;
			this.ImportTariffFindBox.Visible = false;
			// 
			// ImportTariffFindBoxAUCClass
			// 
			this.ImportTariffFindBoxAUCClass.AllowDrop = true;
			this.ImportTariffFindBoxAUCClass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ImportTariffFindBoxAUCClass, "PivotsForBinding.CI_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNum)));
			this.ImportTariffFindBoxAUCClass.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("C4E43E10-F09F-4A7F-97FB-3FD4B1EA11EF", "Tariff");
			this.ImportTariffFindBoxAUCClass.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.ImportTariffFindBoxAUCClass.Name = "ImportTariffFindBoxAUCClass";
			this.ImportTariffFindBoxAUCClass.PreBoundMaxLength = 10;
			this.ImportTariffFindBoxAUCClass.ShouldResize = true;
			this.ImportTariffFindBoxAUCClass.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.ImportTariffFindBoxAUCClass.TabIndex = 1;
			this.ImportTariffFindBoxAUCClass.Visible = false;
			// 
			// ExportTariffFindBox
			// 
			this.ExportTariffFindBox.AllowDrop = true;
			this.ExportTariffFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExportTariffFindBox, "PivotsForBinding.CI_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNum)));
			this.ExportTariffFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("d80a7e04-ae82-4454-b617-ee481a7d1fd7", "Tariff");
			this.ExportTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.ExportTariffFindBox.Name = "ExportTariffFindBox";
			this.ExportTariffFindBox.PreBoundMaxLength = 10;
			this.ExportTariffFindBox.ShouldResize = true;
			this.ExportTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.ExportTariffFindBox.TabIndex = 1;
			this.ExportTariffFindBox.Visible = false;
			// 
			// ExportTariffFindBoxAHECC
			// 
			this.ExportTariffFindBoxAHECC.AllowDrop = true;
			this.ExportTariffFindBoxAHECC.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExportTariffFindBoxAHECC, "PivotsForBinding.CI_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNum)));
			this.ExportTariffFindBoxAHECC.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("d80a7e04-ae82-4454-b617-ee481a7d1fd7", "Tariff");
			this.ExportTariffFindBoxAHECC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.ExportTariffFindBoxAHECC.Name = "ExportTariffFindBoxAHECC";
			this.ExportTariffFindBoxAHECC.PreBoundMaxLength = 10;
			this.ExportTariffFindBoxAHECC.ShouldResize = true;
			this.ExportTariffFindBoxAHECC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.ExportTariffFindBoxAHECC.TabIndex = 1;
			this.ExportTariffFindBoxAHECC.Visible = false;
			// 
			// LastAuditDateDateEdit
			// 
			this.LastAuditDateDateEdit.AllowDrop = true;
			this.LastAuditDateDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LastAuditDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastAuditDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastAuditDateDateEdit, "PivotsForBinding.CI_LastAuditedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_LastAuditedDate)));
			this.LastAuditDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(695, 45, true);
			this.LastAuditDateDateEdit.Name = "LastAuditDateDateEdit";
			this.LastAuditDateDateEdit.TabIndex = 3;
			// 
			// AuditedByCodeFindBox
			// 
			this.AuditedByCodeFindBox.AllowDrop = true;
			this.AuditedByCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AuditedByCodeFindBox, "PivotsForBinding.CI_LastAuditedUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_LastAuditedUser)));
			this.AuditedByCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(695, 19, true);
			this.AuditedByCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.AuditedByCodeFindBox.Name = "AuditedByCodeFindBox";
			this.AuditedByCodeFindBox.ShouldResize = true;
			this.AuditedByCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.AuditedByCodeFindBox.TabIndex = 2;
			// 
			// ClassificationFindBox
			// 
			this.ClassificationFindBox.AllowDrop = true;
			this.ClassificationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClassificationFindBox, "PivotsForBinding.CI_CC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC)));
			this.ClassificationFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|18267989-663D-4D7F-8988-B7C5441CC570", "Look Up");
			this.ClassificationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ClassificationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 19, true);
			this.ClassificationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ExportClassification;
			this.ClassificationFindBox.Name = "ClassificationFindBox";
			this.ClassificationFindBox.PreBoundMaxLength = 35;
			this.ClassificationFindBox.ShouldResize = true;
			this.ClassificationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 20, true);
			this.ClassificationFindBox.TabIndex = 0;
			// 
			// ImportClassGroupBox
			// 
			this.ImportClassGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportClassGroupBox.Controls.Add(this.ImportClassificationAddInfoStringTextBox);
			this.ImportClassGroupBox.Controls.Add(this.LkUpInstrumentCode);
			this.ImportClassGroupBox.Controls.Add(this.LkUpInstrumentType);
			this.ImportClassGroupBox.Controls.Add(this.LkUpTreatmentCode);
			this.ImportClassGroupBox.Controls.Add(this.InstrumentNumberCodeFindBox);
			this.ImportClassGroupBox.Controls.Add(this.OP_CI_AddInfoBoundAddInfoCMRControl);
			this.ImportClassGroupBox.Controls.Add(this.InstrumentTypeDropEdit);
			this.ImportClassGroupBox.Controls.Add(this.TreatmentCodeBoundDropEdit);
			this.ImportClassGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 152, true);
			this.ImportClassGroupBox.Name = "ImportClassGroupBox";
			this.ImportClassGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 99, true);
			this.ImportClassGroupBox.TabIndex = 2;
			this.ImportClassGroupBox.TabStop = false;
			this.ImportClassGroupBox.Text = "Import Classification";
			// 
			// ImportClassificationAddInfoStringTextBox
			// 
			this.ImportClassificationAddInfoStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportClassificationAddInfoStringTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ImportClassificationAddInfoStringTextBox, "PivotsForBinding.ClassificationAddInfoString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ClassificationAddInfoString)));
			this.ImportClassificationAddInfoStringTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|0C24AD62-DCB6-42DA-AA8F-54FF25CF890B", "Look Up");
			this.ImportClassificationAddInfoStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.ImportClassificationAddInfoStringTextBox.Name = "ImportClassificationAddInfoStringTextBox";
			this.ImportClassificationAddInfoStringTextBox.ReadOnly = true;
			this.ImportClassificationAddInfoStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 20, true);
			this.ImportClassificationAddInfoStringTextBox.TabIndex = 1;
			// 
			// LkUpInstrumentCode
			// 
			this.LkUpInstrumentCode.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LkUpInstrumentCode, "PivotsForBinding.ImportClassInstrumentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ImportClassInstrumentCode)));
			this.LkUpInstrumentCode.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|ACCCAC71-D616-4EDD-89BC-A5DFFA50E1A8", "Look Up");
			this.LkUpInstrumentCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(879, 71, true);
			this.LkUpInstrumentCode.Name = "LkUpInstrumentCode";
			this.LkUpInstrumentCode.ReadOnly = true;
			this.LkUpInstrumentCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.LkUpInstrumentCode.TabIndex = 7;
			// 
			// LkUpInstrumentType
			// 
			this.LkUpInstrumentType.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LkUpInstrumentType, "PivotsForBinding.ImportClassInstrumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ImportClassInstrumentType)));
			this.LkUpInstrumentType.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|4756A448-7386-4B42-A374-20AF7C4E78A5", "Look Up");
			this.LkUpInstrumentType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 71, true);
			this.LkUpInstrumentType.Name = "LkUpInstrumentType";
			this.LkUpInstrumentType.ReadOnly = true;
			this.LkUpInstrumentType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.LkUpInstrumentType.TabIndex = 5;
			// 
			// LkUpTreatmentCode
			// 
			this.LkUpTreatmentCode.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LkUpTreatmentCode, "PivotsForBinding.ImportClassTreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ImportClassTreatmentCode)));
			this.LkUpTreatmentCode.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|64E69A3E-697A-4897-A877-3579CB4A91FE", "Look Up");
			this.LkUpTreatmentCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 71, true);
			this.LkUpTreatmentCode.Name = "LkUpTreatmentCode";
			this.LkUpTreatmentCode.ReadOnly = true;
			this.LkUpTreatmentCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.LkUpTreatmentCode.TabIndex = 3;
			// 
			// InstrumentNumberCodeFindBox
			// 
			this.InstrumentNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InstrumentNumberCodeFindBox, "PivotsForBinding.AddInfo.ZA_InstrumentCode_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.ZA_InstrumentCode_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.Lookups.CMRInstrumentNumberList)));
			this.InstrumentNumberCodeFindBox.BindToList = "PivotsForBinding.AddInfo.Lookups.CMRInstrumentNumberList";
			this.InstrumentNumberCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|CF3D8E6A-C380-4C3E-B1A2-5AEEAD032DF5", "Treatment Inst. No");
			this.InstrumentNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(722, 71, true);
			this.InstrumentNumberCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.InstrumentNumber;
			this.InstrumentNumberCodeFindBox.Name = "InstrumentNumberCodeFindBox";
			this.InstrumentNumberCodeFindBox.PopupCaption = "Instrument Number";
			this.InstrumentNumberCodeFindBox.PreBoundMaxLength = 8;
			this.InstrumentNumberCodeFindBox.ShouldResize = true;
			this.InstrumentNumberCodeFindBox.ShowDescriptionBox = false;
			this.InstrumentNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.InstrumentNumberCodeFindBox.TabIndex = 6;
			this.InstrumentNumberCodeFindBox.Tag = "";
			// 
			// OP_CI_AddInfoBoundAddInfoCMRControl
			// 
			this.OP_CI_AddInfoBoundAddInfoCMRControl.AllowDrop = true;
			this.OP_CI_AddInfoBoundAddInfoCMRControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OP_CI_AddInfoBoundAddInfoCMRControl, "PivotsForBinding.AddInfo.AddInfoLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.AddInfoLine)));
			this.OP_CI_AddInfoBoundAddInfoCMRControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|AB736B6B-257F-47B4-A279-ACFC2EF2E98F", "Add Info");
			this.OP_CI_AddInfoBoundAddInfoCMRControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 19, true);
			this.OP_CI_AddInfoBoundAddInfoCMRControl.Name = "OP_CI_AddInfoBoundAddInfoCMRControl";
			this.OP_CI_AddInfoBoundAddInfoCMRControl.ShowCMRAddInfo = true;
			this.OP_CI_AddInfoBoundAddInfoCMRControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 20, true);
			this.OP_CI_AddInfoBoundAddInfoCMRControl.TabIndex = 0;
			// 
			// InstrumentTypeDropEdit
			// 
			this.InstrumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InstrumentTypeDropEdit, "PivotsForBinding.AddInfo.ZA_InstrumentType_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.ZA_InstrumentType_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).InstrumentTypeList)));
			this.InstrumentTypeDropEdit.BindToList = "PivotsForBinding.InstrumentTypeList";
			this.InstrumentTypeDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|1EC07D13-E8C9-470B-A294-4DF5E073F268", "Treatment Inst. Type");
			this.InstrumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 71, true);
			this.InstrumentTypeDropEdit.Name = "InstrumentTypeDropEdit";
			this.InstrumentTypeDropEdit.PreBoundMaxLength = 3;
			this.InstrumentTypeDropEdit.ShowDescriptionBox = false;
			this.InstrumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.InstrumentTypeDropEdit.TabIndex = 4;
			// 
			// TreatmentCodeBoundDropEdit
			// 
			this.TreatmentCodeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TreatmentCodeBoundDropEdit, "PivotsForBinding.AddInfo.ZA_TreatmentCode_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.ZA_TreatmentCode_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).TreatmentCodeList)));
			this.TreatmentCodeBoundDropEdit.BindToList = "PivotsForBinding.TreatmentCodeList";
			this.TreatmentCodeBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|EF1B5E2D-4299-42FA-B213-5DD379DF3667", "Treatment Code");
			this.TreatmentCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 71, true);
			this.TreatmentCodeBoundDropEdit.Name = "TreatmentCodeBoundDropEdit";
			this.TreatmentCodeBoundDropEdit.PreBoundMaxLength = 3;
			this.TreatmentCodeBoundDropEdit.ShowDescriptionBox = false;
			this.TreatmentCodeBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.TreatmentCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TreatmentCodeBoundDropEdit.TabIndex = 2;
			// 
			// CMRDefaultCPDecAnswersButton
			// 
			this.CMRDefaultCPDecAnswersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CMRDefaultCPDecAnswersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(827, 8, true);
			this.CMRDefaultCPDecAnswersButton.Name = "CMRDefaultCPDecAnswersButton";
			this.CMRDefaultCPDecAnswersButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CMRDefaultCPDecAnswersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.CMRDefaultCPDecAnswersButton.TabIndex = 4;
			this.CMRDefaultCPDecAnswersButton.Text = "CP Dec Answers";
			this.CMRDefaultCPDecAnswersButton.ToolTipCaption = null;
			this.CMRDefaultCPDecAnswersButton.Click += new System.EventHandler(this.CMRDefaultCPDecAnswersButton_Click);
			// 
			// fAQISUserControl
			// 
			this.fAQISUserControl.AllowDrop = true;
			this.fAQISUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fAQISUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fAQISUserControl.Name = "fAQISUserControl";
			this.fAQISUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 225, true);
			this.fAQISUserControl.TabIndex = 0;
			// 
			// HTIQuarantineTabPage
			// 
			this.HTIQuarantineTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|28BA8C77-E755-44C9-8200-1D6991EFCEF8", "Quarantine");
			this.HTIQuarantineTabPage.Controls.Add(this.fAQISUserControl);
			this.HTIQuarantineTabPage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.HTIQuarantineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HTIQuarantineTabPage.Name = "HTIQuarantineTabPage";
			this.HTIQuarantineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 225, true);
			this.HTIQuarantineTabPage.TabIndex = 0;
			// 
			// HTEQuarantineTabPage
			// 
			this.HTEQuarantineTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("2dde82ea-1aad-48b1-a263-573c076b6de7", "Quarantine");
			this.HTEQuarantineTabPage.Controls.Add(this.fHTEAQISUserControl);
			this.HTEQuarantineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HTEQuarantineTabPage.Name = "HTEQuarantineTabPage";
			this.HTEQuarantineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 115, true);
			this.HTEQuarantineTabPage.TabIndex = 8;
			this.HTEQuarantineTabPage.Text = "Quarantine";
			// 
			// fHTEAQISUserControl
			// 
			this.fHTEAQISUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.fHTEAQISUserControl, "PivotsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)))));
			this.fHTEAQISUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fHTEAQISUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fHTEAQISUserControl.Name = "fHTEAQISUserControl";
			this.fHTEAQISUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 225, true);
			this.fHTEAQISUserControl.TabIndex = 1;
			// 
			// HTIPermitsTabPage
			// 
			this.HTIPermitsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.HTIPermitsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("1e79fac7-0d92-4e0b-a339-776c5846e78f", "Permits");
			this.HTIPermitsTabPage.Controls.Add(this.ICSPermitsGroupBox);
			this.HTIPermitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HTIPermitsTabPage.Name = "HTIPermitsTabPage";
			this.HTIPermitsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HTIPermitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 225, true);
			this.HTIPermitsTabPage.TabIndex = 9;
			// 
			// ICSPermitsGroupBox
			// 
			this.ICSPermitsGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9dae6263-c393-4945-b511-b13894e0e933", "Permits");
			this.ICSPermitsGroupBox.Controls.Add(this.ICSPermitsGrid);
			this.ICSPermitsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ICSPermitsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ICSPermitsGroupBox.Name = "ICSPermitsGroupBox";
			this.ICSPermitsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 219, true);
			this.ICSPermitsGroupBox.TabIndex = 0;
			this.ICSPermitsGroupBox.TabStop = false;
			// 
			// ICSPermitsGrid
			// 
			this.ICSPermitsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ICSPermitsGrid, "PivotsForBinding.ICSPermits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ICSPermits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ICSPermit)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ICSPermits)).SyncRoot)).CY_Data)));
			this.ICSPermitsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9a3a0ec8-0ba3-4cd0-8bd2-311248931247", "Number", "Permit Number", "");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ICSPermitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ICSPermitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ICSPermitsGrid.GridId = "497cf311-c6e2-4037-ad6b-a3d72e981a92";
			this.ICSPermitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ICSPermitsGrid.LayoutKey = "ICSPermitsGrid";
			this.ICSPermitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ICSPermitsGrid.Name = "ICSPermitsGrid";
			this.ICSPermitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 200, true);
			this.ICSPermitsGrid.TabIndex = 0;
			// 
			// AUOrgSupplierPartFormCustomsControl
			// 
			this.Name = "AUOrgSupplierPartFormCustomsControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 382, true);
			this.detailsPanel.ResumeLayout(false);
			this.detailsPanel.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.CI_UsageCommentTextBox.ResumeLayout(true);
			this.CI_UsageCommentTextBox.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.AttributesLeftSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).EndInit();
			this.AttributesLeftSplitContainer.ResumeLayout(false);
			this.AttributesLeftSplitContainer.PerformLayout();
			this.Attributes1GroupBox.ResumeLayout(false);
			this.Attributes1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).EndInit();
			this.Attributes1Grid.ResumeLayout(false);
			this.Attributes1Grid.PerformLayout();
			this.AttributesRightSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).EndInit();
			this.AttributesRightSplitContainer.ResumeLayout(false);
			this.AttributesRightSplitContainer.PerformLayout();
			this.Attributes2GroupBox.ResumeLayout(false);
			this.Attributes2GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).EndInit();
			this.Attributes2Grid.ResumeLayout(false);
			this.Attributes2Grid.PerformLayout();
			this.Attributes3GroupBox.ResumeLayout(false);
			this.Attributes3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).EndInit();
			this.Attributes3Grid.ResumeLayout(false);
			this.Attributes3Grid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportClassGroupBox.ResumeLayout(false);
			this.ExportClassGroupBox.PerformLayout();
			this.ExportTariffFindBoxAHECC.ResumeLayout(true);
			this.ExportTariffFindBoxAHECC.PerformLayout();
			this.ExportTariffFindBox.ResumeLayout(true);
			this.ExportTariffFindBox.PerformLayout();
			this.ImportTariffFindBoxAUCClass.ResumeLayout(true);
			this.ImportTariffFindBoxAUCClass.PerformLayout();
			this.ImportTariffFindBox.ResumeLayout(true);
			this.ImportTariffFindBox.PerformLayout();
			this.LastAuditDateDateEdit.ResumeLayout(true);
			this.LastAuditDateDateEdit.PerformLayout();
			this.AuditedByCodeFindBox.ResumeLayout(true);
			this.AuditedByCodeFindBox.PerformLayout();
			this.ClassificationFindBox.ResumeLayout(true);
			this.ClassificationFindBox.PerformLayout();
			this.ImportClassGroupBox.ResumeLayout(false);
			this.ImportClassGroupBox.PerformLayout();
			this.InstrumentNumberCodeFindBox.ResumeLayout(true);
			this.InstrumentNumberCodeFindBox.PerformLayout();
			this.OP_CI_AddInfoBoundAddInfoCMRControl.ResumeLayout(true);
			this.OP_CI_AddInfoBoundAddInfoCMRControl.PerformLayout();
			this.InstrumentTypeDropEdit.ResumeLayout(true);
			this.InstrumentTypeDropEdit.PerformLayout();
			this.TreatmentCodeBoundDropEdit.ResumeLayout(true);
			this.TreatmentCodeBoundDropEdit.PerformLayout();
			this.fAQISUserControl.ResumeLayout(true);
			this.fAQISUserControl.PerformLayout();
			this.HTIQuarantineTabPage.ResumeLayout(false);
			this.HTIQuarantineTabPage.PerformLayout();
			this.HTEQuarantineTabPage.ResumeLayout(false);
			this.HTEQuarantineTabPage.PerformLayout();
			this.fHTEAQISUserControl.ResumeLayout(true);
			this.fHTEAQISUserControl.PerformLayout();
			this.HTIPermitsTabPage.ResumeLayout(false);
			this.HTIPermitsTabPage.PerformLayout();
			this.ICSPermitsGroupBox.ResumeLayout(false);
			this.ICSPermitsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ICSPermitsGrid)).EndInit();
			this.ICSPermitsGrid.ResumeLayout(false);
			this.ICSPermitsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox ExportClassGroupBox;
		private ZDateEdit LastAuditDateDateEdit;
		private ZCodeFindBox AuditedByCodeFindBox;
		private ZGuidFindBox ClassificationFindBox;
		private ZGroupBox ImportClassGroupBox;
		private ZCodeFindBox InstrumentNumberCodeFindBox;
		private ZButton CMRDefaultCPDecAnswersButton;
		private AddInfoControlOptionalCMR OP_CI_AddInfoBoundAddInfoCMRControl;
		private ZDropEdit InstrumentTypeDropEdit;
		private ZDropEdit TreatmentCodeBoundDropEdit;
		private ZTextBox LkUpTreatmentCode;
		private ZTextBox LkUpInstrumentType;
		private ZTextBox LkUpInstrumentCode;
		private ZTextBox ImportClassificationAddInfoStringTextBox;
		internal ZTabPage HTIQuarantineTabPage;
		private AQISUserControl fAQISUserControl;
		Enterprise.Customs.AU.Declaration.GUI.UniversalTariffImportFindBox ImportTariffFindBox;
		Enterprise.Customs.AU.Declaration.GUI.AUCClassFindBox ImportTariffFindBoxAUCClass;
		private Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox ExportTariffFindBox;
		private Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox ExportTariffFindBoxAHECC;
		internal ZTabPage HTEQuarantineTabPage;
		private HTEAQISUserControl fHTEAQISUserControl;
		private ZTabPage HTIPermitsTabPage;
		private ZGroupBox ICSPermitsGroupBox;
		private ZGrid ICSPermitsGrid;
	}
}
