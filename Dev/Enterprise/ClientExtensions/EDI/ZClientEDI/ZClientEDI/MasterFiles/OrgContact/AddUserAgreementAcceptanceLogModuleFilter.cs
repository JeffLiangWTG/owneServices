using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class AddUserAgreementAcceptanceLogModuleFilter : ModuleGuidForeignCollectionFilter
	{
		protected AddUserAgreementAcceptanceLogModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public AddUserAgreementAcceptanceLogModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, EdiUserAgreementAcceptanceLogSchema.PK, list, parentBusinessObjectType)
		{
		}

		public AddUserAgreementAcceptanceLogModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, EdiUserAgreementAcceptanceLogSchema.PK, () => new EdiUserAgreementAcceptanceLogCollection(factory), parentBusinessObjectType)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var subQueryOrgContact = new ZDBOnlySubQuery(typeof(EdiUserAgreementAcceptanceLog), EdiUserAgreementAcceptanceLogSchema.EUL_EUA, ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);
			subQueryOrgContact.AddToFilter(subModuleFilter);

			var contactSubQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
			contactSubQuery.AddSubQuery(EdiCustomerUserAccountSchema.PK, subQueryOrgContact, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(OrgContact));
			query.AddSubQuery(contactSubQuery, JoinCondition.And);
			return query;
		}
	}
}
