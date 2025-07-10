
namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class SecurityTabUserControl
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
			this.SecurityDetailsGroupBox.SuspendLayout();
			this.PlaceOfUnloadingFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PlaceOfUnloadingFindBox
			// 
			this.PlaceOfUnloadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 70, true);
			this.PlaceOfUnloadingFindBox.ShowDescriptionBox = false;
			this.PlaceOfUnloadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			// 
			// SecurityTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "SecurityTabUserControl";
			this.SecurityDetailsGroupBox.ResumeLayout(false);
			this.SecurityDetailsGroupBox.PerformLayout();
			this.PlaceOfUnloadingFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
