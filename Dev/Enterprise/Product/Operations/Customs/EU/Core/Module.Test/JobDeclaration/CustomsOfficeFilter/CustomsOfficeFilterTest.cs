using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(CustomsOfficeFilter))]
	class CustomsOfficeFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Property", "", Filter.Property);
				AssertEquals("Purpose", "", Filter.Purpose);
			});
		}

		public void TestClear()
		{
			Filter.Purpose = "XXX";
			Filter.Property = "ES009999";

			Filter.Clear();

			CombineAssertions(() =>
			{
				AssertEquals("Property", "", Filter.Property);
				AssertEquals("Purpose", "", Filter.Purpose);
			});
		}

		public void TestIsEmpty()
		{
			Filter.Purpose = "";
			Filter.Property = "";

			CombineAssertions(() =>
			{
				AssertEquals("Empty as all properties are empty", true, Filter.IsEmpty);

				Filter.Property = "ES009999";
				AssertEquals("Not empty as 'Property' is not empty", false, Filter.IsEmpty);

				Filter.Property = "";
				Filter.Purpose = "XXX";
				AssertEquals("Not empty as 'Purpose' is not empty", false, Filter.IsEmpty);
			});
		}

		public void TestPurposeList()
		{
			AssertMultilineASCIIEquals("PurposeList has the list given as parameter", "ENT, EXP", Filter.PurposeList.CodesAsString);
		}

		public void TestSerialisation()
		{
			Filter.Purpose = "AAA";
			Filter.Property = "ES009999";

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("serialisation", SampleXml, writer.ToString());
			}
		}

		public void TestDeserialisation()
		{
			using (StringReader reader = new StringReader(SampleXml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter"); // because we follow a broken pattern for reading xml.
				((IXmlSerializable)Filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement(); // because we follow a broken pattern for reading xml.
			}

			CombineAssertions(delegate
			{
				AssertEquals("Purpose", "AAA", Filter.Purpose);
				AssertEquals("Property", "ES009999", Filter.Property);
			});
		}

		const string SampleXml =
			"<Filter>\r\n" +
			"  <Comparer>starts with</Comparer>\r\n" +
			"  <Property>ES009999</Property>\r\n" +
			"  <Purpose>AAA</Purpose>\r\n" +
			"</Filter>\r\n" +
			"";

		CodeDescriptionPairList PurposeList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair("ENT", "Description 1");
				list.AddPair("EXP", "Description 2");
				return list;
			}
		}

		CustomsOfficeFilter Filter
		{
			get { return filter ?? (filter = new CustomsOfficeFilter("description", delegate { return new ZQuery(); }, PurposeList)); }
		}
		CustomsOfficeFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomsOfficeFilter("description", delegate
			{ return new ZQuery(); }, PurposeList);
		}
	}
}
