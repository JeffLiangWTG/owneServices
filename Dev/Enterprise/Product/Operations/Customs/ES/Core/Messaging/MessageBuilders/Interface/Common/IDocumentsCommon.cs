using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IDocumentsCommon
{
	ZString Name { get; }
	ZString Number { get; }
}

public interface ICommonDocumentSequenceNumber : IDocumentsCommon
{
	ZString SequenceNumber { get; }
}

public interface ICommonDocumentGoodsItemId : IDocumentsCommon
{
	ZString GoodsItemId { get; }
}
