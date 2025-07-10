using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class QueueFilterHelper
	{
		public QueueFilterHelper(IQueueFilterBusinessObject filterBizObj)
		{
			this.FilterBizObj = filterBizObj;
		}

		public readonly IQueueFilterBusinessObject FilterBizObj;

		public void AddIsInProcessQueueToFilter(ZQuery query)
		{
			AddSubQueryToProcessQueueToFilter(query, JoinCondition.And, FilterBizObj.QueueNameColumn, SQLComparisonOperator.NotEqual, ZString.Empty);
		}

		public ZQuery GetQueueRemarksFilter(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddSubQueryToProcessQueueToFilter(query, JoinCondition.And, FilterBizObj.QueueRemarksColumn, @operator, value);
			return query;
		}

		public void AddQueueNameAndStatusesToFilter(ZQuery query)
		{
			if (!FilterBizObj.QueueStatus.IsEmpty)
			{
				ZQuery queueNameAndStatusesFilter = FilterBizObj.QueueStatus.GetMultipleCodeFilter(FilterBizObj.QueueNameColumn, FilterBizObj.QueueReasonColumn, FilterBizObj.QueueStatusColumn);
				AddSubQueryToProcessQueueToFilter(query, JoinCondition.And, queueNameAndStatusesFilter);
			}
		}

		#region AddUnworkedToFilter

		public void AddUnworkedToFilter(ZQuery query, ZBool value)
		{
			if (value)
			{
				ZDBOnlySubQuery notInSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, true);
				notInSubQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, User.ServiceUserCode);
				notInSubQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);

				ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(FilterBizObj.QueueParentType);
				dbOnlyQuery.AddSubQuery(notInSubQuery, JoinCondition.And);
				query.AddToFilter(dbOnlyQuery, JoinCondition.And);
			}
		}

		public void AddUnworkedTodayToFilter(ZQuery query, ZBool value)
		{
			if (value)
			{
				ZDBOnlySubQuery notInSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, true);
				notInSubQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, Env.Time.GetUtcFromLocalTime(ZDateTime.Now.Date.ToDateTime()));
				notInSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);
				notInSubQuery.AddToFilter(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, User.ServiceUserCode);

				ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(FilterBizObj.QueueParentType);
				dbOnlyQuery.AddSubQuery(notInSubQuery, JoinCondition.And);
				query.AddToFilter(dbOnlyQuery, JoinCondition.And);
			}
		}

		#endregion

		#region Refund Enquiry filter

		public void AddRefundEnquiryToFilter(ZQuery query, ZBool value)
		{
			if (value)
			{
				ZDBOnlySubQuery subDeclarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusHAWBSchema.CS_JE_CustomsFormalEntry);
				ZDBOnlySubQuery subQueuQuery = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);
				subQueuQuery.AddToFilter(ProcessQueueSchema.P4_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
				subQueuQuery.AddToFilter(ProcessQueueSchema.P4_CustomFlag4, value);
				subDeclarationQuery.AddSubQuery(subQueuQuery, JoinCondition.And);
				ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(FilterBizObj.QueueParentType);
				dbOnlyQuery.AddSubQuery(subDeclarationQuery, JoinCondition.And);
				query.AddToFilter(dbOnlyQuery);
			}
		}

		#endregion

		#region AddSubQueryToProcessQueueToFilter

		public class ProcessQueueSubGroup : ModuleFilterSubGroup
		{
			readonly Type parentType;
			readonly JoinCondition joinCondition;
			readonly bool notIn;

			public ProcessQueueSubGroup(Type parentType, JoinCondition joinCondition, bool notIn = false)
			{
				this.parentType = parentType;
				this.joinCondition = joinCondition;
				this.notIn = notIn;
			}

			public ProcessQueueSubGroup(Type parentType, bool notIn = false)
			{
				this.parentType = parentType;
				this.joinCondition = JoinCondition.And;
				this.notIn = notIn;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(parentType);
				var subQuery = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID, notIn);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, joinCondition);
				return result;
			}
		}

		public void AddSubQueryToProcessQueueToFilter(ZQuery query, JoinCondition joinCondition, SchemaColumn processQueueColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			if (comparisonOperator == SQLComparisonOperator.NotEqual || !value.IsEmpty)
			{
				ZQuery subQueryFilter = new ZQuery();
				subQueryFilter.AddToFilter(processQueueColumn, comparisonOperator, value);
				AddSubQueryToProcessQueueToFilter(query, joinCondition, subQueryFilter);
			}
		}

		public ZQuery GetSubQueryToProcessQueueToFilter(SchemaColumn processQueueColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			if (comparisonOperator == SQLComparisonOperator.NotEqual || !value.IsEmpty)
			{
				ZQuery subQueryFilter = new ZQuery();
				subQueryFilter.AddToFilter(processQueueColumn, comparisonOperator, value);
				return subQueryFilter;
			}
			return new ZQuery();
		}

		public void AddSubQueryToProcessQueueToFilter(ZQuery query, JoinCondition joinCondition, ZQuery subQueryFilter)
		{
			AddSubQueryToProcessQueueToFilter(query, joinCondition, subQueryFilter, false);
		}

		public void AddSubQueryToProcessQueueToFilter(ZQuery query, JoinCondition joinCondition, ZQuery subQueryFilter, bool notIn)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(FilterBizObj.QueueParentType);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID, notIn);

			subQuery.AddToFilter(subQueryFilter);
			dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(dBOnlyQuery, joinCondition);
		}

		#endregion

		#region AddSubQueryToOrganisationQuery

		public void AddOrgAddressSubQueryToOrganisationQuery(ZDBOnlyQuery query, SchemaGuidColumn addressFKColumn, SchemaStringColumn orgAddressColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), addressFKColumn);
			orgAddressSubQuery.AddToFilter(orgAddressColumn, comparisonOperator, value);
			query.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
		}

		public class OrgAddressSubGroup : ModuleFilterSubGroup
		{
			readonly SchemaGuidColumn orgColumn;
			readonly Type type;

			public OrgAddressSubGroup(Type type, SchemaGuidColumn orgColumn)
			{
				this.type = type;
				this.orgColumn = orgColumn;
			}
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(type);
				ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), orgColumn);
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);

				orgAddressSubQuery.AddToFilter(filter);

				orgHeaderSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				result.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				return result;
			}
		}

		public void AddOrgFullNameSubQueryToOrganisationQuery(ZDBOnlyQuery query, SchemaGuidColumn addressFKColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), addressFKColumn);
			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			query.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
		}

		public ZQuery GetOrgFullNameSubQueryToOrganisationQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery orgHeaderSubQuery = new ZQuery();
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
			return orgHeaderSubQuery;
		}

		public class OrgFullNameSubGroup : ModuleFilterSubGroup
		{
			readonly SchemaGuidColumn orgColumn;
			public OrgFullNameSubGroup(SchemaGuidColumn orgColumn)
			{
				this.orgColumn = orgColumn;
			}
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), orgColumn);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
		}

		public void AddOrgCusCodeSubQueryToOrganisationQuery(ZDBOnlyQuery query, SchemaGuidColumn addressFKColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), addressFKColumn);
			var orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber);
			orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, comparisonOperator, value);

			orgAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgCusCodeSubQuery, JoinCondition.And);
			query.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
		}

		public class OrgCusCodeSubGroup : ModuleFilterSubGroup
		{
			readonly SchemaGuidColumn organisationFKColumn;

			public OrgCusCodeSubGroup(SchemaGuidColumn organisationFKColumn)
			{
				this.organisationFKColumn = organisationFKColumn;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
				ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), organisationFKColumn);
				ZDBOnlySubQuery orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);

				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber);
				orgCusCodeSubQuery.AddToFilter(filter);

				orgHeaderSubQuery.AddSubQuery(orgCusCodeSubQuery, JoinCondition.And);

				result.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion
	}
}
