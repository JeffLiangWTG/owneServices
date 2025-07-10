
namespace Enterprise.Customs.KR.GUI
{
	partial class LocalExportInvoiceLineDetailsGroupBoxUserControl
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
            this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LocalExportDetailsUserControl = new Enterprise.Customs.KR.GUI.LocalExportDetailsUserControl();
            this.QuantityAndWeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LocalExportQuantityAndWeightUserControl = new Enterprise.Customs.KR.GUI.LocalExportQuantityAndWeightUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DetailsGroupBox.SuspendLayout();
            this.LocalExportDetailsUserControl.SuspendLayout();
            this.QuantityAndWeightGroupBox.SuspendLayout();
            this.LocalExportQuantityAndWeightUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
            // 
            // DetailsGroupBox
            // 
            this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("6DD064AC-08E4-4E3F-A392-46189E131576", "Details");
            this.DetailsGroupBox.Controls.Add(this.LocalExportDetailsUserControl);
            this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DetailsGroupBox.Name = "DetailsGroupBox";
            this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 181, true);
            this.DetailsGroupBox.TabIndex = 0;
            this.DetailsGroupBox.TabStop = false;
            // 
            // LocalExportDetailsUserControl
            // 
            this.LocalExportDetailsUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LocalExportDetailsUserControl, ".");
            this.LocalExportDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocalExportDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
            this.LocalExportDetailsUserControl.Name = "LocalExportDetailsUserControl";
			this.LocalExportDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 163, true);
            this.LocalExportDetailsUserControl.TabIndex = 0;
            // 
            // QuantityAndWeightGroupBox
            // 
            this.QuantityAndWeightGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("59680B32-42AD-42CA-B57F-7D25A4D5C087", "Quantity and Weight");
            this.QuantityAndWeightGroupBox.Controls.Add(this.LocalExportQuantityAndWeightUserControl);
            this.QuantityAndWeightGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.QuantityAndWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
            this.QuantityAndWeightGroupBox.Name = "QuantityAndWeightGroupBox";
            this.QuantityAndWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 111, true);
            this.QuantityAndWeightGroupBox.TabIndex = 1;
            this.QuantityAndWeightGroupBox.TabStop = false;
            // 
            // LocalExportQuantityAndWeightUserControl
            // 
            this.LocalExportQuantityAndWeightUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LocalExportQuantityAndWeightUserControl, ".");
            this.LocalExportQuantityAndWeightUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocalExportQuantityAndWeightUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
            this.LocalExportQuantityAndWeightUserControl.Name = "LocalExportQuantityAndWeightUserControl";
            this.LocalExportQuantityAndWeightUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 94, true);
            this.LocalExportQuantityAndWeightUserControl.TabIndex = 0;
            // 
            // LocalExportInvoiceLineDetailsGroupBoxUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.QuantityAndWeightGroupBox);
            this.Controls.Add(this.DetailsGroupBox);
            this.Name = "LocalExportInvoiceLineDetailsGroupBoxUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 296, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DetailsGroupBox.ResumeLayout(false);
            this.DetailsGroupBox.PerformLayout();
            this.LocalExportDetailsUserControl.ResumeLayout(true);
            this.LocalExportDetailsUserControl.PerformLayout();
            this.QuantityAndWeightGroupBox.ResumeLayout(false);
            this.QuantityAndWeightGroupBox.PerformLayout();
            this.LocalExportQuantityAndWeightUserControl.ResumeLayout(true);
            this.LocalExportQuantityAndWeightUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox QuantityAndWeightGroupBox;
		private KR.GUI.LocalExportDetailsUserControl LocalExportDetailsUserControl;
		private KR.GUI.LocalExportQuantityAndWeightUserControl LocalExportQuantityAndWeightUserControl;
	}
}
