using System;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.DocumentVisualizer.GUI
{
	class VisualizerDropButton : ZDropButton
	{
		public const double ButtonAspectRatio = 0.7;
		const int DepthEffectAdjustmentPixel = 1;

		protected override Point ButtonPoint
		{
			get
			{
				return ControlDpiScalingHelper.NewScaledPoint(ClientRectangle.Right - Convert.ToInt32(ClientRectangle.Height * ButtonAspectRatio) - DepthEffectAdjustmentPixel, ClientRectangle.Top + DepthEffectAdjustmentPixel, false);
			}
		}

		protected override Size ButtonSize
		{
			get
			{
				var depthEffectAdjustmentPixel = DepthEffectAdjustmentPixel * 2; //account for 1 pixel adjustment for top and bottom

				var width = Convert.ToInt32(ClientRectangle.Height * ButtonAspectRatio);
				var height = Height - depthEffectAdjustmentPixel;

				return ControlDpiScalingHelper.NewScaledSize(width, height, false);
			}
		}
	}
}
