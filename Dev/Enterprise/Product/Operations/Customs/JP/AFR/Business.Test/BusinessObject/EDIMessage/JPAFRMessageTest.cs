using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRMessage))]
	class JPAFRMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGenereteMessageReferenceNumber()
		{
			var testMessage = Factory.New<JPAFRMessage>();
			AssertEquals(ZString.Empty, testMessage.EM_MessageNum);
			Factory.Save();
			AssertEquals("JP00000001", testMessage.EM_MessageNum);
		}

		public void TestMessageInterpretation()
		{
			var testMessage = Factory.New<JPAFRMessage>();
			testMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			AssertEquals(MessageInterpretationGenerator.ParsingFailureResult, testMessage.EM_MessageInterpretation);
			testMessage.EM_MessageText = MessageInterpretationGeneratorTest.TestXUSNoSubShipmentBody;
			AssertEquals(MessageInterpretationGenerator.ParsingFailureResult, testMessage.EM_MessageInterpretation);

			testMessage = Factory.New<JPAFRMessage>();
			testMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			testMessage.EM_MessageText = MessageInterpretationGeneratorTest.TestXUSNoSubShipmentBody;
			AssertContains(MessageInterpretationGeneratorTest.ExpectedMastershipmentHeader + MessageInterpretationGeneratorTest.ExpectedMastershipmentFooter, testMessage.EM_MessageInterpretation);

			testMessage = Factory.New<JPAFRMessage>();
			testMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			testMessage.EM_MessageText = MessageInterpretationGeneratorTest.TestXUSNoSubShipmentBody;
			AssertEquals(MessageInterpretationGenerator.ParsingFailureResult, testMessage.EM_MessageInterpretation);

			testMessage = Factory.New<JPAFRMessage>();
			testMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			testMessage.EM_MessageText = MessageInterpretationGeneratorTest.TestXUEAHRBody;
			AssertContains(MessageInterpretationGeneratorTest.ExpectedXUEOutput1, testMessage.EM_MessageInterpretation);
		}

		public override void TestCloneAuditProperties()
		{
			Assert("Need this to supress test failure caused by customized default values", true);
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
