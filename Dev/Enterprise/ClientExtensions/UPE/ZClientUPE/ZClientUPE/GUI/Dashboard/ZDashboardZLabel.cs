using System.Drawing;

using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.GUI
{
	public class ZDashboardZLabel : ZLabel
	{
		public override Font Font
		{
			get { return fFont; }
		}
		readonly Font fFont = new Font("Arial Black", 23F, FontStyle.Bold);
	}
}
