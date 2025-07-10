using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ADD_DDT_V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ADD_DDT;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[CodeAlive("This will be used in the next WI")]
public class H1ImportAmendmentC40MessageBuilder : H1ImportCommonMessageBuilder<IH1ImportAmendmentC40MessageDataProvider, AddDdtV1Ent>
{
	public H1ImportAmendmentC40MessageBuilder(IH1ImportAmendmentC40MessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected override AddDdtV1Ent GenerateXMLMessage() =>
		new()
		{
			Message = GetPopulatedMessage(),
			AddDdt = GetPopulatedAddDdt()
		};

	AddDdtType GetPopulatedAddDdt() =>
		new()
		{
			ImportOperation = GetPopulatedImportOperation(),
			GoodsItem = provider.GoodsItems.ConvertToCollection(GetPopulatedGoodsItem)
		};

	MCciOperationType01D GetPopulatedImportOperation() =>
		new()
		{
			Lrn = provider.LRN,
			CustomsRegistrationNumber = provider.CustomsRegistrationNumber
		};

	GoodsItemTypeD05 GetPopulatedGoodsItem(IH1ImportAmendmentC40GoodsItem goodsItemProvider)
	{
		var goodsItem = GetPopulatedCommonGoodsShipmentItem<GoodsItemTypeD05>(goodsItemProvider);
		if (goodsItem is not null)
		{
			goodsItem.PreviousDocument = goodsItemProvider.PreviousDocuments.ConvertToCollection(GetPopulatedH1CommonPreviousDocument<MPreviousDocumentType03SinCc>);
		}

		return goodsItem;
	}
}
