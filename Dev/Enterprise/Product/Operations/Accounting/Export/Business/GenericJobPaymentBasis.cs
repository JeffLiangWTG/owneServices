using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Export.Business
{
	public class GenericJobPaymentBasis : IJobPaymentBasis
	{
		public ZGuid JobChargePK { get; set; }
		public ZGuid ConsolCostPK { get; set; }
		public ZBool IsCost { get; set; }
		public ZString AdapterID { get; set; }
		public ZString AdapterType { get; set; }
		public ZString ChargeableDescription { get; set; }
		public ZDecimal MinRate { get; set; }
		public ZDecimal MaxRate { get; set; }
		public ZDecimal FlatRate { get; set; }
		public ZDecimal PerUnitRate { get; set; }
		public ZDecimal ChargeableAmount { get; set; }
		public ZString ChargeableUnit { get; set; }
		public ZString ChargeableUnitType { get; set; }
		public ZString RateUnit { get; set; }
		public ZString RateUnitType { get; set; }
		public ZString RateCurrency { get; set; }
		public ZString RateCurrencyDescription { get; set; }
		public ZString RateReference { get; set; }
	}
}
