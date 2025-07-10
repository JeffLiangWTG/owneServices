using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class IETS215DataProvider : PNTSMessageHeaderProvider, IIETS215MessageHeader
{
	public IETS215DataProvider(TemporaryStorageMessageSendingObject messageSendingAction) : base(messageSendingAction)
	{
	}

	public string RefToMessageId => ZString.Empty;

	public string LanguageCode => Core.SharedConstants.Languages.English;

	public string Lrn => temporaryStorageHeader.LRN;

	public string Mrn => temporaryStorageHeader.PreviousDocuments.FirstOrDefault()?.CSI_ReferenceNumber ?? ZString.Empty;

	public DateTime DeclarationDate => MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind();

	public IPNTSPartyWithNameAndCommunications Declarant => declarant ??= new PNTSDeclarantProvider(temporaryStorageHeader.Declarant);
	IPNTSPartyWithNameAndCommunications declarant;

	public IPNTSRepresentative Representative => representative ??= temporaryStorageHeader.Representative != null ? new PNTSRepresentativeProvider(temporaryStorageHeader) : null;
	IPNTSRepresentative representative;

	public IIETS215ConsignmentHeaderMasterLevel ConsignmentHeaderMasterLevel => consignmentHeaderMasterLevel ??= new IETS215ConsignmentHeaderMasterLevelProvider(temporaryStorageHeader);
	IIETS215ConsignmentHeaderMasterLevel consignmentHeaderMasterLevel;
}
