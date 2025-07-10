using System.Collections;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitReportLookups : CusExitReportUcc6Lookups
	{
		public CusExitReportLookups(CusExitReport parent)
			: base(parent)
		{
		}

		public ICollection Locations => LocationsHelper.GetESLocationsCusCodeList(Factory);
	}
}
