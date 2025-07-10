using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WinzorFramework;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TaskPanel : ZPanel
	{
		protected override EventAttribute EventAttributes => base.EventAttributes | EventAttribute.MouseDown | EventAttribute.ContextMenu;

		protected override string ControlStyleString => base.ControlStyleString + GetFadeBackgroundStyleString();

		string GetFadeBackgroundStyleString()
		{
			var styleString = string.Empty;
			var backColor = cell.BackColor;
			var backgroundFadeColor = cell.BackgroundFadeColor;
			if (backColor.HasValue && backgroundFadeColor.HasValue)
			{
				styleString = $"background-image:linear-gradient({(int)sectionViewModel.GradientAngle + 90}deg, " +
						$"rgba({backColor.Value.R}, {backColor.Value.G}, {backColor.Value.B}, {Utilities.Round((decimal)(backColor.Value.A / 255.0), 2)}), " +
						$"rgba({backgroundFadeColor.Value.R}, {backgroundFadeColor.Value.G}, {backgroundFadeColor.Value.B}, {Utilities.Round((decimal)(backgroundFadeColor.Value.A / 255.0), 2)}));";
			}
			return styleString;
		}
	}
}
