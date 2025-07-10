using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;

namespace Enterprise.xTMessaging.Business.Test
{
	public abstract class XMLMessageCreatorTest<T> : TestCaseWithFactory
		where T : IEDIMessageCreator
	{
		public void TestErrorCreatingEDIMessage_InvalidXml()
		{
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("I am not an XML");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var exception = AssertExceptionThrown<MsgProcessingException>(() => EDIMessageCreator.CreateEDIMessagesForInterchange(testMsgBody, Factory));
			AssertEquals("MessageProcessException is thrown", typeof(MsgProcessingException), exception.GetType());
			AssertEquals("EDIInterchange should be flagged as Error", EDIInterchange.Status.Error, Interchange.EI_Status);
			AssertContains("XML Error occurred when creating EDIMessage: Root element is missing.", exception.Message);
		}

		public abstract void TestGenerateEDIMessages();

		protected IEDIMessageCreator EDIMessageCreator
		{
			get
			{
				return eDIMessageCreator ?? (eDIMessageCreator = GetMessageCreator());
			}
		}
		IEDIMessageCreator eDIMessageCreator;

		protected abstract IEDIMessageCreator GetMessageCreator();

		protected TestUtils.TestLogger Logger
		{
			get { return logger ?? (logger = new TestUtils.TestLogger()); }
		}
		TestUtils.TestLogger logger;

		protected EDIInterchange Interchange
		{
			get { return interchange ?? (interchange = GetEDIInterchange()); }
		}
		EDIInterchange interchange;

		protected abstract EDIInterchange GetEDIInterchange();

		protected override void TearDown()
		{
			base.TearDown();
			logger = null;
		}
	}
}
