using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class UPEOrganisationFilterBusinessObject : OrganisationFilterBusinessObject
	{
		#region Construction

		public UPEOrganisationFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: this(factory, row, OrgModuleType.Standard)
		{
		}

		public UPEOrganisationFilterBusinessObject(BusinessObjectFactory factory, DataRow row, OrgModuleType moduleType)
			: base()
		{
		}

		public UPEOrganisationFilterBusinessObject()
			: base()
		{
		}

		#endregion

		#region Standard Module Overrides

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters = base.GetModuleFiltersCore();

			AddRelationshipGuidsFilters(filters);

			return filters;
		}

		#endregion

		#region ClassifierOrRVCode

		public const string ClassifierOrRVCode = "Classifier or RV";
		public const string ClassifierOrRVDescription = "Classifier or RV";

		protected ZQuery GetClassifierOrRVFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddClassifierStaffAssignmentToFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		#endregion

		#region RelationshipGuidsFilter

		void AddRelationshipGuidsFilters(ModuleFilterCollection filters)
		{
			var classifierStaffAssignmentSubGroup = new ClassifierStaffAssignmentSubGroup();
			var classifierStaffAssignmentFilter = filters.AddGuidFilter(ClassifierOrRVDescription,
										 ModuleIDs.GlbStaff, GlbStaffSchema.PK,
										 GetRelationshipGuidList(ClassifierOrRVCode));
			classifierStaffAssignmentFilter.Category = RelationshipOrgStaff;
			classifierStaffAssignmentFilter.SubGroup = classifierStaffAssignmentSubGroup;
		}

		public override IBusinessObjectCollection GetRelationshipGuidList(string guidRelationshipFilter)
		{
			IBusinessObjectCollection result;
			switch (guidRelationshipFilter)
			{
				case ClassifierOrRVCode:
					result = Staff;
					break;
				default:
					result = base.GetRelationshipGuidList(guidRelationshipFilter);
					break;
			}

			return result;
		}

		#endregion

		public override ReadOnlyCodeDescriptionPairList GetDropEditRelationship_List(string relationshipCode)
		{
			ReadOnlyCodeDescriptionPairList parentList = base.GetDropEditRelationship_List(relationshipCode);
			var collection = parentList as CodeDescriptionPairList;
			if (relationshipCode == OrgConstants.FilterControl.DropEditRelationships.Description.CustomsCodeType &&
				collection != null &&
				!collection.ContainsCode(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber))
			{
				collection.AddPair(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumberDescription);
			}
			return collection ?? parentList;
		}

		#region AddClassifierStaffAssignmentToFilter

		public class ClassifierStaffAssignmentSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery consigneeStaffAssignmentSubQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);

				ZDBOnlySubQuery staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
				staffSubQuery.AddToFilter(filter);

				consigneeStaffAssignmentSubQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staffSubQuery, JoinCondition.And);

				ZQuery classifierOrRVFilter = new ZQuery();
				classifierOrRVFilter.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, SQLComparisonOperator.Equal, UPEStaffRoles.Codes.Classifier);
				classifierOrRVFilter.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_Role, SQLComparisonOperator.Equal, UPEStaffRoles.Codes.RV);
				consigneeStaffAssignmentSubQuery.AddToFilter(classifierOrRVFilter, JoinCondition.And);

				dBOnlyQuery.AddSubQuery(consigneeStaffAssignmentSubQuery, JoinCondition.And);
				return dBOnlyQuery;
			}
		}

		void AddClassifierStaffAssignmentToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery consigneeStaffAssignmentSubQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);

			ZDBOnlySubQuery staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffSubQuery.AddToFilter(GlbStaffSchema.PK, @operator, value);

			consigneeStaffAssignmentSubQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staffSubQuery, JoinCondition.And);

			ZQuery classifierOrRVFilter = new ZQuery();
			classifierOrRVFilter.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, SQLComparisonOperator.Equal, UPEStaffRoles.Codes.Classifier);
			classifierOrRVFilter.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_Role, SQLComparisonOperator.Equal, UPEStaffRoles.Codes.RV);
			consigneeStaffAssignmentSubQuery.AddToFilter(classifierOrRVFilter, JoinCondition.And);

			dBOnlyQuery.AddSubQuery(consigneeStaffAssignmentSubQuery, JoinCondition.And);
			query.AddToFilter(dBOnlyQuery, JoinCondition.And);
		}

		#endregion
	}
}
