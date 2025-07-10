using System;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public static class RegistryItemTagExtensions
	{
		public static bool CheckItemIsVisible(this RegistryItemTag tag, GlbCompanyCollection companies, IRegistryItemVisibility registryItemVisibility = null)
		{
			if (tag.RegistryItem.HasOption(RegistryOptions.IsHidden) ||
				tag.RegistryItem.HasOption(RegistryOptions.IsOnlyForDevelopers) && !GlbStaff.CurrentUser.GS_IsDeveloper ||
				tag.RegistryItem.HasOption(RegistryOptions.IsOnlyForSupport) && !IsSupportUser ||
				tag.RegistryItem.HasOption(RegistryOptions.IsOnlyForController) && !GlbStaff.CurrentUser.GS_IsController ||
				tag.RegistryItem.HasOption(RegistryOptions.IsOnlyForCargoWise) && !Env.IsCargoWiseDomain(tag.GetIPGlobalProperties().DomainName))
			{
				return false;
			}

			return !tag.RegistryItem.CountryFilterPKs.Any() || IsVisibleForAnyCompany(tag, companies, registryItemVisibility);
		}

		static bool IsSupportUser
		{
			get
			{
				if (GlbStaff.CurrentUser.IsSupportUser)
				{
					return true;
				}

				// Everybody is a support user in ediProd.
				// Also the support password is disabled in ZClientEDI, so without this nobody in ediProd can edit the support registry.
				// Skip the EDI check when running tests since the test database also has an enterprise code of EDI.
				return !Globals.IsTest && Env.Registry.RawRegistry.SystemEnterpriseCode.Value == "EDI";
			}
		}

		static bool IsVisibleForAnyCompany(RegistryItemTag tag, GlbCompanyCollection companies, IRegistryItemVisibility registryItemVisibility = null)
		{
			return companies.Cast<GlbCompany>().Any(company => tag.RegistryItem.IsVisible(company.PK.ToGuid(), Guid.Empty, Guid.Empty, registryItemVisibility: registryItemVisibility));
		}
	}
}
