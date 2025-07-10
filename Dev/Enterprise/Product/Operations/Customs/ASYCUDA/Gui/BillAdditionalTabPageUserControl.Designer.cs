namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class BillAdditionalTabPageUserControl
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
			this.Panel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Panel
			// 
			this.Panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0); //new System.Drawing.Point(0, 0);
			this.Panel.Name = "Panel";
			this.Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 151); //new System.Drawing.Size(626, 151);
			this.Panel.TabIndex = 0;
			// 
			// BillAdditionalTabPageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.Panel);
			this.Name = "BillAdditionalTabPageUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel Panel;
	}
}
