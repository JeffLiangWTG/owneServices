using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalController))]
	class ARCreditNoteApprovalControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(ARCreditNoteApprovalRequest);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARCreditNoteApproval;
		}
	}
}
