using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class InlandTransportCodeDescriptionPairListBuilder : EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder
{
	public InlandTransportCodeDescriptionPairListBuilder(EU.Business.Declaration.JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override void AddInlandWaterwayTransportPairs(CodeDescriptionPairList result)
	{
		base.AddInlandWaterwayTransportPairs(result);
		result.AddPair(MeansOfTransportList.Codes.ImoShipIdentificationNumber, MeansOfTransportList.Descriptions.ImoShipIdentificationNumber);
		result.AddPair(MeansOfTransportList.Codes.NameOfTheSeaGoingVessel, MeansOfTransportList.Descriptions.NameOfTheSeaGoingVessel);
	}

	protected override void AddSeaPairs(CodeDescriptionPairList result)
	{
		base.AddSeaPairs(result);
		result.AddPair(MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode, MeansOfTransportList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
		result.AddPair(MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel);
	}
}
