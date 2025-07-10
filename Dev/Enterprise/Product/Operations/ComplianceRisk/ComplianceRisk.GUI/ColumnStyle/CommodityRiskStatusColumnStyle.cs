using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Desc = Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Descriptions;
using GridColor = Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants.GridColor;

namespace Enterprise.ComplianceRisk.GUI
{
	public class CommodityRiskStatusColumnStyle : ZDropEditColumnStyle
	{
		public CommodityRiskStatusColumnStyle(CommodityRiskStatusColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}

		#region Paint

#if !WINZOR

		protected override void Paint(Graphics graphics, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			var riskStatusDescription = (ZString)GetColumnValueAtRow(source, paintingRowNum);

			if (riskStatusDescription == Desc.Clear.ToString() || riskStatusDescription == Desc.Released.ToString())
			{
				backBrush = new SolidBrush(GridColor.Clear);
			}
			else if (riskStatusDescription == Desc.AssessmentNotInitialized.ToString())
			{
				backBrush = new SolidBrush(GridColor.JobAssessmentNotInitiatedBack);
				foreBrush = new SolidBrush(GridColor.JobAssessmentNotInitiatedFore);
			}
			else if (riskStatusDescription == Desc.PossibleRisk.ToString())
			{
				backBrush = new SolidBrush(GridColor.PossibleRisk);
			}
			else
			{
				backBrush = new SolidBrush(GridColor.Block);
			}

			graphics.FillRectangle(backBrush, bounds);

			if (ShouldDrawNotifications(source))
			{
				backBrush = NotificationProvider.DecideBackgroundColorBrush(source, paintingRowNum, backBrush);
				bounds = NotificationProvider.PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(graphics, source, paintingRowNum, bounds, backBrush);
			}

			PaintText(graphics, bounds, source, paintingRowNum, riskStatusDescription, DataGridTableStyle.DataGrid.Font, backBrush, foreBrush, false);
		}
#endif
		#endregion
	}

	public class CommodityRiskStatusColumnStyleInfo : ZDropEditColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(CommodityRiskStatusColumnStyle); }
		}
	}
}
