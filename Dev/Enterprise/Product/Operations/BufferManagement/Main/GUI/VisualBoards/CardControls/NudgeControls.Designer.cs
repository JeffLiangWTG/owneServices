namespace Enterprise.BufferManagement.GUI
{
	partial class NudgeControls
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LabelVoteDown = new CargoWise.Windows.UI.KLabel();
			this.LabelVoteUp = new CargoWise.Windows.UI.KLabel();
			this.NudgeAmount = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// LabelVoteDown
			// 
			this.LabelVoteDown.AutoSize = true;
			this.LabelVoteDown.BackColor = System.Drawing.Color.Transparent;
			this.LabelVoteDown.Font = new System.Drawing.Font("Wingdings", 16F);
			this.LabelVoteDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, -3, true);
			this.LabelVoteDown.Name = "LabelVoteDown";
			this.LabelVoteDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 25, true);
			this.LabelVoteDown.TabIndex = 38;
			this.LabelVoteDown.Text = "D";
			this.LabelVoteDown.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LabelVoteDown_MouseDown);
			// 
			// LabelVoteUp
			// 
			this.LabelVoteUp.AutoSize = true;
			this.LabelVoteUp.BackColor = System.Drawing.Color.Transparent;
			this.LabelVoteUp.Font = new System.Drawing.Font("Wingdings", 16F);
			this.LabelVoteUp.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.LabelVoteUp.Name = "LabelVoteUp";
			this.LabelVoteUp.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 25, true);
			this.LabelVoteUp.TabIndex = 37;
			this.LabelVoteUp.Text = "";
			this.LabelVoteUp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LabelVoteUp_MouseDown);
			// 
			// TextBoxNudgeAmount
			// 
			this.NudgeAmount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(252)))), ((int)(((byte)(215)))));
			this.NudgeAmount.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.NudgeAmount.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NudgeAmount, false);
			this.NudgeAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 4, true);
			this.NudgeAmount.Name = "TextBoxNudgeAmount";
			this.NudgeAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 13, true);
			this.NudgeAmount.TabIndex = 39;
			// 
			// NudgeControls
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LabelVoteDown);
			this.Controls.Add(this.LabelVoteUp);
			this.Controls.Add(this.NudgeAmount);
			this.Name = "NudgeControls";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public CargoWise.Windows.UI.KLabel LabelVoteDown;
		public CargoWise.Windows.UI.KLabel LabelVoteUp;
		public ZArchitecture.ZCalcEdit NudgeAmount;
	}
}
