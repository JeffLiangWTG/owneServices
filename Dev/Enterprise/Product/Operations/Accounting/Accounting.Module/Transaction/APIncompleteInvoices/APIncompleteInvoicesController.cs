using System;
using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Transaction
{
	public class APIncompleteInvoicesController : APIncompleteTransactionsWithApprovalRequestsController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APIncompleteInvoices; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesInvoice; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APIncompleteInvoice; }
		}

		protected override IEnumerable<ControllerID> GetRelatedControllerIDs()
			=> new[] { ControllerIDs.APInvoiceLinkedToApproval, ControllerIDs.APInvoiceNewForApproval };

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APInvoice); }
		}
	}
}
