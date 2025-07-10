using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.WCB.GUI
{
	public partial class WCBDataImporterForm : DataImporterForm
	{
		Enterprise.ZArchitecture.GUI.ZRadioButton FreightlinerFormatRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton DaimlerFormatRadioButton;

		new void InitializeComponent()
		{
			this.DaimlerFormatRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.FreightlinerFormatRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 496, true);
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 529, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 24, true);
			// 
			// DaimlerFormatRadioButton
			// 
			this.DaimlerFormatRadioButton.AutoCheck = false;
			this.DaimlerFormatRadioButton.AutoSize = true;
			this.DaimlerFormatRadioButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("WCBDataImporterForm|8e9930b1-9fd2-474e-96e8-a3a704128024", "Daimler Invoice");
			this.DaimlerFormatRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DaimlerFormatRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 12, true);
			this.DaimlerFormatRadioButton.Name = "DaimlerFormatRadioButton";
			this.DaimlerFormatRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.DaimlerFormatRadioButton.TabIndex = 1;
			// 
			// FreightlinerFormatRadioButton
			// 
			this.FreightlinerFormatRadioButton.AutoCheck = false;
			this.FreightlinerFormatRadioButton.AutoSize = true;
			this.FreightlinerFormatRadioButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("WCBDataImporterForm|99f54b82-0c7e-4b39-8c3e-0d6ae9c91e1d", "Freightliner Invoice");
			this.FreightlinerFormatRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FreightlinerFormatRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 12, true);
			this.FreightlinerFormatRadioButton.Name = "FreightlinerFormatRadioButton";
			this.FreightlinerFormatRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.FreightlinerFormatRadioButton.TabIndex = 2;
			//
			// ProgressTextBox
			//

			// 
			// WCBDataImporterForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Controls.Add(this.DaimlerFormatRadioButton);
			this.Controls.Add(this.FreightlinerFormatRadioButton);
			this.Name = "WCBDataImporterForm";
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.FreightlinerFormatRadioButton, 0);
			this.Controls.SetChildIndex(this.DaimlerFormatRadioButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
