using System.Drawing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public class ZScrollablePictureBox : ZUserControl
	{
		public Image Image { get; set; }

		public int Zoom { get; set; }
	}
}
