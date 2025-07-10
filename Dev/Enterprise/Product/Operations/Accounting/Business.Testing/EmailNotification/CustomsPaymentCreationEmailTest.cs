using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using CustomsPaymentCreationResult = Enterprise.Accounting.Business.JobInvoicing.CustomsPaymentCreator.CustomsPaymentCreationResult;
using DataProviderWithNotifications = Enterprise.Accounting.Business.JobInvoicing.CustomsPaymentCreator.DataProviderWithNotifications;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class CustomsPaymentCreationEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		public void TestSendToPostMasterGroupWhenGroupIsNoLongerValid()
		{
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Delete();
			var message = new ZStringBuilder();
			message.Append("Error Messages");
			var paymentCreationResult = new CustomsPaymentCreationResult(JobInvoicing.PaymentPostingResult.Partially, message, "HyperlinkText", true);
			var invoice = Factory.New<APInvoice>();
			var provider = new DataProviderWithNotifications(ChargesProvider1, new[] { invoice }, "");
			provider.CustomsAmount = 110m;
			provider.CustomsTaxAmount = 5m;
			provider.InvoiceAmount = 110m;
			provider.InvoiceTaxAmount = 5m;
			paymentCreationResult.DataProvidersWithNotifications.Add(provider);
			var dataProvider = new DataProviderWithNotifications(ChargesProvider1, new[] { invoice }, "Statement 1:");
			paymentCreationResult.DataProvidersWithNotifications.Add(dataProvider);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var emailSender = new CustomsPaymentCreationEmail();
			AssertNoExceptionThrown(() => emailSender.Send(group.PK, Factory, paymentCreationResult, "ABCD1234"));
			AssertEquals("Emails Created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef lastEmail = Env.OutgoingMailManager.EmailsCreated[0];
			Assert(lastEmail.Recipients.Contains(EmailAddress));
			Assert(lastEmail.Body.Contains("Statement 1:"));
			Assert(lastEmail.Subject.Contains("Auto-Payment partially successful for ABCD1234"));
		}

		public void TestEmailGenerationForPaymentCreationSuccess()
		{
			GlbGroup group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "PMG"));
			var message = new ZStringBuilder();
			message.Append("Error Messages");
			CustomsPaymentCreationResult paymentCreationResult = new CustomsPaymentCreationResult(JobInvoicing.PaymentPostingResult.Successful, message, "HyperlinkText", false);
			APInvoice invoice = Factory.New<APInvoice>();
			DataProviderWithNotifications provider = new DataProviderWithNotifications(ChargesProvider1, new[] { invoice }, "");
			provider.CustomsAmount = 110m;
			provider.CustomsTaxAmount = 5m;
			provider.InvoiceAmount = 110m;
			provider.InvoiceTaxAmount = 5m;
			paymentCreationResult.DataProvidersWithNotifications.Add(provider);
			DataProviderWithNotifications dataProvider = new DataProviderWithNotifications(ChargesProvider1, new[] { invoice }, "");
			paymentCreationResult.DataProvidersWithNotifications.Add(dataProvider);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			CustomsPaymentCreationEmail emailSender = new CustomsPaymentCreationEmail();
			emailSender.Send(group.PK, Factory, paymentCreationResult, "ABCD1234");
			AssertEquals("Emails Created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef lastEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Subject", "Auto-Payment for ABCD1234", lastEmail.Subject);
			AssertContains("Body", "Auto-Payment for ABCD1234", lastEmail.Body);
			string expected = @"A payment was created successfully for HyperlinkText</br></br>Error Messages</br></br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Unique Number</th><th>Customs Amount</th><th>Customs Tax Amount</th><th>AP Inv. Amount</th><th>AP Inv. Tax Amount</th><th>Comment</th></tr></thead><tr><td>ABCD1234</td><td>110</td><td>5</td><td>110</td><td>5</td><td>AP Invoice  is already paid</td></tr><tr><td>ABCD1234</td><td>110.0</td><td>0</td><td>0</td><td>0</td><td>AP Invoice  is already paid</td></tr></table>";
			AssertContains("Body", expected, lastEmail.Body);
		}

		public void TestEmailGenerationForPaymentCreationFailure()
		{
			GlbGroup group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "PMG"));
			var message = new ZStringBuilder();
			message.Append("Error Messages");
			CustomsPaymentCreationResult paymentCreationResult = new CustomsPaymentCreationResult(JobInvoicing.PaymentPostingResult.Failed, message, "HyperlinkText", false);
			APInvoice invoice = Factory.New<APInvoice>();
			DataProviderWithNotifications provider = new DataProviderWithNotifications(ChargesProvider1, new[] { invoice }, "");
			provider.CustomsAmount = 110m;
			provider.CustomsTaxAmount = 5m;
			provider.InvoiceAmount = 110m;
			provider.InvoiceTaxAmount = 5m;
			paymentCreationResult.DataProvidersWithNotifications.Add(provider);
			DataProviderWithNotifications dataProvider = new DataProviderWithNotifications(ChargesProvider1, new[] { invoice }, "Statement 1:");
			paymentCreationResult.DataProvidersWithNotifications.Add(dataProvider);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			CustomsPaymentCreationEmail emailSender = new CustomsPaymentCreationEmail();
			emailSender.Send(group.PK, Factory, paymentCreationResult, "ABCD1234");
			AssertEquals("Emails Created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef lastEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Subject", "Auto-Payment failure for ABCD1234", lastEmail.Subject);
			AssertContains("Body", "Auto-Payment failure for ABCD1234", lastEmail.Body);
			string expected = @"The following errors were encountered while trying to automatically create a payment for HyperlinkText</br></br></br>Error Messages</br></br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Unique Number</th><th>Customs Amount</th><th>Customs Tax Amount</th><th>AP Inv. Amount</th><th>AP Inv. Tax Amount</th><th>Comment</th></tr></thead><tr><td>ABCD1234</td><td>110</td><td>5</td><td>110</td><td>5</td><td>AP Invoice  is already paid</td></tr><tr><td>ABCD1234</td><td>110.0</td><td>0</td><td>0</td><td>0</td><td>Statement 1:AP Invoice  is already paid</td>";
			AssertContains("Body", expected, lastEmail.Body);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupStaffMemberEmailAddress();
			ZGuid creditor = ZGuid.NewZGuid();
			CustomsCharge charge1 = new CustomsCharge(null, "TEST", 110.0m, 0m, true, creditor);
			CustomsCharge charge2 = new CustomsCharge(null, "TES2", 120.0m, 0m, false, creditor);
			CustomsCharge charge3 = new CustomsCharge(null, "TES3", 140.0m, 0m, true, creditor);
			CustomsCharge1 = CreateMockCustomCharge(charge1, charge2);
			CustomsCharge2 = CreateMockCustomCharge(charge1, charge3);
			CustomsJob1 = CreateMockCustomsJobProvider(Factory, "AAA111");
			CustomsJob2 = CreateMockCustomsJobProvider(Factory, "BBB222");
			ChargesProvider1 = CreateMockCustomsChargesProvider(CustomsJob1, "ABCD1234", CustomsCharge1);
			CreateMockCustomsChargesProvider(CustomsJob2, "WXYZ5678", CustomsCharge1, CustomsCharge2);
		}

		protected void SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
		}

		protected ZString EmailAddress
		{
			get
			{
				return new ZString("blahblah@whatever.example");
			}
		}

		MockCustomCharge CreateMockCustomCharge(params CustomsCharge[] charges)
		{
			var mockCustomCharge = new MockCustomCharge();
			mockCustomCharge.fIsActive = true;
			mockCustomCharge.fCustomsCharges = charges;
			return mockCustomCharge;
		}

		MockCustomsJobProvider CreateMockCustomsJobProvider(BusinessObjectFactory factory, ZString jobNumber)
		{
			var mockCustomsJobProvider = Factory.New<MockCustomsJobProvider>();
			mockCustomsJobProvider.JobNumber = jobNumber;
			return mockCustomsJobProvider;
		}

		MockCustomsChargesProvider CreateMockCustomsChargesProvider(MockCustomsJobProvider customsJobProvider, ZString invoiceNumber, params ICustomsCharges[] customsCharges)
		{
			var mockCustomsChargesProvider = new MockCustomsChargesProvider();
			mockCustomsChargesProvider.CustomsJob = customsJobProvider;
			mockCustomsChargesProvider.Factory = Factory;
			mockCustomsChargesProvider.CustomsCharges = customsCharges;
			mockCustomsChargesProvider.InvoiceNumber = invoiceNumber;
			return mockCustomsChargesProvider;
		}

		protected override Type EmailDefType
		{
			get
			{
				return (typeof(CustomsPaymentCreationEmail));
			}
		}

		MockCustomsJobProvider CustomsJob1, CustomsJob2;
		MockCustomsChargesProvider ChargesProvider1;
		MockCustomCharge CustomsCharge1, CustomsCharge2;
	}
}
