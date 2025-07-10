using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDeclarationH7MessageDataProvider : IESEDIMessageCollectionProvider
	{
		IDeclarationH7Header Header { get; }
		IReadOnlyCollection<IDeclarationH7Line> Lines { get; }
	}

	public interface IDeclarationH7Header
	{
		ZString SupervisingCustomsOffice { get; }
		ZDecimal GrossWeight { get; }
		IH7Representative Representative { get; }
		IH7Declarant Declarant { get; }
		IPartyProvider Exporter { get; }
		IH7Importer Importer { get; }
		IReadOnlyCollection<IDocumentsCommon> SupportingDocuments { get; }
		IReadOnlyCollection<IDocumentsCommon> AdditionalReferences { get; }
		IReadOnlyCollection<IDocumentsCommon> TransportDocuments { get; }
		IReadOnlyCollection<IH7AdditionalInfo> AdditionalInformation { get; }
		IH7Value TransportCostToDestination { get; }
		IReadOnlyCollection<IDocumentsCommon> PreviousDocuments { get; }
		ZString ReferenceNumberUCR { get; }
		IReadOnlyCollection<IH7AdditionalFiscalRef> AdditionalFiscalRef { get; }
		IReadOnlyCollection<ZString> AdditionalProcedures { get; }
		ZString GoodsLocation { get; }
	}

	public interface IH7Representative
	{
		IPartyContactProvider ContactInfo { get; }
		ZString Id { get; }
		ZInt Status { get; }
	}

	public interface IH7Declarant : IPartyProvider
	{
		IPartyContactProvider ContactInfo { get; }
		ZBool IsImporter { get; }
	}

	public interface IH7Importer : IPartyProvider
	{
		IPartyContactProvider ContactInfo { get; }
		ZBool IsParticular { get; }
	}

	public interface IH7AdditionalInfo
	{
		ZString Code { get; }
		ZString Description { get; }
	}

	public interface IH7Value
	{
		ZDecimal Amount { get; }
		ZString CurrencyCode { get; }
	}

	public interface IH7AdditionalFiscalRef
	{
		ZString Id { get; }
		ZString Role { get; }
	}

	public interface IDeclarationH7Line
	{
		ZInt LineNumber { get; }
		IH7Value Value { get; }
		ZString GoodsDescription { get; }
		ZString CommodityCode { get; }
		ZDecimal GrossWeight { get; }
		ZDecimal ComplementaryUnitsQty { get; }
		ZInt NumberOfPackages { get; }
	}
}
