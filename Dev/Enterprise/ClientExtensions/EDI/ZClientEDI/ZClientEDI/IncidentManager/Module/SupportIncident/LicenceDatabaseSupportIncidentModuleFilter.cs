using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class LicenceDatabaseSupportIncidentModuleFilter : ModuleGuidForeignCollectionFilter
	{
		public LicenceDatabaseSupportIncidentModuleFilter(ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(description, moduleID, primaryKeyColumn, LicenceDatabaseSchema.PK, () => new LicenceDatabaseNonDependentCollection(factory), parentBusinessObjectType)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch,
			ModuleTextFilter.ComparisonConstants.IsBlank,
			ModuleTextFilter.ComparisonConstants.IsNotBlank,
			ModuleTextFilter.ComparisonConstants.Exact,
			ModuleTextFilter.ComparisonConstants.NotEqual
		};

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = base.GetQueryUsingFilterColumns();
			if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact || ComparisonOperator == ModuleTextFilter.ComparisonConstants.NotEqual)
			{
				if (Property == ZGuid.Empty)
				{
					query = new ZQuery();
				}
			}
			else if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsBlank || ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsNotBlank)
			{
				query = new ZQuery(IncidentMainSchema.IM_LD, SqlComparisonOperator, DBNull.Value, ComparisonOptions);
			}
			return query;
		}

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var subQueryLicenceDatabase = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK, ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);
			subQueryLicenceDatabase.AddToFilter(subModuleFilter);

			return BuildQuery(subQueryLicenceDatabase);
		}

		ZQuery BuildQuery(ZDBOnlySubQuery subModuleFilter)
		{
			var queryIncident = new ZDBOnlyQuery(typeof(SupportIncident));
			queryIncident.AddSubQuery(IncidentMainSchema.IM_LD, subModuleFilter, JoinCondition.And);
			return queryIncident;
		}
	}
}
