using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(InvoiceCharge))]
public class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
{
	public void TestTypeDecider()
	{
		Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == typeof(InvoiceCharge));
	}
}
