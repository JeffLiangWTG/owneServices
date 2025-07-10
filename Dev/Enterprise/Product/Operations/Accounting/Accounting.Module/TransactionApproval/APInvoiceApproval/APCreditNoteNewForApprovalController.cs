using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class APCreditNoteNewForApprovalController : APCreditNoteController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APInvoiceApproval_NewCreditNote; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.APInvoiceApproval_Cancel; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.APInvoiceApproval_Edit; }
		}
		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.APInvoiceApproval; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APCreditNoteNewForApproval; }
		}

		protected override IEnumerable<ControllerID> GetValidControllerIDCollection()
		{
			return new List<ControllerID> { ControllerIDs.APCreditNote, IDCore, ControllerIDs.APIncompleteCreditNote };
		}
	}
}
