using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARCreditNoteApprovalRequestCollection : TransactionApprovalRequestCollection<ARCreditNoteApprovalRequest>
	{
		public ARCreditNoteApprovalRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ARCreditNoteApprovalRequestCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override string[] GetApprovalType()
		{
			return new[] { Constants.GenApprovalRequestApprovalType.ARCreditNote, Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal };
		}
	}
}