using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISProducerCodeForm
	{
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.aQISPorducerCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.aQISPorducerCodeGroupBox.SuspendLayout();
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
			this.oKButton.TabIndex = 10;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AQISPorducerCodeGroupBox
			// 
			this.aQISPorducerCodeGroupBox.Controls.Add(this.zGrid1);
			this.aQISPorducerCodeGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.aQISPorducerCodeGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aQISPorducerCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISPorducerCodeGroupBox.Name = "AQISPorducerCodeGroupBox";
			this.aQISPorducerCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 244, true);
			this.aQISPorducerCodeGroupBox.TabIndex = 24;
			this.aQISPorducerCodeGroupBox.TabStop = false;
			this.aQISPorducerCodeGroupBox.Text = "Quarantine Producer Code";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "AQISProducerCodes";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISProducerCodes)));
			this.zGrid1.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+AQISProducerCodeList";
			zCodeFindBoxColumnStyleInfo1.Caption = "Code";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Code";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AQISProducerCode;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 225, true);
			this.zGrid1.TabIndex = 0;
			this.FormToolTip.SetToolTip(this.zGrid1, "Relates to food shipments. A requirement of Quarantine IFP scheme, the producer code in" +
				"dicates who actually manufactured the product, not who supplied it. The broker n" +
				"ominates a producer code for a line of food when required by certain Quarantine " +
				"profiles.");
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.AQISProducerCode)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISProducerCodes)))).CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISProducerCode)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISProducerCodes)))).Code)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AQISProducerCode)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISProducerCodes)))).Lookups.AQISProducerCodeList)));
			// 
			// AQISProducerCodeForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 300, true);
			this.Controls.Add(this.aQISPorducerCodeGroupBox);
			this.Controls.Add(this.oKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AQISProducerCodeForm";
			this.Text = "Producer Code Form";
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.aQISPorducerCodeGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.aQISPorducerCodeGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		private ZButton oKButton;
		internal ZArchitecture.ZGrid zGrid1;
		private ZGroupBox aQISPorducerCodeGroupBox;
	}
}
