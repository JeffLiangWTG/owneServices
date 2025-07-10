using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.GUI
{
	partial class AUImportTCOBulkChangeForm
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
		protected override void InitializeComponent()
		{
			this.CompletedLabel = new CargoWise.Windows.UI.KLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 346, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(541);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange);
			// 
			// CompletedLabel
			// 
			this.CompletedLabel.AutoSize = true;
			this.CompletedLabel.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CompletedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 63, true);
			this.CompletedLabel.Name = "CompletedLabel";
			this.CompletedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 17, true);
			this.CompletedLabel.TabIndex = 40;
			this.CompletedLabel.Text = "Bulk TCO Changes are Ready to be Saved.";
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = null;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 174, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 44;
			this.CloseButton.Text = "Cancel";
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.CaptionResourceString = null;
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 174, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 23, true);
			this.PostButton.TabIndex = 43;
			this.PostButton.Text = "Save";
			// 
			// AUImportTCOBulkChangeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 370, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.PostButton);
			this.Controls.Add(this.CompletedLabel);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.AUImportTariffBulkChange";
			this.Name = "AUImportTCOBulkChangeForm";
			this.Text = "Tariff Bulk Change";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CompletedLabel, 0);
			this.Controls.SetChildIndex(this.PostButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KLabel CompletedLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton PostButton;
	}
}

