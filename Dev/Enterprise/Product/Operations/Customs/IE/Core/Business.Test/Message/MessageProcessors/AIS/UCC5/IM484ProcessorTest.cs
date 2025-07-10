using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM484;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM484Processor))]
	sealed class IM484ProcessorTest : EntryHeaderMessageProcessorTest<IM484Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM484Provider>
	{
		protected override ZString MessageFriendlyName => "IM484: Document Presentation Request";

		protected override IM484Processor Processor => new IM484Processor(logger, typeof(Im484));

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, @"
A Request Document Presentation (IM484) message has been received for Job B00001000.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>MRN</td>
		<td>12MRN345CDEFG678R9</td>
	</tr>
	<tr>
		<td>LRN</td>
		<td>LRN</td>
	</tr>
	<tr>
		<td>Request Date</td>
		<td>20-Sep-23</td>
	</tr>
	<tr>
		<td>Date Limit</td>
		<td>21-Sep-23</td>
	</tr>
	<tr>
		<td>Document Type</td>
		<td>D001</td>
	</tr>
	<tr>
		<td>Document reference</td>
		<td>DocInfo1</td>
	</tr>
	<tr>
		<td>Document Type</td>
		<td>D002</td>
	</tr>
	<tr>
		<td>Document reference</td>
		<td>DocInfo2</td>
	</tr>
</table>");
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request Document Presentation (IM484) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM484;

		protected override ZString MessageText => Serialize(new Im484()
		{
			Declaration = new DeclarationType()
			{
				Mrn = "12MRN345CDEFG678R9",
				Lrn25 = "LRN",
				RequestDate = "20230920",
				DateLimit = "20230921",
			},
			GoodsShipment = new Collection<DocumentAdditionalInformationType>()
			{
				new DocumentAdditionalInformationType()
				{
					DocumentType = "D001",
					DocumentComplementaryInformation = "DocInfo1"
				},
				new DocumentAdditionalInformationType()
				{
					DocumentType = "D002",
					DocumentComplementaryInformation = "DocInfo2"
				}
			}
		});
	}
}
