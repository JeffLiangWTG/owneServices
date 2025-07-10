using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5FETaxItem : IImport5FETaxItem
	{
		public string DutyTaxType { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public decimal BeforeAmount { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public decimal AfterAmount { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public decimal AmountDifference { get; set; }

		ZString IImport5FETaxItem.DutyTaxType => DutyTaxType;
		ZDecimal IImport5FETaxItem.BeforeAmount => BeforeAmount;
		ZDecimal IImport5FETaxItem.AfterAmount => AfterAmount;
		ZDecimal IImport5FETaxItem.AmountDifference => AmountDifference;
	}
}
