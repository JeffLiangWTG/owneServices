namespace Enterprise.Startup
{
	public partial class UpgForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.TaskProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.TaskLabel = new System.Windows.Forms.Label();
			this.SubtaskLabel = new System.Windows.Forms.Label();
			this.SubtaskProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.CloseButton = new System.Windows.Forms.Button();
			this.CompletedTasksRichTextBox = new System.Windows.Forms.RichTextBox();
			this.WarningLabel = new System.Windows.Forms.Label();
			this.CopyLogsButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// TaskProgressBar
			//
			this.TaskProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TaskProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 96);
			this.TaskProgressBar.Name = "TaskProgressBar";
			this.TaskProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 15);
			this.TaskProgressBar.TabIndex = 0;
			//
			// TaskLabel
			//
			this.TaskLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TaskLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TaskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64);
			this.TaskLabel.Name = "TaskLabel";
			this.TaskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 28);
			this.TaskLabel.TabIndex = 1;
			this.TaskLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			// SubtaskLabel
			//
			this.SubtaskLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SubtaskLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SubtaskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 120);
			this.SubtaskLabel.Name = "SubtaskLabel";
			this.SubtaskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 24);
			this.SubtaskLabel.TabIndex = 3;
			this.SubtaskLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			// SubtaskProgressBar
			//
			this.SubtaskProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SubtaskProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 144);
			this.SubtaskProgressBar.Name = "SubtaskProgressBar";
			this.SubtaskProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 15);
			this.SubtaskProgressBar.TabIndex = 2;
			//
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CloseButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(780, 736);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.Text = "Cancel";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			//
			// CompletedTasksRichTextBox
			//
			this.CompletedTasksRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CompletedTasksRichTextBox.BackColor = System.Drawing.Color.White;
			this.CompletedTasksRichTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CompletedTasksRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176);
			this.CompletedTasksRichTextBox.Name = "CompletedTasksRichTextBox";
			this.CompletedTasksRichTextBox.ReadOnly = true;
			this.CompletedTasksRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
			this.CompletedTasksRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 554);
			this.CompletedTasksRichTextBox.TabIndex = 4;
			this.CompletedTasksRichTextBox.Text = "";
			this.CompletedTasksRichTextBox.WordWrap = false;
			//
			// WarningLabel
			//
			this.WarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.WarningLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.WarningLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WarningLabel.ForeColor = System.Drawing.Color.MediumBlue;
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 48);
			this.WarningLabel.TabIndex = 6;
			this.WarningLabel.Text = "UPGRADE IN PROGRESS\r\nThis can be a time-consuming operation.";
			this.WarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// CopyLogsButton
			//
			this.CopyLogsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CopyLogsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CopyLogsButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CopyLogsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 736);
			this.CopyLogsButton.Name = "CopyLogsButton";
			this.CopyLogsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23);
			this.CopyLogsButton.TabIndex = 7;
			this.CopyLogsButton.Text = "Copy Logs";
			this.CopyLogsButton.Click += new System.EventHandler(this.CopyLogsButton_Click);
			//
			// UpgForm
			//
			this.AcceptButton = this.CloseButton;
			this.AutoScaleBaseSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 13);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 766);
			this.ControlBox = false;
			this.Controls.Add(this.CopyLogsButton);
			this.Controls.Add(this.WarningLabel);
			this.Controls.Add(this.CompletedTasksRichTextBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SubtaskProgressBar);
			this.Controls.Add(this.TaskProgressBar);
			this.Controls.Add(this.SubtaskLabel);
			this.Controls.Add(this.TaskLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 400);
			this.Name = "UpgForm";
			this.Text = "Upgrade Database";
			this.Load += new System.EventHandler(this.UpgForm_Load);
			this.ResumeLayout(false);
		}
		#endregion

		CargoWise.Windows.UI.KProgressBar TaskProgressBar;
		System.Windows.Forms.Label TaskLabel;
		System.Windows.Forms.Label SubtaskLabel;
		CargoWise.Windows.UI.KProgressBar SubtaskProgressBar;
		internal System.Windows.Forms.Button CloseButton;
		System.Windows.Forms.RichTextBox CompletedTasksRichTextBox;
		System.Windows.Forms.Label WarningLabel;
		System.Windows.Forms.Button CopyLogsButton;
	}
}
