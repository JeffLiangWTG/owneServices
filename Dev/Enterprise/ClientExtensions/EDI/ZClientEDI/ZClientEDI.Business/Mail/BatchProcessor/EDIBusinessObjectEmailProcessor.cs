using CargoWise.EntityFramework;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Mail.Business
{
	public abstract class EDIBusinessObjectEmailProcessor<T> : BusinessObjectEmailProcessor<T> where T : BusinessObject, IAllowAttachEmailsToEDocs
	{
		protected override T LoadBusinessObject(MailItem mailItem, IEmailProcessorLogger logger)
		{
			var subject = mailItem.MI_Subject;
			var identifier = GetBusinessObjectIdentifierFromSubject(subject);
			logger.Log(LogType.Information, true, "Identifier from mail subject: {0}", identifier);

			if (string.IsNullOrEmpty(identifier))
			{
				return null;
			}

			var factory = mailItem.Factory;

			return LoadFromIdentifier(factory, identifier);
		}

		internal abstract string GetBusinessObjectIdentifierFromSubject(string subject);

		protected abstract T LoadFromIdentifier(BusinessObjectFactory factory, string identifier);
	}
}
