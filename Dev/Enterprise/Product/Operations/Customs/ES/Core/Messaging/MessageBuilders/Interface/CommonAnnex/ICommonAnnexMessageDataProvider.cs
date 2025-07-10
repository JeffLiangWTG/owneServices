using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICommonAnnexMessageDataProvider : IESEDIMessageCollectionProvider
{
	ZString Operation { get; }
	ZString Reference { get; }
	ZString RequestDispatchTagName { get; }
	ZString DispatchRequest { get; }
	IAnnexDocCommon Document { get; }
	ZString AdministrationCode { get; }
}
