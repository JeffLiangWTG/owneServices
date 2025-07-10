
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public class IFTMIN5MGoodsDetailsDataRow : IFTMIN5MBaseDataRow
	{
		public IFTMIN5MGoodsDetailsDataRow(ZString dataRow)
			: base(dataRow)
		{
		}

		public IFTMIN5MGoodsDetailsDataRow(IFTMIN5MBaseDataRow dataRow)
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

			public static class MarksAndNumbers
			{
				public const int Length = 20;
				public const int Position = 6;
			}

			public static class NumberOfPackages
			{
				public const int Length = 7;
				public const int Position = 26;
			}

			public static class PackagingDescription
			{
				public const int Length = 10;
				public const int Position = 33;
			}

			public static class GoodsDetailsDescription1
			{
				public const int Length = 40;
				public const int Position = 43;
			}

			public static class GoodsDetailsDescription2
			{
				public const int Length = 40;
				public const int Position = 83;
			}

			public static class GrossWeightKG
			{
				public const int Length = 10;
				public const int Position = 123;
			}

			public static class Volume
			{
				public const int Length = 8;
				public const int Position = 133;
			}

			public static class ADRNumber
			{
				public const int Length = 8;
				public const int Position = 141;
			}

			public static class ADRClass
			{
				public const int Length = 4;
				public const int Position = 149;
			}

			public static class ADRDigit
			{
				public const int Length = 4;
				public const int Position = 153;
			}

			public static class ADRLetter
			{
				public const int Length = 4;
				public const int Position = 157;
			}

			public static class ContainerProgr
			{
				public const int Length = 3;
				public const int Position = 161;
			}
		}

		public ZInt LineCounter
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.LineCounter.Position, Constants.LineCounter.Length).Trim()); }
		}

		public ZString MarksAndNumbers
		{
			get { return DataRow.SubstringSafe(Constants.MarksAndNumbers.Position, Constants.MarksAndNumbers.Length).Trim(); }
		}

		public ZInt NumberOfPackages
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.NumberOfPackages.Position, Constants.NumberOfPackages.Length).Trim()); }
		}

		public ZString PackagingDescription
		{
			get { return DataRow.SubstringSafe(Constants.PackagingDescription.Position, Constants.PackagingDescription.Length).Trim(); }
		}

		public ZString GoodsDetailsDescription1
		{
			get { return DataRow.SubstringSafe(Constants.GoodsDetailsDescription1.Position, Constants.GoodsDetailsDescription1.Length).Trim(); }
		}

		public ZString GoodsDetailsDescription2
		{
			get { return DataRow.SubstringSafe(Constants.GoodsDetailsDescription2.Position, Constants.GoodsDetailsDescription2.Length).Trim(); }
		}

		public ZDecimal GrossWeightKG
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.GrossWeightKG.Position, Constants.GrossWeightKG.Length).Trim()); }
		}

		public ZDecimal Volume
		{
			get { return ToZDecimal(DataRow.SubstringSafe(Constants.Volume.Position, Constants.Volume.Length).Trim()); }
		}

		public ZString ADRNumber
		{
			get { return DataRow.SubstringSafe(Constants.ADRNumber.Position, Constants.ADRNumber.Length).Trim(); }
		}

		public ZString ADRClass
		{
			get { return DataRow.SubstringSafe(Constants.ADRClass.Position, Constants.ADRClass.Length).Trim(); }
		}

		public ZString ADRDigit
		{
			get { return DataRow.SubstringSafe(Constants.ADRDigit.Position, Constants.ADRDigit.Length).Trim(); }
		}

		public ZString ADRLetter
		{
			get { return DataRow.SubstringSafe(Constants.ADRLetter.Position, Constants.ADRLetter.Length).Trim(); }
		}

		public ZInt ContainerProgr
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.ContainerProgr.Position, Constants.ContainerProgr.Length).Trim()); }
		}
	}
}
