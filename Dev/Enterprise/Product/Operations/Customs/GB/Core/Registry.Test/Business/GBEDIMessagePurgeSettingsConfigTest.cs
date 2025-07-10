using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing;

[TestedType(typeof(GBEDIMessagePurgeSettingsConfig))]
sealed class GBEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
{
	public void TestGetPurgeSettings()
	{
		var config = new GBEDIMessagePurgeSettingsConfig();
		short expectedDurationforCDS = 7;
		short expectedDurationforOthers = 4;
		var expectedUnit = TimeUnit.Year;

		var expected = new[]
		{
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCustomsDeclarationServices, expectedDurationforCDS, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCDSViaCCSUK, expectedDurationforCDS, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCDSDISQuery, expectedDurationforCDS, expectedUnit),

			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCcsuk, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCnsCompass, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCommonTransitConvention, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCustomsEMCS, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCustomsGVMSManifest, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbCustomsNCTS, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbEdifactShared, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMcpClaimUcn, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMcpPortHealth, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMcpRra01AndRra11, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMcpRra12, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMessageICSGreatBritain, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMessageICSNorthernIreland, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbMiscTextAndIslService, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.GbNesAllMessageTypes, expectedDurationforOthers, expectedUnit),
			GetExpectedPurgeSetting(ApplicationCodeList.Codes.Pentant, expectedDurationforOthers, expectedUnit),
		};

		AssertContainsExactElementsInAnyOrder(expected, config.GetPurgeSettings().Select(x => (x.ApplicationCode, x.Interchanges[0].PurgeTime, x.Interchanges[0].PurgeTimeUnit, x.PurgeTime, x.PurgeTimeUnit)));

		static (ZString applicationCode, ZShort interchangePurgeTime, ZGuid interchangePurgeTimeUnit, ZShort purgeTime, ZGuid purgeTimeUnit) GetExpectedPurgeSetting(ZString applicationCode, ZShort purgeTime, ZGuid purgeTimeUnit) =>
			(applicationCode, purgeTime, purgeTimeUnit, purgeTime, purgeTimeUnit);
	}
}
