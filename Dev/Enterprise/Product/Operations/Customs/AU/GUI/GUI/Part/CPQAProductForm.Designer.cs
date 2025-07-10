using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAProductForm
	{
		protected override void InitializeComponent()
		{
			this.userControlPanel = new ZPanel();
			this.cpqaProductUserControl1 = new CPQAProductUserControl();
			this.oKButton = new ZButton();
			this.cMRRefreshCPDecQuestionsButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.userControlPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 359, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(308);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(309);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AUOrgSupplierPart);
			// 
			// UserControlPanel
			// 
			this.userControlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.userControlPanel.Controls.Add(this.cpqaProductUserControl1);
			this.userControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.userControlPanel.Name = "UserControlPanel";
			this.userControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 324, true);
			this.userControlPanel.TabIndex = 2;
			// 
			// cpqaProductUserControl1
			// 
			this.cpqaProductUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cpqaProductUserControl1, ".");
			this.cpqaProductUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cpqaProductUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cpqaProductUserControl1.Name = "cpqaProductUserControl1";
			this.cpqaProductUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 324, true);
			this.cpqaProductUserControl1.TabIndex = 2;
			// 
			// OKButton
			// 
			this.oKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 332, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.oKButton.TabIndex = 5;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// CMRRefreshCPDecQuestionsButton
			// 
			this.cMRRefreshCPDecQuestionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cMRRefreshCPDecQuestionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 332, true);
			this.cMRRefreshCPDecQuestionsButton.Name = "CMRRefreshCPDecQuestionsButton";
			this.cMRRefreshCPDecQuestionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 22, true);
			this.cMRRefreshCPDecQuestionsButton.TabIndex = 4;
			this.cMRRefreshCPDecQuestionsButton.Text = "Refresh CP Dec Questions";
			this.cMRRefreshCPDecQuestionsButton.Click += new EventHandler(this.CMRRefreshCPDecQuestionsButton_Click);
			// 
			// CPQAProductForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 382, true);
			this.Controls.Add(this.cMRRefreshCPDecQuestionsButton);
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.userControlPanel);
			this.DataSourceType = typeof(AUOrgSupplierPart);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 386, true);
			this.Name = "CPQAProductForm";
			this.Controls.SetChildIndex(this.userControlPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.cMRRefreshCPDecQuestionsButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.userControlPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		ZPanel userControlPanel;
		ZButton oKButton;
		ZButton cMRRefreshCPDecQuestionsButton;
		CPQAProductUserControl cpqaProductUserControl1;
	}
}
