using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5ULTaxItem : IImport5ULTaxItem
	{
		public string TaxItem { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal Tax { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal PenaltyAmount { get; set; }

		ZString IImport5ULTaxItem.TaxItem => TaxItem;
		ZDecimal IImport5ULTaxItem.Tax => Tax;
		ZDecimal IImport5ULTaxItem.PenaltyAmount => PenaltyAmount;
	}
}
