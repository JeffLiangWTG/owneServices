using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Events))]
	sealed class EventsNumberTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Events value = null;
			value = new Xsd.EventsCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.Events value = new Xsd.Events();
			AssertEquals("IsSpecified should be false by default", false, value.IsSpecified);
			value.Event.AddNew();
			AssertEquals("IsSpecified should be true when items have been added to the collection", true, value.IsSpecified);
		}
	}
}
