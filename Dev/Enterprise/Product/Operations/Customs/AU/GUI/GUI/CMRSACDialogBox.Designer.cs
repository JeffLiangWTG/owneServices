using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CMRSACDialogBox
	{
		protected override void InitializeComponent()
		{
			this.sACCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.sACQuestionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.valueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.thesaurusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 260, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// SACCancelButton
			// 
			this.sACCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.sACCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.sACCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sACCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 236, true);
			this.sACCancelButton.Name = "SACCancelButton";
			this.sACCancelButton.TabIndex = 1;
			this.sACCancelButton.Text = "Cancel";
			this.sACCancelButton.Click += new System.EventHandler(this.SACCancelButton_Click);
			// 
			// OKButton
			// 
			this.oKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.oKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 236, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.TabIndex = 0;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// zLabel2
			// 
			this.zLabel2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.zLabel2.IsFontBold = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 192, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 40, true);
			this.zLabel2.TabIndex = 7;
			this.zLabel2.Text = "Words found in Thesaurus:";
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zLabel1
			// 
			this.zLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 168, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.zLabel1.TabIndex = 6;
			this.zLabel1.Text = "Value:";
			// 
			// SACQuestionsLabel
			// 
			this.sACQuestionsLabel.BindTo = "SACQuestion";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SACDialogBizo)(null)).SACQuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SACDialogBizo)(null)).SACQuestion)));
			this.sACQuestionsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.sACQuestionsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.sACQuestionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sACQuestionsLabel.Name = "SACQuestionsLabel";
			this.sACQuestionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 164, true);
			this.sACQuestionsLabel.TabIndex = 5;
			this.sACQuestionsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ValueLabel
			// 
			this.valueLabel.BindTo = "Value";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SACDialogBizo)(null)).ValueInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SACDialogBizo)(null)).Value)));
			this.valueLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.valueLabel.IsFontBold = true;
			this.valueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 168, true);
			this.valueLabel.Name = "ValueLabel";
			this.valueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 23, true);
			this.valueLabel.TabIndex = 8;
			// 
			// ThesaurusLabel
			// 
			this.thesaurusLabel.BindTo = "Thesaurus";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.SACDialogBizo)(null)).ThesaurusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SACDialogBizo)(null)).Thesaurus)));
			this.thesaurusLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.thesaurusLabel.IsFontBold = true;
			this.thesaurusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 192, true);
			this.thesaurusLabel.Name = "ThesaurusLabel";
			this.thesaurusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 40, true);
			this.thesaurusLabel.TabIndex = 9;
			this.thesaurusLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// CMRSACDialogBox
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 284, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 318, true);
			this.ControlBox = false;
			this.Controls.Add(this.thesaurusLabel);
			this.Controls.Add(this.valueLabel);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.sACQuestionsLabel);
			this.Controls.Add(this.sACCancelButton);
			this.Controls.Add(this.oKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.SACDialogBizo";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.MinimizeBox = false;
			this.Name = "CMRSACDialogBox";
			this.Text = "Self Assessed Clearance";
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.sACCancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.sACQuestionsLabel, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.valueLabel, 0);
			this.Controls.SetChildIndex(this.thesaurusLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		private ZButton oKButton;
		private ZButton sACCancelButton;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel sACQuestionsLabel;
		private ZArchitecture.ZLabel valueLabel;
		private ZArchitecture.ZLabel thesaurusLabel;
	}
}
