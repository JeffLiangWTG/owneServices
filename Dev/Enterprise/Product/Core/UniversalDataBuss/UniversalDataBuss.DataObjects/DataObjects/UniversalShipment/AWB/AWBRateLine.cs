using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public class AWBRateLine : IDataObject
	{
		public ZInt? LineNumber { get; set; }
		[MaxLength(4)]
		public ZString? NoOfPiecesOrRCP { get; set; }
		public ZDecimal? GrossWeight { get; set; }
		public CodeDescriptionPair1Char WeightUnit { get; set; }
		public CodeDescriptionPair1Char RateClass { get; set; }
		[MaxLength(7)]
		public ZString? CommodityItem { get; set; }
		public ZDecimal? ChargeableWeight { get; set; }
		public ZDecimal? RateChargeOrDiscount { get; set; }
		public ZDecimal? TotalCharge { get; set; }

		#region Nature And Quantity of Goods

		public CodeDescriptionPair1Char NatureAndQtyOfGoodsType { get; set; }
		[MaxLength(35)]
		public ZString? GoodsDescription { get; set; }
		[MaxLength(35)]
		public ZString? Consolidation { get; set; }
		public AWBRateLineDimensions Dimensions { get; set; }
		public AWBRateLineVolume Volume { get; set; }
		[MaxLength(35)]
		public ZString? ULDNumber { get; set; }
		public ZInt? ShippersLoadAndCount { get; set; }
		[MaxLength(35)]
		public ZString? HarmonizedCommodityCode { get; set; }
		[MaxLength(2)]
		public Country CountryOfOrigin { get; set; }

		#endregion
	}
}
