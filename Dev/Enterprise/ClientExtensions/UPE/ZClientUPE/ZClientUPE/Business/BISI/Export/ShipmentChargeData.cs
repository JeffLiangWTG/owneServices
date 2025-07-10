using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	#region enum ShipmentChargeTypeCode

	public enum ShipmentChargeTypeCode
	{
		Unknown,
		Duty = 201,
		VAT = 205,
		GST = 206,
		Other = 216,
		Quarantine = 224,
		Tradegate = 231,
		QuarantinePermit = 309,
		Security = 348,
		Disbursement = 405,
		Terminal = 436,
		Insurance = 445,
		Freight = 500,
		ExtendedAreaSurcharge = 524,
		FuelSurcharge = 545,
		QuantumView = 565,
		ContactFee = 431,
		ChargePerLine = 410
	}

	public abstract class ShipmentChargeDescription
	{
		public const string Freight = "FREIGHT";
		public const string SecurityFee = "SECURITY FEE";
		public const string ITFCharges = "ITF CHARGES";
		public const string VAT = "VAT";
	}

	#endregion

	public class ShipmentChargeData
	{
		public ShipmentChargeData(ShipmentChargeTypeCode typeCode, ZDecimal grossAmount, ZString currencyCode)
		{
			fTypeCode = typeCode;
			fGrossAmount = grossAmount;
			this.CurrencyCode = currencyCode;
		}

		public ZString TypeCode
		{
			get { return ((int)fTypeCode).ToString(); }
		}

		public ShipmentChargeTypeCode TypeCodeEnum
		{
			get { return fTypeCode; }
		}

		public ZDecimal GrossAmount
		{
			get { return fGrossAmount; }
		}

		public ZDecimal PercentageRate
		{
			get { return 0m; }
		}

		public ZString CurrencyCode
		{
			get;
		}

		public void AddAmount(ZDecimal amountToAdd)
		{
			fGrossAmount += amountToAdd;
		}

		readonly ShipmentChargeTypeCode fTypeCode;
		ZDecimal fGrossAmount;
	}
}
