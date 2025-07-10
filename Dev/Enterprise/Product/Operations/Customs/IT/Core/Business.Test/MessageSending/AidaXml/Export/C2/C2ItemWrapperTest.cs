using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class C2ItemWrapperTest : C2ItemWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryLine is null", () => new C2ItemWrapper(null, exportMessageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory argument is null", () => new C2ItemWrapper(EntryLine, null));
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Item.Authorizations), 0, wrapper.Authorizations.Count);

		var authorization = new Mock<IAuthorization>();
		entryLineWrapperMock.Setup(e => e.ExportAuthorizations).Returns(new[] { authorization.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Item.Authorizations), 1, wrapper.Authorizations.Count);
	}

	public override void TestItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Item.ItemNumber), 0, wrapper.ItemNumber);

		entryLineWrapperMock.Setup(e => e.ItemNumber).Returns(145);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Item.ItemNumber), 145, wrapper.ItemNumber);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Item.PreviousDocuments), 0, wrapper.PreviousDocuments.Count);

		var previousDocument = new Mock<IPreviousDocument>();
		entryLineWrapperMock.Setup(e => e.ExportPreviousDocuments).Returns(new[] { previousDocument.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Item.PreviousDocuments), 1, wrapper.PreviousDocuments.Count);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC2Item.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		entryLineWrapperMock.Setup(e => e.ExportTransportChargesMethodOfPayment).Returns("A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Item.TransportChargesMethodOfPayment), "A", wrapper.TransportChargesMethodOfPayment);
	}

	protected override IC2Item CreateWrapper() => new C2ItemWrapper(EntryLine, exportMessageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();
		entryLineWrapperMock = new Mock<ICusEntryLineCustomsMessageWrapper>();

		exportMessageSendingWrapperFactory = new ExportMessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewCusEntryLineCustomsMessageWrapper(entryLineWrapperMock.Object)
			.Build();
	}

	CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	Mock<ICusEntryLineCustomsMessageWrapper> entryLineWrapperMock;
}
