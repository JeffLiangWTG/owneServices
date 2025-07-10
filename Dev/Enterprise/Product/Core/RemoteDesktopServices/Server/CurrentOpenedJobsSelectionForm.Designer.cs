using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.RemoteDesktopServices.Server
{
	partial class CurrentOpenedJobsSelectionForm
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
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.startDropHandlerBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.startDropHandlerBindingSource)).BeginInit();
			this.SuspendLayout();

			this.startDropHandlerBindingSource.DataSource = OpenedJobs;
			// 
			// comboBox1
			//
			this.comboBox1.DataSource = this.startDropHandlerBindingSource.DataSource;
			this.comboBox1.DisplayMember = "Text";
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = ControlDpiScalingHelper.NewScaledPoint(36, 101, true);
			this.comboBox1.Margin = ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = ControlDpiScalingHelper.NewScaledSize(386, 24, true);
			this.comboBox1.TabIndex = 0;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.AutoSize = false;
			this.DescriptionLabel.Location = ControlDpiScalingHelper.NewScaledPoint(33, 42, true);
			this.DescriptionLabel.Margin = ControlDpiScalingHelper.NewScaledPadding(4, 0, 4, 0, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = ControlDpiScalingHelper.NewScaledSize(392, 35, true);
			this.DescriptionLabel.TabIndex = 1;
			this.DescriptionLabel.Text = Res.GetString("E7BDC318-4DD0-42C6-BD2E-E06483172FB0", "Failed to find out the form you dropped to. Please select from opened jobs.");
			// 
			// okButton
			// 
			this.okButton.Location = ControlDpiScalingHelper.NewScaledPoint(349, 140, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// CurrentOpenedJobsSelectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(450, 225, true);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.comboBox1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.Name = "CurrentOpenedJobsSelectionForm";
			this.Text = Res.GetString("4653391B-C6CC-4AE7-8FEC-524A27E1E44C", "Select From Opened Jobs");
			((System.ComponentModel.ISupportInitialize)(this.startDropHandlerBindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label DescriptionLabel;
		private System.Windows.Forms.BindingSource startDropHandlerBindingSource;
		internal System.Windows.Forms.Button okButton;
	}
}
