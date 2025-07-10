using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Moq;
using NUnit.Framework;
using CancellationStatus = CargoWise.Customs.IT.MessageDefinitions.CancellationStatus;
using ICustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CancellationResponseMessageSubProcessorTest : TestCaseWithFactory
{
	public void TestProcessWithConfirmedResponse()
	{
		var (_, entryHeader, _) = dataGenerator.GetEntryInfoWithSentMessage();
		var (payInfoOne, _) = dataGenerator.GetEntryPayInfos();

		CombineAssertions("PRE-CONDITIONS", () => AssertEntryStatusesAndNoInvalidatedEntryPayInfo(entryHeader, payInfoOne, ""));

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);

		var responseMessageMock = new Mock<ICustomsResponseMessage>();
		responseMessageMock.Setup(x => x.CancellationStatus).Returns(CancellationStatus.Confirmed);

		var messageSubProcessor = new CancellationResponseMessageSubProcessor();
		messageSubProcessor.ProcessMessage(adapter, responseMessageMock.Object);

		CombineAssertions("POST-CONDITIONS", () => AssertCancellationConfirmed(entryHeader, payInfoOne));
	}

	public void TestProcessWithNotConfirmedResponse()
	{
		var (_, entryHeader, _) = dataGenerator.GetEntryInfoWithSentMessage();
		var (payInfoOne, _) = dataGenerator.GetEntryPayInfos();

		CombineAssertions("PRE-CONDITIONS", () => AssertEntryStatusesAndNoInvalidatedEntryPayInfo(entryHeader, payInfoOne, ""));

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);

		var responseMessageMock = new Mock<ICustomsResponseMessage>();
		responseMessageMock.Setup(x => x.CancellationStatus).Returns(CancellationStatus.None);

		var messageSubProcessor = new CancellationResponseMessageSubProcessor();
		messageSubProcessor.ProcessMessage(adapter, responseMessageMock.Object);

		CombineAssertions("POST-CONDITIONS", () => AssertEntryStatusesAndNoInvalidatedEntryPayInfo(entryHeader, payInfoOne, "ACS"));
	}

	[ExpectNoExceptions]
	public void TestProcessWithRejectedCancellation()
	{
		var responseMessageMock = new Mock<ICustomsResponseMessage>();
		responseMessageMock.Setup(x => x.CancellationStatus).Returns(CancellationStatus.Rejected);

		var entryAdapterMock = new Mock<IXmlCustomsLinkedObjectAdapter>();
		entryAdapterMock.Setup(x => x.SetStatusAsAcceptedBySystem());
		entryAdapterMock.Setup(x => x.SetStatusAsError());

		var messageSubProcessor = new CancellationResponseMessageSubProcessor();
		messageSubProcessor.ProcessMessage(entryAdapterMock.Object, responseMessageMock.Object);

		entryAdapterMock.Verify(x => x.SetStatusAsError(), Times.Once());
		entryAdapterMock.Verify(x => x.SetStatusAsAcceptedBySystem(), Times.Never());
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataGenerator = new CancellationMessageProcessorTestDataGenerator(Factory);
		dataGenerator.Generate();
	}

	CancellationMessageProcessorTestDataGenerator dataGenerator;

	void AssertEntryStatusesAndNoInvalidatedEntryPayInfo(CusEntryHeader entryHeader, CusEntryPayInfo payInfoOne, string expectedMessageStatus)
	{
		AssertEquals(nameof(entryHeader.CH_Status), expectedMessageStatus, entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "", entryHeader.CH_EntryStatus);

		var entryPayInfo = GetInvalidatedEntryPayInfo(entryHeader, payInfoOne);
		AssertNull("Invalidated Entry Pay Info", entryPayInfo);
	}

	void AssertCancellationConfirmed(CusEntryHeader entryHeader, CusEntryPayInfo payInfoOne)
	{
		AssertEquals(nameof(entryHeader.CH_Status), "ACS", entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "CNC", entryHeader.CH_EntryStatus);

		var entryPayInfo = GetInvalidatedEntryPayInfo(entryHeader, payInfoOne);
		AssertNotNull("Invalidated Entry Pay Info", entryPayInfo);
		AssertEquals(nameof(entryPayInfo.C9_PaymentAmount), -1234.22m, entryPayInfo.C9_PaymentAmount);
	}

	CusEntryPayInfo GetInvalidatedEntryPayInfo(CusEntryHeader entryHeader, CusEntryPayInfo payInfoOne)
	{
		entryHeader.EntryPayInfos.Load();
		return entryHeader
			.EntryPayInfos
			.ElementsAsEnumerable
			.SingleOrDefault(i => i.C9_IncomingPayResponseNo == payInfoOne.C9_IncomingPayResponseNo && i.C9_PaymentAmount < 0);
	}
}
