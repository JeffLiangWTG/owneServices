using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public abstract class ImportMessageHeaderProvider : IImportMessageHeader
	{
		protected ImportMessageHeaderProvider(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;

		public abstract string MessageGroup { get; }

		public string InterchangeRecipientID => Declaration.JE_CustomsOffice;

		public IPartyID InterchangeSender => SenderAndBinDetails.Sender;

		public string AuthorisationNumber => SenderAndBinDetails.Bin;

		(IPartyID Sender, string Bin) SenderAndBinDetails
		{
			get
			{
				if (!senderAndBinDetails.HasValue)
				{
					senderAndBinDetails = EORIHelper.GetImportMessageSenderAndBinDetails(Declaration);
				}
				return senderAndBinDetails.Value;
			}
		}
		(IPartyID interchangeSender, string authorisationNumber)? senderAndBinDetails;

		public abstract IImportHeader Header { get; }

		public IDateAndTime PreparationDateAndTimeCET => preparationDateAndTimeCET ?? (preparationDateAndTimeCET = new CentralEuropeanStandardDateAndTimeProvider(true));
		IDateAndTime preparationDateAndTimeCET;
	}
}
