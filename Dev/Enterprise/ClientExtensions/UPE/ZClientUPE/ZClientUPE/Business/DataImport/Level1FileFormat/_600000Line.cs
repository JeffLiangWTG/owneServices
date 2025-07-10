

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _600000Line : RecordLine
	{
		#region Constants

		public new class Constants : RecordLine.Constants
		{
			public static class Filler
			{
				public const int Length = 11;
				public const int Position = 50;
			}

			public static class BagNumber
			{
				public const int Length = 4;
				public const int Position = 61;
			}

			public static class PackageWeight
			{
				public const int Length = 3;
				public const int Position = 65;
			}

			public static class PackageWegihtUQ
			{
				public const int Length = 3;
				public const int Position = 68;
			}

			public static class PackageRevenue
			{
				public const int Length = 9;
				public const int Position = 71;
			}

			public static class CurrencyCode
			{
				public const int Length = 3;
				public const int Position = 80;
			}

			public static class InsuranceChargeAmount
			{
				public const int Length = 9;
				public const int Position = 83;
			}

			public static class InsuranceCurrencyCode
			{
				public const int Length = 3;
				public const int Position = 92;
			}

			public static class OversizePackage
			{
				public const int Length = 1;
				public const int Position = 95;
			}

			public static class RegisterChargeAmount
			{
				public const int Length = 9;
				public const int Position = 96;
			}

			public static class RegisterChargeCurrencyCode
			{
				public const int Length = 3;
				public const int Position = 105;
			}

			public static class ScanningInfo
			{
				public const int Length = 29;
				public const int Position = 108;
			}

			public static class InboundContainerNumber
			{
				public const int Length = 11;
				public const int Position = 137;
			}

			public static class InboundFlightNumber
			{
				public const int Length = 8;
				public const int Position = 148;
			}

			public static class OutboundFlightNumber
			{
				public const int Length = 8;
				public const int Position = 156;
			}

			public static class IncompleteShipment
			{
				public const int Length = 1;
				public const int Position = 164;
			}

			public static class BypassBagOrContainer
			{
				public const int Length = 1;
				public const int Position = 165;
			}

			public static class ExpandedChildPackageTrackingNumber
			{
				public const int Length = 35;
				public const int Position = 166;
			}

			public static class ExpandedPackageWeight
			{
				public const int Length = 7;
				public const int Position = 201;
			}

			public static class PackageLoad
			{
				public const int Length = 14;
				public const int Position = 208;
			}

			public static class EndFiller
			{
				public const int Length = 156;
				public const int Position = 222;
			}
		}

		#endregion

		public _600000Line(string value)
			: base(value)
		{
		}

		public string ShortTrackingNumber
		{
			get { return Value.SubstringSafe(Constants.Filler.Position, Constants.Filler.Length).Trim(); }
		}

		public string LongTrackingNumber
		{
			get { return Value.SubstringSafe(Constants.ExpandedChildPackageTrackingNumber.Position, Constants.ExpandedChildPackageTrackingNumber.Length).Trim(); }
		}
	}
}
