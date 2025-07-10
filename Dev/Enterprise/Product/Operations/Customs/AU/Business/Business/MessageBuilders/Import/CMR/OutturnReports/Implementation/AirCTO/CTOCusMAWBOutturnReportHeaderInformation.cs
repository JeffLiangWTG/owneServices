using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusMAWBOutturnReportHeaderInformation : CusMAWBBaseOutturnReportHeaderInformation, IAirOutturnReportHeaderInformation
	{
		public CTOCusMAWBOutturnReportHeaderInformation(CTOCusMAWB mAWB, CusUnderbond underbond)
			: base(mAWB, underbond)
		{
		}

		#region Implementation

		protected override IAirOutturnReportLineInformation[] GetLines(CusUnderbond underbond)
		{
			ArrayList result = new ArrayList();
			if (underbond != null)
			{
				foreach (CusOutturn outturn in underbond.Outturns)
				{
					CTOCusHAWB hAWB = outturn.Parent as CTOCusHAWB;
					if (hAWB != null)
					{
						result.Add(new CTOCusHAWBOutturnReportLineInformation(hAWB, outturn));
					}
					CusPartShip partShip = outturn.Parent as CusPartShip;
					if (partShip != null)
					{
						result.Add(new CTOCusPartShipOutturnReportLineInformation(partShip, (CTOCusHAWB)partShip.HouseBill, outturn));
					}
				}
			}
			return (IAirOutturnReportLineInformation[])result.ToArray(typeof(IAirOutturnReportLineInformation));
		}

		#endregion
	}
}
