using System.Drawing;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	sealed class ZSqlProfilerGridColumnNotificationProvider : ZGridColumnNotificationProvider
	{
		public ZSqlProfilerGridColumnNotificationProvider(IGridColumnStyle columnStyle)
			: base(columnStyle)
		{
		}

		public override void OnEnterEditControl(CurrencyManager source, int rowNum, Rectangle editControlRect, Control anchor = null)
		{
		}
	}
}
