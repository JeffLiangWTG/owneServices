using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CusRelatedEventsProvider : Customs.Business.CusRelatedEventsProvider, Integration.Customs.CA.ICusRelatedEventsProvider
	{
		protected override IEnumerable<ZString> GetAvailableApplicationCodes() => CusSCAOceanBill.ApplicationCodes;
	}
}
