using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CancellationMessageProcessorTestDataGenerator
{
	public CancellationMessageProcessorTestDataGenerator(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}

	public (JobDeclaration, CusEntryHeader, EDIMessage) GetEntryInfoWithSentMessage()
		=> (declaration, entryHeader, sentMessage);

	public (CusEntryPayInfo, CusEntryPayInfo) GetEntryPayInfos()
		=> (payInfoOne, payInfoTwo);

	public void Generate()
	{
		declaration = factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sentMessage = entryHeader.Messages.AddNew();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("12345");
		sentMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.Ucc6CancellationType;

		payInfoOne = entryHeader.EntryPayInfos.AddNew();
		payInfoOne.C9_IncomingPayResponseNo = "123";
		payInfoOne.C9_PaymentAmount = 1234.22m;
		payInfoOne.C9_PaymentDate = new ZDateTime(2022, 09, 01);
		payInfoOne.C9_PaymentParty = "E";
		payInfoOne.C9_PaymentStatus = "PEN";

		payInfoTwo = entryHeader.EntryPayInfos.AddNew();
		payInfoTwo.C9_IncomingPayResponseNo = "XYZ";
		payInfoTwo.C9_PaymentAmount = 789.99m;
		payInfoTwo.C9_PaymentDate = new ZDateTime(2022, 09, 04);
		payInfoTwo.C9_PaymentParty = "G";
		payInfoTwo.C9_PaymentStatus = "PEN";
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	EDIMessage sentMessage;
	CusEntryPayInfo payInfoOne, payInfoTwo;
	readonly BusinessObjectFactory factory;
}
