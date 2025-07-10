using System;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestDate(2025, 5, 26, 11, 48, 00)]
	public abstract class G3CommonResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : ESCommonResponseMessageProcessorTest<TResponse, AsycudaManifestHeader, TResponseProvider>
		where TResponse : G3CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode, IG3Lrn, IG3CommonErrors
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			if (businessObject is AsycudaManifestHeader header)
			{
				AssertEquals("AMA_MessageStatus", EDIMessageStatusList.Codes.Failed, header.AMA_MessageStatus);
			}

			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "There is an error in XML document ", concatenatedUserLogStrings);
			AssertContains("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: There is an error in XML document ", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		protected void AssertReferencedBillSendsQueryH7(AsycudaBill bill, string expectedMrn)
		{
			var sentMessage = bill.Messages.Where(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.H7Query).FirstOrDefault();
			var delayTime = new DateTime(2025, 5, 26, 11, 50, 30);
			AssertNotNull("Query H7 message should be sent", sentMessage);
			Assert("Query H7 message should include MRN", sentMessage.EM_MessageText.Contains($"<MRN_H7>{expectedMrn}</MRN_H7>"));
			AssertEquals("Query H7 message's EM_HeldUntilDate as EM_SystemCreateTimeUTC plus 150 seconds.", delayTime, sentMessage.EM_HeldUntilDate);
		}

		protected void AssertReferencedBillHasNoH7QuerySent(AsycudaBill bill)
		{
			var messages = bill.Messages.Where(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.H7Query);
			AssertNullOrEmpty("Pre-condition", bill.H7MovementReferenceNumber);
			AssertEquals("Query H7 Message should not be sent", 0, messages.Count());
		}

		protected void AssertMessage(TestEdiMessage message, string expectedSubType, string expectedStatus, string expectedMessage)
		{
			AssertEquals("Message Sub Type", expectedSubType, message.EM_MessageSubType);
			AssertEquals("Message Status", expectedStatus, message.EM_Status);
			AssertEquals("Message Interpretation", expectedMessage, message.EM_MessageInterpretation);
		}

		protected void AssertReferencedBillsMessageStatus(string expectedMessageStatus)
		{
			AssertEquals($"referencedBill1 should be {expectedMessageStatus}", expectedMessageStatus, referencedBill1.ABL_MessageStatus);
			AssertEquals($"referencedBill2 should be {expectedMessageStatus}", expectedMessageStatus, referencedBill2.ABL_MessageStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = ApplicationReference;

			staffWithCertificateHelper = new StaffWithCertificateTestHelper(Factory);
			header.AMA_GS_NKCustomsAgent = staffWithCertificateHelper.Staff.GS_Code;

			SetSentInterchange(header, InterchangeID);

			referencedBill1 = header.Bills.AddNew();
			referencedBill1.ABL_BillNumber = "ES000002";
			referencedBill1.H7MovementReferenceNumber = bill1Mrn;
			referencedBill1.G3LocalReferenceNumber = billLrnResponse;

			referencedBill2 = header.Bills.AddNew();
			referencedBill2.ABL_BillNumber = "ES000003";
			referencedBill2.H7MovementReferenceNumber = bill2Mrn;
			referencedBill2.G3LocalReferenceNumber = billLrnResponse;

			referencedBill3 = header.Bills.AddNew();
			referencedBill3.ABL_BillNumber = "ES000003";
			referencedBill3.G3LocalReferenceNumber = billLrnResponse;
			Factory.Save();
		}

		protected AsycudaManifestHeader header;
		protected AsycudaBill referencedBill1;
		protected AsycudaBill referencedBill2;
		protected AsycudaBill referencedBill3;

		protected const string bill1Mrn = "24ES123456A0000001";
		protected const string bill2Mrn = "24ES123456A0000002";
		protected const string billLrnResponse = "ACME20REF0000001";

		protected StaffWithCertificateTestHelper staffWithCertificateHelper;
	}
}
