using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CMRDrawbackDecsForm
	{
		protected override void InitializeComponent()
		{
			this.buttonsPanel = new ZPanel();
			this.oKButton = new ZButton();
			this.cPQACancelButton = new ZButton();
			this.mainPanel = new ZPanel();
			this.declarationQuestionsGroupBox = new ZGroupBox();
			this.cPQAsForDrawbackControl = new CPQAsForDrawbackControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.buttonsPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.declarationQuestionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 558, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// ButtonsPanel
			// 
			this.buttonsPanel.Controls.Add(this.oKButton);
			this.buttonsPanel.Controls.Add(this.cPQACancelButton);
			this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 518, true);
			this.buttonsPanel.Name = "ButtonsPanel";
			this.buttonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 40, true);
			this.buttonsPanel.TabIndex = 1;
			// 
			// OKButton
			// 
			this.oKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 8, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.oKButton.TabIndex = 0;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// CPQACancelButton
			// 
			this.cPQACancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cPQACancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 8, true);
			this.cPQACancelButton.Name = "CPQACancelButton";
			this.cPQACancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.cPQACancelButton.TabIndex = 1;
			this.cPQACancelButton.Text = "Cancel";
			this.cPQACancelButton.Click += new EventHandler(this.CPQACancelButton_Click);
			// 
			// MainPanel
			// 
			this.mainPanel.Controls.Add(this.declarationQuestionsGroupBox);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "MainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 518, true);
			this.mainPanel.TabIndex = 0;
			// 
			// DeclarationQuestionsGroupBox
			// 
			this.declarationQuestionsGroupBox.Controls.Add(this.cPQAsForDrawbackControl);
			this.declarationQuestionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.declarationQuestionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.declarationQuestionsGroupBox.Name = "DeclarationQuestionsGroupBox";
			this.declarationQuestionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 518, true);
			this.declarationQuestionsGroupBox.TabIndex = 0;
			this.declarationQuestionsGroupBox.TabStop = false;
			this.declarationQuestionsGroupBox.Text = "Declaration Questions";
			// 
			// CPQAsForDrawbackControl
			// 
			this.cPQAsForDrawbackControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cPQAsForDrawbackControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.cPQAsForDrawbackControl.Name = "CPQAsForDrawbackControl2";
			this.cPQAsForDrawbackControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 499, true);
			this.cPQAsForDrawbackControl.TabIndex = 0;
			// 
			// CMRDrawbackDecsForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 582, true);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.buttonsPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobDeclaration";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 616, true);
			this.Name = "CMRDrawbackDecsForm";
			this.Text = "CPQA";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.buttonsPanel, 0);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.buttonsPanel.ResumeLayout(false);
			this.mainPanel.ResumeLayout(false);
			this.declarationQuestionsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		ZPanel buttonsPanel;
		ZButton oKButton;
		ZButton cPQACancelButton;
		ZPanel mainPanel;
		ZGroupBox declarationQuestionsGroupBox;
		CPQAsForDrawbackControl cPQAsForDrawbackControl;
	}
}
