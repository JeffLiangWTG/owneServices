using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APPaymentProcessingFilterBusinessObject))]
	public class APPaymentProcessingFilterBusinessObjectTest : PaymentProcessingFilterBusinessObjectTest
	{
		public void TestPaymentBatchNumber()
		{
			var batch = Factory.New<AccPaymentBatch>();
			batch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			Approval2.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			Approval2.AV_APB_PaymentBatch = batch.PK;

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBO["Payment Batch Number"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = batch.APB_BatchNumber;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));
		}

		public void TestLedgerCodeProperty()
		{
			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsPayable, GetLedgerCode());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APPaymentProcessingFilterBusinessObject();
		}

		protected override Type GetPaymentApprovalType() => typeof(APPaymentApprovalWithAuthorisation);
	}
}
