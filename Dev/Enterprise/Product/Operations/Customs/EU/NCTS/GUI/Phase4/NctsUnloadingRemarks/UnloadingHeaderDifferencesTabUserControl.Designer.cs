namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UnloadingHeaderDifferencesTabUserControl
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
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.UnloadingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResetUnloadedItemsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StateOfSealsOkDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UnloadingCompleteDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UnloadingConformsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UnloadingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.UnloadingDifferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoOfSealsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalGrossMassOriginalValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SealsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TotalGrossMassChangedValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReCalcTotalsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OtherNotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VehicleNationalityChangedValueDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VehicleNationalityOriginalValueDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VehicleIdOriginalValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnloadedValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeclaredValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackageCountChangedValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ItemCountChangedValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VehicleIdChangedValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageCountOriginalValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ItemCountOriginalValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.UnloadingDetailsGroupBox.SuspendLayout();
			this.StateOfSealsOkDropEdit.SuspendLayout();
			this.UnloadingCompleteDropEdit.SuspendLayout();
			this.UnloadingConformsDropEdit.SuspendLayout();
			this.UnloadingDateDateEdit.SuspendLayout();
			this.UnloadingDifferencesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).BeginInit();
			this.SealsGrid.SuspendLayout();
			this.VehicleNationalityChangedValueDropEdit.SuspendLayout();
			this.VehicleNationalityOriginalValueDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.UnloadingDetailsGroupBox);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.UnloadingDifferencesGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 721, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(140);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// UnloadingDetailsGroupBox
			// 
			this.UnloadingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0c806c76-f320-4a67-b07a-49ee4727f44a", "Unloading Details");
			this.UnloadingDetailsGroupBox.Controls.Add(this.ResetUnloadedItemsButton);
			this.UnloadingDetailsGroupBox.Controls.Add(this.StateOfSealsOkDropEdit);
			this.UnloadingDetailsGroupBox.Controls.Add(this.UnloadingCompleteDropEdit);
			this.UnloadingDetailsGroupBox.Controls.Add(this.UnloadingConformsDropEdit);
			this.UnloadingDetailsGroupBox.Controls.Add(this.UnloadingDateDateEdit);
			this.UnloadingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingDetailsGroupBox.Name = "UnloadingDetailsGroupBox";
			this.UnloadingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 140, true);
			this.UnloadingDetailsGroupBox.TabIndex = 0;
			this.UnloadingDetailsGroupBox.TabStop = false;
			// 
			// ResetUnloadedItemsButton
			// 
			this.ResetUnloadedItemsButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("1d96ce64-0fa7-4554-a91f-dc7c2f7d233f", "Reset Unloaded Items");
			this.ResetUnloadedItemsButton.IsCaptionOverridden = false;
			this.ResetUnloadedItemsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(549, 19, true);
			this.ResetUnloadedItemsButton.Name = "ResetUnloadedItemsButton";
			this.ResetUnloadedItemsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ResetUnloadedItemsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.ResetUnloadedItemsButton.TabIndex = 4;
			this.ResetUnloadedItemsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ResetUnloadedItemsButton.ToolTipCaption = null;
			this.ResetUnloadedItemsButton.UseVisualStyleBackColor = true;
			this.ResetUnloadedItemsButton.Click += new System.EventHandler(this.ResetUnloadedItemsButton_Click);
			// 
			// StateOfSealsOkDropEdit
			// 
			this.StateOfSealsOkDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateOfSealsOkDropEdit, "UnloadingRemark.G9_StateOfSealsOk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingRemark.G9_StateOfSealsOk)));
			this.StateOfSealsOkDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 74, true);
			this.StateOfSealsOkDropEdit.Name = "StateOfSealsOkDropEdit";
			this.StateOfSealsOkDropEdit.ShouldResizeByMaxLength = true;
			this.StateOfSealsOkDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.StateOfSealsOkDropEdit.TabIndex = 2;
			// 
			// UnloadingCompleteDropEdit
			// 
			this.UnloadingCompleteDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingCompleteDropEdit, "UnloadingRemark.G9_UnloadingCompletion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingRemark.G9_UnloadingCompletion)));
			this.UnloadingCompleteDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 100, true);
			this.UnloadingCompleteDropEdit.Name = "UnloadingCompleteDropEdit";
			this.UnloadingCompleteDropEdit.ShouldResizeByMaxLength = true;
			this.UnloadingCompleteDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.UnloadingCompleteDropEdit.TabIndex = 3;
			// 
			// UnloadingConformsDropEdit
			// 
			this.UnloadingConformsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingConformsDropEdit, "UnloadingRemark.G9_Conform");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingRemark.G9_Conform)));
			this.UnloadingConformsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 48, true);
			this.UnloadingConformsDropEdit.Name = "UnloadingConformsDropEdit";
			this.UnloadingConformsDropEdit.ShouldResizeByMaxLength = true;
			this.UnloadingConformsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.UnloadingConformsDropEdit.TabIndex = 1;
			// 
			// UnloadingDateDateEdit
			// 
			this.UnloadingDateDateEdit.AllowDrop = true;
			this.UnloadingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.UnloadingDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.UnloadingDateDateEdit, "UnloadingRemark.G9_UnloadingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingRemark.G9_UnloadingDate)));
			this.UnloadingDateDateEdit.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("e2ac7ab0-1f04-4f6b-8975-d7de80206de6", "Unloading Date");
			this.UnloadingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 22, true);
			this.UnloadingDateDateEdit.Name = "UnloadingDateDateEdit";
			this.UnloadingDateDateEdit.TabIndex = 0;
			// 
			// UnloadingDifferencesGroupBox
			// 
			this.UnloadingDifferencesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("49c5debe-175c-469c-b997-545264adb208", "Unloading Differences");
			this.UnloadingDifferencesGroupBox.Controls.Add(this.NoOfSealsTextBox);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.TotalGrossMassOriginalValueCalcEdit);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.SealsGrid);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.TotalGrossMassChangedValueCalcEdit);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.ReCalcTotalsButton);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.OtherNotesTextBox);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.VehicleNationalityChangedValueDropEdit);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.VehicleNationalityOriginalValueDropEdit);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.VehicleIdOriginalValueTextBox);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.UnloadedValueLabel);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.DeclaredValueLabel);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.PackageCountChangedValueTextBox);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.ItemCountChangedValueTextBox);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.VehicleIdChangedValueTextBox);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.PackageCountOriginalValueTextBox);
			this.UnloadingDifferencesGroupBox.Controls.Add(this.ItemCountOriginalValueTextBox);
			this.UnloadingDifferencesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDifferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingDifferencesGroupBox.Name = "UnloadingDifferencesGroupBox";
			this.UnloadingDifferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 577, true);
			this.UnloadingDifferencesGroupBox.TabIndex = 1;
			this.UnloadingDifferencesGroupBox.TabStop = false;
			// 
			// NoOfSealsTextBox
			// 
			this.BindingSource.SetBindingMember(this.NoOfSealsTextBox, "UnloadingRemark.G9_NoOfSeals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingRemark.G9_NoOfSeals)));
			this.NoOfSealsTextBox.CaptionResourceString = null;
			this.NoOfSealsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 172, true);
			this.NoOfSealsTextBox.Name = "NoOfSealsTextBox";
			this.NoOfSealsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.NoOfSealsTextBox.TabIndex = 12;
			// 
			// TotalGrossMassOriginalValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossMassOriginalValueCalcEdit, "ArrivalMovementHeader.TotalGrossMassInKilograms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.TotalGrossMassInKilograms)));
			this.TotalGrossMassOriginalValueCalcEdit.CaptionResourceString = null;
			this.TotalGrossMassOriginalValueCalcEdit.DecimalPlaces = 2;
			this.TotalGrossMassOriginalValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 94, true);
			this.TotalGrossMassOriginalValueCalcEdit.Name = "TotalGrossMassOriginalValueCalcEdit";
			this.TotalGrossMassOriginalValueCalcEdit.ReadOnly = true;
			this.TotalGrossMassOriginalValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TotalGrossMassOriginalValueCalcEdit.TabIndex = 3;
			this.TotalGrossMassOriginalValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SealsGrid
			// 
			this.SealsGrid.AllowNavigation = false;
			this.SealsGrid.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
			this.BindingSource.SetBindingMember(this.SealsGrid, "ArrivalMovementHeader.Seals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.Seals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Seal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.Seals)).SyncRoot)).CY_Data)));
			this.SealsGrid.CaptionText = "Seals";
			this.SealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(259);
			this.SealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SealsGrid.GridId = "c94d500b-aec8-4de9-a2cd-24a9aa1c3684";
			this.SealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SealsGrid.LayoutKey = "SealsGrid";
			this.SealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 199, true);
			this.SealsGrid.Name = "SealsGrid";
			this.SealsGrid.PreferredColumnWidth = 441;
			this.SealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 122, true);
			this.SealsGrid.TabIndex = 13;
			// 
			// TotalGrossMassChangedValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossMassChangedValueCalcEdit, "UnloadingMovementHeader.BM_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.BM_GrossWeight)));
			this.TotalGrossMassChangedValueCalcEdit.CaptionResourceString = null;
			this.TotalGrossMassChangedValueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalGrossMassChangedValueCalcEdit, false);
			this.TotalGrossMassChangedValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 94, true);
			this.TotalGrossMassChangedValueCalcEdit.Name = "TotalGrossMassChangedValueCalcEdit";
			this.TotalGrossMassChangedValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TotalGrossMassChangedValueCalcEdit.TabIndex = 9;
			this.TotalGrossMassChangedValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReCalcTotalsButton
			// 
			this.ReCalcTotalsButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("70b93138-c92b-4486-a0cf-6d9fbe8301a2", "Recalculate Totals");
			this.ReCalcTotalsButton.IsCaptionOverridden = false;
			this.ReCalcTotalsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(549, 40, true);
			this.ReCalcTotalsButton.Name = "ReCalcTotalsButton";
			this.ReCalcTotalsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ReCalcTotalsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.ReCalcTotalsButton.TabIndex = 14;
			this.ReCalcTotalsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ReCalcTotalsButton.ToolTipCaption = null;
			this.ReCalcTotalsButton.UseVisualStyleBackColor = true;
			this.ReCalcTotalsButton.Click += new System.EventHandler(this.ReCalcTotalsButton_Click);
			// 
			// OtherNotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.OtherNotesTextBox, "HeaderUnloadingNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).HeaderUnloadingNotes)));
			this.OtherNotesTextBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("860b6b17-55d6-40c9-9b04-8712c1ec0a50", "Unloading Remarks");
			this.OtherNotesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OtherNotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 327, true);
			this.OtherNotesTextBox.Multiline = true;
			this.OtherNotesTextBox.Name = "OtherNotesTextBox";
			this.OtherNotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 107, true);
			this.OtherNotesTextBox.TabIndex = 14;
			// 
			// VehicleNationalityChangedValueDropEdit
			// 
			this.VehicleNationalityChangedValueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleNationalityChangedValueDropEdit, "UnloadedMeansOfTransportAtDepartureNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadedMeansOfTransportAtDepartureNationality)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VehicleNationalityChangedValueDropEdit, false);
			this.VehicleNationalityChangedValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 68, true);
			this.VehicleNationalityChangedValueDropEdit.Name = "VehicleNationalityChangedValueDropEdit";
			this.VehicleNationalityChangedValueDropEdit.ShouldResizeByMaxLength = true;
			this.VehicleNationalityChangedValueDropEdit.ShowDescriptionBox = false;
			this.VehicleNationalityChangedValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.VehicleNationalityChangedValueDropEdit.TabIndex = 8;
			// 
			// VehicleNationalityOriginalValueDropEdit
			// 
			this.VehicleNationalityOriginalValueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleNationalityOriginalValueDropEdit, "ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry)));
			this.VehicleNationalityOriginalValueDropEdit.Enabled = false;
			this.VehicleNationalityOriginalValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 68, true);
			this.VehicleNationalityOriginalValueDropEdit.Name = "VehicleNationalityOriginalValueDropEdit";
			this.VehicleNationalityOriginalValueDropEdit.ShouldResizeByMaxLength = true;
			this.VehicleNationalityOriginalValueDropEdit.ShowDescriptionBox = false;
			this.VehicleNationalityOriginalValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.VehicleNationalityOriginalValueDropEdit.TabIndex = 2;
			// 
			// VehicleIdOriginalValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleIdOriginalValueTextBox, "ArrivalMovementHeader.BM_TransportAtDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_TransportAtDeparture)));
			this.VehicleIdOriginalValueTextBox.CaptionResourceString = null;
			this.VehicleIdOriginalValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 42, true);
			this.VehicleIdOriginalValueTextBox.Name = "VehicleIdOriginalValueTextBox";
			this.VehicleIdOriginalValueTextBox.ReadOnly = true;
			this.VehicleIdOriginalValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.VehicleIdOriginalValueTextBox.TabIndex = 1;
			// 
			// UnloadedValueLabel
			// 
			this.UnloadedValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("1a4392e3-f64e-45ed-8449-a0ba4153f929", "Unloaded Value");
			this.UnloadedValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnloadedValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 16, true);
			this.UnloadedValueLabel.Name = "UnloadedValueLabel";
			this.UnloadedValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.UnloadedValueLabel.TabIndex = 6;
			// 
			// DeclaredValueLabel
			// 
			this.DeclaredValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("629b324e-8129-449f-bc74-946087152b50", "Declared Value");
			this.DeclaredValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DeclaredValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 16, true);
			this.DeclaredValueLabel.Name = "DeclaredValueLabel";
			this.DeclaredValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DeclaredValueLabel.TabIndex = 0;
			// 
			// PackageCountChangedValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackageCountChangedValueTextBox, "UnloadingMovementHeader.TotalNumberOfPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.TotalNumberOfPackages)));
			this.PackageCountChangedValueTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PackageCountChangedValueTextBox, false);
			this.PackageCountChangedValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 146, true);
			this.PackageCountChangedValueTextBox.Name = "PackageCountChangedValueTextBox";
			this.PackageCountChangedValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.PackageCountChangedValueTextBox.TabIndex = 11;
			// 
			// ItemCountChangedValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.ItemCountChangedValueTextBox, "UnloadingMovementHeader.TotalNumberOfItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.TotalNumberOfItems)));
			this.ItemCountChangedValueTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ItemCountChangedValueTextBox, false);
			this.ItemCountChangedValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 120, true);
			this.ItemCountChangedValueTextBox.Name = "ItemCountChangedValueTextBox";
			this.ItemCountChangedValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ItemCountChangedValueTextBox.TabIndex = 10;
			// 
			// VehicleIdChangedValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleIdChangedValueTextBox, "UnloadedMeansOfTransportAtDepartureIdentity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadedMeansOfTransportAtDepartureIdentity)));
			this.VehicleIdChangedValueTextBox.CaptionResourceString = null;
			this.VehicleIdChangedValueTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VehicleIdChangedValueTextBox, false);
			this.VehicleIdChangedValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 42, true);
			this.VehicleIdChangedValueTextBox.Name = "VehicleIdChangedValueTextBox";
			this.VehicleIdChangedValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.VehicleIdChangedValueTextBox.TabIndex = 7;
			this.VehicleIdChangedValueTextBox.ReadOnlyChanged += new System.EventHandler(this.VehicleIdChangedValueTextBox_ReadOnlyChanged);
			// 
			// PackageCountOriginalValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackageCountOriginalValueTextBox, "ArrivalMovementHeader.TotalNumberOfPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.TotalNumberOfPackages)));
			this.PackageCountOriginalValueTextBox.CaptionResourceString = null;
			this.PackageCountOriginalValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 146, true);
			this.PackageCountOriginalValueTextBox.Name = "PackageCountOriginalValueTextBox";
			this.PackageCountOriginalValueTextBox.ReadOnly = true;
			this.PackageCountOriginalValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.PackageCountOriginalValueTextBox.TabIndex = 5;
			// 
			// ItemCountOriginalValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.ItemCountOriginalValueTextBox, "ArrivalMovementHeader.TotalNumberOfItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.TotalNumberOfItems)));
			this.ItemCountOriginalValueTextBox.CaptionResourceString = null;
			this.ItemCountOriginalValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 120, true);
			this.ItemCountOriginalValueTextBox.Name = "ItemCountOriginalValueTextBox";
			this.ItemCountOriginalValueTextBox.ReadOnly = true;
			this.ItemCountOriginalValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ItemCountOriginalValueTextBox.TabIndex = 4;
			// 
			// UnloadingHeaderDifferencesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "UnloadingHeaderDifferencesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 721, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.UnloadingDetailsGroupBox.ResumeLayout(false);
			this.UnloadingDetailsGroupBox.PerformLayout();
			this.StateOfSealsOkDropEdit.ResumeLayout(true);
			this.StateOfSealsOkDropEdit.PerformLayout();
			this.UnloadingCompleteDropEdit.ResumeLayout(true);
			this.UnloadingCompleteDropEdit.PerformLayout();
			this.UnloadingConformsDropEdit.ResumeLayout(true);
			this.UnloadingConformsDropEdit.PerformLayout();
			this.UnloadingDateDateEdit.ResumeLayout(true);
			this.UnloadingDateDateEdit.PerformLayout();
			this.UnloadingDifferencesGroupBox.ResumeLayout(false);
			this.UnloadingDifferencesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).EndInit();
			this.SealsGrid.ResumeLayout(false);
			this.SealsGrid.PerformLayout();
			this.VehicleNationalityChangedValueDropEdit.ResumeLayout(true);
			this.VehicleNationalityChangedValueDropEdit.PerformLayout();
			this.VehicleNationalityOriginalValueDropEdit.ResumeLayout(true);
			this.VehicleNationalityOriginalValueDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox UnloadingDifferencesGroupBox;
		protected ZArchitecture.ZGrid SealsGrid;
		private ZArchitecture.GUI.ZButton ReCalcTotalsButton;
		private ZArchitecture.ZTextBox OtherNotesTextBox;
		private ZArchitecture.GUI.ZDropEdit VehicleNationalityChangedValueDropEdit;
		private ZArchitecture.GUI.ZDropEdit VehicleNationalityOriginalValueDropEdit;
		private ZArchitecture.ZTextBox VehicleIdOriginalValueTextBox;
		private ZArchitecture.ZLabel UnloadedValueLabel;
		private ZArchitecture.ZLabel DeclaredValueLabel;
		private ZArchitecture.ZTextBox PackageCountChangedValueTextBox;
		private ZArchitecture.ZTextBox ItemCountChangedValueTextBox;
		private ZArchitecture.ZTextBox VehicleIdChangedValueTextBox;
		private ZArchitecture.ZTextBox PackageCountOriginalValueTextBox;
		private ZArchitecture.ZTextBox ItemCountOriginalValueTextBox;
		private ZArchitecture.GUI.ZDateEdit UnloadingDateDateEdit;
		private ZArchitecture.GUI.ZDropEdit UnloadingConformsDropEdit;
		private ZArchitecture.GUI.ZDropEdit UnloadingCompleteDropEdit;
		private ZArchitecture.GUI.ZDropEdit StateOfSealsOkDropEdit;
		private ZArchitecture.GUI.ZGroupBox UnloadingDetailsGroupBox;
		private ZArchitecture.GUI.ZButton ResetUnloadedItemsButton;
		private ZArchitecture.ZCalcEdit TotalGrossMassOriginalValueCalcEdit;
		private ZArchitecture.ZCalcEdit TotalGrossMassChangedValueCalcEdit;
		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private ZArchitecture.ZTextBox NoOfSealsTextBox;
	}
}
