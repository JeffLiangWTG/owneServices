using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusRelatedEventsProvider : Customs.Business.CusRelatedEventsProvider, Integration.Customs.AU.ICusRelatedEventsProvider
	{
		protected override IEnumerable<ZString> GetAvailableApplicationCodes() => new ZString[]
		{
			Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages
		};
	}
}
