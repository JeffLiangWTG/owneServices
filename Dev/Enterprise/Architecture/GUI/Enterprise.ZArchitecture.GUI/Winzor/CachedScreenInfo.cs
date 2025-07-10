using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Enterprise.Core.Forms
{
	public enum HorizontalState { OffScreenLeft, Visible, OffScreenRight }
	public enum VerticalState { OffScreenTop, Visible, OffScreenBottom }

	public class CachedScreenInfo
	{
		public static CachedScreenInfo Instance { get; } = new CachedScreenInfo();

		public Rectangle PrimaryScreenInfo => new Rectangle(0, 0, 3840, 2160);

		public Rectangle[] ScreenInfos => new[] { PrimaryScreenInfo };

		public List<Rectangle> BoundsInfos => ScreenInfos.ToList();

		public Rectangle FromControl(Control control) => PrimaryScreenInfo;

		public Rectangle FromPoint(Point location) => PrimaryScreenInfo;

		public Rectangle getCurrentScreen(Point point) => PrimaryScreenInfo;

		public HorizontalState GetHorizontalState(Point point) => default(HorizontalState);

		public VerticalState GetVerticalState(Point point, int height) => default(VerticalState);
	}
}
