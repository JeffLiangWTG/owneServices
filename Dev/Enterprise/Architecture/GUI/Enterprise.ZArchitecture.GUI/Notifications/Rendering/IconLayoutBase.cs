using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public abstract class IconLayoutBase : IIconLayout
	{
		public const int XOffset = 1;
		public const int YOffset = 1;

		public Control Target { get; set; }
		public bool SemiTransparent { get; set; }

		public virtual void ResizeControl(bool revert) { }
		public abstract Rectangle Compute(Rectangle area, Size clip);
	}
}