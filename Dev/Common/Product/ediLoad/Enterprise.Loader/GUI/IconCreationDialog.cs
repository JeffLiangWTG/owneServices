using System.Windows.Forms;
using CargoWise.BrandManager;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Loader
{
	sealed class IconCreationDialog : Form, IIconCreationDialog
	{
		private PictureBox pictureBox1;
		private Label label1;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private CheckBox StartMenuCheckBox;
		private CheckBox DesktopCheckBox;
		private RadioButton JustMeRadioButton;
		private RadioButton AllUsersRadioButton;
		private Button CreateShortcutsButton;
		private Button DontCreateButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private readonly System.ComponentModel.Container components;
		private Label instanceDescriptionLabel;
		private TextBox instanceDescriptionTextBox;

		readonly IconCreationOptions fIconCreationOptions = new IconCreationOptions();

		public IconCreationOptions IconCreationOptions
		{
			get
			{
				return fIconCreationOptions;
			}
		}

		public IconCreationDialog()
		{
			InitializeComponent();
			this.Icon = BrandingFactory.Instance.ProductIcon;
			this.pictureBox1.Image = BrandingFactory.Instance.ProductIcon.ToBitmap();
		}

		public void SetInstanceDescription(string description, bool isReadonly)
		{
			this.instanceDescriptionTextBox.Text = description;
			this.instanceDescriptionTextBox.ReadOnly = isReadonly;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new Label();
			this.groupBox1 = new GroupBox();
			this.StartMenuCheckBox = new CheckBox();
			this.DesktopCheckBox = new CheckBox();
			this.groupBox2 = new GroupBox();
			this.JustMeRadioButton = new RadioButton();
			this.AllUsersRadioButton = new RadioButton();
			this.CreateShortcutsButton = new Button();
			this.DontCreateButton = new Button();
			this.pictureBox1 = new PictureBox();
			this.instanceDescriptionLabel = new Label();
			this.instanceDescriptionTextBox = new TextBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(64, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(216, 57);
			this.label1.TabIndex = 0;
			this.label1.Text = "This is the first time you have run this instance of this application on this comput" +
	"er.  Choose how you would like to create shortcut icons.";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.StartMenuCheckBox);
			this.groupBox1.Controls.Add(this.DesktopCheckBox);
			this.groupBox1.FlatStyle = FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 72);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(280, 72);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Where do you want the icons?";
			// 
			// StartMenuCheckBox
			// 
			this.StartMenuCheckBox.Checked = true;
			this.StartMenuCheckBox.CheckState = CheckState.Checked;
			this.StartMenuCheckBox.FlatStyle = FlatStyle.System;
			this.StartMenuCheckBox.Location = new System.Drawing.Point(16, 16);
			this.StartMenuCheckBox.Name = "StartMenuCheckBox";
			this.StartMenuCheckBox.Size = new System.Drawing.Size(168, 24);
			this.StartMenuCheckBox.TabIndex = 0;
			this.StartMenuCheckBox.Text = "In the Start Menu";
			// 
			// DesktopCheckBox
			// 
			this.DesktopCheckBox.Checked = true;
			this.DesktopCheckBox.CheckState = CheckState.Checked;
			this.DesktopCheckBox.FlatStyle = FlatStyle.System;
			this.DesktopCheckBox.Location = new System.Drawing.Point(16, 40);
			this.DesktopCheckBox.Name = "DesktopCheckBox";
			this.DesktopCheckBox.Size = new System.Drawing.Size(168, 24);
			this.DesktopCheckBox.TabIndex = 1;
			this.DesktopCheckBox.Text = "On the Desktop";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.JustMeRadioButton);
			this.groupBox2.Controls.Add(this.AllUsersRadioButton);
			this.groupBox2.FlatStyle = FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 152);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(280, 72);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Who should see the icons?";
			// 
			// JustMeRadioButton
			// 
			this.JustMeRadioButton.Checked = true;
			this.JustMeRadioButton.FlatStyle = FlatStyle.System;
			this.JustMeRadioButton.Location = new System.Drawing.Point(16, 16);
			this.JustMeRadioButton.Name = "JustMeRadioButton";
			this.JustMeRadioButton.Size = new System.Drawing.Size(168, 24);
			this.JustMeRadioButton.TabIndex = 0;
			this.JustMeRadioButton.TabStop = true;
			this.JustMeRadioButton.Text = "Just me";
			// 
			// AllUsersRadioButton
			// 
			this.AllUsersRadioButton.FlatStyle = FlatStyle.System;
			this.AllUsersRadioButton.Location = new System.Drawing.Point(16, 40);
			this.AllUsersRadioButton.Name = "AllUsersRadioButton";
			this.AllUsersRadioButton.Size = new System.Drawing.Size(168, 24);
			this.AllUsersRadioButton.TabIndex = 1;
			this.AllUsersRadioButton.Text = "All users of this computer";
			// 
			// CreateShortcutsButton
			// 
			this.CreateShortcutsButton.DialogResult = DialogResult.OK;
			this.CreateShortcutsButton.FlatStyle = FlatStyle.System;
			this.CreateShortcutsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			this.CreateShortcutsButton.Location = new System.Drawing.Point(16, 257);
			this.CreateShortcutsButton.Name = "CreateShortcutsButton";
			this.CreateShortcutsButton.Size = new System.Drawing.Size(128, 23);
			this.CreateShortcutsButton.TabIndex = 3;
			this.CreateShortcutsButton.Text = "Create Shortcuts";
			// 
			// DontCreateButton
			// 
			this.DontCreateButton.DialogResult = DialogResult.Cancel;
			this.DontCreateButton.FlatStyle = FlatStyle.System;
			this.DontCreateButton.Location = new System.Drawing.Point(152, 257);
			this.DontCreateButton.Name = "DontCreateButton";
			this.DontCreateButton.Size = new System.Drawing.Size(128, 23);
			this.DontCreateButton.TabIndex = 4;
			this.DontCreateButton.Text = "Don\'t Create Shortcuts";
			this.DontCreateButton.Click += new System.EventHandler(this.DontCreateButton_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(8, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(48, 48);
			this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// instanceDescriptionLabel
			// 
			this.instanceDescriptionLabel.AutoSize = true;
			this.instanceDescriptionLabel.Location = new System.Drawing.Point(8, 231);
			this.instanceDescriptionLabel.Name = "instanceDescriptionLabel";
			this.instanceDescriptionLabel.Size = new System.Drawing.Size(107, 13);
			this.instanceDescriptionLabel.TabIndex = 5;
			this.instanceDescriptionLabel.Text = "Instance Description:";
			// 
			// instanceDescriptionTextBox
			// 
			this.instanceDescriptionTextBox.Location = new System.Drawing.Point(121, 231);
			this.instanceDescriptionTextBox.Name = "instanceDescriptionTextBox";
			this.instanceDescriptionTextBox.Size = new System.Drawing.Size(167, 20);
			this.instanceDescriptionTextBox.TabIndex = 6;
			// 
			// IconCreationDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 288);
			this.Controls.Add(this.instanceDescriptionTextBox);
			this.Controls.Add(this.instanceDescriptionLabel);
			this.Controls.Add(this.DontCreateButton);
			this.Controls.Add(this.CreateShortcutsButton);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "IconCreationDialog";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Create Shortcuts";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.IconCreationDialog_Closing);
			this.Load += new System.EventHandler(this.IconCreationDialog_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		void IIconCreationDialog.ShowDialog()
		{
			ShowDialog();
		}

		void IconCreationDialog_Load(object sender, System.EventArgs e)
		{
			CreateShortcutsButton.Select();
		}

		void IconCreationDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			IconCreationOptions.AllUsers = AllUsersRadioButton.Checked;
			IconCreationOptions.Desktop = DesktopCheckBox.Checked;
			IconCreationOptions.Programs = StartMenuCheckBox.Checked;
			IconCreationOptions.InstanceDescription = instanceDescriptionTextBox.Text;
		}

		void DontCreateButton_Click(object sender, System.EventArgs e)
		{
			DesktopCheckBox.Checked = false;
			StartMenuCheckBox.Checked = false;
		}
	}
}
