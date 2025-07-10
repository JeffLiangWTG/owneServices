using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IQueryH7MessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString DeclarationMRN { get; }
		ZString G3DeclarationMRN { get; }
		ZString NextH7DeclarationMRN { get; }
	}
}
