using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

sealed class Ucc6JobDeclarationMessageSendingObjectBaseOnlyTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestResetCustomsMessageTextOnMessageTypeChange()
	{
		var sendingObjectMock = new Mock<Ucc6MessageSendingObjectForTest>(entryHeader, sendingObjectParent) { CallBase = true };
		sendingObjectMock.Protected().Setup("ResetCustomsMessageText");

		sendingObjectMock.Object.MessageType = "CAN";
		sendingObjectMock.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestResetCustomsMessageTextOnVOCReasonChange()
	{
		var sendingObjectMock = new Mock<Ucc6JobDeclarationMessageSendingObject>(entryHeader, sendingObjectParent) { CallBase = true };
		sendingObjectMock.Protected().Setup("ResetCustomsMessageText");

		sendingObjectMock.Object.VOCReason = "A";
		sendingObjectMock.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestResetCustomsMessageTextOnCancellationAndAmendmentLegislativeReferenceChange()
	{
		var sendingObjectMock = new Mock<Ucc6JobDeclarationMessageSendingObject>(entryHeader, sendingObjectParent) { CallBase = true };
		sendingObjectMock.Protected().Setup("ResetCustomsMessageText");

		sendingObjectMock.Object.CancellationAndAmendmentLegislativeReference = "A";
		sendingObjectMock.VerifyAll();
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
	}

	CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObjectParent sendingObjectParent;
}
