using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IG3CommonMessageDataProvider : IESEDIMessageCollectionProvider
	{
		IG3Message Message { get; }
	}

	public interface IG3Message
	{
		ZString Sender { get; }
	}

	public interface IG3CommonHeader
	{
		ZString LRN { get; }
		ZString CustomsOffice { get; }
		ZString PersonPresentingGoods { get; }
		IG3Declarant Declarant { get; }
		IG3Representative Representative { get; }
		ZDateTime DeclarationDate { get; }
		ZDateTime PresentationDate { get; }
	}

	public interface IG3Declarant
	{
		ZString IdNumber { get; }
		ZString Name { get; }
		IG3FullAddress FullAddress { get; }
		IG3Communication Communication { get; }
	}

	public interface IG3FullAddress
	{
		ZString Street { get; }
		ZString StreetAddLine { get; }
		ZString Number { get; }
		ZString POBox { get; }
		ZString SubDivision { get; }
		ZString Country { get; }
		ZString PostCode { get; }
		ZString City { get; }
	}

	public interface IG3Communication
	{
		ZString CommunicationType { get; }
		ZString CommunicationId { get; }
	}

	public interface IG3Representative
	{
		ZString IdNumber { get; }
		ZString Status { get; }
		ZString Name { get; }
		IG3Communication Communication { get; }
	}

	public interface IG3MasterConsignment
	{
		IReadOnlyCollection<ICommonDocumentGoodsItemId> PreviousDocument { get; }
		IDocumentsCommon TransportDocument { get; }
		ZString Receptacle { get; }
		IGenericLocation LocationOfGoods { get; }
		IReadOnlyCollection<ZString> TransportEquipmentContainers { get; }
	}

	public interface IG3HouseConsignment
	{
		IReadOnlyCollection<ICommonDocumentGoodsItemId> PreviousDocument { get; }
		IDocumentsCommon TransportDocument { get; }
	}
}
