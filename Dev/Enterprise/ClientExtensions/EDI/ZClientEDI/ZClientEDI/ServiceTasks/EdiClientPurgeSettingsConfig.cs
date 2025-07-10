using System;
using System.Collections.Generic;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.ServiceTasks
{
	class EdiClientPurgeSettingsConfig : ZClientSpecificPurgeSettingConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			return Array.Empty<ApplicationCodeObj>();
		}
	}
}
