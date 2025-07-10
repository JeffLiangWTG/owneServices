using System.Collections;
using CargoWise.Application;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common.Shared
{
	public static class CustomsTemplateCopyableFinder
	{
		public static void CopyCountrySpecificData(ICommonShipment originalObject, ICommonShipment clonedObject)
		{
			var provider = GetCustomsTemplateCopyableProvider();
			if (provider != null)
			{
				provider.CloneCountrySpecificData(originalObject, clonedObject);
			}
		}

		internal static Integration.Customs.Shared.ICustomsTemplateCopyableProvider GetCustomsTemplateCopyableProvider()
		{
			return GetCustomsTemplateCopyableProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static Integration.Customs.Shared.ICustomsTemplateCopyableProvider GetCustomsTemplateCopyableProvider(string countryCode)
		{
			Integration.Customs.Shared.ICustomsTemplateCopyableProvider provider = null;

			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				countryCode = Core.Constants.CountryCodes.UnitedStates;
				var allProviders = ObjectFactory.Get<Hashtable>("CustomsTemplateCopyableProviders");
				var providerHandle = allProviders.ContainsKey(countryCode) ? (ObjectHandle)allProviders[countryCode] : null;

				provider = providerHandle != null ? (Integration.Customs.Shared.ICustomsTemplateCopyableProvider)providerHandle.GetObject() : null;
			}

			return provider;
		}
	}
}

