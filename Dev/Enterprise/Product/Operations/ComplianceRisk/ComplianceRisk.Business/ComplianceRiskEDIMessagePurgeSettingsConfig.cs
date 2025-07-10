using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.ComplianceRisk.Business
{
	internal class ComplianceRiskEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			ZShort defaultDur = 6;
			var defaultUnit = TimeUnit.Month;
			yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.CPWRequestMessage, new InterchangeObjCollection() { NewInterchangeConfigObj(defaultDur, defaultUnit) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EDIMessageSubTypeList.Codes.ComplianceRiskAssessment, EDIMessageSubTypeList.Descriptions.ComplianceRiskAssessment, defaultDur, defaultUnit)
			);
		}
	}
}
