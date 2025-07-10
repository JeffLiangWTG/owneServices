namespace Enterprise.Customs.BR.GUI
{
	partial class SingleMessageSendingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.CredentialGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CredentialGroupBox.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 129, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.BaseSingleMessageSendingObject);
			// 
			// CredentialGroupBox
			// 
			this.CredentialGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6195c59f-ed04-47df-a6fc-da855f294a3d", "Enter Credential");
			this.CredentialGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.CredentialGroupBox.Controls.Add(this.SendButton);
			this.CredentialGroupBox.Controls.Add(this.CancelButton);
			this.CredentialGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialGroupBox.Name = "CredentialGroupBox";
			this.CredentialGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 129, true);
			this.CredentialGroupBox.TabIndex = 0;
			this.CredentialGroupBox.TabStop = false;
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("9caaad35-1afe-4731-992f-62501877af8b", "Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 99, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("185389de-cd75-4d21-806f-0bae9ce1b447", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 99, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "BrokerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.BaseSingleMessageSendingObject)(null)).BrokerCode)));
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("c5f11f74-d262-4224-846c-bbbcfb22dda2", "Broker");
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 40, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// GoodsCatalogMessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("080bc4c8-66bb-497f-9790-29e0c3439ac5", "Send Goods Catalog");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 153, true);
			this.Controls.Add(this.CredentialGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.BaseSingleMessageSendingObject);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GoodsCatalogMessageSendingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CredentialGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CredentialGroupBox.ResumeLayout(false);
			this.CredentialGroupBox.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox CredentialGroupBox;
		internal new ZArchitecture.GUI.ZButton CancelButton;
		internal ZArchitecture.GUI.ZButton SendButton;
		internal ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
	}
}
