using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class CalloutChargeSchemaTest : TestCase
	{
		public void TestSchema()
		{
			AssertEquals("Amount", CalloutCharge.Schema.Amount);
			AssertEquals("NonTaxableAmount", CalloutCharge.Schema.NonTaxableAmount);
			AssertEquals("TaxableAmount", CalloutCharge.Schema.TaxableAmount);
			AssertEquals("Discount", CalloutCharge.Schema.Discount);
			AssertEquals("NettAmount", CalloutCharge.Schema.NettAmount);
			AssertEquals("GSTAmount", CalloutCharge.Schema.GSTAmount);
		}
	}
}
