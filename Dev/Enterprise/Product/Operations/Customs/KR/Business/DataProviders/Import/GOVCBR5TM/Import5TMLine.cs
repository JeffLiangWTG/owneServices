using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5TMLine : IImport5TMLine
	{
		public int EntryLineNo { get; set; }
		public string HSCode { get; set; }
		public string HSDescription { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Weight)]
		public decimal NetWeightInKG { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal ValueForVAT { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal VAT { get; set; }

		ZInt IImport5TMLine.EntryLineNo => EntryLineNo;
		ZString IImport5TMLine.HSCode => HSCode;
		ZString IImport5TMLine.HSDescription => HSDescription;
		ZDecimal IImport5TMLine.NetWeightKG => NetWeightInKG;
		ZDecimal IImport5TMLine.ValueForVAT => ValueForVAT;
		ZDecimal IImport5TMLine.VAT => VAT;
	}
}
