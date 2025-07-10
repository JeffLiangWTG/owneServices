using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiCustomerUserAccountFilterBusinessObject : FilterStripBusinessObject
	{
		protected FilterCategory licenceDatabaseCategory = new FilterCategory((NoResString)"Licence Database");

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagsFilter(filters);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("System User ID", EdiCustomerUserAccountSchema.EUA_UserID);
			filters.AddTextFilter("User FullName", EdiCustomerUserAccountSchema.EUA_FullName);
			filters.AddTextFilter("User Email", EdiCustomerUserAccountSchema.EUA_Email);
			filters.AddTextFilter("Contact Relationship Status", EdiCustomerUserAccountSchema.EUA_ContactRelationshipStatus, ContactRelationshipStatus);

			var productFilter = filters.AddTextFilter("Product", GetLicenceDatabaseProduct, ProductTypesList);
			productFilter.Category = licenceDatabaseCategory;
			productFilter.MaxLength = LicenceDatabaseSchema.LD_Product.MaxLength;

			var serverFilter = filters.AddTextFilter("Server Code", GetLicenceDatabaseServerCode);
			serverFilter.Category = licenceDatabaseCategory;
			serverFilter.MaxLength = LicenceDatabaseSchema.LD_ServerCode.MaxLength;

			var systemReferenceIDFilter = filters.AddTextFilter("Tenant ID", GetLicenceDatabaseTenantID);
			systemReferenceIDFilter.Category = licenceDatabaseCategory;
			systemReferenceIDFilter.MaxLength = LicenceDatabaseSchema.LD_TenantID.MaxLength;
		}

		ZQuery GetLicenceDatabaseProduct(SQLComparisonOperator comparison, ZString key)
		{
			return GetLicenceDatabaseBaseQuery(LicenceDatabaseSchema.LD_Product, comparison, key);
		}

		ZQuery GetLicenceDatabaseServerCode(SQLComparisonOperator comparison, ZString key)
		{
			return GetLicenceDatabaseBaseQuery(LicenceDatabaseSchema.LD_ServerCode, comparison, key);
		}

		ZQuery GetLicenceDatabaseTenantID(SQLComparisonOperator comparison, ZString key)
		{
			return GetLicenceDatabaseBaseQuery(LicenceDatabaseSchema.LD_TenantID, comparison, key);
		}

		ZQuery GetLicenceDatabaseBaseQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparison, ZString key)
		{
			var licenceDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licenceDatabaseSubQuery.AddToFilter(JoinCondition.And, schemaColumn, comparison, key);

			var ediCustomerUserAccountQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			ediCustomerUserAccountQuery.AddSubQuery(EdiCustomerUserAccountSchema.EUA_LD, licenceDatabaseSubQuery, JoinCondition.And);

			var query = new ZQuery();
			query.AddToFilter(ediCustomerUserAccountQuery);
			return query;
		}

		#endregion

		#region Flags

		void AddFlagsFilter(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter("User Active", new string[] { "User Active" }, new GetFlagsQuery[] { GetUserIDActive });
		}

		ZQuery GetUserIDActive(ZBool value)
		{
			return new ZQuery(EdiCustomerUserAccountSchema.EUA_IsActive, value);
		}

		#endregion

		#region Lookups

		ContactRelationshipStatusList ContactRelationshipStatus
		{
			get { return contactRelationshipStatus ?? (contactRelationshipStatus = new ContactRelationshipStatusList()); }
		}
		ContactRelationshipStatusList contactRelationshipStatus;

		ProductTypes ProductTypesList
		{
			get { return productTypesList ?? (productTypesList = new ProductTypes(true, true)); }
}
		ProductTypes productTypesList;

		#endregion
	}
}
