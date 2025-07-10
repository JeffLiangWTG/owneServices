using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class OrganisationAcceptanceLogModuleFilter : ModuleGuidForeignCollectionFilter
	{
		protected OrganisationAcceptanceLogModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			   : base(category, parentCollection)
		{
		}

		public OrganisationAcceptanceLogModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn organisationKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, OrgHeaderSchema.PK, list, parentBusinessObjectType)
		{
			this.organisationKeyColumn = organisationKeyColumn;
		}

		public OrganisationAcceptanceLogModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn organisationKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, OrgHeaderSchema.PK, () => new OrgHeaderCollection(factory), parentBusinessObjectType)
		{
			this.organisationKeyColumn = organisationKeyColumn;
		}

		readonly SchemaGuidColumn organisationKeyColumn;

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var subQueryOrgHeader = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);
			subQueryOrgHeader.AddToFilter(subModuleFilter);

			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
			contactSubQuery.AddSubQuery(organisationKeyColumn, subQueryOrgHeader, JoinCondition.And);

			var subQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiUserAgreementAcceptanceLogSchema.EUL_EUA);
			subQuery.AddSubQuery(contactSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}
	}
}
