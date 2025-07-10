using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class SyntaxEDIFACTMessageHandlerTest : TestCaseWithFactory
	{
		public void TestDoProcessingReturningStatus()
		{
			var testHandler = new SyntaxEDIFACTMessageHandlerForTest(logger);
			var message = Factory.New<EDIMessage>();
			testHandler.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestProperties()
		{
			var testGroupPK = Env.Registry.PostMasterGroup;
			var nominatedGroup = Constants.EmailTo.NominatedGroup;
			var testHandler = new SyntaxEDIFACTMessageHandlerForTest(logger);
			CombineAssertions(() =>
			{
				AssertEquals("AcknowledgementEmailGroup", testGroupPK, testHandler.AcknowledgementEmailGroup_Exposed);
				AssertEquals("AcknowledgementEmailMode", nominatedGroup, testHandler.AcknowledgementEmailMode_Exposed);
				AssertEquals("ImpedimentEmailGroup", testGroupPK, testHandler.ImpedimentEmailGroup_Exposed);
				AssertEquals("ImpedimentEmailMode", nominatedGroup, testHandler.ImpedimentEmailMode_Exposed);
				AssertEquals("ErrorEmailGroup", testGroupPK, testHandler.ErrorEmailGroup_Exposed);
				AssertEquals("ErrorEmailMode", nominatedGroup, testHandler.ErrorEmailMode_Exposed);
				AssertNull("EmailResponseLinkedObject", testHandler.EmailResponseLinkedObject_Exposed);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}

		LoggingInformation logger;
	}
	class SyntaxEDIFACTMessageHandlerForTest : SyntaxEDIFACTMessageHandler
	{
		public SyntaxEDIFACTMessageHandlerForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		public ZGuid AcknowledgementEmailGroup_Exposed => base.AcknowledgementEmailGroup;

		public ZString AcknowledgementEmailMode_Exposed => base.AcknowledgementEmailMode;

		public ZGuid ImpedimentEmailGroup_Exposed => base.ImpedimentEmailGroup;

		public ZString ImpedimentEmailMode_Exposed => base.ImpedimentEmailMode;

		public ZGuid ErrorEmailGroup_Exposed => base.ErrorEmailGroup;

		public ZString ErrorEmailMode_Exposed => base.ErrorEmailMode;

		public  BusinessObject EmailResponseLinkedObject_Exposed => base.EmailResponseLinkedObject;
	}
}
