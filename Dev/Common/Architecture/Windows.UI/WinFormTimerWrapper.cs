using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Data;

namespace CargoWise.Windows.UI
{
	[ToolboxItem(false)]
	public class WinFormTimerWrapper : Timer
	{
		protected override void OnTick(EventArgs e)
		{
			if (!DbEnv.Instance.IsTimerDisabledDuringDbUpgrade)
			{
				base.OnTick(e);
			}
		}
	}
}
