namespace Enterprise.DataConverters.CustomsFiles
{
	public partial class MainForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TableToImportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LimitedClassRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ProductsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ClassificationRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.DataSourceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InterbaseRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ExcelRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.StatusLabel = new CargoWise.Windows.UI.KStatusBar();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StartConversionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TreatRecsThatExistGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TreatUpdateRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.TreatExcludeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.OpenFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.BrowseFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ProcessDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RowsImportingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImportingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowsUpdatedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UpdateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowsExcludedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExcludedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowsProcessedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProcessedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CopyLogToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OutputListBox = new CargoWise.Windows.UI.KListBox();
			this.ToCSVCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.ConnectionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConnectionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TableToImportGroupBox.SuspendLayout();
			this.DataSourceGroupBox.SuspendLayout();
			this.TreatRecsThatExistGroupBox.SuspendLayout();
			this.ProcessDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// TableToImportGroupBox
			// 
			this.TableToImportGroupBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|TableToImport", "Table to import");
			this.TableToImportGroupBox.Controls.Add(this.LimitedClassRadioButton);
			this.TableToImportGroupBox.Controls.Add(this.ProductsRadioButton);
			this.TableToImportGroupBox.Controls.Add(this.ClassificationRadioButton);
			this.TableToImportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 48, true);
			this.TableToImportGroupBox.Name = "TableToImportGroupBox";
			this.TableToImportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 96, true);
			this.TableToImportGroupBox.TabIndex = 3;
			this.TableToImportGroupBox.TabStop = false;
			// 
			// LimitedClassRadioButton
			// 
			this.LimitedClassRadioButton.AutoCheck = false;
			this.LimitedClassRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|LimitedClass", "Limited Class");
			this.LimitedClassRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LimitedClassRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 74, true);
			this.LimitedClassRadioButton.Name = "LimitedClassRadioButton";
			this.LimitedClassRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 16, true);
			this.LimitedClassRadioButton.TabIndex = 1;
			// 
			// ProductsRadioButton
			// 
			this.ProductsRadioButton.AutoCheck = false;
			this.ProductsRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|Products", "Products");
			this.ProductsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ProductsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 49, true);
			this.ProductsRadioButton.Name = "ProductsRadioButton";
			this.ProductsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.ProductsRadioButton.TabIndex = 2;
			// 
			// ClassificationRadioButton
			// 
			this.ClassificationRadioButton.AutoCheck = false;
			this.ClassificationRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|Classifications", "Classifications");
			this.ClassificationRadioButton.Checked = true;
			this.ClassificationRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClassificationRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 24, true);
			this.ClassificationRadioButton.Name = "ClassificationRadioButton";
			this.ClassificationRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 16, true);
			this.ClassificationRadioButton.TabIndex = 0;
			this.ClassificationRadioButton.TabStop = true;
			// 
			// DataSourceGroupBox
			// 
			this.DataSourceGroupBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|DataSource", "Data source");
			this.DataSourceGroupBox.Controls.Add(this.InterbaseRadioButton);
			this.DataSourceGroupBox.Controls.Add(this.ExcelRadioButton);
			this.DataSourceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 48, true);
			this.DataSourceGroupBox.Name = "DataSourceGroupBox";
			this.DataSourceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 96, true);
			this.DataSourceGroupBox.TabIndex = 2;
			this.DataSourceGroupBox.TabStop = false;
			// 
			// InterbaseRadioButton
			// 
			this.InterbaseRadioButton.AutoCheck = false;
			this.InterbaseRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|Cyberfreight", "Cyberfreight");
			this.InterbaseRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.InterbaseRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 41, true);
			this.InterbaseRadioButton.Name = "InterbaseRadioButton";
			this.InterbaseRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.InterbaseRadioButton.TabIndex = 3;
			this.InterbaseRadioButton.CheckedChanged += new System.EventHandler(this.InterbaseRadioButton_CheckedChanged);
			// 
			// ExcelRadioButton
			// 
			this.ExcelRadioButton.AutoCheck = false;
			this.ExcelRadioButton.Checked = true;
			this.ExcelRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|ExcelData", "Excel data");
			this.ExcelRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcelRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 19, true);
			this.ExcelRadioButton.Name = "ExcelRadioButton";
			this.ExcelRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.ExcelRadioButton.TabIndex = 2;
			// 
			// StatusLabel
			// 
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 533, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 22, true);
			this.StatusLabel.TabIndex = 36;
			this.StatusLabel.Text = "Please select a file and click Start Import button";
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|Close", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 497, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// StartConversionButton
			// 
			this.StartConversionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.StartConversionButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|StartImport", "Start Import");
			this.StartConversionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 152, true);
			this.StartConversionButton.Name = "StartConversionButton";
			this.StartConversionButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.StartConversionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 23, true);
			this.StartConversionButton.TabIndex = 8;
			this.StartConversionButton.ToolTipCaption = null;
			this.StartConversionButton.Click += new System.EventHandler(this.StartConversionButton_Click);
			// 
			// PathTextBox
			// 
			this.PathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 23, true);
			this.PathTextBox.Name = "PathTextBox";
			this.PathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 20, true);
			this.PathTextBox.TabIndex = 0;
			this.PathTextBox.Extensions.Get<CargoWise.Windows.UI.ILabelCaptionRenderer>().Visible = false;
			// 
			// TreatRecsThatExistGroupBox
			// 
			this.TreatRecsThatExistGroupBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|TreatRecordsThatAlreadyExist", "Treat records that already exist");
			this.TreatRecsThatExistGroupBox.Controls.Add(this.TreatUpdateRadioButton);
			this.TreatRecsThatExistGroupBox.Controls.Add(this.TreatExcludeRadioButton);
			this.TreatRecsThatExistGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 48, true);
			this.TreatRecsThatExistGroupBox.Name = "TreatRecsThatExistGroupBox";
			this.TreatRecsThatExistGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 48, true);
			this.TreatRecsThatExistGroupBox.TabIndex = 4;
			this.TreatRecsThatExistGroupBox.TabStop = false;
			// 
			// TreatUpdateRadioButton
			// 
			this.TreatUpdateRadioButton.AutoCheck = false;
			this.TreatUpdateRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|Update", "Update");
			this.TreatUpdateRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TreatUpdateRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 23, true);
			this.TreatUpdateRadioButton.Name = "TreatUpdateRadioButton";
			this.TreatUpdateRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.TreatUpdateRadioButton.TabIndex = 1;
			// 
			// TreatExcludeRadioButton
			// 
			this.TreatExcludeRadioButton.AutoCheck = false;
			this.TreatExcludeRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|Exclude", "Exclude");
			this.TreatExcludeRadioButton.Checked = true;
			this.TreatExcludeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TreatExcludeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 23, true);
			this.TreatExcludeRadioButton.Name = "TreatExcludeRadioButton";
			this.TreatExcludeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.TreatExcludeRadioButton.TabIndex = 0;
			this.TreatExcludeRadioButton.TabStop = true;
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
			this.OpenFileDialog.Title = "Please select file";
			this.OpenFileDialog.ValidateNames = true;
			// 
			// BrowseFileButton
			// 
			this.BrowseFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseFileButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|Browse", "Browse...");
			this.BrowseFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 23, true);
			this.BrowseFileButton.Name = "BrowseFileButton";
			this.BrowseFileButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BrowseFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseFileButton.TabIndex = 1;
			this.BrowseFileButton.ToolTipCaption = null;
			this.BrowseFileButton.Click += new System.EventHandler(this.BrowseFileButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|LocationOfTheDataDirectoryOrDataFile", "Location of the Data Directory or Data File:");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel1.TabIndex = 30;
			// 
			// ProcessDetailsGroupBox
			// 
			this.ProcessDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProcessDetailsGroupBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|ImportLog", "Import Log");
			this.ProcessDetailsGroupBox.Controls.Add(this.RowsImportingLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.ImportingLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.RowsUpdatedLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.UpdateLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.RowsExcludedLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.ExcludedLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.RowsProcessedLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.ProcessedLabel);
			this.ProcessDetailsGroupBox.Controls.Add(this.CopyLogToClipboardButton);
			this.ProcessDetailsGroupBox.Controls.Add(this.OutputListBox);
			this.ProcessDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 180, true);
			this.ProcessDetailsGroupBox.Name = "ProcessDetailsGroupBox";
			this.ProcessDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 313, true);
			this.ProcessDetailsGroupBox.TabIndex = 34;
			this.ProcessDetailsGroupBox.TabStop = false;
			// 
			// RowsImportingLabel
			// 
			this.RowsImportingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsImportingLabel.AutoSize = true;
			this.RowsImportingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsImportingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 293, true);
			this.RowsImportingLabel.Name = "RowsImportingLabel";
			this.RowsImportingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsImportingLabel.TabIndex = 17;
			this.RowsImportingLabel.Text = "0";
			// 
			// ImportingLabel
			// 
			this.ImportingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportingLabel.AutoSize = true;
			this.ImportingLabel.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|RowsImporting", "Rows Importing:");
			this.ImportingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ImportingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 272, true);
			this.ImportingLabel.Name = "ImportingLabel";
			this.ImportingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ImportingLabel.TabIndex = 16;
			// 
			// RowsUpdatedLabel
			// 
			this.RowsUpdatedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsUpdatedLabel.AutoSize = true;
			this.RowsUpdatedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsUpdatedLabel.ForeColor = System.Drawing.Color.Green;
			this.RowsUpdatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 293, true);
			this.RowsUpdatedLabel.Name = "RowsUpdatedLabel";
			this.RowsUpdatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsUpdatedLabel.TabIndex = 15;
			this.RowsUpdatedLabel.Text = "0";
			// 
			// UpdateLabel
			// 
			this.UpdateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UpdateLabel.AutoSize = true;
			this.UpdateLabel.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|RowsUpdated", "Rows Updated:");
			this.UpdateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UpdateLabel.ForeColor = System.Drawing.Color.Green;
			this.UpdateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 272, true);
			this.UpdateLabel.Name = "UpdateLabel";
			this.UpdateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.UpdateLabel.TabIndex = 14;
			// 
			// RowsExcludedLabel
			// 
			this.RowsExcludedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsExcludedLabel.AutoSize = true;
			this.RowsExcludedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsExcludedLabel.ForeColor = System.Drawing.Color.Blue;
			this.RowsExcludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 293, true);
			this.RowsExcludedLabel.Name = "RowsExcludedLabel";
			this.RowsExcludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsExcludedLabel.TabIndex = 13;
			this.RowsExcludedLabel.Text = "0";
			// 
			// ExcludedLabel
			// 
			this.ExcludedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExcludedLabel.AutoSize = true;
			this.ExcludedLabel.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|RowsExcluded", "Rows Excluded:");
			this.ExcludedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExcludedLabel.ForeColor = System.Drawing.Color.Blue;
			this.ExcludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 272, true);
			this.ExcludedLabel.Name = "ExcludedLabel";
			this.ExcludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ExcludedLabel.TabIndex = 11;
			// 
			// RowsProcessedLabel
			// 
			this.RowsProcessedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RowsProcessedLabel.AutoSize = true;
			this.RowsProcessedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RowsProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 293, true);
			this.RowsProcessedLabel.Name = "RowsProcessedLabel";
			this.RowsProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsProcessedLabel.TabIndex = 5;
			this.RowsProcessedLabel.Text = "0";
			// 
			// ProcessedLabel
			// 
			this.ProcessedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ProcessedLabel.AutoSize = true;
			this.ProcessedLabel.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|RowsProcessed", "Rows Processed:");
			this.ProcessedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 272, true);
			this.ProcessedLabel.Name = "ProcessedLabel";
			this.ProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ProcessedLabel.TabIndex = 4;
			// 
			// CopyLogToClipboardButton
			// 
			this.CopyLogToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyLogToClipboardButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|CopyLogToClipboard", "Copy Log to Clipboard");
			this.CopyLogToClipboardButton.Enabled = false;
			this.CopyLogToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(507, 272, true);
			this.CopyLogToClipboardButton.Name = "CopyLogToClipboardButton";
			this.CopyLogToClipboardButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CopyLogToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 23, true);
			this.CopyLogToClipboardButton.TabIndex = 1;
			this.CopyLogToClipboardButton.ToolTipCaption = null;
			this.CopyLogToClipboardButton.Visible = false;
			this.CopyLogToClipboardButton.Click += new System.EventHandler(this.CopyLogToClipboardButton_Click);
			// 
			// OutputListBox
			// 
			this.OutputListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputListBox.Font = new System.Drawing.Font("Tahoma", 8F);
			this.OutputListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.OutputListBox.Name = "OutputListBox";
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 238, true);
			this.OutputListBox.TabIndex = 0;
			// 
			// ToCSVCheckBox
			// 
			this.ToCSVCheckBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|ToCSVFileOnly", "To CSV file only");
			this.ToCSVCheckBox.Enabled = false;
			this.ToCSVCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ToCSVCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 114, true);
			this.ToCSVCheckBox.Name = "ToCSVCheckBox";
			this.ToCSVCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ToCSVCheckBox.TabIndex = 5;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|ConnectionString", "Connection String:");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 156, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel2.TabIndex = 41;
			// 
			// ConnectionTextBox
			// 
			this.ConnectionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|64b54756-c2d4-4488-af49-d2f8ebcf79cd", "Connection String");
			this.ConnectionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConnectionTextBox.Enabled = false;
			this.ConnectionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 154, true);
			this.ConnectionTextBox.Name = "ConnectionTextBox";
			this.ConnectionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 20, true);
			this.ConnectionTextBox.TabIndex = 6;
			// 
			// ConnectionButton
			// 
			this.ConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|BuldConnectionString", "Build Connection String");
			this.ConnectionButton.Enabled = false;
			this.ConnectionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 152, true);
			this.ConnectionButton.Name = "ConnectionButton";
			this.ConnectionButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ConnectionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.ConnectionButton.TabIndex = 7;
			this.ConnectionButton.ToolTipCaption = null;
			this.ConnectionButton.Click += new System.EventHandler(this.ConnectionButton_Click);
			// 
			// MainForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 555, true);
			this.Controls.Add(this.ConnectionButton);
			this.Controls.Add(this.ConnectionTextBox);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.PathTextBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ToCSVCheckBox);
			this.Controls.Add(this.DataSourceGroupBox);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.StartConversionButton);
			this.Controls.Add(this.TreatRecsThatExistGroupBox);
			this.Controls.Add(this.BrowseFileButton);
			this.Controls.Add(this.ProcessDetailsGroupBox);
			this.Controls.Add(this.TableToImportGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 325, true);
			this.Name = "MainForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TableToImportGroupBox.ResumeLayout(false);
			this.TableToImportGroupBox.PerformLayout();
			this.DataSourceGroupBox.ResumeLayout(false);
			this.DataSourceGroupBox.PerformLayout();
			this.TreatRecsThatExistGroupBox.ResumeLayout(false);
			this.TreatRecsThatExistGroupBox.PerformLayout();
			this.ProcessDetailsGroupBox.ResumeLayout(false);
			this.ProcessDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox TableToImportGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ProductsRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ClassificationRadioButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DataSourceGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ExcelRadioButton;
		private CargoWise.Windows.UI.KStatusBar StatusLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton StartConversionButton;
		private Enterprise.ZArchitecture.ZTextBox PathTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TreatRecsThatExistGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton TreatUpdateRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton TreatExcludeRadioButton;
		private Enterprise.ZArchitecture.GUI.ZOpenFileDialog OpenFileDialog;
		private Enterprise.ZArchitecture.GUI.ZButton BrowseFileButton;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ProcessDetailsGroupBox;
		private Enterprise.ZArchitecture.ZLabel RowsUpdatedLabel;
		private Enterprise.ZArchitecture.ZLabel UpdateLabel;
		private Enterprise.ZArchitecture.ZLabel RowsExcludedLabel;
		private Enterprise.ZArchitecture.ZLabel ExcludedLabel;
		private Enterprise.ZArchitecture.ZLabel RowsProcessedLabel;
		private Enterprise.ZArchitecture.ZLabel ProcessedLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CopyLogToClipboardButton;
		private CargoWise.Windows.UI.KListBox OutputListBox;
		private Enterprise.ZArchitecture.ZLabel ImportingLabel;
		private Enterprise.ZArchitecture.ZLabel RowsImportingLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZRadioButton InterbaseRadioButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ToCSVCheckBox;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		protected internal Enterprise.ZArchitecture.ZTextBox ConnectionTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZButton ConnectionButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton LimitedClassRadioButton;
	}
}
