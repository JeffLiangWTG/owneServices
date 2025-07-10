using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM428;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM428Processor))]
	class IM428ProcessorTest : EntryHeaderMessageProcessorTest<IM428Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM428Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM428;

		protected override ZString MessageText => GetMessageText(needCreateTaxes: true);

		protected override ZString MessageFriendlyName => "IM428: Customs Declaration Acceptance";

		protected override IM428Processor Processor => new IM428Processor(logger, typeof(Im428));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Accepted, messageAttachee.CH_EntryStatus);

			AssertEquals("MovementReferenceNumber", "12MRN345CDEFG678R9", messageAttachee.MovementReferenceNumber);
			AssertEquals("MovementReferenceNumberIssueDate", new DateTime(2023, 08, 10, 14, 30, 45), messageAttachee.MovementReferenceNumberIssueDate);

			MessageProcessorHelperTest.AssertConfirmedDutiesAndTaxesAdded(messageAttachee.MergedLines[0]);

			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration Acceptance (IM428) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Customs Registration Number</td><td>12CRN345ABCDE678R9</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Declaration Acceptance Date and Time</td><td>10-Aug-23 14:30</td></tr><tr><td>Declaration Type</td><td>IM</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Response Date Limit</td><td>11-Aug-23</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Customs Declaration Acceptance (IM428) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		public void TestMRNWithSpecificAdditionalDeclarationType()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();
			messageAttachee.MovementReferenceNumberSetter(originalMRN);

			var messageText = GetMessageText(needCreateTaxes: false, isSpecificAdditionalDeclarationType: true);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals("SimplifiedDeclarationMRN ", SimplifiedDeclarationMRNWithSpecificAdditionalDeclarationType, messageAttachee.SimplifiedDeclarationMRN);
				AssertEquals("MovementReferenceNumber ", "12MRN345CDEFG678R9", messageAttachee.MovementReferenceNumber);
			}
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var entryHeader = result.messageAttachee;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 12345;

			return result;
		}

		protected virtual ZString SimplifiedDeclarationMRNWithSpecificAdditionalDeclarationType => originalMRN;

		protected virtual ZString GetMessageText(bool needCreateTaxes = false, bool isSpecificAdditionalDeclarationType = false)
		{
			return Serialize(new Im428
			{
				ImportOperation = new MCciOperationType45
				{
					Lrn = "LRN001",
					CustomsRegistrationNumber = "12CRN345ABCDE678R9",
					Mrn = "12MRN345CDEFG678R9",
					DeclarationAcceptanceDateAndTime = new DateTime(2023, 08, 10, 14, 30, 45),
					DeclarationType = "IM",
					AdditionalDeclarationType = isSpecificAdditionalDeclarationType ? AdditionalDeclarationTypeList.C : "A",
					ResponseDateLimit = new DateTime(2023, 08, 11, 14, 30, 45),
					PreferredPaymentMethod = "A",
					Remarks = "Remarks001",
				},
				SupervisingCustomsOffice = new MScoType { ReferenceNumber = "SCO12345" },
				CustomsOfficeLodgement = new MScoType { ReferenceNumber = "LCO12345" },
				Declarant = new MDeclarantType { IdentificationNumber = "ID1" },
				Representative = new MRepresentativeType { IdentificationNumber = "REPRESENTATIVE", Status = "0" },
				DeferredPayment = new System.Collections.ObjectModel.Collection<MDeferredPaymentType> { },
				GoodsShipment = new Im428GoodsShipmentType
				{
					Taxes = new Taxes02Type { },
					GoodsShipmentItem = new System.Collections.ObjectModel.Collection<Im428GoodsShipmentItemType>
				{
					new Im428GoodsShipmentItemType
					{
						DeclarationGoodsItemNumber = "12345",
						CalculationOfTaxes = needCreateTaxes ? CreateCulationOfTaxes() : null,
					},
				},
				},
			});
		}

		MCalculationOfTaxesType03 CreateCulationOfTaxes()
		{
			return new MCalculationOfTaxesType03()
			{
				DutiesAndTaxes = MessageProcessorHelperTest.CreateDutiesAndTaxesType03Data()
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalMRN = "00MRN345CDEFG678R9";
		}

		ZString originalMRN;
	}
}
