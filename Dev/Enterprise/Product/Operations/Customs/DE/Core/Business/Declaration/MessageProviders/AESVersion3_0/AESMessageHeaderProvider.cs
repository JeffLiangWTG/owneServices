using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public abstract class AESMessageHeaderProvider : IAESMessageHeader
	{
		protected AESMessageHeaderProvider(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Declaration = Argument.NotNull(entryHeader.Declaration, "entryHeader.Declaration");
		}
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;

		public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

		public string InterchangeRecipientID => Declaration.JE_CustomsOffice;

		public IPartyID InterchangeSender => SenderAndBinDetails.Sender;

		public string AuthorizationNumber => SenderAndBinDetails.Bin;

		public abstract IAESHeader AESHeader { get; }

		(IPartyID Sender, ZString Bin) SenderAndBinDetails
		{
			get
			{
				if (!senderAndBinDetails.HasValue)
				{
					var sendRepresentative = EntryHeader.EntryInstruction?.ZG_PartyConstellation.IsDigitCheckSatisfied(2, '1') ?? false;
					var declaration = EntryHeader.Declaration;
					var senderAddress = sendRepresentative ? declaration.Representative : declaration.Declarant;
					var api = senderAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber);

					senderAndBinDetails = api.IsEmpty ? EORIHelper.GetSenderDetailsFromRegistry() : (PartyIDProvider.NewOrNull(senderAddress), api);
				}
				return senderAndBinDetails.Value;
			}
		}

		public IDateAndTime PreparationDateAndTimeUtc => preparationDateTimeUtc ?? (preparationDateTimeUtc = new UniversalDateAndTimeProvider(true));
		IDateAndTime preparationDateTimeUtc;

		(IPartyID interchangeSender, ZString authorisationNumber)? senderAndBinDetails;
	}
}
