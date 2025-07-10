using System;
using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class APCreditNoteLinkedToApprovalController : APTransactionsLinkedToApprovalController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APInvoiceApproval_NewCreditNote; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APCreditNoteLinkedToApproval; }
		}

		protected override IEnumerable<ControllerID> GetRelatedControllerIDs()
			=> new[] { ControllerIDs.APIncompleteCreditNote, ControllerIDs.APCreditNoteNewForApproval };

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APCreditNote); }
		}

		protected override IEnumerable<ControllerID> GetValidControllerIDCollection()
		{
			return new List<ControllerID> { ID, ControllerIDs.APIncompleteCreditNote };
		}
	}
}
