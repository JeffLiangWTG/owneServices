using System.Windows.Forms;

namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class GoodsItemDetailsUserControl
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
			this.CustomsStatusUserControl = new Enterprise.Customs.IT.NCTS.GUI.GoodsItemCustomsStatusUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsStatusUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// CustomsStatusUserControl
			// 
			this.CustomsStatusUserControl.AllowDrop = true;
			this.CustomsStatusUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 56, true);
			this.CustomsStatusUserControl.Name = "CustomsStatusUserControl";
			this.CustomsStatusUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 22, true);
			this.CustomsStatusUserControl.TabIndex = 2;
			// 
			// GoodsItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsStatusUserControl);
			this.Name = "GoodsItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsStatusUserControl.ResumeLayout(true);
			this.CustomsStatusUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.Customs.IT.NCTS.GUI.GoodsItemCustomsStatusUserControl CustomsStatusUserControl;
	}
}
