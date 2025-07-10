using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBOutturnReportHeaderInformation : CusMAWBBaseOutturnReportHeaderInformation, IAirOutturnReportHeaderInformation
	{
		public CusHAWBOutturnReportHeaderInformation(CusHAWB hAWB, CusUnderbond underbond)
			: base(hAWB.MAWB, underbond)
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
