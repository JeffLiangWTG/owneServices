using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageSendableCustomsEntry))]
sealed class TemporaryStorageSendableCustomsEntryTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStorageSendableCustomsEntry(header: null));
	}

	public void TestCustomsProfile()
	{
		header.AMA_CustomsProfile = "CUSPROF";
		ISendableCustomsEntry sendableCustomsEntry = new TemporaryStorageSendableCustomsEntry(header);
		AssertEquals("CustomsProfile", "CUSPROF", sendableCustomsEntry.CustomsProfile);
	}

	public void TestConsumeGuarantee()
	{
		ISendableCustomsEntry sendableCustomsEntry = new TemporaryStorageSendableCustomsEntry(header);
		AssertNoExceptionThrown(() => sendableCustomsEntry.ConsumeGuarantee(factory: null, message: null));
	}

	public void TestMarksAsSent()
	{
		var sentMessage = new Mock<IMessageType>().Object;

		ISendableCustomsEntry sendableCustomsEntry = new TemporaryStorageSendableCustomsEntry(header);
		sendableCustomsEntry.MarkAsSent(sentMessage);
		AssertEquals("AMA_MessageStatus", "SNT", header.AMA_MessageStatus);
	}

	public void TestPreProcessBeforeSending()
	{
		ISendableCustomsEntry sendableCustomsEntry = new TemporaryStorageSendableCustomsEntry(header);
		AssertNoExceptionThrown(() => sendableCustomsEntry.PreProcessBeforeSending());
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;
}
