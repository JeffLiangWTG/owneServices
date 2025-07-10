using System;
using Enterprise.Accounting.Business.CashBook.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing
{
	[TestedType(typeof(DirectReceiptLine))]
	class DirectReceiptLineTest : DirectTransactionLineBaseTest
	{
		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(DirectReceipt);
		}

		public void TestInvertSigns()
		{
			AssertEquals(false, ((DirectReceiptLine)TestBizO).InvertSigns_ForTestOnly);
		}

		public void TestLineType()
		{
			AssertEquals(ZArchitecture.Core.TransactionTypes.DirectReceipt, ((DirectReceiptLine)TestBizO).LineType_ForTestOnly);
		}
	}
}
