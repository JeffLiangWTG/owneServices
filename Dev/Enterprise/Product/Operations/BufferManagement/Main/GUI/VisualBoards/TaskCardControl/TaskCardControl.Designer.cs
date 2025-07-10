namespace Enterprise.BufferManagement.GUI
{
	partial class TaskCardControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.NoteTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NoteTextPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NoteTextPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// NoteTextLabel
			// 
			this.NoteTextLabel.AutoSize = true;
			this.NoteTextLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.NoteTextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NoteTextLabel.ForeColor = System.Drawing.Color.White;
			this.NoteTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NoteTextLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.NoteTextLabel.Name = "NoteTextLabel";
			this.NoteTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.NoteTextLabel.Font = new System.Drawing.Font(NoteTextLabel.Font.FontFamily, 8f);
			this.NoteTextLabel.TabIndex = 5;
			this.NoteTextLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.NoteTextLabel.UseMnemonic = false;
			// 
			// NoteTextPanel
			// 
			this.NoteTextPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.NoteTextPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.NoteTextPanel.Controls.Add(this.NoteTextLabel);
			this.NoteTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 47, true);
			this.NoteTextPanel.Name = "NoteTextPanel";
			this.NoteTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 16, true);
			this.NoteTextPanel.TabIndex = 6; 
			// 
			// TaskCardControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.NoteTextPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "TaskCardControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 63, true);
			this.DoubleClick += new System.EventHandler(this.TaskCardControl_DoubleClick);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NoteTextPanel.ResumeLayout(false);
			this.NoteTextPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		public ZArchitecture.ZLabel NoteTextLabel;
		public ZArchitecture.GUI.ZPanel NoteTextPanel;
		private System.ComponentModel.IContainer components;
	}
}
