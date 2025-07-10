using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM416;
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
	[TestedType(typeof(IM416Processor))]
	class IM416ProcessorTest : EntryHeaderMessageProcessorTest<IM416Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM416;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString EntryStatusWhenIsSpecificAdditionalDeclarationType => AISEntryStatusList.Codes.AwaitingSupplementaryDeclaration;

		protected override ZString MessageFriendlyName => "IM416: Customs Declaration Rejection";

		protected override IM416Processor Processor => new IM416Processor(logger, typeof(Im416));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Rejected, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration Rejection (IM416) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Rejection Date</td><td>10-Aug-23</td></tr><tr><td>Rejection Motivation Text</td><td>Rejection Motivation Text</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Customs Declaration Rejection (IM416) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		public void TestStatusWithFunctionalErrorAndSpecificAdditionalDeclarationType()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();

			var messageText = GetMessageText(isExistFunctionalError: true, isSpecificAdditionalDeclarationType: true);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals("CH_EntryStatus", EntryStatusWhenIsSpecificAdditionalDeclarationType, messageAttachee.CH_EntryStatus);
				AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);
			}
		}

		protected virtual ZString GetMessageText(bool isExistFunctionalError = false, bool isSpecificAdditionalDeclarationType = false)
		{
			return Serialize(new Im416
			{
				ImportOperation = new MCciOperationType416
				{
					AdditionalDeclarationType = isSpecificAdditionalDeclarationType ? AdditionalDeclarationTypeList.X : "A",
					Lrn = "LRN001",
					RejectionDate = new DateTime(2023, 08, 10, 14, 30, 45),
					RejectionMotivationText = "Rejection Motivation Text",
				},
				CustomsOfficeLodgement = new MScoType { ReferenceNumber = "LCO12345" },
				Representative = new MRepresentativeType { IdentificationNumber = "REPRESENTATIVE", Status = "0" },
				Declarant = new MDeclarantType { IdentificationNumber = "DECLARANT" },
				FunctionalError = !isExistFunctionalError ? null : new Collection<MFunctionalErrorType01>
				{
					new MFunctionalErrorType01()
					{
						SequenceNumber = "1",
						ErrorPointer = "Error Pointer 1",
						ErrorCode = "21",
						ErrorReason = "Error Reason"
					},
				},
			});
		}
	}
}
