using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM428Processor))]
	sealed class IM428ProcessorTest : EntryHeaderMessageProcessorTest<IM428Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM428Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM428;

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardUCC5IM428Text();

		protected override ZString MessageFriendlyName => "IM428: MRN allocation message";

		protected override IM428Processor Processor => processor;

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Accepted, messageAttachee.CH_EntryStatus);

			AssertEquals("MovementReferenceNumber", AISInterchangeProcessorTestHelper.Typical.Mrn, messageAttachee.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate", new DateTime(2024, 02, 22), messageAttachee.MovementReferenceNumberIssueDate);

			AssertMessageInterpretation(incomingMessage, IM428MessageInterpreterTest.DefaultInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(
				$"{incomingMessage.MessageTypeWithDescription} Response for {AISInterchangeProcessorTestHelper.Typical.JobNumber}",
				new[] { $"An Export MRN Allocation (IM428) message has been received from customs for Job {AISInterchangeProcessorTestHelper.Typical.JobNumber}" },
				new string[] { "staff1@where.com" });
		}

		IM428Processor processor;
		protected override void SetUp()
		{
			base.SetUp();
			processor = new IM428Processor(logger, typeof(Im428));
		}
	}
}
