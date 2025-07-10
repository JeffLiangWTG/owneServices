using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Interop;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZMessageBox
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
#if DEBUG
		internal
#endif
		void InitializeComponent()
		{
			this.PictureBox = new ZPictureBox();
			this.Button1 = new ZButton();
			this.Button2 = new ZButton();
			this.Button3 = new ZButton();
			this.TextBox = new KTextBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.PictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// PictureBox
			// 
			this.PictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 40, true);
			this.PictureBox.TabIndex = 0;
			this.PictureBox.TabStop = false;
			// 
			// Button1
			// 
			this.Button1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.Button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 60, true);
			this.Button1.Name = "Button1";
			this.Button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Button1.TabIndex = 2;
			// 
			// Button2
			// 
			this.Button2.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.Button2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 60, true);
			this.Button2.Name = "Button2";
			this.Button2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Button2.TabIndex = 3;
			// 
			// Button3
			// 
			this.Button3.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.Button3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 60, true);
			this.Button3.Name = "Button3";
			this.Button3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Button3.TabIndex = 4;
			// 
			// TextBox
			// 
			this.TextBox.AcceptsReturn = true;
			this.TextBox.BackColor = System.Drawing.Color.White;
			this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.TextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 8, true);
			this.TextBox.Multiline = true;
			this.TextBox.Name = "TextBox";
			this.TextBox.ReadOnly = true;
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 48, true);
			this.TextBox.TabIndex = 5;
			this.TextBox.TabStop = false;
			// 
			// ZMessageBox
			// 

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 94, true);
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.Button3);
			this.Controls.Add(this.Button2);
			this.Controls.Add(this.Button1);
			this.Controls.Add(this.PictureBox);
			this.TopMost = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ZMessageBox";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "MessageBox";
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.PictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
