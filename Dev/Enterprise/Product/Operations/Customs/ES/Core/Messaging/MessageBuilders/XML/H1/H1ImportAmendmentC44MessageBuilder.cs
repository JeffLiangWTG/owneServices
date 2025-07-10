using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ctypes;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_SUP_DOC;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.SUP_DOC_V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[CodeAlive("Used in tests via [TestedType]; will be used in WI00773559")]
public class H1ImportAmendmentC44MessageBuilder : H1ImportCommonMessageBuilder<IH1ImportAmendmentC44MessageDataProvider, SupDocV1Ent>
{
	public H1ImportAmendmentC44MessageBuilder(IH1ImportAmendmentC44MessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected override SupDocV1Ent GenerateXMLMessage() =>
		new()
		{
			Message = GetPopulatedMessage(),
			SupDoc = GetPopulatedSupDoc()
		};

	SupDocType GetPopulatedSupDoc() =>
		new()
		{
			ImportOperation = GetPopulatedImportOperation(),
			GoodsItem = provider.GoodsItems.ConvertToCollection(GetPopulatedGoodsItem)
		};

	MCciOperationType01 GetPopulatedImportOperation() =>
		new()
		{
			Lrn = provider.LRN,
			Mrn = provider.MRN
		};

	GoodsItemTypeD04 GetPopulatedGoodsItem(IH1ImportAmendmentC44GoodsItem goodsItemProvider)
	{
		var goodsItem = GetPopulatedCommonGoodsShipmentItem<GoodsItemTypeD04>(goodsItemProvider);
		if (goodsItem is not null)
		{
			goodsItem.SupportingDocument = goodsItemProvider.SupportingDocuments.ConvertToCollection(GetPopulatedH1CommonLineSupportingDocument<MSupportingDocumentType01DSinCc>);
		}

		return goodsItem;
	}
}
