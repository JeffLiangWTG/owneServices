using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.OCR
{
	public partial class OCRResultGridForm : ZChildForm
	{
		ZGrid OCRResultBoundGrid;
		Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		Enterprise.ZArchitecture.GUI.ZButton ACancelButton;

		new void InitializeComponent()
		{
			this.OCRResultBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ACancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OCRResultBoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 280, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(312);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(313);
			// 
			// OCRResultBoundGrid
			// 
			this.OCRResultBoundGrid.AllowNavigation = false;
			this.OCRResultBoundGrid.AllowSorting = false;
			this.OCRResultBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OCRResultBoundGrid.CaptionVisible = false;
			this.OCRResultBoundGrid.GridId = "6707338b-8bd5-4188-aafe-ba2fc567e229";
			this.OCRResultBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OCRResultBoundGrid.LayoutKey = "OCRResultBoundGrid";
			this.OCRResultBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.OCRResultBoundGrid.Name = "OCRResultBoundGrid";
			this.OCRResultBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 242, true);
			this.OCRResultBoundGrid.TabIndex = 1;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OCRResultGridForm|92e6cf00-009b-4c7f-a5d2-2200e070d41e", "&Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 252, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// ACancelButton
			// 
			this.ACancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ACancelButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OCRResultGridForm|563753f4-6fe2-4134-8746-27c76359dc75", "&Cancel");
			this.ACancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 252, true);
			this.ACancelButton.Name = "ACancelButton";
			this.ACancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ACancelButton.TabIndex = 3;
			this.ACancelButton.Click += new System.EventHandler(this.ACancelButton_Click);
			// 
			// OCRResultGridForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 302, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 150, true);
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OCRResultGridForm|a7270a7e-5fa8-46a3-bcf6-b478e25aeaf9", "Converted Text");
			this.Controls.Add(this.ACancelButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.OCRResultBoundGrid);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "OCRResultGridForm";
			this.Controls.SetChildIndex(this.OCRResultBoundGrid, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.ACancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OCRResultBoundGrid)).EndInit();
			this.ResumeLayout(false);
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
