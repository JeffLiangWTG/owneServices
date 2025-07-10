using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.SailingDates))]
	sealed class SailingDatesTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.SailingDates value = null;
			value = new Xsd.SailingDatesCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.SailingDates sailingDates = new Xsd.SailingDates();
			AssertEquals("Should not be specified by default", false, sailingDates.IsSpecified);

			sailingDates.AvailableDate = ZDateTime.Now;
			AssertEquals("Should be specified", true, sailingDates.IsSpecified);
			sailingDates.AvailableDate = ZDateTime.Empty;

			sailingDates.CutOffDate = ZDateTime.Now;
			AssertEquals("Should be specified", true, sailingDates.IsSpecified);
			sailingDates.CutOffDate = ZDateTime.Empty;

			sailingDates.ReceivalCommencesDate = ZDateTime.Now;
			AssertEquals("Should be specified", true, sailingDates.IsSpecified);
			sailingDates.ReceivalCommencesDate = ZDateTime.Empty;

			sailingDates.StorageDate = ZDateTime.Now;
			AssertEquals("Should be specified", true, sailingDates.IsSpecified);
			sailingDates.StorageDate = ZDateTime.Empty;

			sailingDates.IsSpecified = false;
			AssertEquals("Should not be specified when IsSpecified set to false explicitly", false, sailingDates.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
				"If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
				18, TypeDescriptor.GetProperties(typeof(Xsd.SailingDates)).Count);
		}
	}
}
