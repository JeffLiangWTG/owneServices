using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class BondedWarehouseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestAssesmentDate()
		{
			AssertEquals("AssessmentDate is today if date is not set on entry instruction.", ZDateTime.Today, processor.AssessmentDateExposed);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 11, 22);
			Factory.Save();
			AssertEquals("AssessmentDate comes from entry instruction if possible.", new ZDateTime(2020, 11, 22), processor.AssessmentDateExposed);
		}

		public void TestCountryCode()
		{
			AssertEquals(Core.Constants.CountryCodes.Germany, processor.CountryCodeExposed);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var outgoinginterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoinginterchange.EI_InterchangeNum = "111";
			var outgoingMessage = Factory.NewWithValidTestData<AtlasEDIMessage>();

			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = outgoinginterchange.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "222";
			var incomingMessage = Factory.NewWithValidTestData<AtlasEDIMessage>();
			entry.Messages.Add(incomingMessage);
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			Factory.Save();

			processor = new BondedWarehouseMessageProcessorForTest(incomingMessage.PK, null, (email, message) => { });
		}

		CusEntryInstruction entryInstruction;
		BondedWarehouseMessageProcessorForTest processor;
	}

	class BondedWarehouseMessageProcessorForTest : BondedWarehouseMessageProcessor
	{
		public BondedWarehouseMessageProcessorForTest(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed, sendMail)
		{
		}

		public ZDateTime AssessmentDateExposed => AssessmentDate;

		public ZString CountryCodeExposed => CountryCode;
	}
}
