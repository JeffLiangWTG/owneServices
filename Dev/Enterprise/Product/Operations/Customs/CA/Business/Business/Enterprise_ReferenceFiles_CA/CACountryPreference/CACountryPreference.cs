using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	using Enterprise.ZArchitecture.Schema;

	public class CACountryPreference : AutoCACountryPreference
	{
		public CACountryPreference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CACountryPreference Load(BusinessObjectFactory factory, ZString code)
		{
			return factory.LoadFromUniqueKey<CACountryPreference>(CACountryPreferenceSchema.CA_CountryCode, code);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("744B3F8A-4A96-4FBC-825E-AD30FC8940BB", "Country/Region Code: '{0}'", CA_CountryCode); }
		}
	}
}
