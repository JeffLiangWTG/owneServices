using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalController))]
	class TransactionPendingAllocationApprovalControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(TransactionPendingAllocationApprovalRequest);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TransactionsPendingAllocationApproval;
		}
	}
}
