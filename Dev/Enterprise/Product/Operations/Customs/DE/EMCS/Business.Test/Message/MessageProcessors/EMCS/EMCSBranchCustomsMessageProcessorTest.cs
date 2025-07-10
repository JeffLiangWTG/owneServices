using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class EMCSBranchCustomsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestResolveEMCSMessageProcessors()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsEmcsSystem;
			message.EM_MessageSubType = Messaging.EmcsMessageSubTypeList.Codes.Emb;
			var messages = EmcsResponseMessageDetails.Instance.ResponseMessages.Select(d => d.Key).ToList();
			CombineAssertions(() =>
			{
				foreach (var technicalMessageName in messages)
				{
					message.EM_ApplicationReference = technicalMessageName;
					var prc = processor.GetApplicationTypeProcessorCore(message);
					AssertEquals(technicalMessageName, true, prc is BranchCustomsApplicationTypeMessageProcessor);
				}
			});
		}

		public void TestResolveEMCSMessageProcessors_InvalidApplicationReference()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsEmcsSystem;
			message.EM_ApplicationReference = "GCRECE";
			var prc = processor.GetApplicationTypeProcessorCore(message);
			AssertNull(prc);
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new EMCSBranchCustomsMessageProcessor
			{
				Logger = new LoggingInformation()
			};
		}
		EMCSBranchCustomsMessageProcessor processor;
	}
}
