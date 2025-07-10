using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public abstract class ExitMessageHeaderProvider : IExitMessageHeader
	{
		public ExitMessageHeaderProvider(CusExitReport cusExitReport)
		{
			this.cusExitReport = Argument.NotNull(cusExitReport, nameof(cusExitReport));
		}

		public IDateAndTime PreparationDateAndTimeUtc => preparationDateAndTimeUtc ?? (preparationDateAndTimeUtc = new UniversalDateAndTimeProvider());
		IDateAndTime preparationDateAndTimeUtc;

		public IPartyID InterchangeSender => SenderAndBinDetails.Sender;

		public string AuthorizationNumber => SenderAndBinDetails.Bin;

		public string InterchangeRecipientID => cusExitReport.CER_OfficeOfExit.ValueOrNullIfEmpty();

		public abstract IExitHeader ExitHeader { get; }

		public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

		(IPartyID Sender, ZString Bin) SenderAndBinDetails
		{
			get
			{
				if (!senderAndBinDetails.HasValue)
				{
					var senderAddress = cusExitReport.Header.Carrier;
					var api = senderAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber);

					senderAndBinDetails = api.IsEmpty ? EORIHelper.GetSenderDetailsFromRegistry() : (PartyIDProvider.NewOrNull(senderAddress), api);
				}
				return senderAndBinDetails.Value;
			}
		}

		(IPartyID interchangeSender, ZString authorizationNumber)? senderAndBinDetails;

		protected readonly CusExitReport cusExitReport;
	}
}
