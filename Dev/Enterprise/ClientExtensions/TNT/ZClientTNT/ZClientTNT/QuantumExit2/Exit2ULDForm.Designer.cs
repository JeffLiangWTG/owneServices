using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT
{
	public partial class Exit2ULDForm : ZChildForm
	{
		private CargoWise.Windows.UI.KTextBox ConsolTextBox;
		private CargoWise.Windows.UI.KTextBox MawbTextBox;
		private CargoWise.Windows.UI.KTextBox MbagTextBox;
		private CargoWise.Windows.UI.KTextBox ULDTextBox;
		private CargoWise.Windows.UI.KLabel label1;
		private CargoWise.Windows.UI.KLabel label2;
		private CargoWise.Windows.UI.KLabel label3;
		private CargoWise.Windows.UI.KLabel label4;
		private CargoWise.Windows.UI.KButton OKButton;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.ConsolTextBox = new CargoWise.Windows.UI.KTextBox();
			this.MawbTextBox = new CargoWise.Windows.UI.KTextBox();
			this.MbagTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ULDTextBox = new CargoWise.Windows.UI.KTextBox();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.label3 = new CargoWise.Windows.UI.KLabel();
			this.label4 = new CargoWise.Windows.UI.KLabel();
			this.OKButton = new CargoWise.Windows.UI.KButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 88, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 10, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(288);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(288);
			// 
			// ConsolTextBox
			// 
			this.ConsolTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 40, true);
			this.ConsolTextBox.Name = "ConsolTextBox";
			this.ConsolTextBox.ReadOnly = true;
			this.ConsolTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ConsolTextBox.TabIndex = 1;
			// 
			// MawbTextBox
			// 
			this.MawbTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 40, true);
			this.MawbTextBox.Name = "MawbTextBox";
			this.MawbTextBox.ReadOnly = true;
			this.MawbTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.MawbTextBox.TabIndex = 3;
			// 
			// MbagTextBox
			// 
			this.MbagTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 40, true);
			this.MbagTextBox.Name = "MbagTextBox";
			this.MbagTextBox.ReadOnly = true;
			this.MbagTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.MbagTextBox.TabIndex = 5;
			// 
			// ULDTextBox
			// 
			this.ULDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ULDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 40, true);
			this.ULDTextBox.Name = "ULDTextBox";
			this.ULDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.ULDTextBox.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 11, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.label1.TabIndex = 0;
			this.label1.Text = "Consol";
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 11, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.label2.TabIndex = 2;
			this.label2.Text = "MAWB";
			// 
			// label3
			// 
			this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 11, true);
			this.label3.Name = "label3";
			this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.label3.TabIndex = 4;
			this.label3.Text = "MBAG No.";
			// 
			// label4
			// 
			this.label4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 11, true);
			this.label4.Name = "label4";
			this.label4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.label4.TabIndex = 6;
			this.label4.Text = "ULD No.";
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 72, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 26, true);
			this.OKButton.TabIndex = 8;
			this.OKButton.Text = "OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Exit2ULDForm
			// 
			this.AcceptButton = this.OKButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 110, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ULDTextBox);
			this.Controls.Add(this.MbagTextBox);
			this.Controls.Add(this.MawbTextBox);
			this.Controls.Add(this.ConsolTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Exit2ULDForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Enter ULD No.";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ConsolTextBox, 0);
			this.Controls.SetChildIndex(this.MawbTextBox, 0);
			this.Controls.SetChildIndex(this.MbagTextBox, 0);
			this.Controls.SetChildIndex(this.ULDTextBox, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
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
