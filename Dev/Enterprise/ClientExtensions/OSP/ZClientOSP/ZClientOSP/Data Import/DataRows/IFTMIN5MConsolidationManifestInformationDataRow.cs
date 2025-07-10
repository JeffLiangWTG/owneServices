
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	class IFTMIN5MConsolidationManifestInformationDataRow : IFTMIN5MBaseDataRow
	{
		public IFTMIN5MConsolidationManifestInformationDataRow(ZString dataRow)
			: base(dataRow)
		{
		}

		public IFTMIN5MConsolidationManifestInformationDataRow(IFTMIN5MBaseDataRow dataRow)
			: base(dataRow)
		{
		}

		static class Constants
		{
			public static class ManifestNumber
			{
				public const int Length = 20;
				public const int Position = 3;
			}

			public static class ManifestDate
			{
				public const int Length = 8;
				public const int Position = 23;
			}

			public static class CodeVessel
			{
				public const int Length = 6;
				public const int Position = 31;
			}

			public static class VojageNumber
			{
				public const int Length = 8;
				public const int Position = 37;
			}

			public static class DeparturePortCode
			{
				public const int Length = 6;
				public const int Position = 45;
			}

			public static class SealineCode
			{
				public const int Length = 6;
				public const int Position = 51;
			}

			public static class ArrivePortCode
			{
				public const int Length = 6;
				public const int Position = 57;
			}

			public static class ETD
			{
				public const int Length = 8;
				public const int Position = 63;
			}

			public static class ETA
			{
				public const int Length = 8;
				public const int Position = 71;
			}
		}

		public ZString ManifestNumber
		{
			get { return DataRow.SubstringSafe(Constants.ManifestNumber.Position, Constants.ManifestNumber.Length).Trim(); }
		}

		public ZDateTime ManifestDate
		{
			get { return ToZDateTime(DataRow.SubstringSafe(Constants.ManifestDate.Position, Constants.ManifestDate.Length).Trim()); }
		}

		public ZString CodeVessel
		{
			get { return DataRow.SubstringSafe(Constants.CodeVessel.Position, Constants.CodeVessel.Length).Trim(); }
		}

		public ZString VojageNumber
		{
			get { return DataRow.SubstringSafe(Constants.VojageNumber.Position, Constants.VojageNumber.Length).Trim(); }
		}

		public ZString DeparturePortCode
		{
			get { return DataRow.SubstringSafe(Constants.DeparturePortCode.Position, Constants.DeparturePortCode.Length).Trim(); }
		}

		public ZString SealineCode
		{
			get { return DataRow.SubstringSafe(Constants.SealineCode.Position, Constants.SealineCode.Length).Trim(); }
		}

		public ZString ArrivePortCode
		{
			get { return DataRow.SubstringSafe(Constants.ArrivePortCode.Position, Constants.ArrivePortCode.Length).Trim(); }
		}

		public ZDateTime ETD
		{
			get { return ToZDateTime(DataRow.SubstringSafe(Constants.ETD.Position, Constants.ETD.Length).Trim()); }
		}

		public ZDateTime ETA
		{
			get { return ToZDateTime(DataRow.SubstringSafe(Constants.ETA.Position, Constants.ETA.Length).Trim()); }
		}
	}
}
