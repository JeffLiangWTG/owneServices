using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class NationalAdditionalCodeProvider : CusCodeDataWithOrderProvider
	{
		public NationalAdditionalCodeProvider(ZString countryCode) : base(countryCode)
		{
		}

		public static NationalAdditionalCodeProvider GetByNationalAdditionalCodeSupporter(INationalAdditionalCodeSupporter cusCodeDataWithOrderSupporter) => GetByCountryCode(cusCodeDataWithOrderSupporter?.GetCountryCodeForCodeProvider() ?? ZString.Empty);

		public static NationalAdditionalCodeProvider GetByCountryCode(ZString countryCode)
		{
			NationalAdditionalCodeProvider result = null;
			if (!countryCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("NationalAdditionalCodeProviders");
				var objectHandle = (ObjectHandle)types[countryCode.ToString()];
				result = (NationalAdditionalCodeProvider)objectHandle?.GetObject(countryCode) ?? new NationalAdditionalCodeProvider(countryCode);
			}
			return result;
		}
	}
}
