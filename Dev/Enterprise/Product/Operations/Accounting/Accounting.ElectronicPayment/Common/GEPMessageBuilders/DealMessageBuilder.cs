using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	class DealMessageBuilder : GEPRequestMessage
	{
		internal DealMessageBuilder(AccEPaymentDeal deal)
		{
			Argument.NotNull(deal, nameof(deal));
			Deal = deal;
		}

		AccEPaymentDeal Deal { get; }

		GlobalElectronicPayment.GlobalElectronicPayment GEPMessage => gepMessage ?? (gepMessage = new AccEPaymentDealToGEPConverter().ConvertDealToGEP(Deal));
		GlobalElectronicPayment.GlobalElectronicPayment gepMessage;

		#region IGEPMessageProcessRequest

		protected override string GetMessageTypeDescription() => Res.GetString("1e0d4f14-ea17-432d-96a6-f66462055a26", "deal");

		protected override string GetMessageReferenceNumber() => Deal.AED_InternalReference;

		protected override GlobalElectronicPayment.GlobalElectronicPayment CreateGEPMessage() => GEPMessage;

		protected override IEPaymentDeliveryContextValueProvider GetPaymentDeliveryContextValueProvider() => Deal;

		protected override ITransactionParticipant GetTransactionParticipant() => Deal.Factory;

		protected override void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully() => Deal.AED_Status = DealStatusCodes.Requested;

		protected override void SetToErrorStatusCore(BusinessObject bizo, string errorMessage)
		{
			var deal = (AccEPaymentDeal)bizo;
			deal.AED_Status = DealStatusCodes.SubmissionFailed;
			var maxLength = AccEPaymentDealSchema.AED_ErrorDescription.MaxLength;
			deal.AED_ErrorDescription = errorMessage.Length > maxLength ? errorMessage.Substring(0, maxLength) : errorMessage;
		}

		protected override ZGuid GetPK() => Deal.PK;

		protected override ZString GetTablePrefix() => AccEPaymentDealSchema.Constants.Prefix;

		#endregion
	}
}
