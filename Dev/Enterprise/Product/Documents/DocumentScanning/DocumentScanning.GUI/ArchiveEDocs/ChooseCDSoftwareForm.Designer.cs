using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ChooseCDSoftwareForm : ZChildForm
	{
		private Enterprise.ZArchitecture.GUI.ZButton CancelCDButton;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.ZLabel FirstLabel;
		private Enterprise.ZArchitecture.ZLabel SecondLabel;
		private Enterprise.ZArchitecture.ZLabel ThirdLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private Enterprise.ZArchitecture.ZLabel FourthLabel;

		new void InitializeComponent()
		{
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FourthLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ThirdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SecondLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FirstLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelCDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 280, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 22, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(272);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(273);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.ChooseCDSoftwareManager);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.MainGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ChooseCDSoftwareForm|207d49d0-3946-4de8-a39b-e9d350d59974", "Copying eDocs to your Computer");
			this.MainGroupBox.Controls.Add(this.FourthLabel);
			this.MainGroupBox.Controls.Add(this.ThirdLabel);
			this.MainGroupBox.Controls.Add(this.SecondLabel);
			this.MainGroupBox.Controls.Add(this.FirstLabel);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 230, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			// 
			// FourthLabel
			// 
			this.FourthLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 156, true);
			this.FourthLabel.Name = "FourthLabel";
			this.FourthLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 37, true);
			this.FourthLabel.TabIndex = 3;
			// 
			// ThirdLabel
			// 
			this.ThirdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 97, true);
			this.ThirdLabel.Name = "ThirdLabel";
			this.ThirdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 52, true);
			this.ThirdLabel.TabIndex = 2;
			// 
			// SecondLabel
			// 
			this.SecondLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 59, true);
			this.SecondLabel.Name = "SecondLabel";
			this.SecondLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 30, true);
			this.SecondLabel.TabIndex = 1;
			// 
			// FirstLabel
			// 
			this.FirstLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 22, true);
			this.FirstLabel.Name = "FirstLabel";
			this.FirstLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 30, true);
			this.FirstLabel.TabIndex = 0;
			// 
			// CancelCDButton
			// 
			this.CancelCDButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelCDButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ChooseCDSoftwareForm|ec3f5764-d9c5-4863-be59-2e68da05bd99", "Cancel CD");
			this.CancelCDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 252, true);
			this.CancelCDButton.Name = "CancelCDButton";
			this.CancelCDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelCDButton.TabIndex = 2;
			this.CancelCDButton.Click += new System.EventHandler(this.CancelCDButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ChooseCDSoftwareForm|6898a121-0ed9-40de-a56d-3f1d27de5fe2", "Copy Files >");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 252, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ChooseCDSoftwareForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 302, true);
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ChooseCDSoftwareForm|f3593887-8f8a-4d64-9706-fabe2147f95a", "eDocs CD Creation");
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CancelCDButton);
			this.Controls.Add(this.MainGroupBox);
			this.DataSourceAssemblyName = "DocumentScanning";
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.ChooseCDSoftwareManager);
			this.DataSourceTypeName = "Enterprise.DocumentScanning.Business.ChooseCDSoftwareManager";
			this.Name = "ChooseCDSoftwareForm";
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			this.Controls.SetChildIndex(this.CancelCDButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
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
