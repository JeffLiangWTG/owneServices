using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public class ConditionsColumnStyle : ZMultiLineTextBoxColumnStyle
	{
		public ConditionsColumnStyle(ConditionsColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}

		#region Paint

#if !WINZOR

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
		{
			var conditions = (ZString)GetColumnValueAtRow(source, paintingRowNum);
			if (!conditions.IsEmpty)
			{
				backBrush = new SolidBrush(DeniedPartyConstants.GridColor.NotScreened);

				g.FillRectangle(backBrush, bounds);
				if (ShouldDrawNotifications(source))
				{
					backBrush = NotificationProvider.DecideBackgroundColorBrush(source, paintingRowNum, backBrush);
					bounds = NotificationProvider.PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(g, source, paintingRowNum, bounds, backBrush);
				}

				var cellText = ColumnTextAtRow(source, paintingRowNum);
				PaintText(g, bounds, source, paintingRowNum, cellText, DataGridTableStyle.DataGrid.Font, backBrush, foreBrush, false);
			}
			else
			{
				base.Paint(g, bounds, source, paintingRowNum, backBrush, foreBrush, alignedToRight);
			}
		}

#endif
		#endregion
	}

	public class ConditionsColumnStyleInfo : ZMultiLineTextBoxColumnInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(ConditionsColumnStyle); }
		}
	}
}
