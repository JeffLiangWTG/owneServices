using System;
using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Transaction
{
	public class APIncompleteCreditNotesController : APIncompleteTransactionsWithApprovalRequestsController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APIncompleteInvoices; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesCreditNote; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APIncompleteCreditNote; }
		}

		protected override IEnumerable<ControllerID> GetRelatedControllerIDs()
			=> new[] { ControllerIDs.APCreditNoteLinkedToApproval, ControllerIDs.APCreditNoteNewForApproval };

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APCreditNote); }
		}
	}
}
