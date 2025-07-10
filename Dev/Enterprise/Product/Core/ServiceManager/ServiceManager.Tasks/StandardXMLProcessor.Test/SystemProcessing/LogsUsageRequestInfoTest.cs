using System;
using System.Xml;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class LogsUsageRequestInfoTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			string xml =
@"<LogsRequest>
<DateFromUtc>2016/10/01</DateFromUtc>
<DateToUtc>2016/10/30</DateToUtc>
<ServiceTaskCode>ABC</ServiceTaskCode>
<IncidentNumber>CS01234567</IncidentNumber>
<HostServerName>LON-SSQL-20B\MSSQLSERVER2</HostServerName>
<HostDBName>ODYSSEYSEIHAM</HostDBName>
<HostConnectionServerName>1234</HostConnectionServerName>
<MaxZipSize>100</MaxZipSize>
</LogsRequest>";

			var info = new LogsRequestInfo(xml);
			AssertEquals(new DateTime(2016, 10, 1), info.DateFromUtc);
			AssertEquals(new DateTime(2016, 10, 30), info.DateToUtc);
			AssertEquals("ABC", info.ServiceTaskCode);
			AssertEquals("CS01234567", info.IncidentNumber);
			AssertEquals(@"LON-SSQL-20B\MSSQLSERVER2", info.HostServerName);
			AssertEquals("ODYSSEYSEIHAM", info.HostDBName);
			AssertEquals("1234", info.HostConnectionServerName);
			AssertEquals(100, info.MaxZipSize);
		}

		public void TestInvalidXml()
		{
			string badXml1 =
@"<LogsRequest>
<DateFromUtc>2016/10/01</DateFromUtc>
<ServiceTaskCode>ABC</ServiceTaskCode>
<IncidentNumber>CS01234567</IncidentNumber>
<HostServerName>LON-SSQL-20B\MSSQLSERVER2</HostServerName>
<HostDBName>ODYSSEYSEIHAM</HostDBName>
<HostConnectionServerName>1234</HostConnectionServerName>
<MaxZipSize>100</MaxZipSize>
</LogsRequest>";

			string badXml2 =
@"<Foo>foo</Foo>";

			AssertExceptionThrown<XmlException>(() => new LogsRequestInfo("blah"));
			AssertExceptionThrown<XmlException>(() => new LogsRequestInfo(badXml1));
			AssertExceptionThrown<XmlException>(() => new LogsRequestInfo(badXml2));
		}
	}
}
