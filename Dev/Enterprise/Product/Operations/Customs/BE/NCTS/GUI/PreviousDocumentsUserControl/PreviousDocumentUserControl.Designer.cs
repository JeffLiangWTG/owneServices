using Enterprise.Customs.BE.NCTS.GUI;

namespace Enterprise.Customs.BE.NCTS
{
	partial class PreviousDocumentUserControl
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
			this.ReferenceNumberN785UserControl = new PreviousDocumentN785ReferenceUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumberN785UserControl.SuspendLayout();
			this.SuspendLayout();
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument);
			// 
			// ReferenceNumberN785UserControl
			// 
			this.ReferenceNumberN785UserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceNumberN785UserControl, ".");
			this.ReferenceNumberN785UserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 252, true);
			this.ReferenceNumberN785UserControl.Name = "ReferenceNumberN785UserControl";
			this.ReferenceNumberN785UserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 22, true);
			this.ReferenceNumberN785UserControl.TabIndex = 1;
			// 
			// PreviousDocumentUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumberN785UserControl);
			this.Name = "PreviousDocumentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumberN785UserControl.ResumeLayout(true);
			this.ReferenceNumberN785UserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public PreviousDocumentN785ReferenceUserControl ReferenceNumberN785UserControl;
		#endregion
	}
}
