using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business;

public sealed class TransportMeansDataProvider : ITransportMeans
{
	public static TransportMeansDataProvider New(JobDeclaration declaration) => declaration == null ? null : new TransportMeansDataProvider(declaration);

	TransportMeansDataProvider(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;

	public string Nationality => declaration.JE_RN_NKTransportNationality;

	public string IdentificationNumber
	{
		get
		{
			switch (declaration.JE_TransportMode)
			{
				case TransportModes.Air:
					return declaration.JE_VoyageFlightNo;
				case TransportModes.FixedTransportInstallations:
				case TransportModes.InlandWaterwayTransport:
				case TransportModes.OwnPropulsion:
				case TransportModes.Rail:
				case TransportModes.Road:
					return declaration.JE_VesselName;
				default:
					return null;
			}
		}
	}

	public string TypeOfIdentification => declaration.JE_TransportMode.ToString() switch
	{
		TransportModes.Air => "40",
		TransportModes.FixedTransportInstallations => "99",
		TransportModes.InlandWaterwayTransport => "81",
		TransportModes.OwnPropulsion => declaration.JE_TransportMeans,
		TransportModes.Rail => "21",
		TransportModes.Road => "30",
		_ => null
	};
}

