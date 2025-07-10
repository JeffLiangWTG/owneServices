using System.ComponentModel;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(LogFileRecord))]
	class LogFileRecordTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEventList()
		{
			var provider = new Mock<ILogViewerDataProvider>(MockBehavior.Strict);

			var file1 =
@"2007-07-20 16:54:53      Information              Record1
2007-07-20 16:55:30      Information              Record2
2007-07-20 16:56:15      Information              Record3";
			provider.Setup(x => x.GetBytes("TST_20070720.TXT")).Returns(Encoding.ASCII.GetBytes(file1));

			var logFileRecord = new LogFileRecord("TST_20070720.TXT", "hostA");
			AssertEquals(0, logFileRecord.EventList.Count);

			logFileRecord.LogViewerDataProvider = provider.Object;
			AssertEquals(3, logFileRecord.EventList.Count);
			AssertEquals("Record1", logFileRecord.EventList[0].Message);
			AssertEquals("Record2", logFileRecord.EventList[1].Message);
			AssertEquals("Record3", logFileRecord.EventList[2].Message);

			logFileRecord.EventList.Sort("DateTime", ListSortDirection.Descending);
			AssertEquals("Record3", logFileRecord.EventList[0].Message);
			AssertEquals("Record2", logFileRecord.EventList[1].Message);
			AssertEquals("Record1", logFileRecord.EventList[2].Message);

			var file2 =
@"2007-07-20 16:54:53      Information              Record1
2007-07-20 16:55:30      Information              Record2
2007-07-20 16:56:15      Information              Record3
2007-07-20 16:57:01      Information              Record4";

			provider.Setup(x => x.GetBytes("TST_20070720.TXT")).Returns(Encoding.ASCII.GetBytes(file2));

			logFileRecord.ReloadEvents();
			AssertEquals(4, logFileRecord.EventList.Count);
			AssertEquals("Record4", logFileRecord.EventList[0].Message);
			AssertEquals("Record3", logFileRecord.EventList[1].Message);
			AssertEquals("Record2", logFileRecord.EventList[2].Message);
			AssertEquals("Record1", logFileRecord.EventList[3].Message);
			AssertEquals("hostA", logFileRecord.Host);
		}

		public void TestSend()
		{
			var provider = new Mock<ILogViewerDataProvider>(MockBehavior.Strict);

			var file1 =
@"2007-07-20 16:54:53      Information              Record1
2007-07-20 16:55:30      Information              Record2
2007-07-20 16:56:15      Information              Record3";
			provider.Setup(x => x.GetBytes("TST_20070720.TXT")).Returns(Encoding.ASCII.GetBytes(file1));

			var logFileRecord = new LogFileRecord("TST_20070720.TXT", "hostB");
			logFileRecord.LogViewerDataProvider = provider.Object;
			logFileRecord.Send("test1@test.ts", "test2@test.ts");

			var mail = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
			AssertEquals("TST_20070720.TXT on hostB", mail.Subject);
			AssertEquals(2, mail.Recipients.Count);
			AssertEquals("test1@test.ts", mail.Recipients[0]);
			AssertEquals("test2@test.ts", mail.Recipients[1]);
			AssertEquals(1, mail.Attachments.Count);
			AssertEquals("TST_20070720.TXT.zip", mail.Attachments[0].DisplayName);
			var zipExtractor = new ZipExtractor();
			using (var zipStream = new MemoryStream(mail.Attachments[0].Data, false))
			{
				using (var ms = new MemoryStream())
				{
					zipExtractor.ExtractZipStream(zipStream, ms, "TST_20070720.TXT");
					Assert(Encoding.UTF8.GetString(ms.ToArray()).StartsWith("2007-07-20 16:54:53      Information              Record1"));
				}
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewLogFileRecord();
		}

		public static LogFileRecord GetNewLogFileRecord()
		{
			return new LogFileRecord(Path.GetRandomFileName(), "");
		}

		#endregion
	}
}
