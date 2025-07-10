using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IENSDeclarationMessageDataProvider : IENSCommonMessageDataProvider
	{
		IENSDeclarationHeader Header { get; }
		ZString LodgingCustomsOffice { get; }
	}

	public interface IENSDeclarationHeader : IENSCommonHeader
	{
		ZString ReferenceNumber { get; }
		ZString DeclarationPlace { get; }
		ZString DeclarationPlaceLanguage { get; }
		ZDateTime DeclarationDate { get; }
	}
}
