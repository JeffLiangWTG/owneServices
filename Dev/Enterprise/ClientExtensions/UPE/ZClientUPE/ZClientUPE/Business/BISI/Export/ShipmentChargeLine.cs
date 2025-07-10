using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ShipmentChargeLine : RecordBodyLine
	{
		public ShipmentChargeLine(IShipmentData shipmentData, ShipmentChargeData chargeData)
			: base(shipmentData)
		{
			this.ChargeData = chargeData;
		}

		#region Overrides

		protected override ZString LineType
		{
			get { return LineTypes.ChargeDetails; }
		}

		protected override void AppendContentFields(ZStringBuilder lineBuilder)
		{
			AppendFixedLengthField(lineBuilder, ChargeData.TypeCode, Length.TypeCode);
			AppendFixedLengthField(lineBuilder, ChargeData.GrossAmount, DecimalPlace.GrossAmount, Length.GrossAmount);
			AppendFixedLengthField(lineBuilder, ChargeData.PercentageRate, DecimalPlace.PercentageRate, Length.PercentageRate);
			AppendFixedLengthField(lineBuilder, ChargeData.CurrencyCode, Length.CurrencyCode);
			AppendFixedLengthField(lineBuilder, "", Length.Filler);
		}

		public ZBool IsCustomCharge { get; set; }

		#endregion

		#region Constants

		public class Length : BaseLength
		{
			public const int TypeCode = 3;
			public const int GrossAmount = 13;
			public const int PercentageRate = 4;
			public const int CurrencyCode = 3;
			public const int Filler = 245;
		}

		abstract class DecimalPlace
		{
			public const int GrossAmount = 2;
			public const int PercentageRate = 3;
		}

		#endregion

		public IShipmentData ShipmentData
		{
			get { return (IShipmentData)base.LineKey; }
		}

		public readonly ShipmentChargeData ChargeData;
	}
}
