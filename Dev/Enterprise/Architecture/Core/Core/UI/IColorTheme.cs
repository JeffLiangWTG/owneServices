using System.Drawing;

namespace Enterprise.ZArchitecture.Core
{
	public interface IColorTheme
	{
		Color FormBackgroundColor { get; }
		Color MainFormBackgroundColor { get; }
		Color TabBackgroundColor { get; }
		Color GridBackgroundColor { get; }
		Color FilterBackgroundColor { get; }
		Color GridAlternatingRowColor { get; }
		Color GridReadOnlyColor { get; }
		Color ToolbarColor { get; }
		Color ButtonColor { get; }
		Color NavBarBackgroundColor { get; }
		Color NavBarTextColor { get; }
		Color NavBarButtonColor1 { get; }
		Color NavBarGroupBackground1 { get; }
		Color NavBarGroupSelected1 { get; }
		Color NavBarGroupHeaderBackground { get; }
		Color NavBarRecentPanelBackground { get; }
		Color TitleBarBackground { get; }
		Color TitleBarText { get; }
	}
}
