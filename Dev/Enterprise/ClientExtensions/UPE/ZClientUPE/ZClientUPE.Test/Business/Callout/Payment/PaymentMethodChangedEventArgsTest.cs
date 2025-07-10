using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class PaymentMethodChangedEventArgsTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			PaymentMethodChangedEventArgs e = new PaymentMethodChangedEventArgs(UPECargoPaymentMethod.CreditCard);
			AssertEquals(UPECargoPaymentMethod.CreditCard, e.PaymentMethod);
		}
	}
}
