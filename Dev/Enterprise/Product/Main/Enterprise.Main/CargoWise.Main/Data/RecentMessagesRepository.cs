using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Main.Extensions;
using CargoWise.Main.Navigation;
using Enterprise.MasterFiles.Business;

namespace CargoWise.Main.Data;

public class RecentMessagesRepository() : IRecentMessagesRepository
{
	BusinessObjectFactory _factory;
	BusinessObjectFactory Factory => _factory ??= new BusinessObjectFactory();

	public Task<IEnumerable<RecentMessage>> GetLatestMessagesAsync(int limit) => Task.Run(() => GetLatestMessages(limit));

	public IEnumerable<RecentMessage> GetLatestMessages(int limit)
	{
		if (limit <= 0)
		{
			yield break;
		}

		limit = Math.Min(limit, 100); // Limit to 100 messages

		foreach (var message in GetLatestMessagesToUser(GlbStaff.CurrentUser.PK.ToGuid(), limit))
		{
			if (Factory.Load(message.JobTableCode, message.JobId) is BusinessObject bizo)
			{
				message.JobType = DataBoundResourceStrings.GetStringForTable(bizo.GetType());
				message.JobCode = bizo.HumanReadableItemCode;
			}

			yield return message;
		}
	}

	[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Faster to query DB directly and we there is no need for bizo type reference.")]
	List<RecentMessage> GetLatestMessagesToUser(Guid userId, int limit)
	{
		const string sqlQuery = @"
			select top {0}
				   jcc.JCC_ParentID                                            JobID,
				   jcc.JCC_ParentTableCode                                     JobTableCode,
				   jcc.JCC_PK                                                  JobConversationID,
				   COALESCE(gs_sender.GS_FullName, oc_sender.OC_ContactName)   SenderName,
				   COALESCE(gb.GB_BranchName, oh.OH_FullName)                  SenderCompanyName,
				   CASE WHEN LEN(jcm.JCM_Body) > {1}
				        THEN LEFT(jcm.JCM_Body, {1}) + '...'
				        ELSE jcm.JCM_Body END                                  MessageBody,
				   jcm.JCM_PostedTimeUtc                                       PostedTimeUtc

			  from dbo.JobConversationParticipant jcp
			  join dbo.JobConversation            jcc        on (jcc.JCC_PK = jcp.JCP_JCC_Conversation)
			  join dbo.JobConversationMessage     jcm        on (jcm.JCM_JCC_Conversation = jcc.JCC_PK)
			  join dbo.JobConversationParticipant jcp_sender on (jcp_sender.JCP_PK = jcm.JCM_JCP_Participant)
		 left join dbo.GlbStaff                   gs_sender  on (jcp_sender.JCP_ParticipantTableCode = 'GS' and gs_sender.GS_PK = jcp_sender.JCP_ParticipantID)
		 left join dbo.OrgContact                 oc_sender  on (jcp_sender.JCP_ParticipantTableCode = 'OC' and oc_sender.OC_PK = jcp_sender.JCP_ParticipantID)
		 left join dbo.OrgHeader                  oh         on (oh.OH_PK = oc_sender.OC_OH)
		 left join dbo.GlbBranch                  gb         on (gb.GB_PK = gs_sender.GS_GB_HomeBranch)

			 where 1=1
			   and jcp.JCP_ParticipantID = @UserID
			   and (jcp_sender.JCP_ParticipantTableCode = 'OC' OR jcp_sender.JCP_ParticipantID != jcp.JCP_ParticipantID)
			   and jcc.JCC_ParentTableCode not in ('GCR')
			   and jcm.JCM_IsSystem = 0
			   and jcm.JCM_IsInternal = 0
			   and not exists (
					select top 1 1
					  from dbo.JobConversationMessage     sender_jcm
					  join dbo.JobConversationParticipant sender_jcp on (sender_jcp.JCP_PK = sender_jcm.JCM_JCP_Participant)
					 where 1=1
					   and sender_jcm.JCM_JCC_Conversation = jcm.JCM_JCC_Conversation
					   and sender_jcm.JCM_IsSystem = 0
					   and sender_jcm.JCM_IsInternal = 0
					   and sender_jcm.JCM_PostedTimeUtc > jcm.JCM_PostedTimeUtc
			   )
		  order by jcm.JCM_PostedTimeUtc desc, jcm.JCM_PK
		";

		const int maxMessageLength = 300;
		var sqlText = string.Format(sqlQuery, limit, maxMessageLength);

		using var sqlCmd = ((IDbConnected)Factory).Connection.Command(sqlText);
		sqlCmd.AddParameter("@UserID", SqlDbType.UniqueIdentifier, userId);

		var messages = new List<RecentMessage>();
		using var reader = sqlCmd.ExecuteReader();
		while (reader.Read())
		{
			messages.Add(new RecentMessage
			{
				Id = reader["JobConversationID"] != DBNull.Value ? (Guid)reader["JobConversationID"] : Guid.Empty,
				JobId = reader["JobID"] != DBNull.Value ? (Guid)reader["JobID"] : Guid.Empty,
				JobTableCode = reader["JobTableCode"] != DBNull.Value ? (string)reader["JobTableCode"] : string.Empty,
				SenderName = reader["SenderName"] != DBNull.Value ? (string)reader["SenderName"] : string.Empty,
				SenderCompanyName = reader["SenderCompanyName"] != DBNull.Value ? (string)reader["SenderCompanyName"] : string.Empty,
				Body = reader["MessageBody"] != DBNull.Value ? ((string)reader["MessageBody"]).ReplaceWhitespaces(replacement: " ") : string.Empty,
				PostedTime = reader["PostedTimeUtc"] != DBNull.Value ? (DateTime)reader["PostedTimeUtc"] : DateTime.MinValue,
				PostedTimeAgo = reader["PostedTimeUtc"] != DBNull.Value ? ((DateTime)reader["PostedTimeUtc"]).ToFriendlyTimeAgoString() : string.Empty,
			});
		}

		return messages;
	}
}
