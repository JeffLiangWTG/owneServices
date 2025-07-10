using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class BondedWarehouseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestAssessmentDate()
		{
			AssertEquals("AssessmentDate is today if date is not set on entry instruction.", ZDateTime.Today, processor.AssessmentDateExposed);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 11, 22);
			Factory.Save();
			AssertEquals("AssessmentDate comes from entry instruction if possible.", new ZDateTime(2020, 11, 22), processor.AssessmentDateExposed);
		}

		public void TestCountryCode()
		{
			AssertEquals(Core.Constants.CountryCodes.France, processor.CountryCodeExposed);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "123";
			outgoingMessage = Factory.NewWithValidTestData<FREDIMessage>();
			outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "123.";
			incomingMessage = Factory.NewWithValidTestData<DeltaCImportFREDIMessage>();
			entry.Messages.Add(incomingMessage);
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			Factory.Save();

			processor = new BondedWarehouseMessageProcessorForTest(incomingMessage.PK, null, (email, msg) => { });
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		CusEntryInstruction entryInstruction;
		FREDIMessage outgoingMessage;
		FREDIMessage incomingMessage;
		BondedWarehouseMessageProcessorForTest processor;
	}

	class BondedWarehouseMessageProcessorForTest : BondedWarehouseMessageProcessor
	{
		public BondedWarehouseMessageProcessorForTest(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed, sendMail)
		{
		}

		public ZDateTime AssessmentDateExposed => base.AssessmentDate;
		public ZString CountryCodeExposed => base.CountryCode;
	}
}
