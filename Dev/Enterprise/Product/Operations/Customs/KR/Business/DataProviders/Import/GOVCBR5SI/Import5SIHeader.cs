using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SIHeader : IImport5SIHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Import5SILine[] MailItemIDs { get; set; }

		ZString IImport5SIHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImport5SIHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport5SIHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IEnumerable<IImport5SILine> IImport5SIHeader.MailItemIDs => MailItemIDs;
	}
}
