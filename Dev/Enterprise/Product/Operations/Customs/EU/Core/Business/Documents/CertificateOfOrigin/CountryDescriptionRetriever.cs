using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class CountryDescriptionRetriever
	{
		public CountryDescriptionRetriever(ZString countryCode, BusinessObjectFactory factory)
		{
			this.countryCode = Argument.NotNull(countryCode, nameof(countryCode));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly ZString countryCode;
		readonly BusinessObjectFactory factory;

		public ZString RetrieveCountryDescription()
		{
			var country = !countryCode.IsEmpty ? factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode) : null;
			return country?.Description ?? ZString.Empty;
		}
	}
}
