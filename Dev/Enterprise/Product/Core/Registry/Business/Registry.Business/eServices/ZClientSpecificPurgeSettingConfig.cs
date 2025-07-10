using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class ZClientSpecificPurgeSettingConfig : PurgeSettingsConfig
	{
		public static ZClientSpecificPurgeSettingConfig New()
		{
			var type = TypeDecider.GetTypeForBinding(typeof(ZClientSpecificPurgeSettingConfig));
			return (ZClientSpecificPurgeSettingConfig)Activator.CreateInstance(type);
		}

		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			return Array.Empty<ApplicationCodeObj>();
		}
	}
}
