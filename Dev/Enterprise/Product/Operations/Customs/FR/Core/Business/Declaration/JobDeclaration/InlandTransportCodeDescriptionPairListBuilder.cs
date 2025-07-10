using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration;

public sealed class InlandTransportCodeDescriptionPairListBuilder : EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder
{
	public InlandTransportCodeDescriptionPairListBuilder(EU.Business.Declaration.JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override void AddInlandWaterwayTransportPairs(CodeDescriptionPairList result)
	{
		result.AddPair(MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode, MeansOfTransportList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
		result.AddPair(MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel);
		result.DefaultCode = MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode;
	}

	protected override void AddSeaPairs(CodeDescriptionPairList result)
	{
		result.AddPair(MeansOfTransportList.Codes.ImoShipIdentificationNumber, MeansOfTransportList.Descriptions.ImoShipIdentificationNumber);
		result.AddPair(MeansOfTransportList.Codes.NameOfTheSeaGoingVessel, MeansOfTransportList.Descriptions.NameOfTheSeaGoingVessel);
		result.DefaultCode = MeansOfTransportList.Codes.ImoShipIdentificationNumber;
	}
}
