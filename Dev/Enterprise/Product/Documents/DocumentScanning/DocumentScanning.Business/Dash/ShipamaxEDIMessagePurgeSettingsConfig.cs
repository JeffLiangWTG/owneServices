using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentScanning.Business
{
	public class ShipamaxEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddUnpurgableApplicationCode(ApplicationCodeList.Codes.ShipamaxIntegration);
		}
	}
}
