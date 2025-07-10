using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(MilestoneDates))]
	sealed class MilestoneDatesTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.MilestoneDates milestoneDates = new Xsd.MilestoneDates();
			AssertEquals(false, milestoneDates.IsSpecified);

			milestoneDates.Estimated = ZDateTime.Now;
			AssertEquals(true, milestoneDates.IsSpecified);

			milestoneDates.Estimated = ZDateTime.Empty;
			milestoneDates.Actual = ZDateTime.Now;
			AssertEquals(true, milestoneDates.IsSpecified);

			milestoneDates.Actual = ZDateTime.Empty;
			AssertEquals(false, milestoneDates.IsSpecified);
		}

		public void TestFromEstimatedAndActual()
		{
			ZDateTime estimatedDateTime = new ZDateTime(2005, 1, 1);
			ZDateTime actualDateTime = new ZDateTime(2005, 2, 2);

			Xsd.MilestoneDates milestoneDates = Xsd.MilestoneDates.FromEstimatedAndActual(ZDateTime.Empty, actualDateTime);
			AssertNotNull(milestoneDates);
			AssertEquals(false, milestoneDates.Estimated.IsValid);
			AssertEquals(actualDateTime, milestoneDates.Actual);

			milestoneDates = Xsd.MilestoneDates.FromEstimatedAndActual(estimatedDateTime, actualDateTime);
			AssertNotNull(milestoneDates);
			AssertEquals(estimatedDateTime, milestoneDates.Estimated);
			AssertEquals(true, milestoneDates.Actual.IsValid);
			AssertEquals(actualDateTime, milestoneDates.Actual);

			milestoneDates = Xsd.MilestoneDates.FromEstimatedAndActual(estimatedDateTime, ZDateTime.Empty);
			AssertNotNull("FromEstimatedAndActual using DateTime", milestoneDates);
			AssertEquals(estimatedDateTime, milestoneDates.Estimated);
			AssertEquals(false, milestoneDates.Actual.IsValid);
		}
	}
}
