using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5TMHeader : IImport5TMHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public Organisation Payer { get; set; }
		public Import5TMLine[] EntryLines { get; set; }

		ZString IImport5TMHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		IOrganization IImport5TMHeader.Payer => Payer;
		IEnumerable<IImport5TMLine> IImport5TMHeader.EntryLines => EntryLines;
	}
}
