using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM416;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM416Processor))]
	sealed class IM416ProcessorTest : EntryHeaderMessageProcessorTest<IM416Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM416Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM416;

		protected override ZString MessageText => Serialize(GetMessageObject());

		protected override ZString MessageFriendlyName => "IM416: Customs Declaration Rejection";

		protected override IM416Processor Processor => new IM416Processor(logger, typeof(Im416));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Rejected, messageAttachee.CH_EntryStatus);
			var jobNumber = messageAttachee.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Customs Declaration Rejection (IM416) message has been received for Job {jobNumber}.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Rejection Date</td><td>10-Aug-23</td></tr><tr><td>Rejection Motivation Text</td><td>Rejection Motivation Text</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		Im416 GetMessageObject(bool isExistFunctionalError = false)
		{
			return new Im416
			{
				Declaration = new DeclarationType()
				{
					DeclarationType11 = "EX",
					AdditionalDeclarationType12 = "A",
					Lrn25 = "LRN001",
					RejectionDate = "20230810",
					RejectionMotivationText = "Rejection Motivation Text",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IEDUB100"
					}
				},
				FunctionalError = !isExistFunctionalError ? null : new Collection<FunctionalErrorType>
				{
					new FunctionalErrorType()
					{
						ErrorPointer = "Error Pointer 1",
						ErrorReason = "Error Reason"
					},
				},
			};
		}
	}
}
