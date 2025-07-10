using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public abstract class MonthlyClosingMessageHeaderProvider : IImportMessageHeader
	{
		protected MonthlyClosingMessageHeaderProvider(CusReconDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		protected readonly CusReconDeclaration Declaration;

		public abstract string MessageGroup { get; }

		public IPartyID InterchangeSender => SenderAndBinDetails.Sender;

		public string InterchangeRecipientID => Declaration.CRD_CustomsOffice;

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
