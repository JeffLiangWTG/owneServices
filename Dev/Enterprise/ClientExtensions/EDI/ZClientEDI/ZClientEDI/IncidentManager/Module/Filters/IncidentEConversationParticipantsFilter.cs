using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentEConversationParticipantsFilter<T> : IncidentGroupEConversationParticipantsFilter<T> where T : BusinessObject
	{
		public IncidentEConversationParticipantsFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, moduleId, primaryKeyColumn, list, parentBusinessObjectType)
		{
		}

		protected override void AddJobConversationSubQuery(ZDBOnlyQuery query, ZDBOnlySubQuery subQueryConversation)
		{
			query.AddSubQuery(IncidentMainSchema.IM_INC_Request, subQueryConversation, JoinCondition.And);
		}
	}
}
