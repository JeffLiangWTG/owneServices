using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SelectLoadListForm
	{
		protected override void InitializeComponent()
		{
			this.conslPKBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.cancelButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.oKButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 70, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			// 
			// ConslPKBoundGuidFindBox
			// 
			this.conslPKBoundGuidFindBox.BindTo = "ConsolPK";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CFSLoadListSelector)(null)).ConsolPK)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CFSLoadListSelector)(null)).ConsolPKInfo)));
			this.conslPKBoundGuidFindBox.BindToList = "LoadListConsol_List";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CFSLoadListSelector)(null)).LoadListConsol_List)));
			this.conslPKBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 8, true);
			this.conslPKBoundGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.LoadListConsol;
			this.conslPKBoundGuidFindBox.Name = "ConslPKBoundGuidFindBox";
			this.conslPKBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.conslPKBoundGuidFindBox.TabIndex = 1;
			// 
			// CancelButton1
			// 
			this.cancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 40, true);
			this.cancelButton1.Name = "CancelButton1";
			this.cancelButton1.TabIndex = 2;
			this.cancelButton1.Text = "Cancel";
			this.cancelButton1.Click += new System.EventHandler(this.CancelButton1_Click);
			// 
			// OKButton1
			// 
			this.oKButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			this.oKButton1.Name = "OKButton1";
			this.oKButton1.TabIndex = 3;
			this.oKButton1.Text = "OK";
			this.oKButton1.Click += new System.EventHandler(this.OKButton1_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Load List:";
			// 
			// SelectLoadListForm
			// 
			this.AcceptButton = this.oKButton1;

			this.CancelButton = this.cancelButton1;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 94, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.oKButton1);
			this.Controls.Add(this.cancelButton1);
			this.Controls.Add(this.conslPKBoundGuidFindBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CFSLoadListSelector";
			this.Name = "SelectLoadListForm";
			this.Text = "Select Load List";
			this.Controls.SetChildIndex(this.conslPKBoundGuidFindBox, 0);
			this.Controls.SetChildIndex(this.cancelButton1, 0);
			this.Controls.SetChildIndex(this.oKButton1, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		ZGuidFindBox conslPKBoundGuidFindBox;
		ZButton cancelButton1;
		ZButton oKButton1;
		ZArchitecture.ZLabel zLabel1;
	}
}
