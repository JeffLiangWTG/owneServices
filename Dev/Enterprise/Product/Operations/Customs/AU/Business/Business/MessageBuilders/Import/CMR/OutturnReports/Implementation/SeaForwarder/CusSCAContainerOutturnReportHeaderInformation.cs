using System.Collections;
using System.Collections.Generic;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerOutturnReportHeaderInformation : CusSCAOceanBillOutturnReportHeaderInformation, ISeaOutturnReportHeaderInformation
	{
		public CusSCAContainerOutturnReportHeaderInformation(CusSCAContainer container, CusUnderbond underbond)
			: base(container.OceanBill, underbond)
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
					if (parent is CusSCAContainer)
					{
						result.Add(new CusSCAContainerOutturnReportLineInformation(parent as CusSCAContainer, outturn));
					}
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
