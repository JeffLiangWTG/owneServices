using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM482;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM482Processor))]
	sealed class IM482ProcessorTest : EntryHeaderMessageProcessorTest<IM482Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM482Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM482;

		protected override ZString MessageFriendlyName => "IM482: Documents Request";

		protected override IM482Processor Processor => new IM482Processor(logger, typeof(Im482));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertMessageInterpretation(incomingMessage, @"
A Document Request (IM482) message has been received for Job B00001000.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>MRN</td>
		<td>12MRN345CDEFG678R9</td>
	</tr>
	<tr>
		<td>LRN</td>
		<td>LRN123</td>
	</tr>
	<tr>
		<td>Request Date</td>
		<td>14-Mar-24</td>
	</tr>
	<tr>
		<td>Date Limit</td>
		<td>13-Apr-24</td>
	</tr>
	<tr>
		<td>Document Type</td>
		<td>Y023</td>
	</tr>
	<tr>
		<td>Document reference</td>
		<td>comp info1</td>
	</tr>
	<tr>
		<td>Document Type</td>
		<td>U713</td>
	</tr>
	<tr>
		<td>Document reference</td>
		<td>comp info 2</td>
	</tr>
</table>");

			var requestedDocuments = messageAttachee.EntryInstruction.RequestedDocuments;
			AssertEquals("RequestedDocuments.Count", 2, requestedDocuments.Count);
			var doc1 = requestedDocuments[0];
			AssertEquals("CSI_Code", "Y023", doc1.CSI_Code);
			AssertEquals("RequestInformation", "comp info1", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2024, 3, 14), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2024, 4, 13), doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, doc1.CSI_Status);
			var doc2 = requestedDocuments[1];
			AssertEquals("CSI_Code", "U713", doc2.CSI_Code);
			AssertEquals("RequestInformation", "comp info 2", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2024, 3, 14), doc2.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2024, 4, 13), doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc2.CSI_Status);
		}

		protected override ZString MessageText => Serialize(new Im482
		{
			Declaration = new DeclarationType()
			{
				Mrn = "12MRN345CDEFG678R9",
				Lrn25 = "LRN123",
				RequestDate = "20240314",
				DateLimit = "20240413",
			},
			AdditionalInformation = new Collection<DocumentAdditionalInformationType>()
			{
				new DocumentAdditionalInformationType()
				{
					DocumentComplementaryInformation = "comp info1",
					DocumentType = "Y023",
				},
				new DocumentAdditionalInformationType()
				{
					DocumentComplementaryInformation = "comp info 2",
					DocumentType = "U713",
				}
			}
		});

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			result.messageAttachee.CH_CEI_Instruction = entryInstruction.PK;
			var docExisted = result.messageAttachee.EntryInstruction.RequestedDocuments.AddNew();

			docExisted.CSI_Code = "Y023";
			docExisted.CSI_Description = "comp info";
			docExisted.CSI_AdditionalDescription = "1";
			docExisted.CSI_DateOfIssue = new ZDateTime(2024, 3, 14);
			docExisted.CSI_DateOfExpiry = new ZDateTime(2024, 4, 13);
			docExisted.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			return result;
		}
	}
}
