using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal abstract class GEPRequestMessage : IGEPRequestMessage
	{
		string IGEPRequestMessage.MessageTypeDescription => GetMessageTypeDescription();

		string IGEPRequestMessage.MessageReferenceNumber => GetMessageReferenceNumber();

		ZGuid IGEPRequestMessage.PK => GetPK();

		ZString IGEPRequestMessage.TablePrefix => GetTablePrefix();

		GlobalElectronicPayment.GlobalElectronicPayment IGEPRequestMessage.CreateGEPMessage() => CreateGEPMessage();

		IEPaymentDeliveryContextValueProvider IGEPRequestMessage.GetPaymentDeliveryContextValueProvider() => GetPaymentDeliveryContextValueProvider();

		void IGEPRequestMessage.PerformAfterCreatingEDIMessageAndInterchangeSuccessfully() => PerformAfterCreatingEDIMessageAndInterchangeSuccessfully();

		void IGEPRequestMessage.SetToErrorStatus(string errorMessage) => SetToErrorStatus(errorMessage);

		ITransactionParticipant IGEPRequestMessage.GetTransactionParticipant() => GetTransactionParticipant();

		protected abstract string GetMessageTypeDescription();

		protected abstract string GetMessageReferenceNumber();

		protected abstract ZGuid GetPK();

		protected abstract ZString GetTablePrefix();

		protected abstract GlobalElectronicPayment.GlobalElectronicPayment CreateGEPMessage();

		protected abstract IEPaymentDeliveryContextValueProvider GetPaymentDeliveryContextValueProvider();

		protected abstract void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully();

		protected abstract ITransactionParticipant GetTransactionParticipant();

		void SetToErrorStatus(string errorMessage)
		{
			var bizo = ReloadInNewFactory();
			SetToErrorStatusCore(bizo, errorMessage);
			bizo.Factory.Save();
		}

		protected abstract void SetToErrorStatusCore(BusinessObject bizo, string errorMessage);

		protected BusinessObject ReloadInNewFactory()
		{
			var factory = new BusinessObjectFactory();
			return factory.Load(GetTablePrefix(), GetPK());
		}
	}
}
