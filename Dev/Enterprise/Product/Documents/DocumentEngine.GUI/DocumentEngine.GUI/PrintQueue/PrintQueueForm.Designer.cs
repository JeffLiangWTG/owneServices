using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class PrintQueueForm : ZForm
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZOpenFileDialog openFileDialog;
		private ZTemplateTabControl MainTabControl;
		private ZTabPage PrintQueueTabPage;
		private ZGroupBox PrinterSpecificInformationGroupBox;
		private ZButton ClearPrintersettingsButton;
		private ZButton LoadXLSwithPrintersettingsButton;
		private ZLabel IsLoadedLabel;
		private ZDateEdit LastUsedDateEdit;
		private ZGroupBox ScaleAndMarginsGroupBox;
		private ZLabel ScalePercentLabel;
		private ZCalcEdit ScaleCalcEdit;
		private ZCalcEdit ColumnScaleCalcEdit;
		private ZCalcEdit RowScaleCalcEdit;
		private ZCalcEdit LeftMarginCalcEdit;
		private ZCalcEdit TopMarginCalcEdit;
		private ZLabel ColumnScalePercentLabel;
		private ZLabel RowScalePercentLabel;
		private ZLabel WidthUnitsLabel;
		private ZLabel HeightUnitsLabel;
		private ZGroupBox GeneralGroupBox;
		private ZCheckBox SupressLetterheadCheckBox;
		private ZCheckBox IsRollPaperCheckBox;
		private ZDropEdit SQ_PrintLanguageDropEdit;
		private ZTextBox DisplayNameTextBox;
		private ZCheckBox IsActiveCheckBox;
		private ZGroupBox SystemGroupBox;
		private ZTextBox TechnicalNameTextBox;
		private ZTextBox ServerNameTextBox;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zEventTabPage1;
		private DocumentDelivery.PrinterHelpControl printerHelpControl1;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.openFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PrintQueueTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.printerHelpControl1 = new Enterprise.DocumentEngine.GUI.DocumentDelivery.PrinterHelpControl();
			this.PrinterSpecificInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClearPrintersettingsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LoadXLSwithPrintersettingsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IsLoadedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LastUsedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ScaleAndMarginsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ScalePercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ScaleCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ColumnScaleCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RowScaleCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LeftMarginCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TopMarginCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ColumnScalePercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowScalePercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WidthUnitsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HeightUnitsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GeneralGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupressLetterheadCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsRollPaperCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SQ_PrintLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DisplayNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SystemGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TechnicalNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ServerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zEventTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.PrintQueueTabPage.SuspendLayout();
			this.printerHelpControl1.SuspendLayout();
			this.PrinterSpecificInformationGroupBox.SuspendLayout();
			this.ScaleAndMarginsGroupBox.SuspendLayout();
			this.GeneralGroupBox.SuspendLayout();
			this.SQ_PrintLanguageDropEdit.SuspendLayout();
			this.SystemGroupBox.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 467, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(610);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 442, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "xls";
			this.openFileDialog.Filter = "ExcelFiles|*.xls";
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.PrintQueueTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 430, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// PrintQueueTabPage
			// 
			this.PrintQueueTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|ffdfafd8-4cd4-400e-88f7-4d800a8955d5", "Print Queue");
			this.PrintQueueTabPage.Controls.Add(this.printerHelpControl1);
			this.PrintQueueTabPage.Controls.Add(this.PrinterSpecificInformationGroupBox);
			this.PrintQueueTabPage.Controls.Add(this.ScaleAndMarginsGroupBox);
			this.PrintQueueTabPage.Controls.Add(this.GeneralGroupBox);
			this.PrintQueueTabPage.Controls.Add(this.SystemGroupBox);
			this.PrintQueueTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PrintQueueTabPage.Name = "PrintQueueTabPage";
			this.PrintQueueTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 403, true);
			this.PrintQueueTabPage.TabIndex = 0;
			// 
			// printerHelpControl1
			// 
			this.printerHelpControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 375, true);
			this.printerHelpControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			this.printerHelpControl1.Name = "printerHelpControl1";
			this.printerHelpControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 23, true);
			this.printerHelpControl1.TabIndex = 4;
			// 
			// PrinterSpecificInformationGroupBox
			// 
			this.PrinterSpecificInformationGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|021838f3-197c-4959-904d-0af40026a65e", "Printer specific information");
			this.PrinterSpecificInformationGroupBox.Controls.Add(this.ClearPrintersettingsButton);
			this.PrinterSpecificInformationGroupBox.Controls.Add(this.LoadXLSwithPrintersettingsButton);
			this.PrinterSpecificInformationGroupBox.Controls.Add(this.IsLoadedLabel);
			this.PrinterSpecificInformationGroupBox.Controls.Add(this.LastUsedDateEdit);
			this.PrinterSpecificInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 224, true);
			this.PrinterSpecificInformationGroupBox.Name = "PrinterSpecificInformationGroupBox";
			this.PrinterSpecificInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 144, true);
			this.PrinterSpecificInformationGroupBox.TabIndex = 3;
			this.PrinterSpecificInformationGroupBox.TabStop = false;
			// 
			// ClearPrintersettingsButton
			// 
			this.ClearPrintersettingsButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|d2fbe43e-5c32-4910-9f4d-d8fc8042e2c8", "Clear");
			this.ClearPrintersettingsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 88, true);
			this.ClearPrintersettingsButton.Name = "ClearPrintersettingsButton";
			this.ClearPrintersettingsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 24, true);
			this.ClearPrintersettingsButton.TabIndex = 2;
			this.ClearPrintersettingsButton.Click += new System.EventHandler(this.ClearPrintersettingsButton_Click);
			// 
			// LoadXLSwithPrintersettingsButton
			// 
			this.LoadXLSwithPrintersettingsButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|a9f302d6-a9f6-4cbf-97cb-920a7a828784", "Load from XLS");
			this.LoadXLSwithPrintersettingsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 56, true);
			this.LoadXLSwithPrintersettingsButton.Name = "LoadXLSwithPrintersettingsButton";
			this.LoadXLSwithPrintersettingsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 24, true);
			this.LoadXLSwithPrintersettingsButton.TabIndex = 1;
			this.LoadXLSwithPrintersettingsButton.Click += new System.EventHandler(this.LoadXLSwithPrintersettingsButton_Click);
			// 
			// IsLoadedLabel
			// 
			this.BindingSource.SetBindingMember(this.IsLoadedLabel, "IsXLSTemplateForPrintSettingsLoaded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).IsXLSTemplateForPrintSettingsLoaded)));
			this.IsLoadedLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|0ce6bf2c-ac22-4f37-94c7-906e6d8fa0ed", "Is XLS Template For Print Settings Loaded");
			this.IsLoadedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.IsLoadedLabel.Name = "IsLoadedLabel";
			this.IsLoadedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 23, true);
			this.IsLoadedLabel.TabIndex = 0;
			// 
			// LastUsedDateEdit
			// 
			this.BindingSource.SetBindingMember(this.LastUsedDateEdit, "SQ_LastUsedDateTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_LastUsedDateTimeUtc)));
			this.LastUsedDateEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|ca8af61b-f111-4c32-94a1-ed2afc82f3b1", "Last Used Date (Local)");
			this.LastUsedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LastUsedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 120, true);
			this.LastUsedDateEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.LastUsedDateEdit.Name = "LastUsedDateEdit";
			this.LastUsedDateEdit.TabIndex = 4;

			// 
			// ScaleAndMarginsGroupBox
			// 
			this.ScaleAndMarginsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|8c8fdeef-3ffc-426b-b431-1d3fbe88c38d", "Scaling and Margins");
			this.ScaleAndMarginsGroupBox.Controls.Add(this.ScalePercentLabel);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.ScaleCalcEdit);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.ColumnScaleCalcEdit);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.RowScaleCalcEdit);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.LeftMarginCalcEdit);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.TopMarginCalcEdit);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.ColumnScalePercentLabel);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.RowScalePercentLabel);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.WidthUnitsLabel);
			this.ScaleAndMarginsGroupBox.Controls.Add(this.HeightUnitsLabel);
			this.ScaleAndMarginsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 224, true);
			this.ScaleAndMarginsGroupBox.Name = "ScaleAndMarginsGroupBox";
			this.ScaleAndMarginsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 144, true);
			this.ScaleAndMarginsGroupBox.TabIndex = 2;
			this.ScaleAndMarginsGroupBox.TabStop = false;
			// 
			// ScalePercentLabel
			// 
			this.ScalePercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 16, true);
			this.ScalePercentLabel.Name = "ScalePercentLabel";
			this.ScalePercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.ScalePercentLabel.TabIndex = 2;
			this.ScalePercentLabel.Text = "%";
			// 
			// ScaleCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ScaleCalcEdit, "SQ_Scale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_Scale)));
			this.ScaleCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 16, true);
			this.ScaleCalcEdit.Name = "ScaleCalcEdit";
			this.ScaleCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ScaleCalcEdit.TabIndex = 1;
			this.ScaleCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ColumnScaleCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ColumnScaleCalcEdit, "SQ_ColumnScale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_ColumnScale)));
			this.ColumnScaleCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|65343cfc-854f-494e-976b-38fccde15652", "Horizontal Scale");
			this.ColumnScaleCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 40, true);
			this.ColumnScaleCalcEdit.Name = "ColumnScaleCalcEdit";
			this.ColumnScaleCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ColumnScaleCalcEdit.TabIndex = 4;
			this.ColumnScaleCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RowScaleCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RowScaleCalcEdit, "SQ_RowScale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_RowScale)));
			this.RowScaleCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|4814850a-88c0-4850-b072-4cdf05a13428", "Vertical Scale");
			this.RowScaleCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 64, true);
			this.RowScaleCalcEdit.Name = "RowScaleCalcEdit";
			this.RowScaleCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.RowScaleCalcEdit.TabIndex = 7;
			this.RowScaleCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LeftMarginCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LeftMarginCalcEdit, "SQ_LeftMargin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_LeftMargin)));
			this.LeftMarginCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|87ca1cf5-8e7e-4538-8e72-3c3c5a3f8e17", "Left Margin");
			this.LeftMarginCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 88, true);
			this.LeftMarginCalcEdit.Name = "LeftMarginCalcEdit";
			this.LeftMarginCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.LeftMarginCalcEdit.TabIndex = 10;
			this.LeftMarginCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TopMarginCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TopMarginCalcEdit, "SQ_TopMargin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_TopMargin)));
			this.TopMarginCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|aaeb0094-dc8e-4d47-8260-37c63b7d9e4a", "Top Margin");
			this.TopMarginCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 112, true);
			this.TopMarginCalcEdit.Name = "TopMarginCalcEdit";
			this.TopMarginCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.TopMarginCalcEdit.TabIndex = 13;
			this.TopMarginCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ColumnScalePercentLabel
			// 
			this.ColumnScalePercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 40, true);
			this.ColumnScalePercentLabel.Name = "ColumnScalePercentLabel";
			this.ColumnScalePercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.ColumnScalePercentLabel.TabIndex = 5;
			this.ColumnScalePercentLabel.Text = "%";
			// 
			// RowScalePercentLabel
			// 
			this.RowScalePercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 64, true);
			this.RowScalePercentLabel.Name = "RowScalePercentLabel";
			this.RowScalePercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.RowScalePercentLabel.TabIndex = 8;
			this.RowScalePercentLabel.Text = "%";
			// 
			// WidthUnitsLabel
			// 
			this.WidthUnitsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.WidthUnitsLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|070456c6-73f4-483d-9c3a-9531d2f352da", "Excel cell-width units");
			this.WidthUnitsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 88, true);
			this.WidthUnitsLabel.Name = "WidthUnitsLabel";
			this.WidthUnitsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 23, true);
			this.WidthUnitsLabel.TabIndex = 11;
			// 
			// HeightUnitsLabel
			// 
			this.HeightUnitsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HeightUnitsLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|8747d556-4b2b-4572-9374-c854f726d2ac", "Excel cell-height units");
			this.HeightUnitsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 112, true);
			this.HeightUnitsLabel.Name = "HeightUnitsLabel";
			this.HeightUnitsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 23, true);
			this.HeightUnitsLabel.TabIndex = 14;
			// 
			// GeneralGroupBox
			// 
			this.GeneralGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|ddf031ab-816d-4e60-bfe8-bdb0ef57e2f2", "General Settings");
			this.GeneralGroupBox.Controls.Add(this.IsRollPaperCheckBox);
			this.GeneralGroupBox.Controls.Add(this.SupressLetterheadCheckBox);
			this.GeneralGroupBox.Controls.Add(this.SQ_PrintLanguageDropEdit);
			this.GeneralGroupBox.Controls.Add(this.DisplayNameTextBox);
			this.GeneralGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.GeneralGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 96, true);
			this.GeneralGroupBox.Name = "GeneralGroupBox";
			this.GeneralGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 120, true);
			this.GeneralGroupBox.TabIndex = 1;
			this.GeneralGroupBox.TabStop = false;
			// 
			// SupressLetterheadCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SupressLetterheadCheckBox, "SQ_SupressLetterhead");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_SupressLetterhead)));
			this.SupressLetterheadCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|97f3fe4f-26ad-4fe6-9dfc-4598f4c4db03", "Suppress printing Letterhead logos");
			this.SupressLetterheadCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SupressLetterheadCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 64, true);
			this.SupressLetterheadCheckBox.Name = "SupressLetterheadCheckBox";
			this.SupressLetterheadCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 24, true);
			this.SupressLetterheadCheckBox.TabIndex = 5;
			// 
			// IsRollPaperCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsRollPaperCheckBox, "SQ_IsRollPaper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_IsRollPaper)));
			this.IsRollPaperCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|D964F41E-F120-43CB-8D3E-8B6156AADE12", "Is Roll Paper");
			this.IsRollPaperCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsRollPaperCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 88, true);
			this.IsRollPaperCheckBox.Name = "IsRollPaperCheckBox";
			this.IsRollPaperCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 24, true);
			this.IsRollPaperCheckBox.TabIndex = 6;
			// 
			// SQ_PrintLanguageDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SQ_PrintLanguageDropEdit, "SQ_PrintLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_PrintLanguage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_PrintLanguage_List)));
			this.SQ_PrintLanguageDropEdit.BindToList = "SQ_PrintLanguage_List";
			this.SQ_PrintLanguageDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|3664c5c8-1ea6-4b54-a164-2d176cdc7cd0", "Dot Matrix Language");
			this.SQ_PrintLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 40, true);
			this.SQ_PrintLanguageDropEdit.Name = "SQ_PrintLanguageDropEdit";
			this.SQ_PrintLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.SQ_PrintLanguageDropEdit.TabIndex = 3;
			// 
			// DisplayNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DisplayNameTextBox, "SQ_DisplayName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_DisplayName)));
			this.DisplayNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 16, true);
			this.DisplayNameTextBox.Name = "DisplayNameTextBox";
			this.DisplayNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 20, true);
			this.DisplayNameTextBox.TabIndex = 1;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "SQ_AllowPrinting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_AllowPrinting)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|a9460d72-b1c2-43fc-b45a-f12202c30f3b", "Allow printing to this queue from the Application");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 24, true);
			this.IsActiveCheckBox.TabIndex = 4;
			// 
			// SystemGroupBox
			// 
			this.SystemGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|5c3700e1-43fa-4125-8d8a-94edc7d6e86f", "Settings based on Windows Printer Control Panel");
			this.SystemGroupBox.Controls.Add(this.TechnicalNameTextBox);
			this.SystemGroupBox.Controls.Add(this.ServerNameTextBox);
			this.SystemGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 8, true);
			this.SystemGroupBox.Name = "SystemGroupBox";
			this.SystemGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 80, true);
			this.SystemGroupBox.TabIndex = 0;
			this.SystemGroupBox.TabStop = false;
			// 
			// TechnicalNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.TechnicalNameTextBox, "SQ_QueueName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_QueueName)));
			this.TechnicalNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|555e2bd8-d8a4-4da6-acb3-e79f72457dcd", "Print Queue");
			this.TechnicalNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 48, true);
			this.TechnicalNameTextBox.Name = "TechnicalNameTextBox";
			this.TechnicalNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 20, true);
			this.TechnicalNameTextBox.TabIndex = 3;
			// 
			// ServerNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ServerNameTextBox, "SQ_ServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue)(null)).SQ_ServerName)));
			this.ServerNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|b9237999-ca4e-4222-ba4c-d6bf884a10ea", "Server Name");
			this.ServerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 24, true);
			this.ServerNameTextBox.Name = "ServerNameTextBox";
			this.ServerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 20, true);
			this.ServerNameTextBox.TabIndex = 1;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 403, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 403, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// PrintQueueForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 491, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintQueueForm|96d3f732-ce7a-4dbe-8b0f-2e2fced4de61", "Print Queue");
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.Scheduler.Business.StmPrintQueue";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "PrintQueueForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.PrintQueueTabPage.ResumeLayout(false);
			this.PrintQueueTabPage.PerformLayout();
			this.printerHelpControl1.ResumeLayout(true);
			this.printerHelpControl1.PerformLayout();
			this.PrinterSpecificInformationGroupBox.ResumeLayout(false);
			this.PrinterSpecificInformationGroupBox.PerformLayout();
			this.ScaleAndMarginsGroupBox.ResumeLayout(false);
			this.ScaleAndMarginsGroupBox.PerformLayout();
			this.GeneralGroupBox.ResumeLayout(false);
			this.GeneralGroupBox.PerformLayout();
			this.SQ_PrintLanguageDropEdit.ResumeLayout(true);
			this.SQ_PrintLanguageDropEdit.PerformLayout();
			this.SystemGroupBox.ResumeLayout(false);
			this.SystemGroupBox.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
