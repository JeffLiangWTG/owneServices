using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.FlightWithFlightNumber))]
	sealed class FlightWithFlightNumberTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.FlightWithFlightNumber sailing = new Xsd.FlightWithFlightNumber();
			AssertEquals("Should not be specified by default", false, sailing.IsSpecified);

			sailing.FlightNoJourneyNoTruckRegNo = "Number";
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.FlightNoJourneyNoTruckRegNo = "";

			sailing.Dates.ReceivalCommencesDate = ZDateTime.Now;
			AssertEquals("Should be specified", true, sailing.IsSpecified);
			sailing.Dates.ReceivalCommencesDate = ZDateTime.Empty;

			sailing.IsSpecified = false;
			AssertEquals("Should not be specified when IsSpecified set to false explicitly", false, sailing.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
				"If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
				56, TypeDescriptor.GetProperties(typeof(Xsd.FlightWithFlightNumber)).Count);
			// Henry needs to fix this
		}
	}
}
