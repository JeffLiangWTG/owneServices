using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GEIErrorNotificationEmailTest : TestCaseWithFactory
	{
		public void TestEmailSubject_WithNullEDIMessage()
		{
			var email = new GEIErrorNotificationEmail(null, PrepareEmailTestData());

			AssertEquals("Email Subject match With Null EDIMessage.", "E-Reporting error notification for Transaction AR INV INV0000890", email.GetSubject_forTest());
		}

		public void TestEmailSubject_WithNullCompany()
		{
			var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
			ediMessage.EM_GB = ZGuid.Empty;

			var email = new GEIErrorNotificationEmail(ediMessage, PrepareEmailTestData());

			AssertEquals("Email Subject match With Null Company.", "E-Reporting error notification for Transaction AR INV INV0000890", email.GetSubject_forTest());
		}

		public void TestEmailSubject_WithEmptyCompanyCode()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = ZString.Empty;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
			ediMessage.EM_GB = branch.PK;

			var email = new GEIErrorNotificationEmail(ediMessage, PrepareEmailTestData());

			AssertEquals("Email Subject match With Empty Company Code.", "E-Reporting error notification for Transaction AR INV INV0000890", email.GetSubject_forTest());
		}

		public void TestEmailSubject_WithProperCompanyCode()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "GCC";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
			ediMessage.EM_GB = branch.PK;

			var email = new GEIErrorNotificationEmail(ediMessage, PrepareEmailTestData());

			AssertEquals("Email Subject match With Empty GC_Code.", "E-Reporting error notification for Transaction AR INV INV0000890 [GCC]", email.GetSubject_forTest());
		}

		IncorrectTransactionDetails PrepareEmailTestData()
		{
			var errors = new List<ZString>();
			errors.Add("This is a very long Error Description 1 that cannot fit into one line in an email, so we expect this error message to be wrapped when it exceeds the character limit and cannot fit into one page, so that the readbility of this message will be imrpoved to a great extent.");
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionNum = "INV0000890";
			transaction.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;

			return IncorrectTransactionDetails.FromBizo(transaction, errors);
		}
	}
}
