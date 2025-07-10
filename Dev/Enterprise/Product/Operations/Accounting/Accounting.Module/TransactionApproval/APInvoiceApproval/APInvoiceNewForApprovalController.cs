using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class APInvoiceNewForApprovalController : APInvoiceController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APInvoiceApproval_NewInvoice; }
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
			get { return ControllerIDs.APInvoiceNewForApproval; }
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return bizObject is MultipleReversingProviderForHeader ? Env.Security.None : CheckPointForDelete;
		}

		protected override IEnumerable<ControllerID> GetValidControllerIDCollection()
		{
			return new List<ControllerID> { ControllerIDs.APInvoice, IDCore, ControllerIDs.APIncompleteInvoice };
		}
	}
}
