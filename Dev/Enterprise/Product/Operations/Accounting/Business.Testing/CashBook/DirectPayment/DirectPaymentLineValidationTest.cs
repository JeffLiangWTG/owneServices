using System;
using Enterprise.Accounting.Business.CashBook.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	[TestedType(typeof(DirectPaymentLine))]
	class DirectPaymentLineValidationTest : DirectTransactionLineBaseValidationTest
	{
		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(DirectPayment);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DirectPayment testDirectPayment = Factory.NewWithValidTestData<DirectPayment>();
			testDirectPayment.Lines.Add(Factory.NewWithValidTestData(GetExpectedBusinessObjectType()));
			TestBizO = (DirectPaymentLine)testDirectPayment.Lines[0];
		}
	}
}
