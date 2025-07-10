using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class LoggerStreamTest : TestCaseWithFactory
	{
		public void TestLoggerStreamWrite()
		{
			const string testString = "ABCDEFGHIJKLMNOPQ";

			byte[] inputByteArray = Encoding.UTF8.GetBytes(testString);

			var logger = new LoggerForTest();
			LoggerStream loggerStream = new LoggerStream(logger);
			loggerStream.Write(inputByteArray, 0, inputByteArray.Length);
			AssertEquals(testString, logger.LogEntries.First());
		}

		public void TestLoggerStreamWithLogMessageHandlerWrite()
		{
			const string testString = "ABCDEFGHIJKLMNOPQ";

			byte[] inputByteArray = Encoding.UTF8.GetBytes(testString);

			var logger = new LoggerForTest();
			LogMessageHandler handler = (type, message) => { logger.Log(LogType.Debug, message); };
			LoggerStream loggerStream = new LoggerStream(handler);
			loggerStream.Write(inputByteArray, 0, inputByteArray.Length);
			AssertEquals(testString, logger.LogEntries.First());
		}

		public void TestSetPositionException()
		{
			var logger = new LoggerForTest();
			LoggerStream loggerStream = new LoggerStream(logger);
			AssertExceptionThrown(typeof(NotImplementedException), () => loggerStream.Position = 0);
		}

		public void TestLoggerStreamWriteWithTrailingSpace()
		{
			const string testString = "Test trailing Space\0\0\0\0";
			const string testStringWithoutTrailingSpace = "Test trailing Space";

			byte[] inputByteArray = Encoding.UTF8.GetBytes(testString);

			var logger = new LoggerForTest();
			LoggerStream loggerStream = new LoggerStream(logger);
			loggerStream.Write(inputByteArray, 0, inputByteArray.Length);
			AssertEquals("The trailing space should be removed", testStringWithoutTrailingSpace, logger.LogEntries.First());
		}
	}
}
