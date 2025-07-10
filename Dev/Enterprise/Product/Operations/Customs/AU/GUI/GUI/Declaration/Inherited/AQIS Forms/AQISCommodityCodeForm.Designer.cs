using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISCommodityCodeForm
	{
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.aQISCommodityCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.aQISCommodityCodeGroupBox.SuspendLayout();
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
			// AQISCommodityCodeGroupBox
			// 
			this.aQISCommodityCodeGroupBox.Controls.Add(this.zGrid1);
			this.aQISCommodityCodeGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.aQISCommodityCodeGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aQISCommodityCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISCommodityCodeGroupBox.Name = "AQISCommodityCodeGroupBox";
			this.aQISCommodityCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 244, true);
			this.aQISCommodityCodeGroupBox.TabIndex = 21;
			this.aQISCommodityCodeGroupBox.TabStop = false;
			this.aQISCommodityCodeGroupBox.Text = "Quarantine Commodity Codes";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "AQISCommodityCodes";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISCommodityCodes)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+AQISCommodityCodeList";
			zDropEditColumnStyleInfo1.Caption = "Code";
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 225, true);
			this.zGrid1.TabIndex = 0;
			this.FormToolTip.SetToolTip(this.zGrid1, @"The Quarantine Commodity Code is a Quarantine initiative that allows for a more detailed description of the goods. Importers/brokers have the option of selecting a Commodity Code during the process of lodging an import declaration to identify the particular commodity of interest to Quarantine within a broad Customs tariff classification. Along with the tariff classification, the Commodity Code allows Quarantine to have a more detailed description of the goods therefore assisting in timely and accurate processing. Using a Commodity Code will allow improved efficiency of entry lodgement by reducing the need for users to answer a series of entry lodgement questions to identify commodities of interest to Quarantine (within tariff classifications containing goods of potential quarantine risk). NOTE: There will not be any Commodity Codes available when the ICS is implemented, but will be phased in for certain tariffs post ICS implementation. Full details regarding the Commodity Code initiative are outlined in the AQIS Import Clearance Notice to Industry 2003/04 – No 5.");
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.AQISCommodityCode)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISCommodityCodes)))).CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISCommodityCode)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISCommodityCodes)))).Code)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AQISCommodityCode)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISCommodityCodes)))).Lookups.AQISCommodityCodeList)));
			// 
			// AQISCommodityCodeForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 300, true);
			this.Controls.Add(this.aQISCommodityCodeGroupBox);
			this.Controls.Add(this.oKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AQISCommodityCodeForm";
			this.Text = "Commodity Code Form";
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.aQISCommodityCodeGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.aQISCommodityCodeGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		private ZButton oKButton;
		private ZGroupBox aQISCommodityCodeGroupBox;
		internal ZArchitecture.ZGrid zGrid1;
	}
}
