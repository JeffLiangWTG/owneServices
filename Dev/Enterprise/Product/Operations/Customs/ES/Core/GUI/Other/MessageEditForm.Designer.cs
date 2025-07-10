namespace Enterprise.Customs.ES.GUI
{
	partial class MessageEditForm
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
			this.zTextBoxMessage = new Enterprise.ZArchitecture.ZTextBox();
			this.zButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 24, true);
			// 
			// zTextBoxMessage
			// 
			this.zTextBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.zTextBoxMessage.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBoxMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.zTextBoxMessage.Multiline = true;
			this.zTextBoxMessage.Name = "zTextBoxMessage";
			this.zTextBoxMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBoxMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 171, true);
			this.zTextBoxMessage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("8534B6EC-39DB-40B1-9DB5-834C5D9A80FF", "Edit");
			this.zTextBoxMessage.TabIndex = 1;
			// 
			// zButtonOK
			// 
			this.zButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.zButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 189, true);
			this.zButtonOK.Name = "zButtonOK";
			this.zButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonOK.TabIndex = 2;
			this.zButtonOK.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("05DCEC7B-C712-43E1-B57A-B743FBAA67C0", "OK");
			this.zButtonOK.UseVisualStyleBackColor = true;
			// 
			// zButtonCancel
			// 
			this.zButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 189, true);
			this.zButtonCancel.Name = "zButtonCancel";
			this.zButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonCancel.TabIndex = 3;
			this.zButtonCancel.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("25C5CFDE-0DB7-4496-B321-58584186162B", "Cancel");
			this.zButtonCancel.UseVisualStyleBackColor = true;
			// 
			// MessageEditForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 241, true);
			this.Controls.Add(this.zTextBoxMessage);
			this.Controls.Add(this.zButtonCancel);
			this.Controls.Add(this.zButtonOK);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.Name = "MessageEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("30CDEAFF-EEFE-47EA-88D0-451E2D7179ED", "Message Text");
			this.Controls.SetChildIndex(this.zButtonOK, 0);
			this.Controls.SetChildIndex(this.zButtonCancel, 0);
			this.Controls.SetChildIndex(this.zTextBoxMessage, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZTextBox zTextBoxMessage;
		private Enterprise.ZArchitecture.GUI.ZButton zButtonOK;
		private Enterprise.ZArchitecture.GUI.ZButton zButtonCancel;
	}
}
