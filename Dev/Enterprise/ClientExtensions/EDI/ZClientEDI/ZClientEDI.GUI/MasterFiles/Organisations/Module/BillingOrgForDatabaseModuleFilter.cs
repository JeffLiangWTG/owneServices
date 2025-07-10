using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class BillingOrgForDatabaseModuleFilter : ModuleGuidForeignCollectionFilter
	{
		public BillingOrgForDatabaseModuleFilter(ZString description, IBusinessObjectCollection list)
			: base(description, ClientModuleRegistration.LicenceDatabase, OrgHeaderSchema.PK, LicenceDatabaseSchema.PK, list, typeof(OrgHeader))
		{
		}

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var query = GetNewQueryForSelectedFilters();
			var inOperator = ComparisonOperator == ComparisonConstants.AnyMatch ? "in" : "not in";

			string billingOrgSql = FormattableString.Invariant(
$@"{OrgHeaderSchema.Constants.PK} {inOperator}
(
	select
		OH_PK = coalesce(L9_OH_InvoiceTo, LC_OH)
	from 
		dbo.ClientInvoiceDelivery
		join dbo.LicenceCompany on L9_LC = LC_PK
		join dbo.LicenceHeader on LA_LC = LC_PK
		join dbo.LicenceDatabase on LA_LD = LD_PK
	where 
		L9_IsBilled = 'Y'
		and	LD_IsActive = 1 
		and LA_IsActive = 1
		and L9_ServerCode in (LD_ServerCode, '')
		and {subModuleFilter.ParameterisedText.ParameterisedQueryText}
)");

			query.AddFilterAndZSQLParameterCollection(billingOrgSql, new ZSqlParameterCollection(subModuleFilter.Params), ignoreParameterSuffix: true);
			return query;
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};
	}
}
