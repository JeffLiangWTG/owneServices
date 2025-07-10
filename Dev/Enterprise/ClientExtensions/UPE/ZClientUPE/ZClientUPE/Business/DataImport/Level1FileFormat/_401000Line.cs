
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _401000Line : _OrganisationLine
	{
		#region Constants

		public new class Constants : RecordLine.Constants
		{
			public static class DeliveryComp
			{
				public const int Length = 4;
				public const int Position = 50;
			}

			public static class DeliveryCentre
			{
				public const int Length = 4;
				public const int Position = 54;
			}

			public static class Number
			{
				public const int Length = 10;
				public const int Position = 58;
			}

			public static class Name
			{
				public const int Length = 35;
				public const int Position = 68;
			}

			public static class ContactName
			{
				public const int Length = 25;
				public const int Position = 103;
			}

			public static class Building
			{
				public const int Length = 35;
				public const int Position = 128;
			}

			public static class Street
			{
				public const int Length = 35;
				public const int Position = 163;
			}

			public static class City
			{
				public const int Length = 35;
				public const int Position = 198;
			}

			public static class State
			{
				public const int Length = 2;
				public const int Position = 253;
			}

			public static class CountryCode
			{
				public const int Length = 3;
				public const int Position = 264;
			}

			public static class PostCode
			{
				public const int Length = 9;
				public const int Position = 255;
			}

			public static class Phone
			{
				public const int Length = 14;
				public const int Position = 267;
			}

			public static class Fax
			{
				public const int Length = 14;
				public const int Position = 281;
			}

			public static class LeadTrackingNumberForGCCShipment
			{
				public const int Length = 11;
				public const int Position = 295;
			}

			public static class IPT_CNY_CD
			{
				public const int Length = 2;
				public const int Position = 306;
			}

			public static class IPT_POR_CD
			{
				public const int Length = 5;
				public const int Position = 308;
			}

			public static class TotalNumberOfShipmentsForGCCShipment
			{
				public const int Length = 6;
				public const int Position = 313;
			}

			public static class TotalPackageCountForGCCShipment
			{
				public const int Length = 7;
				public const int Position = 319;
			}

			public static class WeightUnitOfQuantityForGCCShipment
			{
				public const int Length = 3;
				public const int Position = 326;
			}

			public static class TotalWeightForGCCShipment
			{
				public const int Length = 14;
				public const int Position = 329;
			}

			public static class Filler
			{
				public const int Length = 35;
				public const int Position = 343;
			}
		}

		#endregion

		public _401000Line(string value)
			: base(value)
		{
		}

		protected override string UnformattedAccountNumber
		{
			get { return Value.SubstringSafe(Constants.Number.Position, Constants.Number.Length).Trim(); }
		}

		public override string Name
		{
			get { return Value.SubstringSafe(Constants.Name.Position, Constants.Name.Length).Trim(); }
		}

		public override string Street1
		{
			get { return Value.SubstringSafe(Constants.Building.Position, Constants.Building.Length).Trim(); }
		}

		public override string Street2
		{
			get { return Value.SubstringSafe(Constants.Street.Position, Constants.Street.Length).Trim(); }
		}

		public override string City
		{
			get { return Value.SubstringSafe(Constants.City.Position, Constants.City.Length).Trim(); }
		}

		public override string Country
		{
			get { return Value.SubstringSafe(Constants.CountryCode.Position, Constants.CountryCode.Length).Trim(); }
		}

		public override string State
		{
			get { return Value.SubstringSafe(Constants.State.Position, Constants.State.Length).Trim(); }
		}

		public override string PostCode
		{
			get { return Value.SubstringSafe(Constants.PostCode.Position, Constants.PostCode.Length).Trim(); }
		}

		protected override string UnformattedPhoneNumber
		{
			get { return Value.SubstringSafe(Constants.Phone.Position, Constants.Phone.Length).Trim(); }
		}

		protected override string UnformattedFaxNumber
		{
			get { return Value.SubstringSafe(Constants.Fax.Position, Constants.Fax.Length).Trim(); }
		}

		public override string ContactName
		{
			get { return Value.SubstringSafe(Constants.ContactName.Position, Constants.ContactName.Length).Trim(); }
		}

		#region GCC Details

		public ZString LeadTrackingNumberForGCCShipment
		{
			get { return Value.SubstringSafe(Constants.LeadTrackingNumberForGCCShipment.Position, Constants.LeadTrackingNumberForGCCShipment.Length).Trim(); }
		}

		public ZInt TotalNumberOfShipmentsForGCCShipment
		{
			get
			{
				string valueAsString = Value.SubstringSafe(Constants.TotalNumberOfShipmentsForGCCShipment.Position, Constants.TotalNumberOfShipmentsForGCCShipment.Length);
				return ToZInt(valueAsString);
			}
		}

		public ZInt TotalPackageCountForGCCShipment
		{
			get
			{
				string valueAsString = Value.SubstringSafe(Constants.TotalPackageCountForGCCShipment.Position, Constants.TotalPackageCountForGCCShipment.Length);
				return ToZInt(valueAsString);
			}
		}

		public string WeightUnitOfQuantityForGCCShipment
		{
			get
			{
				string result = Value.SubstringSafe(Constants.WeightUnitOfQuantityForGCCShipment.Position, Constants.WeightUnitOfQuantityForGCCShipment.Length).Trim();
				return result == "LBS" ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;
			}
		}

		public ZDecimal TotalWeightForGCCShipment
		{
			get
			{
				string valueAsString = Value.SubstringSafe(Constants.TotalWeightForGCCShipment.Position, Constants.TotalWeightForGCCShipment.Length);
				return ToZDecimal(valueAsString);
			}
		}

		#endregion
	}
}
