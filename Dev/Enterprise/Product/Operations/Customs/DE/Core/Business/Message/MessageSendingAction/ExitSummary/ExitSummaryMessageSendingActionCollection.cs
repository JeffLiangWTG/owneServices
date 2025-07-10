using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ExitSummaryMessageSendingActionCollection : MessageSendingActionCollection
	{
		public ExitSummaryMessageSendingActionCollection(CusExitControlHeader exitHeader, IEnumerable<BusinessObject> messagingObjects)
			: base(messagingObjects, x => ((CusExitDetail)x).CED_MovementReferenceNumber, exitHeader.Factory)
		{
		}

		public ExitSummaryMessageSendingActionCollection(CusExitControlHeader exitHeader) : this(exitHeader, exitHeader.CusExitDetails)
		{
		}

		public new ExitSummaryMessageSendingAction AddNew() => (ExitSummaryMessageSendingAction)base.AddNew();

		public new ExitSummaryMessageSendingAction this[int index] => (ExitSummaryMessageSendingAction)base[index];

		public ExitSummaryMessageSendingAction AddNew(CusExitDetail exitDetail)
		{
			var result = (ExitSummaryMessageSendingAction)GetSendingAction(exitDetail);
			Add(result);
			return result;
		}

		protected override MessageSendingAction GetSendingAction(BusinessObject messagingObject) => new ExitSummaryMessageSendingAction((CusExitDetail)messagingObject);
	}
}
