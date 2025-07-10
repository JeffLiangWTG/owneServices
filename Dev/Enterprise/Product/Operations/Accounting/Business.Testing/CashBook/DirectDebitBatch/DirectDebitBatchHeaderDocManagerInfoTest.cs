using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	[TestedType(typeof(DirectDebitBatchHeaderDocManagerInfo))]
	public class DirectDebitBatchHeaderDocManagerInfoTest : AccountingDocManagerInfoTest
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<DirectDebitBatchHeader>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			DirectDebitBatchHeader batchHeader = Factory.New<DirectDebitBatchHeader>();
			APPayment trans1 = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			ARPayment trans2 = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment trans3 = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			batchHeader.Lines.Add(trans1);
			batchHeader.Lines.Add(trans2);
			batchHeader.Lines.Add(trans3);
			return batchHeader;
		}

		public void TestRelatedObjects()
		{
			AssertEquals(3, ((DirectDebitBatchHeader)GetPopulatedParentBusinessObject()).DocManagerInfo.RelatedObjects.Length);
		}
	}
}
