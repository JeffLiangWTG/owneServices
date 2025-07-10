using System.Collections;
using CargoWise.Application;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class LandedCostOnlyConfigurationProviderEU : ILandedCostOnlyConfigurationProvider
	{
		public ILandedCostOnlyConfiguration GetLandedCostOnlyConfiguration(BaseJobDeclaration declaration)
		{
			ILandedCostOnlyConfiguration result = null;
			var countryCode = declaration?.CountryCode;
			var types = (Hashtable)ObjectFactory.Get("LandedCostingOnlyConfigurations");

			var objectHandle = (ObjectHandle)types[countryCode.ToString()];
			result = (ILandedCostOnlyConfiguration)objectHandle?.GetObject();

			if (result == null)
			{
				objectHandle = (ObjectHandle)types["EU"];
				result = (ILandedCostOnlyConfiguration)objectHandle?.GetObject();
			}

			return result;
		}
	}
}
