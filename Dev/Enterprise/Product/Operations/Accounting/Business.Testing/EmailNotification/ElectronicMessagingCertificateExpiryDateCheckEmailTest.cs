using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Accounting.ElectronicMessaging.ElectronicMessagingCertificateExpiryDateCheck;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class ElectronicMessagingCertificateExpiryDateCheckEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(ElectronicMessagingCertificateExpiryDateCheckEmail);
			}
		}

		protected virtual string CompanyLevelSubject => "E-Reporting Certificate expiry notification[TAU]";

		protected virtual string CompanyLevelBody(ControllerID controllerID, GlbCompany company, EInvoicingCertificateCredential[] credentials)
		{
			var companyLink = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, company.PK.ToGuid());

			return $@"<html>
<style>
</style>
<body>
<div class=WordSection1>
<p class=MsoNormal>The following E-Invoicing certificates are closed to the
expiry date.<o:p></o:p></p>
<p class=MsoNormal><o:p>&nbsp;</o:p></p>
<p class=MsoNormal><a href='{companyLink}'>Company [TAU]Dummy AU Company</a><o:p></o:p></p>
<p class=MsoNormal>1. Dummy PAC1, Issuer: DummyIssuer1, Expiry Date: 20-Jul-22 18:30<o:p></o:p></p>
<p class=MsoNormal>2. Dummy PAC2, Issuer: DummyIssuer2, Expiry Date: 20-Jul-22 19:30<o:p></o:p></p>
<p class=MsoNormal>3. Dummy PAC3, Issuer: DummyIssuer3, Expiry Date: 15-Jul-22 19:30<o:p></o:p></p>
<p class=MsoNormal>4. Dummy PAC4, Issuer: DummyIssuer4, Expiry Date: 09-Jul-22 19:30<o:p></o:p></p>
<p class=MsoNormal><o:p>&nbsp;</o:p></p>
<p class=MsoNormal>Please check and make the necessary extension if required.<o:p></o:p></p>
</div>
</body>
</html>";
		}

		protected virtual string BranchLevelBody(ControllerID controllerID, GlbBranch branch, EInvoicingCertificateCredential[] credentials)
		{
			var branchLink = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, branch.PK.ToGuid());

			return $@"<html>
<style>
</style>
<body>
<div class=WordSection1>
<p class=MsoNormal>The following E-Invoicing certificates are closed to the
expiry date.<o:p></o:p></p>
<p class=MsoNormal><o:p>&nbsp;</o:p></p>
<p class=MsoNormal><a href='{branchLink}'>Branch [AU1]Dummy Perth Branch</a><o:p></o:p></p>
<p class=MsoNormal>1. Dummy PAC1, Issuer: DummyIssuer1, Expiry Date: 20-Jul-22 18:30<o:p></o:p></p>
<p class=MsoNormal>2. Dummy PAC2, Issuer: DummyIssuer2, Expiry Date: 20-Jul-22 19:30<o:p></o:p></p>
<p class=MsoNormal>3. Dummy PAC4, Issuer: DummyIssuer4, Expiry Date: 09-Jul-22 18:30<o:p></o:p></p>
<p class=MsoNormal><o:p>&nbsp;</o:p></p>
<p class=MsoNormal>Please check and make the necessary extension if required.<o:p></o:p></p>
</div>
</body>
</html>";
		}

		protected virtual string BranchLevelSubject => "E-Reporting Certificate expiry notification[TAU - AU1]";

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

		ElectronicMessagingCertificateExpiryDateCheckEmail CreateEmailDefType(GlbCompany company, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials)
			=> (ElectronicMessagingCertificateExpiryDateCheckEmail)Activator.CreateInstance(EmailDefType, new object[] { company, recipientGuid, credentials });

		ElectronicMessagingCertificateExpiryDateCheckEmail CreateEmailDefType(GlbBranch branch, Guid recipientGuid, IEnumerable<EInvoicingCertificateCredential> credentials)
			=> (ElectronicMessagingCertificateExpiryDateCheckEmail)Activator.CreateInstance(EmailDefType, new object[] { branch, recipientGuid, credentials });

		void AssertMail(ElectronicMessagingCertificateExpiryDateCheckEmail mail, string expectedSubject, string expectedBody, IEnumerable<string> expectedReceivers)
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
