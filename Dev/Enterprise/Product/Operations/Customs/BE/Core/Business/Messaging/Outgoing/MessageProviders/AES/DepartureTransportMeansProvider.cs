using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class DepartureTransportMeansProvider : IDepartureTransportMeans
{
	readonly JobDeclaration declaration;
	public DepartureTransportMeansProvider(JobDeclaration transport, ZString identificationNumber, ZString nationality, int sequence)
	{
		declaration = Argument.NotNull(transport, nameof(transport));
		SequenceNumber = sequence;
		IdentificationNumber = identificationNumber.ToUpper();
		Nationality = nationality.ToUpper();
	}

	public int SequenceNumber { get; }

	public int? TypeOfIdentification => int.TryParse(declaration.JE_TransportMeans, out int x) ? x : null;

	public string IdentificationNumber { get; }

	public string Nationality { get; }
}
