using System;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class DepartureMeansOfTransportWrapper : IMeansOfTransport
{
	DepartureMeansOfTransportWrapper(string identificationNumber
		, string typeOfIdentification
		, string nationality)
	{
		this.identificationNumber = identificationNumber;
		this.typeOfIdentification = typeOfIdentification;
		this.nationality = nationality;

		lazyTypeOfIdentification = new Lazy<int>(GetTypeOfIdentification);
	}

	public static DepartureMeansOfTransportWrapper NewOrNull(string identificationNumber
		, string typeOfIdentification
		, string nationality)
	{
		if (string.IsNullOrWhiteSpace(identificationNumber))
		{
			return null;
		}

		return new DepartureMeansOfTransportWrapper(identificationNumber, typeOfIdentification, nationality);
	}

	int IMeansOfTransport.TypeOfIdentification => lazyTypeOfIdentification.Value;

	string IMeansOfTransport.IdentificationNumber => identificationNumber;

	string IMeansOfTransport.Nationality => nationality;

	int GetTypeOfIdentification()
	{
		return int.TryParse(typeOfIdentification, out var result)
			? result
			: -1;
	}

	readonly string identificationNumber;
	readonly string typeOfIdentification;
	readonly string nationality;
	readonly Lazy<int> lazyTypeOfIdentification;
}
