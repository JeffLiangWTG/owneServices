using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public static class ValidationConstants
	{
		public static string FlightNumberDoesNotStartWithAValidIATAAirCode
		{
			get { return ResString.GetMultilingualString("{D6E868F3-0B7F-43E7-AC65-3936A500FD2D}", "Flight Number must start with a valid 2-letter IATA Airline code."); }
		}

		public static string UNLOCODoesNotHaveAnIATACode(string code)
		{
			return ResString.GetMultilingualString("{49A3EFEF-8659-40A8-B00C-C04CDC36DFA0}", "UNLOCO '{0}' does not have an IATA code setup.", code);
		}

		public static string ManifestNumberIsRequired(string label)
		{
			return ResString.GetMultilingualString("{0BD0D82C-3426-4B7B-8BF8-A9ADBFAAD9AD}", "{0} is required.", label);
		}

		public static string ManifestMustGoThruSupportedCountries
		{
			get { return ResString.GetMultilingualString("{2A663E92-EB25-4D07-AF27-43DCE881AD6D}", "The manifest has to load from or discharge in one of the supported countries or tranship through one."); }
		}

		public static string CarrierRequiresCCC(string countryCode)
		{
			return ResString.GetMultilingualString("{D506ED15-6491-49A0-825A-61C35C54C1C9}", "Carrier requires CCC code for {0}.", countryCode);
		}

		public static string RequiresCustomsCode(ZString code, ZString countryCode)
		{
			return ResString.GetMultilingualString("{818F8B0C-6D6E-4ED2-A781-84745896D389}", "Requires a Customs Code of type '{0}' for country {1}.", code, countryCode);
		}

		public static string PartyRequiresCustomsCode(ZString party, ZString customsCodes)
		{
			return ResString.GetMultilingualString("{C916CDCA-D49A-4785-A6E6-44A8669DAE4B}", "The party '{0}' requires the following Custom Code setup in Organization > Config > Customs Codes:\r\n{1}", party, customsCodes);
		}

		public static ZString FieldIsMandatory(ZString description, ZString countryCode)
		{
			return ResString.GetMultilingualString("{25E68272-6BCF-4247-B2CA-D3C71FE8B343}", "{0} for {1}.", description, countryCode);
		}

		public static ZString FieldIsMandatory(ZString description)
		{
			return ResString.GetMultilingualString("{44E5EFDA-344C-47DD-BF21-1C0B7E2DF39F}", "{0}", description);
		}

		public static string MissingCustomsOffice(string customsOffice)
		{
			return ResString.GetMultilingualString("{E99A6A29-1761-49D0-9363-D318A0B58C2A}", "The Customs Office in {0} is a critical field and must not be empty before creating a message. Please supply a value.", customsOffice);
		}

		public static string MissingEmailAddress
		{
			get { return ResString.GetMultilingualString("{887CBB97-72BC-45AB-B885-95B4D5ABA28D}", "Your staff profile requires an email address as this is needed for messaging."); }
		}

		public static string MustHaveManifestType
		{
			get { return ResString.GetMultilingualString("{371D6F40-5C6B-4677-BCBB-1E76DDF6ECE8}", "The Manifest Type field is a critical field and its validation errors must be addressed before you may send."); }
		}

		public static string BillIsRequiredForManifestType(string label, string manifestType)
		{
			return ResString.GetMultilingualString("{3341A50C-BD35-44C9-8D2A-E8B2E55FDA1D}", "A Bill matching {0} is required when Manifest Type is {1}.", label, manifestType);
		}

		public static string OnlyOneBillIsAllowed(string manifestType)
		{
			return ResString.GetMultilingualString("{247888D7-4209-4B03-A170-398235BCF6BC}", "Only one Bill is allowed when Manifest Type is {0}.", manifestType);
		}

		public static string ManifestNumberShouldEuqalToBillNumber(string label)
		{
			return ResString.GetMultilingualString("{1C79A112-31B3-4FF7-B74A-1B2A2C3FDDAE}", "{0} should equal to Bill Number.", label);
		}

		public static string InvalidPort(string unlocoPort)
			=> ResString.GetMultilingualString("317BCAEE-D55C-452F-8BCE-78634801D7E9", "The code entered is expected to match a \"Local Code\" on the {0} UNLOCO record.", unlocoPort);
	}
}
