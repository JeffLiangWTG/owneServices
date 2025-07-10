
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public class IFTMIN5MContainersOnGroupageDataRow : IFTMIN5MBaseDataRow
	{
		public IFTMIN5MContainersOnGroupageDataRow(ZString dataRow)
			: base(dataRow)
		{
		}

		public IFTMIN5MContainersOnGroupageDataRow(IFTMIN5MBaseDataRow dataRow)
			: base(dataRow)
		{
		}

		static class Constants
		{
			public static class ContainerProgr
			{
				public const int Length = 3;
				public const int Position = 3;
			}

			public static class TypeContainer
			{
				public const int Length = 6;
				public const int Position = 6;
			}

			public static class PlateContainer
			{
				public const int Length = 4;
				public const int Position = 12;
			}

			public static class NumberContainer
			{
				public const int Length = 6;
				public const int Position = 16;
			}

			public static class CheckDigitContainer
			{
				public const int Length = 1;
				public const int Position = 22;
			}

			public static class Seal
			{
				public const int Length = 20;
				public const int Position = 23;
			}

			public static class TruckPlateOrIDNumber
			{
				public const int Length = 20;
				public const int Position = 43;
			}
		}

		public ZInt ContainerProgr
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.ContainerProgr.Position, Constants.ContainerProgr.Length).Trim()); }
		}

		public ZString TypeContainer
		{
			get { return DataRow.SubstringSafe(Constants.TypeContainer.Position, Constants.TypeContainer.Length).Trim(); }
		}

		public ZString PlateContainer
		{
			get { return DataRow.SubstringSafe(Constants.PlateContainer.Position, Constants.PlateContainer.Length).Trim(); }
		}

		public ZInt NumberContainer
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.NumberContainer.Position, Constants.NumberContainer.Length).Trim()); }
		}

		public ZInt CheckDigitContainer
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.CheckDigitContainer.Position, Constants.CheckDigitContainer.Length).Trim()); }
		}

		public ZString Seal
		{
			get { return DataRow.SubstringSafe(Constants.Seal.Position, Constants.Seal.Length).Trim(); }
		}

		public ZString TruckPlateRrIDNumber
		{
			get { return DataRow.SubstringSafe(Constants.TruckPlateOrIDNumber.Position, Constants.TruckPlateOrIDNumber.Length).Trim(); }
		}
	}
}
