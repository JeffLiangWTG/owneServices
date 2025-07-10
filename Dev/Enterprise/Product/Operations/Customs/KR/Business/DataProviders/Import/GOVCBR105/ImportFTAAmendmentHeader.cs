using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportFTAAmendmentHeader : ImportFTAHeaderCore, IImportFTAAmendmentHeader
	{
		public DateTime EntryReleaseDate { get; set; }
		public Organisation Declarant { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public ImportFTAAmendmentItem[] Items { get; set; }

		ZDate IImportFTAAmendmentHeader.EntryReleaseDate => (ZDate)EntryReleaseDate;
		IOrganization IImportFTAAmendmentHeader.Declarant => Declarant;
		ZString IImportFTAAmendmentHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImportFTAAmendmentHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IEnumerable<IImportFTAAmendmentItem> IImportFTAAmendmentHeader.Items => Items;
	}
}
