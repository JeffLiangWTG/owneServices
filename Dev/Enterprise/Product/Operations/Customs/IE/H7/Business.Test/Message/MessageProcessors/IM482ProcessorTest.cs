using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM482;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM482Processor))]
	class IM482ProcessorTest : AISH7MessageProcessorTest<IM482Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM482Provider>
	{
		protected override IM482Processor Processor => new IM482Processor(logger, typeof(Im482));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM482;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM482;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);
			var requestedDocuments = messageAttachee.RequestedDocuments;
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

			AssertMessageInterpretation(incomingMessage, @"A Documents Request (IM482) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>LRN123</td></tr><tr><td>Request Date</td><td>14-Mar-24 00:00</td></tr><tr><td>Date Limit</td><td>13-Apr-24 00:00</td></tr></table><br />
<br />Document Additional Information: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Type</td><td>Y023</td></tr><tr><td>Complementary Information</td><td>comp info1</td></tr></table><br />
<br />Document Additional Information: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Type</td><td>U713</td></tr><tr><td>Complementary Information</td><td>comp info 2</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im482
				{
					Declaration = new DeclarationType()
					{
						Mrn = "12MRN345CDEFG678R9",
						Lrn = "LRN123",
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
				}
			);
		}

		protected override (AsycudaManifestHeader declaration, AsycudaBill messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var docExisted = result.messageAttachee.RequestedDocuments.AddNew();

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
