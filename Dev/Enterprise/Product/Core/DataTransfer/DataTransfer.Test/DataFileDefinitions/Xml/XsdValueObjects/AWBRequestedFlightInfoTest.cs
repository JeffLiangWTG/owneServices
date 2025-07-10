using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(AWBRequestedFlightInfo))]
	sealed class AWBRequestedFlightInfoTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				14, typeof(AWBRequestedFlightInfo).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			Xsd.AWBRequestedFlightInfo requestedFlightInfo = new Xsd.AWBRequestedFlightInfo();
			AssertEquals("Should not be specified by default", false, requestedFlightInfo.IsSpecified);

			requestedFlightInfo.FlightNo = "ABC";
			AssertEquals("Should be specified if flight no is not empty", true, requestedFlightInfo.IsSpecified);

			requestedFlightInfo.FlightNo = "";
			requestedFlightInfo.Carrier = "lala";
			AssertEquals("Should be specified if carrier is not empty", true, requestedFlightInfo.IsSpecified);
		}
	}
}
