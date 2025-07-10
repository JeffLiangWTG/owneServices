using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI
{
	public class CellNotificationSuspender : Disposable
	{
		public CellNotificationSuspender(Control control)
		{
			list = new List<ZGrid>();
			SuspendCore(control);
		}
		readonly List<ZGrid> list;

		void SuspendCore(Control control)
		{
			var grid = control as ZGrid;
			if (grid != null)
			{
				list.Add(grid);
				grid.SuspendCellNotifications();
			}
			foreach (Control innerControl in control.Controls)
			{
				SuspendCore(innerControl);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (!isDisposed)
				{
					foreach (var grid in list)
					{
						grid.ResumeCellNotifications();
					}
					isDisposed = true;
				}
			}
		}

		bool isDisposed;
	}
}
