using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.DocumentVisualizer.GUI
{
	class VisualizerDropEdit : ZDropEdit
	{
		protected override void SynchroniseControlSizes()
		{
			// Some WinForms layout event causes the DescriptionBox to get pushed out of bounds. Keep it inside the control
			if (DescriptionBox.Top != 0)
			{
				ControlDpiScalingHelper.SetTop(DescriptionBox, 0, true);
			}

			DescriptionBox.AutoSize = false;

			if (DescriptionBox.Height != Height)
			{
				ControlDpiScalingHelper.SetHeight(DescriptionBox, Height, false);
			}

			if (DropButton.Height != Height)
			{
				ControlDpiScalingHelper.SetHeight(DropButton, Height, false);
			}

			SetControlWidth(Width);
		}

		protected override int GetButtonWidth()
		{
			// This will return the width required to render the DropButton (button + included text box)
			// The addition is split into two, being the first, the padding of the CodeBox inside the control, and the second an additional space left so the button doesn't overlap with the CodeBox
			// They're split to account for rounding errors when rendering on high DPI monitors

			const int codeBoxPadding = 3;
			const int additionalSpace = 1;

			return Convert.ToInt32(ClientRectangle.Height * VisualizerDropButton.ButtonAspectRatio) + ControlDpiScalingHelper.ScaleToCurrentDpiX(codeBoxPadding) + ControlDpiScalingHelper.ScaleToCurrentDpiX(additionalSpace);
		}

		protected override ZDropButton NewDropButton()
		{
			return new VisualizerDropButton();
		}
	}
}
