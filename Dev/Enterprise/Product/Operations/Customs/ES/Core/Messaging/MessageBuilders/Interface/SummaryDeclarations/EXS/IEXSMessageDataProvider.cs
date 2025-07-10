using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IEXSMessageDataProvider : ISummaryDeclarationsCommonMessageDataProvider
	{
		IEXSHeader Header { get; }
		IDocumentsCommon TransportDocument { get; }
		IPartyProvider Consignor { get; }
		IPartyProvider Consignee { get; }
		IReadOnlyCollection<IEXSAdditionalActor> AdditionalActors { get; }
		IReadOnlyCollection<IDocumentsCommon> AdditionalInfo { get; }
		IReadOnlyCollection<IEXSLine> Lines { get; }
		IReadOnlyCollection<ZString> ItineraryCountries { get; }
		ZString CustomsOffice { get; }
		IEXSPartyProvider LodgingPerson { get; }
		IEXSRepresentative Representative { get; }
		IEXSContactPersonWithId Carrier { get; }
		IReadOnlyCollection<ZString> Seals { get; }
	}

	public interface IEXSHeader
	{
		ZString ReferenceNumber { get; }
		ZString GoodsLocation { get; }
		ZInt TotalLinesNum { get; }
		ZInt TotalPackagesQty { get; }
		ZDecimal TotalGrossWeight { get; }
		ZDateTime DeclarationDate { get; }
		ZString DeclarationPlace { get; }
		ZString SpecificCircumstanceInd { get; }
		ZString NatSpecificCircumstanceInd { get; }
		ZString DocumentOperationIndicator { get; }
		ZString DocumentReferenceNumber { get; }
		ZString MethodOfPayment { get; }
	}

	public interface IEXSLine
	{
		ZInt LineNumber { get; }
		ZString GoodsDescription { get; }
		ZDecimal GrossWeight { get; }
		ZString MethodOfPayment { get; }
		ZString UNDangerousCode { get; }
		ZString ReferenceNumber { get; }
		IReadOnlyCollection<IDocumentsCommon> Certificates { get; }
		IEXSDocument PreviousDocument { get; }
		IPartyProvider Consignor { get; }
		ZString CommodityCode { get; }
		IPartyProvider Consignee { get; }
		IReadOnlyCollection<ZString> Containers { get; }
		IReadOnlyCollection<IEXSPackage> Packages { get; }
		ZString CusCode { get; }
		IReadOnlyCollection<IEXSAdditionalActor> AdditionalActors { get; }
		IReadOnlyCollection<IDocumentsCommon> AdditionalInfo { get; }
	}

	public interface IEXSDocument : IDocumentsCommon
	{
		ZString LineNumber { get; }
	}

	public interface IEXSPackage : IPackageCommon
	{
		ZInt PackagesQty { get; }
		ZBool IsPackTypeBulk { get; }
	}

	public interface IEXSAdditionalActor
	{
		ZString Role { get; }
		ZString Id { get; }
	}

	public interface IEXSPartyProvider : IPartyProvider
	{
		ZString EmailAddress { get; }
	}

	public interface IEXSRepresentative : IEXSContactPersonWithId
	{
		ZString DirectRepresentation { get; }
	}

	public interface IEXSContactPersonWithId : IPartyNameProvider
	{
		ZString Phone { get; }
		ZString EmailAddress { get; }
	}
}
