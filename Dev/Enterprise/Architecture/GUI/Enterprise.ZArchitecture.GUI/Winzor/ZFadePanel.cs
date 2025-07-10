using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZFadePanel
	{
		protected override string ControlStyleString => $"{base.ControlStyleString}{FadeGradientStyleString}{BackgroundSizeString}{BackgroundPositionString}";

		internal string FadeGradientStyleString
		{
			get
			{
				var styleString = string.Empty;
				var isGradientColorAvailable = !FadeStartColor.IsEmpty && !FadeEndColor.IsEmpty;
				var isGradientPercentageAvailable = GradientStartPercent.HasValue && GradientSizePercent.HasValue;
				if (isGradientColorAvailable && isGradientPercentageAvailable)
				{
					styleString = $"background-color:transparent;background-image:linear-gradient({(int)GradientAngle + 90}deg, " +
						$"rgba({FadeStartColor.R}, {FadeStartColor.G}, {FadeStartColor.B}, {Core.Utilities.Round((decimal)(FadeStartColor.A / 255.0), 2)}) " +
						$"{(int)(GradientStartPercent.Value * 100)}%, " +
						$"rgba({FadeEndColor.R}, {FadeEndColor.G}, {FadeEndColor.B}, {Core.Utilities.Round((decimal)(FadeEndColor.A / 255.0), 2)}) " +
						$"{(int)((GradientStartPercent.Value + GradientSizePercent) * 100)}%);";
				}
				return styleString;
			}
		}

		string BackgroundSizeString => $"background-size:{Width}px {Height}px;";

		string BackgroundPositionString
		{
			get
			{
				var positionRelativeToForm = Location;
				var current = Parent;
				while (current != null && current is not Form)
				{
					positionRelativeToForm.X += current.Location.X;
					positionRelativeToForm.Y += current.Location.Y;
					current = current.Parent;
				}
				return $"background-position:{positionRelativeToForm.X}px {positionRelativeToForm.Y}px;";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Class name")]
		protected override string ClassName => "fadepanel";
	}
}
