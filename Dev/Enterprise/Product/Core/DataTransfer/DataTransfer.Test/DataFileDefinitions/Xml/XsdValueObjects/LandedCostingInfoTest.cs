using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.LandedCostingInfo))]
	sealed class LandedCostingInfoTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.LandedCostingInfo value = null;
			value = new Xsd.LandedCostingInfo();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.LandedCostingInfo landedCostingInfo = new Xsd.LandedCostingInfo();
			AssertEquals("Should not be specified by default", false, landedCostingInfo.IsSpecified);

			landedCostingInfo.LineType = "ACT";
			AssertEquals("Should be specified", false, landedCostingInfo.IsSpecified);

			landedCostingInfo.LCGroupCharges.AddNew();
			AssertEquals("Should be specified", true, landedCostingInfo.IsSpecified);

			landedCostingInfo.IsSpecified = false;
			AssertEquals("Should not be specified when IsSpecified set to false explicitly", false, landedCostingInfo.IsSpecified);
		}
	}
}
