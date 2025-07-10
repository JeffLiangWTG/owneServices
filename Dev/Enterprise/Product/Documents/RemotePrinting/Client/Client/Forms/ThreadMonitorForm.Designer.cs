namespace Enterprise.RemotePrinting.Client
{
	partial class ThreadMonitorForm
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
		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule", Justification = "Embedding a print icon, not a product icon")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ThreadMonitorForm));
			this.ThreadGroupBox = new System.Windows.Forms.GroupBox();
			this.ThreadLabel = new System.Windows.Forms.Label();
			this.ThreadComboBox = new System.Windows.Forms.ComboBox();
			this.AutomaticTrackingGroupBox = new System.Windows.Forms.GroupBox();
			this.IntervalNumber = new System.Windows.Forms.NumericUpDown();
			this.StartButton = new System.Windows.Forms.Button();
			this.IntervalTypeComboBox = new System.Windows.Forms.ComboBox();
			this.IntervalTypeLabel = new System.Windows.Forms.Label();
			this.IntervalNumberLabel = new System.Windows.Forms.Label();
			this.ManualTrackingGroupBox = new System.Windows.Forms.GroupBox();
			this.ClearButton = new System.Windows.Forms.Button();
			this.CopyAllButton = new System.Windows.Forms.Button();
			this.CollectThreadSnapshotButton = new System.Windows.Forms.Button();
			this.TimesListBox = new System.Windows.Forms.ListBox();
			this.StackTraceTextBox = new System.Windows.Forms.TextBox();
			this.CurrentTimer = new System.Windows.Forms.Timer(this.components);
			this.ThreadGroupBox.SuspendLayout();
			this.AutomaticTrackingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IntervalNumber)).BeginInit();
			this.ManualTrackingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ThreadGroupBox
			// 
			this.ThreadGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ThreadGroupBox.AutoSize = true;
			this.ThreadGroupBox.Controls.Add(this.ThreadLabel);
			this.ThreadGroupBox.Controls.Add(this.ThreadComboBox);
			this.ThreadGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ThreadGroupBox.Location = new System.Drawing.Point(21, 8);
			this.ThreadGroupBox.Margin = new System.Windows.Forms.Padding(5);
			this.ThreadGroupBox.Name = "ThreadGroupBox";
			this.ThreadGroupBox.Padding = new System.Windows.Forms.Padding(5);
			this.ThreadGroupBox.Size = new System.Drawing.Size(1803, 136);
			this.ThreadGroupBox.TabIndex = 0;
			this.ThreadGroupBox.TabStop = false;
			this.ThreadGroupBox.Text = "Thread to Track";
			// 
			// ThreadLabel
			// 
			this.ThreadLabel.AutoSize = true;
			this.ThreadLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ThreadLabel.Location = new System.Drawing.Point(162, 59);
			this.ThreadLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.ThreadLabel.Name = "ThreadLabel";
			this.ThreadLabel.Size = new System.Drawing.Size(104, 32);
			this.ThreadLabel.TabIndex = 1;
			this.ThreadLabel.Text = "Thread";
			// 
			// ThreadComboBox
			// 
			this.ThreadComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ThreadComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.ThreadComboBox.FormattingEnabled = true;
			this.ThreadComboBox.Location = new System.Drawing.Point(286, 54);
			this.ThreadComboBox.Margin = new System.Windows.Forms.Padding(5);
			this.ThreadComboBox.Name = "ThreadComboBox";
			this.ThreadComboBox.Size = new System.Drawing.Size(347, 39);
			this.ThreadComboBox.TabIndex = 0;
			// 
			// AutomaticTrackingGroupBox
			// 
			this.AutomaticTrackingGroupBox.Controls.Add(this.IntervalNumber);
			this.AutomaticTrackingGroupBox.Controls.Add(this.StartButton);
			this.AutomaticTrackingGroupBox.Controls.Add(this.IntervalTypeComboBox);
			this.AutomaticTrackingGroupBox.Controls.Add(this.IntervalTypeLabel);
			this.AutomaticTrackingGroupBox.Controls.Add(this.IntervalNumberLabel);
			this.AutomaticTrackingGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AutomaticTrackingGroupBox.Location = new System.Drawing.Point(21, 167);
			this.AutomaticTrackingGroupBox.Margin = new System.Windows.Forms.Padding(5);
			this.AutomaticTrackingGroupBox.Name = "AutomaticTrackingGroupBox";
			this.AutomaticTrackingGroupBox.Padding = new System.Windows.Forms.Padding(5);
			this.AutomaticTrackingGroupBox.Size = new System.Drawing.Size(875, 186);
			this.AutomaticTrackingGroupBox.TabIndex = 1;
			this.AutomaticTrackingGroupBox.TabStop = false;
			this.AutomaticTrackingGroupBox.Text = "Automatic Tracking";
			// 
			// IntervalNumber
			// 
			this.IntervalNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.IntervalNumber.Location = new System.Drawing.Point(286, 45);
			this.IntervalNumber.Margin = new System.Windows.Forms.Padding(5);
			this.IntervalNumber.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.IntervalNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.IntervalNumber.Name = "IntervalNumber";
			this.IntervalNumber.Size = new System.Drawing.Size(352, 39);
			this.IntervalNumber.TabIndex = 1;
			this.IntervalNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// StartButton
			// 
			this.StartButton.BackColor = System.Drawing.Color.DarkSeaGreen;
			this.StartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StartButton.Location = new System.Drawing.Point(658, 37);
			this.StartButton.Margin = new System.Windows.Forms.Padding(5);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(190, 122);
			this.StartButton.TabIndex = 3;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = false;
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// IntervalTypeComboBox
			// 
			this.IntervalTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.IntervalTypeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.IntervalTypeComboBox.FormattingEnabled = true;
			this.IntervalTypeComboBox.Location = new System.Drawing.Point(286, 104);
			this.IntervalTypeComboBox.Margin = new System.Windows.Forms.Padding(5);
			this.IntervalTypeComboBox.Name = "IntervalTypeComboBox";
			this.IntervalTypeComboBox.Size = new System.Drawing.Size(347, 39);
			this.IntervalTypeComboBox.TabIndex = 2;
			// 
			// IntervalTypeLabel
			// 
			this.IntervalTypeLabel.AutoSize = true;
			this.IntervalTypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IntervalTypeLabel.Location = new System.Drawing.Point(84, 110);
			this.IntervalTypeLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.IntervalTypeLabel.Name = "IntervalTypeLabel";
			this.IntervalTypeLabel.Size = new System.Drawing.Size(177, 32);
			this.IntervalTypeLabel.TabIndex = 2;
			this.IntervalTypeLabel.Text = "Interval Type";
			// 
			// IntervalNumberLabel
			// 
			this.IntervalNumberLabel.AutoSize = true;
			this.IntervalNumberLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IntervalNumberLabel.Location = new System.Drawing.Point(41, 48);
			this.IntervalNumberLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.IntervalNumberLabel.Name = "IntervalNumberLabel";
			this.IntervalNumberLabel.Size = new System.Drawing.Size(214, 32);
			this.IntervalNumberLabel.TabIndex = 1;
			this.IntervalNumberLabel.Text = "Interval Number";
			// 
			// ManualTrackingGroupBox
			// 
			this.ManualTrackingGroupBox.Controls.Add(this.ClearButton);
			this.ManualTrackingGroupBox.Controls.Add(this.CopyAllButton);
			this.ManualTrackingGroupBox.Controls.Add(this.CollectThreadSnapshotButton);
			this.ManualTrackingGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ManualTrackingGroupBox.Location = new System.Drawing.Point(915, 167);
			this.ManualTrackingGroupBox.Margin = new System.Windows.Forms.Padding(5);
			this.ManualTrackingGroupBox.Name = "ManualTrackingGroupBox";
			this.ManualTrackingGroupBox.Padding = new System.Windows.Forms.Padding(5);
			this.ManualTrackingGroupBox.Size = new System.Drawing.Size(909, 186);
			this.ManualTrackingGroupBox.TabIndex = 2;
			this.ManualTrackingGroupBox.TabStop = false;
			this.ManualTrackingGroupBox.Text = "Manual Tracking";
			// 
			// ClearButton
			// 
			this.ClearButton.BackColor = System.Drawing.Color.LightCoral;
			this.ClearButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClearButton.Location = new System.Drawing.Point(682, 37);
			this.ClearButton.Margin = new System.Windows.Forms.Padding(5);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(190, 122);
			this.ClearButton.TabIndex = 6;
			this.ClearButton.Text = "Clear";
			this.ClearButton.UseVisualStyleBackColor = false;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// CopyAllButton
			// 
			this.CopyAllButton.BackColor = System.Drawing.Color.White;
			this.CopyAllButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CopyAllButton.Location = new System.Drawing.Point(460, 37);
			this.CopyAllButton.Margin = new System.Windows.Forms.Padding(5);
			this.CopyAllButton.Name = "CopyAllButton";
			this.CopyAllButton.Size = new System.Drawing.Size(190, 122);
			this.CopyAllButton.TabIndex = 5;
			this.CopyAllButton.Text = "Copy All";
			this.CopyAllButton.UseVisualStyleBackColor = false;
			this.CopyAllButton.Click += new System.EventHandler(this.CopyAllButton_Click);
			// 
			// CollectThreadSnapshotButton
			// 
			this.CollectThreadSnapshotButton.BackColor = System.Drawing.Color.SkyBlue;
			this.CollectThreadSnapshotButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CollectThreadSnapshotButton.Location = new System.Drawing.Point(37, 37);
			this.CollectThreadSnapshotButton.Margin = new System.Windows.Forms.Padding(5);
			this.CollectThreadSnapshotButton.Name = "CollectThreadSnapshotButton";
			this.CollectThreadSnapshotButton.Size = new System.Drawing.Size(390, 122);
			this.CollectThreadSnapshotButton.TabIndex = 4;
			this.CollectThreadSnapshotButton.Text = "Collect Thread Snapshot";
			this.CollectThreadSnapshotButton.UseVisualStyleBackColor = false;
			this.CollectThreadSnapshotButton.Click += new System.EventHandler(this.CollectThreadSnapshotButton_Click);
			// 
			// TimesListBox
			// 
			this.TimesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.TimesListBox.FormattingEnabled = true;
			this.TimesListBox.HorizontalScrollbar = true;
			this.TimesListBox.ItemHeight = 31;
			this.TimesListBox.Location = new System.Drawing.Point(21, 370);
			this.TimesListBox.Margin = new System.Windows.Forms.Padding(5);
			this.TimesListBox.Name = "TimesListBox";
			this.TimesListBox.Size = new System.Drawing.Size(581, 748);
			this.TimesListBox.TabIndex = 3;
			this.TimesListBox.SelectedIndexChanged += new System.EventHandler(this.TimesListBox_SelectedIndexChanged);
			// 
			// StackTraceTextBox
			// 
			this.StackTraceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StackTraceTextBox.Location = new System.Drawing.Point(612, 370);
			this.StackTraceTextBox.Margin = new System.Windows.Forms.Padding(5);
			this.StackTraceTextBox.Multiline = true;
			this.StackTraceTextBox.Name = "StackTraceTextBox";
			this.StackTraceTextBox.ReadOnly = true;
			this.StackTraceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.StackTraceTextBox.Size = new System.Drawing.Size(1209, 748);
			this.StackTraceTextBox.TabIndex = 4;
			// 
			// ThreadMonitorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1845, 1165);
			this.Controls.Add(this.StackTraceTextBox);
			this.Controls.Add(this.TimesListBox);
			this.Controls.Add(this.ManualTrackingGroupBox);
			this.Controls.Add(this.AutomaticTrackingGroupBox);
			this.Controls.Add(this.ThreadGroupBox);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(5);
			this.MinimumSize = new System.Drawing.Size(1845, 1165);
			this.Name = "ThreadMonitorForm";
			this.Text = "Thread Monitor (Hang Debug)";
			this.ThreadGroupBox.ResumeLayout(false);
			this.ThreadGroupBox.PerformLayout();
			this.AutomaticTrackingGroupBox.ResumeLayout(false);
			this.AutomaticTrackingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IntervalNumber)).EndInit();
			this.ManualTrackingGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox ThreadGroupBox;
		private System.Windows.Forms.Label ThreadLabel;
		protected System.Windows.Forms.ComboBox ThreadComboBox;
		private System.Windows.Forms.GroupBox AutomaticTrackingGroupBox;
		private System.Windows.Forms.Label IntervalNumberLabel;
		protected System.Windows.Forms.NumericUpDown IntervalNumber;
		private System.Windows.Forms.Label IntervalTypeLabel;
		protected System.Windows.Forms.ComboBox IntervalTypeComboBox;
		protected System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.GroupBox ManualTrackingGroupBox;
		protected System.Windows.Forms.Button CollectThreadSnapshotButton;
		protected System.Windows.Forms.Button CopyAllButton;
		protected System.Windows.Forms.Button ClearButton;
		protected System.Windows.Forms.ListBox TimesListBox;
		protected System.Windows.Forms.TextBox StackTraceTextBox;
		protected System.Windows.Forms.Timer CurrentTimer;
	}
}
