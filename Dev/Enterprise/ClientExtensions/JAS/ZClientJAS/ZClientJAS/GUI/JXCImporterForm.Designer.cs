
namespace Enterprise.Client.JAS.GUI
{
	partial class JXCImporterForm
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (this.openFileDialog1 != null)
				{
					this.openFileDialog1.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ImportFilePathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SummaryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.openFileDialog1 = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 432, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.JAS.Business.JXC.Import.JXCDataImporterBizO);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 13, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "File Path";
			// 
			// ImportFilePathTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportFilePathTextBox, "ImportFilePath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.JAS.Business.JXC.Import.JXCDataImporterBizO)(null)).ImportFilePath)));
			this.ImportFilePathTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ImportFilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 6, true);
			this.ImportFilePathTextBox.Name = "ImportFilePathTextBox";
			this.ImportFilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ImportFilePathTextBox.TabIndex = 1;
			// 
			// SummaryTextBox
			// 
			this.SummaryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SummaryTextBox, "ImportSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.JAS.Business.JXC.Import.JXCDataImporterBizO)(null)).ImportSummary)));
			this.SummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 32, true);
			this.SummaryTextBox.Multiline = true;
			this.SummaryTextBox.Name = "SummaryTextBox";
			this.SummaryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(671, 364, true);
			this.SummaryTextBox.TabIndex = 3;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 4, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseButton.TabIndex = 2;
			this.BrowseButton.Text = "Browse";
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 402, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ImportButton.TabIndex = 4;
			this.ImportButton.Text = "Import";
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(611, 402, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Title = "Select JXC file to import";
			// 
			// JXCImporterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 456, true);
			this.ControlBox = false;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.ImportFilePathTextBox);
			this.Controls.Add(this.SummaryTextBox);
			this.DataSourceAssemblyName = "ZClientJAS";
			this.DataSourceType = typeof(Enterprise.Client.JAS.Business.JXC.Import.JXCDataImporterBizO);
			this.DataSourceTypeName = "Enterprise.Client.JAS.Business.JXC.Import.JXCDataImporterBizO";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "JXCImporterForm";
			this.Text = "JXCImporterForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SummaryTextBox, 0);
			this.Controls.SetChildIndex(this.ImportFilePathTextBox, 0);
			this.Controls.SetChildIndex(this.BrowseButton, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZLabel zLabel1;
		internal Enterprise.ZArchitecture.ZTextBox ImportFilePathTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SummaryTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton BrowseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ImportButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZOpenFileDialog openFileDialog1;
	}
}
