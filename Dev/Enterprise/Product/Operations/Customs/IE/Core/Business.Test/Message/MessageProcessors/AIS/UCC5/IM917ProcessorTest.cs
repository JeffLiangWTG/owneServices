using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM917;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM917Processor))]
	sealed class IM917ProcessorTest : EntryHeaderMessageProcessorTest<IM917Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM917Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Error, entry.CH_Status);
		}

		protected override ZString MessageFriendlyName => "IM917: Syntax Error Notification";

		protected override IM917Processor Processor => new IM917Processor(logger, typeof(Im917));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM917;

		protected override ZString MessageText => Serialize(
			new Im917
			{
				XmlNegativeAcknowledgement = new System.Collections.ObjectModel.Collection<XmlNegativeAcknowledgement>(new[]
				{
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "1",
						ErrorColumnNumber = "2",
						ErrorReason = "BAD"
					},
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "3",
						ErrorColumnNumber = "4",
						ErrorReason = "THRILLER"
					}
				})
			});
	}
}
