
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _500000Line : RecordLine
	{
		#region Constants

		public new class Constants : RecordLine.Constants
		{
			public static class UnitQuantity
			{
				public const int Length = 4;
				public const int Position = 50;
			}

			public static class UnitQuantityCode
			{
				public const int Length = 3;
				public const int Position = 54;
			}

			public static class Description
			{
				public const int Length = 104;
				public const int Position = 57;
			}

			public static class Price
			{
				public const int Length = 10;
				public const int Position = 161;
			}

			public static class CurrencyCode
			{
				public const int Length = 3;
				public const int Position = 171;
			}

			public static class InvoiceNumber
			{
				public const int Length = 20;
				public const int Position = 174;
			}

			public static class CountryOfOrigin
			{
				public const int Length = 2;
				public const int Position = 194;
			}

			public static class CommodityCode
			{
				public const int Length = 20;
				public const int Position = 196;
			}

			public static class LicenceNumber
			{
				public const int Length = 15;
				public const int Position = 216;
			}

			public static class CountryOfUltimateDestination
			{
				public const int Length = 2;
				public const int Position = 231;
			}

			public static class PartNumber
			{
				public const int Length = 35;
				public const int Position = 233;
			}

			public static class Filler
			{
				public const int Length = 110;
				public const int Position = 268;
			}
		}

		#endregion

		public _500000Line(string value)
			: base(value)
		{
		}

		public ZString Description
		{
			get { return Value.SubstringSafe(Constants.Description.Position, Constants.Description.Length).Trim(); }
		}

		public ZDecimal Quantity
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.UnitQuantity.Position, Constants.UnitQuantity.Length)); }
		}

		public ZString UnitOfQuantity
		{
			get
			{
				ZString uPSUnitOfQuantity = Value.SubstringSafe(Constants.UnitQuantityCode.Position, Constants.UnitQuantityCode.Length).Trim();
				return UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode(uPSUnitOfQuantity);
			}
		}

		public ZDecimal Price
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.Price.Position, Constants.Price.Length)) / 100; }
		}

		public ZString CurrencyCode
		{
			get { return Value.SubstringSafe(Constants.CurrencyCode.Position, Constants.CurrencyCode.Length).Trim(); }
		}

		public ZString InvoiceNumber
		{
			get { return Value.SubstringSafe(Constants.InvoiceNumber.Position, Constants.InvoiceNumber.Length).Trim(); }
		}

		public ZString CountryOfOrigin
		{
			get { return Value.SubstringSafe(Constants.CountryOfOrigin.Position, Constants.CountryOfOrigin.Length).Trim(); }
		}

		public ZString CommodityCode
		{
			get { return Value.SubstringSafe(Constants.CommodityCode.Position, Constants.CommodityCode.Length).Trim(); }
		}

		public ZString LicenceNumber
		{
			get { return Value.SubstringSafe(Constants.LicenceNumber.Position, Constants.LicenceNumber.Length).Trim(); }
		}

		public ZString CountryOfUltimateDestination
		{
			get { return Value.SubstringSafe(Constants.CountryOfUltimateDestination.Position, Constants.CountryOfUltimateDestination.Length).Trim(); }
		}

		public ZString PartNumber
		{
			get { return Value.SubstringSafe(Constants.PartNumber.Position, Constants.PartNumber.Length).Trim(); }
		}
	}
}
