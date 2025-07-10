using System;
using System.Linq;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public partial class InstallationResultsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.ErrorLabel = new System.Windows.Forms.Label();
			this.WarningLabel = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.WarningErrorTextBox = new System.Windows.Forms.TextBox();
			this.panel3 = new System.Windows.Forms.Panel();
			this.OKButton = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.ErrorLabel);
			this.panel1.Controls.Add(this.WarningLabel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(4);
			this.panel1.Size = new System.Drawing.Size(416, 56);
			this.panel1.TabIndex = 0;
			// 
			// ErrorLabel
			// 
			this.ErrorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ErrorLabel.Location = new System.Drawing.Point(4, 4);
			this.ErrorLabel.Name = "ErrorLabel";
			this.ErrorLabel.Size = new System.Drawing.Size(408, 48);
			this.ErrorLabel.TabIndex = 1;
			this.ErrorLabel.Text = "The Application encountered errors and is unable to start. A list of problems is sh" +
				"own below. You or your IT administrator should correct these problems.";
			this.ErrorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ErrorLabel.Visible = false;
			// 
			// WarningLabel
			// 
			this.WarningLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WarningLabel.Location = new System.Drawing.Point(4, 4);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = new System.Drawing.Size(408, 48);
			this.WarningLabel.TabIndex = 0;
			this.WarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.WarningLabel.Visible = false;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.WarningErrorTextBox);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 56);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.panel2.Size = new System.Drawing.Size(416, 278);
			this.panel2.TabIndex = 1;
			// 
			// WarningErrorTextBox
			// 
			this.WarningErrorTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.WarningErrorTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WarningErrorTextBox.Location = new System.Drawing.Point(4, 0);
			this.WarningErrorTextBox.Multiline = true;
			this.WarningErrorTextBox.Name = "WarningErrorTextBox";
			this.WarningErrorTextBox.ReadOnly = true;
			this.WarningErrorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.WarningErrorTextBox.Size = new System.Drawing.Size(408, 278);
			this.WarningErrorTextBox.TabIndex = 0;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.OKButton);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel3.Location = new System.Drawing.Point(0, 334);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(416, 40);
			this.panel3.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = new System.Drawing.Point(328, 8);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(75, 23);
			this.OKButton.TabIndex = 0;
			this.OKButton.Text = "OK";
			// 
			// InstallationResultsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(416, 374);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel1);
			this.Name = "InstallationResultsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Installation Results";
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		internal System.Windows.Forms.Panel panel1;
		internal System.Windows.Forms.Panel panel2;
		internal System.Windows.Forms.Panel panel3;
		internal System.Windows.Forms.Label WarningLabel;
		internal System.Windows.Forms.Label ErrorLabel;
		internal System.Windows.Forms.Button OKButton;
		internal System.Windows.Forms.TextBox WarningErrorTextBox;
	}
}
