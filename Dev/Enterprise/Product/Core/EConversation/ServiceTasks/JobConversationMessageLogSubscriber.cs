using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.EConversation.ServiceTasks
{
	[Serializable]
	public class JobConversationMessageLogSubscriber : LogSubscriber
	{
		#region LogSubscriber Overrides

		public override string Name => "JobConversationMessageLogSubscriber";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For log purposes")]
		public override string FriendlyName => "Job Conversation Message Log Subscriber";

		public override string[] TableNames => new string[] { JobConversationMessageSchema.Constants.TableName };

		public override string[] EventTypes => new string[] { Events.AddedARecordToTheSystem.Code };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs.Length > 0)
			{
				var factory = queuedLogs.First().Factory;
				var messages = factory.Load<JobConversationMessage>(new ZQuery(JobConversationMessageSchema.PK, queuedLogs.Select(log => log.SJ_ParentID)));

				ImproveDBPerformance(factory, messages);

				foreach (var grouping in messages.Where(message => message.Conversation.JCC_ParentTableCode != IncidentRequestSchema.Constants.Prefix).GroupBy(m => (m.Conversation, m.Sender)).OrderBy(group => group.Max(m => m.JCM_PostedTimeUtc)))
				{
					EConversationEmailBuilder.GenerateAndQueueEmailNotifications(grouping.Key.Conversation, grouping.Key.Sender, grouping);
				}
			}
		}

		#endregion

		static void ImproveDBPerformance(BusinessObjectFactory factory, IEnumerable<JobConversationMessage> messages)
		{
			var conversations = factory.Load<JobConversation>(new ZQuery(JobConversationSchema.PK, messages.Select(message => message.JCM_JCC_Conversation)));
			factory.Load<JobConversationParticipant>(new ZQuery(JobConversationParticipantSchema.JCP_JCC_Conversation, conversations.Select(conversation => conversation.PK)));

			foreach (var conversation in conversations)
			{
				var parentType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(conversation.JCC_ParentTableCode, false);

				factory.AddFetchHint(parentType, conversation.JCC_ParentID);
				factory.AddFetchHint(typeof(JobConversationMessage), new ZQuery(JobConversationMessageSchema.JCM_JCC_Conversation, conversation.PK));
			}
		}
	}
}
