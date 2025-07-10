using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class MessageSendingWrapperFactoryTest : TestCaseWithFactory
{
	public void TestGetNewHouseConsignmentCustomsMessageWrapper()
	{
		AssertType<HouseConsignmentCustomsMessageWrapper>(
			"HouseConsignmentCustomsMessageWrapper type",
			messageSendingWrapperFactory.GetNewHouseConsignmentCustomsMessageWrapper(bill));
	}

	public void TestGetNewConsignmentItemCustomsMessageWrapper()
	{
		AssertType<ConsignmentItemCustomsMessageWrapper>(
			"ConsignmentItemCustomsMessageWrapper type",
			messageSendingWrapperFactory.GetNewConsignmentItemCustomsMessageWrapper(goodsItem));
	}

	public void TestGetNewNctsHeaderWrapper()
	{
		AssertType<NctsHeaderWrapper>(
			"NctsHeaderWrapper type",
			messageSendingWrapperFactory.GetNewNctsHeaderWrapper(header));
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		bill = header.Bills.AddNew();
		goodsItem = bill.GoodsItems.AddNew();
		messageSendingWrapperFactory = new MessageSendingWrapperFactory();
	}

	NctsHeader header;
	NctsBill bill;
	NctsDepartureCargoDesc goodsItem;

	IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
