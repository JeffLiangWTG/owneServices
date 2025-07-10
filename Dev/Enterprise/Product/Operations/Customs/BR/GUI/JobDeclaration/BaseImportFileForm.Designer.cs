namespace Enterprise.Customs.BR.GUI
{
	public partial class BaseImportFileForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.OpenFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileContentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LogDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LogDetailsListBox = new Enterprise.ZArchitecture.GUI.ZListBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LogDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 552, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.NFEImportObjectParent);
			// 
			// OpenFileDialog
			// 
			this.OpenFileDialog.AddExtension = true;
			this.OpenFileDialog.CheckFileExists = true;
			this.OpenFileDialog.CheckPathExists = true;
			this.OpenFileDialog.DefaultExt = "";
			this.OpenFileDialog.DereferenceLinks = true;
			this.OpenFileDialog.Filter = "XML Files|*.xml";
			this.OpenFileDialog.FilterIndex = 1;
			this.OpenFileDialog.InitialDirectory = "";
			this.OpenFileDialog.Multiselect = true;
			this.OpenFileDialog.ReadOnlyChecked = false;
			this.OpenFileDialog.RestoreDirectory = false;
			this.OpenFileDialog.ShowHelp = false;
			this.OpenFileDialog.SupportMultiDottedExtensions = false;
			this.OpenFileDialog.Title = "";
			this.OpenFileDialog.ValidateNames = true;
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FileNameTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FileNameTextBox, false);
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 12, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.ReadOnly = true;
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 15, true);
			this.FileNameTextBox.TabIndex = 1;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ae2ada69-171c-4ca1-82ae-70b5ff273127", "Browse");
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 8, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseButton.TabIndex = 2;
			this.BrowseButton.ToolTipCaption = null;
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("659e7a80-f295-46cc-9a68-6f7ab02efdf1", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 523, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 23, true);
			this.ImportButton.TabIndex = 6;
			this.ImportButton.ToolTipCaption = null;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("64dc8f97-c6e9-4021-ae3a-381233f42066", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 523, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FileContentGroupBox
			//
			this.FileContentGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("5af024c9-af8e-4e36-8d11-bdd7c2221f0d", "Content");
			this.FileContentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FileContentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 38, true);
			this.FileContentGroupBox.Name = "FileContentGroupBox";
			this.FileContentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 332, true);
			this.FileContentGroupBox.TabIndex = 8;
			this.FileContentGroupBox.TabStop = false;
			// 
			// LogDetailsGroupBox
			// 
			this.LogDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LogDetailsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ecaca3dd-2cd8-484e-8dc1-3cb09393f8a3", "Log Details");
			this.LogDetailsGroupBox.Controls.Add(this.LogDetailsListBox);
			this.LogDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 374, true);
			this.LogDetailsGroupBox.Name = "LogDetailsGroupBox";
			this.LogDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 139, true);
			this.LogDetailsGroupBox.TabIndex = 10;
			this.LogDetailsGroupBox.TabStop = false;
			// 
			// LogListBox
			// 
			this.LogDetailsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LogDetailsListBox.ItemHeight = 25;
			this.LogDetailsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 22, true);
			this.LogDetailsListBox.Name = "LogListBox";
			this.LogDetailsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 102, true);
			this.LogDetailsListBox.TabIndex = 11;
			// 
			// BaseImportFileForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("F2073B29-C78D-4FA9-BFDE-4E94158B89BA", "NFE Import");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 576, true);
			this.Controls.Add(this.LogDetailsGroupBox);
			this.Controls.Add(this.FileContentGroupBox);
			this.Controls.Add(this.FileNameTextBox);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.NFEImportObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 443, true);
			this.Name = "BaseImportFileForm";
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.BrowseButton, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.FileContentGroupBox, 0);
			this.Controls.SetChildIndex(this.LogDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LogDetailsGroupBox.ResumeLayout(false);
			this.LogDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZOpenFileDialog OpenFileDialog;
		internal Enterprise.ZArchitecture.ZTextBox FileNameTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton BrowseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ImportButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZGroupBox FileContentGroupBox;
		internal ZArchitecture.GUI.ZGroupBox LogDetailsGroupBox;
		internal ZArchitecture.GUI.ZListBox LogDetailsListBox;
	}
}
