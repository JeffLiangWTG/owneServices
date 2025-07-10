using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SMHeader : Import934_5SMHeader, IImport5SMHeader
	{
		public ZString ValueDeclarationTemplateNumber { get; set; }

		public Import5SMFormC FormCData { get; set; }

		public Import934_5SMFormD FormDData { get; set; }

		public Import5SMLine[] EntryLines { get; set; }

		ZString IImport5SMHeader.ValueDeclarationTemplateNumber => ValueDeclarationTemplateNumber;

		IImport5SMFormC IImport5SMHeader.FormCData => FormCData;

		IImport934_5SMFormD IImport5SMHeader.FormDData => FormDData;

		IEnumerable<IImport5SMLine> IImport5SMHeader.EntryLines => EntryLines;
		ZString IImport934_5SMHeader.ValuationMethod => ValuationMethod;

		ZString IImport934_5SMHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;

		ZString IImport934_5SMHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;

		IOrganization IImport934_5SMHeader.Payer => Payer;

		IOrganization IImport934_5SMHeader.Supplier => Supplier;

		IOrganization IImport934_5SMHeader.Importer => Importer;

		ZString IImport934_5SMHeader.PurchaseOrderNo => PurchaseOrderNo;

		IValueDeclarationPerson IImport934_5SMHeader.Author => Author;

		IValueDeclarationPerson IImport934_5SMHeader.ResponsiblePerson => ResponsiblePerson;
	}
}
