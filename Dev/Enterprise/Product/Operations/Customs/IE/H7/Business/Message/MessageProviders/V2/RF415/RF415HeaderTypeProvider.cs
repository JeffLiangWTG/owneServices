using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public  class RF415HeaderTypeProvider : IRF415HeaderType
	{
		public RF415HeaderTypeProvider(RF415MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
		}

		readonly RF415MessageSendingObject sendingObject;

		public string ApplicationReferenceId => AISOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder;

		public string ApplicationDecisionCodeType => sendingObject.RefundType;

		public string Signature => null;

		public int TotalNumberOfDocuments => sendingObject.DocumentSendingObjectCollection.Count;
	}
}
