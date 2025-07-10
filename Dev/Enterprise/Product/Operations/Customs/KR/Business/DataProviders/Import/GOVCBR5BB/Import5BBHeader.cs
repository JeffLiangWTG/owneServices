using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("Import5BBHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5BBHeader : IImport5BBHeader
	{
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public string ImportDeclarationNumber { get; set; }
		public string TariffRateClassification { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyRate)]
		public decimal TariffRate { get; set; }
		public Organisation Declarant { get; set; }
		public Import5BBLine[] EntryLines { get; set; }

		ZString IImport5BBHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport5BBHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IEnumerable<IImport5BBLine> IImport5BBHeader.EntryLines => EntryLines;

		ZString IImport5BAHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImport5BAHeader.TariffRateClassification => TariffRateClassification;
		ZDecimal IImport5BAHeader.TariffRate => TariffRate;
		IOrganization IImport5BAHeader.Declarant => Declarant;
		IEnumerable<IImport5BALine> IImport5BAHeader.EntryLines => null;
		ZDate IImport5BAHeader.ImportDeclarationDate => ZDate.Empty;
	}
}
