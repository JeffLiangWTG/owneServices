using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class IncoTermAndCustomsChargeFactory : EUIncoTermAndCustomsChargeFactory
{
	public IncoTermAndCustomsChargeFactory() { }

	protected override ICustomsChargeCode[] GetCharges()
	{
		return ChargesProvider.Codes;
	}
	protected override void SetupIncotermChargeConfigurations()
	{
		var provider = new ChargesProvider();
		foreach (var code in ChargesProvider.Codes)
		{
			foreach (var incoTerm in ChargesProvider.ConfiguredIncoTerms)
			{
				AddChargeConfiguration(incoTerm, code, provider.GetChargeConfiguration(incoTerm, code));
			}
		}
	}

	protected override void SetupErrorConfiguration()
	{
	}

	protected override string FreightToEUBorderCodeCore => BECustomsChargeTypeList.Codes.OverseasFreight;

	protected override MultilingualString FreightToEUBorderDesc => BECustomsChargeTypeList.Descriptions.OverseasFreight;

	protected override string FreightAfterEUBorderCodeCore => BECustomsChargeTypeList.Codes.Transport;

	protected override MultilingualString FreightAfterEUBorderDesc => BECustomsChargeTypeList.Descriptions.Transport;
}
