using System.Collections;
using System.Collections.Generic;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotOutturnReportHeaderInformation : CusSCAOceanBillOutturnReportHeaderInformation, ISeaOutturnReportHeaderInformation
	{
		public CusSCAPivotOutturnReportHeaderInformation(CusSCAPivot pivot, CusUnderbond underbond)
			: base(pivot.OceanBill, underbond)
		{
		}

		#region Implementation

		protected override IEnumerable<ISeaOutturnReportLineInformation> GetLines(CusUnderbond underbond)
		{
			ArrayList result = new ArrayList();
			if (underbond != null)
			{
				foreach (CusOutturn outturn in underbond.Outturns)
				{
					IOutturnableLine parent = outturn.Parent;
					if (parent is CusSCAPivot)
					{
						result.Add(new CusSCAPivotOutturnReportLineInformation(parent as CusSCAPivot, outturn));
					}
				}
			}

			return (IEnumerable<ISeaOutturnReportLineInformation>)result.ToArray(typeof(ISeaOutturnReportLineInformation));
		}

		#endregion
	}
}
