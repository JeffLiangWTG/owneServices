
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _202000Line : RecordLine
	{
		public new class Constants : RecordLine.Constants
		{
			public static class ExpandedShipmentWeight
			{
				public const int Length = 7;
				public const int Position = 85;
			}

			public static class ExpandedPackagesInShipment
			{
				public const int Length = 5;
				public const int Position = 99;
			}

			public static class PackageTrackingNumber
			{
				public const int Length = 35;
				public const int Position = 50;
			}
		}

		public _202000Line(ZString value)
			: base(value)
		{
		}

		public ZString PackageTrackingNumber
		{
			get { return Value.SubstringSafe(Constants.PackageTrackingNumber.Position, Constants.PackageTrackingNumber.Length).Trim(); }
		}

		public ZDecimal Weight
		{
			get { return ToZDecimal(Value.SubstringSafe(Constants.ExpandedShipmentWeight.Position, Constants.ExpandedShipmentWeight.Length)); }
		}

		public ZInt PiecesManifested
		{
			get { return ToZInt(Value.SubstringSafe(Constants.ExpandedPackagesInShipment.Position, Constants.ExpandedPackagesInShipment.Length)); }
		}
	}
}
