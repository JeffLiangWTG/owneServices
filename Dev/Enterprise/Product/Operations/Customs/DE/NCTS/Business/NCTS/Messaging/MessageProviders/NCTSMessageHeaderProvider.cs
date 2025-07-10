using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public abstract class NCTSMessageHeaderProvider<T> : INCTSMessageHeader where T : INCTSHeader
	{
		protected readonly NctsHeader nctsHeader;

		protected NCTSMessageHeaderProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

		public IPartyID InterchangeSender => SenderAndBinDetails.Sender;

		public string AuthenticationNumber => SenderAndBinDetails.Bin;

		public string InterchangeRecipientID
		{
			get
			{
				string interchangeRecipient;
				if (nctsHeader.IsArrivalMovement)
				{
					interchangeRecipient = nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCode.ValueOrNullIfEmpty();
				}
				else
				{
					interchangeRecipient = nctsHeader.MovementHeader.DepartureCustomsOfficeCode.ValueOrNullIfEmpty();
				}
				return interchangeRecipient;
			}
		}

		public INCTSPartyIDContact AuthorisedConsignee => authorisedConsignee ?? (authorisedConsignee = NCTSAuthorisedConsigneeProvider.NewOrNull(nctsHeader.DestinationTrader));
		INCTSPartyIDContact authorisedConsignee;

		public IDateAndTime PreparationDateAndTimeUtc => preparationDateAndTimeCET ?? (preparationDateAndTimeCET = new UniversalDateAndTimeProvider());
		IDateAndTime preparationDateAndTimeCET;

		public INCTSHeader Header => header ?? (header = (INCTSHeader)Activator.CreateInstance(typeof(T), nctsHeader));

		protected INCTSHeader header;

		(IPartyID Sender, string Bin) SenderAndBinDetails
		{
			get
			{
				if (!senderAndBinDetails.HasValue)
				{
					if (nctsHeader.IsArrivalMovement)
					{
						senderAndBinDetails = (PartyIDProvider.NewOrNull(nctsHeader.DestinationTrader),
												nctsHeader.DestinationTrader.Address.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber));
					}
					else
					{
						senderAndBinDetails = (PartyIDProvider.NewOrNull(nctsHeader.Principal),
												nctsHeader.Principal.Address.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber));
					}
					if (senderAndBinDetails.Value.interchangeSender.IsIdentificationEmpty() || string.IsNullOrEmpty(senderAndBinDetails.Value.authenticationNumber))
					{
						senderAndBinDetails = EORIHelper.GetSenderDetailsFromRegistry();
					}
				}
				return senderAndBinDetails.Value;
			}
		}
		(IPartyID interchangeSender, string authenticationNumber)? senderAndBinDetails;
	}
}
