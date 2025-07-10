
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public class IFTMIN5MGoodsDataRow : IFTMIN5MBaseDataRow
	{
		public IFTMIN5MGoodsDataRow(ZString dataRow)
			: base(dataRow)
		{
		}

		 public IFTMIN5MGoodsDataRow(IFTMIN5MBaseDataRow dataRow)
			: base(dataRow)
		{
		}

		static class Constants
		{
			public static class LineCounter
			{
				public const int Length = 3;
				public const int Position = 3;
			}

			public static class GoodsCode
			{
				public const int Length = 6;
				public const int Position = 6;
			}

			public static class GoodsDescription
			{
				public const int Length = 35;
				public const int Position = 12;
			}

			public static class PackageTypeCode
			{
				public const int Length = 6;
				public const int Position = 47;
			}

			public static class NumberOfPackages
			{
				public const int Length = 7;
				public const int Position = 53;
			}

			public static class GrossWeightKG
			{
				public const int Length = 10;
				public const int Position = 60;
			}

			public static class NetWeightKG
			{
				public const int Length = 12;
				public const int Position = 70;
			}

			public static class Volume
			{
				public const int Length = 8;
				public const int Position = 82;
			}

			public static class LenghtMetres
			{
				public const int Length = 7;
				public const int Position = 90;
			}

			public static class AdditionalMeasureUnitCode
			{
				public const int Length = 6;
				public const int Position = 97;
			}

			public static class AdditionalQuantity
			{
				public const int Length = 8;
				public const int Position = 103;
			}

			public static class NumberOfPallets
			{
				public const int Length = 3;
				public const int Position = 111;
			}

			public static class TypeOfPalletCode
			{
				public const int Length = 6;
				public const int Position = 114;
			}

			public static class MarksAndNumbers
			{
				public const int Length = 20;
				public const int Position = 120;
			}

			public static class GoodsValueCurrencyCode
			{
				public const int Length = 6;
				public const int Position = 140;
			}

			public static class GoodsValueAmount
			{
				public const int Length = 13;
				public const int Position = 146;
			}
		}

		public ZInt CodeVessel
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.LineCounter.Position, Constants.LineCounter.Length).Trim()); }
		}

		public ZString GoodsCode
		{
			get { return DataRow.SubstringSafe(Constants.GoodsCode.Position, Constants.GoodsCode.Length).Trim(); }
		}

		public ZString GoodsDescription
		{
			get { return DataRow.SubstringSafe(Constants.GoodsDescription.Position, Constants.GoodsDescription.Length).Trim(); }
		}

		public ZString PackageTypeCode
		{
			get { return DataRow.SubstringSafe(Constants.PackageTypeCode.Position, Constants.PackageTypeCode.Length).Trim(); }
		}

		public ZDecimal NumberOfPackages
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.NumberOfPackages.Position, Constants.NumberOfPackages.Length).Trim()); }
		}

		public ZDecimal GrossWeightKG
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.GrossWeightKG.Position, Constants.GrossWeightKG.Length).Trim()); }
		}

		public ZDecimal NetWeightKG
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.NetWeightKG.Position, Constants.NetWeightKG.Length).Trim()); }
		}

		public ZDecimal Volume
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.Volume.Position, Constants.Volume.Length).Trim()); }
		}

		public ZDecimal LenghtMetres
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.LenghtMetres.Position, Constants.LenghtMetres.Length).Trim()); }
		}

		public ZString AdditionalMeasureUnitCode
		{
			get { return DataRow.SubstringSafe(Constants.AdditionalMeasureUnitCode.Position, Constants.AdditionalMeasureUnitCode.Length).Trim(); }
		}

		public ZDecimal AdditionalQuantity
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.AdditionalQuantity.Position, Constants.AdditionalQuantity.Length).Trim()); }
		}

		public ZInt NumberOfPallets
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.NumberOfPallets.Position, Constants.NumberOfPallets.Length).Trim()); }
		}

		public ZString MarksAndNumbers
		{
			get { return DataRow.SubstringSafe(Constants.MarksAndNumbers.Position, Constants.MarksAndNumbers.Length).Trim(); }
		}

		public ZString TypeOfPalletCode
		{
			get { return DataRow.SubstringSafe(Constants.TypeOfPalletCode.Position, Constants.TypeOfPalletCode.Length).Trim(); }
		}

		public ZString GoodsValueCurrencyCode
		{
			get { return DataRow.SubstringSafe(Constants.GoodsValueCurrencyCode.Position, Constants.GoodsValueCurrencyCode.Length).Trim(); }
		}

		public ZDecimal GoodsValueAmount
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.GoodsValueAmount.Position, Constants.GoodsValueAmount.Length).Trim()); }
		}
	}
}
