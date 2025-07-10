
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public class IFTMIN5MShipmentsDetailsDataRow : IFTMIN5MBaseDataRow
	{
		public IFTMIN5MShipmentsDetailsDataRow(ZString dataRow)
			: base(dataRow)
		{
		}

		public IFTMIN5MShipmentsDetailsDataRow(IFTMIN5MBaseDataRow dataRow)
			: base(dataRow)
		{
		}

		protected override string DateTimeFormat
		{
			get { return "yyMMdd"; }
		}

		static class Constants
		{
			public static class ShipmentNumber
			{
				public const int Length = 20;
				public const int Position = 3;
			}

			public static class ShipmentDate
			{
				public const int Length = 7;
				public const int Position = 23;
			}

			public static class TermsOfDelivery
			{
				public const int Length = 6;
				public const int Position = 30;
			}

			public static class TransportType
			{
				public const int Length = 6;
				public const int Position = 36;
			}

			public static class DepartmentCode
			{
				public const int Length = 6;
				public const int Position = 42;
			}

			public static class RequestedDeliveryDate
			{
				public const int Length = 7;
				public const int Position = 48;
			}

			public static class RequestedDeliveryTime
			{
				public const int Length = 4;
				public const int Position = 55;
			}

			public static class DeliveryPriority
			{
				public const int Length = 6;
				public const int Position = 59;
			}

			public static class Remark1
			{
				public const int Length = 60;
				public const int Position = 65;
			}

			public static class DeliveryOnWheels
			{
				public const int Length = 1;
				public const int Position = 125;
			}

			public static class DeliveryLimitCompulsory
			{
				public const int Length = 1;
				public const int Position = 126;
			}

			public static class BarcodesQuantity
			{
				public const int Length = 3;
				public const int Position = 127;
			}
		}

		public ZString ShipmentNumber
		{
			get { return DataRow.SubstringSafe(Constants.ShipmentNumber.Position, Constants.ShipmentNumber.Length).Trim(); }
		}

		public ZDateTime ShipmentDate
		{
			get { return ToZDateTime(DataRow.SubstringSafe(Constants.ShipmentDate.Position, Constants.ShipmentDate.Length).Right(6).Trim()); }
		}

		public ZString TermsOfDelivery
		{
			get { return DataRow.SubstringSafe(Constants.TermsOfDelivery.Position, Constants.TermsOfDelivery.Length).Trim(); }
		}

		public ZString TransportType
		{
			get { return DataRow.SubstringSafe(Constants.TransportType.Position, Constants.TransportType.Length).Trim(); }
		}

		public ZString DepartmentCode
		{
			get { return DataRow.SubstringSafe(Constants.DepartmentCode.Position, Constants.DepartmentCode.Length).Trim(); }
		}

		public ZDateTime RequestedDeliveryDateTime
		{
			get
			{
				ZDateTime result = ToZDateTime(DataRow.SubstringSafe(Constants.RequestedDeliveryDate.Position, Constants.RequestedDeliveryDate.Length).Right(6).Trim());
				return !result.IsEmpty && result.IsValid ? result.AddHours(ToZInt(DataRow.SubstringSafe(Constants.RequestedDeliveryTime.Position, Constants.RequestedDeliveryTime.Length).Left(2).Trim())).AddMinutes(ToZInt(DataRow.SubstringSafe(Constants.RequestedDeliveryTime.Position, Constants.RequestedDeliveryTime.Length).Right(2).Trim())) : result;
			}
		}

		public ZString DeliveryPriority
		{
			get { return DataRow.SubstringSafe(Constants.DeliveryPriority.Position, Constants.DeliveryPriority.Length).Trim(); }
		}

		public ZString Remark1
		{
			get { return DataRow.SubstringSafe(Constants.Remark1.Position, Constants.Remark1.Length).Trim(); }
		}

		public ZString DeliveryOnWheels
		{
			get { return DataRow.SubstringSafe(Constants.DeliveryOnWheels.Position, Constants.DeliveryOnWheels.Length).Trim(); }
		}

		public ZString DeliveryLimitCompulsory
		{
			get { return DataRow.SubstringSafe(Constants.DeliveryLimitCompulsory.Position, Constants.DeliveryLimitCompulsory.Length).Trim(); }
		}

		public ZInt BarcodesQuantity
		{
			get { return ToZInt(DataRow.SubstringSafe(Constants.BarcodesQuantity.Position, Constants.BarcodesQuantity.Length).Trim()); }
		}
	}
}
