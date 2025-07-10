using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class BondedWarehouseEntryMessageProcessorTest : TestCaseWithFactory
{
	public void TestAssessmentDate()
	{
		var processor = GetNewProcessor();
		AssertEquals("AssessmentDate is today if date is not set on entry instruction.", ZDateTime.Today, processor.AssessmentDateExposed);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 11, 22);
		Factory.Save();
		AssertEquals("AssessmentDate comes from entry instruction if possible.", new ZDateTime(2020, 11, 22), processor.AssessmentDateExposed);
	}

	public void TestCountryCode()
	{
		var processor = GetNewProcessor();
		AssertEquals(Core.Constants.CountryCodes.Italy, processor.CountryCodeExposed);
	}

	public void TestIsOriginalError()
	{
		var processor = GetNewProcessor();
		var factory = Factory;
		entryHeader.CH_EntryStatus = ZString.Empty;
		entryHeader.CH_Status = "FFT";
		factory.Save();
		AssertEquals("When EntryStatus is empty And status is in [FFT,ERO]", true, processor.IsOriginalErrorExposed);

		processor = GetNewProcessor();
		entryHeader.CH_EntryStatus = ZString.Empty;
		entryHeader.CH_Status = "ERO";
		factory.Save();
		AssertEquals("When EntryStatus is empty And status is in [FFT,ERO]", true, processor.IsOriginalErrorExposed);

		processor = GetNewProcessor();
		entryHeader.CH_EntryStatus = "CNC";
		entryHeader.CH_Status = "ERO";
		factory.Save();
		AssertEquals("When EntryStatus is Not empty And status is in [FFT,ERO]", false, processor.IsOriginalErrorExposed);

		processor = GetNewProcessor();
		entryHeader.CH_EntryStatus = ZString.Empty;
		entryHeader.CH_Status = "ACS";
		factory.Save();
		AssertEquals("When EntryStatus is empty And status is NOT in [FFT,ERO]", false, processor.IsOriginalErrorExposed);
	}

	public void TestHasBeenWithdrawn()
	{
		var processor = GetNewProcessor();
		AssertEquals(false, processor.HasBeenWithdrawnExposed);
	}

	public void TestIsWithdrawalError()
	{
		var processor = GetNewProcessor();
		AssertEquals("When AcceptedBySystem And Cancelled", false, processor.IsWithdrawalErrorExposed);
	}

	public void TestIsAmendmentClear()
	{
		var processor = GetNewProcessor();
		AssertEquals(false, processor.IsAmendmentClearExposed);
	}

	public void IsAmendmentErrorClear()
	{
		var processor = GetNewProcessor();
		AssertEquals(false, processor.IsAmendmentErrorExposed);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var factory = Factory;

		var declaration = factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var outgoingInterchange = factory.New<EDIInterchange>();
		outgoingInterchange.EI_InterchangeNum = "123";

		var outgoingMessage = entryHeader.Messages.AddNew();
		outgoingMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("1");
		outgoingMessage.EM_EI = outgoingInterchange.PK;
		outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

		var incomingInterchange = factory.New<EDIInterchange>();
		incomingInterchange.EI_InterchangeNum = "123.";

		incomingMessage = entryHeader.Messages.AddNew();
		incomingMessage.EM_EI = incomingInterchange.PK;
		incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

		factory.Save();
	}

	BondedWarehouseMessageProcessorForTest GetNewProcessor()
		=> new BondedWarehouseMessageProcessorForTest(incomingMessage.PK, null, (email, message) => { });

	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
	ITEDIMessage incomingMessage;
}

sealed class BondedWarehouseMessageProcessorForTest : BondedWarehouseEntryMessageProcessor
{
	public BondedWarehouseMessageProcessorForTest(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed, sendMail)
	{
	}

	public ZDateTime AssessmentDateExposed => AssessmentDate;

	public ZString CountryCodeExposed => CountryCode;

	internal bool HasBeenWithdrawnExposed => HasBeenWithdrawn;
	internal bool IsWithdrawalErrorExposed => IsWithdrawalError;
	internal bool IsOriginalErrorExposed => IsOriginalError;
	internal bool IsAmendmentClearExposed => IsAmendmentClear;
	internal bool IsAmendmentErrorExposed => IsAmendmentError;
}
