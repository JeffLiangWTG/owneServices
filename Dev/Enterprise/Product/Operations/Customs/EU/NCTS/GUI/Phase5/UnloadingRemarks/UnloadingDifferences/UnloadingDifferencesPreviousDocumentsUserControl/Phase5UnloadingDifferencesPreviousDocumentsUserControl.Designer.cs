namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5UnloadingDifferencesPreviousDocumentsUserControl
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
			this.PreviousDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// PreviousDocumentGroupBox
			// 
			this.PreviousDocumentGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("c8d8a987-12d0-454f-be94-04637c72dfc9", "Previous Documents");
			this.PreviousDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentGroupBox.Name = "PreviousDocumentGroupBox";
			this.PreviousDocumentGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 5, 0, 0, true);
			this.PreviousDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 135, true);
			this.PreviousDocumentGroupBox.TabIndex = 0;
			this.PreviousDocumentGroupBox.TabStop = false;
			// 
			// Phase5UnloadingDifferencesPreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("aaad27e8-a263-4781-ab76-7cc0b764f549", "Previous Documents");
			this.Controls.Add(this.PreviousDocumentGroupBox);
			this.Name = "Phase5UnloadingDifferencesPreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox PreviousDocumentGroupBox;
	}
}
