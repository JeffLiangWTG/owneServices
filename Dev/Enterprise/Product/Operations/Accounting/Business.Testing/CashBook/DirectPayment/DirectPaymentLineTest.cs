using System;
using Enterprise.Accounting.Business.CashBook.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	[TestedType(typeof(DirectPaymentLine))]
	class DirectPaymentLineTest : DirectTransactionLineBaseTest
	{
		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(DirectPayment);
		}

		public void TestInvertSigns()
		{
			AssertEquals(true, ((DirectPaymentLine)TestBizO).InvertSigns_ForTestOnly);
		}

		public void TestLineType()
		{
			AssertEquals(ZArchitecture.Core.TransactionTypes.DirectPayment, ((DirectPaymentLine)TestBizO).LineType_ForTestOnly);
		}
	}
}
