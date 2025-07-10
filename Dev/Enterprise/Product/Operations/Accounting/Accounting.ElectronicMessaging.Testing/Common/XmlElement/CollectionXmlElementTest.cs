using System.Linq;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class CollectionXmlElementTest : XmlElementBaseTest
	{
		public override void TestHasValue()
		{
			IXmlElement collectionXElement = new CollectionXmlElement<object>(new object[] { null }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			Assert(!collectionXElement.HasValue);

			collectionXElement = new CollectionXmlElement<object>(new object[] { 1, 2 }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			Assert(collectionXElement.HasValue);
		}

		public override void TestToXElements()
		{
			IXmlElement collectionXElement = new CollectionXmlElement<object>(new object[] { null }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			AssertNull(collectionXElement.ToXElements());

			collectionXElement = new CollectionXmlElement<object>(new object[] { 1, 2 }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			var xmlElements = collectionXElement.ToXElements().ToArray();
			AssertNotNull(xmlElements);
			AssertEquals(DummyDocumentXmlns + "SampleTag1", xmlElements.First().Name);
			AssertEquals(2, xmlElements.Length);
			AssertEquals(@"<SampleTag1 xmlns=""http://schemas.wtg.com"">1</SampleTag1>", xmlElements[0].ToString());
			AssertEquals(@"<SampleTag1 xmlns=""http://schemas.wtg.com"">2</SampleTag1>", xmlElements[1].ToString());
		}

		public override void TestToString()
		{
			IXmlElement collectionXElement = new CollectionXmlElement<object>(new object[] { null }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			AssertNullOrEmpty(collectionXElement.ToString());

			collectionXElement = new CollectionXmlElement<object>(new object[] { 1, 2 }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			var xml = collectionXElement.ToString();
			AssertEquals(@"<SampleTag1 xmlns=""http://schemas.wtg.com"">1</SampleTag1>
<SampleTag1 xmlns=""http://schemas.wtg.com"">2</SampleTag1>", xml);

			var complexCollectionXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleCollectionTag", collectionXElement);
			xml = complexCollectionXElement.ToString();
			AssertEquals(@"<SampleCollectionTag xmlns=""http://schemas.wtg.com"">
  <SampleTag1>1</SampleTag1>
  <SampleTag1>2</SampleTag1>
</SampleCollectionTag>", xml);
		}

		public override void TestToWriteToXmlStream()
		{
			IXmlElement collectionXElement = new CollectionXmlElement<object>(new object[] { null }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			AssertNullOrEmpty(collectionXElement.ToString());

			collectionXElement = new CollectionXmlElement<object>(new object[] { 1, 2 }, o => new SimpleXmlElement(DummyDocumentXmlns, "SampleTag1", o));
			var xml = collectionXElement.StreamToString();
			AssertNotNullOrEmpty(xml);
			AssertEquals(@"<SampleTag1 xmlns=""http://schemas.wtg.com"">1</SampleTag1>
<SampleTag1 xmlns=""http://schemas.wtg.com"">2</SampleTag1>", xml);

			var complexCollectionXElement = new ComplexXmlElement(DummyDocumentXmlns, "SampleCollectionTag", collectionXElement);
			xml = complexCollectionXElement.StreamToString();
			AssertNotNullOrEmpty(xml);
			AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?>
<SampleCollectionTag xmlns=""http://schemas.wtg.com"">
  <SampleTag1>1</SampleTag1>
  <SampleTag1>2</SampleTag1>
</SampleCollectionTag>", xml);
		}
	}
}
