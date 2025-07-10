using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	[Serializable]
	public class IncidentApprovalLogSubscriber : LogSubscriber
	{
		public override string Name => "IncidentApprovalLogSubscriber";

		public override string[] TableNames => new string[] { IncidentRequestSchema.Constants.TableName };

		public override string[] EventTypes => new string[] { Events.IncidentApprovalRequestedCode };

		public override bool IsClientSpecificSubscriber => true;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			foreach (var logsPerParent in queuedLogs.Where(x => x.SJ_GS_NKUser == User.WebUserCode).GroupBy(x => x.SJ_ParentID))
			{
				var query = new ZQuery(IncidentRequestSchema.PK, logsPerParent.Key).AddToFilter(IncidentRequestSchema.INC_Status, StatusApprovalRequested);
				var incidentRequest = factory.LoadTop1<IncidentRequest>(query);

				if (incidentRequest != null)
				{
					SendEmailSafe(incidentRequest, DefaultLogger, queuedLogs[0].Factory);
				}
			}
		}

		static void SendEmailSafe(IncidentRequest incidentRequest, ILogger logger, BusinessObjectFactory mailFactory)
		{
			try
			{
				var approverSubQuery = new ZDBOnlySubQuery(typeof(OrgDocument), OrgDocumentSchema.OD_OC);
				approverSubQuery.AddToFilter(OrgDocumentSchema.OD_DocumentGroup, EDIOrgDocumentGroupTypes.Codes.ERequestPendingApprovals);
				var approverQuery = new ZDBOnlyQuery(typeof(OrgContact));
				approverQuery.AddToFilter(OrgContactSchema.OC_OH, incidentRequest.ReportedBy.OC_OH);
				approverQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
				approverQuery.AddToFilter(OrgContactSchema.OC_Email, SQLComparisonOperator.NotEqual, "");
				approverQuery.AddSubQuery(approverSubQuery, JoinCondition.And);
				var approvers = incidentRequest.Factory.Load<OrgContact>(approverQuery).Select(x => x.OC_Email.ToString());

				var followerSubQuery = new ZDBOnlySubQuery(typeof(JobConversation), JobConversationSchema.PK);
				followerSubQuery.AddToFilter(JobConversationSchema.JCC_ParentTableCode, incidentRequest.TablePrefix);
				followerSubQuery.AddToFilter(JobConversationSchema.JCC_ParentID, incidentRequest.PK);
				var followerQuery = new ZDBOnlyQuery(typeof(JobConversationParticipant));
				followerQuery.AddSubQuery(JobConversationParticipantSchema.JCP_JCC_Conversation, followerSubQuery, JoinCondition.And);
				var followers = incidentRequest.Factory.Load<JobConversationParticipant>(followerQuery).Where(x => x.Parent.IsActive && !x.Parent.Email.IsEmpty).Select(x => x.Parent.Email.ToString());

				var template = new ERequestPendingApprovalNotificationMessageContentBuilder(incidentRequest);
				var incident = incidentRequest.Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_IncidentNumber, incidentRequest.INC_IncidentNumber));
				var email = EDIEmailBuilder.GetInstance(incident).BuildHtmlEmailDefByTemplate(template);
				if (email != null)
				{
					email.FromAddress = SupportIncident.SupportEmailAddress;
					email.FromDisplayName = SupportIncident.SupportDisplayName;

					var reporter = new[] { incidentRequest.ReportedBy.OC_Email.ToString() };
					var toRecipients = Enumerable.Empty<string>();
					var ccRecipients = Enumerable.Empty<string>();
					IEqualityComparer<string> comparer = StringComparer.OrdinalIgnoreCase;

					if (approvers.Any())
					{
						toRecipients = approvers;
						ccRecipients = reporter.Concat(followers);
					}
					else
					{
						toRecipients = reporter;
						ccRecipients = followers;
					}

					toRecipients = toRecipients.Distinct(comparer);
					ccRecipients = ccRecipients.Distinct(comparer).Except(toRecipients, comparer);

					email.AddRecipientForUserCommunication(toRecipients.ToArray(), RecipientDef.RecipientTypes.TO);
					if (ccRecipients.Any())
					{
						email.AddRecipientForUserCommunication(ccRecipients.ToArray(), RecipientDef.RecipientTypes.CC);
					}

					Env.OutgoingMailManager.Create(mailFactory, email);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var msg = incidentRequest.INC_IncidentNumber + " failed to send approval request notification email";
				logger?.Error(msg, ex);
				ErrorReporter.ReportOnce(msg, ex);
				SendInternalNotification(msg, ex.ToString(), mailFactory);
			}
		}

		static void SendInternalNotification(string subject, string body, BusinessObjectFactory mailFactory)
		{
			try
			{
				var regItem = EDIDataRegistry.Instance.InternalNotificationGroup;
				var mail = new EmailDef
				{
					Subject = subject,
					Body = body + System.Environment.NewLine + System.Environment.NewLine
					+ "You are receiving this email because you are a member of the group in registry " + FullName(regItem)
				};

				Env.OutgoingMailManager.Create(mailFactory, mail, regItem.Value, GroupSourceLocator.GetFromRegistryItem(regItem));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Unhandled Exception while sending internal notification", e);
			}
		}

		static string FullName(IRegistryItem regItem) => string.Join(" > ", regItem.Categories) + " > " + regItem.Caption;

		internal const string StatusApprovalRequested = "APR";
	}
}
