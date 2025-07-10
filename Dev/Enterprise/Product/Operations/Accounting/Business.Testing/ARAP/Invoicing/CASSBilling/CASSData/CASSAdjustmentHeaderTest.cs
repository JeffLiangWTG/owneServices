using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CASSAdjustmentHeader))]
	public class CASSAdjustmentHeaderTest : CASSDataTest
	{
		public void TestInvoicePeriod()
		{
			var adjHeader = new CASSAdjustmentHeader();
			adjHeader.BillingPeriodEnd = new ZDateTime(2015, 03, 12);
			AssertEquals("Invoice Period for [12 Mar 2015]", 201505, adjHeader.InvoicePeriod);

			adjHeader.BillingPeriodEnd = new ZDateTime(2015, 03, 22);
			AssertEquals("Invoice Period for [22 Mar 2015]", 201506, adjHeader.InvoicePeriod);

			adjHeader.BillingPeriodEnd = new ZDateTime(2014, 09, 01);
			AssertEquals("Invoice Period for [01 Sep 2014]", 201417, adjHeader.InvoicePeriod);
		}

		protected override CASSData GetCASSData()
		{
			return new CASSAdjustmentHeader();
		}
	}

	[TestedType(typeof(CASSAdjustmentHeader))]
	public class CASSAdjustmentHeaderValueObjectTest : ValueObjectTestCase
	{
	}
}
