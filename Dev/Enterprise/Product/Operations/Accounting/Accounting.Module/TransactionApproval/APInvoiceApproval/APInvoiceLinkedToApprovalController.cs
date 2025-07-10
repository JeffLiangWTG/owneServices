using System;
using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class APInvoiceLinkedToApprovalController : APTransactionsLinkedToApprovalController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APInvoiceApproval_NewInvoice; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APInvoiceLinkedToApproval; }
		}

		protected override IEnumerable<ControllerID> GetRelatedControllerIDs()
			=> new[] { ControllerIDs.APIncompleteInvoice, ControllerIDs.APInvoiceNewForApproval };

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APInvoice); }
		}

		protected override IEnumerable<ControllerID> GetValidControllerIDCollection()
		{
			return new List<ControllerID> { ID, ControllerIDs.APIncompleteInvoice };
		}
	}
}
