using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class TransactionRequestApprovalHtmlEmailDef : HtmlEmailDef, IEmailCreator
	{
		EmailSendResult IEmailCreator.Create(ITransactionParticipant factory)
		{
			var result = EmailSendResult.Unsuccessful;

			try
			{
				Env.OutgoingMailManager.Create(factory, this);
				result = EmailSendResult.Successful;
			}
			catch (EmailHasNoRecipientsException)
			{
			}
			catch (EmailNotCompleteException)
			{
			}

			return result;
		}
	}
}
