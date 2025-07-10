using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class XmlConverterTest : TransactionedTestCase
	{
		public void TestConvertToIssueXmlFormat_HavingCallStackData()
		{
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLExtractDataOutput.txt")))
			{
				var xElement = XElement.Load(stream);
				AssertEquals("Correct Converted XML", new EmbeddedResourceRetriever(GetType().Assembly).GetString("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLConvertedIssueXMLOutput.txt", Encoding.ASCII), XmlConverter.ConvertToIssueXmlFormat(xElement));
			}
		}

		public void TestConvertToIssueXmlFormat_NotHavingCallStackData()
		{
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLExtractDataOutput_NotHavingCallStackData.txt")))
			{
				var xElement = XElement.Load(stream);
				AssertEquals("Not Correct Converted XML", new EmbeddedResourceRetriever(GetType().Assembly).GetString("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLConvertedIssueXMLOutput_NotHavingCallStackData.txt", Encoding.ASCII), XmlConverter.ConvertToIssueXmlFormat(xElement));
			}
		}

		public void TestSuccessParseXElement()
		{
			var path = "ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLConverterInput.txt";
			var embeddedResource = new EmbeddedResourceRetriever(GetType().Assembly);
			var data = embeddedResource.GetString(path, Encoding.ASCII);
			AssertNotEquals("Get Data from Embedded Resource should not be empty", string.Empty, data);
			using (var stream = new MemoryStream(embeddedResource.GetBytes(path)))
			{
				var xElement = XElement.Load(stream);
				xElement.ExtractData(new EventLogDataTransmissionHandler());
				AssertNoExceptionThrown("Fail to Convert From XML to XML", () =>
				{
					var result = XmlConverter.ConvertToIssueXmlFormat(xElement);
				});
			}
		}

		public void TestExtractData_HavingCallStackData()
		{
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLConverterInput.txt")))
			{
				var xElement = XElement.Load(stream);
				xElement.ExtractData(new TestEventLogDataTransmissionHandler());
				AssertEquals("Not Correct Converted XML", new EmbeddedResourceRetriever(GetType().Assembly).GetString("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLExtractDataOutput.txt", Encoding.ASCII), xElement.ToString());
			}
		}

		public void TestExtractData_NotHavingCallStackData()
		{
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLConverterInput_NotHavingCallStackData.txt")))
			{
				var xElement = XElement.Load(stream);
				xElement.ExtractData(new TestEventLogDataTransmissionHandler());
				AssertEquals("Not Correct Converted XML", new EmbeddedResourceRetriever(GetType().Assembly).GetString("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLExtractDataOutput_NotHavingCallStackData.txt", Encoding.ASCII), xElement.ToString());
			}
		}

		class TestEventLogDataTransmissionHandler : EventLogDataTransmissionHandler
		{
			public override GlbCompany CurrentCompany
			{
				get
				{
					var company = new BusinessObjectFactory().NewWithValidTestData<GlbCompany>();
					company.GC_Name = "Test Company";
					return company;
				}
			}

			public override GlbStaff CurrentUser
			{
				get
				{
					var staff = new BusinessObjectFactory().NewWithValidTestData<GlbStaff>();
					staff.GS_LoginName = "skywalker";
					staff.GS_EmailAddress = "advertisement@starwar.com";
					return staff;
				}
			}
		}
	}
}
