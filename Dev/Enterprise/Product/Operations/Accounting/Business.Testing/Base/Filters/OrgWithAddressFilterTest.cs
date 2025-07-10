using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Filters.Testing
{
	[TestedType(typeof(OrgWithAddressFilter))]
	internal sealed class OrgWithAddressFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			organization = Factory.NewWithValidTestData<OrgHeader>().PK;
			address = Factory.NewWithValidTestData<OrgAddress>().PK;
		}

		ZGuid organization;
		ZGuid address;

		public void TestDefaultValues()
		{
			AssertEquals("Organization", ZGuid.Empty, Filter.Organization);
			AssertEquals("Address", ZGuid.Empty, Filter.Address);
		}

		public void TestClear()
		{
			Filter.Organization = organization;
			Filter.Address = address;

			Filter.Clear();

			AssertEquals("Organization", ZGuid.Empty, Filter.Organization);
			AssertEquals("Address", ZGuid.Empty, Filter.Address);
		}

		public void TestIsEmpty()
		{
			Filter.Organization = ZGuid.Empty;
			Filter.Address = ZGuid.Empty;

			AssertEquals("Empty as all properties are empty", true, Filter.IsEmpty);

			Filter.Organization = organization;
			AssertEquals("Not empty as 'Organization' is not empty", false, Filter.IsEmpty);

			Filter.Organization = ZGuid.Empty;
			Filter.Address = address;
			AssertEquals("Not empty as 'Address' is not empty", false, Filter.IsEmpty);
		}

		public void TestSerialisation()
		{
			Filter.Organization = organization;
			Filter.Address = address;

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("serialisation", string.Format(SampleXml, organization.ToString(), address.ToString()), writer.ToString());
			}
		}

		public void TestDeserilisation()
		{
			using (StringReader reader = new StringReader(string.Format(SampleXml, organization.ToString(), address.ToString())))
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
				AssertEquals("Organization", organization, Filter.Organization);
				AssertEquals("Address", address, Filter.Address);
			});
		}

		#region Implementation

		readonly string SampleXml =
 @"<Filter>
  <Comparer>starts with</Comparer>
  <Property />
  <Organization>{0}</Organization>
  <Address>{1}</Address>
</Filter>";

		OrgWithAddressFilter Filter
		{
			get { return filter ?? (filter = new OrgWithAddressFilter("description", (x, y) => { return new ZQuery(); }, true)); }
		}
		OrgWithAddressFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgWithAddressFilter("description", (x, y) => { return new ZQuery(); }, true);
		}

		#endregion
	}
}
