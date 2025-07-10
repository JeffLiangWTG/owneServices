using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentGroupEConversationParticipantsFilter<T> : ModuleGuidForeignCollectionFilter where T : BusinessObject
	{
		public IncidentGroupEConversationParticipantsFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
				: base(description, moduleId, primaryKeyColumn, JobConversationParticipantSchema.JCP_JCC_Conversation, list, parentBusinessObjectType)
		{
			tablePrefix = BusinessObjectFactory.GetTableCodeFromType(typeof(T));
			schema = BusinessObjectFactory.GetTableSchemaFromType(typeof(T));
		}

		public IncidentGroupEConversationParticipantsFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, moduleId, primaryKeyColumn, JobConversationParticipantSchema.JCP_JCC_Conversation, listDelegate, parentBusinessObjectType)
		{
			tablePrefix = BusinessObjectFactory.GetTableCodeFromType(typeof(T));
			schema = BusinessObjectFactory.GetTableSchemaFromType(typeof(T));
		}

		readonly string tablePrefix;

		readonly ITableSchema schema;

		protected override string ParentPkColumnNameForAllMatch => JobConversationSchema.PK.Name;

		protected override string AllMatchSubqueryStatement(ZQuery topLevelQuery)
		{
			return $"AND child.{JobConversationParticipantSchema.JCP_ParticipantTableCode.Name} = '{tablePrefix}' AND child.{JobConversationParticipantSchema.JCP_ParticipantID.Name} IN (SELECT {schema.PK.Name} FROM {schema.TableName} child WHERE ({{0}}))";
		}

		ZDBOnlySubQuery CreateAllMatchSubQuery(FilterStripBusinessObject filterBusinessObject)
		{
			var parameters = new ZSqlParameterCollection();
			var allMatchSql = CreateAllMatchSql(null, filterBusinessObject, parameters);
			var sql = $"{ParentPkColumnNameForAllMatch} = ALL ({allMatchSql})";
			var subQueryConversation = new ZDBOnlySubQuery(typeof(JobConversation), JobConversationSchema.JCC_ParentID);
			subQueryConversation.AddFilterAndZSQLParameterCollection(sql, parameters);
			return subQueryConversation;
		}

		ZDBOnlySubQuery CreateAnyNoneMatchSubQuery(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var subQueryParticipantType = new ZDBOnlySubQuery(typeof(T), schema.PK);
			subQueryParticipantType.AddToFilter(subModuleFilter);

			var subQueryParticipant = new ZDBOnlySubQuery(typeof(JobConversationParticipant), JobConversationParticipantSchema.JCP_JCC_Conversation,
				notIn: ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch);
			subQueryParticipant.AddSubQuery(JobConversationParticipantSchema.JCP_ParticipantID, subQueryParticipantType, JoinCondition.And);
			subQueryParticipant.AddToFilter(JobConversationParticipantSchema.JCP_ParticipantTableCode, tablePrefix);

			var subQueryConversation = new ZDBOnlySubQuery(typeof(JobConversation), JobConversationSchema.JCC_ParentID);
			subQueryConversation.AddSubQuery(subQueryParticipant, JoinCondition.And);
			return subQueryConversation;
		}

		ZDBOnlySubQuery CreateConversationSubQuery(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.AllMatch)
			{
				return CreateAllMatchSubQuery(filterBusinessObject);
			}
			return CreateAnyNoneMatchSubQuery(filterBusinessObject, subModuleFilter);
		}

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.AllMatch && filterBusinessObject.ActiveModuleFilters.Count == 0)
			{
				return new ZQuery();
			}
			var query = GetNewQueryForSelectedFilters();

			var subQueryConversation = CreateConversationSubQuery(filterBusinessObject, subModuleFilter);

			AddJobConversationSubQuery(query, subQueryConversation);

			return query;
		}

		protected virtual void AddJobConversationSubQuery(ZDBOnlyQuery query, ZDBOnlySubQuery subQueryConversation)
		{
			query.AddSubQuery(subQueryConversation, JoinCondition.And);
		}
	}
}
