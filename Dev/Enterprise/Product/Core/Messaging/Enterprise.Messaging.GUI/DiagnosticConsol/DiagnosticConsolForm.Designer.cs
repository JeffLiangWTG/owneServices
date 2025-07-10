namespace Enterprise.Messaging.GUI
{
	public partial class DiagnosticConsolForm
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DiagnosticMessagesListBox = new CargoWise.Windows.UI.KListBox();
			this.StartButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DiagTimer = new System.Windows.Forms.Timer(this.components);
			this.AbortButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 553, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 24, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 514, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.Text = "&Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Visible = false;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// DiagnosticMessagesListBox
			// 
			this.DiagnosticMessagesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DiagnosticMessagesListBox.Font = new System.Drawing.Font("Tahoma", 8F);
			this.DiagnosticMessagesListBox.HorizontalScrollbar = true;
			this.DiagnosticMessagesListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.DiagnosticMessagesListBox.Name = "DiagnosticMessagesListBox";
			this.DiagnosticMessagesListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 485, true);
			this.DiagnosticMessagesListBox.TabIndex = 1;
			// 
			// StartButton
			// 
			this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.StartButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 514, true);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.StartButton.TabIndex = 5;
			this.StartButton.Text = "&Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// DiagTimer
			// 
			this.DiagTimer.Interval = 2000;
			this.DiagTimer.Tick += new System.EventHandler(this.DiagTimer_Tick);
			// 
			// AbortButton
			// 
			this.AbortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AbortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 514, true);
			this.AbortButton.Name = "AbortButton";
			this.AbortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AbortButton.TabIndex = 3;
			this.AbortButton.Text = "Cancel";
			this.AbortButton.UseVisualStyleBackColor = true;
			this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
			// 
			// CopyToClipboardButton
			// 
			this.CopyToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CopyToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 514, true);
			this.CopyToClipboardButton.Name = "CopyToClipboardButton";
			this.CopyToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 23, true);
			this.CopyToClipboardButton.TabIndex = 2;
			this.CopyToClipboardButton.Text = "Copy to Clipboard";
			this.CopyToClipboardButton.UseVisualStyleBackColor = true;
			this.CopyToClipboardButton.Click += new System.EventHandler(this.CopyToClipboardButton_Click);
			// 
			// DiagnosticConsolForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("f9047a3a-c06a-42ff-91ce-918e53214a42", "Diagnostic Consol");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 577, true);
			this.Controls.Add(this.CopyToClipboardButton);
			this.Controls.Add(this.AbortButton);
			this.Controls.Add(this.StartButton);
			this.Controls.Add(this.DiagnosticMessagesListBox);
			this.Controls.Add(this.CloseButton);
			this.Name = "DiagnosticConsolForm";
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.DiagnosticMessagesListBox, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.AbortButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CopyToClipboardButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		CargoWise.Windows.UI.KListBox DiagnosticMessagesListBox;
		public Enterprise.ZArchitecture.GUI.ZButton StartButton;
		public System.Windows.Forms.Timer DiagTimer;
		public Enterprise.ZArchitecture.GUI.ZButton AbortButton;
		public Enterprise.ZArchitecture.GUI.ZButton CopyToClipboardButton;
		public Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	}
}
