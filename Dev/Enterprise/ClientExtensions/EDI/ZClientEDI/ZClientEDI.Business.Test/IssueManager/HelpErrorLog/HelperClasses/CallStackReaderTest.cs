using System;
using System.Text;
using System.Xml;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public class CallStackReaderTest : TestCaseWithXmlDoc
	{
		[ExpectNoExceptions]
		public void TestCallStackText_Mock()
		{
			XmlDocument testDoc = new XmlDocument();
			testDoc.Load(SmallCallstackTestFile);

			var mock = new Mock<IAppendStrategy>();
			mock.Setup(m => m.AppendMatch("", It.IsAny<StringBuilder>()));
			mock.Setup(m => m.AppendMatch("----- Exception caught and reported here -----", It.IsAny<StringBuilder>()));
			mock.Setup(m => m.AppendMatch("   at System.Environment.GetStackTrace(Exception e)", It.IsAny<StringBuilder>()));
			mock.Setup(m => m.AppendMatch("   at Enterprise.ZArchitecture.Core.ExceptionFullTracer.CaptureTraceToReporter(Exception ex)", It.IsAny<StringBuilder>()));
			mock.Setup(m => m.AppendMatch("   at Dummy.ForTest", It.IsAny<StringBuilder>()));

			CallStackReader reader = new CallStackReader(testDoc);
			reader.CallStackText(mock.Object);

			mock.VerifyAll();
		}

		public void TestCallStackText_Strategy_StringConstructor()
		{
			XmlDocument testDoc = new XmlDocument();
			testDoc.Load(SmallCallstackTestFile);

			CallStackReader reader = new CallStackReader(testDoc.OuterXml);
			IAppendStrategy strategy = new AppendAllStrategy();
			AssertEquals(expected, reader.CallStackText(strategy));
		}

		public void TestCallStackText_Strategy_XmlDocConstructor()
		{
			XmlDocument testDoc = new XmlDocument();
			testDoc.Load(SmallCallstackTestFile);

			CallStackReader reader = new CallStackReader(testDoc);
			IAppendStrategy strategy = new AppendAllStrategy();
			AssertEquals(expected, reader.CallStackText(strategy));
		}

		const string expected = @"
----- Exception caught and reported here -----
at System.Environment.GetStackTrace(Exception e)
at Enterprise.ZArchitecture.Core.ExceptionFullTracer.CaptureTraceToReporter(Exception ex)
at Dummy.ForTest
";

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCallStackText_NullStrategy()
		{
			CallStackReader reader = new CallStackReader(new XmlDocument());
			reader.CallStackText(null);
		}
	}
}
