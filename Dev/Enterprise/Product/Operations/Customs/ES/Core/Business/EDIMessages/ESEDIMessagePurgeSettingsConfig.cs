using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.ES.Business;

sealed class ESEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.ESCustomsMessage, [NewInterchangeConfigObj(10, TimeUnit.Year)], 10, TimeUnit.Year);
	}
}
