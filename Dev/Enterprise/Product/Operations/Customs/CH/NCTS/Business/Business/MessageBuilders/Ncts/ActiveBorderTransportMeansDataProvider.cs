using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

internal class ActiveBorderTransportMeansDataProvider : IActiveBorderTransportMeans
{
	public static IEnumerable<IActiveBorderTransportMeans> NewCollection(NctsDepartureMovementHeader movementHeader)
	{
		return (movementHeader != null && !movementHeader.BM_ExportTransportMode.IsEmpty) ? GetNewCollection(movementHeader) : null;
	}

	static IEnumerable<IActiveBorderTransportMeans> GetNewCollection(NctsDepartureMovementHeader movementHeader)
	{
		var index = 1;
		yield return new ActiveBorderTransportMeansDataProvider(movementHeader, index++);
		foreach (var cusTransportMeans in movementHeader.AdditionalTransportAtBorderList)
		{
			yield return new ActiveBorderTransportMeansDataProvider(cusTransportMeans, index++);
		}
	}

	ActiveBorderTransportMeansDataProvider(NctsDepartureMovementHeader movementHeader, int sequenceNumber)
	{
		SequenceNumber = sequenceNumber;
		TypeOfIdentification = movementHeader.BM_ActiveBorderIdentificationType.ReturnNullIfEmpty();
		IdentificationNumber = movementHeader.BM_TOLCarrierID.ReturnNullIfEmpty();
		ConveyanceReferenceNumber = movementHeader.BM_ConveyanceNumber.ReturnNullIfEmpty();
		Nationality = movementHeader.BM_RN_NKTOLCarrierNationality.ReturnNullIfEmpty();
		CustomsOfficeAtBorderReferenceNumber = movementHeader.BM_CustomsOfficeAtBorder;
	}

	ActiveBorderTransportMeansDataProvider(DepartureCusTransportMeans cusTransportMeans, int sequenceNumber)
	{
		SequenceNumber = sequenceNumber;
		TypeOfIdentification = cusTransportMeans.TPM_TypeOfIdentification.ReturnNullIfEmpty();
		IdentificationNumber = cusTransportMeans.TPM_IdentificationNumber.ReturnNullIfEmpty();
		ConveyanceReferenceNumber = cusTransportMeans.TPM_ReferenceNumber.ReturnNullIfEmpty();
		Nationality = cusTransportMeans.TPM_RN_NKTransportNationality.ReturnNullIfEmpty();
		CustomsOfficeAtBorderReferenceNumber = cusTransportMeans.TPM_CustomsOffice;
	}

	public int SequenceNumber { get; }

	public string ConveyanceReferenceNumber { get; }

	public string CustomsOfficeAtBorderReferenceNumber { get; }

	public string Nationality { get; }

	public string IdentificationNumber { get; }

	public string TypeOfIdentification { get; }
}
