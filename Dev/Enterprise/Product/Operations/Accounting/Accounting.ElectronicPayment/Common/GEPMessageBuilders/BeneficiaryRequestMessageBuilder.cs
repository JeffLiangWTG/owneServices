using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using BeneficiaryRequestStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal class BeneficiaryRequestMessageBuilder : GEPRequestMessage
	{
		internal BeneficiaryRequestMessageBuilder(AccEPaymentBeneficiaryRequest requestBizO)
		{
			Argument.NotNull(requestBizO, nameof(requestBizO));
			BeneficiaryRequest = requestBizO;
		}

		protected AccEPaymentBeneficiaryRequest BeneficiaryRequest { get; }

		GlobalElectronicPayment.GlobalElectronicPayment GEPMessage => gepMessage ?? (gepMessage = GetGEPMessageCore());
		GlobalElectronicPayment.GlobalElectronicPayment gepMessage;

		protected virtual GlobalElectronicPayment.GlobalElectronicPayment GetGEPMessageCore() =>
			new AccEPaymentBeneficiaryRequestToGEPConverter().ConvertBeneficiaryRequestToGEP(BeneficiaryRequest);

		#region IGEPMessageProcessRequest

		protected override string GetMessageTypeDescription() => Res.GetString("266f90b5-d764-498f-b92f-11f06b238b75", "beneficiary request");

		protected override string GetMessageReferenceNumber() => BeneficiaryRequest.ABR_InternalReference;

		protected override GlobalElectronicPayment.GlobalElectronicPayment CreateGEPMessage() => GEPMessage;

		protected override IEPaymentDeliveryContextValueProvider GetPaymentDeliveryContextValueProvider() => BeneficiaryRequest;

		protected override ITransactionParticipant GetTransactionParticipant() => BeneficiaryRequest.Factory;

		protected override void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully()
		{
			if (BeneficiaryRequest.ABR_Status != BeneficiaryRequestStatusCodes.Partial)
			{
				BeneficiaryRequest.ABR_Status = BeneficiaryRequestStatusCodes.Requested;
			}
		}

		protected override void SetToErrorStatusCore(BusinessObject bizo, string errorMessage)
		{
			var beneficiaryRequest = (AccEPaymentBeneficiaryRequest)bizo;
			beneficiaryRequest.ABR_Status = BeneficiaryRequestStatusCodes.Error;
			beneficiaryRequest.ABR_LastResponseReceivedUtc = ZDateTime.UtcNow;
			var maxLength = AccEPaymentBeneficiaryRequestSchema.ABR_ErrorDescription.MaxLength;
			beneficiaryRequest.ABR_ErrorDescription = errorMessage.Length > maxLength ? errorMessage.Substring(0, maxLength) : errorMessage;
		}

		protected override ZGuid GetPK() => BeneficiaryRequest.PK;

		protected override ZString GetTablePrefix() => AccEPaymentBeneficiaryRequestSchema.Constants.Prefix;

		#endregion
	}
}
