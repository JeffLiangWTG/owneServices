using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI.DataImport
{
	public partial class Level1DataImportForm : ZChildForm, INotifications
	{
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZTabPage LoadDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage SummaryTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SummaryGroupBox;
		private CargoWise.Windows.UI.KTextBox SummaryTextBox;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		internal CargoWise.Windows.UI.KProgressBar progressBar;
		internal Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		internal Enterprise.ZArchitecture.GUI.ZButton LoadButton;
		protected Level1DataImportDetailControl detailControl;
		protected Level1DataImportDetailForManifestControl detailForManifestControl;
		protected Level1DataImportReasonsControl reasonsControl;
		protected Level1DataImportReasonsForManifestControl reasonsForManifestControl;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.LoadDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.reasonsForManifestControl = new Enterprise.Client.UPE.GUI.DataImport.Level1DataImportReasonsForManifestControl();
			this.reasonsControl = new Enterprise.Client.UPE.GUI.DataImport.Level1DataImportReasonsControl();
			this.detailForManifestControl = new Enterprise.Client.UPE.GUI.DataImport.Level1DataImportDetailForManifestControl();
			this.detailControl = new Enterprise.Client.UPE.GUI.DataImport.Level1DataImportDetailControl();
			this.SummaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SummaryTextBox = new CargoWise.Windows.UI.KTextBox();
			this.progressBar = new CargoWise.Windows.UI.KProgressBar();
			this.LoadButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.LoadDetailsTabPage.SuspendLayout();
			this.reasonsForManifestControl.SuspendLayout();
			this.reasonsControl.SuspendLayout();
			this.detailForManifestControl.SuspendLayout();
			this.detailControl.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.SummaryGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(353);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.DataImport.Level1DataImport);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 492, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "&Close";
			this.CloseButton.ToolTipCaption = null;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 492, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.Text = "&Save";
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.LoadDetailsTabPage);
			this.MainTabControl.Controls.Add(this.SummaryTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 482, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// LoadDetailsTabPage
			// 
			this.LoadDetailsTabPage.Controls.Add(this.reasonsForManifestControl);
			this.LoadDetailsTabPage.Controls.Add(this.reasonsControl);
			this.LoadDetailsTabPage.Controls.Add(this.detailForManifestControl);
			this.LoadDetailsTabPage.Controls.Add(this.detailControl);
			this.LoadDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LoadDetailsTabPage.Name = "LoadDetailsTabPage";
			this.LoadDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 460, true);
			this.LoadDetailsTabPage.TabIndex = 0;
			this.LoadDetailsTabPage.Text = "Load Details";
			// 
			// reasonsForManifestControl
			// 
			this.reasonsForManifestControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reasonsForManifestControl, ".");
			this.reasonsForManifestControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 147, true);
			this.reasonsForManifestControl.Name = "reasonsForManifestControl";
			this.reasonsForManifestControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 305, true);
			this.reasonsForManifestControl.TabIndex = 2;
			// 
			// reasonsControl
			// 
			this.reasonsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reasonsControl, ".");
			this.reasonsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 147, true);
			this.reasonsControl.Name = "reasonsControl";
			this.reasonsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 305, true);
			this.reasonsControl.TabIndex = 2;
			// 
			// detailForManifestControl
			// 
			this.detailForManifestControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.detailForManifestControl, ".");
			this.detailForManifestControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.detailForManifestControl.Name = "detailForManifestControl";
			this.detailForManifestControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 133, true);
			this.detailForManifestControl.TabIndex = 1;
			// 
			// detailControl
			// 
			this.detailControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.detailControl, ".");
			this.detailControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.detailControl.Name = "detailControl";
			this.detailControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 133, true);
			this.detailControl.TabIndex = 1;
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.Controls.Add(this.SummaryGroupBox);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 460, true);
			this.SummaryTabPage.TabIndex = 1;
			this.SummaryTabPage.Text = "Summary";
			// 
			// SummaryGroupBox
			// 
			this.SummaryGroupBox.Controls.Add(this.SummaryTextBox);
			this.SummaryGroupBox.Controls.Add(this.progressBar);
			this.SummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryGroupBox.Name = "SummaryGroupBox";
			this.SummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 460, true);
			this.SummaryGroupBox.TabIndex = 0;
			this.SummaryGroupBox.TabStop = false;
			// 
			// SummaryTextBox
			// 
			this.SummaryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.SummaryTextBox.Multiline = true;
			this.SummaryTextBox.Name = "SummaryTextBox";
			this.SummaryTextBox.ReadOnly = true;
			this.SummaryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 417, true);
			this.SummaryTextBox.TabIndex = 0;
			// 
			// progressBar
			// 
			this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.progressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 648, true);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 39, true);
			this.progressBar.TabIndex = 1;
			// 
			// LoadButton
			// 
			this.LoadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LoadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 492, true);
			this.LoadButton.Name = "LoadButton";
			this.LoadButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LoadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.LoadButton.TabIndex = 1;
			this.LoadButton.Text = "&Load";
			this.LoadButton.ToolTipCaption = null;
			this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
			// 
			// Level1DataImportForm
			// 
			this.CancelButton = this.CloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 547, true);
			this.ControlBox = false;
			this.Controls.Add(this.LoadButton);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.DataImport.Level1DataImport);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.DataImport.Level1DataImport";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "Level1DataImportForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Level 1 Load";
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.LoadButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.LoadDetailsTabPage.ResumeLayout(false);
			this.LoadDetailsTabPage.PerformLayout();
			this.reasonsForManifestControl.ResumeLayout(true);
			this.reasonsForManifestControl.PerformLayout();
			this.reasonsControl.ResumeLayout(true);
			this.reasonsControl.PerformLayout();
			this.detailForManifestControl.ResumeLayout(true);
			this.detailForManifestControl.PerformLayout();
			this.detailControl.ResumeLayout(true);
			this.detailControl.PerformLayout();
			this.SummaryTabPage.ResumeLayout(false);
			this.SummaryTabPage.PerformLayout();
			this.SummaryGroupBox.ResumeLayout(false);
			this.SummaryGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
