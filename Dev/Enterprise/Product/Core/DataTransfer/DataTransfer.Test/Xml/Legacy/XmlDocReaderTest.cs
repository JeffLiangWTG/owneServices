using System;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlDocReaderTest : TestCaseWithFactory
	{
		public void TestGetValueAsDateTime()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>2004-12-11</Child></Test>");
			XmlNode parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals(new ZDateTime(2004, 12, 11), Reader.GetValueAsDateTime(parentNode, "Child"));

			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>2004-sdfsd1</Child></Test>");
			parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals(ZDateTime.Empty, Reader.GetValueAsDateTime(parentNode, "Child"));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestGetValueAsDateTimeOffset()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>2004-12-11</Child></Test>");
			XmlNode parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals(new ZDateTimeOffset(2004, 12, 11, 0, 0, 0, 0, TimeSpan.FromHours(11)), Reader.GetValueAsDateTimeOffset(parentNode, "Child"));

			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>2004-sdfsd1</Child></Test>");
			parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals(ZDateTimeOffset.Empty, Reader.GetValueAsDateTimeOffset(parentNode, "Child"));

			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>2004-12-11T11:12:13.123+08:00</Child></Test>");
			parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals(new ZDateTimeOffset(2004, 12, 11, 11, 12, 13, 123, TimeSpan.FromHours(8)), Reader.GetValueAsDateTimeOffset(parentNode, "Child"));

			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>2004-12-11T11:12:13.123+00:00</Child></Test>");
			parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals(new ZDateTimeOffset(2004, 12, 11, 11, 12, 13, 123, TimeSpan.Zero), Reader.GetValueAsDateTimeOffset(parentNode, "Child"));

			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>2004-12-11T11:12:13.123Z</Child></Test>");
			parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals(new ZDateTimeOffset(2004, 12, 11, 11, 12, 13, 123, TimeSpan.Zero), Reader.GetValueAsDateTimeOffset(parentNode, "Child"));
		}

		public void TestGetValue()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child>ChildValue</Child></Test>");
			XmlNode parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals("ChildValue", Reader.GetValue(parentNode, "Child"));
		}

		public void TestGetAttributeValueUsingXPath()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child description=\"young1\">ChildValue</Child></Test>");
			XmlNode parentNode = xmlDoc.SelectSingleNode("//Test");

			AssertEquals("young1", Reader.GetAttributeValue(parentNode, "Child", "description"));
		}

		public void TestGetAttributeValueUsingXmlNode()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Child description=\"young1\">ChildValue</Child></Test>");
			XmlNode childNode = xmlDoc.SelectSingleNode("//Child");

			AssertEquals("young1", Reader.GetAttributeValue(childNode, "description"));
		}

		public void TestGetValueWithAttributeValue()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<Test><Children><Child attrib=\"attrib1\">ChildValue1</Child><Child attrib=\"attrib2\">ChildValue2</Child></Children></Test>");
			XmlNode parentNode = xmlDoc.SelectSingleNode("//Children");

			AssertEquals("ChildValue1", Reader.GetValue(parentNode, "Child", "attrib", "attrib1"));
			AssertEquals("ChildValue2", Reader.GetValue(parentNode, "Child", "attrib", "attrib2"));
		}

		public void TestGetOrganisation()
		{
			#region Xml String

			string xml =
				"<Carrier EDICode=\"TESTCODE\">" +
				"<OrganisationDetails>					" +
				"<Name>SOME FREIGHT</Name>" +
				"<Location>AUSYD</Location>" +
				"<Addresses>" +
				"<Address>" +
				"<AddressLine1>BURNLEIGH</AddressLine1>" +
				"<AddressLine2>BURNS BAY RD</AddressLine2>" +
				"<CityOrSuburb>LANE COVE</CityOrSuburb>" +
				"<StateOrProvince>NSW</StateOrProvince>" +
				"<PostCode>2066</PostCode>	" +
				"<TelephoneNumbers>" +
				"<TelephoneNumber NumberType=\"Business\">9763766</TelephoneNumber>" +
				"</TelephoneNumbers>" +
				"<Email>nospam@edi.com.au</Email>" +
				"</Address>" +
				"</Addresses>" +
				"<WebAddress>www.edi.com.au</WebAddress>" +
				"<RegistrationNumbers>" +
				"<RegistrationNumber>" +
				"<CountryOfRegistration>ZA</CountryOfRegistration>" +
				"<NumberType>CCD</NumberType>" +
				"<Number>123456789</Number>" +
				"</RegistrationNumber>" +
				"</RegistrationNumbers>" +
				"</OrganisationDetails>" +
				"</Carrier>";

			#endregion

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			XmlNode orgNode = xmlDoc.SelectSingleNode("Carrier");

			OrgHeader org = Reader.GetOrganisation(Factory, null, orgNode);

			AssertEquals("SOMFRESYD", org.OH_Code);
			AssertEquals("SOME FREIGHT", org.OH_FullName);
			AssertEquals("AUSYD", org.OH_RL_NKClosestPort);
			AssertEquals("BURNLEIGH", org.MainAddress.OA_Address1);
			AssertEquals("BURNS BAY RD", org.MainAddress.OA_Address2);
			AssertEquals("LANE COVE", org.MainAddress.OA_City);
			AssertEquals("NSW", org.MainAddress.OA_State);
			AssertEquals("2066", org.MainAddress.OA_PostCode);
			AssertEquals("9763766", org.MainAddress.OA_Phone);
			AssertEquals("nospam@edi.com.au", org.MainAddress.OA_Email);
			AssertEquals("www.edi.com.au", org.MainWebURL.PU_URL);
			AssertEquals("ZA", org.CustomsCodes[0].OK_RN_NKCodeCountry);
			AssertEquals("CCD", org.CustomsCodes[0].OK_CodeType);
			AssertEquals("123456789", org.CustomsCodes[0].OK_CustomsRegNo);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			XmlDocument xmlDoc = new XmlDocument();
			Reader = new XmlDocReader(xmlDoc);
		}
		XmlDocReader Reader;

		#endregion
	}
}
