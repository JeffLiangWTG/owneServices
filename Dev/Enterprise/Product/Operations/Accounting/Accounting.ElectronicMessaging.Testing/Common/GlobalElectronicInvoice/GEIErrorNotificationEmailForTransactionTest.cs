using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GEIErrorNotificationEmailForTransactionTest : EInvoicingTransactionErrorNotificationEmailTest
	{
		protected override EInvoicingTransactionErrorNotificationEmail GetNewBusinessObject()
		{
			var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
			ediMessage.EM_MessageNum = "EDI000123";
			var errors = new List<ZString>();
			errors.Add("This is a very long Error Description 1 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.");
			errors.Add("This is a very long Error Description 2 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.");
			errors.Add("This is a very long Error Description 3 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.");
			errors.Add("This is a very long Error Description 4 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.");
			errors.Add("This is a very long Error Description 5 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.");
			errors.Add("This is a very long Error Description 6 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.");
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionNum = "INV0000890";
			transaction.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var incorrectTransactionDetails = IncorrectTransactionDetails.FromBizo(transaction, errors);
			return new GEIErrorNotificationEmail(ediMessage, incorrectTransactionDetails);
		}

		protected override ZString GetExpectedSubject()
		{
			return "E-Reporting error notification for Transaction AR INV INV0000890 [EDI]";
		}

		protected override IEnumerable<ZString> GetExpectedBodyParts()
		{
			var parts = new List<ZString>();
			parts.Add(@"<html><body>
The following transaction was not successfully submitted to E-Reporting authority:
<br/><br/>
<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=ARInvoice&BusinessEntityPK=");
			parts.Add(@">AR INV INV0000890</a><br/>
<br/>
If you wish to re-submit the transaction, please reset the status of each transaction to Queued in the Receivables Transactions module.
<br/><br/>
<b>

Error Details:
</b>
<div style='width:1200px; '>
This is a very long Error Description 1 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.<br/>This is a very long Error Description 2 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.<br/>This is a very long Error Description 3 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.<br/>This is a very long Error Description 4 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.<br/>This is a very long Error Description 5 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.<br/>This is a very long Error Description 6 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.
</div>
</br>
<b>
Related EDI Message:
</b><a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=EDIMessage&BusinessEntityPK=");
			parts.Add(@">EDI000123</a><br/>
<b>
Company:
</b>
EDI - Eagle Datamation International
<br/>
</body></html>");
			return parts;
		}
	}
}
