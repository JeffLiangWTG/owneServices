using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal class QuoteMessageBuilder : GEPRequestMessage
	{
		internal QuoteMessageBuilder(AccEPaymentQuote quote)
		{
			Argument.NotNull(quote, nameof(quote));
			Quote = quote;
		}

		AccEPaymentQuote Quote { get; }

		GlobalElectronicPayment.GlobalElectronicPayment GEPMessage => gepMessage ?? (gepMessage = new AccEPaymentQuoteToGEPConverter().ConvertQuoteToGEP(Quote));
		GlobalElectronicPayment.GlobalElectronicPayment gepMessage;

		#region IGEPMessageProcessRequest

		protected override string GetMessageTypeDescription() => Res.GetString("b536196d-0271-4b77-9c00-38268c23b0fc", "quote");

		protected override string GetMessageReferenceNumber() => Quote.QU_InternalReference;

		protected override GlobalElectronicPayment.GlobalElectronicPayment CreateGEPMessage() => GEPMessage;

		protected override IEPaymentDeliveryContextValueProvider GetPaymentDeliveryContextValueProvider() => Quote;

		protected override ITransactionParticipant GetTransactionParticipant() => Quote.Factory;

		protected override void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully() => Quote.QU_Status = QuoteStatusCodes.Requested;

		protected override void SetToErrorStatusCore(BusinessObject bizo, string errorMessage)
		{
			var quote = (AccEPaymentQuote)bizo;
			quote.QU_Status = QuoteStatusCodes.Failed;
			var maxLength = AccEPaymentQuoteSchema.QU_ErrorDescription.MaxLength;
			quote.QU_ErrorDescription = errorMessage.Length > maxLength ? errorMessage.Substring(0, maxLength) : errorMessage;
		}

		protected override ZGuid GetPK() => Quote.PK;

		protected override ZString GetTablePrefix() => AccEPaymentQuoteSchema.Constants.Prefix;

		#endregion
	}
}
