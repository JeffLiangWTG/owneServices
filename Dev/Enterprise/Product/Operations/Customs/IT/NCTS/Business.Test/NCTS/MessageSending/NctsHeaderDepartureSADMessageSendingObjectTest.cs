using System;
using System.Linq;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsHeaderDepartureSADMessageSendingObjectTest<TMessageSendingObject> : NctsHeaderDepartureMessageSendingObjectTest<TMessageSendingObject>
	where TMessageSendingObject : NctsHeaderDepartureSADMessageSendingObject
{
	public void TestCombinedCustomsMessageSubTypeIncludingNBLines()
	{
		var goodsItems = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItems.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		goodsItems.PreviousDocuments.AddNew().CSI_Procedure = "A3";

		messageSendingObject = GetMessageSendingObject(nctsHeader);
		AssertEquals("When NctsHeader has GroupedPreviousDocuments, CombinedCustomsMessageSubType", "ET + NB", messageSendingObject.CombinedCustomsMessageSubType);
	}

	public void TestMessageHeader()
	{
		AssertNotNull(nameof(messageSendingObject.MessageHeader), messageSendingObject.MessageHeader);
		AssertEquals($"{nameof(messageSendingObject.MessageHeader)} type", ExpectedMessageHeaderType, messageSendingObject.MessageHeader.GetType());
	}

	protected abstract Type ExpectedMessageHeaderType { get; }

	public void TestMessageLines()
	{
		AssertNotNull(nameof(messageSendingObject.MessageLines), messageSendingObject.MessageLines);

		nctsMovementHeader.GoodsItems.AddNew();
		AssertEquals($"{nameof(messageSendingObject.MessageLines)} item type", ExpectedMessageLineType, messageSendingObject.MessageLines.Single().GetType());
	}

	protected abstract Type ExpectedMessageLineType { get; }

	public void TestNBMessages()
	{
		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();

		AssertEquals("[PRE-CONDITION] NBGroupedPreviousDocuments", 0, goodsItem1.NBGroupedPreviousDocuments.Count());
		var messageSendingObject = GetMessageSendingObject(nctsHeader);
		AssertEquals("When NctsHeader does not have NBGroupedPreviousDocuments, NBMessages count", 0, messageSendingObject.NBMessages.Count());

		goodsItem1.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		goodsItem1.PreviousDocuments.AddNew().CSI_Procedure = "A3";

		CombineAssertions("NBMessages", () =>
		{
			AssertEquals("[PRE-CONDITION] GoodsItem1-> NBGroupedPreviousDocuments", 2, goodsItem1.NBGroupedPreviousDocuments.Count());
			AssertEquals("[PRE-CONDITION] GoodsItem2-> NBGroupedPreviousDocuments", 0, goodsItem2.NBGroupedPreviousDocuments.Count());
		});
		messageSendingObject = GetMessageSendingObject(nctsHeader);
		AssertEquals("NBMessages count", 1, messageSendingObject.NBMessages.Count());

		goodsItem2.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		goodsItem2.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		goodsItem2.PreviousDocuments.AddNew().CSI_Procedure = "A3";

		CombineAssertions("NBMessages", () =>
		{
			AssertEquals("[PRE-CONDITION] GoodsItem1-> NBGroupedPreviousDocuments", 2, goodsItem1.NBGroupedPreviousDocuments.Count());
			AssertEquals("[PRE-CONDITION] GoodsItem2-> NBGroupedPreviousDocuments", 3, goodsItem2.NBGroupedPreviousDocuments.Count());
		});
		messageSendingObject = GetMessageSendingObject(nctsHeader);
		AssertEquals("NBMessages count", 2, messageSendingObject.NBMessages.Count());

		goodsItem1.BY_Status = "NBA";
		messageSendingObject = GetMessageSendingObject(nctsHeader);
		AssertEquals("NBMessages count", 1, messageSendingObject.NBMessages.Count());
	}

	protected override string ExpectedSubType => SADConstants.MessageSubTypes.ET;
}
