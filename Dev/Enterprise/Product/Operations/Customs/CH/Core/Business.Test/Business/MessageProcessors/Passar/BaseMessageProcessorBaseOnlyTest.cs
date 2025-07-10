using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BaseMessageProcessor))]
sealed class BaseMessageProcessorBaseOnlyTest : TestCaseWithFactory
{
	public void TestCanProcess() => CombineAssertions(() =>
	{
		var ediMessage = Factory.New<EDIMessage>();
		var messageProcessor = new BaseMessageProcessorForTesting();

		messageProcessor.messageSubTypesToInclude = new ZString[] { "S01", "S02" };
		AssertCanProcess("Specific sub-types", true, "A01", "T01", "S01");
		AssertCanProcess("Specific sub-types", false, "A01", "T03", "S01");
		AssertCanProcess("Specific sub-types", false, "A01", "T01", "S03");
		AssertCanProcess("Specific sub-types, but wrong application", false, "A03", "T01", "S01");

		messageProcessor.messageSubTypesToInclude = Array.Empty<ZString>();
		AssertCanProcess("All sub-types", true, "A01", "T01", "S01");
		AssertCanProcess("All sub-types", false, "A01", "T03", "S03");
		AssertCanProcess("All sub-types, but wrong application", false, "A03", "T01", "S01");

		void AssertCanProcess(string info, bool expectedResult, string applicationCode, string messageType, string messageSubType)
		{
			ediMessage.EM_ApplicationCode = applicationCode;
			ediMessage.EM_MessageType = messageType;
			ediMessage.EM_MessageSubType = messageSubType;
			AssertEquals($"ApplicationCode={applicationCode} MessageType={messageType} MessageSubType={messageSubType} - {info}", expectedResult, messageProcessor.CanProcess(ediMessage));
		}
	});

	class BaseMessageProcessorForTesting : BaseMessageProcessor
	{
		public BaseMessageProcessorForTesting() : base(new LoggingInformation())
		{
		}

		protected override void ProcessMessageCore(CHEDIMessage message)
		{
		}

		protected override string MessageFriendlyNameCore { get; }

		protected override string ApplicationCodeCore => "A01";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { "T01", "T02" };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => messageSubTypesToInclude;
		internal IReadOnlyList<ZString> messageSubTypesToInclude;
	}
}
