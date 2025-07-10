using System;
using CargoWise.Application;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public static class GlobalManifestActionMenuFactory
	{
		public static IGlobalManifestActionMenuItemInfo[] GetCountryMenuItem(CargoWise.EntityFramework.BusinessObjectFactory factory)
		{
			switch (MasterFiles.Business.GlbCompany.CurrentCompany.Country.Code)
			{
				case Core.Constants.CountryCodes.Israel:
					{
						return new IGlobalManifestActionMenuItemInfo[]
						{
							ObjectFactory.Get<IL.ILManifestInboundInterchangeImporter>("IL.ILManifestInboundInterchangeImporter", new object[] { factory })
						};
					}
				default:
					return Array.Empty<IGlobalManifestActionMenuItemInfo>();
			}
		}
	}
}
