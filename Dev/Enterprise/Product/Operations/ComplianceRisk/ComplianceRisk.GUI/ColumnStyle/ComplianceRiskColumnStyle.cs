using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using ComplianceRiskDesc = Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Descriptions;
using GridColor = Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants.GridColor;
using ScreeningStatusDesc = Enterprise.MasterFiles.Integration.ScreeningStatusesList.Descriptions;

namespace Enterprise.ComplianceRisk.GUI
{
	public class ComplianceRiskColumnStyle : ZTextBoxColumnStyle
	{
		public ComplianceRiskColumnStyle(ComplianceRiskColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}

		#region Paint

#if !WINZOR

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
		{
			var riskStatus = (ZString)GetColumnValueAtRow(source, paintingRowNum);
			if (riskStatus == ComplianceRiskDesc.Released.ToString()
				|| riskStatus == ComplianceRiskDesc.Clear.ToString()
				|| riskStatus == ScreeningStatusDesc.Clear.ToString())
			{
				backBrush = new SolidBrush(GridColor.Clear);
			}
			else if (riskStatus == ComplianceRiskDesc.PotentialRisk.ToString()
				|| riskStatus == ComplianceRiskDesc.Blocked.ToString()
				|| riskStatus == ComplianceRiskDesc.NotChecked.ToString()
				|| riskStatus == ComplianceRiskDesc.HighRisk.ToString()
				|| riskStatus == ComplianceRiskDesc.Held.ToString())
			{
				backBrush = new SolidBrush(GridColor.Block);
			}
			else if (riskStatus == ScreeningStatusDesc.PermanentClear.ToString())
			{
				backBrush = new SolidBrush(GridColor.PermanentClear);
			}
			else if (riskStatus == ComplianceRiskDesc.OverrideClear.ToString())
			{
				backBrush = new SolidBrush(GridColor.JobCleared);
			}
			else if (riskStatus == ComplianceRiskDesc.AssessmentNotInitialized.ToString())
			{
				backBrush = new SolidBrush(GridColor.JobAssessmentNotInitiatedBack);
				foreBrush = new SolidBrush(GridColor.JobAssessmentNotInitiatedFore);
			}
			else if (riskStatus == ComplianceRiskDesc.PossibleRisk.ToString())
			{
				backBrush = new SolidBrush(GridColor.PossibleRisk);
			}
			else
			{
				backBrush = new SolidBrush(GridColor.Matched);
			}

			g.FillRectangle(backBrush, bounds);
			if (ShouldDrawNotifications(source))
			{
				backBrush = NotificationProvider.DecideBackgroundColorBrush(source, paintingRowNum, backBrush);
				bounds = NotificationProvider.PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(g, source, paintingRowNum, bounds, backBrush);
			}

			var cellText = ColumnTextAtRow(source, paintingRowNum);
			PaintText(g, bounds, source, paintingRowNum, cellText, DataGridTableStyle.DataGrid.Font, backBrush, foreBrush, false);
		}

#endif
		#endregion
	}

	public class ComplianceRiskColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(ComplianceRiskColumnStyle); }
		}
	}
}
