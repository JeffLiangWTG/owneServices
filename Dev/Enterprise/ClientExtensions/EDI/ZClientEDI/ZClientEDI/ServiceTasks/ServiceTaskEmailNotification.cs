using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.ServiceTasks
{
	public static class ServiceTaskEmailNotification
	{
		public static void SendEmailToGroup(string subject, string body, GuidRegistryItem notificationGroup, ILogger log)
		{
			if (notificationGroup.Value != ZGuid.Empty)
			{
				var emailDef = new EmailDef();
				emailDef.Subject = subject;
				emailDef.Body = body;
				try
				{
					Env.OutgoingMailManager.CreateAndSave(emailDef, notificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(notificationGroup));
				}
				catch (EmailHasNoRecipientsException ex)
				{
					log.Error(ex.Message);
				}
			}
		}
	}
}
