using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public static class UCC6TemporaryStorageBillControlHelper
	{
		public static UCC6TemporaryStorageAdditionalInfosUserControlWithGrid GetUCC6TemporaryStorageAdditionalInfosUserControlWithGrid()
		{
			return (UCC6TemporaryStorageAdditionalInfosUserControlWithGrid)Activator.CreateInstance(GetUCC6TemporaryStorageAdditionalInfosUserControlWithGridType());
		}

		static Type GetUCC6TemporaryStorageAdditionalInfosUserControlWithGridType()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Ireland:
					return ObjectFactory.GetType<Integration.Customs.IE.IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid>();
				default:
					return ObjectFactory.GetType<Integration.Customs.EU.IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid>();
			}
		}
	}
}
