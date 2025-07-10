using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAClassificationForm
	{
		protected override void InitializeComponent()
		{
			this.oKButton = new ZButton();
			this.cMRRefreshCPDecQuestionsButton = new ZButton();
			this.cpqaClassificationUserControl1 = new CPQAClassificationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 359, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 23, true);
			this.MainStatusBar.TabIndex = 2;
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
			this.BindingSource.DataSourceType = typeof(Classification);
			// 
			// OKButton
			// 
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 335, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.oKButton.TabIndex = 2;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// CMRRefreshCPDecQuestionsButton
			// 
			this.cMRRefreshCPDecQuestionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(549, 335, true);
			this.cMRRefreshCPDecQuestionsButton.Name = "CMRRefreshCPDecQuestionsButton";
			this.cMRRefreshCPDecQuestionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.cMRRefreshCPDecQuestionsButton.TabIndex = 1;
			this.cMRRefreshCPDecQuestionsButton.Text = "Refresh CP Dec Questions";
			this.cMRRefreshCPDecQuestionsButton.Click += new EventHandler(this.CMRRefreshCPDecQuestionsButton_Click);
			// 
			// cpqaClassificationUserControl1
			// 
			this.cpqaClassificationUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cpqaClassificationUserControl1, ".");
			this.cpqaClassificationUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cpqaClassificationUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 332, true);
			this.cpqaClassificationUserControl1.Name = "cpqaClassificationUserControl1";
			this.cpqaClassificationUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 332, true);
			this.cpqaClassificationUserControl1.TabIndex = 0;
			// 
			// CPQAClassificationForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 382, true);
			this.Controls.Add(this.cMRRefreshCPDecQuestionsButton);
			this.Controls.Add(this.cpqaClassificationUserControl1);
			this.Controls.Add(this.oKButton);
			this.DataSourceType = typeof(Classification);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 420, true);
			this.Name = "CPQAClassificationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.cpqaClassificationUserControl1, 0);
			this.Controls.SetChildIndex(this.cMRRefreshCPDecQuestionsButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		CPQAClassificationUserControl cpqaClassificationUserControl1;
		ZButton cMRRefreshCPDecQuestionsButton;
		ZButton oKButton;
	}
}
