namespace Enterprise.Client.EDI.DbRestoreKey
{
	partial class DbRestoreKeyForm
	{
		protected Enterprise.ZArchitecture.ZTextBox ServerTextBox;
		protected Enterprise.ZArchitecture.ZTextBox SessionIdTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton GenerateButton;
		protected Enterprise.ZArchitecture.ZTextBox KeyTextBox;
		protected Enterprise.ZArchitecture.ZLabel ServerLabel;
		protected Enterprise.ZArchitecture.ZLabel DatabaseLabel;
		protected Enterprise.ZArchitecture.ZLabel SessionIdLabel;
		protected Enterprise.ZArchitecture.ZLabel KeyLabel;
		protected Enterprise.ZArchitecture.ZTextBox DatabaseTextBox;

		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;

		protected override void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ServerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DatabaseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SessionIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GenerateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.KeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ServerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DatabaseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SessionIdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.KeyLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 183, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 26, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.DbRestoreKey.DbRestoreKeyGenerator);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 146, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Text = "Close";
			// 
			// ServerTextBox
			// 
			this.ServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServerTextBox, "ServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.DbRestoreKey.DbRestoreKeyGenerator)(null)).ServerName)));
			this.ServerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 12, true);
			this.ServerTextBox.Name = "ServerTextBox";
			this.ServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 20, true);
			this.ServerTextBox.TabIndex = 1;
			this.ServerTextBox.TextChanged += new System.EventHandler(this.ServerTextBox_TextChanged);
			// 
			// DatabaseTextBox
			// 
			this.DatabaseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DatabaseTextBox, "DatabaseName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.DbRestoreKey.DbRestoreKeyGenerator)(null)).DatabaseName)));
			this.DatabaseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DatabaseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 38, true);
			this.DatabaseTextBox.Name = "DatabaseTextBox";
			this.DatabaseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 20, true);
			this.DatabaseTextBox.TabIndex = 3;
			this.DatabaseTextBox.TextChanged += new System.EventHandler(this.DatabaseTextBox_TextChanged);
			// 
			// SessionIdTextBox
			// 
			this.SessionIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SessionIdTextBox, "SessionId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.DbRestoreKey.DbRestoreKeyGenerator)(null)).SessionId)));
			this.SessionIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SessionIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 64, true);
			this.SessionIdTextBox.Name = "SessionIdTextBox";
			this.SessionIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 20, true);
			this.SessionIdTextBox.TabIndex = 5;
			this.SessionIdTextBox.TextChanged += new System.EventHandler(this.SessionIdTextBox_TextChanged);
			// 
			// GenerateButton
			// 
			this.GenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 90, true);
			this.GenerateButton.Name = "GenerateButton";
			this.GenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.GenerateButton.TabIndex = 6;
			this.GenerateButton.Text = "Generate";
			this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
			// 
			// KeyTextBox
			// 
			this.KeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.KeyTextBox, "ReleaseKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.DbRestoreKey.DbRestoreKeyGenerator)(null)).ReleaseKey)));
			this.KeyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.KeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 120, true);
			this.KeyTextBox.Name = "KeyTextBox";
			this.KeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 20, true);
			this.KeyTextBox.TabIndex = 8;
			// 
			// ServerLabel
			// 
			this.ServerLabel.AutoSize = true;
			this.ServerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 15, true);
			this.ServerLabel.Name = "ServerLabel";
			this.ServerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 13, true);
			this.ServerLabel.TabIndex = 0;
			this.ServerLabel.Text = "Server:";
			// 
			// DatabaseLabel
			// 
			this.DatabaseLabel.AutoSize = true;
			this.DatabaseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 41, true);
			this.DatabaseLabel.Name = "DatabaseLabel";
			this.DatabaseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.DatabaseLabel.TabIndex = 2;
			this.DatabaseLabel.Text = "Database:";
			// 
			// SessionIdLabel
			// 
			this.SessionIdLabel.AutoSize = true;
			this.SessionIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 67, true);
			this.SessionIdLabel.Name = "SessionIdLabel";
			this.SessionIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.SessionIdLabel.TabIndex = 4;
			this.SessionIdLabel.Text = "Session ID:";
			// 
			// KeyLabel
			// 
			this.KeyLabel.AutoSize = true;
			this.KeyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 123, true);
			this.KeyLabel.Name = "KeyLabel";
			this.KeyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
			this.KeyLabel.TabIndex = 7;
			this.KeyLabel.Text = "Key:";
			// 
			// DbRestoreKeyForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 209, true);
			this.Controls.Add(this.KeyLabel);
			this.Controls.Add(this.SessionIdLabel);
			this.Controls.Add(this.DatabaseLabel);
			this.Controls.Add(this.ServerLabel);
			this.Controls.Add(this.KeyTextBox);
			this.Controls.Add(this.GenerateButton);
			this.Controls.Add(this.SessionIdTextBox);
			this.Controls.Add(this.DatabaseTextBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ServerTextBox);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.DbRestoreKey.DbRestoreKeyGenerator);
			this.DataSourceTypeName = "Enterprise.Client.EDI.DbRestoreKey.DbRestoreKeyGenerator";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 245, true);
			this.Name = "DbRestoreKeyForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Database Restore - Release Key Generator";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ServerTextBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.DatabaseTextBox, 0);
			this.Controls.SetChildIndex(this.SessionIdTextBox, 0);
			this.Controls.SetChildIndex(this.GenerateButton, 0);
			this.Controls.SetChildIndex(this.KeyTextBox, 0);
			this.Controls.SetChildIndex(this.ServerLabel, 0);
			this.Controls.SetChildIndex(this.DatabaseLabel, 0);
			this.Controls.SetChildIndex(this.SessionIdLabel, 0);
			this.Controls.SetChildIndex(this.KeyLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
