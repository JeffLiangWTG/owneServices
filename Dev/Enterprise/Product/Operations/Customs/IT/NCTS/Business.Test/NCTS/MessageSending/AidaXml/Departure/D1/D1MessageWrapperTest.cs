using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class D1MessageWrapperTest : D1MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => new D1MessageWrapper(null, messageSendingWrapperFactory.Object));
		AssertExceptionThrown<ArgumentNullException>("When message sending factory is null", () => new D1MessageWrapper(header, null));
		AssertNoExceptionThrown(() => new D1MessageWrapper(header, messageSendingWrapperFactory.Object));
	}

	public override void TestConsignment()
	{
		var wrapper = CreateWrapper();
		var consignment = wrapper.Consignment;
		AssertNotNull(nameof(ID1Message.Consignment), consignment);
		AssertType<D1ConsignmentWrapper>(consignment);

		var amendmentMock = new Mock<INctsAmendment>();
		wrapper = new D1MessageWrapper(header, messageSendingWrapperFactory.Object, amendmentMock.Object);
		AssertSame(nameof(ID1Consignment.Amendment), amendmentMock.Object, wrapper.Consignment.Amendment);
	}

	public override void TestHouseConsignments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1Message.HouseConsignments), wrapper.HouseConsignments);
		AssertEquals($"{nameof(ID1Message.HouseConsignments)} Count", 0, wrapper.HouseConsignments.Count);

		var bill = header.Bills.AddNew();
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1Message.HouseConsignments), wrapper.HouseConsignments);
		AssertEquals($"{nameof(ID1Message.HouseConsignments)} Count", 1, wrapper.HouseConsignments.Count);
		AssertType<D1HouseConsignmentWrapper>($"{nameof(ID1Message.HouseConsignments)} Type", wrapper.HouseConsignments.Single());

		bill.B0_BillStatus = "DEL";
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1Message.HouseConsignments), wrapper.HouseConsignments);
		AssertEquals($"{nameof(ID1Message.HouseConsignments)} Count", 0, wrapper.HouseConsignments.Count);

		bill.B0_BillStatus = "DLR";
		wrapper = CreateWrapper();
		AssertNotNull(nameof(ID1Message.HouseConsignments), wrapper.HouseConsignments);
		AssertEquals($"{nameof(ID1Message.HouseConsignments)} Count", 0, wrapper.HouseConsignments.Count);
	}

	protected override ID1Message CreateWrapper()
	{
		return new D1MessageWrapper(header, messageSendingWrapperFactory.Object);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeaderWrapper = new Mock<INctsHeaderWrapper>();
		messageSendingWrapperFactory = new Mock<IMessageSendingWrapperFactory>();
		messageSendingWrapperFactory.Setup(x => x.GetNewNctsHeaderWrapper(It.IsAny<NctsHeader>())).Returns(nctsHeaderWrapper.Object);
	}

	Mock<IMessageSendingWrapperFactory> messageSendingWrapperFactory;
	Mock<INctsHeaderWrapper> nctsHeaderWrapper;
}
