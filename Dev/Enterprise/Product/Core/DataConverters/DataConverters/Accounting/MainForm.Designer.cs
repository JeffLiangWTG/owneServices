namespace Enterprise.DataConverters.Accounting
{
	public partial class MainForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.PathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StartConversionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DebtorsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.CreditorsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.BrowseFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.oGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OutPutListTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CopyOutputToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TotalAmountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalAmountTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RowsErrorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.RowsProcessedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.OpenFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.PostDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InterbaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConnectionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConnectionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.oGroupBox1.SuspendLayout();
			this.PostDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 443, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 24, true);
			this.MainStatusBar.Text = "Please select a file and click Start Import button";
			// 
			// PathTextBox
			// 
			this.PathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PathTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|9b77ab4c-a6b3-41af-a42a-d24fbcef9aef", "Location of the data file");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PathTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.PathTextBox.Name = "PathTextBox";
			this.PathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 20, true);
			this.PathTextBox.TabIndex = 1;
			// 
			// StartConversionButton
			// 
			this.StartConversionButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|a77e1c61-16cf-4e22-aece-1fa61662f8e1", "Start import");
			this.StartConversionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 100, true);
			this.StartConversionButton.Name = "StartConversionButton";
			this.StartConversionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 21, true);
			this.StartConversionButton.TabIndex = 11;
			this.StartConversionButton.Click += new System.EventHandler(this.StartConversionButton_Click);
			// 
			// DebtorsRadioButton
			// 
			this.DebtorsRadioButton.AutoCheck = false;
			this.DebtorsRadioButton.AutoSize = true;
			this.DebtorsRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|e001fbce-6a2d-4582-86af-b5f29d90464c", "Debtors");
			this.DebtorsRadioButton.Checked = true;
			this.DebtorsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DebtorsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 50, true);
			this.DebtorsRadioButton.Name = "DebtorsRadioButton";
			this.DebtorsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.DebtorsRadioButton.TabIndex = 4;
			this.DebtorsRadioButton.TabStop = true;
			// 
			// CreditorsRadioButton
			// 
			this.CreditorsRadioButton.AutoCheck = false;
			this.CreditorsRadioButton.AutoSize = true;
			this.CreditorsRadioButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|95644748-123d-411a-919c-d199a45d8377", "Creditors");
			this.CreditorsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CreditorsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 50, true);
			this.CreditorsRadioButton.Name = "CreditorsRadioButton";
			this.CreditorsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.CreditorsRadioButton.TabIndex = 5;
			// 
			// BrowseFileButton
			// 
			this.BrowseFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseFileButton.AutoSize = true;
			this.BrowseFileButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|cff80269-b3e4-4023-8b30-b693eb6e32ae", "Browse...");
			this.BrowseFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 22, true);
			this.BrowseFileButton.Name = "BrowseFileButton";
			this.BrowseFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 23, true);
			this.BrowseFileButton.TabIndex = 2;
			this.BrowseFileButton.Click += new System.EventHandler(this.BrowseFileButton_Click);
			// 
			// oGroupBox1
			// 
			this.oGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.oGroupBox1.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|108e8b51-4ee0-47e5-8e15-65b5726ae107", "Import Output");
			this.oGroupBox1.Controls.Add(this.OutPutListTextBox);
			this.oGroupBox1.Controls.Add(this.CopyOutputToClipboardButton);
			this.oGroupBox1.Controls.Add(this.TotalAmountLabel);
			this.oGroupBox1.Controls.Add(this.TotalAmountTextLabel);
			this.oGroupBox1.Controls.Add(this.RowsErrorLabel);
			this.oGroupBox1.Controls.Add(this.zLabel6);
			this.oGroupBox1.Controls.Add(this.RowsProcessedLabel);
			this.oGroupBox1.Controls.Add(this.zLabel5);
			this.oGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 126, true);
			this.oGroupBox1.Name = "oGroupBox1";
			this.oGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 286, true);
			this.oGroupBox1.TabIndex = 13;
			this.oGroupBox1.TabStop = false;
			// 
			// OutPutListTextBox
			// 
			this.OutPutListTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OutPutListTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("bbeda896-d2da-4f3b-b75e-4328bf476b93", "Source lines with problems");
			this.OutPutListTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OutPutListTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OutPutListTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.OutPutListTextBox.Multiline = true;
			this.OutPutListTextBox.Name = "OutPutListTextBox";
			this.OutPutListTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.OutPutListTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 241, true);
			this.OutPutListTextBox.TabIndex = 1;
			this.OutPutListTextBox.WordWrap = false;
			// 
			// CopyOutputToClipboardButton
			// 
			this.CopyOutputToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyOutputToClipboardButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|6119fc6b-d393-49a9-9cd9-df50f9e5f7ed", "Copy output to Clipboard");
			this.CopyOutputToClipboardButton.Enabled = false;
			this.CopyOutputToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 256, true);
			this.CopyOutputToClipboardButton.Name = "CopyOutputToClipboardButton";
			this.CopyOutputToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 22, true);
			this.CopyOutputToClipboardButton.TabIndex = 8;
			this.CopyOutputToClipboardButton.Visible = false;
			this.CopyOutputToClipboardButton.Click += new System.EventHandler(this.ClipboardCopyButton_Click);
			// 
			// TotalAmountLabel
			// 
			this.TotalAmountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalAmountLabel.AutoSize = true;
			this.TotalAmountLabel.ForeColor = System.Drawing.Color.Blue;
			this.TotalAmountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 82, true);
			this.TotalAmountLabel.Name = "TotalAmountLabel";
			this.TotalAmountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.TotalAmountLabel.TabIndex = 7;
			this.TotalAmountLabel.Text = "0";
			// 
			// TotalAmountTextLabel
			// 
			this.TotalAmountTextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalAmountTextLabel.AutoSize = true;
			this.TotalAmountTextLabel.ForeColor = System.Drawing.Color.Blue;
			this.TotalAmountTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 82, true);
			this.TotalAmountTextLabel.Name = "TotalAmountTextLabel";
			this.TotalAmountTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.TotalAmountTextLabel.TabIndex = 6;
			// 
			// RowsErrorLabel
			// 
			this.RowsErrorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RowsErrorLabel.AutoSize = true;
			this.RowsErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.RowsErrorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 59, true);
			this.RowsErrorLabel.Name = "RowsErrorLabel";
			this.RowsErrorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsErrorLabel.TabIndex = 5;
			this.RowsErrorLabel.Text = "0";
			// 
			// zLabel6
			// 
			this.zLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel6.AutoSize = true;
			this.zLabel6.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|328f48da-d7ee-45a4-a291-d60ffd10596a", "Rows With Errors:");
			this.zLabel6.ForeColor = System.Drawing.Color.Red;
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 59, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 13, true);
			this.zLabel6.TabIndex = 4;
			// 
			// RowsProcessedLabel
			// 
			this.RowsProcessedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RowsProcessedLabel.AutoSize = true;
			this.RowsProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 37, true);
			this.RowsProcessedLabel.Name = "RowsProcessedLabel";
			this.RowsProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.RowsProcessedLabel.TabIndex = 3;
			this.RowsProcessedLabel.Text = "0";
			// 
			// zLabel5
			// 
			this.zLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel5.AutoSize = true;
			this.zLabel5.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|c9a9a8d1-c468-4781-ab1b-5af68e4ec0d0", "Rows Processed:");
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 37, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.zLabel5.TabIndex = 2;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|4fffe6df-4625-480d-b16b-fee1f9c7830b", "Save");
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 418, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.SaveButton.TabIndex = 14;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|c0618b19-6698-4b0a-aa0b-5fb14d0fbc72", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(533, 418, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CloseButton.TabIndex = 15;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ImportProgressBar
			// 
			this.ImportProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ImportProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 100, true);
			this.ImportProgressBar.Name = "ImportProgressBar";
			this.ImportProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 21, true);
			this.ImportProgressBar.TabIndex = 12;
			this.ImportProgressBar.Visible = false;
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
			this.OpenFileDialog.Title = Res.GetString("86c1b8f5-10a8-42d9-a284-bdf142400663", "Please select file");
			this.OpenFileDialog.ValidateNames = true;
			// 
			// PostDateDateEdit
			// 
			this.PostDateDateEdit.AllowDrop = true;
			this.PostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateDateEdit.AutoCompleteYear = true;
			this.PostDateDateEdit.AutoSize = true;
			this.PostDateDateEdit.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|1cdb1074-a063-4621-a6db-a9327d0c2dd9", "Post Date");
			this.PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 48, true);
			this.PostDateDateEdit.Name = "PostDateDateEdit";
			this.PostDateDateEdit.TabIndex = 7;
			// 
			// InterbaseCheckBox
			// 
			this.InterbaseCheckBox.AutoSize = true;
			this.InterbaseCheckBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|9e66e818-fa82-4ae7-8923-937b7049cdcd", "Import From InterBase");
			this.InterbaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.InterbaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 47, true);
			this.InterbaseCheckBox.Name = "InterbaseCheckBox";
			this.InterbaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.InterbaseCheckBox.TabIndex = 3;
			this.InterbaseCheckBox.CheckedChanged += new System.EventHandler(this.InterbaseCheckBox_CheckedChanged);
			// 
			// ConnectionTextBox
			// 
			this.ConnectionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|64b54756-c2d4-4488-af49-d2f8ebcf79cd", "Connection String");
			this.ConnectionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConnectionTextBox.Enabled = false;
			this.ConnectionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 74, true);
			this.ConnectionTextBox.Name = "ConnectionTextBox";
			this.ConnectionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.ConnectionTextBox.TabIndex = 9;
			// 
			// ConnectionButton
			// 
			this.ConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionButton.AutoSize = true;
			this.ConnectionButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|ea52cdda-20e1-4aa2-acbd-f48718fe44ed", "Build Connection String");
			this.ConnectionButton.Enabled = false;
			this.ConnectionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 72, true);
			this.ConnectionButton.Name = "ConnectionButton";
			this.ConnectionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.ConnectionButton.TabIndex = 10;
			this.ConnectionButton.Click += new System.EventHandler(this.ConnectionButton_Click);
			// 
			// MainForm
			// 
			this.AcceptButton = this.StartConversionButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DataConverters.Res.GetData("MainForm|cf7f4df9-72ff-48aa-bce1-2303d4250e74", "Accounting Data Conversion");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 467, true);
			this.Controls.Add(this.ConnectionTextBox);
			this.Controls.Add(this.ConnectionButton);
			this.Controls.Add(this.PathTextBox);
			this.Controls.Add(this.DebtorsRadioButton);
			this.Controls.Add(this.PostDateDateEdit);
			this.Controls.Add(this.InterbaseCheckBox);
			this.Controls.Add(this.ImportProgressBar);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.oGroupBox1);
			this.Controls.Add(this.BrowseFileButton);
			this.Controls.Add(this.CreditorsRadioButton);
			this.Controls.Add(this.StartConversionButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 325, true);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.StartConversionButton, 0);
			this.Controls.SetChildIndex(this.CreditorsRadioButton, 0);
			this.Controls.SetChildIndex(this.BrowseFileButton, 0);
			this.Controls.SetChildIndex(this.oGroupBox1, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ImportProgressBar, 0);
			this.Controls.SetChildIndex(this.InterbaseCheckBox, 0);
			this.Controls.SetChildIndex(this.PostDateDateEdit, 0);
			this.Controls.SetChildIndex(this.DebtorsRadioButton, 0);
			this.Controls.SetChildIndex(this.PathTextBox, 0);
			this.Controls.SetChildIndex(this.ConnectionButton, 0);
			this.Controls.SetChildIndex(this.ConnectionTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.oGroupBox1.ResumeLayout(false);
			this.oGroupBox1.PerformLayout();
			this.PostDateDateEdit.ResumeLayout(true);
			this.PostDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox PathTextBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton DebtorsRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton CreditorsRadioButton;
		private Enterprise.ZArchitecture.GUI.ZButton BrowseFileButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox oGroupBox1;
		private CargoWise.Windows.UI.KProgressBar ImportProgressBar;
		private Enterprise.ZArchitecture.ZLabel zLabel5;
		private Enterprise.ZArchitecture.ZLabel RowsErrorLabel;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.ZLabel TotalAmountLabel;
		private Enterprise.ZArchitecture.ZLabel TotalAmountTextLabel;
		private Enterprise.ZArchitecture.GUI.ZOpenFileDialog OpenFileDialog;
		private Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.ZLabel RowsProcessedLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CopyOutputToClipboardButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PostDateDateEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox InterbaseCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZButton ConnectionButton;
		protected internal Enterprise.ZArchitecture.ZTextBox ConnectionTextBox;
		private Enterprise.ZArchitecture.ZTextBox OutPutListTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton StartConversionButton;
	}
}
