using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GLAccountFormat
{
	public class GLNumberBox : ZPanel
	{
		public const int BoxWidth = 30;
		public const int BoxHeight = 30;

		public GLNumberBox() : this("")
		{
		}

		public GLNumberBox(string text)
		{
			this.BorderStyle = BorderStyle.FixedSingle;
			ControlDpiScalingHelper.SetWidth(this, BoxWidth, true);
			ControlDpiScalingHelper.SetHeight(this, BoxHeight, true);

			CaptionLabel = new ZLabel();
			CaptionLabel.Parent = this;
			CaptionLabel.TextAlign = ContentAlignment.MiddleCenter;
			CaptionLabel.BackColor = Color.Transparent;
			CaptionLabel.Dock = DockStyle.Fill;
			CaptionLabel.Click += new EventHandler(CaptionLabel_Click);
			Caption = text;
		}

		public event EventHandler LabelClicked;

		public string Caption
		{
			get { return CaptionLabel.Text; }
			set
			{
				CaptionLabel.Text = value;
				UpdateColor();
			}
		}

		public int Index
		{
			get { return fIndex; }
			set { fIndex = value; }
		}

		public bool IsHighlighted
		{
			get { return fIsHighlighted; }
		}

		public void Highlight()
		{
			fIsHighlighted = true;
			BackColor = Color.LightGreen;
		}

		public void UnHighlight()
		{
			fIsHighlighted = false;
			BackColor = StatusBackColor;
		}

		#region Implementation
		protected Label CaptionLabel;
		protected Color StatusBackColor;
		protected int fIndex;
		protected bool fIsHighlighted;

		protected void CaptionLabel_Click(object sender, EventArgs e)
		{
			if (LabelClicked != null)
			{
				LabelClicked(this, e);
			}
		}

		protected void UpdateColor()
		{
			StatusBackColor = (string.IsNullOrEmpty(Caption)) ? DefaultBackColor : Color.AliceBlue;
			BackColor = StatusBackColor;
		}
		#endregion
	}
}
