using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CustomFlagForm : Enterprise.ZArchitecture.GUI.ZChildForm
	{
		protected Enterprise.ZArchitecture.ZLabel zLabel1;
		protected Enterprise.ZArchitecture.ZLabel zLabel2;
		protected Enterprise.ZArchitecture.ZLabel zLabel3;
		protected Enterprise.ZArchitecture.ZTextBox zTextBox1;
		protected Enterprise.ZArchitecture.ZTextBox zTextBox2;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;
		protected Enterprise.ZArchitecture.GUI.ZButton btnCancelButton;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Authorisation Received By:";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.zLabel2.TabIndex = 1;
			this.zLabel2.Text = "Person Authorised:";
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.zLabel3.TabIndex = 1;
			this.zLabel3.Text = "Remarks:";
			// 
			// zTextBox1
			// 
			this.zTextBox1.BindTo = "PersonAuthorised";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails)(null)).PersonAuthorisedInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails)(null)).PersonAuthorised)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 32, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.zTextBox1.TabIndex = 1;
			this.zTextBox1.Text = "";
			// 
			// zTextBox2
			// 
			this.zTextBox2.BindTo = "Remarks";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails)(null)).RemarksInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails)(null)).Remarks)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 56, true);
			this.zTextBox2.Multiline = true;
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 96, true);
			this.zTextBox2.TabIndex = 2;
			this.zTextBox2.Text = "";
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.BindTo = "AuthorisationReceivedBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails)(null)).AuthorisationReceivedByInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails)(null)).AuthorisationReceivedBy)));
			this.zDropEdit1.BindToList = "Lookups+AuthReceivedByList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails)(null)).Lookups.AuthReceivedByList)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 8, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.zDropEdit1.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 160, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.TabIndex = 3;
			this.OKButton.Text = "OK";
			// 
			// btnCancelButton
			// 
			this.btnCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 160, true);
			this.btnCancelButton.Name = "btnCancelButton";
			this.btnCancelButton.TabIndex = 4;
			this.btnCancelButton.Text = "Cancel";
			// 
			// CustomFlagForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 216, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.btnCancelButton);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.UPECusHAWBFlagDetails";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 248, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 248, true);
			this.Name = "CustomFlagForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.btnCancelButton, 0);
			this.Controls.SetChildIndex(this.zTextBox2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
