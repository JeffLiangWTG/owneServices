using System.Xml.Linq;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class DataCallStackReaderTest : TestCase
	{
		public void TestRightData()
		{
			XNamespace xmlPath = "I_am_a_Test";
			var testData = new XElement(xmlPath + "Data", string.Empty);
			var outputData = new XElement(xmlPath + "Data", new XElement(xmlPath + "Call", "at blablablabla"));
			new DataCallStackReader("at blablablabla", xmlPath).ExtractCallStack(testData);
			AssertEquals("Right GetResult DataCallStackReader", outputData.ToString(), testData.ToString());
		}

		public void TestCorrectOutputFromValidInput()
		{
			XNamespace xmlPath = "I_am_a_Test";
			string data = @"Application: ConsoleApplication1.exe
      Framework Version: v4.0.30319
      Description: The process was terminated due to an unhandled exception.
      Exception Info: System.NullReferenceException
      Stack:
      at ConsoleApplication1.Program.Main(System.String[])";
			string expected = "<ConvertedData xmlns=\"I_am_a_Test\"" + @">
  <Source>ConsoleApplication1.exe</Source>
  <VersionNumber>v4.0.30319</VersionNumber>
  <Message>The process was terminated due to an unhandled exception.</Message>
  <ExceptionType>System.NullReferenceException</ExceptionType>
  <Call>at ConsoleApplication1.Program.Main(System.String[])</Call>
</ConvertedData>";
			var testData = new XElement(xmlPath + "ConvertedData", string.Empty);
			new DataCallStackReader(data, xmlPath).ExtractCallStack(testData);
			AssertEquals("Right GetResult DataCallStackReader", expected, testData.ToString());
		}
	}
}
