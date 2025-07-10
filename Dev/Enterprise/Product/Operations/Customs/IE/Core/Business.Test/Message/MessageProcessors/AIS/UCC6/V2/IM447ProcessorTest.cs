using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM447;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM447Processor))]
	class IM447ProcessorTest : EntryHeaderMessageProcessorTest<IM447Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM447Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM447;

		protected override ZString MessageText => Serialize(new Im447
		{
			ImportOperation = new MCciOperationType01
			{
				Lrn = "LRN001",
				Mrn = "12MRN345ABCDE678R9",
			},
			SupervisingCustomsOffice = new MScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new MScoType { ReferenceNumber = "LCO12345" },
			Declarant = new MDeclarantType { IdentificationNumber = "DEC001" },
			ControlResult = new MControlResultType04
			{
				Code = "A4",
				Date = new DateTime(2023, 08, 11, 14, 30, 45),
				Remarks = "Remarks001",
				SupportingDocumentsProvided = "0",
			},
			ControlResults = new System.Collections.ObjectModel.Collection<MItemControlResultsType02>
			{
				new MItemControlResultsType02
				{
					SequenceNumber = "1", DeclarationGoodsItemNumber = "10001", ControlResultCode = "A4",
					ResultsOfControl = new System.Collections.ObjectModel.Collection<MControlResultType05>
					{
						new MControlResultType05
						{
							SequenceNumber = "1", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 09, 14, 30, 45), Remarks = "Control Results Remarks 011",
							ControlDetails = new System.Collections.ObjectModel.Collection<MControlDetailsType>
							{
								new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TD", AttributePointer = "Attribute Pointer 111", CorrectedValue = "Corrected Value 111", Remarks = "Control Details Remarks 111" },
								new MControlDetailsType { SequenceNumber = "2", TypeOfDiscrepancies = "TE", AttributePointer = "Attribute Pointer 112", CorrectedValue = "Corrected Value 112", Remarks = "Control Details Remarks 112" },
							}
						},
						new MControlResultType05
						{
							SequenceNumber = "2", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 10, 14, 30, 45), Remarks = "Control Results Remarks 012",
							ControlDetails = new System.Collections.ObjectModel.Collection<MControlDetailsType>
							{
								new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TF", AttributePointer = "Attribute Pointer 121", CorrectedValue = "Corrected Value 111", Remarks = "Control Details Remarks 121" },
								new MControlDetailsType { SequenceNumber = "2", TypeOfDiscrepancies = "TG", AttributePointer = "Attribute Pointer 122", CorrectedValue = "Corrected Value 112", Remarks = "Control Details Remarks 122" },
							}
						},
					}
				},
				new MItemControlResultsType02
				{
					SequenceNumber = "2", DeclarationGoodsItemNumber = "20001", ControlResultCode = "A4",
					ResultsOfControl = new System.Collections.ObjectModel.Collection<MControlResultType05>
					{
						new MControlResultType05
						{
							SequenceNumber = "1", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 11, 14, 30, 45), Remarks = "Control Results Remarks 021",
							ControlDetails = new System.Collections.ObjectModel.Collection<MControlDetailsType>
							{
								new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TH", AttributePointer = "Attribute Pointer 211", CorrectedValue = "Corrected Value 211", Remarks = "Control Details Remarks 211" },
							}
						}
					}
				},
			},
		});

		protected override ZString MessageFriendlyName => "IM447: Documentary Control Result";

		protected override IM447Processor Processor => new IM447Processor(logger, typeof(Im447));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL716", "Control Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL716", "Red", "Physical Control - R", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType("CL740", "Risk Area Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL740", "100000", "Cash control", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			return base.CreateSetupData();
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.AmendmentRequested, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Documentary Control Result (IM447) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr></table><br />
<br />Control Result<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Code</td><td>A4</td></tr><tr><td>Date</td><td>11-Aug-23</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr><tr><td>Supporting Documents Provided</td><td>0</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Results</td><td>1</td></tr><tr><td>-Declaration Goods Item Number</td><td>10001</td></tr><tr><td>-Control Result Code</td><td>A4</td></tr><tr><td>-Results of Control</td><td>1</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>09-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 011</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TD</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 111</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 111</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 111</td></tr><tr><td>--Control Details</td><td>2</td></tr><tr><td>---Type of Discrepancies</td><td>TE</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 112</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 112</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 112</td></tr><tr><td>-Results of Control</td><td>2</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>10-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 012</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TF</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 121</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 111</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 121</td></tr><tr><td>--Control Details</td><td>2</td></tr><tr><td>---Type of Discrepancies</td><td>TG</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 122</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 112</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 122</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Results</td><td>2</td></tr><tr><td>-Declaration Goods Item Number</td><td>20001</td></tr><tr><td>-Control Result Code</td><td>A4</td></tr><tr><td>-Results of Control</td><td>1</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>11-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 021</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TH</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 211</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 211</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 211</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Documentary Control Result (IM447) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		public void TestAssertWhenCodeDoesNotStardWithA()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();
			var instruction = messageAttachee.EntryInstruction;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			var messageText = Serialize(new Im447 { ControlResult = new MControlResultType04 { Code = "B1", }, });
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.NotReleased, messageAttachee.CH_EntryStatus);
			}
		}
	}
}
