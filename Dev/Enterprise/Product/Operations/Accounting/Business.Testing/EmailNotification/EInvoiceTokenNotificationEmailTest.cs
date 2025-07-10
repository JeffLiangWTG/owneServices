using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Accounting.ElectronicMessaging.EmailNotification;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class EInvoiceTokenNotificationEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
			=> typeof(EInvoiceTokenNotificationEmail);

		protected virtual string CompanyLevelSubject => "E-Invoice Token Expiry Notification (TAU)";

		protected virtual string CompanyLevelBody(ControllerID controllerID, GlbCompany company, EInvoicingCertificateCredential[] credentials)
		{
			var link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, company.PK.ToGuid());

			return @$"<html>
	<style></style>
	<body>
		<p>The following E-Invoicing Refresh Token is about to expire:</p>
		<p>Company TAU : Dummy AU Company</p>
		<p>Refresh Token, Issue Date : {credentials[0].GP_IssueDate:dd-MMMM-yyyy} , Expiry Date : 20-July-2022</p>
		<p>Re-authorization is required before the refresh token expires.</p>
		<p>
			<a href='{link}'>Company [TAU]Dummy AU Company</a>
		</p>
	</body>
</html>";
		}

		protected virtual string BranchLevelBody(ControllerID controllerID, GlbBranch branch, EInvoicingCertificateCredential[] credentials)
		{
			var link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, branch.PK.ToGuid());

			return @$"<html>
	<style></style>
	<body>
		<p>The following E-Invoicing Refresh Token is about to expire:</p>
		<p>Company TAU : Company</p>
		<p>Refresh Token, Issue Date : {credentials[0].GP_IssueDate:dd-MMMM-yyyy} , Expiry Date : 20-July-2022</p>
		<p>Re-authorization is required before the refresh token expires.</p>
		<p>
			<a href='{link}'>Branch [AU1]Dummy Perth Branch</a>
		</p>
	</body>
</html>";
		}

		protected virtual string BranchLevelSubject => "E-Invoice Token Expiry Notification (TAU)";

		public void TestCompanyLevelFormatting()
		{
			var company = TestObjectCreator.CreateNewCompany("TAU", CountryCodes.Australia, TestObjectCreator.ABIGAS);
			company.GC_Name = "Dummy AU Company";

			var credentials = new[] { TestObjectCreator.CreateCompanyCertificate(company, "DummyIssuer1", "Dummy PAC1", new DateTime(2022, 07, 20, 18, 30, 59))
				, TestObjectCreator.CreateCompanyCertificate(company, "DummyIssuer2", "Dummy PAC2", new DateTime(2022, 07, 20, 19, 30, 59))
				, TestObjectCreator.CreateCompanyCertificate(company, "DummyIssuer3", "Dummy PAC3", new DateTime(2022, 07, 15, 19, 30, 59))
				, TestObjectCreator.CreateCompanyCertificate(company, "DummyIssuer4", "Dummy PAC4", new DateTime(2022, 07, 09, 19, 30, 59))
			};

			AssertMail(CreateEmailDefType(company, TestNotificationGroup.PK.ToGuid(), credentials)
				, CompanyLevelSubject
				, CompanyLevelBody(ControllerIDs.GlbCompany, company, credentials)
				, new[] { "testmail1@wisetechglobal.com", "testmail2@wisetechglobal.com" }
			);
		}

		public void TestBranchLevelLevelFormatting()
		{
			var branch = TestObjectCreator.CreateBranch("AU1"
				, TestObjectCreator.CreateNewCompany("TAU", CountryCodes.Australia, TestObjectCreator.ABIGAS)
				, TestObjectCreator.ABIGAS
			);
			branch.GB_BranchName = "Dummy Perth Branch";

			var credentials = new[] { TestObjectCreator.CreateBranchCertificate(branch, "DummyIssuer1", "Dummy PAC1", new DateTime(2022, 07, 20, 18, 30, 59))
				, TestObjectCreator.CreateBranchCertificate(branch, "DummyIssuer2", "Dummy PAC2", new DateTime(2022, 07, 20, 19, 30, 59))
				, TestObjectCreator.CreateBranchCertificate(branch, "DummyIssuer4", "Dummy PAC4", new DateTime(2022, 07, 09, 18, 30, 59))
			};

			AssertMail(CreateEmailDefType(branch, TestNotificationGroup.PK.ToGuid(), credentials)
				, BranchLevelSubject
				, BranchLevelBody(ControllerIDs.GlbBranch, branch, credentials)
				, new[] { "testmail1@wisetechglobal.com", "testmail2@wisetechglobal.com" }
			);
		}

		EInvoiceTokenNotificationEmail CreateEmailDefType(GlbCompany company, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials)
			=> (EInvoiceTokenNotificationEmail)Activator.CreateInstance(EmailDefType, new object[] { company, recipientGuid, credentials });

		EInvoiceTokenNotificationEmail CreateEmailDefType(GlbBranch branch, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials)
			=> (EInvoiceTokenNotificationEmail)Activator.CreateInstance(EmailDefType, new object[] { branch, recipientGuid, credentials });

		void AssertMail(EInvoiceTokenNotificationEmail mail, string expectedSubject, string expectedBody, IEnumerable<string> expectedReceivers)
		{
			mail.Send();
			CombineAssertions(() =>
			{
				AssertEquals("Send Mail", mail, Env.OutgoingMailManager.EmailsCreated.Last());
				AssertEquals("Subject", expectedSubject, mail.Subject);
				AssertEquals("Body", expectedBody, mail.Body);
				AssertContainsExactElementsInAnyOrder("Recipients", expectedReceivers
					, mail.Recipients.Cast<RecipientDef>().Select(x => x.Email)
				);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestNotificationGroup = TestObjectCreator.CreateStaffGroup("GP1");

			var staff1 = TestObjectCreator.CreateStaff("AAA");
			staff1.GS_EmailAddress = "testmail1@wisetechglobal.com";
			TestNotificationGroup.Staff.Add(staff1);

			var staff2 = TestObjectCreator.CreateStaff("AAB");
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
