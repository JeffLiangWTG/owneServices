using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IdentityApplication
{
	public class EdiIdentityApplicationFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);
			AddLicenseFilter(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Application Name", EdiIdentityApplicationSchema.IDA_ApplicationName);
			filters.AddTextFilter("Client Id", EdiIdentityApplicationSchema.IDA_ClientID);
			filters.AddTextFilter("Application Module", EdiIdentityApplicationSchema.IDA_ApplicationModule);
			filters.AddTextFilter("Tenant ID", GetTenantQuery);
			filters.AddTextFilter("Product", GetProductQuery, new EdiIdentityApplicationLookups(null).ProductTypeList);
			filters.AddTextFilter("Application Type", GetApplicationTypeQuery, new EdiIdentityApplicationLookups(null).DatabaseTypesList);
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var redirectUrlStatusListFilter = filters.AddTextFilter("Redirect URL Status", GetRedirectUrlStatusQuery, RedirectUrlStatusList);
			redirectUrlStatusListFilter.Category = FilterCategories.StatusAndFlags;
			redirectUrlStatusListFilter.MultilingualDescription = ResString.GetMultilingualString("3FA35E4B-3C6D-4B99-8AD2-B9664CBC0791", "Redirect URL Status");

			var rolledBackFilter = filters.AddTextFilter("Roll-back Status", GetRollBackStatusQuery, RollBackStatusList);
			rolledBackFilter.Category = FilterCategories.StatusAndFlags;
			rolledBackFilter.MultilingualDescription = ResString.GetMultilingualString("DB135355-B916-4E8C-947B-7FA923C1B52D", "Roll-back Status");
		}
		void AddLicenseFilter(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("License Database", ClientModuleRegistration.LicenceDatabase, EdiIdentityApplicationSchema.IDA_LD, LicenceDatabaseList);
		}

		internal static string StatusRolledBack => Res.GetString("E17045A2-6FED-4276-94D3-7C934B32F029", "Rolled Back");
		internal static string StatusNonRolledBack => Res.GetString("30BA73DC-6D91-403B-A154-E0193BF037BC", "Not Rolled Back");
		internal static string ShowAllDescription => Res.GetString("F57B4464-E532-4396-A93E-25A03B9E2369", "Show all records");

		CodeDescriptionPairList RedirectUrlStatusList
		{
			get
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddPair(StatusAll, ShowAllDescription);

				foreach (CodeDescriptionPair item in new EdiIdentityApplicationRedirectUrlStatus())
				{
					codeDescriptionPairList.Add(item);
				}
				return codeDescriptionPairList;
			}
		}

		CodeDescriptionPairList RollBackStatusList
		{
			get
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddPair(StatusAll, ShowAllDescription);
				codeDescriptionPairList.AddPair(StatusRolledBack, Res.GetString("52830A7F-53AA-4437-816F-4701D1320472", "Rolled Back Only"));
				codeDescriptionPairList.AddPair(StatusNonRolledBack, Res.GetString("BA498F30-3A8E-4244-8F93-89357E9B315B", "Non-Rolled Back Only"));
				return codeDescriptionPairList;
			}
		}

		ZQuery GetRedirectUrlStatusQuery(ZString value)
		{
			if (StatusAll.EqualsUnresolvedOrLocalized(value, ignoreCase: false))
			{
				return new ZQuery();
			}

			return new ZQuery(EdiIdentityApplicationSchema.IDA_RedirectUrlStatus, value);
		}

		ZQuery GetRollBackStatusQuery(ZString value)
		{
			if (StatusAll.EqualsUnresolvedOrLocalized(value, ignoreCase: false))
			{
				return new ZQuery();
			}

			return new ZQuery(EdiIdentityApplicationSchema.IDA_IsRollback, value == StatusRolledBack);
		}

		ZQuery GetTenantQuery(SQLComparisonOperator comparisonOperator, ZString key)
		{
			var ediIdentityTenantSubQuery = new ZDBOnlySubQuery(typeof(EdiIdentityTenant), EdiIdentityTenantSchema.PK);
			ediIdentityTenantSubQuery.AddToFilter(JoinCondition.And, EdiIdentityTenantSchema.IDT_TenantId, comparisonOperator, key);

			var ediIdentityApplicationQuery = new ZDBOnlyQuery(typeof(EdiIdentityApplication));
			ediIdentityApplicationQuery.AddSubQuery(EdiIdentityApplicationSchema.IDA_IDT, ediIdentityTenantSubQuery, JoinCondition.And);

			return ediIdentityApplicationQuery;
		}

		ZQuery GetProductQuery(SQLComparisonOperator comparisonOperator, ZString key)
		{
			var licenceProductSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licenceProductSubQuery.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_Product, comparisonOperator, key);

			var ediIdentityApplicationQuery = new ZDBOnlyQuery(typeof(EdiIdentityApplication));
			ediIdentityApplicationQuery.AddSubQuery(EdiIdentityApplicationSchema.IDA_LD, licenceProductSubQuery, JoinCondition.And);

			var applicationProductQuery = new ZQuery(EdiIdentityApplicationSchema.IDA_Product, comparisonOperator, key);
			applicationProductQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_LD, SQLComparisonOperator.Equal, DBNull.Value);

			ediIdentityApplicationQuery.AddToFilter(applicationProductQuery, JoinCondition.Or);
			return ediIdentityApplicationQuery;
		}

		ZQuery GetApplicationTypeQuery(SQLComparisonOperator comparisonOperator, ZString key)
		{
			var licenceTypeSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licenceTypeSubQuery.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_LicenceType, comparisonOperator, key);

			var ediIdentityApplicationQuery = new ZDBOnlyQuery(typeof(EdiIdentityApplication));
			ediIdentityApplicationQuery.AddSubQuery(EdiIdentityApplicationSchema.IDA_LD, licenceTypeSubQuery, JoinCondition.And);

			var applicationTypeQuery = new ZQuery(EdiIdentityApplicationSchema.IDA_ApplicationType, comparisonOperator, key);
			applicationTypeQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_LD, SQLComparisonOperator.Equal, DBNull.Value);

			ediIdentityApplicationQuery.AddToFilter(applicationTypeQuery, JoinCondition.Or);
			return ediIdentityApplicationQuery;
		}

		LicenceDatabaseNonDependentCollection LicenceDatabaseList  => licenceDatabaseList ??= new LicenceDatabaseNonDependentCollection(Factory);
		LicenceDatabaseNonDependentCollection licenceDatabaseList;
	}
}
