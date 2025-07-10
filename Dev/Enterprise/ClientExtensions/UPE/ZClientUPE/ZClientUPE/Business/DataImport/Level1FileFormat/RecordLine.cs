using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class RecordLine : UPERecordBase
	{
		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Class is inherited and cannot be static")]
		public class Constants
		{
			public const int RecordKeyLength = 44;

			public static class RecordTypes
			{
				public const string _100000 = "100000";
				public const string _200000 = "200000";
				public const string _201000 = "201000";
				public const string _202000 = "202000";
				public const string _300000 = "300000";
				public const string _400000 = "400000";
				public const string _401000 = "401000";
				public const string _402000 = "402000";
				public const string _500000 = "500000";
				public const string _500000Type = "5";
				public const string _600000 = "600000";
				public const string _600000Type = "6";
				public const string _900000 = "900000";
			}

			public static class OriginCountry
			{
				public const int Length = 2;
				public const int Position = 0;
			}

			public static class OriginPort
			{
				public const int Length = 4;
				public const int Position = 2;
			}

			public static class DestinationCountry
			{
				public const int Length = 2;
				public const int Position = 6;
			}

			public static class DestinationPort
			{
				public const int Length = 4;
				public const int Position = 8;
			}

			public static class ImportDate
			{
				public const int Length = 6;
				public const int Position = 12;
			}

			public static class MAWB
			{
				public const int Length = 14;
				public const int Position = 18;
			}

			public static class DutyType
			{
				public const int Length = 1;
				public const int Position = 32;
			}

			public static class ShipmentNumber
			{
				public const int Length = 11;
				public const int Position = 33;
			}

			public static class RecordType
			{
				public const int Length = 6;
				public const int Position = 44;
			}
		}

		#endregion

		public RecordLine(ZString value)
		{
			this.Value = value;
		}

		public string RecordKey
		{
			get { return Value.Left(Constants.RecordKeyLength); }
		}

		public ZString OriginCountry
		{
			get { return Value.SubstringSafe(Constants.OriginCountry.Position, Constants.OriginCountry.Length); }
		}

		public string OriginPort
		{
			get { return Value.SubstringSafe(Constants.OriginPort.Position, Constants.OriginPort.Length); }
		}

		public string DestinationCountry
		{
			get { return Value.SubstringSafe(Constants.DestinationCountry.Position, Constants.DestinationCountry.Length); }
		}

		public string DestinationPort
		{
			get { return Value.SubstringSafe(Constants.DestinationPort.Position, Constants.DestinationPort.Length); }
		}

		public string DutyType
		{
			get
			{
				string result = "";

				string dutyType = Value.SubstringSafe(Constants.DutyType.Position, Constants.DutyType.Length);
				switch (dutyType)
				{
					case "D":
						result = DutyTypeCodeDescriptionPairList.Codes.Dutiable;
						break;
					case "N":
						result = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
						break;
					case "L":
						result = DutyTypeCodeDescriptionPairList.Codes.LowValue;
						break;
					case "C":
						result = DutyTypeCodeDescriptionPairList.Codes.GCC;
						break;
				}

				return result;
			}
		}

		public ZString ShipmentNumber
		{
			get { return Value.SubstringSafe(Constants.ShipmentNumber.Position, Constants.ShipmentNumber.Length).Trim(); }
		}

		public readonly ZString Value;
	}
}
