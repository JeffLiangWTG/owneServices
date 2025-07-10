using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM405Processor))]
	class IM405ProcessorTest : EntryHeaderMessageProcessorTest<IM405Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM405Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM405;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText()
		{
			AISInterchangeProcessorTestHelper.CreateCL180ReferenceTestData(Factory);

			return Serialize(
				new Im405
				{
					ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType405
					{
						Mrn = "12MRN345CDEFG678R9",
						AmendmentRejectionDate = new DateTime(2023, 08, 10, 14, 30, 45),
						AmendmentRejectionMotivationText = "Amendment Rejection Motivation Text",
						Remarks = "Remarks001",
					},
					CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
					CustomsOfficeOfPresentation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MPcoType { ReferenceNumber = "PCO12345" },
					Representative = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MRepresentativeType { IdentificationNumber = "REP001", Status = "0" },
					Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "DEC001" },
					FunctionalError = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01>
					{
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
						{
							SequenceNumber = "1",
							ErrorPointer = "ErrorPointer001",
							ErrorCode = "13",
							ErrorReason = "ER1",
							Remarks = "Functional Error Remarks 1",
							OriginalAttributeValue = "Original Attribute Value 1",
						},
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
						{
							SequenceNumber = "2",
							ErrorPointer = "ErrorPointer002",
							ErrorCode = "52",
							ErrorReason = "ER2",
							Remarks = "Functional Error Remarks 2",
							OriginalAttributeValue = "Original Attribute Value 2",
						},
					}
				});
		}

		protected override ZString MessageFriendlyName => "IM405: Amendment Request Rejection";

		protected override IM405Processor Processor => new IM405Processor(logger, typeof(Im405));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, IM405MessageInterpreterTest.ExpectedInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Amendment Request Rejection (IM405) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
