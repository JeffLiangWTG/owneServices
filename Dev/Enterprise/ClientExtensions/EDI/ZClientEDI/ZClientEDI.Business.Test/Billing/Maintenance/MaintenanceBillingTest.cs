using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceBilling))]
	public class MaintenanceBillingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGenerate()
		{
			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "AAA");
			LicenceHeader licHeader1b = BillingTestHelper.CreateAnotherDatabase(licHeader1a, "BBB");
			LicenceCompany company1 = licHeader1a.Company;

			LicenceHeader licHeader2a = BillingTestHelper.CreateLicence(Factory, "YYY");
			LicenceCompany company2 = licHeader2a.Company;

			LicenceHeader licHeaderMissingBilling = BillingTestHelper.CreateLicence(Factory, "BAD");
			LicenceCompany companyMissingBilling = licHeaderMissingBilling.Company;

			ConfigureMaintenanceLicence(licHeader1a);
			ConfigureMaintenanceLicence(licHeader1b);
			ConfigureMaintenanceLicence(licHeader2a);
			ConfigureMaintenanceLicence(licHeaderMissingBilling);
			companyMissingBilling.InvoiceDeliveries.DeleteAll();

			// maintenance billing for the second database is to company2
			company1.InvoiceDeliveries[0].L9_OH_InvoiceTo = company2.LC_OH;

			Factory.Save();

			var maintenanceBilling = new MaintenanceBilling(Factory);
			maintenanceBilling.Filter.DueDateFrom = ZDateTime.Empty;
			maintenanceBilling.Filter.DueDateTo = ZDateTime.Empty;
			maintenanceBilling.Generate(null);

			AssertEquals("bill count", 4, maintenanceBilling.Bills.Count);
			MaintenanceBill bill1 = maintenanceBilling.Bills.Cast<MaintenanceBill>().First(s => s.LicHeader.PK == licHeader1a.PK);
			MaintenanceBill bill2 = maintenanceBilling.Bills.Cast<MaintenanceBill>().First(s => s.LicHeader.PK == licHeader1b.PK);
			MaintenanceBill bill3 = maintenanceBilling.Bills.Cast<MaintenanceBill>().First(s => s.LicHeader.PK == licHeader2a.PK);
			MaintenanceBill bill4 = maintenanceBilling.Bills.Cast<MaintenanceBill>().First(s => s.LicHeader.PK == licHeaderMissingBilling.PK);
		}

		[ExpectNoExceptions]
		public void TestGenerateWithInvalidStartAndEndDate()
		{
			var maintenanceBilling = new MaintenanceBilling(Factory);
			maintenanceBilling.Filter.DueDateFrom = new ZDateTime(" ");
			maintenanceBilling.Filter.DueDateTo = new ZDateTime(" ");
			maintenanceBilling.Generate(null);
		}

		public void TestGenerate_CombinedInvoice()
		{
			LicenceHeader licParent = CreateMaintenanceLicence(Factory, "AAA");
			LicenceHeader licChild1 = CreateMaintenanceLicence(Factory, "BBB");
			LicenceHeader licChild2 = CreateMaintenanceLicence(Factory, "CCC");
			licChild1.Company.InvoiceDeliveries[0].L9_OH_InvoiceTo = licParent.Company.LC_OH;
			licChild2.Company.InvoiceDeliveries[0].L9_OH_InvoiceTo = licParent.Company.LC_OH;

			var prices = CreateMaintenancePriceList(licParent, 1000m);

			Factory.Save();

			var maintenanceBilling = new MaintenanceBilling(Factory);
			maintenanceBilling.Filter.DueDateFrom = ZDateTime.Empty;
			maintenanceBilling.Filter.DueDateTo = ZDateTime.Empty;
			maintenanceBilling.Generate(null);
			AssertEquals("bill count", 3, maintenanceBilling.Bills.Count);
			AssertEquals("invoice count", 1, maintenanceBilling.Recipients.Count);
		}

		public void TestGenerate_Fees()
		{
			LicenceHeader licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			LicenceHeader licHeader2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var fee1a = AddFee(licHeader1.Company, 2000m, "Fee 1a");
			var fee1b = AddFee(licHeader1.Company, 500m, "Fee 1b");
			var fee2a = AddFee(licHeader2.Company, 11m, "Fee 2a");
			var fee2b = AddFee(licHeader2.Company, 7m, "Fee 2b");

			var nonMaintenanceFee = AddFee(licHeader2.Company, 7m, "Fee X1");
			nonMaintenanceFee.L8_SystemCode = BillingConstants.BillingSystem.ODM;

			var expiredFee = AddFee(licHeader2.Company, 7m, "Fee X2");
			expiredFee.L8_EndDate = new ZDateTime(2010, 6, 30);
			var expiredWithinDueDateRangeFee = AddFee(licHeader2.Company, 7m, "Fee X3");
			expiredWithinDueDateRangeFee.L8_EndDate = new ZDateTime(2010, 11, 30);

			Factory.Save();

			var maintenanceBilling = new MaintenanceBilling(Factory);
			maintenanceBilling.Filter.DueDateFrom = new ZDateTime(2009, 1, 1);
			maintenanceBilling.Filter.DueDateTo = new ZDateTime(2009, 12, 31);
			maintenanceBilling.Generate(null);
			AssertEquals("no fees in date range", 0, maintenanceBilling.Recipients.Count);

			maintenanceBilling.Filter.DueDateFrom = new ZDateTime(2010, 11, 1);
			maintenanceBilling.Filter.DueDateTo = new ZDateTime(2011, 2, 1);
			maintenanceBilling.Generate(null);
			AssertEquals("fee recipient count", 2, maintenanceBilling.Recipients.Count);
			MaintenanceBillRecipient recipient1 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader1.LA_LC);
			MaintenanceBillRecipient recipient2 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader2.LA_LC);
			AssertEquals("fee count 1", 2, recipient1.Fees.Count);
			AssertEquals("fee count 2", 2, recipient2.Fees.Count);
			AssertEquals("due date", new ZDateTime(2011, 1, 1), recipient1.DueDate);
			AssertEquals("due date", new ZDateTime(2011, 1, 1), recipient2.DueDate);

			AssertEquals("fee1a present", fee1a.PK, recipient1.Fees[0].Fee.PK);
			AssertEquals("fee1b present", fee1b.PK, recipient1.Fees[1].Fee.PK);
			AssertEquals("fee2a present", fee2a.PK, recipient2.Fees[0].Fee.PK);
			AssertEquals("fee2b present", fee2b.PK, recipient2.Fees[1].Fee.PK);

			AssertEquals(new ZDateTime(2011, 1, 1), recipient1.DueDate);

			maintenanceBilling.Filter.DueDateFrom = new ZDateTime(2011, 1, 2);
			maintenanceBilling.Filter.DueDateTo = ZDateTime.Empty;
			maintenanceBilling.Generate(null);
			AssertEquals("fee recipient count", 2, maintenanceBilling.Recipients.Count);
			recipient1 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader1.LA_LC);
			recipient2 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader2.LA_LC);
			AssertEquals("fee count 1", 2, recipient1.Fees.Count);
			AssertEquals("fee count 2", 2, recipient2.Fees.Count);
			AssertEquals("due date", new ZDateTime(2012, 1, 1), recipient1.DueDate);
			AssertEquals("due date", new ZDateTime(2012, 1, 1), recipient2.DueDate);
		}

		public void TestGenerate_Fees_PayingOrgNotLicenced()
		{
			var orgWithoutLicence = Factory.NewWithValidTestData<OrgHeader>();
			orgWithoutLicence.CompanyData.OB_IsDebtor = true;
			LicenceHeader lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var fee1 = AddFee(lic1.Company, 2000m, "Fee 1a");
			fee1.L8_LD = lic1.LA_LD;
			var delivery = lic1.Company.InvoiceDeliveries.Count == 0 ? lic1.Company.InvoiceDeliveries.AddNew() : lic1.Company.InvoiceDeliveries[0];
			delivery.L9_GB_InvoicingBranch = Env.CurrentBranchPK;
			delivery.L9_RX_NKInvoiceCurrency = "AUD";
			delivery.L9_OH_InvoiceTo = orgWithoutLicence.PK;

			Factory.Save();

			var maintenanceBilling = new MaintenanceBilling(Factory);

			maintenanceBilling.Filter.DueDateFrom = new ZDateTime(2010, 11, 1);
			maintenanceBilling.Filter.DueDateTo = new ZDateTime(2011, 2, 1);
			maintenanceBilling.Generate(null);
			AssertEquals("fee recipient count", 1, maintenanceBilling.Recipients.Count);
			var recipient1 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.OrganisationPK == orgWithoutLicence.PK);
			AssertEquals("fee count 1", 1, recipient1.Fees.Count);
			AssertEquals("due date", new ZDateTime(2011, 1, 1), recipient1.DueDate);

			AssertEquals("fee1 present", fee1.PK, recipient1.Fees[0].Fee.PK);

			AssertEquals(new ZDateTime(2011, 1, 1), recipient1.DueDate);
		}

		public void TestGenerate_Fees_IncludeLoginCompanyOnly()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.Company.GC_RN_NKCountryCode = "US";
			Factory.Save();

			using (branch1.SetAsTemporaryContext())
			{
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				BillingTestHelper.CreateChargeCodeForCompany(factory2, branch1.GB_GC, null, "USFEE");
				factory2.Save();
			}

			LicenceHeader licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var fee1a = AddFee(licHeader1.Company, 2000m, "Fee 1a");
			fee1a.L8_ChargeCode = "USFEE";
			var fee1b = AddFee(licHeader1.Company, 500m, "Fee 1b");
			BillingTestHelper.SetInvoicing(licHeader1, branch1.PK);

			LicenceHeader licHeader2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var fee2a = AddFee(licHeader2.Company, 11m, "Fee 2a");
			BillingTestHelper.SetInvoicing(licHeader2, Env.CurrentBranchPK);

			Factory.Save();

			// IncludeLoginCompanyOnly = true, current login company
			{
				var maintenanceBilling = new MaintenanceBilling(new BusinessObjectFactory() { RefreshEnabled = false });
				maintenanceBilling.Filter.DueDateFrom = new ZDateTime(2010, 11, 1);
				maintenanceBilling.Filter.DueDateTo = new ZDateTime(2011, 2, 1);
				maintenanceBilling.Filter.IncludeLoginCompanyInvoicesOnly = true;
				maintenanceBilling.Generate(null);
				AssertEquals("fee recipient count", 1, maintenanceBilling.Recipients.Count);
				var recipient1 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader2.LA_LC);
				AssertEquals("fee count 1", 1, recipient1.Fees.Count);
				AssertEquals("fee2a present", fee2a.PK, recipient1.Fees[0].Fee.PK);
			}

			// IncludeLoginCompanyOnly = true, other login company
			using (branch1.SetAsTemporaryContext())
			{
				var maintenanceBilling = new MaintenanceBilling(new BusinessObjectFactory() { RefreshEnabled = false });
				maintenanceBilling.Filter.DueDateFrom = new ZDateTime(2010, 11, 1);
				maintenanceBilling.Filter.DueDateTo = new ZDateTime(2011, 2, 1);
				maintenanceBilling.Filter.IncludeLoginCompanyInvoicesOnly = true;
				maintenanceBilling.Generate(null);
				AssertEquals("fee recipient count", 1, maintenanceBilling.Recipients.Count);
				var recipient1 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader1.LA_LC);
				AssertEquals("fee count 2", 2, recipient1.Fees.Count);
				AssertEquals("fee1a present", fee1a.PK, recipient1.Fees[0].Fee.PK);
				AssertEquals("fee1b present", fee1b.PK, recipient1.Fees[1].Fee.PK);
			}

			// IncludeLoginCompanysOnly = false
			{
				var maintenanceBilling = new MaintenanceBilling(new BusinessObjectFactory() { RefreshEnabled = false });
				maintenanceBilling.Filter.DueDateFrom = new ZDateTime(2010, 11, 1);
				maintenanceBilling.Filter.DueDateTo = new ZDateTime(2011, 2, 1);
				maintenanceBilling.Filter.IncludeLoginCompanyInvoicesOnly = false;
				maintenanceBilling.Generate(null);
				AssertEquals("fee recipient count", 2, maintenanceBilling.Recipients.Count);
				MaintenanceBillRecipient recipient1 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader1.LA_LC);
				MaintenanceBillRecipient recipient2 = maintenanceBilling.Recipients.Cast<MaintenanceBillRecipient>().FirstOrDefault(s => s.LicCompany.PK == licHeader2.LA_LC);
				AssertEquals("fee count 1", 2, recipient1.Fees.Count);
				AssertEquals("fee count 2", 1, recipient2.Fees.Count);
				AssertEquals("fee1a present", fee1a.PK, recipient1.Fees[0].Fee.PK);
				AssertEquals("fee1b present", fee1b.PK, recipient1.Fees[1].Fee.PK);
				AssertEquals("fee2a present", fee2a.PK, recipient2.Fees[0].Fee.PK);
			}
		}

		ClientLicenceFee AddFee(LicenceCompany company, ZDecimal amount, ZString description)
		{
			return BillingTestHelper.CreateMaintenanceFee(company, description, amount, "UPGASS");
		}

		public void TestGenerate_PerDatabaseFeeBasis()
		{
			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "AAA");
			LicenceHeader licHeader1b = BillingTestHelper.CreateAnotherLicence(licHeader1a, "BBB");
			LicenceCompany company1 = licHeader1a.Company;

			LicenceHeader licHeader2a = BillingTestHelper.CreateLicence(Factory, "YYY");
			LicenceCompany company2 = licHeader2a.Company;

			ConfigureMaintenanceLicence(licHeader1a);
			ConfigureMaintenanceLicence(licHeader1b);
			ConfigureMaintenanceLicence(licHeader2a);

			var forwarder1a = licHeader1a.Modules.FindByCode(Env.Licence.Forwarder.Name);
			var forwarder1b = licHeader1b.Modules.FindByCode(Env.Licence.Forwarder.Name);
			var accountant1b = licHeader1b.Modules.FindByCode(Env.Licence.Accountant.Name);
			var accountant2a = licHeader2a.Modules.FindByCode(Env.Licence.Accountant.Name);
			forwarder1a.LM_LicenceType = LicenceTypes.Codes.PUR;
			forwarder1b.LM_LicenceType = LicenceTypes.Codes.PUR;
			accountant1b.LM_LicenceType = LicenceTypes.Codes.PUR;
			accountant2a.LM_LicenceType = LicenceTypes.Codes.PUR;
			forwarder1a.LM_UserCount = 1;
			forwarder1b.LM_UserCount = 1;
			accountant1b.LM_UserCount = 1;
			accountant2a.LM_UserCount = 1;
			ZDecimal corePrice = 100m;
			var prices1a = CreateMaintenancePriceList(licHeader1a, corePrice);
			var prices1b = CreateMaintenancePriceList(licHeader1b, corePrice);
			var prices2a = CreateMaintenancePriceList(licHeader2a, corePrice);
			BillingTestHelper.AddPriceItem(prices1a, forwarder1a.LM_GroupModuleCode, BillingConstants.FeeType.Database, "", 990);
			BillingTestHelper.AddPriceItem(prices1b, accountant1b.LM_GroupModuleCode, BillingConstants.FeeType.Database, "", 500);
			BillingTestHelper.AddPriceItem(prices2a, accountant2a.LM_GroupModuleCode, BillingConstants.FeeType.Database, "", 300);

			Factory.Save();

			var maintenanceBilling = new MaintenanceBilling(Factory);
			maintenanceBilling.Filter.DueDateFrom = ZDateTime.Empty;
			maintenanceBilling.Filter.DueDateTo = ZDateTime.Empty;
			maintenanceBilling.Generate(null);

			AssertEquals("bill count", 3, maintenanceBilling.Bills.Count);
			MaintenanceBill bill1 = maintenanceBilling.Bills.Cast<MaintenanceBill>().First(s => s.LicHeader.PK == licHeader1a.PK);
			MaintenanceBill bill2 = maintenanceBilling.Bills.Cast<MaintenanceBill>().First(s => s.LicHeader.PK == licHeader1b.PK);
			MaintenanceBill bill3 = maintenanceBilling.Bills.Cast<MaintenanceBill>().First(s => s.LicHeader.PK == licHeader2a.PK);

			ZDecimal coreMaintenance = licHeader1a.GetCoreModule().LM_UserCount * corePrice * 0.3m;
			AssertEquals("bill1 has maintenance for chinese only", coreMaintenance + 990 * 0.3m, bill1.Maintenance);
			AssertEquals("bill2 has maintenance for french only", coreMaintenance + 500 * 0.3m, bill2.Maintenance);
			AssertEquals("bill3 has maintenance for french only", coreMaintenance + 300 * 0.3m, bill3.Maintenance);
		}

		public void TestSave()
		{
			LicenceHeader licHeader1a = CreateMaintenanceLicence(Factory, "AAA");

			Factory.Save();

			var maintenanceBilling = new MaintenanceBilling(Factory);
			maintenanceBilling.Filter.DueDateFrom = ZDateTime.Empty;
			maintenanceBilling.Filter.DueDateTo = ZDateTime.Empty;
			maintenanceBilling.Generate(null);
			AssertEquals("bill count", 1, maintenanceBilling.Bills.Count);

			MaintenanceBill bill = maintenanceBilling.Bills[0];
			bill.LicHeader.Billing.L0_NextMaintenancePercent = 29;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			LicenceHeader headerReloaded = factory2.Load<LicenceHeader>(licHeader1a.PK);
			AssertEquals("no change yet", 25m, headerReloaded.ReadonlyBilling.L0_NextMaintenancePercent);

			maintenanceBilling.BillsFactory.Save();

			factory2 = new BusinessObjectFactory();
			headerReloaded = factory2.Load<LicenceHeader>(licHeader1a.PK);
			AssertEquals("changed", 29m, headerReloaded.ReadonlyBilling.L0_NextMaintenancePercent);
		}

		public void TestCreateInvoice()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.Company.GC_RN_NKCountryCode = "AU";
			branch2.Company.GC_RN_NKCountryCode = "US";
			Factory.Save();

			LicenceHeader licHeader1 = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			LicenceHeader licHeader2 = BillingTestHelper.CreateMaintenanceLicence(Factory, "BBB");
			LicenceHeader licHeader3 = BillingTestHelper.CreateMaintenanceLicence(Factory, "CCC");
			LicenceHeader licHeader4 = BillingTestHelper.CreateMaintenanceLicence(Factory, "CCC");

			MaintenanceBillRecipientForTest recipient1;
			MaintenanceBillRecipientForTest recipient2;
			MaintenanceBillRecipientForTest recipient3;
			MaintenanceBillRecipientForTest recipient4;
			using (branch1.SetAsTemporaryContext())
			{
				recipient1 = new MaintenanceBillRecipientForTest(Factory, branch1.PK, licHeader1.Company.LC_OH, "AUD", ZDateTime.Empty);
				recipient2 = new MaintenanceBillRecipientForTest(Factory, branch1.PK, licHeader2.Company.LC_OH, "AUD", ZDateTime.Empty);
			}
			using (branch2.SetAsTemporaryContext())
			{
				recipient3 = new MaintenanceBillRecipientForTest(Factory, branch2.PK, licHeader3.Company.LC_OH, "AUD", ZDateTime.Empty);
				recipient4 = new MaintenanceBillRecipientForTest(Factory, branch2.PK, licHeader4.Company.LC_OH, "AUD", ZDateTime.Empty);
			}

			var maintenanceBilling = new MaintenanceBilling(Factory);

			AssertEquals("pre", 0, recipient1.CreateInvoiceCalls);
			AssertEquals("pre", 0, recipient2.CreateInvoiceCalls);
			AssertEquals("pre", 0, recipient3.CreateInvoiceCalls);
			AssertEquals("pre", 0, recipient4.CreateInvoiceCalls);

			maintenanceBilling.CreateInvoices(new MaintenanceBillRecipient[] { recipient1, recipient3 }, null);

			AssertEquals("invoice", 1, recipient1.CreateInvoiceCalls);
			AssertEquals("invoice", 0, recipient2.CreateInvoiceCalls);
			AssertEquals("invoice", 1, recipient3.CreateInvoiceCalls);
			AssertEquals("invoice", 0, recipient4.CreateInvoiceCalls);

			AssertEquals("no attachment", 0, recipient1.Attachments.Length);

			EDIDataRegistry.Instance.InvoiceAttachmentDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "INV");
			string attachmentFolder = Path.Combine(Env.TempPath, "TempAttach");
			Directory.CreateDirectory(attachmentFolder);
			string genericClientFolder = Path.Combine(attachmentFolder, MaintenanceBilling.ClientAttachmentFolderName);
			Directory.CreateDirectory(genericClientFolder);

			try
			{
				maintenanceBilling.AttachmentFolder = attachmentFolder;
				string usFolder = Path.Combine(attachmentFolder, "US");
				Directory.CreateDirectory(usFolder);
				string usClientFolder = Path.Combine(usFolder, MaintenanceBilling.ClientAttachmentFolderName);
				Directory.CreateDirectory(usClientFolder);
				File.WriteAllBytes(Path.Combine(attachmentFolder, "file1.txt"), Encoding.ASCII.GetBytes("hello world"));
				File.WriteAllBytes(Path.Combine(attachmentFolder, "file2.txt"), Encoding.ASCII.GetBytes("bye world"));
				File.WriteAllBytes(Path.Combine(usFolder, "USfile1.txt"), Encoding.ASCII.GetBytes("hello US"));
				File.WriteAllBytes(Path.Combine(usFolder, "USfile2.txt"), Encoding.ASCII.GetBytes("bye US"));
				File.WriteAllBytes(Path.Combine(usClientFolder, recipient4.OrganisationCode + " Client4a.txt"), Encoding.ASCII.GetBytes("hello recipient4 from US"));
				File.WriteAllBytes(Path.Combine(genericClientFolder, recipient4.OrganisationCode + " Client4b.txt"), Encoding.ASCII.GetBytes("hello recipient4 from the world"));

				File.WriteAllBytes(Path.Combine(usClientFolder, recipient2.OrganisationCode + " Client2.txt"), Encoding.ASCII.GetBytes("recipient2 is not billed from US"));

				maintenanceBilling.CreateInvoices(new MaintenanceBillRecipient[] { recipient2, recipient4 }, null);

				AssertEquals(2, recipient2.Attachments.Length);
				AssertEquals(4, recipient4.Attachments.Length);

				var attach2a = recipient2.Attachments.First(s => s.Key == "file1.txt");
				var attach2b = recipient2.Attachments.First(s => s.Key == "file2.txt");
				var attach4a = recipient4.Attachments.First(s => s.Key == "USfile1.txt");
				var attach4b = recipient4.Attachments.First(s => s.Key == "USfile2.txt");
				var attach4c = recipient4.Attachments.First(s => s.Key == recipient4.OrganisationCode + " Client4a.txt");
				var attach4d = recipient4.Attachments.First(s => s.Key == recipient4.OrganisationCode + " Client4b.txt");

				AssertEquals("hello world", Encoding.ASCII.GetString(attach2a.Value));
				AssertEquals("bye world", Encoding.ASCII.GetString(attach2b.Value));
				AssertEquals("hello US", Encoding.ASCII.GetString(attach4a.Value));
				AssertEquals("bye US", Encoding.ASCII.GetString(attach4b.Value));
				AssertEquals("hello recipient4 from US", Encoding.ASCII.GetString(attach4c.Value));
				AssertEquals("hello recipient4 from the world", Encoding.ASCII.GetString(attach4d.Value));
			}
			finally
			{
				Directory.Delete(attachmentFolder, true);
				maintenanceBilling.AttachmentFolder = ZString.Empty;
			}
		}

		public void TestAttachmentFolder()
		{
			var maintenanceBilling = new MaintenanceBilling(Factory);
			AssertNoErrors(maintenanceBilling.AttachmentFolderInfo);

			maintenanceBilling.AttachmentFolder = @"c:>;";
			AssertHasError(maintenanceBilling.AttachmentFolderInfo, "Folder not found.");

			EDIDataRegistry.Instance.InvoiceAttachmentDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ZZ2");
			maintenanceBilling.AttachmentFolder = TestCase.ExecutableDirectory;
			AssertHasError(maintenanceBilling.AttachmentFolderInfo, "Doc Type not found in registry " + ((IRegistryItemInternals)EDIDataRegistry.Instance.InvoiceAttachmentDocType).Location);

			EDIDataRegistry.Instance.InvoiceAttachmentDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "INV");
			maintenanceBilling.ValidateAttachmentFolder();
			AssertNoErrors(maintenanceBilling.AttachmentFolderInfo);

			maintenanceBilling.AttachmentFolder = ZString.Empty;
		}

		public void TestShowPerModuleAmounts()
		{
			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "AAA");
			LicenceCompany company1 = licHeader1a.Company;

			LicenceHeader licHeader2a = BillingTestHelper.CreateLicence(Factory, "YYY");
			LicenceCompany company2 = licHeader2a.Company;

			ConfigureMaintenanceLicence(licHeader1a);
			ConfigureMaintenanceLicence(licHeader2a);

			Factory.Save();

			var maintenanceBilling = new MaintenanceBilling(Factory);
			maintenanceBilling.Filter.DueDateFrom = ZDateTime.Empty;
			maintenanceBilling.Filter.DueDateTo = ZDateTime.Empty;
			maintenanceBilling.Generate(null);

			AssertEquals("bill count", 2, maintenanceBilling.Bills.Count);
			AssertEquals("bill count", 2, maintenanceBilling.Recipients.Count);

			maintenanceBilling.ShowPerModuleAmounts = true;
			AssertEquals(true, maintenanceBilling.Recipients[0].ShowPerModuleAmounts);
			AssertEquals(true, maintenanceBilling.Recipients[1].ShowPerModuleAmounts);

			maintenanceBilling.ShowPerModuleAmounts = false;
			AssertEquals(false, maintenanceBilling.Recipients[0].ShowPerModuleAmounts);
			AssertEquals(false, maintenanceBilling.Recipients[1].ShowPerModuleAmounts);

			maintenanceBilling.Recipients[1].ShowPerModuleAmounts = true;
			AssertEquals(false, maintenanceBilling.ShowPerModuleAmounts.HasValue);

			maintenanceBilling.Recipients[0].ShowPerModuleAmounts = true;
			AssertEquals(true, maintenanceBilling.ShowPerModuleAmounts);

			maintenanceBilling = new MaintenanceBilling(Factory);
			AssertEquals(false, maintenanceBilling.ShowPerModuleAmounts);

			maintenanceBilling.ShowPerModuleAmounts = true;
			AssertEquals(true, maintenanceBilling.ShowPerModuleAmounts);

			maintenanceBilling.ShowPerModuleAmounts = null;
			AssertEquals(false, maintenanceBilling.ShowPerModuleAmounts.HasValue);
		}

		public void TestCreateInvoices_UsesSingleInstanceOfUSSalesTaxCalculator()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.Company.GC_RN_NKCountryCode = "AU";
			branch2.Company.GC_RN_NKCountryCode = "US";

			LicenceHeader licHeader1 = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			LicenceHeader licHeader2 = BillingTestHelper.CreateMaintenanceLicence(Factory, "BBB");
			LicenceHeader licHeader3 = BillingTestHelper.CreateMaintenanceLicence(Factory, "CCC");
			Factory.Save();

			MaintenanceBillRecipient recipient1;
			MaintenanceBillRecipient recipient2;
			MaintenanceBillRecipient recipient3;
			using (branch1.SetAsTemporaryContext())
			{
				recipient1 = new MaintenanceBillRecipient(Factory, branch1.PK, licHeader1.Company.LC_OH, "AUD", ZDate.Today, ZDateTime.Empty);
				recipient2 = new MaintenanceBillRecipient(Factory, branch1.PK, licHeader2.Company.LC_OH, "AUD", ZDate.Today, ZDateTime.Empty);
			}
			using (branch2.SetAsTemporaryContext())
			{
				recipient3 = new MaintenanceBillRecipient(Factory, branch2.PK, licHeader3.Company.LC_OH, "AUD", ZDate.Today, ZDateTime.Empty);
			}

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			mockCalculator.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculator.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			var calculationResult = new CalculationResult(5m);
			mockCalculator.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculator.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			var mockCalculatorFactory = new Mock<IUSSalesTaxCalculatorFactory>();
			mockCalculatorFactory.Setup(x => x.Get()).Returns(mockCalculator.Object);

			var factoryList = (System.Collections.ArrayList)CargoWise.Application.ObjectFactory.Get("IUSSalesTaxCalculator_ClientSpecific");
			var oldFactory = factoryList[0];
			factoryList.Clear();
			factoryList.Add(mockCalculatorFactory.Object);
			try
			{
				var beforeCount = Factory.GetDatabaseCount(typeof(AccTransactionHeader));
				AssertEquals("Precondition: no invoices", 0, beforeCount);

				var maintenanceBilling = new MaintenanceBilling(Factory);
				maintenanceBilling.CreateInvoices(new MaintenanceBillRecipient[] { recipient1, recipient2, recipient3 }, null);

				var count = Factory.CreateNewFactory().GetDatabaseCount(typeof(AccTransactionHeader));
				AssertEquals("Three invoices should be created", 3, count);

				mockCalculatorFactory.Verify(x => x.Get(), Times.Once(), "One USSalesTaxCalculator should be created, and re-used for all invoices across any branch");
				mockCalculator.Verify(x => x.SetSalesTaxLineItem(It.IsAny<InvoicingBase>(), It.IsAny<decimal>()), Times.Exactly(3), "Sales tax line items should be added for each invoice");
				mockCalculator.Verify(x => x.Dispose(), Times.Once(), "Dispose should be called once, after all invoices are processed");
			}
			finally
			{
				factoryList.Clear();
				factoryList.Add(oldFactory);
			}
		}

		ClientLicencePriceHeader CreateMaintenancePriceList(LicenceHeader licHeader, ZDecimal corePrice)
		{
			var prices = licHeader.Company.CreatePriceList(false);
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			prices.L6_ValidFrom = ZDateTime.Now.AddMonths(-2);
			prices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.AddPriceItem(prices, "COR", BillingConstants.FeeType.NamedUser, "", corePrice);
			return prices;
		}

		LicenceHeader CreateMaintenanceLicence(BusinessObjectFactory factory, string code)
		{
			return BillingTestHelper.CreateMaintenanceLicence(factory, code);
		}

		void ConfigureMaintenanceLicence(LicenceHeader licence)
		{
			BillingTestHelper.ConfigureMaintenanceLicence(licence);
		}

		class MaintenanceBillRecipientForTest : MaintenanceBillRecipient
		{
			public MaintenanceBillRecipientForTest(BusinessObjectFactory factory, ZGuid branchPK, ZGuid orgPK, ZString currencyCode, ZDateTime dueDate)
				: base(factory, branchPK, orgPK, currencyCode, dueDate)
			{
			}

			public override IEnumerable<ARInvoice> CreateInvoices(KeyValuePair<string, byte[]>[] attachments, IUSSalesTaxCalculator usSalesTaxCalculator = null)
			{
				AssertEquals("branch", Env.CurrentBranch.PK, BranchPK);
				++CreateInvoiceCalls;
				Attachments = attachments;
				return null;
			}

			public KeyValuePair<string, byte[]>[] Attachments;

			public int CreateInvoiceCalls;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BillingTestHelper.CreateChargeCode(Factory, null, "ANNMAINT");
			BillingTestHelper.CreateChargeCode(Factory, null, "UPGASS");
			var chargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, null, EDIDataRegistry.Instance.CommentChargeCode.Value);
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();
		}

		#endregion
	}
}
