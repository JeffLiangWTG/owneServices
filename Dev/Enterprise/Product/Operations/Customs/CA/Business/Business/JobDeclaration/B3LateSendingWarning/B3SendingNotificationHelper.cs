using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class B3SendingNotificationHelper
	{
		public B3SendingNotificationHelper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}

		readonly JobDeclaration declaration;

		BusinessObjectFactory Factory
		{
			get
			{
				return declaration.Factory;
			}
		}

		internal ZString GetHyperLink()
		{
			ZString editFormUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.JobDeclaration, declaration.PK.ToGuid());
			if (!editFormUrl.IsEmpty)
			{
				return ZString.Format("<a href=\"{0}\">{1}</a>", editFormUrl, declaration.HumanReadableName);
			}
			else
			{
				return declaration.HumanReadableName;
			}
		}

		internal void SendEmail(string subject, string bodyText)
		{
			var companyPK = declaration.RegistryCompanyPK;
			var branchPK = declaration.RegistryBranchPK;
			var emailMode = CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);

			var email = new EmailDef();
			email.Subject = subject;
			email.Body = bodyText;
			email.ContentType = EmailContentTypes.HTML;

			if (emailMode == Core.Constants.EmailTo.StaffMember || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				var userToNotify = GetUserToNotify();
				if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty)
				{
					email.AddRecipientForSystemCommunication(userToNotify.GS_EmailAddress);
				}
			}
			if (email.Recipients.Count == 0 || emailMode == Core.Constants.EmailTo.NominatedGroup || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				var emailGroup = new ZGuid(CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty));
				if (emailGroup.IsValid && Factory.Load<GlbGroup>(emailGroup) is GlbGroup group)
				{
					email.AddRecipientForSystemCommunication(group.Staff.Cast<GlbStaff>().Select(x => x.GS_EmailAddress.ToString())
						.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray());
				}
			}

			if (email.Recipients.Count == 0)
			{
				var userEmailToNotify = GetBrokerOrCreateUserEmail();
				if (!userEmailToNotify.IsEmpty)
				{
					email.AddRecipientForSystemCommunication(userEmailToNotify);
				}
			}

			if (email.Recipients.Count > 0)
			{
				Env.OutgoingCustomsMailManager.Create(Factory, email);
			}
		}

		GlbStaff GetUserToNotify()
		{
			var message = declaration.Messages.OfType<Enterprise.Messaging.Business.EDIMessage>().Where(m => m.EM_SystemCreateUser != User.ServiceUserCode && m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
				.OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault();

			if (message != null)
			{
				return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, message.EM_SystemCreateUser);
			}

			return null;
		}

		ZString GetBrokerOrCreateUserEmail()
		{
			var result = declaration.CusAgent?.GS_EmailAddress ?? ZString.Empty;
			return result.IsEmpty ? Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, declaration.JE_SystemCreateUser)?.GS_EmailAddress ?? ZString.Empty : result;
		}
	}
}
