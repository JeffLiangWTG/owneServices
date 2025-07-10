using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IAddress
	{
		ZString Line { get; }
		ZString CityName { get; }
		ZString CountryCode { get; }
		ZString PostcodeID { get; }
	}

	public class AddressWrapper : IAddress
	{
		AddressWrapper(ZString line, ZString cityName, ZString countryCode, ZString postcodeId, int lineLength = 35)
		{
			this.line = line;
			this.cityName = cityName;
			this.countryCode = countryCode;
			this.postcodeId = postcodeId;
			this.lineLength = lineLength;
		}

		public static AddressWrapper New(ZString line, ZString cityName, ZString countryCode, ZString postcodeId, int lineLength = 35)
		{
			return new AddressWrapper(line, cityName, countryCode, postcodeId, lineLength);
		}

		public static AddressWrapper New(OrgAddress address, int lineLength = 35)
		{
			return new AddressWrapper(GetAddressLine(address.Address1, address.Address2), address.City, address.Country?.Code ?? string.Empty, address.Postcode, lineLength);
		}

		public static ZString GetAddressLine(ZString addressLine1, ZString addressLine2)
		{
			return addressLine2.IsEmpty ? addressLine1 : new ZString(addressLine1 + " " + addressLine2).SubstringSafe(0, 35);
		}

		ZString IAddress.Line => line.StripNewlineCharacters(lineLength);

		ZString IAddress.CityName => cityName.StripNewlineCharacters(35);

		ZString IAddress.CountryCode => countryCode.StripNewlineCharacters(2);

		ZString IAddress.PostcodeID
		{
			get
			{
				var result = postcodeId.StripNewlineCharacters(9);
				if ((!line.IsEmpty || !cityName.IsEmpty || !countryCode.IsEmpty) && postcodeId.IsEmpty)
				{
					result = "NA";
				}
				return result;
			}
		}

		readonly ZString line;
		readonly ZString cityName;
		readonly ZString countryCode;
		readonly ZString postcodeId;
		readonly int lineLength;
	}
}
