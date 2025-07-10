using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.IL.Business
{
	class ILEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			ZShort defaultDur = Constants.ILEDIMessagePurgeSettingsConfig.ILCPurgeDuration;
			var defaultUnit = TimeUnit.Year;

			yield return AddApplicationCodeMessageSubTypePurgeType(
				ApplicationCodeList.Codes.ILCustoms,
				new InterchangeObjCollection() { NewInterchangeConfigObj(defaultDur, defaultUnit) },
				new MessageSubTypePurgeTypeObjCollection()
					.Add(ILEDIMessageSubTypeList.Codes.ForwarderManifestResponse, ILEDIMessageSubTypeList.Descriptions.ForwarderManifestResponse, defaultDur, defaultUnit)
				);
		}
	}
}
