using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class EXIT1MessageTypeForm
	{
		protected override void InitializeComponent()
		{
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.confirmedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.confirmingRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.oKBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 160, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 8, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 32, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "What type of message do you wish to send?";
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.confirmedRadioButton);
			this.zGroupBox1.Controls.Add(this.confirmingRadioButton);
			this.zGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 40, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 80, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Message Type";
			// 
			// ConfirmedRadioButton
			// 
			this.confirmedRadioButton.AutoCheck = false;
			this.confirmedRadioButton.BindTo = "ZX_IsConfirmed";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.EXIT1MessageType)(null)).ZX_IsConfirmed)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.EXIT1MessageType)(null)).ZX_IsConfirmedInfo)));
			this.confirmedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.confirmedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 48, true);
			this.confirmedRadioButton.Name = "ConfirmedRadioButton";
			this.confirmedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 24, true);
			this.confirmedRadioButton.TabIndex = 1;
			this.confirmedRadioButton.Text = " Confirmed message to confirm the details?";
			// 
			// ConfirmingRadioButton
			// 
			this.confirmingRadioButton.AutoCheck = false;
			this.confirmingRadioButton.BindTo = "ZX_IsConfirming";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.EXIT1MessageType)(null)).ZX_IsConfirming)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.EXIT1MessageType)(null)).ZX_IsConfirmingInfo)));
			this.confirmingRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.confirmingRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 24, true);
			this.confirmingRadioButton.Name = "ConfirmingRadioButton";
			this.confirmingRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 24, true);
			this.confirmingRadioButton.TabIndex = 0;
			this.confirmingRadioButton.Text = " Confirming message?";
			// 
			// OKBoundButton
			// 
			this.oKBoundButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.oKBoundButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 128, true);
			this.oKBoundButton.Name = "OKBoundButton";
			this.oKBoundButton.TabIndex = 3;
			this.oKBoundButton.Text = "&OK";
			this.oKBoundButton.Click += new System.EventHandler(this.OKBoundButton_Click);
			// 
			// CancelBoundButton
			// 
			this.cancelBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBoundButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 128, true);
			this.cancelBoundButton.Name = "CancelBoundButton";
			this.cancelBoundButton.TabIndex = 4;
			this.cancelBoundButton.Text = "&Cancel";
			this.cancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// EXIT1MessageTypeForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 184, true);
			this.Controls.Add(this.cancelBoundButton);
			this.Controls.Add(this.oKBoundButton);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.zLabel1);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.EXIT1MessageType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EXIT1MessageTypeForm";
			this.Text = "ReplacementTypeForEXIT1";
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.oKBoundButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.cancelBoundButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private ZArchitecture.ZLabel zLabel1;
		private ZGroupBox zGroupBox1;
		private ZRadioButton confirmingRadioButton;
		private ZRadioButton confirmedRadioButton;
		private ZButton oKBoundButton;
		private ZButton cancelBoundButton;
	}
}
