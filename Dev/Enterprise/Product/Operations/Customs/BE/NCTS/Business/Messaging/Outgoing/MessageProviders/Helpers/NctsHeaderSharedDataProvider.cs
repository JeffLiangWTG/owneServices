using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class NctsHeaderSharedDataProvider : INCTSMessageHeader
	{
		protected readonly NctsHeader nctsHeader;

		public NctsHeaderSharedDataProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public string CustomsOfficeOfDeparture => nctsHeader.CommonMovementHeader.DepartureCustomsOfficeCode;

		public virtual string CustomsOfficeOfDestination => nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCode;

		public string MRN => nctsHeader.MovementReferenceNumber;

		public string MessageSender => BECustomsRegistry.Instance.SenderIDs.GetTargetSystemName(Constants.MessageVersionRegistryDomainCodes.NCTSP5);

		public string MessageRecipient => BECustomsRegistry.Instance.CustomsMessageVersion.GetTargetSystemName(Constants.MessageVersionRegistryDomainCodes.NCTSP5);

		public DateTime PreparationDateTime => DateTime.ParseExact(ZDateTime.UtcNow.ToBestReadableDateTimeString(), ZDateTime.BestReadableDateTimeFormat, null);

		public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

		public virtual string MessageType => ZString.Empty;

		public virtual string CorrelationIdentifier
		{
			get
			{
				var correlationIdentifier = nctsHeader.CorrelationIdentifierEntryNumber.CE_EntryLineReference;
				if (correlationIdentifier.IsEmpty)
				{
					correlationIdentifier = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.BM_PaperlessInbondNum : nctsHeader.IsArrivalMovement ? nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum : ZString.Empty;
				}

				return correlationIdentifier;
			}
		}
	}
}
