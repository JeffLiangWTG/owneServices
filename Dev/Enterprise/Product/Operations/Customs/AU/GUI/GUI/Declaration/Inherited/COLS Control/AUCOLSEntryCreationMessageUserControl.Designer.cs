using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSEntryCreationMessageUserControl
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
			this.MessagingLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessagingLabel
			// 
			this.MessagingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MessagingLabel.IsFontBold = true;
			this.MessagingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagingLabel.Name = "MessagingLabel";
			this.MessagingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 481, true);
			this.MessagingLabel.TabIndex = 0;
			this.MessagingLabel.Text = "A COLS Entry could not be found.\r\nPlease change to another tab, then click back t" +
    "o this tab to create a COLS Entry.";
			this.MessagingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.MessagingLabel.UseMnemonic = false;
			// 
			// AUCOLSEntryCreationMessageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MessagingLabel);
			this.Name = "AUCOLSEntryCreationMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 481, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZLabel MessagingLabel;

		#endregion
	}
}
