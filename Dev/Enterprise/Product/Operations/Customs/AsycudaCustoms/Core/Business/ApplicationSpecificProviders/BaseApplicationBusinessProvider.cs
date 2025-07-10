using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public abstract class BaseApplicationBusinessProvider
	{
		public static BaseApplicationBusinessProvider GetApplicationBusinessProvider(BusinessObjectFactory factory, ZString countryCode)
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "ApplicationBusinessProviders_{0}", countryCode); // CachedValueKey
			return factory.GetCachedValue(cacheKey, () =>
			{
				string providerKey;
				if (countryCode.IsBLNSCountry(factory))
				{
					providerKey = Constants.AsycudaCustomsApplicationProviderKey.BLNS;
				}
				else
				{
					providerKey = Constants.AsycudaCustomsApplicationProviderKey.Default;
				}
				var providers = ObjectFactory.Get<Hashtable>("AsycudaCustomsApplicationBusinessProvider");
				var objectHandle = (ObjectHandle)providers?[providerKey];
				return (BaseApplicationBusinessProvider)objectHandle?.GetObject();
			});
		}

		public ZString UniversalTariffType => GetUniversalTariffTypeCore();
		protected abstract ZString GetUniversalTariffTypeCore();
		public abstract ZString UniversalRefDataSource { get; }

		public bool IsReciprocalRates(JobDeclaration declaration)
		{
			var company = declaration?.Company ?? GlbCompany.CurrentCompany;
			var isReciprocalExchangeRate = ZZRefCusConfiguration.Get(company)?.ZZC_IsReciprocalExchangeRate ?? ZString.Empty;
			switch (isReciprocalExchangeRate)
			{
				case IsReciprocalExchangeRateList.Codes.Yes:
					return true;
				case IsReciprocalExchangeRateList.Codes.No:
					return false;
				default:
					return IsReciprocalRatesCore(company);
			}
		}

		protected virtual bool IsReciprocalRatesCore(GlbCompany company) => company.GC_IsReciprocal;
	}
}
