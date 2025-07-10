using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ForwardingConsolIntegrationHelper))]
sealed class ForwardingConsolIntegrationHelperTest : TestCaseWithFactory
{
	public void TestCheckAllDeclarationsHaveSameCustomsOffice() => CombineAssertions(() =>
	{
		const string Message = "Not all declarations have the same customs office.";

		var consol = Factory.NewWithValidTestData<ForwardingConsol>();
		var shipment1 = consol.Shipments.AddNew();
		var shipment2 = consol.Shipments.AddNew();
		var declaration1 = Factory.New<JobDeclaration>();
		var declaration2 = Factory.New<JobDeclaration>();
		declaration1.JE_JS = shipment1.PK;
		declaration2.JE_JS = shipment2.PK;
		var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
		var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = CHLogicalStatusList.Codes.Accepted;
		entryHeader2.CH_Status = CHLogicalStatusList.Codes.Accepted;

		var helper = new ForwardingConsolIntegrationHelper();
		var queryProviderMock = new Mock<IDocumentSupporterQueryProvider>();

		declaration1.JE_CustomsOffice = "CH0001";
		declaration2.JE_CustomsOffice = "CH0002";
		AssertEquals("Not same office", false, helper.CanPrintGroupDeliveryNoteForConsol(consol, queryProviderMock.Object));
		queryProviderMock.Verify(q => q.ShowMessage(Message, MessageCaption), Times.Once);
		queryProviderMock.Verify(q => q.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

		declaration2.JE_CustomsOffice = "CH0001";
		queryProviderMock.Invocations.Clear();
		AssertEquals("Same office", true, helper.CanPrintGroupDeliveryNoteForConsol(consol, queryProviderMock.Object));
		queryProviderMock.Verify(q => q.ShowMessage(Message, MessageCaption), Times.Never);
		queryProviderMock.Verify(q => q.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	});

	public void TestCheckAllDeclarationCleared() => CombineAssertions(() =>
	{
		const string Message = "Some declarations have not been cleared yet. Continue?";

		var consol = Factory.NewWithValidTestData<ForwardingConsol>();
		var shipment1 = consol.Shipments.AddNew();
		var shipment2 = consol.Shipments.AddNew();
		var declaration1 = Factory.New<JobDeclaration>();
		var declaration2 = Factory.New<JobDeclaration>();
		declaration1.JE_JS = shipment1.PK;
		declaration2.JE_JS = shipment2.PK;
		var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
		var entryHeader2a = declaration2.ActiveEntryHeaders.AddNew();
		var entryHeader2b = declaration2.ActiveEntryHeaders.AddNew();
		var entryHeader2c = declaration2.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = CHLogicalStatusList.Codes.Accepted;
		entryHeader2a.CH_Status = CHLogicalStatusList.Codes.Accepted;
		entryHeader2c.CH_Status = CHLogicalStatusList.Codes.Accepted;

		var helper = new ForwardingConsolIntegrationHelper();

		AssertResult(false, true, false, CHLogicalStatusList.Codes.Invalid);
		AssertResult(true, true, true, CHLogicalStatusList.Codes.Invalid);
		AssertResult(true, false, false, CHLogicalStatusList.Codes.Accepted);

		void AssertResult(bool expectedResult, bool expectConfirmation, bool confirmationAnswer, string status)
		{
			entryHeader2b.CH_Status = status;
			var queryProviderMock = new Mock<IDocumentSupporterQueryProvider>();
			queryProviderMock.Setup(q => q.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(confirmationAnswer);
			AssertEquals($"Result: status={status} answer={confirmationAnswer}", expectedResult, helper.CanPrintGroupDeliveryNoteForConsol(consol, queryProviderMock.Object));
			queryProviderMock.Verify(q => q.ShowConfirmation(Message, MessageCaption), expectConfirmation ? Times.Once() : Times.Never(), $"Confirmation show: status={status} answer={confirmationAnswer}");
			queryProviderMock.Verify(q => q.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}
	});

	public void TestCheckNoDeclarations() => CombineAssertions(() =>
	{
		const string Message = "There are no Swiss declarations.";

		var consol = Factory.NewWithValidTestData<ForwardingConsol>();
		var shipment = consol.Shipments.AddNew();
		var declaration = Factory.New<Integration.Customs.US.IJobDeclaration>();
		declaration.JE_JS = shipment.PK;

		var helper = new ForwardingConsolIntegrationHelper();

		var queryProviderMock = new Mock<IDocumentSupporterQueryProvider>();

		AssertEquals("No Swiss declarations", false, helper.CanPrintGroupDeliveryNoteForConsol(consol, queryProviderMock.Object));
		queryProviderMock.Verify(q => q.ShowMessage(Message, MessageCaption), Times.Once);
		queryProviderMock.Verify(q => q.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	});

	public void TestCheckAtLeastOneDeclarationHaveEntries() => CombineAssertions(() =>
	{
		const string Message = "No declaration has entries.";

		var consol = Factory.NewWithValidTestData<ForwardingConsol>();
		var shipment1 = consol.Shipments.AddNew();
		var shipment2 = consol.Shipments.AddNew();
		var declaration1 = Factory.New<JobDeclaration>();
		var declaration2 = Factory.New<JobDeclaration>();
		declaration1.JE_JS = shipment1.PK;
		declaration2.JE_JS = shipment2.PK;

		var helper = new ForwardingConsolIntegrationHelper();

		var queryProviderMock = new Mock<IDocumentSupporterQueryProvider>();
		AssertEquals("No entries", false, helper.CanPrintGroupDeliveryNoteForConsol(consol, queryProviderMock.Object));
		queryProviderMock.Verify(q => q.ShowMessage(Message, MessageCaption), Times.Once);
		queryProviderMock.Verify(q => q.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

		var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = CHLogicalStatusList.Codes.Accepted;

		queryProviderMock = new Mock<IDocumentSupporterQueryProvider>();
		AssertEquals("One entry", true, helper.CanPrintGroupDeliveryNoteForConsol(consol, queryProviderMock.Object));
		queryProviderMock.Verify(q => q.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		queryProviderMock.Verify(q => q.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	});

	const string MessageCaption = "Import Group Delivery Note";
}
