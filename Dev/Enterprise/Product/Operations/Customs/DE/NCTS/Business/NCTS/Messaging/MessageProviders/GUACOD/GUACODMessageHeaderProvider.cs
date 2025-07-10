using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class GUACODMessageHeaderProvider : INCTSMessageHeader
	{
		public GUACODMessageHeaderProvider(SendAccessCodeViewModel sendAccessCode)
		{
			this.sendAccessCode = sendAccessCode;
		}

		public IDateAndTime PreparationDateAndTimeUtc => new UniversalDateAndTimeProvider();

		public IPartyID InterchangeSender => SenderAndBinDetails.Sender;

		public string AuthenticationNumber => SenderAndBinDetails.Bin;

		public string InterchangeRecipientID => sendAccessCode.OfficeOfGuarantee.ValueOrNullIfEmpty();

		public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

		public INCTSPartyIDContact AuthorisedConsignee => null;

		public INCTSHeader Header => header ?? (header = new GUACODHeaderProvider(sendAccessCode));
		INCTSHeader header;

		(IPartyID Sender, string Bin) SenderAndBinDetails
		{
			get
			{
				if (!senderAndBinDetails.HasValue)
				{
					var orgProxyMainAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress;
					senderAndBinDetails = (PartyIDProvider.NewOrNull(orgProxyMainAddress), orgProxyMainAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber));

					if (senderAndBinDetails.Value.interchangeSender.IsIdentificationEmpty() || string.IsNullOrEmpty(senderAndBinDetails.Value.authenticationNumber))
					{
						senderAndBinDetails = EORIHelper.GetSenderDetailsFromRegistry();
					}
				}
				return senderAndBinDetails.Value;
			}
		}
		(IPartyID interchangeSender, string authenticationNumber)? senderAndBinDetails;

		readonly SendAccessCodeViewModel sendAccessCode;
	}
}
