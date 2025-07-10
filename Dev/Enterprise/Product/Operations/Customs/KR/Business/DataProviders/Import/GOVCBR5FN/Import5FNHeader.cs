using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5FNHeader : IImport5FNHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public string TypeOfBusiness { get; set; }
		public Organisation Payer { get; set; }
		public Organisation CustomsBroker { get; set; }

		ZString IImport5FNHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImport5FNHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport5FNHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		ZString IImport5FNHeader.TypeOfBusiness => TypeOfBusiness;
		IOrganization IImport5FNHeader.Payer => Payer;
		IOrganization IImport5FNHeader.CustomsBroker => CustomsBroker;
	}
}
