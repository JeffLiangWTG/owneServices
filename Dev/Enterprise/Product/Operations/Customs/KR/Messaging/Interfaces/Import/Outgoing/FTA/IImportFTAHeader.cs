using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportFTAHeader : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		ZString LawCode { get; }
		ZString StatementNumber5WN { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._14)]
		ZDate DepartureDate { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._13A)]
		ZString DepartureCountryCode { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._13B)]
		ZString DeparturePort { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._27)]
		ZString TransshipmentYN { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._16)]
		ZDate TransshipmentDate { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._15A)]
		ZString TransshipmentCountryCode { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._15B)]
		ZString TransshipmentPort { get; }
		IOrganization Importer { get; }
		IOrganization Supplier { get; }
		IOrganization Manufacturer { get; }
		IEnumerable<IImportFTALine> EntryLines { get; }
	}
}
