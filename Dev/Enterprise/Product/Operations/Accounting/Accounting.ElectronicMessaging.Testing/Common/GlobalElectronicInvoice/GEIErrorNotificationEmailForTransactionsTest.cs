using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GEIErrorNotificationEmailForTransactionsTest : EInvoicingTransactionErrorNotificationEmailTest
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

			var transaction1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction1.AH_TransactionNum = "INV000783";
			transaction1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transaction1.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var transaction2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction2.AH_TransactionNum = "CRD000261";
			transaction2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transaction2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			var transaction3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction3.AH_TransactionNum = "INV000118";
			transaction3.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			transaction3.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var incorrectTransactionDetails = new[]
			{
				IncorrectTransactionDetails.FromBizo(transaction1, new [] { (ZString)"Transaction the first was not successful because..." }),
				IncorrectTransactionDetails.FromBizo(transaction2, Enumerable.Empty<ZString>()),
				IncorrectTransactionDetails.FromBizo(transaction3, new [] { (ZString)"Some kind of error for the third transaction", (ZString)"And another" })
			};

			return new GEIErrorNotificationEmail(ediMessage, incorrectTransactionDetails);
		}

		protected override ZString GetExpectedSubject()
		{
			return "E-Reporting error notification for 3 items [EDI]";
		}

		protected override IEnumerable<ZString> GetExpectedBodyParts()
		{
			var parts = new List<ZString>();
			parts.Add(@"<html><body>
The following items were not successfully submitted to E-Reporting authority:
<br/><br/>
<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=ARInvoice&BusinessEntityPK=");
			parts.Add(@">Transaction AR INV INV000783</a><br/>
<b>
&nbsp;&nbsp;
Error Details:
</b>
<div style='width:1200px; margin-left:30px;'>
Transaction the first was not successful because...
</div>
<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=ARCreditNote&BusinessEntityPK=");
			parts.Add(@">Transaction AR CRD CRD000261</a><br/>
<a href='edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=APInvoice&BusinessEntityPK=");
			parts.Add(@">Transaction AP INV INV000118</a><br/>
<b>
&nbsp;&nbsp;
Error Details:
</b>
<div style='width:1200px; margin-left:30px;'>
Some kind of error for the third transaction<br/>And another
</div>
</br>
If you wish to re-submit the items, please reset the status of each item to Queued in the appropriate module.
<br/><br/>
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
