namespace Enterprise.DataConverters.CustomsFiles.NZ.GUI
{
	public partial class ClassificationAndPartImporterForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.DataSourcePathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DataSourcePathBrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.DataToImportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ClassificationsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.DataSourceTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExcelFileRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ImportLogGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.RowsProcessedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.RowsTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.RowsInvalidLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.RowsExcludedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.RowsUpdatedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.RowsCreatedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.LogListBox = new CargoWise.Windows.UI.KListBox();
			this.UpdateExistingRecordsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.StartImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyLogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OpenFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.FolderBrowserDialog = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DataToImportGroupBox.SuspendLayout();
			this.DataSourceTypeGroupBox.SuspendLayout();
			this.ImportLogGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 476, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(352);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DataConverters.DummyImporter);
			// 
			// DataSourcePathTextBox
			// 
			this.DataSourcePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DataSourcePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 23, true);
			this.DataSourcePathTextBox.Name = "DataSourcePathTextBox";
			this.DataSourcePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.DataSourcePathTextBox.TabIndex = 34;
			// 
			// DataSourcePathBrowseButton
			// 
			this.DataSourcePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DataSourcePathBrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 21, true);
			this.DataSourcePathBrowseButton.Name = "DataSourcePathBrowseButton";
			this.DataSourcePathBrowseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DataSourcePathBrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.DataSourcePathBrowseButton.TabIndex = 35;
			this.DataSourcePathBrowseButton.Text = "Browse...";
			this.DataSourcePathBrowseButton.ToolTipCaption = null;
			this.DataSourcePathBrowseButton.Click += new System.EventHandler(this.DataSourcePathBrowseButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 5, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 13, true);
			this.zLabel1.TabIndex = 33;
			this.zLabel1.Text = "Location of the Deliverance directory or data file:";
			// 
			// DataToImportGroupBox
			// 
			this.DataToImportGroupBox.Controls.Add(this.ProductsRadioButton);
			this.DataToImportGroupBox.Controls.Add(this.ClassificationsRadioButton);
			this.DataToImportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 8, true);
			this.DataToImportGroupBox.Name = "DataToImportGroupBox";
			this.DataToImportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 72, true);
			this.DataToImportGroupBox.TabIndex = 36;
			this.DataToImportGroupBox.TabStop = false;
			this.DataToImportGroupBox.Text = "Data to Import";
			// 
			// ProductsRadioButton
			// 
			this.ProductsRadioButton.AutoCheck = false;
			this.ProductsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ProductsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.ProductsRadioButton.Name = "ProductsRadioButton";
			this.ProductsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 16, true);
			this.ProductsRadioButton.TabIndex = 41;
			this.ProductsRadioButton.Text = "Products";
			// 
			// ClassificationsRadioButton
			// 
			this.ClassificationsRadioButton.AutoCheck = false;
			this.ClassificationsRadioButton.Checked = true;
			this.ClassificationsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClassificationsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.ClassificationsRadioButton.Name = "ClassificationsRadioButton";
			this.ClassificationsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 16, true);
			this.ClassificationsRadioButton.TabIndex = 40;
			this.ClassificationsRadioButton.TabStop = true;
			this.ClassificationsRadioButton.Text = "Classifications";
			// 
			// DataSourceTypeGroupBox
			// 
			this.DataSourceTypeGroupBox.Controls.Add(this.ExcelFileRadioButton);
			this.DataSourceTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.DataSourceTypeGroupBox.Name = "DataSourceTypeGroupBox";
			this.DataSourceTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 72, true);
			this.DataSourceTypeGroupBox.TabIndex = 37;
			this.DataSourceTypeGroupBox.TabStop = false;
			this.DataSourceTypeGroupBox.Text = "Data Source Type";
			// 
			// ExcelFileRadioButton
			// 
			this.ExcelFileRadioButton.AutoCheck = false;
			this.ExcelFileRadioButton.Checked = true;
			this.ExcelFileRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcelFileRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 20, true);
			this.ExcelFileRadioButton.Name = "ExcelFileRadioButton";
			this.ExcelFileRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 16, true);
			this.ExcelFileRadioButton.TabIndex = 41;
			this.ExcelFileRadioButton.Text = "CSV File";
			// 
			// ImportLogGroupBox
			// 
			this.ImportLogGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ImportLogGroupBox.Controls.Add(this.ProgressBar);
			this.ImportLogGroupBox.Controls.Add(this.RowsProcessedLabel);
			this.ImportLogGroupBox.Controls.Add(this.zLabel13);
			this.ImportLogGroupBox.Controls.Add(this.RowsTotalLabel);
			this.ImportLogGroupBox.Controls.Add(this.zLabel11);
			this.ImportLogGroupBox.Controls.Add(this.RowsInvalidLabel);
			this.ImportLogGroupBox.Controls.Add(this.zLabel10);
			this.ImportLogGroupBox.Controls.Add(this.RowsExcludedLabel);
			this.ImportLogGroupBox.Controls.Add(this.zLabel8);
			this.ImportLogGroupBox.Controls.Add(this.RowsUpdatedLabel);
			this.ImportLogGroupBox.Controls.Add(this.zLabel6);
			this.ImportLogGroupBox.Controls.Add(this.RowsCreatedLabel);
			this.ImportLogGroupBox.Controls.Add(this.zLabel3);
			this.ImportLogGroupBox.Controls.Add(this.LogListBox);
			this.ImportLogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.ImportLogGroupBox.Name = "ImportLogGroupBox";
			this.ImportLogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 366, true);
			this.ImportLogGroupBox.TabIndex = 38;
			this.ImportLogGroupBox.TabStop = false;
			this.ImportLogGroupBox.Text = "Import Log";
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 325, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 24, true);
			this.ProgressBar.TabIndex = 14;
			// 
			// RowsProcessedLabel
			// 
			this.RowsProcessedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RowsProcessedLabel.AutoSize = true;
			this.RowsProcessedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 346, true);
			this.RowsProcessedLabel.Name = "RowsProcessedLabel";
			this.RowsProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsProcessedLabel.TabIndex = 13;
			this.RowsProcessedLabel.Text = "0";
			this.RowsProcessedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel13
			// 
			this.zLabel13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel13.AutoSize = true;
			this.zLabel13.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 325, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.zLabel13.TabIndex = 12;
			this.zLabel13.Text = "Rows Processed:";
			this.zLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RowsTotalLabel
			// 
			this.RowsTotalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RowsTotalLabel.AutoSize = true;
			this.RowsTotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 346, true);
			this.RowsTotalLabel.Name = "RowsTotalLabel";
			this.RowsTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsTotalLabel.TabIndex = 11;
			this.RowsTotalLabel.Text = "0";
			this.RowsTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel11
			// 
			this.zLabel11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel11.AutoSize = true;
			this.zLabel11.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 325, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.zLabel11.TabIndex = 10;
			this.zLabel11.Text = "Total Rows:";
			this.zLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RowsInvalidLabel
			// 
			this.RowsInvalidLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsInvalidLabel.AutoSize = true;
			this.RowsInvalidLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsInvalidLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 346, true);
			this.RowsInvalidLabel.Name = "RowsInvalidLabel";
			this.RowsInvalidLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsInvalidLabel.TabIndex = 9;
			this.RowsInvalidLabel.Text = "0";
			this.RowsInvalidLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel10
			// 
			this.zLabel10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel10.AutoSize = true;
			this.zLabel10.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 325, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.zLabel10.TabIndex = 8;
			this.zLabel10.Text = "Rows Invalid:";
			this.zLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RowsExcludedLabel
			// 
			this.RowsExcludedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsExcludedLabel.AutoSize = true;
			this.RowsExcludedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsExcludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 346, true);
			this.RowsExcludedLabel.Name = "RowsExcludedLabel";
			this.RowsExcludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsExcludedLabel.TabIndex = 7;
			this.RowsExcludedLabel.Text = "0";
			this.RowsExcludedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel8
			// 
			this.zLabel8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel8.AutoSize = true;
			this.zLabel8.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 325, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.zLabel8.TabIndex = 6;
			this.zLabel8.Text = "Rows Excluded:";
			this.zLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RowsUpdatedLabel
			// 
			this.RowsUpdatedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsUpdatedLabel.AutoSize = true;
			this.RowsUpdatedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsUpdatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 346, true);
			this.RowsUpdatedLabel.Name = "RowsUpdatedLabel";
			this.RowsUpdatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsUpdatedLabel.TabIndex = 5;
			this.RowsUpdatedLabel.Text = "0";
			this.RowsUpdatedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel6
			// 
			this.zLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel6.AutoSize = true;
			this.zLabel6.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 325, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 13, true);
			this.zLabel6.TabIndex = 4;
			this.zLabel6.Text = "Rows Updated:";
			this.zLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RowsCreatedLabel
			// 
			this.RowsCreatedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsCreatedLabel.AutoSize = true;
			this.RowsCreatedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsCreatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 346, true);
			this.RowsCreatedLabel.Name = "RowsCreatedLabel";
			this.RowsCreatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsCreatedLabel.TabIndex = 3;
			this.RowsCreatedLabel.Text = "0";
			this.RowsCreatedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel3
			// 
			this.zLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel3.AutoSize = true;
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 325, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 13, true);
			this.zLabel3.TabIndex = 2;
			this.zLabel3.Text = "Rows Created:";
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LogListBox
			// 
			this.LogListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LogListBox.Font = new System.Drawing.Font("Tahoma", 8F);
			this.LogListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.LogListBox.Name = "LogListBox";
			this.LogListBox.ScrollAlwaysVisible = true;
			this.LogListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.LogListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 303, true);
			this.LogListBox.TabIndex = 0;
			// 
			// UpdateExistingRecordsCheckBox
			// 
			this.UpdateExistingRecordsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpdateExistingRecordsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 56, true);
			this.UpdateExistingRecordsCheckBox.Name = "UpdateExistingRecordsCheckBox";
			this.UpdateExistingRecordsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 16, true);
			this.UpdateExistingRecordsCheckBox.TabIndex = 39;
			this.UpdateExistingRecordsCheckBox.Text = "Update Existing Records";
			// 
			// StartImportButton
			// 
			this.StartImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StartImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 451, true);
			this.StartImportButton.Name = "StartImportButton";
			this.StartImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.StartImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
			this.StartImportButton.TabIndex = 40;
			this.StartImportButton.Text = "Start Import";
			this.StartImportButton.ToolTipCaption = null;
			this.StartImportButton.Click += new System.EventHandler(this.StartImportButton_Click);
			// 
			// CopyLogButton
			// 
			this.CopyLogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CopyLogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 451, true);
			this.CopyLogButton.Name = "CopyLogButton";
			this.CopyLogButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CopyLogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
			this.CopyLogButton.TabIndex = 41;
			this.CopyLogButton.Text = "Copy Log";
			this.CopyLogButton.ToolTipCaption = null;
			this.CopyLogButton.Click += new System.EventHandler(this.CopyLogButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 451, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.CloseButton.TabIndex = 42;
			this.CloseButton.Text = "Close";
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OpenFileDialog
			// 
			this.OpenFileDialog.AddExtension = true;
			this.OpenFileDialog.CheckFileExists = true;
			this.OpenFileDialog.CheckPathExists = true;
			this.OpenFileDialog.DefaultExt = "";
			this.OpenFileDialog.DereferenceLinks = true;
			this.OpenFileDialog.Filter = "";
			this.OpenFileDialog.FilterIndex = 1;
			this.OpenFileDialog.InitialDirectory = "";
			this.OpenFileDialog.Multiselect = false;
			this.OpenFileDialog.ReadOnlyChecked = false;
			this.OpenFileDialog.RestoreDirectory = false;
			this.OpenFileDialog.ShowHelp = false;
			this.OpenFileDialog.SupportMultiDottedExtensions = false;
			this.OpenFileDialog.Title = "";
			this.OpenFileDialog.ValidateNames = true;
			// 
			// FolderBrowserDialog
			// 
			this.FolderBrowserDialog.CreateDirectory = false;
			this.FolderBrowserDialog.Description = "";
			this.FolderBrowserDialog.RequireMappablePath = false;
			this.FolderBrowserDialog.RootFolder = System.Environment.SpecialFolder.Desktop;
			this.FolderBrowserDialog.ShowNewFolderButton = true;
			// 
			// ClassificationAndPartImporterForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 500, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.CopyLogButton);
			this.Controls.Add(this.StartImportButton);
			this.Controls.Add(this.UpdateExistingRecordsCheckBox);
			this.Controls.Add(this.ImportLogGroupBox);
			this.Controls.Add(this.DataSourceTypeGroupBox);
			this.Controls.Add(this.DataToImportGroupBox);
			this.Controls.Add(this.DataSourcePathTextBox);
			this.Controls.Add(this.DataSourcePathBrowseButton);
			this.Controls.Add(this.zLabel1);
			this.DataSourceAssemblyName = "Enterprise.DataConverters";
			this.DataSourceType = typeof(Enterprise.DataConverters.DummyImporter);
			this.DataSourceTypeName = "Enterprise.DataConverters.DummyImporter";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 376, true);
			this.Name = "ClassificationAndPartImporterForm";
			this.Text = "Classification and Part Importer";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ClassificationAndPartImporter_Closing);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.DataSourcePathBrowseButton, 0);
			this.Controls.SetChildIndex(this.DataSourcePathTextBox, 0);
			this.Controls.SetChildIndex(this.DataToImportGroupBox, 0);
			this.Controls.SetChildIndex(this.DataSourceTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.ImportLogGroupBox, 0);
			this.Controls.SetChildIndex(this.UpdateExistingRecordsCheckBox, 0);
			this.Controls.SetChildIndex(this.StartImportButton, 0);
			this.Controls.SetChildIndex(this.CopyLogButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DataToImportGroupBox.ResumeLayout(false);
			this.DataToImportGroupBox.PerformLayout();
			this.DataSourceTypeGroupBox.ResumeLayout(false);
			this.DataSourceTypeGroupBox.PerformLayout();
			this.ImportLogGroupBox.ResumeLayout(false);
			this.ImportLogGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DataToImportGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DataSourceTypeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ImportLogGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ExcelFileRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ClassificationsRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ProductsRadioButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UpdateExistingRecordsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZButton CopyLogButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private CargoWise.Windows.UI.KListBox LogListBox;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.ZLabel zLabel8;
		private Enterprise.ZArchitecture.ZLabel zLabel10;
		private Enterprise.ZArchitecture.ZLabel zLabel11;
		private Enterprise.ZArchitecture.GUI.ZButton StartImportButton;
		private Enterprise.ZArchitecture.ZLabel RowsCreatedLabel;
		private Enterprise.ZArchitecture.ZLabel RowsUpdatedLabel;
		private Enterprise.ZArchitecture.ZLabel RowsExcludedLabel;
		private Enterprise.ZArchitecture.ZLabel RowsInvalidLabel;
		private Enterprise.ZArchitecture.ZLabel RowsTotalLabel;
		private Enterprise.ZArchitecture.ZLabel RowsProcessedLabel;
		private Enterprise.ZArchitecture.ZLabel zLabel13;
		private Enterprise.ZArchitecture.ZTextBox DataSourcePathTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton DataSourcePathBrowseButton;
		private CargoWise.Windows.UI.KProgressBar ProgressBar;
		private Enterprise.ZArchitecture.GUI.ZOpenFileDialog OpenFileDialog;
		private Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog FolderBrowserDialog;

	}
}
