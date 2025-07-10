using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class IXmlElementExtensionsTest : TestCaseWithFactory
	{
		public void TestStreamToString()
		{
			var documentXmlns = XNamespace.Get("http://schemas.wtg.com");
			IXmlElement nonEmptyTagElement = new SimpleXmlElement(documentXmlns, "SampleTag", "Valid String Value");
			AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?>
<SampleTag xmlns=""http://schemas.wtg.com"">Valid String Value</SampleTag>", nonEmptyTagElement.StreamToString());
		}

		public void TestAdd()
		{
			var documentXmlns = XNamespace.Get("http://schemas.wtg.com");
			IXmlElement nonEmptyTagElement1 = new SimpleXmlElement(documentXmlns, "SampleTag", "Valid String Value 1");
			IXmlElement nonEmptyTagElement2 = new SimpleXmlElement(documentXmlns, "SampleTag", "Valid String Value 2");

			var sequencedElements = new List<IXmlElementWithSequence>();
			sequencedElements.Add(nonEmptyTagElement1);
			AssertEquals(nonEmptyTagElement1, sequencedElements[0].XmlElement);
			AssertEquals(1, sequencedElements[0].Sequence);

			sequencedElements.Add(nonEmptyTagElement2);
			AssertEquals(nonEmptyTagElement2, sequencedElements[1].XmlElement);
			AssertEquals(2, sequencedElements[1].Sequence);
		}
	}
}
