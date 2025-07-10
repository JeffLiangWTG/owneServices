using System.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.Business
{
	public class JASMailSender
	{
		virtual public void SendEmail(ZGuid notificationGroupPK, IRegistryItem notificationGroup, string subject, string body, string attachment)
		{
			EmailDef emailDef = new EmailDef();

			emailDef.Subject = subject;
			emailDef.Body = body;

			if (attachment != null && File.Exists(attachment))
			{
				emailDef.Attachments.Add(new AttachmentDef(attachment));
			}

			if (!notificationGroupPK.IsEmpty)
			{
				OutgoingMailManager.CreateAndSave(emailDef, notificationGroupPK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(notificationGroup));
			}
		}

		protected virtual IOutgoingMailManager OutgoingMailManager
		{
			get
			{
				return Env.OutgoingMailManager;
			}
		}
	}
}
