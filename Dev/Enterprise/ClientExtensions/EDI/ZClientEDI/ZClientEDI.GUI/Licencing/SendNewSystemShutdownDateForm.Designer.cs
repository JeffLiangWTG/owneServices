namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class SendNewSystemShutdownDateForm
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
		protected new void InitializeComponent()
		{
			this.NewSystemShutDownDateBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.oldVersionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.expiryMonthButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.expiryWeekButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.expiredButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.expiryMonthBox = new Enterprise.ZArchitecture.ZTextBox();
			this.expiryWeekBox = new Enterprise.ZArchitecture.ZTextBox();
			this.expiryBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NewSystemShutDownDateBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 376, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 10, true);
			this.MainStatusBar.Text = "4";
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.SystemShutdownDate);
			// 
			// NewSystemShutDownDateBox
			// 
			this.NewSystemShutDownDateBox.AllowDrop = true;
			this.NewSystemShutDownDateBox.AutoCompleteMonthThreshold = 1;
			this.NewSystemShutDownDateBox.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NewSystemShutDownDateBox, "NewSystemShutdownDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Licencing.Business.SystemShutdownDate)(null)).NewSystemShutdownDate)));
			this.NewSystemShutDownDateBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("22011c53-6d71-40e3-81b8-74199f5361e3", "System Shutdown Date");
			this.NewSystemShutDownDateBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 30, true);
			this.NewSystemShutDownDateBox.Name = "NewSystemShutDownDateBox";
			this.NewSystemShutDownDateBox.TabIndex = 1;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.SendButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 345, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 23, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.Text = "Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.Controls.Add(this.oldVersionLabel);
			this.zGroupBox1.Controls.Add(this.zLabel1);
			this.zGroupBox1.Controls.Add(this.expiryMonthButton);
			this.zGroupBox1.Controls.Add(this.expiryWeekButton);
			this.zGroupBox1.Controls.Add(this.expiredButton);
			this.zGroupBox1.Controls.Add(this.expiryMonthBox);
			this.zGroupBox1.Controls.Add(this.expiryWeekBox);
			this.zGroupBox1.Controls.Add(this.expiryBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 59, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 277, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Custom Expiry Messages";
			// 
			// oldVersionLabel
			// 
			this.oldVersionLabel.ForeColor = System.Drawing.Color.Red;
			this.oldVersionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 37, true);
			this.oldVersionLabel.Name = "oldVersionLabel";
			this.oldVersionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 16, true);
			this.oldVersionLabel.TabIndex = 2;
			this.oldVersionLabel.Text = "This database is currently on older software that doesn\'t show custom expiry mess" +
    "ages.";
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 17, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 16, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Leave blank to use the standard message. The macro {ExpiryDate} will be replaced " +
    "with the actual expiry date and time.";
			// 
			// expiryMonthButton
			// 
			this.expiryMonthButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.expiryMonthButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(617, 198, true);
			this.expiryMonthButton.Name = "expiryMonthButton";
			this.expiryMonthButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 23, true);
			this.expiryMonthButton.TabIndex = 8;
			this.expiryMonthButton.Text = "...";
			this.expiryMonthButton.UseVisualStyleBackColor = true;
			this.expiryMonthButton.Click += new System.EventHandler(this.expiryMonthButton_Click);
			// 
			// expiryWeekButton
			// 
			this.expiryWeekButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.expiryWeekButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(617, 130, true);
			this.expiryWeekButton.Name = "expiryWeekButton";
			this.expiryWeekButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 23, true);
			this.expiryWeekButton.TabIndex = 6;
			this.expiryWeekButton.Text = "...";
			this.expiryWeekButton.UseVisualStyleBackColor = true;
			this.expiryWeekButton.Click += new System.EventHandler(this.expiryWeekButton_Click);
			// 
			// expiredButton
			// 
			this.expiredButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.expiredButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(617, 62, true);
			this.expiredButton.Name = "expiredButton";
			this.expiredButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 23, true);
			this.expiredButton.TabIndex = 4;
			this.expiredButton.Text = "...";
			this.expiredButton.UseVisualStyleBackColor = true;
			this.expiredButton.Click += new System.EventHandler(this.expiredButton_Click);
			// 
			// expiryMonthBox
			// 
			this.expiryMonthBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.expiryMonthBox, "ExpiryMonthMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.SystemShutdownDate)(null)).ExpiryMonthMessage)));
			this.expiryMonthBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7481f421-0b2d-460a-b590-4c65f042b300", "Expiry Within Month");
			this.expiryMonthBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.expiryMonthBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 197, true);
			this.expiryMonthBox.Multiline = true;
			this.expiryMonthBox.Name = "expiryMonthBox";
			this.expiryMonthBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.expiryMonthBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 59, true);
			this.expiryMonthBox.TabIndex = 7;
			// 
			// expiryWeekBox
			// 
			this.expiryWeekBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.expiryWeekBox, "ExpiryWeekMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.SystemShutdownDate)(null)).ExpiryWeekMessage)));
			this.expiryWeekBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("361b0740-2565-4683-9a3d-0701b903307b", "Expiry Within Week");
			this.expiryWeekBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.expiryWeekBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 129, true);
			this.expiryWeekBox.Multiline = true;
			this.expiryWeekBox.Name = "expiryWeekBox";
			this.expiryWeekBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.expiryWeekBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 59, true);
			this.expiryWeekBox.TabIndex = 5;
			// 
			// expiryBox
			// 
			this.expiryBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.expiryBox, "ExpiredMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.SystemShutdownDate)(null)).ExpiredMessage)));
			this.expiryBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("88bf2c31-473f-426b-8ec7-dafff0599a73", "Expired");
			this.expiryBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.expiryBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 61, true);
			this.expiryBox.Multiline = true;
			this.expiryBox.Name = "expiryBox";
			this.expiryBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.expiryBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 59, true);
			this.expiryBox.TabIndex = 3;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 345, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// zLabel2
			// 
			this.zLabel2.ForeColor = System.Drawing.Color.Red;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 7, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 16, true);
			this.zLabel2.TabIndex = 5;
			this.zLabel2.Text = "The system will not be usable after this date unless a new or blank date is sent." +
    "";
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 31, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 16, true);
			this.zLabel3.TabIndex = 6;
			this.zLabel3.Text = "Set to blank to restore automatic monthly renewals.";
			// 
			// SendNewSystemShutdownDateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 386, true);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.NewSystemShutDownDateBox);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.SystemShutdownDate);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 401, true);
			this.Name = "SendNewSystemShutdownDateForm";
			this.Text = "Send Manual System Shutdown Date";
			this.Controls.SetChildIndex(this.NewSystemShutDownDateBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NewSystemShutDownDateBox.ResumeLayout(true);
			this.NewSystemShutDownDateBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit NewSystemShutDownDateBox;
		public ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZTextBox expiryMonthBox;
		private ZArchitecture.ZTextBox expiryWeekBox;
		private ZArchitecture.ZTextBox expiryBox;
		private ZArchitecture.GUI.ZButton expiryMonthButton;
		private ZArchitecture.GUI.ZButton expiryWeekButton;
		private ZArchitecture.GUI.ZButton expiredButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.ZLabel oldVersionLabel;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel3;
	}
}
