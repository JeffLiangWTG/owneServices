using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("Import5BAHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5BAHeader : IImport5BAHeader
	{
		[XmlIgnore]
		public string ImportDeclarationNumber { get; set; }
		[XmlIgnore]
		public DateTime ImportDeclarationDate { get; set; }
		[XmlIgnore]
		public Organisation Declarant { get; set; }
		public string TariffRateClassification { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyRate)]
		public decimal TariffRate { get; set; }
		public Import5BALine[] EntryLines { get; set; }

		ZString IImport5BAHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZDate IImport5BAHeader.ImportDeclarationDate => new ZDate(ImportDeclarationDate);
		IOrganization IImport5BAHeader.Declarant => Declarant;
		ZString IImport5BAHeader.TariffRateClassification => TariffRateClassification;
		ZDecimal IImport5BAHeader.TariffRate => TariffRate;
		IEnumerable<IImport5BALine> IImport5BAHeader.EntryLines => EntryLines;
	}
}
