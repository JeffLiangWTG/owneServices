using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class MetaSupportIncidentProvider : IncidentAssociationQuery, IMetaSupportIncidentProvider
	{
		public MetaSupportIncidentProvider() : this(Db.Connection) { }
		public MetaSupportIncidentProvider(DbConnection dbConnection) : base(dbConnection) { }
		public IEnumerable<IMetaSupportIncident> AllBetween(DateTime fromDate, DateTime toDate)
		{
			return FromGuids(GetIncidentGuidsBetween(fromDate, toDate));
		}

		public IMetaSupportIncident FromGuid(Guid guid)
		{
			return FromGuids(new List<Guid>() { guid }).FirstOrDefault();
		}

		public IEnumerable<IMetaSupportIncident> FromGuids(IEnumerable<Guid> guids)
		{
			return GetSupportIncidents(guids)
				.Select(tuple =>
					new MetaSupportIncident(tuple.SupportIncident, tuple.WorkItems, tuple.ConversationItems));
		}

		IEnumerable<(Dictionary<string, object> SupportIncident, IEnumerable<MetaWorkItem> WorkItems, IEnumerable<MetaConversationItem> ConversationItems)> GetSupportIncidents(IEnumerable<Guid> guids)
		{
			var guidList = guids.ToList();
			var incidents = GetIncidents(guidList);
			var allWorkItems = GetWorkItems(guidList);

			var workItemsByPivot1 = allWorkItems
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.GroupBy(wi => wi[AutoGenPivot.Schema.XX_Relation1ID].ToString())
				.ToDictionary(g => g.Key, g => g.ToList());
			var workItemsByPivot2 = allWorkItems
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.GroupBy(wi => wi[AutoGenPivot.Schema.XX_Relation2ID].ToString())
				.ToDictionary(g => g.Key, g => g.ToList());

			var conversationItemsByIncidentId = GetConversationItems(guidList)
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.GroupBy(ci => ci[AutoIncidentMain.Schema.PK].ToString())
				.ToDictionary(g => g.Key, g => g.ToList());

			foreach (var incident in incidents)
			{
				var incidentGuid = incident[AutoIncidentMain.Schema.PK].ToString();

				var workItemsBy1 = workItemsByPivot1.ContainsKey(incidentGuid)
					? workItemsByPivot1[incidentGuid].Select(x => new MetaWorkItem(x))
					: Enumerable.Empty<MetaWorkItem>();

				var workItemsBy2 = workItemsByPivot2.ContainsKey(incidentGuid)
					? workItemsByPivot2[incidentGuid].Select(x => new MetaWorkItem(x))
					: Enumerable.Empty<MetaWorkItem>();

				var metaConversationItems = conversationItemsByIncidentId.ContainsKey(incidentGuid)
					? conversationItemsByIncidentId[incidentGuid].Select(x => new MetaConversationItem(x))
					: Enumerable.Empty<MetaConversationItem>();

				yield return (
					incident,
					workItemsBy1.Concat(workItemsBy2)
						.OrderBy(wi => wi.SystemCreateTimeUtc)
						.ToList(),
					metaConversationItems
						.OrderBy(ci => ci.PostedTimeUtc)
						.ToList()
				);
			}
		}

		List<Dictionary<string, object>> GetIncidents(IEnumerable<Guid> guids)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@pkList", guids.ToList(), IncidentMainSchema.PK, true)
			};

			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT IM.{0}, IM.{1}, IM.{2}, IM.{3}, IM.{4}, IM.{5}, IM.{6}, IM.{7}, IM.{8}, IM.{9}, ISNULL(ST.{10}, '') AS ST_Description "
				+ "FROM {11} IM LEFT JOIN dbo.StmNote ST ON ({12} = {13} AND {14} = 'Incident Detail') "
				+ "WHERE IM.{15} IN (SELECT Value FROM @pkList) ORDER BY IM.{16}",
				AutoIncidentMain.Schema.PK,  // 0
				AutoIncidentMain.Schema.IM_IncidentNumber, // 1
				AutoIncidentMain.Schema.IM_SystemCreateTimeUtc, // 2
				AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc, // 3
				AutoIncidentMain.Schema.IM_Product, // 4
				AutoIncidentMain.Schema.IM_ProgramArea, // 5
				AutoIncidentMain.Schema.IM_Module, // 6
				AutoIncidentMain.Schema.IM_Priority, // 7
				AutoIncidentMain.Schema.IM_RN_NKCountry, // 8
				AutoIncidentMain.Schema.IM_Description, // 9
				AutoStmNote.Schema.ST_NoteText, // 10
				AutoIncidentMain.Schema.TableName, // 11
				AutoStmNote.Schema.ST_ParentID, // 12
				AutoIncidentMain.Schema.PK, // 13
				AutoStmNote.Schema.ST_Description, // 14
				AutoIncidentMain.Schema.PK, // 15
				AutoIncidentMain.Schema.IM_SystemCreateTimeUtc // 16
			);

			return QueryObjects(sql, parameters).ToList();
		}

		List<Dictionary<string, object>> GetConversationItems(IEnumerable<Guid> guids)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@pkList", guids.ToList(), IncidentMainSchema.PK, true)
			};

			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT IM.{0}, JCM.{1}, JCM.{2} FROM {3} IM JOIN {4} INC ON ({5} = {6}) JOIN {7} JCC ON ({8} = {9}) "
				+ "JOIN {10} JCM ON ({11} = {12} AND {13} = 0) WHERE {14} IN (SELECT Value FROM @pkList)",
				AutoIncidentMain.Schema.PK,
				AutoJobConversationMessage.Schema.JCM_Body,
				AutoJobConversationMessage.Schema.JCM_PostedTimeUtc,
				AutoIncidentMain.Schema.TableName,
				AutoIncidentRequest.Schema.TableName,
				AutoIncidentRequest.Schema.PK,
				AutoIncidentMain.Schema.IM_INC_Request,
				AutoJobConversation.Schema.TableName,
				AutoJobConversation.Schema.JCC_ParentID,
				AutoIncidentRequest.Schema.PK,
				AutoJobConversationMessage.Schema.TableName,
				AutoJobConversationMessage.Schema.JCM_JCC_Conversation,
				AutoJobConversation.Schema.PK,
				AutoJobConversationMessage.Schema.JCM_IsSystem,
				AutoIncidentMain.Schema.PK);

			return QueryObjects(sql, parameters).ToList();
		}

		List<Dictionary<string, object>> GetWorkItems(IEnumerable<Guid> guids)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@pkList", guids.ToList(), IncidentMainSchema.PK, true)
			};
			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0}, {1}, {2}, dbo.ClrUncompressRTFAsPlainText({3}) AS PlainText{4},{5},{6},{7} "
				+ "FROM dbo.{8} JOIN {9} xx ON ({10} = {11} OR {12} = {13}) WHERE {14} IN (SELECT Value FROM @pkList) OR {15} IN (SELECT Value FROM @pkList)",
				AutoWorkItem.Schema.PK,
				AutoWorkItem.Schema.WKI_WorkItemNumber,
				AutoWorkItem.Schema.WKI_Summary,
				AutoWorkItem.Schema.WKI_Details,
				AutoWorkItem.Schema.WKI_Details,
				AutoWorkItem.Schema.WKI_SystemCreateTimeUtc,
				AutoGenPivot.Schema.XX_Relation1ID,
				AutoGenPivot.Schema.XX_Relation2ID,
				AutoWorkItem.Schema.TableName,
				AutoGenPivot.Schema.TableName,
				AutoGenPivot.Schema.XX_Relation1ID,
				AutoWorkItem.Schema.PK,
				AutoGenPivot.Schema.XX_Relation2ID,
				AutoWorkItem.Schema.PK,
				AutoGenPivot.Schema.XX_Relation1ID,
				AutoGenPivot.Schema.XX_Relation2ID);

			return QueryObjects(sql, parameters).ToList();
		}
	}
}
