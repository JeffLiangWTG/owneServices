using System;
using Enterprise.Accounting.Business.CashBook.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing
{
	[TestedType(typeof(DirectReceiptLine))]
	class DirectReceiptLineValidationTest : DirectTransactionLineBaseValidationTest
	{
		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(DirectReceipt);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DirectReceipt testDirectReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			testDirectReceipt.Lines.Add(Factory.NewWithValidTestData(GetExpectedBusinessObjectType()));
			TestBizO = (DirectReceiptLine)testDirectReceipt.Lines[0];
		}
	}
}
