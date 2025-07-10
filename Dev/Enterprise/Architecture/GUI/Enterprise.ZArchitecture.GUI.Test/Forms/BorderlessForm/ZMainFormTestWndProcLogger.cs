using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR

	internal class ZMainFormTestWndProcLogger : ZMainForm
	{
		internal HashSet<Forms.Native.WindowMessages> WndProcMsgLog = new HashSet<Forms.Native.WindowMessages>();

		protected override void WndProc(ref Message m)
		{
			WndProcMsgLog.UnionWith(new[] { (Forms.Native.WindowMessages)m.Msg });
			base.WndProc(ref m);
		}
	}

#endif
}
