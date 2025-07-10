using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public abstract class ImportFTAHeaderCore : IImportFTAHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public string LawCode { get; set; }
		public string StatementNumber5WN { get; set; }
		public DateTime DepartureDate { get; set; }
		public string DepartureCountryCode { get; set; }
		public string DeparturePort { get; set; }
		public string TransshipmentYN { get; set; }
		public DateTime TransshipmentDate { get; set; }
		public string TransshipmentCountryCode { get; set; }
		public string TransshipmentPort { get; set; }
		public Organisation Importer { get; set; }
		public Organisation Supplier { get; set; }
		public Organisation Manufacturer { get; set; }
		public ImportFTALine[] EntryLines { get; set; }

		ZString IImportFTAHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImportFTAHeader.LawCode => LawCode;
		ZString IImportFTAHeader.StatementNumber5WN => StatementNumber5WN;
		ZDate IImportFTAHeader.DepartureDate => (ZDate)DepartureDate;
		ZString IImportFTAHeader.DepartureCountryCode => DepartureCountryCode;
		ZString IImportFTAHeader.DeparturePort => DeparturePort;
		ZString IImportFTAHeader.TransshipmentYN => TransshipmentYN;
		ZDate IImportFTAHeader.TransshipmentDate => (ZDate)TransshipmentDate;
		ZString IImportFTAHeader.TransshipmentCountryCode => TransshipmentCountryCode;
		ZString IImportFTAHeader.TransshipmentPort => TransshipmentPort;
		IOrganization IImportFTAHeader.Importer => Importer;
		IOrganization IImportFTAHeader.Supplier => Supplier;
		IOrganization IImportFTAHeader.Manufacturer => Manufacturer;
		IEnumerable<IImportFTALine> IImportFTAHeader.EntryLines => EntryLines;
	}
}
