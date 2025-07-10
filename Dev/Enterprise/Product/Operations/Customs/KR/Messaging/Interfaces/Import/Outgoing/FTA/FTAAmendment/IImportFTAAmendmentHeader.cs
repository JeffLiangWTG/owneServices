using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportFTAAmendmentHeader : IImportFTAHeader
	{
		ZDate EntryReleaseDate { get; }
		IOrganization Declarant { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IEnumerable<IImportFTAAmendmentItem> Items { get; }
	}
}
