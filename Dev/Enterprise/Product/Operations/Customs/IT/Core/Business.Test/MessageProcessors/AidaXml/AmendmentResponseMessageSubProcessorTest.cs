using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AmendmentResponseMessageSubProcessorTest : TestCaseWithFactory
{
	public void TestProcessMessageWithPositiveButNotConfirmedResponse()
	{
		var (entryHeader, _, _) = dataGenerator.GetEntryInfoWithSentMessage();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var responseMessage = new Data { Stato = 5 };
		var subProcessor = new AmendmentResponseMessageSubProcessor();
		subProcessor.ProcessMessage(adapter, responseMessage);

		AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.AcceptedBySystem, entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "", entryHeader.CH_EntryStatus);
	}

	public void TestProcessMessageWithConfirmedPositiveResponse_VerifyStatus()
	{
		var (entryHeader, _, _) = dataGenerator.GetEntryInfoWithSentMessage();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var responseMessage = new Data { Stato = 4 };
		var subProcessor = new AmendmentResponseMessageSubProcessor();
		subProcessor.ProcessMessage(adapter, responseMessage);

		AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.AcceptedBySystem, entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), ITEntryStatusList.Codes.Amended, entryHeader.CH_EntryStatus);
	}

	public void TestProcessMessageWithPositiveResponse_InvalidateNumbers()
	{
		var (entryHeader, _, _) = dataGenerator.GetEntryInfoWithSentMessage();
		var (payInfoOne, payInfoTwo, _) = dataGenerator.GetCusEntryPayInfos();

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var responseMessage = new Data { Stato = 4 };
		var subProcessor = new AmendmentResponseMessageSubProcessor();
		subProcessor.ProcessMessage(adapter, responseMessage);
		Factory.Save();

		entryHeader.EntryPayInfos.Load();
		var allPaymentInfo = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();

		AssertEquals("CusEntryPayInfo Count", 4, allPaymentInfo.Length);
		AssertCollectionContains(allPaymentInfo, i => i.A93Number == payInfoTwo.A93Number && i.C9_PaymentAmount < 0);
		AssertEquals("PayInfoOne Invalidated Record Count", 1, allPaymentInfo.Count(i => i.A93Number == payInfoOne.A93Number && i.C9_PaymentAmount < 0));
	}

	public void TestProcessMessageWithPositiveResponseAndClearance_HeaderLevel()
	{
		var (a93Info, clearanceInfo) = GetA93AndReleaseInfo();
		clearanceInfo.Livello = "S";

		var (entryHeader, _, entryLineOne) = dataGenerator.GetEntryInfoWithSentMessage();
		var entryLineFee = entryLineOne.Fees.AddNew();
		entryLineFee.CF_ChargeAmount = 200.00m;
		entryLineFee.CF_MethodOfPayment = "E";

		var entryLineTwo = entryHeader.MergedLines.AddNew();
		var entryLineTwoFee = entryLineTwo.Fees.AddNew();
		entryLineTwoFee.CF_ChargeAmount = 300.00m;
		entryLineTwoFee.CF_MethodOfPayment = "E";

		var (payInfoOne, payInfoTwo, _) = dataGenerator.GetCusEntryPayInfos();

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var responseMessage = GetresponseMessage(a93Info, clearanceInfo);
		var subProcessor = new AmendmentResponseMessageSubProcessor();
		subProcessor.ProcessMessage(adapter, responseMessage);
		Factory.Save();

		AssertA93AndClearance(entryHeader: entryHeader, parentObject: entryHeader, payInfoOne: payInfoOne, payInfoTwo: payInfoTwo, expectedA93Amount: 500.00m);
	}

	public void TestProcessMessageWithPositiveResponseAndClearance_ItemLevel()
	{
		var (a93Info, clearanceInfo) = GetA93AndReleaseInfo();
		a93Info.NumeroArticolo = 1;
		clearanceInfo.NumeroArticolo = 1;

		var (entryHeader, _, entryLine) = dataGenerator.GetEntryInfoWithSentMessage();
		var entryLineOne = entryHeader.MergedLines.FirstOrDefault();
		var entryLineFee = entryLineOne.Fees.AddNew();
		entryLineFee.CF_ChargeAmount = 200.00m;
		entryLineFee.CF_MethodOfPayment = "E";

		var (payInfoOne, payInfoTwo, _) = dataGenerator.GetCusEntryPayInfos();
		payInfoTwo.C9_IncomingPayResponseNo = "XYZ-1";

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var responseMessage = GetresponseMessage(a93Info, clearanceInfo);
		var subProcessor = new AmendmentResponseMessageSubProcessor();
		subProcessor.ProcessMessage(adapter, responseMessage);
		Factory.Save();

		AssertA93AndClearance(entryHeader: entryHeader, parentObject: entryLine, payInfoOne: payInfoOne, payInfoTwo: payInfoTwo, expectedA93Amount: 200.00m);
	}

	[ExpectNoExceptions]
	public void TestProcessMessage_SetSentEntryLinesCountCall()
	{
		var responseMessage = new Data();
		var mockAdapter = new Mock<IXmlCustomsLinkedObjectAdapter>();

		mockAdapter.Setup(x => x.GetAllPaymentInfo()).Returns(new List<CusEntryPayInfo>());
		mockAdapter.Setup(x => x.SetSentEntryLinesCount());
		var subProcessor = new AmendmentResponseMessageSubProcessor();
		subProcessor.ProcessMessage(mockAdapter.Object, responseMessage);
		mockAdapter.Verify(x => x.SetSentEntryLinesCount(), Times.Once());
	}

	void AssertA93AndClearance(CusEntryHeader entryHeader, CargoWise.EntityFramework.BusinessObject parentObject, CusEntryPayInfo payInfoOne, CusEntryPayInfo payInfoTwo, ZDecimal expectedA93Amount)
	{
		entryHeader.EntryPayInfos.Load();
		var allPaymentInfo = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();

		AssertEquals("CusEntryPayInfo Count", 5, allPaymentInfo.Length);
		AssertCollectionContains(allPaymentInfo, i => i.A93Number == payInfoTwo.A93Number && i.C9_PaymentAmount < 0);
		AssertEquals("PayInfoOne Invalidated Record Count", 1, allPaymentInfo.Count(i => i.A93Number == payInfoOne.A93Number && i.C9_PaymentAmount < 0));

		AssertEquals("PayInfoTwo must have 3 record counts", 3, allPaymentInfo.Count(i => i.A93Number == payInfoTwo.A93Number));
		AssertEquals("PayInfoTwo must have one negative record count", 1, allPaymentInfo.Count(i => i.A93Number == payInfoTwo.A93Number && i.C9_PaymentAmount < 0));
		var a93sWithPositiveAmount = allPaymentInfo.Where(i => i.A93Number == payInfoTwo.A93Number && i.C9_PaymentAmount > 0);
		AssertEquals("PayInfoTwo must have two positive record count", 2, a93sWithPositiveAmount.Count());
		AssertEquals("Expected A93 amount", expectedA93Amount, a93sWithPositiveAmount.LastOrDefault().C9_PaymentAmount);

		var clearanceCode = CusEntryNumber.Load(parentObject, "CLR", "IT", true);
		AssertNotNull("Clearance Code Entry", clearanceCode);
		AssertEquals("CLR Value", "7JTCQR", clearanceCode.CE_EntryNum);

		AssertEquals("CH_Status", "ACS", entryHeader.CH_Status);
		AssertEquals("CH_EntryStatus", "AMD", entryHeader.CH_EntryStatus);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataGenerator = new AmendmentMessageProcessorTestDataGenerator(Factory);
		dataGenerator.Generate();
	}

	(DataInformazione, DataInformazione) GetA93AndReleaseInfo()
	{
		var a93Info = new DataInformazione
		{
			Codice = 1,
			Numero = "XYZ",
			Data = "2022-23/03/2022E",
			Livello = "P"
		};

		var clearanceInfo = new DataInformazione
		{
			Codice = 2,
			Data = "08/09/2022",
			Numero = "7JTCQR",
			Livello = "S"
		};

		return (a93Info, clearanceInfo);
	}

	Data GetresponseMessage(DataInformazione a93Info, DataInformazione clearanceInfo)
	{
		var responseMessage = new Data
		{
			Informazione = new[] { a93Info, clearanceInfo }.ToCollection(),
			Stato = 4
		};

		return responseMessage;
	}

	AmendmentMessageProcessorTestDataGenerator dataGenerator;
}
