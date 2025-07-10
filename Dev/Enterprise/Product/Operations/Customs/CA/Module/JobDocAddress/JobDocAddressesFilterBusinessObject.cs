using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class JobDocAddressesFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string AddressDescription = "Address Description";
			public const string Organization = "Organization";

			public static MultilingualString AddressDescriptionMultilingualDescription => ResString.GetMultilingualString("CA|JobDocAddressesFilterBusinessObject|AddressDescription", AddressDescription);
			public static MultilingualString OrganisationMultilingualDescription => ResString.GetMultilingualString("CA|JobDocAddressesFilterBusinessObject|Organization", Organization);
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddTextFilter(Schema.AddressDescription, JobDocAddressSchema.E2_AddressType, AddressTypeList)
				.MultilingualDescription = Schema.AddressDescriptionMultilingualDescription;

			filters.AddGuidFilter(Schema.Organization, ModuleIDs.Organisation, GetOrganizationQuery, OrganizationList)
				.MultilingualDescription = Schema.OrganisationMultilingualDescription;

			return filters;
		}

		ZQuery GetOrganizationQuery(ZGuid value)
		{
			ZQuery result = new ZQuery();
			if (value.IsValid)
			{
				ZDBOnlyQuery docAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
				docAddressQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				docAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobDeclarationSchema.Constants.Prefix);

				result.AddToFilter(docAddressQuery);
			}
			return result;
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList AddressTypeList
		{
			get { return Factory.GetCachedValue<DocAddressTypes>(); }
		}

		public OrgHeaderCollection OrganizationList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

	}
}
