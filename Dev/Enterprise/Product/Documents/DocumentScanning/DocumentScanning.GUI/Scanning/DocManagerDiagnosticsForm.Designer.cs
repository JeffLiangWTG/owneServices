using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentScanning.Launch
{
	public partial class DocManagerDiagnosticsForm : KForm
	{
		private CargoWise.Windows.UI.KButton CloseButton;
		private CargoWise.Windows.UI.KTextBox OutputTextBox;
		private CargoWise.Windows.UI.KButton PaperLoadedButton;
		private CargoWise.Windows.UI.KButton ScanButton;
		private CargoWise.Windows.UI.KLabel label1;
		private CargoWise.Windows.UI.KButton OpenDocMaintainButton;
		private CargoWise.Windows.UI.KTextBox JobNoTextBox;
		private CargoWise.Windows.UI.KLabel label2;
		private CargoWise.Windows.UI.KTextBox PassThroughParamsTextBox;
		private CargoWise.Windows.UI.KTextBox StandardParamsTextBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CloseButton = new CargoWise.Windows.UI.KButton();
			this.OutputTextBox = new CargoWise.Windows.UI.KTextBox();
			this.PaperLoadedButton = new CargoWise.Windows.UI.KButton();
			this.ScanButton = new CargoWise.Windows.UI.KButton();
			this.PassThroughParamsTextBox = new CargoWise.Windows.UI.KTextBox();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.OpenDocMaintainButton = new CargoWise.Windows.UI.KButton();
			this.JobNoTextBox = new CargoWise.Windows.UI.KTextBox();
			this.StandardParamsTextBox = new CargoWise.Windows.UI.KTextBox();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 416, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Text = "&Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.AcceptsReturn = true;
			this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.OutputTextBox.Multiline = true;
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 341, true);
			this.OutputTextBox.TabIndex = 0;
			// 
			// PaperLoadedButton
			// 
			this.PaperLoadedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PaperLoadedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 416, true);
			this.PaperLoadedButton.Name = "PaperLoadedButton";
			this.PaperLoadedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.PaperLoadedButton.TabIndex = 5;
			this.PaperLoadedButton.Text = "Paper Detected";
			this.PaperLoadedButton.Click += new System.EventHandler(this.PaperLoadedButton_Click);
			// 
			// ScanButton
			// 
			this.ScanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ScanButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 416, true);
			this.ScanButton.Name = "ScanButton";
			this.ScanButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.ScanButton.TabIndex = 6;
			this.ScanButton.Text = "Scan";
			this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
			// 
			// PassThroughParamsTextBox
			// 
			this.PassThroughParamsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.PassThroughParamsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 355, true);
			this.PassThroughParamsTextBox.Name = "PassThroughParamsTextBox";
			this.PassThroughParamsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 21, true);
			this.PassThroughParamsTextBox.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 357, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 16, true);
			this.label1.TabIndex = 1;
			this.label1.Text = "Pass through params:";
			// 
			// OpenDocMaintainButton
			// 
			this.OpenDocMaintainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OpenDocMaintainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 416, true);
			this.OpenDocMaintainButton.Name = "OpenDocMaintainButton";
			this.OpenDocMaintainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.OpenDocMaintainButton.TabIndex = 7;
			this.OpenDocMaintainButton.Text = "Open Job";
			this.OpenDocMaintainButton.Click += new System.EventHandler(this.OpenDocMaintainButton_Click);
			// 
			// JobNoTextBox
			// 
			this.JobNoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JobNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 416, true);
			this.JobNoTextBox.Name = "JobNoTextBox";
			this.JobNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.JobNoTextBox.TabIndex = 8;
			// 
			// StandardParamsTextBox
			// 
			this.StandardParamsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.StandardParamsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 380, true);
			this.StandardParamsTextBox.Name = "StandardParamsTextBox";
			this.StandardParamsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 21, true);
			this.StandardParamsTextBox.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 382, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 16, true);
			this.label2.TabIndex = 3;
			this.label2.Text = "Standard options:";
			// 
			// DocManagerDiagnosticsForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 454, true);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.StandardParamsTextBox);
			this.Controls.Add(this.JobNoTextBox);
			this.Controls.Add(this.PassThroughParamsTextBox);
			this.Controls.Add(this.OutputTextBox);
			this.Controls.Add(this.OpenDocMaintainButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ScanButton);
			this.Controls.Add(this.PaperLoadedButton);
			this.Controls.Add(this.CloseButton);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "DocManagerDiagnosticsForm";
			this.Text = "DocManager Diagnostics";
			this.Load += new System.EventHandler(this.StartForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
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
