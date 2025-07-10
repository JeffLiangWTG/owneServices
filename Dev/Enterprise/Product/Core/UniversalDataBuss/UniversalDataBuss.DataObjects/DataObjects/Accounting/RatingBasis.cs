using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class RatingBasis : IDataObject
	{
		[MaxLength(50), Mandatory]
		public ZString? OriginType { get; set; }
		[MaxLength(50)]
		public ZString? OriginKey { get; set; }
		public ZDecimal? OriginQuantity { get; set; }
		[MaxLength(2000)]
		public ZString? OriginAdditionalReference { get; set; }
		public ZDecimal? MinimumRate { get; set; }
		public ZDecimal? MaximumRate { get; set; }
		public ZDecimal? FlatRate { get; set; }
		public ZDecimal? PerUnitRate { get; set; }
		public Currency Currency { get; set; }
		public RatingUnit OriginQuantityUnit { get; set; }
		public RatingUnit RateUnit { get; set; }
		public ZBool? IsPercentageRate { get; set; }

		/// <summary>
		/// Type of calculation that the rate is based on. Proxied for JobPaymentBasis.PBS_RateReference.
		/// For example, the value can be UNT for unit calculation, or MIN for minimum rate applied.
		/// </summary>
		[MaxLength(3)]
		public ZString? RateReference { get; set; }
	}
}