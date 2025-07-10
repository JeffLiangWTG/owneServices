
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class CommodityDetailLine : RecordBodyLine
	{
		public CommodityDetailLine(IShipmentData shipmentData, CommodityDetailData commodityData)
			: this(shipmentData, commodityData, 0)
		{
		}

		public CommodityDetailLine(IShipmentData shipmentData, CommodityDetailData commodityData, int index)
			: base(shipmentData)
		{
			this.CommodityData = commodityData;
			this.Index = index;
		}

		public IShipmentData ShipmentData
		{
			get { return (IShipmentData)base.LineKey; }
		}

		public readonly CommodityDetailData CommodityData;

		#region Overrides

		protected override ZString LineType
		{
			get { return (int.Parse(LineTypes.CommodityDetails) + Index).ToString(); }
		}

		protected override void AppendContentFields(ZStringBuilder lineBuilder)
		{
			AppendFixedLengthField(lineBuilder, CommodityData.GoodsDescription, Length.GoodsDescription);
			AppendFixedLengthField(lineBuilder, CommodityData.TariffNumber, Length.TariffNumber);
			AppendFixedLengthField(lineBuilder, CommodityData.CountryOfOrigin, Length.CountryOfOrigin);
			AppendFixedLengthField(lineBuilder, CommodityData.Weight, DecimalPlace.Weight, Length.Weight);
			AppendFixedLengthField(lineBuilder, CommodityData.ItemPrice, DecimalPlace.ItemPrice, Length.ItemPrice);
			AppendFixedLengthField(lineBuilder, CommodityData.CPC, Length.CPC);
			AppendFixedLengthField(lineBuilder, "", Length.Filler);
		}

		#endregion

		#region Constants

		static class Length
		{
			public const int GoodsDescription = 50;
			public const int TariffNumber = 35;
			public const int CountryOfOrigin = 2;
			public const int Weight = 7;
			public const int ItemPrice = 13;
			public const int CPC = 6;
			public const int Filler = 155;
		}

		static class DecimalPlace
		{
			public const int Weight = 1;
			public const int ItemPrice = 2;
		}

		#endregion

		readonly int Index;
	}
}
