using System.ComponentModel;

namespace CargoWise.Windows.UI
{
	public class KToolTip : System.Windows.Forms.ToolTip
	{
		public KToolTip() : base() { }
		public KToolTip(IContainer cont) : base(cont) { }
	}
}
