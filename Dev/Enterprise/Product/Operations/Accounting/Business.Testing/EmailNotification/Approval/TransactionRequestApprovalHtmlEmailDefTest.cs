using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.EmailNotification.Approval
{
	public class TransactionRequestApprovalHtmlEmailDefTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var factory = new BusinessObjectFactory();
			var emailDef = new TransactionRequestApprovalHtmlEmailDef();
			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;

			var result = ((IEmailCreator)emailDef).Create(factory);
			AssertEquals(EmailSendResult.Unsuccessful, result);
			AssertEquals("Emails created count", previousEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);

			emailDef.AddRecipientForUserCommunication("test@recipient.com");
			result = ((IEmailCreator)emailDef).Create(factory);
			AssertEquals(EmailSendResult.Successful, result);

			var tempFromAddress = emailDef.FromAddress;
			emailDef.FromAddress = "InvalidEmailId$%#";
			result = ((IEmailCreator)emailDef).Create(factory);
			AssertEquals(EmailSendResult.Unsuccessful, result);

			emailDef.FromAddress = tempFromAddress;
			result = ((IEmailCreator)emailDef).Create(factory);
			AssertEquals(EmailSendResult.Successful, result);
		}
	}
}
