using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class ImplementationEmailProcessor : EDIBusinessObjectEmailProcessor<EDIProject>
	{
		protected override void AttachEmailToAnExistingBusinessObject(MailItem mailItem, EDIProject project, IEmailProcessorLogger logger)
		{
			string assigneeAddress = GetAssigneeAddressToForwardEmailTo(project);
			string groupManagerAddress = GetGroupManagerAddressToForwardEmailTo(project);
			string mailboxAddress = EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value;

			bool canSendToAssignee = !string.IsNullOrEmpty(assigneeAddress) && assigneeAddress != mailboxAddress;
			bool canSendToGroupManager = !string.IsNullOrEmpty(groupManagerAddress) && groupManagerAddress != mailboxAddress;
			System.Collections.Generic.List<string> recipients = new System.Collections.Generic.List<string>();
			base.AttachEmailToAnExistingBusinessObject(mailItem, project, logger);
			if (canSendToAssignee)
			{
				recipients.Add(assigneeAddress);
			}
			if (canSendToGroupManager)
			{
				recipients.Add(groupManagerAddress);
			}
			if (canSendToAssignee || canSendToGroupManager)
			{
				MailItemCopySender copySender = new MailItemCopySender(new MailItem[] { mailItem });
				copySender.MailAddressToSendCopyTo = string.Join(";", recipients.ToArray());
				copySender.SendCopyTo();
				if (!copySender.Errors.IsEmpty)
				{
					logger.Log(LogType.Error, false, copySender.Errors);
				}
			}
		}

		string GetAssigneeAddressToForwardEmailTo(EDIProject project)
		{
			GlbStaff assignee = project.ProjectManager;
			return (assignee != null && !assignee.GS_EmailAddress.IsEmpty) ? assignee.GS_EmailAddress : ZString.Empty;
		}

		string GetGroupManagerAddressToForwardEmailTo(EDIProject project)
		{
			string result = "";

			GlbStaff assignee = project.ProjectManager;
			if (assignee == null || assignee.GS_EmailAddress.IsEmpty || !assignee.IsWorkingToday)
			{
				GlbStaff appropriateGroupManager = project.GetGroupManager(GetInstallationGroupForProject(project));
				if (appropriateGroupManager != null && !appropriateGroupManager.GS_EmailAddress.IsEmpty)
				{
					result = appropriateGroupManager.GS_EmailAddress;
				}
			}

			return result;
		}

		GlbGroup GetInstallationGroupForProject(EDIProject project)
		{
			ZGuid groupPK = EDIDataRegistry.Instance.IncidentInstallationsGroupENT.Value;
			GlbGroup group = null;
			if (!groupPK.IsEmpty && groupPK.IsValid)
			{
				group = project.Factory.Load<GlbGroup>(groupPK);
			}
			return group;
		}

		public override string EmailTypeName
		{
			get { return "Implementation"; }
		}

		public override string MailApplicationCode
		{
			get { return EDIMailApplication.Implementation; }
		}

		protected override string DocType => "COR";

		internal override string GetBusinessObjectIdentifierFromSubject(string subject)
		{
			Regex pattern = new Regex(@"PRJ[0-9]+");
			return pattern.Match(subject).Value;
		}

		protected override bool ShouldAttachEmailAndSave(MailItem mailItem, EDIProject workTask)
		{
			return true;
		}

		protected override EDIProject LoadFromIdentifier(BusinessObjectFactory factory, string identifier)
		{
			return factory.LoadFromNaturalKey<EDIProject>(WorkProjectSchema.WKP_ProjectNumber, identifier);
		}
	}
}

