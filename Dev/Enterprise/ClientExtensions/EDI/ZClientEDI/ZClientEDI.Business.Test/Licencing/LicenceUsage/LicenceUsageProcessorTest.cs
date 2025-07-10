using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.BatchProcessor.Testing
{
	public class LicenceUsageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess_EdiLicenceUsage()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", createClientCompany: true);
			Factory.Save();

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			{
				var data = new LicenceConsumptionLogSchema();
				var log1 = CreateOdmLog(data, "", "Staff One", "staff.one@test.com");
				log1.UsageTime = new ZDateTime(2018, 3, 5, 9, 32, 0);

				var log2 = CreateOdmLog(data, "", "Staff One", "staff.one@test.com");
				log2.UsageTime = new ZDateTime(2018, 3, 6, 9, 45, 0);

				var writer = new StringWriter();
				serializer.Serialize(writer, data);
				var actualXml = writer.GetStringBuilder().ToString();

				var processor = new LicenceUsageProcessor(null);
				processor.Process(actualXml);

				var usages = LoadAllEdiLicenceUsage();
				AssertEquals(1, usages.Length);
				var usage = usages[0];
				AssertEquals(201803, usage.LX2_Period);
				AssertEquals(lic.ClientCompany.PK, usage.LX2_LCC);
				AssertEquals(BillingConstants.CoreModuleCode, usage.LX2_ModuleCode);
				AssertEquals("ODM", usage.LX2_LicenceMode);
				AssertEquals("staff.one@test.com", usage.Staff.LS_Email);
				AssertEquals("Staff One", usage.Staff.LS_FullName);
				AssertEquals(new ZDateTime(2018, 3, 5, 9, 32, 0), usage.LX2_FirstUsageUtc);
				AssertEquals(new ZDateTime(2018, 3, 6, 9, 45, 0), usage.LX2_LastUsageUtc);
				AssertEquals(2, usage.LX2_UsageCount);
			}

			{
				var data = new LicenceConsumptionLogSchema();
				var log1 = CreateOdmLog(data, "", "Staff One", "staff.one@test.com");
				log1.UsageTime = new ZDateTime(2018, 3, 7, 9, 0, 0);

				var log2 = CreateOdmLog(data, "", "Staff One", "staff.one@test.com");
				log2.UsageTime = new ZDateTime(2018, 3, 8, 9, 0, 0);

				var log3 = CreateOdmLog(data, "", "Staff One", "staff.one@test.com");
				log3.UsageTime = new ZDateTime(2018, 3, 7, 9, 0, 0);
				log3.LicenceModuleCode = "FOR";

				var writer = new StringWriter();
				serializer.Serialize(writer, data);
				var actualXml = writer.GetStringBuilder().ToString();

				var processor = new LicenceUsageProcessor(null);
				processor.Process(actualXml);
				var usages = LoadAllEdiLicenceUsage();
				AssertEquals(2, usages.Length);
				var usageCor = usages.Single(x => x.LX2_ModuleCode == BillingConstants.CoreModuleCode);
				AssertEquals("staff.one@test.com", usageCor.Staff.LS_Email);
				AssertEquals("Staff One", usageCor.Staff.LS_FullName);
				AssertEquals(new ZDateTime(2018, 3, 5, 9, 32, 0), usageCor.LX2_FirstUsageUtc);
				AssertEquals(new ZDateTime(2018, 3, 8, 9, 0, 0), usageCor.LX2_LastUsageUtc);
				AssertEquals("usage count is accumulated", 4, usageCor.LX2_UsageCount);

				var usageFor = usages.Single(x => x.LX2_ModuleCode == "FOR");
				AssertEquals(new ZDateTime(2018, 3, 7, 9, 0, 0), usageFor.LX2_FirstUsageUtc);
				AssertEquals(new ZDateTime(2018, 3, 7, 9, 0, 0), usageFor.LX2_LastUsageUtc);
				AssertEquals("usage count", 1, usageFor.LX2_UsageCount);
			}

			// new period
			{
				var data = new LicenceConsumptionLogSchema();
				var log1 = CreateOdmLog(data, "", "Staff One", "staff.one@test.com");
				log1.UsageTime = new ZDateTime(2018, 4, 4, 9, 0, 0);

				var writer = new StringWriter();
				serializer.Serialize(writer, data);
				var actualXml = writer.GetStringBuilder().ToString();

				var processor = new LicenceUsageProcessor(null);
				processor.Process(actualXml);
				var usages = LoadAllEdiLicenceUsage();
				AssertEquals(3, usages.Length);
				var usage = usages.Single(x => x.LX2_ModuleCode == BillingConstants.CoreModuleCode && x.LX2_Period == 201804);
				AssertEquals("usage count", 1, usage.LX2_UsageCount);
			}
		}

		public void TestProcess_EdiLicenceUsage_UsageToIgnore()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", createClientCompany: true);
			Factory.Save();

			var data = new LicenceConsumptionLogSchema();
			CreateOdmLog(data, "", "CargoWise One Support", "");
			CreateOdmLog(data, "", "CargoWise Support", "");
			CreateOdmLog(data, "", "EDI Support", "");
			CreateOdmLog(data, "", "Staff 1", "one@cargowise.com");
			CreateOdmLog(data, "", "Staff 2", "two@syd.cargowise.com");
			CreateOdmLog(data, "", "Staff 3", "three@cargowise.com.au");
			CreateOdmLog(data, "", "Staff 4", "four@wisetechglobal.com");
			CreateOdmLog(data, "", "Staff 5", "five@syd.wisetechglobal.com");
			CreateOdmLog(data, "", "Staff 6", "five@wg.cargowise.com");
			CreateOdmLog(data, "E", "This is for support", "");

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllEdiLicenceUsage();
			var actual = string.Join(", ", usages.Select(x => x.Staff.LS_FullName).OrderBy(x => x));
			var expected = "Staff 2, Staff 3, Staff 5, Staff 6";
			AssertEquals(expected, actual);
		}

		public void TestProcess_EdiLicenceUsage_Hosted()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", createClientCompany: true);
			lic.Database.LD_HostedLocation = "SYD";
			Factory.Save();

			var data = new LicenceConsumptionLogSchema();
			var log1 = CreateOdmLog(data, "ONE", "Staff One", "one@customer.com");
			var log2 = CreateOdmLog(data, "ONE", "Staff One", "one@customer.com");
			log2.LicenceModuleCode = "RDC";

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllEdiLicenceUsage();
			AssertEquals(1, usages.Length);
			var usage = usages[0];
			AssertEquals("RDC ignored for hosted customer", BillingConstants.CoreModuleCode, usage.LX2_ModuleCode);
		}

		public void TestProcess_EdiLicenceUsage_NotHosted()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", createClientCompany: true);
			lic.Database.LD_HostedLocation = "NCW";
			Factory.Save();

			var data = new LicenceConsumptionLogSchema();
			var log1 = CreateOdmLog(data, "ONE", "Staff One", "one@customer.com");
			var log2 = CreateOdmLog(data, "ONE", "Staff One", "one@customer.com");
			log2.LicenceModuleCode = "RDC";

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllEdiLicenceUsage();
			AssertEquals(2, usages.Length);
			AssertEquals("RDC not ignored for non-hosted customer", true, usages.Any(x => x.LX2_ModuleCode == "RDC"));
		}

		public void TestProcess_EdiLicenceUsage_NonProduction()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", createClientCompany: true);
			lic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			Factory.Save();

			var data = new LicenceConsumptionLogSchema();
			var log1 = CreateOdmLog(data, "ONE", "Staff One", "one@customer.com");
			var log2 = CreateOdmLog(data, "ONE", "Staff One", "one@customer.com");
			log2.LicenceModuleCode = "FOR";
			var log3 = CreateOdmLog(data, "ONE", "Staff One", "one@customer.com");
			log3.LicenceModuleCode = "SAL";

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllEdiLicenceUsage();
			AssertEquals(1, usages.Length);
			AssertEquals("Only COR usage recorded for test system", true, usages.Any(x => x.LX2_ModuleCode == BillingConstants.CoreModuleCode));
			AssertEquals("Only COR usage recorded for test system", false, usages.Any(x => x.LX2_ModuleCode != BillingConstants.CoreModuleCode));
		}

		LicenceConsumptionLogSchemaLicenceConsumptionLogs CreateOdmLog(LicenceConsumptionLogSchema data, string staffCode, string staffName, string staffEmail)
		{
			return CreateLog("ENTCOMSRV", data, staffCode, staffName, staffEmail, BillingConstants.CoreModuleCode, "ODM", new ZDateTime(2018, 3, 5, 9, 32, 0));
		}

		LicenceConsumptionLogSchemaLicenceConsumptionLogs CreateOdmLog(LicenceHeader licHeader, LicenceConsumptionLogSchema data, string staffCode, string staffName, string staffEmail, string moduleCode, ZDateTime usageTime)
		{
			return CreateLog(licHeader?.LicenceCode ?? "", data, staffCode, staffName, staffEmail, moduleCode, "ODM", usageTime);
		}

		LicenceConsumptionLogSchemaLicenceConsumptionLogs CreateLog(string licenceCode, LicenceConsumptionLogSchema data, string staffCode, string staffName, string staffEmail, string moduleCode, string licenceType, ZDateTime usageTime)
		{
			var log = data.Items.AddNew();
			log.StaffCode = staffCode;
			log.StaffName = staffName;
			log.StaffEmail = staffEmail;
			log.UsageTime = usageTime;
			log.LicenceModuleCode = moduleCode;
			log.CompanyLicenceCode = licenceCode;
			log.LicenceType = licenceType;
			return log;
		}

		LicenceConsumptionLogSchemaLicenceConsumptionLogs CreateCptLog(string licenceCode, LicenceConsumptionLogSchema data, string staffCode, string staffName, string staffEmail, string moduleCode, ZDateTime usageTime)
		{
			return CreateLog(licenceCode, data, staffCode, staffName, staffEmail, moduleCode, LicenceTypes.Codes.CPT, usageTime);
		}

		LicenceConsumptionLogSchemaLicenceConsumptionLogs CreateCptLog(LicenceHeader licHeader, LicenceConsumptionLogSchema data, string staffCode, string staffName, string staffEmail, string moduleCode, ZDateTime usageTime)
		{
			return CreateLog(licHeader?.LicenceCode ?? "", data, staffCode, staffName, staffEmail, moduleCode, LicenceTypes.Codes.CPT, usageTime);
		}

		public void TestProcess()
		{
			var licence = CreateAndSaveLicence();
			var data = new LicenceConsumptionLogSchema();
			CreateCptLog(licence, data, "", "Zubin Appoo", "zubs@zubs.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			var usage = usages[0];
			AssertEquals("FOR", usage.LX_ModuleCode);
			AssertEquals("zubs@zubs.com", usage.Staff.LS_Email);
			AssertEquals("Zubin Appoo", usage.Staff.LS_FullName);
			AssertEquals(new ZDateTime(2008, 3, 5, 9, 32, 0), usage.LX_UsageTime);
		}

		ClientLicenceUsage[] LoadAllCptUsages()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery();
			query.OrderBy = ClientLicenceUsageSchema.Constants.LX_UsageTime;
			return factory.Load<ClientLicenceUsage>(query);
		}

		EdiLicenceUsage[] LoadAllEdiLicenceUsage()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery();
			query.OrderBy = EdiLicenceUsageSchema.Constants.LX2_FirstUsageUtc;
			return factory.Load<EdiLicenceUsage>(query);
		}

		public void TestProcess_InvalidLicence()
		{
			var data = new LicenceConsumptionLogSchema();
			CreateCptLog("CRACRACRP", data, "", "Zubin Appoo", "zubs@zubs.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			AssertEquals(0, LoadAllCptUsages().Length);
			AssertEquals(0, LoadAllEdiLicenceUsage().Length);
		}

		public void TestProcess_CorruptData()
		{
			CreateAndSaveLicence();
			var data = CreateUsageDataWithError("Some Error");
			CreateUsageDataWithError(data, "Another Error");
			CreateUsageDataWithError(data, "And Another Error");

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var logger = new TestServiceLogger();
			var processor = new LicenceUsageProcessor(logger);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(3, usages.Length);
			AssertContains("On Demand Licence Usage Error - ZUBCOMSRV\r\nZUBCOMSRV - Some Error\r\nZUBCOMSRV - Another Error\r\nZUBCOMSRV - And Another Error\r\n", logger.ToString());
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcess_CorruptData_NotReported()
		{
			CreateAndSaveLicence();
			var data = CreateUsageDataWithError("Corrupt Usage Data blah blah");

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var logger = new TestServiceLogger();
			var processor = new LicenceUsageProcessor(logger);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotContains("On Demand Licence Usage Error", logger.ToString());
		}

		public void TestProcess_OldVersionXML()
		{
			var licence = CreateAndSaveLicence();

			string xml =
@"<?xml version=""1.0""?>
<LicenceConsumptionLogSchema xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService"">
  <LicenceConsumptionLogs>
	<LicenceModuleCode>COR</LicenceModuleCode>
	<StaffName>Accounts Department</StaffName>
	<StaffEmail>accounts@testaustralia.com</StaffEmail>
	<UsageTime>2010-02-04T22:05:00</UsageTime>
	<CompanyLicenceCode>ZUBCOMSRV</CompanyLicenceCode>
	<LicenceType>ODM</LicenceType>
  </LicenceConsumptionLogs>
  <LicenceConsumptionLogs>
	<LicenceModuleCode>COR</LicenceModuleCode>
	<StaffName>Marc Etta</StaffName>
	<StaffEmail>marc@testaustralia.com</StaffEmail>
	<UsageTime>2010-02-04T22:07:00</UsageTime>
	<CompanyLicenceCode>ZUBCOMSRV</CompanyLicenceCode>
	<LicenceType>ODM</LicenceType>
  </LicenceConsumptionLogs>
</LicenceConsumptionLogSchema>";

			var processor = new LicenceUsageProcessor(null);
			processor.Process(xml);

			var usage = LoadAllEdiLicenceUsage();
			AssertEquals(2, usage.Length);
			AssertEquals("accounts@testaustralia.com", usage[0].Staff.LS_Email);
			AssertEquals("marc@testaustralia.com", usage[1].Staff.LS_Email);
		}

		public void TestProcess_ResentUsage()
		{
			GlbStaff requester = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			requester.GS_EmailAddress = "jim@test.com";
			var licence = CreateAndSaveLicence();

			var databaseStaff = Factory.New<ClientStaff>();
			databaseStaff.LS_LD = licence.Database.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = licence.Company.LC_CompanyCode;
			clientCompany.LCC_LD = licence.Database.PK;

			var oldUsage = CreateCptUsage(clientCompany, databaseStaff, BillingConstants.CoreModuleCode, new ZDateTime(2010, 2, 15, 9, 33, 0));

			var orgContacts = licence.Company.Header.Contacts;
			var contact = orgContacts[orgContacts.Count - 1];
			databaseStaff.LS_Email = contact.OC_Email;
			databaseStaff.LS_FullName = contact.OC_ContactName;
			Assert(!contact.OC_Email.IsEmpty);

			Factory.Save();
			var usage = LoadAllCptUsages();
			AssertEquals(1, usage.Length);

			var data = new LicenceConsumptionLogSchema();
			data.IsRequest = true;
			data.IsRequestSpecified = true;
			data.RequestedBy = Env.CurrentUser.Initials;
			Assert(!data.RequestedBy.IsEmpty);
			data.DateFrom = new ZDate(2010, 2, 1);
			data.DateTo = new ZDate(2010, 2, 28);
			data.EnterpriseCode = "ZUB";
			data.ServerCode = "SRV";
			var log = CreateCptLog(licence, data, "", contact.OC_ContactName, contact.OC_Email, LegacyLicence.Codes.Core, new ZDateTime(2010, 2, 15, 9, 44, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			Factory.Save();

			usage = LoadAllCptUsages();
			AssertEquals(2, usage.Length);
			AssertEquals(contact.OC_Email, usage[0].Staff.LS_Email);
			AssertEquals(contact.OC_Email, usage[1].Staff.LS_Email);
			AssertEquals("old usage unchanged", new ZDateTime(2010, 2, 15, 9, 33, 0), usage[0].LX_UsageTime);
			AssertEquals("new usage present", new ZDateTime(2010, 2, 15, 9, 44, 0), usage[1].LX_UsageTime);

			AssertEquals("requester email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("subject", "Licence Usage received from [ZUB/-/SRV] for " + data.DateFrom.ToShortDateString() + " to " + data.DateTo.ToShortDateString(), email.Subject);
			AssertEquals("body", actualXml, email.Body);
			AssertEquals("recipient", 1, email.Recipients.Count);
			AssertEquals("recipient", "jim@test.com", email.Recipients[0].Email);
		}

		ClientLicenceUsage CreateCptUsage(ClientCompany clientCompany, ClientStaff clientStaff, string moduleCode, ZDateTime usageTime)
		{
			return BillingTestHelper.CreateCptLicenceUsage(clientCompany, clientStaff, moduleCode, usageTime);
		}

		public void TestProcess_ConcurrencyException()
		{
			var licence = CreateAndSaveLicence();

			var data = new LicenceConsumptionLogSchema();
			var log = CreateCptLog(licence, data, "", "Zubin Appoo", "zubs@zubs.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessorForConcurrencyTest(log.StaffName, licence.LA_LD, null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			var usage = usages[0];
			AssertEquals("FOR", usage.LX_ModuleCode);
			AssertNotNull(usage.Staff);
			AssertEquals("zubs@zubs.com", usage.Staff.LS_Email);
			AssertEquals("Zubin Appoo", usage.Staff.LS_FullName);
			AssertEquals(new ZDateTime(2008, 3, 5, 9, 32, 0), usage.LX_UsageTime);
		}

		LicenceConsumptionLogSchema CreateUsageDataWithError(string error)
		{
			var data = new LicenceConsumptionLogSchema();
			CreateUsageDataWithError(data, error);
			return data;
		}

		void CreateUsageDataWithError(LicenceConsumptionLogSchema data, string error)
		{
			var log = CreateLog("ZUBCOMSRV", data, "", "Zubin Appoo", "zubs@zubs.com", "FOR", LicenceTypes.Codes.CPT, new ZDateTime(2008, 3, 5, 9, 32, 0));
			log.ErrorStatus = error;
		}

		public void TestProcess_UnknownContact()
		{
			var licence = CreateAndSaveLicence();

			var data = new LicenceConsumptionLogSchema();
			CreateOdmLog(licence, data, "", "Paul Keating", "keating@test.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var factory = new BusinessObjectFactory();
			OrgContact newContact = (OrgContact)factory.Load<EDIOrgHeader>(licence.Company.Header.PK).Contacts.Find(new ZQuery(OrgContactSchema.OC_Email, "keating@test.com"))[0];
			AssertEquals("Paul Keating", newContact.OC_ContactName);
			AssertEquals("keating@test.com", newContact.OC_Email);
			AssertEquals(licence.Company.Header.PK, newContact.OC_OH);
			AssertEquals(true, newContact.IsInDatabase);
		}

		public void TestProcess_LongContactName()
		{
			ErrorReporter.Clear();

			var licence = CreateAndSaveLicence();

			var data = new LicenceConsumptionLogSchema();
			var log = CreateCptLog(licence, data, "", new ZString('A', OrgContact.Schema.OC_ContactNameMaxLength), "aaaa@test.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			var usage = usages[0];
			AssertEquals(log.StaffEmail, usage.Staff.LS_Email);
			AssertEquals(new ZString('A', OrgContact.Schema.OC_ContactNameMaxLength), usage.Staff.LS_FullName);

			var factory = new BusinessObjectFactory();
			var ediOrgHeader = factory.Load<EDIOrgHeader>(licence.Company.Header.PK);
			var contactList = ediOrgHeader.Contacts;
			var newContact = contactList[0];
			newContact = (OrgContact)ediOrgHeader.Contacts.Find(new ZQuery(OrgContactSchema.OC_Email, log.StaffEmail))[0];
			AssertEquals("name stayed the same size", new ZString('A', OrgContact.Schema.OC_ContactNameMaxLength), newContact.OC_ContactName);
			AssertEquals(log.StaffEmail, newContact.OC_Email);
			AssertEquals(licence.Company.Header.PK, newContact.OC_OH);
			AssertEquals("no error reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestProcess_CreateContactDoesNotLoadAllContactsNorOrgSecurityContacts()
		{
			var group1 = Factory.New<GlbGroup>();
			var group2 = Factory.New<GlbGroup>();

			group1.GG_Code = "G1";
			group1.GG_Desc = "GG1";
			group1.GG_Type = "ORG";
			group1.GG_IsSystemDefined = true;

			group2.GG_Code = "G2";
			group2.GG_Desc = "GG2";
			group2.GG_Type = "ORG";
			group2.GG_IsSystemDefined = false;

			var glbGroupRole1 = Factory.New<GlbGroupRole>();
			glbGroupRole1.GGR_RoleName = "db_datawriter";
			var glbGroupRole2 = Factory.New<GlbGroupRole>();
			glbGroupRole2.GGR_RoleName = "cwRestrictedReaderRole";

			group1.Roles.Add(glbGroupRole1);
			group2.Roles.Add(glbGroupRole2);

			Factory.Save();

			var licence = CreateAndSaveLicence();
			var anotherLicence = Enterprise.Client.EDI.Billing.Business.Test.BillingTestHelper.CreateLicence(Factory, "TTT");
			var org = licence.Company.Header;

			var orgLink = Factory.New<GlbGroupOrgLink>();
			orgLink.GOK_GG_Group = group1.PK;
			orgLink.GOK_OH_Org = org.PK;

			var orgLink2 = Factory.New<GlbGroupOrgLink>();
			orgLink2.GOK_GG_Group = group2.PK;
			orgLink2.GOK_OH_Org = org.PK;

			for (var i = 0; i < 10; ++i)
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "Contact" + i;
				contact.SecurityRightsForBindingOnly[0].OZ_Granted = (i % 1) == 0;

				var groupContactLink = Factory.New<GlbGroupOrgContactLink>();
				groupContactLink.GCK_GG_Group = group1.PK;
				groupContactLink.GCK_OC_Contact = contact.PK;
			}
			var contactOnAnotherOrgWithSameName = anotherLicence.Company.Header.Contacts.AddNew();
			contactOnAnotherOrgWithSameName.OC_ContactName = "Some User";
			contactOnAnotherOrgWithSameName.OC_Email = "SomeUser@test.com";

			Factory.Save();

			var data = new LicenceConsumptionLogSchema();
			{
				CreateCptLog(licence, data, "", "Some User", "SomeUser@test.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));
			}
			{
				CreateCptLog(licence, data, "", org.Contacts[0].OC_ContactName, "", "FOR", new ZDateTime(2008, 3, 5, 9, 45, 0));
			}
			{
				CreateCptLog(licence, data, "", "Some New Name", org.Contacts[0].OC_Email, "FOR", new ZDateTime(2008, 3, 5, 9, 55, 0));
			}

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessorForTest(null);
			processor.Process(actualXml);
			IBusinessObjectFactoryInternals internals = processor.ContactFactory;
			AssertEquals("OrgSecurityContacts records in contact factory", 0, internals.AllBusinessObjects.Count(x => x.GetType() == typeof(OrgSecurityContacts)));
			AssertEquals("only new OrgContact records are in contact factory", 1, internals.AllBusinessObjects.Count(x => x.GetType() == typeof(EDIOrgContact)));
			internals = processor.Factory_Exposed;
			AssertEquals("OrgSecurityContacts records in main factory", 0, internals.AllBusinessObjects.Count(x => x.GetType() == typeof(OrgSecurityContacts)));
			AssertEquals("OrgContact records in main factory", 0, internals.AllBusinessObjects.Count(x => x.GetType() == typeof(EDIOrgContact)));
			Assert("GlbGroup should not be loaded", !internals.AllBusinessObjects.Any(x => x.GetType() == typeof(GlbGroup)));
			Assert("GlbGroupOrgLink should not be loaded", !internals.AllBusinessObjects.Any(x => x.GetType() == typeof(GlbGroupOrgLink)));
			Assert("GlbGroupOrgContactLink should not be loaded", !internals.AllBusinessObjects.Any(x => x.GetType() == typeof(GlbGroupOrgContactLink)));
			Assert("GlbGroupRole should not be loaded", !internals.AllBusinessObjects.Any(x => x.GetType() == typeof(GlbGroupRole)));

			// processing it again should not create any contacts
			processor = new LicenceUsageProcessorForTest(null);
			processor.Process(actualXml);
			internals = processor.ContactFactory;
			AssertEquals("OrgSecurityContacts records in contact factory", 0, internals.AllBusinessObjects.Count(x => x.GetType() == typeof(OrgSecurityContacts)));
			AssertEquals("only new OrgContact records are in contact factory", 0, internals.AllBusinessObjects.Count(x => x.GetType() == typeof(EDIOrgContact)));
		}

		public void TestProcess_HybridConversion()
		{
			var licence = CreateAndSaveLicence();
			var data = new LicenceConsumptionLogSchema();
			CreateLog(licence.LicenceCode, data, "", "Staff Name", "staff@name.com", "FOR", LicenceTypes.Codes.PUR, new ZDateTime(2013, 6, 30, 23, 55, 0));
			CreateLog(licence.LicenceCode, data, "", "Staff Name", "staff@name.com", "FOR", LicenceTypes.Codes.REN, new ZDateTime(2013, 6, 30, 23, 59, 0));

			CreateLog(licence.LicenceCode, data, "", "Staff Name", "staff@name.com", "FOR", LicenceTypes.Codes.PUR, new ZDateTime(2013, 7, 1, 0, 0, 0));
			CreateLog(licence.LicenceCode, data, "", "Staff Name", "staff@name.com", "FOR", LicenceTypes.Codes.REN, new ZDateTime(2013, 7, 1, 0, 0, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllEdiLicenceUsage();
			AssertEquals(3, usages.Length);
			AssertEquals("licence type unchanged before conversion date", LicenceTypes.Codes.PUR, usages[0].LX2_LicenceMode);
			AssertEquals("licence type unchanged before conversion date", LicenceTypes.Codes.REN, usages[1].LX2_LicenceMode);
			AssertEquals("licence type changed to ODM after conversion date", LicenceTypes.Codes.ODM, usages[2].LX2_LicenceMode);
			AssertEquals(2, usages[2].LX2_UsageCount);
		}

		public void TestProcess_ClientCompany()
		{
			var licence = CreateAndSaveLicence();

			var data = new LicenceConsumptionLogSchema();
			var log1 = CreateCptLog(licence, data, "", "Sam", "sam@test.com", LegacyLicence.Codes.Core, new ZDateTime(2014, 9, 17, 10, 0, 0));
			var log2 = CreateCptLog(licence, data, "", "Tester", "tt@test.com", "REC", new ZDateTime(2014, 9, 17, 12, 0, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var query = new ZQuery(ClientCompanySchema.LCC_Code, "COM");
			query.AddToFilter(ClientCompanySchema.LCC_LD, licence.Database.PK);
			var clientCompanies = Factory.Load<ClientCompany>(query);
			AssertEquals("Should be only 1 client company", 1, clientCompanies.Length);

			var clientCompany = clientCompanies[0];
			AssertEquals(licence.Company.LC_OH, clientCompany.LCC_OH);

			var usages = LoadAllCptUsages();
			AssertEquals(2, usages.Length);

			var usage1 = usages[0];
			AssertEquals(LegacyLicence.Codes.Core, usage1.LX_ModuleCode);
			AssertEquals(clientCompany.PK, usage1.LX_LCC);

			var usage2 = usages[1];
			AssertEquals(Env.Licence.Recruiter.Name, usage2.LX_ModuleCode);
			AssertEquals(clientCompany.PK, usage2.LX_LCC);
		}

		public void TestProcess_UsageWithoutMatchedLicenceHeader()
		{
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ServerCode = "SYD";
			database.LD_LE = enterprise.PK;

			var clientCompany = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany.LCC_Code = "ABC";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			var data = new LicenceConsumptionLogSchema();
			var log1 = CreateCptLog("DDDABCSYD", data, "", "Sam", "sam@test.com", LegacyLicence.Codes.Core, new ZDateTime(2014, 9, 17, 10, 0, 0));
			var log2 = CreateCptLog("DDDABCSYD", data, "", "Tester", "tt@test.com", "REC", new ZDateTime(2014, 9, 17, 12, 0, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(2, usages.Length);

			var usage1 = usages[0];
			AssertEquals(LegacyLicence.Codes.Core, usage1.LX_ModuleCode);
			AssertEquals(clientCompany.PK, usage1.LX_LCC);

			var usage2 = usages[1];
			AssertEquals(Env.Licence.Recruiter.Name, usage2.LX_ModuleCode);
			AssertEquals(clientCompany.PK, usage2.LX_LCC);
		}

		public void TestSystemStaff()
		{
			var licence = CreateAndSaveLicence();

			var data = new LicenceConsumptionLogSchema();

			string[] cptOnlyStaffNames =
			{
				"Developer",
				"Controller",
				"EnterpriseBatchProcessor",
			};

			string[] alwaysExcludedStaffNames =
			{
				"EDI Support",
				"CargoWise Support",
				"CargoWise One Support"
			};

			string[] cptOnlyStaffCodes = new string[] { "C", "X", "~BP" };
			string[] alwaysExcludedStaffCodes = new string[] { "E" };

			foreach (string name in cptOnlyStaffNames.Concat(alwaysExcludedStaffNames))
			{
				CreateCptLog(licence, data, "", name, "", "ACP", new ZDateTime(2018, 3, 5, 9, 32, 0));
				CreateOdmLog(licence, data, "", name, "", "FOR", new ZDateTime(2018, 3, 5, 9, 32, 0));
			}
			CreateCptLog(licence, data, "", "Real Person", "", "ACP", new ZDateTime(2018, 3, 5, 9, 32, 0));
			CreateOdmLog(licence, data, "", "Real Person", "", "FOR", new ZDateTime(2018, 3, 5, 9, 32, 0));

			foreach (string code in cptOnlyStaffCodes.Concat(alwaysExcludedStaffCodes))
			{
				CreateCptLog(licence, data, code, "somename", "", "ACP", new ZDateTime(2018, 3, 5, 9, 32, 0));
				CreateOdmLog(licence, data, code, "somename", "", "FOR", new ZDateTime(2018, 3, 5, 9, 32, 0));
			}

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var cptUsages = LoadAllCptUsages();
			var monthlyUsages = LoadAllEdiLicenceUsage();
			Assert(cptUsages.All(x => x.LX_LicenceMode == "CPT"));
			AssertEquals("CPT usage is recorded for system accounts like BatchProcessor", cptOnlyStaffNames.Length + cptOnlyStaffCodes.Length + 1, cptUsages.Length);
			AssertEquals("Monthly usage is CPT usage and ODM real person usage", cptUsages.Length + 1, monthlyUsages.Length);

			foreach (string name in cptOnlyStaffNames)
			{
				AssertEquals("CPT usage recorded " + name, 1, cptUsages.Count(x => x.Staff.LS_FullName == name));
				AssertEquals("CPT usage recorded " + name, 1, monthlyUsages.Count(x => x.Staff.LS_FullName == name && x.LX2_ModuleCode == "ACP"));
				AssertEquals("ODM usage NOT recorded " + name, 0, monthlyUsages.Count(x => x.Staff.LS_FullName == name && x.LX2_ModuleCode == "FOR"));
			}

			foreach (string code in cptOnlyStaffCodes)
			{
				AssertEquals("CPT usage recorded " + code, 1, cptUsages.Count(x => x.Staff.LS_Code == code));
				AssertEquals("CPT usage recorded " + code, 1, monthlyUsages.Count(x => x.Staff.LS_Code == code && x.LX2_ModuleCode == "ACP"));
				AssertEquals("ODM usage NOT recorded " + code, 0, monthlyUsages.Count(x => x.Staff.LS_Code == code && x.LX2_ModuleCode == "FOR"));
			}

			foreach (string name in alwaysExcludedStaffNames)
			{
				AssertEquals("CPT usage NOT recorded " + name, 0, cptUsages.Count(x => x.Staff.LS_FullName == name));
				AssertEquals("CPT usage NOT recorded " + name, 0, monthlyUsages.Count(x => x.Staff.LS_FullName == name && x.LX2_ModuleCode == "ACP"));
				AssertEquals("ODM usage NOT recorded " + name, 0, monthlyUsages.Count(x => x.Staff.LS_FullName == name && x.LX2_ModuleCode == "FOR"));
			}

			foreach (string code in alwaysExcludedStaffCodes)
			{
				AssertEquals("CPT usage NOT recorded " + code, 0, cptUsages.Count(x => x.Staff.LS_Code == code));
				AssertEquals("CPT usage NOT recorded " + code, 0, monthlyUsages.Count(x => x.Staff.LS_Code == code && x.LX2_ModuleCode == "ACP"));
				AssertEquals("ODM usage NOT recorded " + code, 0, monthlyUsages.Count(x => x.Staff.LS_Code == code && x.LX2_ModuleCode == "FOR"));
			}

			AssertEquals(1, cptUsages.Count(x => x.Staff.LS_FullName == "Real Person"));
			AssertEquals(1, monthlyUsages.Count(x => x.Staff.LS_FullName == "Real Person" && x.LX2_ModuleCode == "ACP"));
			AssertEquals(1, monthlyUsages.Count(x => x.Staff.LS_FullName == "Real Person" && x.LX2_ModuleCode == "FOR"));
		}

		public void TestDecodeCompressedEncrypted()
		{
			var data = new LicenceConsumptionLogSchema();
			CreateCptLog("ZUBCOMSRV", data, "", "Jim", "", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));
			CreateCptLog("ZUBCOMSRV", data, "", "ĐĚƀƎ\u00a0ǣ名", "odd@test.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));

			string encryptedtext = LicenceUsageReportBuilder.GetCompressedEncryptedText(data);

			string expectedXml;
			using (MemoryStream xmlStream = new MemoryStream())
			{
				var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
				serializer.Serialize(xmlStream, data);
				xmlStream.Position = 0;
				expectedXml = Encoding.UTF8.GetString(xmlStream.ToArray());
			}

			string xml = LicenceUsageProcessor.DecodeCompressedEncrypted(encryptedtext);
			AssertEquals(expectedXml, xml);
		}

		public void TestProcessIgnoresBlankNameAndEmail()
		{
			var licence = CreateAndSaveLicence();

			var data = new LicenceConsumptionLogSchema();
			CreateCptLog(licence, data, "", "Paul Keating", "keating@test.com", "FOR", new ZDateTime(2008, 3, 5, 9, 32, 0));
			CreateCptLog(licence, data, "", "", "", "FOR", new ZDateTime(2008, 3, 6, 9, 32, 0));
			CreateCptLog(licence, data, "", "", "", "FOR", new ZDateTime(2008, 3, 7, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			var usage = usages[0];
			AssertEquals("keating@test.com", usage.Staff.LS_Email);
			AssertEquals("Paul Keating", usage.Staff.LS_FullName);

			var factory = new BusinessObjectFactory();
			OrgContact newContact = (OrgContact)factory.Load<EDIOrgHeader>(licence.Company.Header.PK).Contacts.Find(new ZQuery(OrgContactSchema.OC_Email, "keating@test.com"))[0];
			AssertEquals("Paul Keating", newContact.OC_ContactName);
			AssertEquals("keating@test.com", newContact.OC_Email);
			AssertEquals(licence.Company.Header.PK, newContact.OC_OH);
			AssertEquals(true, newContact.IsInDatabase);
		}

		public void TestWebModulesRecordedAsWebUser()
		{
			var licence = CreateAndSaveLicence();

			int initialContactCount = licence.Company.Header.Contacts.Count;

			var data = new LicenceConsumptionLogSchema();

			foreach (string moduleCode in new string[] { LegacyLicence.Codes.WebTracker
				, LegacyLicence.Codes.WebTrackerBooking
				, LegacyLicence.Codes.WebTrackerCFS
				, LegacyLicence.Codes.WebTrackerExportBrokerage
				, LegacyLicence.Codes.WebTrackerForwarding
				, LegacyLicence.Codes.WebTrackerImportBrokerage
				, LegacyLicence.Codes.WebTrackerLocalTransport
				, LegacyLicence.Codes.WebTrackerOrderManager
				, LegacyLicence.Codes.WebTrackerShippingManagerBillsOfLading
				, LegacyLicence.Codes.WebTrackerShippingManagerBookings
				, LegacyLicence.Codes.WebTrackerWarehouse })
			{
				CreateOdmLog(licence, data, "", "Test Staff" + moduleCode, moduleCode + "TestStaff@test.com", moduleCode, new ZDateTime(2008, 3, 5, 9, 32, 0));
			}

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllEdiLicenceUsage();
			AssertEquals(11, usages.Length);

			foreach (var usage in usages)
			{
				AssertEquals(User.WebUserCode, usage.Staff.LS_Code);
				AssertEquals("", usage.Staff.LS_Email);
				AssertEquals(User.WebUserName, usage.Staff.LS_FullName);
			}

			AssertEquals("no contacts created", initialContactCount, licence.Company.Header.Contacts.Count);
		}

		public void TestNameOrEmailChange()
		{
			var licence = CreateAndSaveLicence();
			int initialContactCount = licence.Company.Header.Contacts.Count;

			var data = new LicenceConsumptionLogSchema();

			var log = CreateCptLog(licence, data, "TS", "Test Staff", "TestStaff@test.com", LegacyLicence.Codes.Core, new ZDateTime(2008, 3, 5, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			foreach (var usage in usages)
			{
				AssertEquals("TS", usage.Staff.LS_Code);
				AssertEquals("TestStaff@test.com", usage.Staff.LS_Email);
				AssertEquals("Test Staff", usage.Staff.LS_FullName);
			}

			// name change
			log.StaffName = "Test Staff Junior";
			writer = new StringWriter();
			serializer.Serialize(writer, data);
			actualXml = writer.GetStringBuilder().ToString();
			processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);
			usages = LoadAllCptUsages();
			AssertEquals(2, usages.Length);

			foreach (var usage in usages)
			{
				AssertEquals("TS", usage.Staff.LS_Code);
				AssertEquals("TestStaff@test.com", usage.Staff.LS_Email);
				AssertEquals("Test Staff Junior", usage.Staff.LS_FullName);
			}

			// email change
			log.StaffEmail = "keatingjunior@test.com";
			writer = new StringWriter();
			serializer.Serialize(writer, data);
			actualXml = writer.GetStringBuilder().ToString();

			processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);
			usages = LoadAllCptUsages();
			AssertEquals(3, usages.Length);

			foreach (var usage in usages)
			{
				AssertEquals("TS", usage.Staff.LS_Code);
				AssertEquals("keatingjunior@test.com", usage.Staff.LS_Email);
				AssertEquals("Test Staff Junior", usage.Staff.LS_FullName);
			}
		}

		public void TestBranchCode()
		{
			var licence = CreateAndSaveLicence();
			int initialContactCount = licence.Company.Header.Contacts.Count;

			var data = new LicenceConsumptionLogSchema();

			var log = CreateCptLog(licence, data, "TS", "Test Staff", "TestStaff@test.com", LegacyLicence.Codes.Core, new ZDateTime(2008, 3, 5, 9, 32, 0));
			log.BranchCode = "BR1";

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			foreach (var usage in usages)
			{
				AssertEquals("BR1", usage.LX_Branch);
			}
		}

		public void TestStaffCode()
		{
			var licence = CreateAndSaveLicence();
			int initialContactCount = licence.Company.Header.Contacts.Count;

			var data = new LicenceConsumptionLogSchema();

			var log = CreateCptLog(licence, data, "", "Test Staff", "TestStaff@test.com", LegacyLicence.Codes.Core, new ZDateTime(2008, 3, 5, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			foreach (var usage in usages)
			{
				AssertEquals("", usage.Staff.LS_Code);
				AssertEquals("TestStaff@test.com", usage.Staff.LS_Email);
				AssertEquals("Test Staff", usage.Staff.LS_FullName);
			}

			// staff code included
			log.StaffCode = "TS";
			writer = new StringWriter();
			serializer.Serialize(writer, data);
			actualXml = writer.GetStringBuilder().ToString();
			processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);
			usages = LoadAllCptUsages();
			AssertEquals(2, usages.Length);

			foreach (var usage in usages)
			{
				AssertEquals("TS", usage.Staff.LS_Code);
				AssertEquals("TestStaff@test.com", usage.Staff.LS_Email);
				AssertEquals("Test Staff", usage.Staff.LS_FullName);
			}

			// new staff, same name, different code, before 2014-3-1
			log.StaffCode = "T2";
			log.UsageTime = new ZDateTime(2014, 3, 1).AddMinutes(-2);
			writer = new StringWriter();
			serializer.Serialize(writer, data);
			actualXml = writer.GetStringBuilder().ToString();
			processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);
			usages = LoadAllCptUsages();
			AssertEquals(3, usages.Length);
			AssertEquals(3, usages.Count(x => x.Staff.LS_Code == "TS"));

			// new staff, same name, different code, on 2014-3-1
			log.UsageTime = new ZDateTime(2014, 3, 1);
			writer = new StringWriter();
			serializer.Serialize(writer, data);
			actualXml = writer.GetStringBuilder().ToString();
			processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);
			usages = LoadAllCptUsages();

			AssertEquals(4, usages.Length);
			AssertEquals(3, usages.Count(x => x.Staff.LS_Code == "TS"));
			AssertEquals(1, usages.Count(x => x.Staff.LS_Code == "T2"));
		}

		public void TestClientCompanyMatchesCodeOverOrg()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "SRV", false);
			var clientCompanyMatchingCode = BillingTestHelper.CreateClientCompany(lic.Database, "CO1");
			var clientCompanyMatchingOrg = BillingTestHelper.CreateClientCompany(lic.Database, "CO2");
			clientCompanyMatchingOrg.LCC_OH = lic.Company.LC_OH;

			Factory.Save();

			var data = new LicenceConsumptionLogSchema();
			CreateCptLog(lic, data, "", "ENT Name", "", "COR", new ZDateTime(2017, 12, 7, 9, 32, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			AssertEquals(1, usages.Length);
			AssertEquals(clientCompanyMatchingCode.PK, usages[0].LX_LCC);
		}

		public void TestProcessStaffCodeLowerCaseWithEmailChange()
		{
			Factory.RefreshEnabled = false;
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "SRV", true);
			var staff = Factory.New<ClientStaff>();
			staff.LS_Code = "FMS";
			staff.LS_LD = lic.LA_LD;
			staff.LS_FullName = "John Doe";
			staff.LS_Email = "jdoe@test.com";
			Factory.Save();

			var data = new LicenceConsumptionLogSchema();

			CreateCptLog(lic, data, "fms", "John Doe", "mynewemail@test.com", LegacyLicence.Codes.Core, new ZDateTime(2018, 5, 17, 14, 58, 0));

			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			var writer = new StringWriter();
			serializer.Serialize(writer, data);
			var actualXml = writer.GetStringBuilder().ToString();

			var processor = new LicenceUsageProcessor(null);
			processor.Process(actualXml);

			var usages = LoadAllCptUsages();
			var staffReloaded = new BusinessObjectFactory().Load<ClientStaff>(staff.PK);
			AssertEquals(1, usages.Length);
			AssertEquals("mynewemail@test.com", staffReloaded.LS_Email);
		}

		#region Implementation

		class LicenceUsageProcessorForTest : LicenceUsageProcessor
		{
			internal BusinessObjectFactory ContactFactory;

			public LicenceUsageProcessorForTest(ILogger serviceLogger)
				: base(serviceLogger)
			{
			}

			protected override BusinessObjectFactory CreateContactFactory()
			{
				ContactFactory = base.CreateContactFactory();
				Factory_Exposed = Factory;
				return ContactFactory;
			}

			public BusinessObjectFactory Factory_Exposed { get; private set; }
		}

		class LicenceUsageProcessorForConcurrencyTest : LicenceUsageProcessor
		{
			public LicenceUsageProcessorForConcurrencyTest(ZString staffName, ZGuid licenceDatabasePK, ILogger serviceLogger)
				: base(serviceLogger)
			{
				StaffName = staffName;
				LicenceDatabasePK = licenceDatabasePK;
			}

			readonly ZString StaffName;
			readonly ZGuid LicenceDatabasePK;
			bool isSaveConcurrencyTriggered;

			protected override void ProcessSave()
			{
				if (!isSaveConcurrencyTriggered)
				{
					var factory = new BusinessObjectFactory();
					var duplicateStaff = factory.New<ClientStaff>();
					duplicateStaff.LS_FullName = StaffName;
					duplicateStaff.LS_LD = LicenceDatabasePK;
					factory.Save();
					isSaveConcurrencyTriggered = true;
				}
				base.ProcessSave();
			}
		}

		protected override void TearDown()
		{
			ReportProcessorHelper.ClearReportsFromTesting("LicenceUsage");
			base.TearDown();
		}

		LicenceHeader CreateAndSaveLicence()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ZUB", "COM", "SRV", false);
			var contact = lic.Company.Header.Contacts[0];
			contact.OC_Email = "zubs@zubs.com";
			contact.OC_ContactName = "Zubs";
			Factory.Save();
			return lic;
		}

		#endregion
	}
}
