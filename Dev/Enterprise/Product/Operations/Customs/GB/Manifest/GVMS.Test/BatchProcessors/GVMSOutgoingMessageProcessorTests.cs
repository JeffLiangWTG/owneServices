using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.GB.GVMS.Constants;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	class GVMSOutgoingMessageProcessorTests : TestCaseWithFactory
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestProcess()
		{
			GVMSMessageSenderTestHelper.TestProcess(() =>
			{
				var processor = new GVMSOutgoingMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(CancellationToken.None);
			});
		}
		public void TestOutgoingMessageTypes()
		{
			var messageTypeList = GVMSOutgoingMessageProcessor.GetOutgoingMessageSubTypes().ToList<string>();

			CombineAssertions(() =>
			{
				Assert("OutgoingMessageTypes should contain NEW", messageTypeList.Contains(GVMSMessageSubTypes.NEW));
				Assert("OutgoingMessageTypes should contain AMEND", messageTypeList.Contains(GVMSMessageSubTypes.AMEND));
				Assert("OutgoingMessageTypes should contain CANCEL", messageTypeList.Contains(GVMSMessageSubTypes.CANCEL));
				Assert("OutgoingMessageTypes should contain FINALISE", messageTypeList.Contains(GVMSMessageSubTypes.FINALISE));
			});
		}
	}
}
