using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	class RF415HeaderTypeProvider : IRF415HeaderType
	{
		public RF415HeaderTypeProvider(RF415MessageSendingObject sendingObject)
		{
			this.sendingAction = sendingObject;
		}

		readonly RF415MessageSendingObject sendingAction;

		public string ApplicationReferenceId => AISOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder;

		public string ApplicationDecisionCodeType => sendingAction.RefundType;

		public string Signature => null;

		public string TotalNumberOfDocuments => sendingAction.DocumentSendingObjectCollection.Count.ToString();
	}
}
