using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.BE.Business;

public class BEEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		yield return AddApplicationCodeMessageTypePurgeType(
			ApplicationCodeList.Codes.BECustoms,
			new InterchangeObjCollection() { NewInterchangeConfigObj(6, TimeUnit.Month) },
			new MessageTypePurgeTypeObjCollection()
				.Add(SendMessageTypes.Codes.AES, SendMessageTypes.Descriptions.AES, 8, TimeUnit.Year)
				.Add(SendMessageTypes.Codes.IMP, SendMessageTypes.Descriptions.IMP, 8, TimeUnit.Year)
				.Add(SendMessageTypes.Codes.NCT, SendMessageTypes.Descriptions.NCT, 8, TimeUnit.Year));
	}
}
