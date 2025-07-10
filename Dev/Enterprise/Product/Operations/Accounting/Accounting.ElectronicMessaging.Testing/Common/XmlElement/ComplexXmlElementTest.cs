using System.Linq;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class ComplexXmlElementTest : XmlElementBaseTest
	{
		public override void TestHasValue()
		{
			var emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", null);
			IXmlElement complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement);
			Assert(!complexXElement.HasValue);

			var nonEmptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag2", "Valid String Value");
			complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement, nonEmptyTagElement);
			Assert(complexXElement.HasValue);
		}

		public override void TestToXElements()
		{
			var emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", null);
			IXmlElement complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement);
			AssertNull(complexXElement.ToXElements());

			var nonEmptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag2", "Valid String Value");
			complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement, nonEmptyTagElement);
			var xmlElement = complexXElement.ToXElements();
			AssertNotNull(xmlElement);
			AssertEquals(DummyDocumentXmlns + "SampleComplexTag", xmlElement.First().Name);
			AssertEquals(@"<SampleComplexTag xmlns=""http://schemas.wtg.com"">
  <SampleTag2>Valid String Value</SampleTag2>
</SampleComplexTag>", xmlElement.First().ToString());
		}

		public override void TestToString()
		{
			var emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", null);
			IXmlElement complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement);
			AssertNullOrEmpty(complexXElement.ToString());

			var nonEmptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag2", "Valid String Value");
			complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement, nonEmptyTagElement);
			var xml = complexXElement.ToString();
			AssertEquals(@"<SampleComplexTag xmlns=""http://schemas.wtg.com"">
  <SampleTag2>Valid String Value</SampleTag2>
</SampleComplexTag>", xml);
		}

		public override void TestToWriteToXmlStream()
		{
			var emptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", null);
			IXmlElement complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement);
			var xml = complexXElement.StreamToString();
			AssertNullOrEmpty(xml);

			var nonEmptyTagElement = new SimpleXmlElement(DummyDocumentXmlns, "SampleTag2", "Valid String Value");
			complexXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleComplexTag", emptyTagElement, nonEmptyTagElement);
			xml = complexXElement.StreamToString();
			AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?>
<SampleComplexTag xmlns=""http://schemas.wtg.com"">
  <SampleTag2>Valid String Value</SampleTag2>
</SampleComplexTag>", xml);
		}
	}
}
