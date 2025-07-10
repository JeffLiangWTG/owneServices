using System;
using System.Xml;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class LicenceUsageRequestInfoTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			string xml =
@"<LicenceUsageRequest>
<DateFrom>2016/10/01</DateFrom>
<DateTo>2016/10/30</DateTo>
<RequestedBy>ABC</RequestedBy>
</LicenceUsageRequest>";

			var info = new LicenceUsageRequestInfo(xml);
			AssertEquals(new DateTime(2016, 10, 1), info.DateFrom);
			AssertEquals(new DateTime(2016, 10, 30), info.DateTo);
			AssertEquals("ABC", info.RequestedBy);
		}

		public void TestInvalidXml()
		{
			string badXml1 =
@"<LicenceUsageRequest>
<DateFrom>2016/10/01</DateFrom>
<RequestedBy>ABC</RequestedBy>
</LicenceUsageRequest>";

			string badXml2 =
@"<Foo>foo</Foo>";

			AssertExceptionThrown<XmlException>(() => new LicenceUsageRequestInfo("blah"));
			AssertExceptionThrown<XmlException>(() => new LicenceUsageRequestInfo(badXml1));
			AssertExceptionThrown<XmlException>(() => new LicenceUsageRequestInfo(badXml2));
		}
	}
}
