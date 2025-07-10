using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISPermitNumberForm
	{
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.aQISPermitIdGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.aQISPermitIdGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			// 
			// OKButton
			// 
			this.oKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 248, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.TabIndex = 20;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AQISPermitIdGroupBox
			// 
			this.aQISPermitIdGroupBox.Controls.Add(this.zGrid1);
			this.aQISPermitIdGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.aQISPermitIdGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aQISPermitIdGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISPermitIdGroupBox.Name = "AQISPermitIdGroupBox";
			this.aQISPermitIdGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 244, true);
			this.aQISPermitIdGroupBox.TabIndex = 23;
			this.aQISPermitIdGroupBox.TabStop = false;
			this.aQISPermitIdGroupBox.Text = "Quarantine Permit Id";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "AQISPermitIds";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISPermitIds)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 225, true);
			this.zGrid1.TabIndex = 0;
			this.FormToolTip.SetToolTip(this.zGrid1, "The number of a permit issued by Quarantine that authorises the importation of certaind" +
				" commodities that are subject to Quarantine controls of restrictions.");
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.AQISPermitId)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISPermitIds)))).CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISPermitId)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISPermitIds)))).Code)));
			// 
			// AQISPermitNumberForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 300, true);
			this.Controls.Add(this.aQISPermitIdGroupBox);
			this.Controls.Add(this.oKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AQISPermitNumberForm";
			this.Text = "Permit Number Form";
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.aQISPermitIdGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.aQISPermitIdGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		private ZButton oKButton;
		internal ZArchitecture.ZGrid zGrid1;
		private ZGroupBox aQISPermitIdGroupBox;
	}
}
