using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class VisualizerCodeFindBox : ZCodeFindBox
	{
		public VisualizerCodeFindBox(Func<IFindBoxPopup> customFindBoxPopupProvider = null)
		{
			this.customFindBoxPopupProvider = customFindBoxPopupProvider;
			originalCodeBoxSize = CodeBox.Size;
			originalPopupButtonSize = PopupButton.Size;
			originalTextHeight = TextHeight;
		}

		readonly Func<IFindBoxPopup> customFindBoxPopupProvider;
		readonly Size originalCodeBoxSize;
		readonly Size originalPopupButtonSize;
		readonly int originalTextHeight;

		int TextHeight => textHeight ?? (textHeight = TextRenderer.MeasureText("M", CodeBox.Font).Height).Value;
		int? textHeight;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return customFindBoxPopupProvider?.Invoke() ?? base.GetNewPopupForm();
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			ControlDpiScalingHelper.SetHeight(ref PopupButton, CodeBox.Height, false);

			var popupSizeRatio = (decimal)PopupButton.Height / originalPopupButtonSize.Height;
			ControlDpiScalingHelper.SetWidth(ref PopupButton, (int)(originalPopupButtonSize.Width * popupSizeRatio), false);

			var textSizeRatio = (decimal)TextHeight / originalTextHeight;
			ControlDpiScalingHelper.SetWidth(ref CodeBox, Math.Max(0, Math.Min((int)(originalCodeBoxSize.Width * textSizeRatio), Width - (int)(originalPopupButtonSize.Width * popupSizeRatio))), false);
		}

		protected override bool RestrictHeight
		{
			get { return false; }
		}
	}
}
