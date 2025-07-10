using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC6.V1.Testing;
using NUnit.Framework;
using IM409 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM409.Im409;
using IM409Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM409MessageInterpreter))]
	class IM409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM409MessageInterpreter, IIM409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM409;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest() => CreateIncomingMessage(true);

		AISInboundEDIMessage CreateIncomingMessage(bool invalidationDecision)
		{
			AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(GenerateMessage(invalidationDecision)));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => GetExpectedInterpretation(true);

		protected override IIM409Provider GetProvider(TextReader reader) => new IM409Provider(new MailBoxItemProvider<IM409>(reader).Message);

		public void TestWhenInvalidationDecisionIsFalse()
		{
			var message = CreateIncomingMessage(false);
			message.EM_MessageType = MessageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals(GetExpectedInterpretation(false).RemoveLineBreakingsAndIndents(), interpreter.GetInterpretation().RemoveLineBreakingsAndIndents());
		}

		internal static string GetExpectedInterpretation(ZBool decision) => $@"An Invalidation Request Decision (IM409) message has been received from customs for Job B00001000.<br />
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

		IM409 GenerateMessage(bool invalidationDecision)
		{
			return new IM409
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
			};
		}
	}
}
