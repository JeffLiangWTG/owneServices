using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM409;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC6.V1.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM409Processor))]
	class IM409ProcessorTest : AISH7MessageProcessorTest<IM409Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM409Provider>
	{
		protected override IM409Processor Processor => new IM409Processor(logger, typeof(Im409));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM409;

		protected override ZString MessageText
		{
			get
			{
				AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
				return GetMessageText(true);
			}
		}

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM409;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Cancelled, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, GetExpectedInterpretation(true));
		}

		public void TestWhenInvalidationDecisionIsFalse()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();
			var messageText = GetMessageText(false);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions(() =>
				{
					AssertEquals("Logical status", LogicalStatusList.Codes.Invalid, messageAttachee.ABL_MessageStatus);
					AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);
					AssertMessageInterpretation(incomingMessage, GetExpectedInterpretation(false));
				});
			}
		}

		ZString GetMessageText(bool invalidationDecision)
		{
			return Serialize(
				new Im409
				{
					Declaration = new DeclarationType()
					{
						DateOfInvalidation = "20240107",
						DateOfInvalidationDecision = "20240208",
						DateOfInvalidationRequest = "20240309",
						InvalidationDecision = invalidationDecision,
						InvalidationInitiatedByCustoms = true,
						InvalidationJustification = "Invalidation Justification",
						Mrn = "12MRN345CDEFG678R9",
						CustomsOffices = new CustomsOfficesType()
						{
							CustomsOfficeLodgement = "IEDUB100"
						},
						Parties = new PartiesType()
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
						}
					},
					FunctionalError = AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeObjects(),
				});
		}

		string GetExpectedInterpretation(ZBool decision) => $@"An Invalidation Request Decision (IM409) message has been received from customs for Job H7D00000001.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
				<tr><td>Invalidation Decision</td><td>{decision}</td></tr>
				<tr><td>Invalidation Initiated by Customs</td><td>Y</td></tr>
				<tr><td>Invalidation Justification</td><td>Invalidation Justification</td></tr>
				<tr><td>Date of Invalidation Decision</td><td>08-Feb-24</td></tr>
				<tr><td>Date of Invalidation Request</td><td>09-Mar-24</td></tr>
				<tr><td>Date of Invalidation</td><td>07-Jan-24</td></tr>
			</table><br />
			<br />
			{AISUCC6V1ProviderTestHelper.ExpectedFunctionalErrorInterpretation}";
	}
}
