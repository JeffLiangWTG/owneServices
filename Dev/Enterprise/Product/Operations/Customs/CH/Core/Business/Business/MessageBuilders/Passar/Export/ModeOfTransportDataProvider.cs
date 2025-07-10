using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business;

public class ModeOfTransportDataProvider : IModeOfTransport
{
	public static ModeOfTransportDataProvider New(JobDeclaration declaration) => declaration == null ? null : new ModeOfTransportDataProvider(declaration);

	ModeOfTransportDataProvider(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;

	public string InlandModeOfTransport => declaration.JE_TransportMode.ToString() switch
	{
		TransportModes.Air => "4",
		TransportModes.FixedTransportInstallations => "7",
		TransportModes.InlandWaterwayTransport => "8",
		TransportModes.OwnPropulsion => "9",
		TransportModes.Rail => "2",
		TransportModes.Road => "3",
		_ => null
	};

	public ITransportMeans TransportMeans => (declaration.JE_TransportMode != TransportTypeList.Codes.OwnPropulsion && declaration.JE_VoyageFlightNo.IsEmpty && declaration.JE_VesselName.IsEmpty && declaration.JE_RN_NKTransportNationality.IsEmpty) ? null : transportMeans ??= TransportMeansDataProvider.New(declaration);
	ITransportMeans transportMeans;
}
