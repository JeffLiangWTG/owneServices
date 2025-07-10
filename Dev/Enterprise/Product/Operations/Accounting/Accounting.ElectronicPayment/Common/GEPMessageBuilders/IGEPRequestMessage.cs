using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal interface IGEPRequestMessage
	{
		string MessageTypeDescription { get; }
		string MessageReferenceNumber { get; }
		ZGuid PK { get; }
		ZString TablePrefix { get; }
		GlobalElectronicPayment.GlobalElectronicPayment CreateGEPMessage();
		IEPaymentDeliveryContextValueProvider GetPaymentDeliveryContextValueProvider();
		void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully();
		void SetToErrorStatus(string errorMessage);
		ITransactionParticipant GetTransactionParticipant();
	}
}
