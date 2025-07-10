using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6A93NumberPaymentTest : TestCase
{
	public void TestUcc6A93NumberPayment()
	{
		var feeOneMock = new Mock<IFee>();
		var fees = new[] { feeOneMock.Object };
		var ucc6A93NumberPayment = new Ucc6A93NumberPayment("E", new ZDateTime(2022, 01, 01), "Hello", fees);

		AssertNotNull(nameof(Ucc6A93NumberPayment), ucc6A93NumberPayment);
		AssertEquals(nameof(Ucc6A93NumberPayment.PaymentType), "E", ucc6A93NumberPayment.PaymentType);
		AssertEquals(nameof(Ucc6A93NumberPayment.PaymentDate), new ZDate(2022, 01, 01), ucc6A93NumberPayment.PaymentDate);
		AssertEquals(nameof(Ucc6A93NumberPayment.PaymentResponseNo), "Hello", ucc6A93NumberPayment.PaymentResponseNo);
		AssertNotNull(nameof(Ucc6A93NumberPayment.AmountCalculator), ucc6A93NumberPayment.AmountCalculator);
		AssertType<Ucc6A93NumberAmountCalculator>(nameof(Ucc6A93NumberPayment.AmountCalculator), ucc6A93NumberPayment.AmountCalculator);
	}
}
