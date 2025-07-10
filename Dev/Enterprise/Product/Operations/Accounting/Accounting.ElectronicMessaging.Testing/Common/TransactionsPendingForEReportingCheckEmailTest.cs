using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class TransactionsPendingForEReportingCheckEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType => typeof(TransactionsPendingForEReportingCheckEmail);

		public void TestArgumentExceptionWhenNullPassed()
		{
			var exception = AssertExceptionThrown<ArgumentNullException>("The code param should not be null", () => new TransactionsPendingForEReportingCheckEmail(null, Guid.Empty, null));
			AssertEquals("Value cannot be null.\r\nParameter name: code", exception.Message);
		}

		public void TestEmailFormatting()
		{
			var company = TestObjectCreator.CreateNewCompany("TKR", CountryCodes.KoreaSouth, TestObjectCreator.ABIGAS);
			company.GC_Name = "Dummy KR Company";

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00000001");
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00000002");
			var creditNote1 = TestObjectCreator.CreateARCreditNote("00000002", TestObjectCreator.ABIGAS);
			var creditNote2 = TestObjectCreator.CreateARCreditNote("00000001", TestObjectCreator.ABIGAS);
			var adjustment1 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("00000001", 10m, 0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK.ToGuid());
			var adjustment2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("00000003", 20m, 0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK.ToGuid());

			TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Pending);
			TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: EInvoicingPivotState.Pending);
			TestObjectCreator.CreateEInvoicingTransactionPivot(creditNote1, status: EInvoicingPivotState.Pending);
			TestObjectCreator.CreateEInvoicingTransactionPivot(creditNote2, status: EInvoicingPivotState.Pending);
			TestObjectCreator.CreateEInvoicingTransactionPivot(adjustment1, status: EInvoicingPivotState.Pending);
			TestObjectCreator.CreateEInvoicingTransactionPivot(adjustment2, status: EInvoicingPivotState.Pending);

			var invoiceLink1 = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ARInvoice, invoice1.PK.ToGuid());
			var invoiceLink2 = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ARInvoice, invoice2.PK.ToGuid());
			var invoiceLink3 = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ARCreditNote, creditNote2.PK.ToGuid());
			var invoiceLink4 = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ARCreditNote, creditNote1.PK.ToGuid());
			var invoiceLink5 = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ARAdjustmentNote, adjustment1.PK.ToGuid());
			var invoiceLink6 = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ARAdjustmentNote, adjustment2.PK.ToGuid());

			var expectedTransactions = new[] { invoice1, invoice2, creditNote1, creditNote2, adjustment1, adjustment2 }.Select(x => (x.PK, x.AH_TransactionType, x.AH_TransactionNum)).ToArray();
			var email = new TransactionsPendingForEReportingCheckEmail(company.GC_Code, TestNotificationGroup.PK.ToGuid(), expectedTransactions);
			email.Send();

			var expectedRecipients = new[] { "testmail1@wisetechglobal.com", "testmail2@wisetechglobal.com" };
			var expectedSubject = "Transactions pending for E-Reporting notification [TKR]";
			var expectedBody = $@"<html><body>
<p>Following transactions are pending for E-Reporting:</p>
<p><a href='{invoiceLink1}'>Transaction AR INV 00000001</a></p>
<p><a href='{invoiceLink2}'>Transaction AR INV 00000002</a></p>
<p><a href='{invoiceLink3}'>Transaction AR CRD 00000001</a></p>
<p><a href='{invoiceLink4}'>Transaction AR CRD 00000002</a></p>
<p><a href='{invoiceLink5}'>Transaction AR ADJ 00000001</a></p>
<p><a href='{invoiceLink6}'>Transaction AR ADJ 00000003</a></p>
</body></html>
";

			AssertEquals("Mail has been sent", email, Env.OutgoingMailManager.EmailsCreated.Last());
			AssertEquals("Subject", expectedSubject, email.Subject);
			AssertEquals("Body", expectedBody, email.Body);
			AssertContainsExactElementsInAnyOrder("Recipients", expectedRecipients, email.Recipients.Cast<RecipientDef>().Select(x => x.Email));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestNotificationGroup = TestObjectCreator.CreateStaffGroup("GP1");

			var staff1 = TestObjectCreator.CreateStaff("AAA");
			staff1.GS_EmailAddress = "testmail1@wisetechglobal.com";
			TestNotificationGroup.Staff.Add(staff1);

			var staff2 = TestObjectCreator.CreateStaff("BBB");
			staff2.GS_EmailAddress = "testmail2@wisetechglobal.com";
			TestNotificationGroup.Staff.Add(staff2);

			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestNotificationGroup = null;
		}

		GlbGroup TestNotificationGroup;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
