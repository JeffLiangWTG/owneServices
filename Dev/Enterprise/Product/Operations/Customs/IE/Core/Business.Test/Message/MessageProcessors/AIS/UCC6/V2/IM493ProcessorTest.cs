using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM493;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM493Processor))]
	class IM493ProcessorTest : EntryHeaderMessageProcessorTest<IM493Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM493Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM493;

		protected override ZString MessageText => Serialize(new Im493
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType493
			{
				Mrn = "12MRN345CDEFG678R9",
				Lrn = "12LRN323666666",
				DeclarationType = "AA",
				AdditionalDeclarationType = "A",
				LanguageCode = "CA",
			},
			CustomsOfficeOfPresentation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MPcoType { ReferenceNumber = "PCO12345" },
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "PLO12345" },
			Importer = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MImporterType2 { },
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "DECLARANT" },
			GoodsShipment = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MGoodsShipmentType05>()
			{
				new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MGoodsShipmentType05
				{
					SequenceNumber = "1",
					Consignment = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MConsignmentType04 { ContainerIndicator = "1" },
					GoodsShipmentItem = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MGoodsShipmentItemType04>
					{
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MGoodsShipmentItemType04
						{
							SequenceNumber = "1",
							DeclarationGoodsItemNumber = "1",
							Procedure = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MProcedureType02 { RequestedProcedure = "AA", PreviousProcedure = "BB" },
							Commodity = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCommodityType04 {	DescriptionOfGoods = "AAAAA", GoodsMeasure = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MGoodsMeasureType01 { } },
							Packaging = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MPackagingType01>
							{
								new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MPackagingType01 { SequenceNumber = "1", TypeOfPackages = "BX" }
							},
						}
					},
				},
			},
		});

		protected override ZString MessageFriendlyName => "IM493: Amendment Notification for Partial or Deferred Quota Allocation";
		protected override IM493Processor Processor => new IM493Processor(logger, typeof(Im493));
		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertMessageInterpretation(incomingMessage, @"An Amendment Notification for Partial or Deferred Quota Allocation (IM493) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>LRN</td><td>12LRN323666666</td></tr></table>"
			);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
			new[] { "An Amendment Notification for Partial or Deferred Quota Allocation (IM493) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
