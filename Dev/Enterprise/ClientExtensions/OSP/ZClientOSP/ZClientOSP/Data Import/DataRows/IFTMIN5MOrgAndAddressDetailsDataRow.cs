
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public class IFTMIN5MOrgAndAddressDetailsDataRow : IFTMIN5MBaseDataRow
	{
		public IFTMIN5MOrgAndAddressDetailsDataRow(ZString dataRow)
			: base(dataRow)
		{
		}

		public IFTMIN5MOrgAndAddressDetailsDataRow(IFTMIN5MBaseDataRow dataRow)
			: base(dataRow)
		{
		}

		static class Constants
		{
			public static class VATCode
			{
				public const int Length = 16;
				public const int Position = 3;
			}

			public static class Name
			{
				public const int Length = 35;
				public const int Position = 19;
			}

			public static class Address
			{
				public const int Length = 35;
				public const int Position = 54;
			}

			public static class City
			{
				public const int Length = 25;
				public const int Position = 89;
			}

			public static class Province
			{
				public const int Length = 2;
				public const int Position = 114;
			}

			public static class ZIPCode
			{
				public const int Length = 8;
				public const int Position = 116;
			}

			public static class CountryCode
			{
				public const int Length = 6;
				public const int Position = 124;
			}

			public static class CustomerCode
			{
				public const int Length = 6;
				public const int Position = 130;
			}
		}

		public ZString VATCode
		{
			get { return DataRow.SubstringSafe(Constants.VATCode.Position, Constants.VATCode.Length).Trim(); }
		}

		public ZString Name
		{
			get { return DataRow.SubstringSafe(Constants.Name.Position, Constants.Name.Length).Trim(); }
		}

		public ZString Address
		{
			get { return DataRow.SubstringSafe(Constants.Address.Position, Constants.Address.Length).Trim(); }
		}

		public ZString City
		{
			get { return DataRow.SubstringSafe(Constants.City.Position, Constants.City.Length).Trim(); }
		}

		public ZString Province
		{
			get { return DataRow.SubstringSafe(Constants.Province.Position, Constants.Province.Length).Trim(); }
		}

		public ZString ZIPCode
		{
			get { return DataRow.SubstringSafe(Constants.ZIPCode.Position, Constants.ZIPCode.Length).Trim(); }
		}

		public ZString CountryCode
		{
			get { return DataRow.SubstringSafe(Constants.CountryCode.Position, Constants.CountryCode.Length).Trim(); }
		}

		public ZString CustomerCode
		{
			get { return DataRow.SubstringSafe(Constants.CustomerCode.Position, Constants.CustomerCode.Length).Trim(); }
		}
	}
}
