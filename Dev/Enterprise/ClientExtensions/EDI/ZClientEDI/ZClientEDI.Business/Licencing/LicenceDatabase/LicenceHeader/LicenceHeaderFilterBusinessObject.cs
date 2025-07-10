using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceHeaderFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, LicenceFilters.GetOrgPKQuery, OrganisationList);
			filters.AddTextFilter("Organisation Name", LicenceFilters.GetOrgNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, LicenceFilters.GetEnterprisePKQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, LicenceFilters.GetEnterprisePKQuery, LicenceEnterpriseList);
			filters.AddGuidFilter("Database Code", ClientModuleRegistration.LicenceDatabase, LicenceHeaderSchema.LA_LD, LicenceDatabaseList);

			return filters;
		}

		#endregion

		#region Lookups

		#region Organisations

		public OrganisationsFindBoxCollection OrganisationList
		{
			get
			{
				if (fOrganisationList == null)
				{
					fOrganisationList = new OrganisationsFindBoxCollection(Factory);
				}

				return fOrganisationList;
			}
		}

		OrganisationsFindBoxCollection fOrganisationList;

		#endregion

		#region Licence Databases

		public LicenceDatabaseNonDependentCollection LicenceDatabaseList
		{
			get
			{
				if (fLicenceDatabaseList == null)
				{
					fLicenceDatabaseList = new LicenceDatabaseNonDependentCollection(Factory);
				}

				return fLicenceDatabaseList;
			}
		}

		LicenceDatabaseNonDependentCollection fLicenceDatabaseList;

		#endregion

		#region Licence Enterprises

		public LicenceEnterpriseCollection LicenceEnterpriseList
		{
			get
			{
				if (fLicenceEnterpriseList == null)
				{
					fLicenceEnterpriseList = new LicenceEnterpriseCollection(Factory);
				}

				return fLicenceEnterpriseList;
			}
		}

		LicenceEnterpriseCollection fLicenceEnterpriseList;

		#endregion

		#endregion
	}
}