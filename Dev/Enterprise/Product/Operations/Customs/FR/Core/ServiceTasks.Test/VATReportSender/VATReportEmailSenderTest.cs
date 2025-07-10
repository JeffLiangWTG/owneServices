using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	public class VATReportEmailSenderTest : TestCaseWithFactory
	{
		[TestDate(2022, 1, 27)]
		public void TestDoEverything()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TESTOrg";
			importer.OH_FullName = "Test organisation";
			importer.MainAddress.OA_Email = "mainAddress@123.com";

			var contact1 = importer.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "1@123.com";

			var contact2 = importer.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "2@123.com";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;

			var contact3 = importer.Contacts.AddNew();
			contact3.OC_ContactName = "contact3";
			contact3.OC_Email = "2@123.com";
			contact3.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;

			var contact4 = importer.Contacts.AddNew();
			contact4.OC_ContactName = "contact4";
			contact4.OC_Email = "3@123.com";
			contact4.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;

			var contact5 = importer.Contacts.AddNew();
			contact5.OC_ContactName = "contact5";
			contact5.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;

			var contact6 = importer.Contacts.AddNew();
			contact6.OC_ContactName = "contact6";
			contact6.OC_Email = "4@123.com";
			contact6.OC_IsActive = false;
			contact6.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "TestCompany";
			company.GC_Email = "company@123.com";

			var logger = new LoggingInformation();
			var sender = new VATReportEmailSender(Factory, logger);
			sender.DoEverything(importer, company, "", "attachment");

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
			AssertNull(email);
			AssertEquals("There is no VAT report for Test organisation in 12/2021 for TestCompany.", logger.UserLogStrings[0].Trim());
			logger.ClearLogs();

			using (var file = Enterprise.ZArchitecture.Core.TempFile.New())
			{
				sender.DoEverything(importer, company, file.Filename, "attachment1");
				email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertNotNull(email);
				AssertEquals("VAT report for Test organisation in 12/2021 for TestCompany", email.Subject);
				AssertEquals(@"Bonjour,

Veuillez trouver ci-joint le rapport d’auto-liquidation de TVA du mois précédent.

Sincères salutations,
Le service Douane.", email.Body);
				AssertEquals("attachment1", email.Attachments[0].DisplayName);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "2@123.com", "3@123.com" }, email.Recipients.ToStringCollection());
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				logger.ClearLogs();
			}

			using (var file = Enterprise.ZArchitecture.Core.TempFile.New())
			{
				importer.Contacts.RemoveAndDeleteAll();

				sender.DoEverything(importer, company, file.Filename, "attachment2");
				email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertNotNull(email);
				AssertEquals(@"Bonjour,

Veuillez trouver ci-joint le rapport d’auto-liquidation de TVA du mois précédent.

Sincères salutations,
Le service Douane.", email.Body);
				AssertEquals("attachment2", email.Attachments[0].DisplayName);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "mainAddress@123.com" }, email.Recipients.ToStringCollection());
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				logger.ClearLogs();
			}

			using (var file = Enterprise.ZArchitecture.Core.TempFile.New())
			{
				importer.MainAddress.OA_Email = ZString.Empty;

				sender.DoEverything(importer, company, file.Filename, "attachment3");
				email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertNotNull(email);
				AssertEquals(@"Bonjour,

Veuillez trouver ci-joint le rapport d’auto-liquidation de TVA du mois précédent.
Il n'y a pas d'adresse e-mail définie pour l'importateur Test organisation, en conséquence le rapport de TVA ne leur a pas été envoyé, veuillez leur transmettre l'e-mail et configurer l'adresse e-mail dans CargoWiseOne.

Sincères salutations,
Le service Douane.", email.Body);
				AssertEquals("attachment3", email.Attachments[0].DisplayName);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "company@123.com" }, email.Recipients.ToStringCollection());
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				logger.ClearLogs();
			}

			using (var file = Enterprise.ZArchitecture.Core.TempFile.New())
			{
				company.GC_Email = ZString.Empty;

				sender.DoEverything(importer, company, file.Filename, "attachment4");
				email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertNull(email);
				AssertEquals("Please set the email of the contacts with VAT allocated contact of Test organisation.", logger.UserLogStrings[0].Trim());
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				logger.ClearLogs();
			}
		}
	}
}
