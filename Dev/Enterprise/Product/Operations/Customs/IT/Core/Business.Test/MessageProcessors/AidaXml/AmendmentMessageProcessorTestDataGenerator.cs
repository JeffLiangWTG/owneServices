using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AmendmentMessageProcessorTestDataGenerator
{
	public AmendmentMessageProcessorTestDataGenerator(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}

	public (CusEntryHeader, EDIMessage, CusEntryLine) GetEntryInfoWithSentMessage()
		=> (entryHeader, sentMessage, entryLineOne);

	public (CusEntryPayInfo, CusEntryPayInfo, CusEntryPayInfo) GetCusEntryPayInfos()
		=> (payInfoOne, payInfoTwo, payInfoInvalidated);

	public (CusEntryLine, CusEntryLineFee, CusEntryLineFee, CusEntryLineFee) GenerateLineWithFees(short lineNumber, string paymentType)
	{
		var line = entryHeader.MergedLines.AddNew();
		line.CL_LineNumber = lineNumber;

		var feeOne = line.Fees.AddNew();
		feeOne.CF_MethodOfPayment = paymentType;
		feeOne.CF_ChargeAmount = 122.32m;
		feeOne.CF_ChargeType = "DTY";

		var feeTwo = line.Fees.AddNew();
		feeTwo.CF_MethodOfPayment = paymentType;
		feeTwo.CF_ChargeAmount = 22.76m;
		feeTwo.CF_ChargeType = "VAT";

		var feeThree = line.Fees.AddNew();
		feeThree.CF_MethodOfPayment = "E";
		feeThree.CF_ChargeAmount = 2.99m;
		feeThree.CF_ChargeType = "406";

		return (line, feeOne, feeTwo, feeThree);
	}

	public void Generate()
	{
		declaration = factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		entryLineOne = entryHeader.AllEntryLines.AddNew();
		entryLineOne.CL_LineNumber = 1;

		sentMessage = entryHeader.Messages.AddNew();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("12345");
		sentMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType;

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

		payInfoInvalidated = entryHeader.EntryPayInfos.AddNew();
		payInfoInvalidated.C9_IncomingPayResponseNo = payInfoOne.C9_IncomingPayResponseNo;
		payInfoInvalidated.C9_PaymentAmount = (payInfoOne.C9_PaymentAmount * -1);
		payInfoInvalidated.C9_PaymentDate = payInfoOne.C9_PaymentDate;
		payInfoInvalidated.C9_PaymentParty = payInfoOne.C9_PaymentParty;
		payInfoInvalidated.C9_PaymentStatus = payInfoOne.C9_PaymentStatus;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	EDIMessage sentMessage;
	CusEntryPayInfo payInfoOne, payInfoTwo, payInfoInvalidated;
	CusEntryLine entryLineOne;
	readonly BusinessObjectFactory factory;
}
