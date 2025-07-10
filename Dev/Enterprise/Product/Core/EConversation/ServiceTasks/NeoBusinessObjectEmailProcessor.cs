using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.EConversation.ServiceTasks
{
	public abstract class NeoBusinessObjectEmailProcessor<T> : BusinessObjectEmailProcessor<T> where T : BusinessObject, IAllowAttachEmailsToEDocs, IConversationProvider
	{
		public override string MailApplicationCode => "NCV";

		protected override string DocType => GlowRegistry.Instance.NeoConversationsAttachmentDocType.Value;

		protected override bool ShouldProcessFailures => false;

		protected override T LoadBusinessObject(MailItem mailItem, IEmailProcessorLogger logger) => LoadFromUniqueEmailIdentifier(mailItem, logger);

		protected override bool ShouldAttachEmailAndSave(MailItem mailItem, T bizO) => true;

		protected override void AttachEmailToAnExistingBusinessObject(MailItem mailItem, T bizO, IEmailProcessorLogger logger)
		{
			base.AttachEmailToAnExistingBusinessObject(mailItem, bizO, logger);
			var eConvoAttacher = new BusinessObjectEConversationAttacher();
			var mailItemAsEmail = new Email((byte[])mailItem.RawEmailBytes);
			eConvoAttacher.AttachEmailToJobConversation(mailItemAsEmail, DocType, bizO);
		}
	}
}
