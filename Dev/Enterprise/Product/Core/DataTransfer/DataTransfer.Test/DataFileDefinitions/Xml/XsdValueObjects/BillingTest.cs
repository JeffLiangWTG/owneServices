using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Billing))]
	sealed class BillingTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Billing billing = new Billing();

			AssertEquals("Precondition - no ChargeLines.", 0, billing.ChargeLines.Count);
			AssertEquals("With no ChargeLines, IsSpecified should be false.", false, billing.IsSpecified);

			billing.ChargeLines.AddNew();
			AssertEquals("With 1 or more ChargeLine, IsSpecified should be true.", true, billing.IsSpecified);
		}
	}
}
