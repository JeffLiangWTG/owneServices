using System.Drawing;
using System.Windows.Forms;
using WinzorFramework.Extensions;

namespace CargoWise.Windows.UI
{
	public class KProgressBar : ProgressBar
	{
		public void SetForeGroundColor(Color color)
		{
			ProgressBarColor = color;
		}

		public Brush GetForeGroundColor()
		{
			ColorBrush?.Dispose();
			ColorBrush = new SolidBrush(ProgressBarColor);
			return ColorBrush;
		}

		Brush ColorBrush { get; set; }

		Color progressBarColor = Color.Empty;
		Color ProgressBarColor
		{
			get => progressBarColor;
			set => UpdateProperty(ref progressBarColor, value);
		}

		protected override string ProgressBarStyleString
		{
			get
			{
				var progressBarStyleString = base.ProgressBarStyleString;

				if (!progressBarColor.IsEmpty)
				{
					progressBarStyleString += $"background-color:{ProgressBarColor.GetColorStyleValue()};";
				}
				return progressBarStyleString;
			}
		}
	}
}
