using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecTransportMeansDataProvider : IEdecTransportMeans
{
	public static EdecTransportMeansDataProvider New(JobDeclaration declaration) => declaration == null ? null : new EdecTransportMeansDataProvider(declaration);

	protected EdecTransportMeansDataProvider(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	public string TransportMode => MapTransportMode(declaration.JE_TransportMode);

	public string TransportationType => declaration.JE_VehicleType;

	public string TransportationCountry => declaration.JE_RN_NKTransportNationality;

	public string TransportationNumber => declaration.IsAir ? declaration.JE_VoyageFlightNo : declaration.JE_VesselName;

	protected string MapTransportMode(string transportMode)
	{
		switch (transportMode)
		{
			case Core.Constants.TransportModes.Rail:
				return "2";
			case Core.Constants.TransportModes.Road:
				return "3";
			case Core.Constants.TransportModes.Air:
				return "4";
			case Core.Constants.TransportModes.Mail:
				return "5";
			case Core.Constants.TransportModes.FixedTransportInstallations:
				return "7";
			case Core.Constants.TransportModes.InlandWaterwayTransport:
				return "8";
			case Core.Constants.TransportModes.OwnPropulsion:
				return "9";
			default:
				return "0";
		}
	}
}
