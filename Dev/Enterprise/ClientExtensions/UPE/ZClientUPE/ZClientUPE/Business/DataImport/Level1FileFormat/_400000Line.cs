


namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _400000Line : _OrganisationLine
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

			public static class PostCode
			{
				public const int Length = 9;
				public const int Position = 255;
			}

			public static class CountryCode
			{
				public const int Length = 3;
				public const int Position = 264;
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

			public static class PO
			{
				public const int Length = 12;
				public const int Position = 295;
			}

			public static class ReferenceNumber2
			{
				public const int Length = 35;
				public const int Position = 307;
			}

			public static class IRSNumber
			{
				public const int Length = 10;
				public const int Position = 346;
			}

			public static class PONumberSuffix
			{
				public const int Length = 8;
				public const int Position = 356;
			}
		}

		#endregion

		public _400000Line(string value)
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

		public string ReferenceNumber2
		{
			get { return Value.SubstringSafe(Constants.ReferenceNumber2.Position, Constants.ReferenceNumber2.Length).Trim(); }
		}
	}
}
