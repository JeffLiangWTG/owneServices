using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.GUI.GLAccountFormat
{
	public delegate void BoxEventHandler(GLNumberBox box);

	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class GLFormatMask : GroupBox
	{
		public GLFormatMask(string mask, bool isTarget)
		{
			this.Boxes = new List<GLNumberBox>();
			fMask = mask;
			fIsTarget = isTarget;
			DisplayFormat(fMask);
			this.FlatStyle = FlatStyle.System;
		}

		public event EventHandler FullyEntered;
		public event BoxEventHandler BoxClicked;

		public int NumberCount
		{
			get { return Boxes.Count; }
		}

		public bool IsEmpty
		{
			get
			{
				return Boxes.All(box => string.IsNullOrEmpty(box.Caption));
			}
		}

		public bool IsFullyEntered
		{
			get
			{
				return Boxes.All(box => !string.IsNullOrEmpty(box.Caption));
			}
		}

		public string FormatMask
		{
			get
			{
				var mask = fMask.ToCharArray();
				var boxesIndex = 0;

				for (int i = 0; i < mask.Length; i++)
				{
					if (mask[i] == 'X')
					{
						var box = Boxes[boxesIndex];
						boxesIndex++;

						mask[i] = string.IsNullOrEmpty(box.Caption) ? '0' : box.Caption[0];
					}
				}

				return new string(mask);
			}
		}

		public void RequestRemainingNumbers()
		{
			bool focusSet = false;
			for (int i = 0; i < Boxes.Count; i++)
			{
				var box = Boxes[i];
				if (string.IsNullOrEmpty(box.Caption))
				{
					var aTextBox = new ZCalcEdit();
					aTextBox.MaxLength = 1;
					aTextBox.Parent = box;
					aTextBox.BorderStyle = BorderStyle.FixedSingle;
					ControlDpiScalingHelper.SetTop(aTextBox, (box.Height - aTextBox.Height) / 2, false);
					ControlDpiScalingHelper.SetLeft(aTextBox, 5, true);
					ControlDpiScalingHelper.SetWidth(aTextBox, box.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(10), false);
					aTextBox.Leave += new EventHandler(ATextBox_Leave);
					aTextBox.BringToFront();
					if (!focusSet)
					{
						aTextBox.Focus();
						focusSet = true;
					}
				}
			}
		}

		public void HighlightPlaceHolders(int startingIndex, int totalSourceNumber)
		{
			DeselectEverything();

			int leftMostIndex = 0;
			int rightMostIndex = totalSourceNumber - 1;
			int leftBorder = 0;
			int rightBorder = Boxes.Count - 1;
			for (int i = 0; i < Boxes.Count; i++)
			{
				var box = Boxes[i];
				if (!string.IsNullOrEmpty(box.Caption))
				{
					if (box.Index < startingIndex)
					{
						leftBorder = i;
						leftMostIndex = box.Index;
					}
					if (box.Index > startingIndex && box.Index < rightBorder)
					{
						rightBorder = i;
						rightMostIndex = box.Index;
					}
				}
			}
			leftBorder = leftBorder + startingIndex - leftMostIndex;
			rightBorder = rightBorder - (rightMostIndex - startingIndex);

			for (int i = leftBorder; i <= rightBorder; i++)
			{
				var box = Boxes[i];
				if (!box.IsHighlighted && (string.IsNullOrEmpty(box.Caption)))
				{
					box.Highlight();
				}
			}
		}

		public void DeselectEverything()
		{
			foreach (var box in Boxes)
			{
				box.UnHighlight();
			}
		}

		#region Implementation
		protected string fMask;
		protected bool fIsTarget;
		protected List<GLNumberBox> Boxes;

		void Box_LabelClicked(object sender, EventArgs e)
		{
			BoxClicked?.Invoke(sender as GLNumberBox);
		}

		void ATextBox_Leave(object sender, EventArgs e)
		{
			var aTextBox = (ZCalcEdit)sender;
			var box = (GLNumberBox)aTextBox.Parent;

			box.Caption = aTextBox.Text;
			aTextBox.SendToBack();

			if (IsFullyEntered)
			{
				FullyEntered?.Invoke(this, e);
			}
		}

		protected void DisplayFormat(string formatText)
		{
			const int initialLeftPosition = 20;
			const int border = 20;
			const int gap = 10;

			int boxesIndex = 0;
			for (int i = 0; i < formatText.Length; i++)
			{
				switch (formatText[i])
				{
					case 'X':
						var box = new GLNumberBox { Parent = this };
						box.LabelClicked += new EventHandler(Box_LabelClicked);

						ControlDpiScalingHelper.SetLeft(ref box, initialLeftPosition + i * (GLNumberBox.BoxWidth + gap), true);
						ControlDpiScalingHelper.SetTop(ref box, border, true);
						box.Index = boxesIndex;
						if (!fIsTarget)
						{
							box.Caption = string.Format(CultureInfo.InvariantCulture, "X{0}", boxesIndex + 1); // Didnt use it previously, so I am keeping existing functionality
						}

						boxesIndex++;
						Boxes.Add(box);
						break;

					case '.':
						var dot = new GLNumberDot { Parent = this };

						ControlDpiScalingHelper.SetTop(ref dot, border * 2 / 3, true);
						ControlDpiScalingHelper.SetLeft(ref dot, initialLeftPosition + i * (GLNumberBox.BoxWidth + gap) - (gap / 2), true);
						break;
				}
			}

			ControlDpiScalingHelper.SetWidth(this, initialLeftPosition + formatText.Length * (GLNumberBox.BoxWidth + gap), true);
			ControlDpiScalingHelper.SetHeight(this, border * 2 + GLNumberBox.BoxHeight, true);
		}
		#endregion
	}
}
