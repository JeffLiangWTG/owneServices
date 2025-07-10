using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT007DataProvider))]
sealed class NT007DataProviderTest : BaseNctsArrivalMessageDataProviderTest<NT007DataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override NT007DataProvider CreateDataProvider() => new NT007DataProvider(MessageSendingObject);

	public void TestConstructorNullArgument() => AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new NT007DataProvider(null));

	public void TestArrivalOperationTraderAtDestinationReferenceNumber() => CombineAssertions(() =>
	{
		const string validTraderAtDestinationReferenceNumber = "12345";

		MessageSendingObject.NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = ZString.Empty;
		AssertEquals("Initial Arrival Operation Trader at Destination Reference Number", string.Empty, DataProvider.ArrivalOperationTraderAtDestinationReferenceNumber);

		ResetDataProvider();

		MessageSendingObject.NctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = validTraderAtDestinationReferenceNumber;
		AssertEquals("Arrival Operation Trader at Destination Reference Number", validTraderAtDestinationReferenceNumber, DataProvider.ArrivalOperationTraderAtDestinationReferenceNumber);
	});

	public void TestTraderAtDestination() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.TraderAtDestination);
		AssertType<TraderDataProvider>("type", DataProvider.TraderAtDestination);
		AssertSame("cached", DataProvider.TraderAtDestination, DataProvider.TraderAtDestination);
	});

	public void TestApprovedLocationOfGoodsIdentificationNumber() => CombineAssertions(() =>
	{
		const string id = "ZO1000000001";

		AssertEquals("Empty when not available", string.Empty, DataProvider.ApprovedLocationOfGoodsIdentificationNumber);

		var goodsLocation = MessageSendingObject.NctsHeader.ArrivalMovementHeader.GoodsLocation;
		goodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Arrival;
		goodsLocation.Address.E2_GovRegNum = id;

		AssertEquals("Available after setting value", id, DataProvider.ApprovedLocationOfGoodsIdentificationNumber);
	});

	public void TestTransportMeans() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.TransportMeans);
		AssertType<TransportMeansAtArrivalDataProvider>("type", DataProvider.TransportMeans);
		AssertSame("cached", DataProvider.TransportMeans, DataProvider.TransportMeans);
	});

	public void TestGoodsDeclarations() => CombineAssertions(() =>
	{
		NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();

		var goodsDeclarations = DataProvider.GoodsDeclarations.ToArray();
		AssertEquals("count", 2, goodsDeclarations.Length);
		AssertEquals("item 1", 1, goodsDeclarations[0].SequenceNumber);
		AssertEquals("item 2", 2, goodsDeclarations[1].SequenceNumber);

		AssertSame("cached", DataProvider.GoodsDeclarations, DataProvider.GoodsDeclarations);
	});

	public void TestSupernumeraryGoods() => CombineAssertions(() =>
	{
		NctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();
		NctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();

		var supernumeraryGoods = DataProvider.SupernumeraryGoods.ToArray();
		AssertEquals("count", 2, supernumeraryGoods.Length);
		AssertEquals("item 1", 1, supernumeraryGoods[0].SequenceNumber);
		AssertEquals("item 2", 2, supernumeraryGoods[1].SequenceNumber);

		AssertSame("cached", DataProvider.SupernumeraryGoods, DataProvider.SupernumeraryGoods);
	});

	public void TestAdditionalTransitOperations() => CombineAssertions(() =>
	{
		NctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
		NctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();

		var additionalTransitOperations = DataProvider.AdditionalTransitOperations.ToArray();
		AssertEquals("count", 2, additionalTransitOperations.Length);
		AssertEquals("item 1", 1, additionalTransitOperations[0].SequenceNumber);
		AssertEquals("item 2", 2, additionalTransitOperations[1].SequenceNumber);

		AssertSame("cached", DataProvider.AdditionalTransitOperations, DataProvider.AdditionalTransitOperations);
	});

	public void TestOppositeInformation_ReferenceNumber()
	{
		AssertEquals("Reference Number is Message Identification", MessageSendingObject.MessageIdentification, DataProvider.OppositeInformation.ReferenceNumber);
	}
}
