using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZGUISystemInformation
	{
		public static int VerticalScrollBarWidth
		{
			get { return DesignModeFinder.IsDesigning ? 16 : SystemInformation.VerticalScrollBarWidth; }
		}
	}
}
