namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class SendEConversationForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.Label1 = new Enterprise.ZArchitecture.ZLabel();
			this.WarningImageBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.MessageTextBox = new Enterprise.ZArchitecture.GUI.ZAutoCompleteTextBox();
			this.Label2 = new Enterprise.ZArchitecture.ZLabel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AwaitingResponseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DiscardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WarningImageBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 261, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// Label1
			// 
			this.Label1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 14, true);
			this.Label1.Name = "Label1";
			this.Label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 23, true);
			this.Label1.TabIndex = 1;
			this.Label1.Text = "You have pending eConversation message:";
			// 
			// WarningImageBox
			// 
			this.WarningImageBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 5, true);
			this.WarningImageBox.Name = "WarningImageBox";
			this.WarningImageBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 40, true);
			this.WarningImageBox.TabIndex = 2;
			this.WarningImageBox.TabStop = false;
			// 
			// MessageTextBox
			// 
			this.MessageTextBox.AcceptsTab = true;
			this.MessageTextBox.AutocompleteManager = null;
			this.MessageTextBox.EnableValidStateColor = false;
			this.MessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 52, true);
			this.MessageTextBox.Name = "MessageTextBox";
			this.MessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 122, true);
			this.MessageTextBox.TabIndex = 3;
			this.MessageTextBox.Text = "";
			// 
			// Label2
			// 
			this.Label2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 184, true);
			this.Label2.Name = "Label2";
			this.Label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 28, true);
			this.Label2.TabIndex = 4;
			this.Label2.Text = "Please click \"Send\" or \"Awaiting Response\" to send pending message. If you choose" +
    " \"Discard\", the message will be lost.";
			// 
			// SendButton
			// 
			this.SendButton.IsCaptionOverridden = true;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 224, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 5;
			this.SendButton.Text = "Send";
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// AwaitingResponseButton
			// 
			this.AwaitingResponseButton.IsCaptionOverridden = true;
			this.AwaitingResponseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 224, true);
			this.AwaitingResponseButton.Name = "AwaitingResponseButton";
			this.AwaitingResponseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
			this.AwaitingResponseButton.TabIndex = 6;
			this.AwaitingResponseButton.Text = "Awaiting Response";
			this.AwaitingResponseButton.ToolTipCaption = null;
			this.AwaitingResponseButton.UseVisualStyleBackColor = true;
			this.AwaitingResponseButton.Click += new System.EventHandler(this.AwaitingResponseButton_Click);
			// 
			// DiscardButton
			// 
			this.DiscardButton.IsCaptionOverridden = true;
			this.DiscardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 224, true);
			this.DiscardButton.Name = "DiscardButton";
			this.DiscardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DiscardButton.TabIndex = 7;
			this.DiscardButton.Text = "Discard";
			this.DiscardButton.ToolTipCaption = null;
			this.DiscardButton.UseVisualStyleBackColor = true;
			this.DiscardButton.Click += new System.EventHandler(this.DiscardButton_Click);
			this.DiscardButton.MouseEnter += new System.EventHandler(this.DiscardButton_MouseEnter);
			this.DiscardButton.MouseLeave += new System.EventHandler(this.DiscardButton_MouseLeave);
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = ZClientEDI.Res.GetData("b0d29a21-f5c5-4a8b-aae6-1c76f911860b", "Cancel");
			this.CancelButton.IsCaptionOverridden = true;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 224, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 8;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendEConversationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 261, true);
			this.Controls.Add(this.DiscardButton);
			this.Controls.Add(this.AwaitingResponseButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.MessageTextBox);
			this.Controls.Add(this.WarningImageBox);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.CancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SendEConversationForm";
			this.Text = "Pending eConversation Message";
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.Label1, 0);
			this.Controls.SetChildIndex(this.WarningImageBox, 0);
			this.Controls.SetChildIndex(this.MessageTextBox, 0);
			this.Controls.SetChildIndex(this.Label2, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.AwaitingResponseButton, 0);
			this.Controls.SetChildIndex(this.DiscardButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WarningImageBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel Label1;
		private Enterprise.ZArchitecture.GUI.ZPictureBox WarningImageBox;
		private ZArchitecture.GUI.ZAutoCompleteTextBox MessageTextBox;
		private ZArchitecture.ZLabel Label2;
		protected ZArchitecture.GUI.ZButton SendButton;
		protected ZArchitecture.GUI.ZButton AwaitingResponseButton;
		protected ZArchitecture.GUI.ZButton DiscardButton;
		protected new ZArchitecture.GUI.ZButton CancelButton;
	}
}
