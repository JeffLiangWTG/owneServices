using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedOfficeHoursEntry : IExtendedOfficeHoursEntry
	{
		public string ReferenceNumber { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public decimal TotalCustomsValueInUSD { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.NoOfPacks)]
		public decimal TotalPackQty { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal TotalGrossWeightInKG { get; set; }

		ZString IExtendedOfficeHoursEntry.ReferenceNumber => ReferenceNumber;
		ZDecimal IExtendedOfficeHoursEntry.TotalCustomsValueInUSD => TotalCustomsValueInUSD;
		ZDecimal IExtendedOfficeHoursEntry.TotalPackQty => TotalPackQty;
		ZDecimal IExtendedOfficeHoursEntry.TotalGrossWeightInKG => TotalGrossWeightInKG;
	}
}
