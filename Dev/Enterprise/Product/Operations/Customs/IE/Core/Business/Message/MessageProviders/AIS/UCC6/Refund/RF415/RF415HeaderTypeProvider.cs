using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RF415HeaderTypeProvider : IRF415HeaderType
	{
		readonly RefundApplicationMessageSendingAction sendingAction;

		public RF415HeaderTypeProvider(RefundApplicationMessageSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
		}

		public string ApplicationReferenceId => AISOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder;

		public string ApplicationDecisionCodeType => sendingAction.RefundType;

		public string Signature => null;

		public int TotalNumberOfDocuments => sendingAction.DocumentSendingObjectCollection.Count;
	}
}
