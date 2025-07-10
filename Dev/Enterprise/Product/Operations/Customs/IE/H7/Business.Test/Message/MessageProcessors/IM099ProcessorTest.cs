using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM099;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM099Processor))]
	class IM099ProcessorTest : AISH7MessageProcessorTest<IM099Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM099Provider>
	{
		protected override IM099Processor Processor => new IM099Processor(logger, typeof(Im099));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM099;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM099;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);
			AssertMessageInterpretation(incomingMessage, GetExpectedInterpretation());
		}

		public void TestAssertWithSpecificRemarks()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();

			CombineAssertions(() =>
			{
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "refund application required", AISEntryStatusList.Codes.RefundApplicationRequested);
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "REFUND APPLICATION REQUIRED", AISEntryStatusList.Codes.RefundApplicationRequested);
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "insufficient fund", AISEntryStatusList.Codes.InsufficientFund);
				AssertProcessedMessageStatus(incomingMessage, messageAttachee, "Insufficient Fund", AISEntryStatusList.Codes.InsufficientFund);
			});
		}

		void AssertProcessedMessageStatus(AISInboundEDIMessage incomingMessage, AsycudaBill messageAttachee, string remarks, string expectedStatus)
		{
			var serializedMessageText = GetMessageText(remarks);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, serializedMessageText, includeResponseWrap: false);
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			messageAttachee.ABL_BillStatus = string.Empty;

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
				AssertEquals($"Entry status should correspond to the message remark: {remarks}", expectedStatus, messageAttachee.ABL_BillStatus);
				AssertMessageInterpretation(incomingMessage, GetExpectedInterpretation(remarks));
			}
		}

		ZString GetMessageText(string remarks = "Remarks001")
		{
			return Serialize(
				new Im099
				{
					Declaration = new DeclarationType()
					{
						Lrn = "LRN001",
						DateLimitOfResponse = "20230801",
						CustomsOffices = new CustomsOfficeLodgementType() { CustomsOfficeLodgement = "LCO12345" },
						Parties = new DeclarantOnlyType()
						{
							Declarant = new DeclarantType()
							{
								DeclarantName = "Tony",
								DeclarantIdentificationNumber = "DC012345",
								DeclarantAddress = new AddressType()
								{
									DeclarantAddressCity = "New York",
									DeclarantAddressCountry = "US",
									DeclarantAddressStreetAndNumber = "No.1 of Wall Street",
									DeclarantAddressPostCode = "100000"
								}
							}
						},
						Remarks = remarks
					}
				});
		}

		string GetExpectedInterpretation(string remarks = "Remarks001") => $@"A General Notification and Request Information (IM099) message has been received for Job H7D00000001.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>LRN</td><td>LRN001</td></tr>
				<tr><td>Date Limit of Response</td><td>01-Aug-23</td></tr>
				<tr><td>Remarks</td><td>{remarks}</td></tr>
				<tr><td>Customs Office Lodgement</td><td>LCO12345</td></tr>
			</table>";
	}
}
