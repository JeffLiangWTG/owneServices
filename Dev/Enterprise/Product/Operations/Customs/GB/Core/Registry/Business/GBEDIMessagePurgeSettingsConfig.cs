using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.GB.Registry;

public class GBEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		short defaultDurationForCDS = 7;
		short defaultDurationForOthers = 4;
		var defaultUnit = TimeUnit.Year;

		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCustomsDeclarationServices, [NewInterchangeConfigObj(defaultDurationForCDS, defaultUnit)], defaultDurationForCDS, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCDSViaCCSUK, [NewInterchangeConfigObj(defaultDurationForCDS, defaultUnit)], defaultDurationForCDS, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCDSDISQuery, [NewInterchangeConfigObj(defaultDurationForCDS, defaultUnit)], defaultDurationForCDS, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCcsuk, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCnsCompass, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCommonTransitConvention, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCustomsEMCS, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCustomsGVMSManifest, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbCustomsNCTS, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbEdifactShared, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMcpClaimUcn, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMcpPortHealth, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMcpRra01AndRra11, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMcpRra12, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMessageICSGreatBritain, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMessageICSNorthernIreland, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbMiscTextAndIslService, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.GbNesAllMessageTypes, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.Pentant, [NewInterchangeConfigObj(defaultDurationForOthers, defaultUnit)], defaultDurationForOthers, defaultUnit);
	}
}
