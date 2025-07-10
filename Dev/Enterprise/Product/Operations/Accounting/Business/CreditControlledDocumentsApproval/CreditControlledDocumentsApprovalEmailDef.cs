using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class CreditControlledDocumentsApprovalEmailDef : AccountingEmailDef
	{
		readonly string subject;
		readonly string body;

		public CreditControlledDocumentsApprovalEmailDef(CreditControlledDocumentsApproval approvalRequest)
		{
			ContentType = EmailContentTypes.HTML;
			AddReceipients(approvalRequest);
			subject = GetSubjectCore(approvalRequest);
			body = GetBodyCore(approvalRequest);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return null; }
		}

		protected override void SendCore()
		{
			Env.OutgoingMailManager.CreateAndSave(this);
		}

		void AddReceipients(CreditControlledDocumentsApproval approvalRequest)
		{
			if (approvalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested)
			{
				var groupRegItem = ObjectFactory.Get<IAccounting>().Registry.ARCreditControlledDocumentsApprovalNotifyGroup;
				AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty((Guid)groupRegItem.Value, groupRegItem);
			}
			else
			{
				var requestor = new BusinessObjectFactory().LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, approvalRequest.XP_SystemCreateUser);

				if (requestor != null && !requestor.GS_EmailAddress.IsEmpty)
				{
					AddRecipientForUserCommunication(requestor.GS_EmailAddress);
				}
			}
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(CreditControlledDocumentsApproval approvalRequest)
		{
			return Res.GetString("487cb72d-6c6c-42ec-bd5d-360538b43823", "AR Credit Controlled Documents approval request for Job Number '{0}' was {1}",
				approvalRequest.JobNumber,
				approvalRequest.Lookups.ApprovalStatusList.GetDescriptionFromCode(approvalRequest.XP_ApprovalStatus));
		}

		protected override string GetBody()
		{
			return body;
		}

		string GetBodyCore(CreditControlledDocumentsApproval approvalRequest)
		{
			string linkHref = null;
			var controllerID = approvalRequest.ControllerID;

			if (controllerID != null)
			{
				linkHref = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, approvalRequest.XP_ParentID.ToGuid());
			}

			var staffThatPeformedAction = approvalRequest.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code,
					approvalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested ? approvalRequest.XP_SystemCreateUser : approvalRequest.XP_GS_NKApprovingUser1);

			var linkHtml = linkHref == null ? string.Empty :
				(NoResString)@"<p>" +
				Res.GetString("bcb890d3-1c59-44ea-8515-3010605bd8cb", "See {0} for more details.", FormattableString.Invariant($"<a href=\"{linkHref}\">{approvalRequest.JobNumber}</a>")) +
				(NoResString)"</p>";

			return
				(NoResString)"<p>" +
					Res.GetString("a578192e-a051-4953-9869-6964e50780e4", "AR Credit Controlled Documents Approval request with description '{0}' was {1} by user '{2}'.",
					approvalRequest.XP_ReasonDescription,
					approvalRequest.Lookups.ApprovalStatusList.GetDescriptionFromCode(approvalRequest.XP_ApprovalStatus),
					staffThatPeformedAction != null ? (string)staffThatPeformedAction.GS_FullName : (NoResString)"") +
				(NoResString)"</p>" +
				linkHtml;
		}
	}
}
