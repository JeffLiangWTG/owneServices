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
	public class ContactAcceptanceLogModuleFilter : ModuleGuidForeignCollectionFilter
	{
		public ContactAcceptanceLogModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn contactKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, OrgContactSchema.PK, list, parentBusinessObjectType)
		{
			this.contactKeyColumn = contactKeyColumn;
		}

		public ContactAcceptanceLogModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn contactKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString() + " (Multiple)", moduleID, primaryKeyColumn, OrgContactSchema.PK, () => new OrgContactCollection(factory), parentBusinessObjectType)
		{
			this.contactKeyColumn = contactKeyColumn;
		}

		readonly SchemaGuidColumn contactKeyColumn;

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var subQueryOrgContact = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK, ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);
			subQueryOrgContact.AddToFilter(subModuleFilter);

			var contactSubQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.PK);
			contactSubQuery.AddSubQuery(contactKeyColumn, subQueryOrgContact, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
			query.AddSubQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, contactSubQuery, JoinCondition.And);
			return query;
		}
	}
}
