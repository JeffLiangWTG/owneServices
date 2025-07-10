using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBOutturnReportHeaderInformation : CusMAWBBaseOutturnReportHeaderInformation, IAirOutturnReportHeaderInformation
	{
		public CusMAWBOutturnReportHeaderInformation(CusMAWB mAWB, CusUnderbond underbond)
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
					CusMAWB mAWB = outturn.Parent as CusMAWB;
					if (mAWB != null)
					{
						result.Add(new CusMAWBOutturnReportLineInformation(mAWB, outturn));
					}
					CusHAWB hAWB = outturn.Parent as CusHAWB;
					if (hAWB != null)
					{
						result.Add(new CusHAWBOutturnReportLineInformation(hAWB, outturn));
					}
				}
			}
			return (IAirOutturnReportLineInformation[])result.ToArray(typeof(IAirOutturnReportLineInformation));
		}

		#endregion
	}
}
