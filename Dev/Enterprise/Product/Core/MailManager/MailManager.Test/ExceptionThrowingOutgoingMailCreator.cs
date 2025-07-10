using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager.Testing
{
	public class ExceptionThrowingOutgoingMailCreator : OutgoingMailCreator
	{
		protected override MailItem Create_Core(BusinessObjectFactory factory, EmailDef emailToSend, string groupDetail)
		{
			throw new EmailSendFailedException("Email failed to send for test");
		}
	}
}
