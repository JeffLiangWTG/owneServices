using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class ActiveBorderMeansOfTransportWrapper : ActiveBorderTransportMeansTypeDataProviderAbstractClass, IActiveBorderMeansOfTransport
{
	ActiveBorderMeansOfTransportWrapper(string customsOfficeAtBorder, string conveyanceReferenceNumber, string typeOfIdentification, string identificationNumber, string nationality)
	{
		this.customsOfficeAtBorder = customsOfficeAtBorder;
		this.conveyanceReferenceNumber = conveyanceReferenceNumber;
		this.typeOfIdentification = typeOfIdentification;
		this.identificationNumber = identificationNumber;
		this.nationality = nationality;

		lazyTypeOfIdentification = new Lazy<int>(GetTypeOfIdentification);
	}

	static ActiveBorderMeansOfTransportWrapper NewOrNull(ZString customsOfficeAtBorder, ZString conveyanceReferenceNumber, ZString typeOfIdentification, ZString identificationNumber, ZString nationality)
	{
		if (!IsValid(customsOfficeAtBorder, conveyanceReferenceNumber, typeOfIdentification, identificationNumber, nationality))
		{
			return null;
		}
		return new ActiveBorderMeansOfTransportWrapper(customsOfficeAtBorder, conveyanceReferenceNumber, typeOfIdentification, identificationNumber, nationality);
	}

	public static IActiveBorderMeansOfTransport NewOrNull(EU.NCTS.Business.NctsDepartureMovementHeader movementHeader)
	{
		return movementHeader == null
			? null
			: NewOrNull(movementHeader.BM_CustomsOfficeAtBorder, movementHeader.BM_ConveyanceNumber, movementHeader.BM_ActiveBorderIdentificationType, movementHeader.BM_TOLCarrierID, movementHeader.BM_RN_NKTOLCarrierNationality);
	}

	public static IActiveBorderMeansOfTransport NewOrNull(EU.NCTS.Business.DepartureCusTransportMeans departureCusTransportMeans)
	{
		return departureCusTransportMeans == null
			? null
			: NewOrNull(departureCusTransportMeans.TPM_CustomsOffice, departureCusTransportMeans.TPM_ReferenceNumber, departureCusTransportMeans.TPM_TypeOfIdentification, departureCusTransportMeans.TPM_IdentificationNumber, departureCusTransportMeans.TPM_RN_NKTransportNationality);
	}

	public static ActiveBorderTransportMeansTypeDataProviderAbstractClass NewOrNullActiveBorderTransportMeansType(EU.NCTS.Business.NctsDepartureMovementHeader movementHeader)
	{
		return movementHeader is null
			? null
			: NewOrNull(movementHeader.BM_CustomsOfficeAtBorder, movementHeader.BM_ConveyanceNumber, movementHeader.BM_ActiveBorderIdentificationType, movementHeader.BM_TOLCarrierID, movementHeader.BM_RN_NKTOLCarrierNationality);
	}

	public static ActiveBorderTransportMeansTypeDataProviderAbstractClass NewOrNullActiveBorderTransportMeansType(DepartureCusTransportMeans departureCusTransportMeans)
	{
		return departureCusTransportMeans == null
			? null
			: NewOrNull(departureCusTransportMeans.TPM_CustomsOffice, departureCusTransportMeans.TPM_ReferenceNumber, departureCusTransportMeans.TPM_TypeOfIdentification, departureCusTransportMeans.TPM_IdentificationNumber, departureCusTransportMeans.TPM_RN_NKTransportNationality);
	}

	public string CustomsOfficeAtBorder => customsOfficeAtBorder;

	public override string ConveyanceReferenceNumber => conveyanceReferenceNumber;

	public override int? TypeOfIdentification => lazyTypeOfIdentification.Value;

	int IMeansOfTransport.TypeOfIdentification => lazyTypeOfIdentification.Value;

	readonly Lazy<int> lazyTypeOfIdentification;

	public override string IdentificationNumber => identificationNumber;

	public override string Nationality => nationality;

	static bool IsValid(ZString customsOfficeAtBorder, ZString conveyanceReferenceNumber, ZString typeOfIdentification, ZString identificationNumber, ZString nationality)
		=> !customsOfficeAtBorder.IsEmpty || !conveyanceReferenceNumber.IsEmpty || !typeOfIdentification.IsEmpty || !identificationNumber.IsEmpty || !nationality.IsEmpty;

	int GetTypeOfIdentification()
	{
		return int.TryParse(typeOfIdentification, out var result)
			? result
			: -1;
	}

	public override int SequenceNumber => default;

	public override string CustomOfficeAtBorderReferenceNumber => customsOfficeAtBorder;

	readonly string customsOfficeAtBorder;
	readonly string conveyanceReferenceNumber;
	readonly string typeOfIdentification;
	readonly string identificationNumber;
	readonly string nationality;
}
