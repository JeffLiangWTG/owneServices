using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(APInvoiceLinkedToApprovalController))]
	public class APInvoiceLinkedToApprovalControllerTest : APTransactionsLinkedToApprovalControllerTest<APInvoice, InvoiceForm>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APInvoiceLinkedToApproval;
		}

		protected override ControllerID GetRelativeControllerID()
		{
			return ControllerIDs.APIncompleteInvoice;
		}

		protected override APInvoice GetBusinessObject()
		{
			return TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APInvoice>(TestObjectCreator.Creditor1, 100);
		}

		protected override ControllerID ReportInvalidDataSource_CorrectControllerID => ControllerIDs.APInvoice;

		protected override ZController GetController() => Controller as APInvoiceLinkedToApprovalController;

		protected override IEnumerable<ControllerID> EditFormControllerIDs => new[] { ControllerIDs.APIncompleteInvoice, ControllerIDs.APInvoiceLinkedToApproval };

		protected override ControllerID NewFormControllerID => ControllerIDs.APInvoiceNewForApproval;
	}
}
