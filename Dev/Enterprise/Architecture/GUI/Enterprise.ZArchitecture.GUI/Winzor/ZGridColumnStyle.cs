using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Extensions;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace Enterprise.Core.Forms
{
	public abstract partial class ZGridColumnStyle : DataGridTextBoxColumn
	{
		protected override NotificationIcon RowColumnNotification(int row)
		{
			var listManager = parentDataGrid.ListManager;
			NotificationIcon columnNotification = null;

			if (listManager != null && listManager.List != null && row < listManager.List.Count)
			{
				var bo = listManager.List[row] as BusinessObject;
				if (bo != null)
				{
					var info = ZPropertyInfoRetriever.GetZPropertyInfo(this, bo);
					if (info != null && info.HasNotifications())
					{
						columnNotification = new NotificationIcon();
						columnNotification.NotificationType = info.GetHighestSeverityNotificationType()?.EnumValueName?.ToLowerHyphen();
						columnNotification.OnMouseOver = (sender, e) => { NotificationProvider.OnMouseHover(parentDataGrid.ListManager, row, Point.Empty, Rectangle.Empty, columnNotification); };
						columnNotification.OnMouseOut = (sender, e) => { NotificationProvider.OnMouseLeave(parentDataGrid.ListManager, row); };
					}
				}
			}

			return columnNotification;
		}

		protected override string GetCellStyleString(CurrencyManager source, int rowNum)
		{
			return this.TextAlign();
		}
	}
}
