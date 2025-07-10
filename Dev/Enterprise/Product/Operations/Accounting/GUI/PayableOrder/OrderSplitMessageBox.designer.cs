using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class OrderSplitMessageBox
	{
		#region Auto-Generated

		private ZArchitecture.GUI.ZButton NoButton;
		private ZArchitecture.GUI.ZPictureBox pictureBox1;
		private ZLabel MessageLabel;
		private ZArchitecture.GUI.ZButton SplitButton;
		private ZArchitecture.GUI.ZButton NewButton;
		private readonly System.ComponentModel.Container components = null;

		#endregion
		#region Auto-Generated

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
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
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderSplitMessageBox));
			this.SplitButton = new ZArchitecture.GUI.ZButton();
			this.NoButton = new ZArchitecture.GUI.ZButton();
			this.NewButton = new ZArchitecture.GUI.ZButton();
			this.MessageLabel = new ZLabel();
			this.pictureBox1 = new ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// SplitButton
			// 
			this.SplitButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.SplitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 58, true);
			this.SplitButton.Name = "SplitButton";
			this.SplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 24, true);
			this.SplitButton.TabIndex = 0;
			this.SplitButton.Text = "&Split Order";
			this.SplitButton.Click += new System.EventHandler(this.SplitButton_Click);
			// 
			// NoButton
			// 
			this.NoButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.NoButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 58, true);
			this.NoButton.Name = "NoButton";
			this.NoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 24, true);
			this.NoButton.TabIndex = 1;
			this.NoButton.Text = "Do &Nothing";
			this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// NewButton
			// 
			this.NewButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.NewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 58, true);
			this.NewButton.Name = "NewButton";
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
			this.NewButton.TabIndex = 2;
			this.NewButton.Text = "&Create New Order";
			this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 20, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 13, true);
			this.MessageLabel.TabIndex = 3;
			this.MessageLabel.Text = "This order has some incomplete order lines. Would you like to: ";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 12, true);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 5;
			this.pictureBox1.TabStop = false;
			// 
			// OrderSplitMessageBox
			// 
			this.CancelButton = this.NoButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 96, true);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.NewButton);
			this.Controls.Add(this.NoButton);
			this.Controls.Add(this.SplitButton);
			this.Controls.Add(this.pictureBox1);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OrderSplitMessageBox";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "OrderLineMessageBox";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		#endregion

	}
}
