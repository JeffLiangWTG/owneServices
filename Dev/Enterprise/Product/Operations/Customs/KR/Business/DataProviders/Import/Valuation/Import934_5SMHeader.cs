using System;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import934_5SMHeader : IImport934_5SMHeader
	{
		public string ValuationMethod { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Organisation Payer { get; set; }
		public Organisation Supplier { get; set; }
		public Organisation Importer { get; set; }
		public string PurchaseOrderNo { get; set; }
		public DateTime PurchaseOrderDate { get; set; }
		public ValueDeclarationPerson Author { get; set; }
		public ValueDeclarationPerson ResponsiblePerson { get; set; }

		ZString IImport934_5SMHeader.ValuationMethod => ValuationMethod;
		ZString IImport934_5SMHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport934_5SMHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IOrganization IImport934_5SMHeader.Payer => Payer;
		IOrganization IImport934_5SMHeader.Supplier => Supplier;
		IOrganization IImport934_5SMHeader.Importer => Importer;
		ZString IImport934_5SMHeader.PurchaseOrderNo => PurchaseOrderNo;
		ZDate IImport934_5SMHeader.PurchaseOrderDate => (ZDate)PurchaseOrderDate;
		IValueDeclarationPerson IImport934_5SMHeader.Author => Author;
		IValueDeclarationPerson IImport934_5SMHeader.ResponsiblePerson => ResponsiblePerson;
	}
}
