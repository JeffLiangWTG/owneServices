using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIInterchangeTextFilter))]
	sealed class EDIInterchangeTextFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new EDIInterchangeTextFilter("Body Text", new EDIInterchangeFilterBusinessObject());
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override ZString ExpectedDescription
		{
			get { return "Body Text"; }
		}

		public void TestEDIInterchangeTextFilter()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_BodyData = CompressBytes(Encoding.UTF8.GetBytes("Test Data"));
			interchange.EI_SystemCreateTimeUtc = new DateTime(2016, 05, 18);
			Factory.Save();

			var interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			var filter = (EDIInterchangeTextFilter)interchangeFilterBusinessObject["Body Text"];
			var subGroup = (ModuleFilterSubGroup)filter.SubGroup;
			{
				filter.Property = "Test";
				filter.ComparisonOperator = EDIInterchangeTextFilter.ComparisonConstants.StartsWith;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Body text StartsWith 'Test' should be 1", 1, interchanges.Length);
				Assert("Body text StartsWith 'Test' Should contain testInterchange", interchanges.Contains<EDIInterchange>(interchange));
			}

			{
				filter.Property = "Test";
				filter.ComparisonOperator = EDIInterchangeTextFilter.ComparisonConstants.Contains;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Body text contains 'Test' should be 1", 1, interchanges.Length);
				Assert("Body text contains 'Test' Should contain testInterchange", interchanges.Contains<EDIInterchange>(interchange));
			}

			{
				filter.Property = "AAA";
				filter.ComparisonOperator = EDIInterchangeTextFilter.ComparisonConstants.NotContain;
				EDIInterchange[] interchanges = Factory.Load<EDIInterchange>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Body text not contains 'AAA' should be 1", 1, interchanges.Length);
				Assert("Body text not contains 'AAA' Should contain testInterchange", interchanges.Contains<EDIInterchange>(interchange));
			}
		}

		public void TestTwoBlobFields_ShouldNotThrowException()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.SYS;

			#region Big XML
			var bodyText = @"<SystemInterchange xmlns=""http://www.cargowise.com/Schemas/System"">
  <Header>
    <SenderID>DIMMEYTRN</SenderID>
    <RecipientID>EDIAUSSYD</RecipientID>
  </Header>
  <Body>
	<CurrentVersionReport compressed=""1"">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
aDh/Bw7Ac2Zh4BUVEh/zDcj7/Q==</CurrentVersionReport>
  </Body>
</SystemInterchange>";

			var headerNText = "<InterchangeInfo xmlns=\"http://www.edi.com.au/EnterpriseService/\"><Date>2018-01-11T15:02:30.00+08:00</Date><XmlType>LightWeight</XmlType><EDIOrganisation EDICode=\"D0106\" OwnerCode=\"D0106\"><OrganisationDetails><Name>DT China (Shanghai) Ltd. Shenzhen Branch</Name><Location Country=\"CHINA\" City=\"SHENZHEN\">SZX</Location><Addresses><Address AddressType=\"MAIN\"><AddressLine1>Room1008, Jintian Building,</AddressLine1><AddressLine2>No. 1199 Heping Road, Luohu District,</AddressLine2><AddressCode>Room1008, Jintian Buildin</AddressCode><CityOrSuburb>SHENZHEN</CityOrSuburb><TelephoneNumbers><TelephoneNumber NumberType=\"Business\">+86 755 82485853/82485855</TelephoneNumber><TelephoneNumber NumberType=\"Fax\">+86 755 82485881</TelephoneNumber></TelephoneNumbers><Language>ENG</Language><CompanyName>DT China (Shanghai) Ltd. Shenzhen Branch</CompanyName><Location>SZX</Location><Sequence>1</Sequence></Address></Addresses></OrganisationDetails></EDIOrganisation></InterchangeInfo>";

			#endregion

			interchange.EI_BodyText = bodyText;
			interchange.EI_HeaderNText = headerNText;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var interchangeFromDB = otherFactory.Load<EDIInterchange>(interchange.PK);

			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var fullText = interchangeFromDB.EI_InterchangeTextShort;
			});
		}

		public void TestEDIInterchangeTextFilterValidationErrors()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_BodyData = CompressBytes(Encoding.UTF8.GetBytes("Test Data"));
			interchange.EI_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();

			var interchangeFilterBusinessObject = new EDIInterchangeFilterBusinessObject();
			var textFilter = (EDIInterchangeTextFilter)interchangeFilterBusinessObject["Body Text"];
			textFilter.IsActive = true;
			textFilter.Property = "Test";
			textFilter.ComparisonOperator = EDIInterchangeTextFilter.ComparisonConstants.StartsWith;

			interchangeFilterBusinessObject.FilterStrips.AddNew("Body Text");
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");

			interchangeFilterBusinessObject.FilterStrips.AddNew("Interchange Time");
			var dateFilter = (ModuleDateFilter)interchangeFilterBusinessObject.FilterStrips[1].CurrentModuleFilter;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-10);
			dateFilter.Property2 = ZDateTime.Now.AddDays(+1);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");

			dateFilter.Property1 = ZDateTime.Now.AddDays(-3);
			dateFilter.Property2 = ZDateTime.Now.AddDays(+1);
			textFilter.Validation.ValidateAll();
			AssertNoErrors(textFilter.PropertyInfo);
		}

		static byte[] CompressBytes(byte[] buffer)
		{
			using (MemoryStream ms = new MemoryStream(buffer.Length))
			{
				WriteCompressedStream(buffer, ms);
				return ms.ToArray();
			}
		}

		static void WriteCompressedStream(byte[] bytes, Stream stream)
		{
			stream.Write(new byte[] { (byte)'P', (byte)'Z' }, 0, 2);

			using (DeflateStream zipStream = new DeflateStream(stream, CompressionMode.Compress, true))
			{
				zipStream.Write(bytes, 0, bytes.Length);
			}
		}
	}
}
