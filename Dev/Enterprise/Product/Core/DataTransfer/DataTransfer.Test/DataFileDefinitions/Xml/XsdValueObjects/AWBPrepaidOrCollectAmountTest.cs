using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(AWBPrepaidOrCollectAmount))]
	sealed class AWBPrepaidOrCollectAmountTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				26, typeof(AWBPrepaidOrCollectAmount).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			Xsd.AWBPrepaidOrCollectAmount prepaidOrCollectAmt = new Xsd.AWBPrepaidOrCollectAmount();
			AssertEquals("Should not be specified by default", false, prepaidOrCollectAmt.IsSpecified);

			prepaidOrCollectAmt.Weight = 15.3m;
			AssertEquals("Should be specified if weight is not empty", true, prepaidOrCollectAmt.IsSpecified);

			prepaidOrCollectAmt.Weight = 0m;
			prepaidOrCollectAmt.Valuation = 15.3m;
			AssertEquals("Should be specified if valuation is not empty", true, prepaidOrCollectAmt.IsSpecified);

			prepaidOrCollectAmt.Valuation = 0m;
			prepaidOrCollectAmt.Tax = 15.3m;
			AssertEquals("Should be specified if tax is not empty", true, prepaidOrCollectAmt.IsSpecified);

			prepaidOrCollectAmt.Tax = 0m;
			prepaidOrCollectAmt.OtherChargeDueAgent = 15.3m;
			AssertEquals("Should be specified if other charge due agent amount is not empty", true, prepaidOrCollectAmt.IsSpecified);

			prepaidOrCollectAmt.Tax = 0m;
			prepaidOrCollectAmt.OtherChargeDueCarrier = 15.3m;
			AssertEquals("Should be specified if other charge due carrier amount is not empty", true, prepaidOrCollectAmt.IsSpecified);
		}
	}
}
