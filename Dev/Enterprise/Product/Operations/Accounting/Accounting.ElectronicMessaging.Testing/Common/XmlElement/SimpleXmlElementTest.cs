using System.Linq;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class SimpleXmlElementTest : XmlElementBaseTest
	{
		public override void TestHasValue()
		{
			IXmlElement emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, string.Empty, "Valid String Value");
			Assert(!emptyTagElement.HasValue);

			emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag", null);
			Assert(!emptyTagElement.HasValue);

			emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag", "Valid String Value");
			Assert(emptyTagElement.HasValue);
		}

		public override void TestToXElements()
		{
			IXmlElement emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, string.Empty, "Valid String Value");
			AssertNull(emptyTagElement.ToXElements());

			emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag", null);
			AssertNull(emptyTagElement.ToXElements());

			IXmlElement nonEmptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag", "Valid String Value");
			var xmlElement = nonEmptyTagElement.ToXElements();
			AssertNotNull(xmlElement);
			AssertEquals(DummyDocumentXmlns + "SampleTag", xmlElement.First().Name);
			AssertEquals(@"<SampleTag xmlns=""http://schemas.wtg.com"">Valid String Value</SampleTag>", xmlElement.First().ToString());
		}

		public override void TestToString()
		{
			IXmlElement emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, string.Empty, "Valid String Value");
			AssertNullOrEmpty(emptyTagElement.ToString());

			IXmlElement nonEmptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag", "Valid String Value");
			var xml = nonEmptyTagElement.ToString();
			AssertEquals(@"<SampleTag xmlns=""http://schemas.wtg.com"">Valid String Value</SampleTag>", xml);
		}

		public override void TestToWriteToXmlStream()
		{
			IXmlElement nonEmptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag", "Valid String Value");
			var xml = nonEmptyTagElement.StreamToString();
			AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?>
<SampleTag xmlns=""http://schemas.wtg.com"">Valid String Value</SampleTag>", xml);
		}
	}
}
