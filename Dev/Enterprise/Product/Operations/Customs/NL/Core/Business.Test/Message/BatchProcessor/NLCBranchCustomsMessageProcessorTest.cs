using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLCBranchCustomsMessageProcessorTest : TestCaseWithFactory
{
	public void TestMessageProcessors_DMS()
	{
		var branchCustomsMessageProcessor = new NLCBranchCustomsMessageProcessor
		{
			Logger = new LoggingInformation()
		};
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NLCustoms;
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;

		CombineAssertions(() =>
		{
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC404A, typeof(IE404And504MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC410A, typeof(IE410MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC426A, typeof(IE426MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC428A, typeof(IE428And528MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC429A, typeof(IE429And529MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC431A, typeof(IE431And531MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC531C, typeof(IE531MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC438A, typeof(IE438MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC451A, typeof(IE451MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC456A, typeof(IE456And556MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC460A, typeof(IE460And560MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC504C, typeof(IE404And504MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC509C, typeof(IE509MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC528C, typeof(IE428And528MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC529C, typeof(IE429And529MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC551C, typeof(IE551MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC556C, typeof(IE456And556MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC560C, typeof(IE460And560MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC582C, typeof(IE582MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CC599C, typeof(IE599MessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CCEXTA, typeof(EXTMessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CCRCVA, typeof(RCVMessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CCREGA, typeof(REGMessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CCRFIA, typeof(RFIMessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CCRRDA, typeof(RRDMessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.Control, typeof(ControlMessageProcessor));
			AssertMessageProcessor(NLIncomingMessageSubTypeList.Codes.CCAMDA, typeof(AMDMessageProcessor));
		});

		void AssertMessageProcessor(string messageSubType, Type messageProcessorType)
		{
			message.EM_MessageSubType = messageSubType;
			var processor = branchCustomsMessageProcessor.GetApplicationTypeProcessorCore(message);
			AssertEquals(messageSubType, messageProcessorType, processor.GetType());
		}
	}

	public void TestMessageProcessors_NCTS()
	{
		var branchCustomsMessageProcessor = new NLCBranchCustomsMessageProcessor
		{
			Logger = new LoggingInformation()
		};
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NLCustoms;
		message.EM_MessageType = NLEDIMessageTypes.Codes.NCT;
		message.EM_MessageSubType = NLIncomingMessageSubTypeList.Codes.CC140C;
		var processor = branchCustomsMessageProcessor.GetApplicationTypeProcessorCore(message);
		AssertNotNull(processor);
	}

	public void TestExcludeBranchFilter()
	{
		var branchCustomsMessageProcessor = new NLCBranchCustomsMessageProcessorForTest();
		AssertEquals("ExcludeBranchFilter should be true", true, branchCustomsMessageProcessor.ExcludeBranchFilterExposed);
	}

	public void TestMessageShouldBeProcessedInASeparateFactory()
	{
		var branchCustomsMessageProcessor = new NLCBranchCustomsMessageProcessorForTest();
		AssertEquals("MessageShouldBeProcessedInASeparateFactory should be true", true, branchCustomsMessageProcessor.MessageShouldBeProcessedInASeparateFactoryExposed);
	}

	sealed class NLCBranchCustomsMessageProcessorForTest : NLCBranchCustomsMessageProcessor
	{
		public NLCBranchCustomsMessageProcessorForTest() : base()
		{
		}

		public bool ExcludeBranchFilterExposed => ExcludeBranchFilter;

		public bool MessageShouldBeProcessedInASeparateFactoryExposed => MessageShouldBeProcessedInASeparateFactory;
	}
}
