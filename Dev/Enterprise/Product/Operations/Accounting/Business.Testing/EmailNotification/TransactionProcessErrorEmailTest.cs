using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Testing.EmailNotification
{
	public class TransactionProcessErrorEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType => typeof(TransactionProcessErrorEmail);

		public void TestArgumentExceptionWhenNullPassed1()
		{
			var exception = AssertExceptionThrown<ArgumentNullException>("The errorTransaction param should not be null", () => new TransactionProcessErrorEmail(null));
			AssertEquals("Value cannot be null.\r\nParameter name: errorTransaction", exception.Message);
		}

		public void TestBody()
		{
			var errorTransaction = TestObjectCreator.CreateTransactionPendingAllocation("11112222", TestObjectCreator.Creditor1, 100);
			var orgHeader = Factory.Load<OrgHeader>(errorTransaction.AH_OH);
			var transactionLink = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.TransactionsPendingAllocation, errorTransaction.PK.ToGuid());
			var expectedBody = $@"<html>
<style>
	table {{
		border-collapse:collapse 
	}}
	table td {{
		padding:0in 5.4pt 0in 5.4pt;	
		border:solid windowtext 1.0pt;
		height:14.5pt
	}}
</style>
<body>
<p>The xml file associated with this transaction contains one or more warnings or errors associated with transactional data contained in the file.</p>

<p>As a result, the transaction has not automatically posted from the Transactions Pending Allocation (TPA) screen in Cargowise using the ATP trigger.</p>

<p>Follow the link to the transaction and go to the <strong>Notes</strong> tab to find the associated warnings/errors that will help you fix the transaction.</p>

<p><span><a href='{transactionLink}'>{errorTransaction.AH_TransactionNum}</a></span></p>

<p>Alternatively, if you are unable to access the link, use the following information to identify the transaction:</p>
<table>
<tr>
	<td>
	Creditor Code:
	</td>
	<td width = 250>
	{orgHeader.OH_Code}
	</td>
</tr>
<tr>
	<td>
	Creditor Name:
	</td>
	<td width = 250>
	{orgHeader.OH_FullName}
	</td>
</tr>
<tr>
	<td>
	Post Date:
	</td>
	<td width = 250>
	{errorTransaction.AH_PostDate.ToShortDateString()}
	</td>
</tr>
<tr>
	<td>
	Transaction number:
	</td>
	<td width = 250>
	{errorTransaction.AH_TransactionNum}
	</td>
</tr>
<tr>
	<td>
	Amount (incl tax):
	</td>
	<td width = 250>
	{errorTransaction.AH_OSExTaxAmount.ToString()}
	</td>
</tr>
<tr>
	<td>
	Description:
	</td>
	<td width = 250>
	{errorTransaction.AH_Desc}
	</td>
</tr>
</table>
</body></html>";

			var email = new TransactionProcessErrorEmail(errorTransaction);
			AssertEquals("Body of email", expectedBody, email.Body);
		}

		public override void TestSend()
		{
			var fallbackStaffGroupPK = TestObjectCreator.CreateRecipientGroup("TTT", "XXX", "TTT@test.com");
			NotificationDataRegistry.Instance.XMSFailureFallBackNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackStaffGroupPK.PK.ToGuid());

			AssertRecipient("TransactionsPendingAllocationXMLPostingFailureNotificationGroup registry error, recipient should be fallback to XMSFailureFallBackNotificationGroup",
				ZGuid.NewZGuid().ToGuid(),
				"TTT@test.com",
				1);

			var recipientGroup = TestObjectCreator.CreateRecipientGroup("MMM", "YYY", "test@test.com");
			AssertRecipient("Recipient should be TransactionsPendingAllocationXMLPostingFailureNotificationGroup",
				recipientGroup.PK.ToGuid(),
				"test@test.com",
				2);
		}

		public void TestBodyIfOrgHeaderIsNull()
		{
			var errorTransaction = TestObjectCreator.CreateTransactionPendingAllocation("11112222", TestObjectCreator.Creditor1, 100);
			errorTransaction.AH_OH = ZGuid.Empty;
			var expectedContent = @"
<tr>
	<td>
	Creditor Code:
	</td>
	<td width = 250>
	
	</td>
</tr>
<tr>
	<td>
	Creditor Name:
	</td>
	<td width = 250>
	
	</td>
</tr>";
			var email = new TransactionProcessErrorEmail(errorTransaction);
			AssertContains("Body of email", expectedContent, email.Body);
		}

		void AssertRecipient(string comment, Guid notificationGroupPK, string expectedRecipient, int expectedEmailCount)
		{
			NotificationDataRegistry.Instance.TransactionsPendingAllocationXMLPostingFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroupPK); // to check empty handling

			var errorTransaction = TestObjectCreator.CreateTransactionPendingAllocation("11112222", TestObjectCreator.Creditor1, 100);
			var transactionProcessErrorEmail = new TransactionProcessErrorEmail(errorTransaction);
			transactionProcessErrorEmail.Send();
			AssertEquals("Should have sent email", expectedEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(comment, expectedRecipient, Env.OutgoingMailManager.EmailsCreated.Last().Recipients[0]);
		}

		public void TestSubject()
		{
			var errorTransaction = TestObjectCreator.CreateTransactionPendingAllocation("11112222", TestObjectCreator.Creditor1, 100);
			var email = new TransactionProcessErrorEmail(errorTransaction);
			AssertEquals("Subject of email", "Unable to post transaction 11112222 due to warnings/errors identified as part of ATP trigger event", email.Subject);
		}

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));

		TestObjectCreator fTestObjectCreator;
	}
}
