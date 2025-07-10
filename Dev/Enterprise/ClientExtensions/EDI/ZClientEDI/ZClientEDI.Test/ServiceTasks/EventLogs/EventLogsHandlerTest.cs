using System.Text;
using System.Xml.Linq;
using CargoWise.IO;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class EventLogsHandlerTest : TestCase
	{
		public void TestRightDataWithImportantData()
		{
			XNamespace xmlPath = "http://schemas.microsoft.com/win/2004/08/events/event";
			var dataInside = new EmbeddedResourceRetriever(GetType().Assembly).GetString("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestXMLConverterInput.txt", Encoding.ASCII);
			AssertNotEquals("Get Data from Embedded Resource should not be empty", string.Empty, dataInside);
			XElement xmlFile = XElement.Parse(dataInside);
			AssertEquals("Right root element of XML file", "Event", xmlFile.Name.LocalName);
			XName eventDataName = xmlPath + "EventData";
			XElement system = xmlFile.Element(xmlPath + "System");
			var eventData = xmlFile.Element(eventDataName).Elements();
			var eventLogsHandler = new EventLogsCallStackExtractor(eventData, xmlPath);
			var dataEventLogsHandler = new EmbeddedResourceRetriever(GetType().Assembly).GetString("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventLogsHandler1.txt", Encoding.ASCII);
			AssertNotEquals("Get DataEventLogsHandler from Embedded Resource should not be empty", string.Empty, dataEventLogsHandler);
			AssertEquals("Incorrect Returned Data XML of EventLogHandler", dataEventLogsHandler, eventLogsHandler.GetData().ToString());
		}

		public void TestNormalData()
		{
			XNamespace xmlPath = "http://schemas.microsoft.com/win/2004/08/events/event";
			string suppressedSmellDirectory = "C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\IDE\\devenv.exe";
			string data = "<Event xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"" + @">
  <System>
  </System>
  <EventData>
		<Data Name=" + "\"Context\"" + @">local system is invalid</Data>
		<Data Name=" + "\"Context\"" + @">InvalidFileException: Could not find " + suppressedSmellDirectory + @"</Data>
    <Data Name=" + "\"ObjId\"" + @">ea 90 d9 3f bc c9 41 4d f6 1b 18 ee b8 a2 52 54 d3 61 4f 2c</Data>
  </EventData>
</Event>";
			string expected = "<ConvertedData xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"" + @">
  <Message>local system is invalid</Message>
  <Message>InvalidFileException: Could not find " + suppressedSmellDirectory + @"</Message>
</ConvertedData>";
			XElement xmlFile = XElement.Parse(data);
			XName eventDataName = xmlPath + "EventData";
			XElement system = xmlFile.Element(xmlPath + "System");
			var eventData = xmlFile.Element(eventDataName).Elements();
			var eventLogsHandler = new EventLogsCallStackExtractor(eventData, xmlPath);
			AssertEquals("Correct Returned Data XML of EventLogHandler", expected, eventLogsHandler.GetData().ToString());
		}

		public void TestUnimportantData_EmptyExtractedData()
		{
			XNamespace xmlPath = "http://schemas.microsoft.com/win/2004/08/events/event";
			string suppressedSmellDirectory = "<Data>C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\IDE\\devenv.exe</Data>";
			string data = "<Event xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"" + @">
  <System>
  </System>
  <EventData>
    <Data>devenv.exe</Data>
    <Data>12.0.30501.0</Data>
    <Data>5361f453</Data>
    <Data>ntdll.dll</Data>
    <Data>6.3.9600.17031</Data>
    <Data>5308893d</Data>
    <Data>c0000005</Data>
    <Data>0001f0a3</Data>
    <Data>2548</Data>
    <Data>01cf9afaca29f56a</Data>
    " + suppressedSmellDirectory + @"
  </EventData>
</Event>";
			XElement xmlFile = XElement.Parse(data);
			XName eventDataName = xmlPath + "EventData";
			XElement system = xmlFile.Element(xmlPath + "System");
			var eventData = xmlFile.Element(eventDataName).Elements();
			var eventLogsHandler = new EventLogsCallStackExtractor(eventData, xmlPath);
			string expected = "<ConvertedData xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"" + @"></ConvertedData>";
			AssertEquals("Correct Returned Data XML of EventLogHandler", expected, eventLogsHandler.GetData().ToString());
		}

		public void TestUnimportantData2AndNormalData_EmptyExtractedData()
		{
			XNamespace xmlPath = "http://schemas.microsoft.com/win/2004/08/events/event";
			string data = "<Event xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"" + @">
  <System>
  </System>
  <EventData>
		<Data Name=" + "\"Context\"" + @">local system</Data>
    <Data Name=" + "\"ObjId\"" + @">ea 90 d9 3f bc c9 41 4d f6 1b 18 ee b8 a2 52 54 d3 61 4f 2c</Data>
  </EventData>
</Event>";
			var xmlFile = XElement.Parse(data);
			var eventDataName = xmlPath + "EventData";
			var system = xmlFile.Element(xmlPath + "System");
			var eventData = xmlFile.Element(eventDataName).Elements();
			var eventLogsHandler = new EventLogsCallStackExtractor(eventData, xmlPath);
			string expected = "<ConvertedData xmlns=\"http://schemas.microsoft.com/win/2004/08/events/event\"" + @"></ConvertedData>";
			AssertEquals("Correct Returned Data XML of EventLogHandler", expected, eventLogsHandler.GetData().ToString());
		}
	}
}
