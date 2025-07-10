using CargoWise.Windows.UI;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms
{
	partial class ZBorderlessForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.TopLeftCornerPanel = new KPanel();
			this.TopRightCornerPanel = new KPanel();
			this.BottomLeftCornerPanel = new KPanel();
			this.BottomRightCornerPanel = new KPanel();
			this.TopBorderPanel = new KPanel();
			this.BottomBorderPanel = new KPanel();
			this.LeftBorderPanel = new KPanel();
			this.RightBorderPanel = new KPanel();
			this.SuspendLayout();
			//
			// TopLeftCornerPanel
			//
			this.TopLeftCornerPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.TopLeftCornerPanel.Cursor = Cursors.SizeNWSE;
			this.TopLeftCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopLeftCornerPanel.Name = "TopLeftCornerPanel";
			this.TopLeftCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(1, 1, true);
			this.TopLeftCornerPanel.TabIndex = 0;
			//
			// TopRightCornerPanel
			//
			this.TopRightCornerPanel.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.TopRightCornerPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.TopRightCornerPanel.Cursor = Cursors.SizeNESW;
			this.TopRightCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(783, 0, true);
			this.TopRightCornerPanel.Name = "TopRightCornerPanel";
			this.TopRightCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(1, 1, true);
			this.TopRightCornerPanel.TabIndex = 1;
			//
			// BottomLeftCornerPanel
			//
			this.BottomLeftCornerPanel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			this.BottomLeftCornerPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.BottomLeftCornerPanel.Cursor = Cursors.SizeNESW;
			this.BottomLeftCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 440, true);
			this.BottomLeftCornerPanel.Name = "BottomLeftCornerPanel";
			this.BottomLeftCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(1, 1, true);
			this.BottomLeftCornerPanel.TabIndex = 1;
			//
			// BottomRightCornerPanel
			//
			this.BottomRightCornerPanel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.BottomRightCornerPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.BottomRightCornerPanel.Cursor = Cursors.SizeNWSE;
			this.BottomRightCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(783, 440, true);
			this.BottomRightCornerPanel.Name = "BottomRightCornerPanel";
			this.BottomRightCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(1, 1, true);
			this.BottomRightCornerPanel.TabIndex = 1;
			//
			// TopBorderPanel
			//
			this.TopBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
		| AnchorStyles.Right)));
			this.TopBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.TopBorderPanel.Cursor = Cursors.SizeNS;
			this.TopBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.TopBorderPanel.Name = "TopBorderPanel";
			this.TopBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(782, 1, true);
			this.TopBorderPanel.TabIndex = 2;
			//
			// BottomBorderPanel
			//
			this.BottomBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left)
		| AnchorStyles.Right)));
			this.BottomBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.BottomBorderPanel.Cursor = Cursors.SizeNS;
			this.BottomBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(1, 440, true);
			this.BottomBorderPanel.Name = "BottomBorderPanel";
			this.BottomBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(782, 1, true);
			this.BottomBorderPanel.TabIndex = 3;
			//
			// LeftBorderPanel
			//
			this.LeftBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
		| AnchorStyles.Left)));
			this.LeftBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.LeftBorderPanel.Cursor = Cursors.SizeWE;
			this.LeftBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.LeftBorderPanel.Name = "LeftBorderPanel";
			this.LeftBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(1, 439, true);
			this.LeftBorderPanel.TabIndex = 4;
			//
			// RightBorderPanel
			//
			this.RightBorderPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
		| AnchorStyles.Right)));
			this.RightBorderPanel.BackColor = Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
			this.RightBorderPanel.Cursor = Cursors.SizeWE;
			this.RightBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(783, 1, true);
			this.RightBorderPanel.Name = "RightBorderPanel";
			this.RightBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(1, 439, true);
			this.RightBorderPanel.TabIndex = 5;
			//
			// ZBorderlessForm
			//
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = Color.White;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(784, 441, true);
			this.Controls.Add(this.RightBorderPanel);
			this.Controls.Add(this.LeftBorderPanel);
			this.Controls.Add(this.BottomBorderPanel);
			this.Controls.Add(this.TopBorderPanel);
			this.Controls.Add(this.TopRightCornerPanel);
			this.Controls.Add(this.BottomLeftCornerPanel);
			this.Controls.Add(this.BottomRightCornerPanel);
			this.Controls.Add(this.TopLeftCornerPanel);
			this.DoubleBuffered = true;
			this.MinimumSize = ControlDpiScalingHelper.NewScaledSize(800, 480, true);
			this.Name = "ZBorderlessForm";
			this.Text = "Title";
			this.SizeChanged += new System.EventHandler(this.ZBorderlessForm_SizeChanged);
			this.ResumeLayout(false);

		}

		#endregion

		protected internal KPanel TopLeftCornerPanel;
		protected internal KPanel TopRightCornerPanel;
		protected internal KPanel BottomLeftCornerPanel;
		protected internal KPanel BottomRightCornerPanel;
		protected internal KPanel TopBorderPanel;
		protected internal KPanel BottomBorderPanel;
		protected internal KPanel LeftBorderPanel;
		protected internal KPanel RightBorderPanel;
	}
}

