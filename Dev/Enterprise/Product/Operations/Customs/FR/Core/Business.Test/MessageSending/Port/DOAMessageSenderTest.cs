using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DOAMessageSenderTest : TestCaseWithFactory
	{
		public void TestGetBuilderManager()
		{
			AssertType<DOAMessageBuilderManager>(sender.GetBuilderManager());
		}

		[TestDate(2023, 02, 15)]
		public void TestPostSend()
		{
			Factory.Save();
			sender.Send();
			var headerLogs = header.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageGenerationFailedCode).ToArray();
			AssertEquals(1, headerLogs.Length);
			AssertEquals("DOA", headerLogs[0].SL_Reference);
			AssertEquals(ZDateTimeOffset.Now, headerLogs[0].EventTimeOffset);
		}

		protected override void SetUp()
		{
			base.SetUp();
			errorCollector = new ErrorCollector();
			header = Factory.New<NctsHeader>();
			dataObject = new DOADataObject();
			sendingObject = new DOAMessageSendingObject(header, dataObject);
			sender = new DOAMessageSenderForTest(sendingObject, errorCollector);
		}

		ErrorCollector errorCollector;
		DOAMessageSenderForTest sender;
		DOAMessageSendingObject sendingObject;
		NctsHeader header;
		DOADataObject dataObject;
	}

	public class DOAMessageSenderForTest : DOAMessageSender
	{
		public DOAMessageSenderForTest(DOAMessageSendingObject objectToSend, ErrorCollector errorCollector) : base(objectToSend, errorCollector)
		{
		}

		public new MessageBuilderManager<DOAMessageSendingObject> GetBuilderManager() => base.GetBuilderManager();
	}
}
