																																																																																																																																																																																																																								using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.Customs.EU.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.BE.Business;

public class IETS115DataProvider : PNTSMessageHeaderProvider, IIETS115MessageHeader
{
	public IETS115DataProvider(TemporaryStorageMessageSendingObject messageSendingAction) : base(messageSendingAction)
	{
	}

	public string RefToMessageId => ZString.Empty;

	public string LanguageCode => Core.SharedConstants.Languages.English;

	public string Lrn => temporaryStorageHeader.LRN;

	public int ENSReUseIndicator => 0;

	public DateTime DeclarationDate => MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind();

	public DateTime DateAndTimeOfPresentationOfTheGoods => MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind();

	public string SupervisingCustomsOffice => temporaryStorageHeader.AMA_CustomsOffice;

	public string CustomsOfficeOfPresentationReferenceNumber => temporaryStorageHeader.PresentationCustomsOffice;

	public string PersonPresentingTheGoodsIdentificationNumber => temporaryStorageHeader.Presenter?.Header.GetConcatenatedSingleOrgCusCode(EuropeanUnionSharedCodeTypes.Eori);

	public IPNTSPartyWithNameAndCommunications Declarant => declarant ??= new PNTSDeclarantProvider(temporaryStorageHeader.Declarant);
	IPNTSPartyWithNameAndCommunications declarant;

	public IPNTSRepresentative Representative => representative ??= temporaryStorageHeader.Representative != null ? new PNTSRepresentativeProvider(temporaryStorageHeader) : null;
	IPNTSRepresentative representative;

	public IConsignmentHeaderMasterLevel ConsignmentHeaderMasterLevel => consignmentHeaderMasterLevel ??= new PNTSConsignmentHeaderMasterLevelProvider(temporaryStorageHeader);
	IConsignmentHeaderMasterLevel consignmentHeaderMasterLevel;
}
