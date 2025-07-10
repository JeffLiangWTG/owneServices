using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM416;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC6.V1.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM416Processor))]
	class IM416ProcessorTest : AISH7MessageProcessorTest<IM416Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM416Provider>
	{
		protected override IM416Processor Processor => new IM416Processor(logger, typeof(Im416));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM416;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM416;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("Entry status", AISEntryStatusList.Codes.Rejected, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration Rejection (IM416) message has been received for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN123456</td></tr><tr><td>Rejection Date</td><td>01-Oct-23</td></tr><tr><td>Rejection Motivation Text</td><td>Sample rejection reason</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for H7D00000001",
				new[] { "A Customs Declaration Rejection (IM416) message has been received for Job H7D00000001." },
				new string[] { "staff1@where.com" });
		}

		public void TestStatusWithFunctionalErrorAndSpecificAdditionalDeclarationType()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();

			var messageText = GetMessageText(true, true);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions(() =>
				{
					AssertEquals("Entry status", AISEntryStatusList.Codes.Rejected, messageAttachee.ABL_BillStatus);
					AssertEquals("Logical status", LogicalStatusList.Codes.Invalid, messageAttachee.ABL_MessageStatus);
				});
			}
		}

		ZString GetMessageText(bool isExistFunctionalError = false, bool isSpecificAdditionalDeclarationType = false)
		{
			return Serialize(new Im416
			{
				Declaration = new DeclarationType
				{
					Additionaldeclarationtype = isSpecificAdditionalDeclarationType ? AdditionalDeclarationTypeList.X : "A",
					Lrn = "LRN123456",
					RejectionDate = "20231001",
					RejectionMotivationText = "Sample rejection reason",
					CustomsOffices = new CustomsOfficeLodgementType
					{
						CustomsOfficeLodgement = "AB123456"
					},
					Parties = new PartiesType
					{
						Declarant = new DeclarantType
						{
							DeclarantName = "John Doe",
							DeclarantIdentificationNumber = "ID123456",
							DeclarantAddress = new AddressType
							{
								DeclarantAddressCity = "Dublin",
								DeclarantAddressCountry = "IE",
								DeclarantAddressStreetAndNumber = "Main Street 123",
								DeclarantAddressPostCode = "D01"
							}
						}
					}
				},
				FunctionalError = !isExistFunctionalError ? null : AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeObjects()
			});
		}
	}
}
