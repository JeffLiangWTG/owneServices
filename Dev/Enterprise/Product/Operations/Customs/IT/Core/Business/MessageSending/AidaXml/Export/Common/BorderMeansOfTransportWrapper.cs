using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class BorderMeansOfTransportWrapper : IMeansOfTransport
{
	public static IMeansOfTransport NewOrNull(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, nameof(declaration));

		var borderTransportMeans = declaration.ZG_BorderTransportMeans;
		var identificationNumber = declaration.IsAir ? declaration.JE_VoyageFlightNo : declaration.JE_VesselName;
		var nationality = declaration.JE_RN_NKTransportNationality;

		if (borderTransportMeans.IsEmpty && identificationNumber.IsEmpty && nationality.IsEmpty)
		{
			return null;
		}

		var typeOfIdentification = GetTypeOfIdentification(borderTransportMeans);
		return new BorderMeansOfTransportWrapper(typeOfIdentification, identificationNumber, nationality);
	}

	BorderMeansOfTransportWrapper(int typeOfIdentification, string identificationNumber, string nationality)
	{
		this.typeOfIdentification = typeOfIdentification;
		this.identificationNumber = identificationNumber;
		this.nationality = nationality;
	}

	readonly int typeOfIdentification;
	readonly string identificationNumber;
	readonly string nationality;

	int IMeansOfTransport.TypeOfIdentification => typeOfIdentification;

	string IMeansOfTransport.IdentificationNumber => identificationNumber;

	string IMeansOfTransport.Nationality => nationality;

	static int GetTypeOfIdentification(ZString borderTransportMeans)
	{
		const int invalidTypeOfIdentification = -1;

		return int.TryParse(borderTransportMeans, out var transportMode)
			? transportMode
			: invalidTypeOfIdentification;
	}
}
