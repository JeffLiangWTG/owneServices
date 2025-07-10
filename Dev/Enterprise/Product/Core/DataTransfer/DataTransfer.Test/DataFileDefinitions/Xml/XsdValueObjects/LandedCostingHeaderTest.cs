using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.LandedCostingHeader))]
	sealed class LandedCostingHeaderTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.LandedCostingHeader value = null;
			value = new Xsd.LandedCostingHeader();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.LandedCostingHeader landedCostingHeader = new Xsd.LandedCostingHeader();
			AssertEquals("Should not be specified by default", false, landedCostingHeader.IsSpecified);

			landedCostingHeader.LandedCostingDate = ZDateTime.Now;
			AssertEquals("Should be specified", false, landedCostingHeader.IsSpecified);

			landedCostingHeader.LandedCostingGroupHeaders.AddNew();
			AssertEquals("Should be specified", true, landedCostingHeader.IsSpecified);

			landedCostingHeader.IsSpecified = false;
			AssertEquals("Should not be specified when IsSpecified set to false explicitly", false, landedCostingHeader.IsSpecified);
		}
	}
}
