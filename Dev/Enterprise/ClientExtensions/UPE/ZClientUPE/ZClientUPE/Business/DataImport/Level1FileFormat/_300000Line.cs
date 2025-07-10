


namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _300000Line : _OrganisationLine
	{
		#region Constants

		public new class Constants : RecordLine.Constants
		{
			public static class RegionDistrict
			{
				public const int Length = 4;
				public const int Position = 50;
			}

			public static class PickupCentre
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

			public static class Building
			{
				public const int Length = 35;
				public const int Position = 103;
			}

			public static class Street
			{
				public const int Length = 35;
				public const int Position = 138;
			}

			public static class City
			{
				public const int Length = 35;
				public const int Position = 173;
			}

			public static class State
			{
				public const int Length = 2;
				public const int Position = 228;
			}

			public static class PostCode
			{
				public const int Length = 9;
				public const int Position = 230;
			}

			public static class Country
			{
				public const int Length = 3;
				public const int Position = 239;
			}

			public static class Phone
			{
				public const int Length = 14;
				public const int Position = 242;
			}

			public static class Fax
			{
				public const int Length = 14;
				public const int Position = 256;
			}

			public static class _3rdPartyAccountNumber
			{
				public const int Length = 10;
				public const int Position = 270;
			}

			public static class _3rdPartyName
			{
				public const int Length = 35;
				public const int Position = 280;
			}

			public static class _3rdPartyCountry
			{
				public const int Length = 2;
				public const int Position = 315;
			}

			public static class CustomsEINNumber
			{
				public const int Length = 15;
				public const int Position = 317;
			}

			public static class ReferenceNumber1
			{
				public const int Length = 35;
				public const int Position = 332;
			}

			public static class ContactName
			{
				public const int Length = 11;
				public const int Position = 367;
			}
		}

		#endregion

		public _300000Line(string value)
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
			get { return Value.SubstringSafe(Constants.Country.Position, Constants.Country.Length).Trim(); }
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

		public string ReferenceNumber1
		{
			get { return Value.SubstringSafe(Constants.ReferenceNumber1.Position, Constants.ReferenceNumber1.Length).Trim(); }
		}
	}
}
