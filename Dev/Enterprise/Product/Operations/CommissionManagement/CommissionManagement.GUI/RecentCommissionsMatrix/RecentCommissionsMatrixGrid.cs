using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public class RecentCommissionsMatrixGrid : ZGrid
	{
#if !WINZOR
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (!DesignModeFinder.IsDesigning)
			{
				if (List != null && List.Count > 0)
				{
					int firstVisibleRow = GetFirstVisibleRow();
					int lastVisibleRow = firstVisibleRow + VisibleRowCount;

					if (lastVisibleRow > List.Count)
					{
						lastVisibleRow = List.Count;
					}

					var graphics = e.Graphics;
					for (int paintingRowIndex = firstVisibleRow; paintingRowIndex < lastVisibleRow; paintingRowIndex++)
					{
						var rowHeaderText = GetRowHeaderTextFromListIndex(paintingRowIndex);
						if (!rowHeaderText.IsEmpty)
						{
							var notificationRect = GetRowNotificationRectangle(paintingRowIndex);
							if (notificationRect.Top != 0)
							{
								using (var brush = new SolidBrush(ForeColor))
								{
									using (var stringFormat = new StringFormat())
									{
										stringFormat.Alignment = StringAlignment.Far;
										TextRendererHelper.DrawText(graphics, rowHeaderText, Font, new Rectangle(0, notificationRect.Y, RowHeaderWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), notificationRect.Y), brush, stringFormat);
									}
								}
							}
						}
					}
				}
			}
		}

		ZString GetRowHeaderTextFromListIndex(int listIndex)
		{
			var list = ListManager != null ? ListManager.List as RecentCommissionsMatrix : null;
			if (list != null)
			{
				var totals = list[listIndex];
				return totals.StatusDescription;
			}

			return ZString.Empty;
		}
#endif

		protected override void RefreshTableStylesCore()
		{
			base.RefreshTableStylesCore();

			foreach (DataGridTableStyle style in TableStyles)
			{
				style.RowHeaderWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			}
		}
	}
}
