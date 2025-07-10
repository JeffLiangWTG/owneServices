using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NewResponseMessageSubProcessorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NewResponseMessageSubProcessor(null));
	}

	public void TestStoreMrnIfPresent()
	{
		var responseMessage = new Data { Mrn = "MRN2022" };
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeaderOne);
		var sentMessage = entryHeaderOne.Messages.AddNew();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123");
		var subProcessor = new NewResponseMessageSubProcessor(sentMessage);
		subProcessor.ProcessMessage(adapter, responseMessage);
		Factory.Save();

		var mrnEntry = CusEntryNumber.Load(entryHeaderOne, "MRN", "IT", true);
		AssertNotNull("MRN Entry", mrnEntry);
		AssertEquals("MRN Entry Value", responseMessage.Mrn, mrnEntry.CE_EntryNum);
		AssertEquals("Entry Status", "REG", entryHeaderOne.CH_EntryStatus);

		responseMessage = new Data();
		adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeaderTwo);
		sentMessage = entryHeaderTwo.Messages.AddNew();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123");
		subProcessor = new NewResponseMessageSubProcessor(sentMessage);
		subProcessor.ProcessMessage(adapter, responseMessage);
		Factory.Save();

		mrnEntry = CusEntryNumber.Load(entryHeaderTwo, "MRN", "IT", true);
		AssertNull("MRN Entry", mrnEntry);
	}

	public void TestProcessReleaseItems_LineLevelItem()
	{
		var responseMessage = new Data
		{
			Informazione = new Collection<DataInformazione>
			{
				new DataInformazione { Livello = "S", NumeroArticolo = 1, Data = "01/01/2022", Numero = "1" }
			}
		};
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeaderOne);
		var sentMessage = entryHeaderOne.Messages.AddNew();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123");
		var subProcessor = new NewResponseMessageSubProcessor(sentMessage);
		subProcessor.ProcessMessage(adapter, responseMessage);

		var clearanceCode = CusEntryNumber.Load(lineOneHeaderOne, "CLR", "IT", true);
		AssertNotNull("Clearance Code Entry", clearanceCode);
		AssertEquals("CLR Value", "1", clearanceCode.CE_EntryNum);
	}

	public void TestProcessReleaseItems_ForHeader()
	{
		var responseMessage = new Data
		{
			Informazione = new Collection<DataInformazione>
			{
				new DataInformazione { Livello = "S", Data = "01/01/2022", Numero = "1" }
			}
		};
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeaderOne);
		var sentMessage = entryHeaderOne.Messages.AddNew();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123");
		var subProcessor = new NewResponseMessageSubProcessor(sentMessage);
		subProcessor.ProcessMessage(adapter, responseMessage);

		var clearanceCode = CusEntryNumber.Load(entryHeaderOne, "CLR", "IT", true);
		AssertNotNull("Clearance Code Entry", clearanceCode);
		AssertEquals("CLR Value", "1", clearanceCode.CE_EntryNum);
		AssertEquals("Entry Status", ITEntryStatusList.Codes.ImportCleared, entryHeaderOne.CH_EntryStatus);
	}

	public void TestProcess_AddA93Numbers()
	{
		var responseMessage = new Data
		{
			Informazione = new Collection<DataInformazione>
			{
				new DataInformazione { Livello = "P", Data = "2022-23/03/2022E-23/03/2022G", Numero = "1" }
			}
		};
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeaderOne);
		var sentMessage = entryHeaderOne.Messages.AddNew();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123");
		var subProcessor = new NewResponseMessageSubProcessor(sentMessage);
		subProcessor.ProcessMessage(adapter, responseMessage);
		entryHeaderOne.EntryPayInfos.Load();

		var entryPayInfos = entryHeaderOne.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		var payInfoHeaderWithTypeE = entryPayInfos.SingleOrDefault(e => e.A93Number == "1" && e.MethodOfPayment == "E");
		var payInfoHeaderWithTypeG = entryPayInfos.SingleOrDefault(e => e.A93Number == "1" && e.MethodOfPayment == "G");

		AssertNotNull("A93Number Payment Type E", payInfoHeaderWithTypeE);
		AssertNotNull("A93Number Payment Type G", payInfoHeaderWithTypeG);

		CombineAssertions(() =>
		{
			AssertEquals("Amount - A93Number Payment Type E", 32.22m, payInfoHeaderWithTypeE.C9_PaymentAmount);
			AssertEquals("Amount - A93Number Payment Type G", 119.97m, payInfoHeaderWithTypeG.C9_PaymentAmount);

			var invalidatedEntry = entryPayInfos.SingleOrDefault(e => e.A93Number == payInfo.A93Number && e.C9_PaymentAmount < 0);
			AssertNull("Invalidated Entry", invalidatedEntry);
		});
	}

	public void TestProcessDepositedDeclarationMessage()
	{
		var responseMessage = new Data() { Stato = 2 };
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeaderOne);
		var subProcessor = new NewResponseMessageSubProcessor(Factory.New<EDIMessage>());

		subProcessor.ProcessMessage(adapter, responseMessage);
		AssertEquals(nameof(entryHeaderOne.CH_EntryStatus), "DEP", entryHeaderOne.CH_EntryStatus);
	}

	public void TestProcessUnderControlDeclarationMessage()
	{
		var responseMessage = new Data() { Stato = 5 };
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeaderOne);
		var subProcessor = new NewResponseMessageSubProcessor(Factory.New<EDIMessage>());

		subProcessor.ProcessMessage(adapter, responseMessage);
		AssertEquals(nameof(entryHeaderOne.CH_EntryStatus), "UCL", entryHeaderOne.CH_EntryStatus);
	}

	[ExpectNoExceptions]
	public void TestProcessGuaranteeTransactions()
	{
		var adapterMock = new Mock<IXmlCustomsLinkedObjectAdapter>();
		adapterMock.As<IGuaranteeTransactionSupporter>().Setup(x => x.ConfirmPendingTransactions(It.IsAny<ZString>()));

		var subProcessor = new NewResponseMessageSubProcessor(Factory.New<EDIMessage>());
		subProcessor.ProcessMessage(adapterMock.Object, new Data { });

		adapterMock.As<IGuaranteeTransactionSupporter>().Verify(x => x.ConfirmPendingTransactions(It.IsAny<ZString>()), Times.Once());
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		entryHeaderOne = jobDeclaration.CustomsEntryHeaders.AddNew();
		lineOneHeaderOne = entryHeaderOne.MergedLines.AddNew();
		lineOneHeaderOne.CL_LineNumber = 1;
		lineTwoHeaderOne = entryHeaderOne.MergedLines.AddNew();
		lineTwoHeaderOne.CL_LineNumber = 2;

		entryHeaderTwo = jobDeclaration.CustomsEntryHeaders.AddNew();
		lineOneHeaderTwo = entryHeaderTwo.MergedLines.AddNew();
		lineOneHeaderTwo.CL_LineNumber = 2;

		payInfo = entryHeaderOne.EntryPayInfos.AddNew();
		payInfo.C9_IncomingPayResponseNo = "1-1";
		payInfo.C9_PaymentAmount = 188.98m;
		payInfo.C9_PaymentDate = new ZDateTime(2020, 01, 29);
		payInfo.C9_PaymentStatus = "PEN";
		payInfo.C9_PaymentParty = "F";

		var feeOne = lineOneHeaderOne.Fees.AddNew();
		feeOne.CF_MethodOfPayment = "E";
		feeOne.CF_ChargeType = "DTY";
		feeOne.CF_ChargeAmount = 32.22m;

		var feeTwo = lineTwoHeaderOne.Fees.AddNew();
		feeTwo.CF_MethodOfPayment = "G";
		feeTwo.CF_ChargeType = "DTY";
		feeTwo.CF_ChargeAmount = 143.99m;

		var feeThree = lineTwoHeaderOne.Fees.AddNew();
		feeThree.CF_MethodOfPayment = "G";
		feeThree.CF_ChargeType = "407";
		feeThree.CF_ChargeAmount = 24.02m;
	}

	JobDeclaration jobDeclaration;
	CusEntryHeader entryHeaderOne, entryHeaderTwo;
	CusEntryLine lineOneHeaderOne, lineOneHeaderTwo, lineTwoHeaderOne;
	CusEntryPayInfo payInfo;
}
