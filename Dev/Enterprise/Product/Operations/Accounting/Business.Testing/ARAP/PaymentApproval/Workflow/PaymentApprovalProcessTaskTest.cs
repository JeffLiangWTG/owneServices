using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class PaymentApprovalProcessTaskTest : ProcessTaskTest
	{
		public void TestParentCotrollerID()
		{
			var paymentApproval = Factory.New(GetExpectedBusinessObjectTypeForPaymentApproval()) as PaymentApprovalBase;
			var processTask = ((IWorkflowProvider)paymentApproval).WorkflowItems.AddNew();
			processTask = processTask as PaymentApprovalProcessTask;
			AssertNotNull(processTask);
			AssertEquals(paymentApproval.AV_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable ? ControllerIDs.ARPaymentProcessing : ControllerIDs.APPaymentProcessing, processTask.ParentControllerID);
		}

		public void TestParentCotrollerIDWithoutParent()
		{
			var task = Factory.New<PaymentApprovalProcessTask>();
			AssertEquals("ParentControllerID", null, task.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var paymentApproval = Factory.New(GetExpectedBusinessObjectTypeForPaymentApproval());
			return ((IWorkflowProvider)paymentApproval).WorkflowItems.AddNew();
		}

		protected abstract Type GetExpectedBusinessObjectTypeForPaymentApproval();
	}

	[TestedType(typeof(PaymentApprovalProcessTask))]
	public class PaymentApprovalProcessTaskTest_ForAPPaymentApprovalWithAuthorisation : PaymentApprovalProcessTaskTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(APPaymentApprovalWithAuthorisation);
	}

	[TestedType(typeof(PaymentApprovalProcessTask))]
	public class PaymentApprovalProcessTaskTest_ForARPaymentApprovalWithAuthorisation  : PaymentApprovalProcessTaskTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(ARPaymentApprovalWithAuthorisation);
	}

	[TestedType(typeof(PaymentApprovalProcessTask))]
	public class PaymentApprovalProcessTaskTest_ForARPaymentApprovalWithoutAuthorisation : PaymentApprovalProcessTaskTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(ARPaymentApprovalWithoutAuthorisation);
	}

	[TestedType(typeof(PaymentApprovalProcessTask))]
	public class PaymentApprovalProcessTaskTest_ForAPPaymentApprovalWithoutAuthorisation : PaymentApprovalProcessTaskTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(APPaymentApprovalWithoutAuthorisation);
	}
}
