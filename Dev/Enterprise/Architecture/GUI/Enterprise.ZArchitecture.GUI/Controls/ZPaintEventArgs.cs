using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	internal class ZPaintEventArgs : PaintEventArgs
	{
		public readonly Rectangle ActualRectangle;

		public ZPaintEventArgs(
			#if WINZOR
			BGraphics graphics,
			#else
			Graphics graphics,
			#endif
			Rectangle actualRectangle)
			: base(graphics, actualRectangle)
		{
			ActualRectangle = actualRectangle;
		}
	}
}
