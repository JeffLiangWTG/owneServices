
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class PWSChargeDetails : UPERecordBase
	{
		#region Constants

		static class Constants
		{
			public abstract class Description
			{
				public const int Length = 20;
				public const int Position = 0;
			}

			public abstract class TaxableAmount
			{
				public const int Length = 17;
				public const int Position = 20;
			}

			public abstract class NonTaxableAmount
			{
				public const int Length = 17;
				public const int Position = 37;
			}

			public abstract class Discount
			{
				public const int Length = 17;
				public const int Position = 54;
			}

			public abstract class NettAmount
			{
				public const int Length = 17;
				public const int Position = 71;
			}
		}

		#endregion

		public PWSChargeDetails(ZString chargeData)
		{
			this.ChargeData = chargeData;
		}

		public ZString ChargeDescription
		{
			get { return ChargeData.SubstringSafe(Constants.Description.Position, Constants.Description.Length).Trim(); }
		}

		public ZDecimal TaxableAmount
		{
			get { return ToZDecimal(ChargeData.SubstringSafe(Constants.TaxableAmount.Position, Constants.TaxableAmount.Length)); }
		}

		public ZDecimal NonTaxableAmount
		{
			get { return ToZDecimal(ChargeData.SubstringSafe(Constants.NonTaxableAmount.Position, Constants.NonTaxableAmount.Length)); }
		}

		public ZDecimal Discount
		{
			get { return ToZDecimal(ChargeData.SubstringSafe(Constants.Discount.Position, Constants.Discount.Length)); }
		}

		public ZDecimal NettAmount
		{
			get { return ToZDecimal(ChargeData.SubstringSafe(Constants.NettAmount.Position, Constants.NettAmount.Length)); }
		}

		readonly ZString ChargeData;
	}
}
