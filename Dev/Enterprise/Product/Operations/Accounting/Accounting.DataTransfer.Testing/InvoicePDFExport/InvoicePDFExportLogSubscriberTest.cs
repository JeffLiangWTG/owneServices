using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.InvoicePDFExport.Testing
{
	[TestedType(typeof(InvoicePDFExportLogSubscriber))]
	public class InvoicePDFExportLogSubscriberTest : LogSubscriberTest<InvoicePDFExportLogSubscriber>
	{
		public void TestRelativeCompanyHasNoActiveBranch()
		{
			var differentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var branch = differentCompany.Branches.AddNew();
			branch.GB_Code = "~01";
			branch.GB_BranchName = "Test Branch";

			var organisation = TestObjectCreator.CreateOrgHeader("ABC", false, false, "AUSYD");
			var orgAddress = TestObjectCreator.CreateAddress(organisation);
			orgAddress.AddAddressType(OrgAddressType.Receivables);
			differentCompany.GC_OH_OrgProxy = organisation.PK;

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("10001"));
			var invoice = CreateARInvoice(branch, job);
			Factory.Save();

			var receipt = Factory.NewWithValidTestData<ARReceipt>();
			receipt.AH_GB = branch.PK;
			receipt.AH_GC = differentCompany.PK;
			receipt.AH_OSExTaxAmount = 1000m;
			receipt.AH_LocalExTaxAmount = 1000m;

			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_GB = branch.PK;
			journal.AH_GC = differentCompany.PK;
			journal.AH_OSExTaxAmount = 390M;
			journal.AH_LocalExTaxAmount = 390M;

			SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Temp.TempPath);
			SetExportInvoiceTypeInRegistry(differentCompany, false, true);

			Factory.Save();

			foreach (var branchItem in differentCompany.Branches)
			{
				branchItem.GB_IsActive = false;
			}

			Factory.Save();

			var noOfLogsBeforeExport = invoice.Logs.GetAllLogs().Count;

			string fullFilePath = Path.Combine(Temp.TempPath, Subscriber.GetFilename(invoice));

			AssertNoExceptionThrown("Should not throw exception if there are no branchs active for transaction's company", () => RunLogWalkerCycleForTest());

			var reloadInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			var dexLog = reloadInvoice.Logs.MostRecentLogByEventTime(Events.DataExport, subscriber.Name);
			var noOfLogsAfterExport = reloadInvoice.Logs.GetAllLogs().Count;
			Assert("PDF file should NOT be created as no active branchs.", !File.Exists(fullFilePath));
			AssertNull("Invoice should not have the DEX event with reference 'InvoicePDFExport'", dexLog);
			AssertEquals("Dex Event should not be added", noOfLogsBeforeExport, noOfLogsAfterExport);
			AssertNull("No Error should be reported", ErrorReporter.LastExceptionReported);
		}

		public void TestProcessQueuedLogs()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "~01";
			branch.GB_BranchName = "Test Branch";

			var organisation = TestObjectCreator.CreateOrgHeader("ABC", false, false, "AUSYD");
			var orgAddress = TestObjectCreator.CreateAddress(organisation);
			orgAddress.AddAddressType(OrgAddressType.Receivables);
			company.GC_OH_OrgProxy = organisation.PK;

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("10001"));
			var invoice = CreateARInvoice(branch, job);
			Factory.Save();

			var receipt = Factory.NewWithValidTestData<ARReceipt>();
			receipt.AH_GB = branch.PK;
			receipt.AH_GC = company.PK;
			receipt.AH_OSExTaxAmount = 1000m;
			receipt.AH_LocalExTaxAmount = 1000m;

			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_GB = branch.PK;
			journal.AH_GC = company.PK;
			journal.AH_OSExTaxAmount = 390M;
			journal.AH_LocalExTaxAmount = 390M;

			SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Temp.TempPath);
			SetExportInvoiceTypeInRegistry(company, false, true);

			Factory.Save();

			var noOfLogsBeforeExport = invoice.Logs.GetAllLogs().Count;

			string fullFilePath = Path.Combine(Temp.TempPath, Subscriber.GetFilename(invoice));

			try
			{
				RunLogWalkerCycleForTest();
				var reloadInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
				var dexLog = reloadInvoice.Logs.MostRecentLogByEventTime(Events.DataExport, subscriber.Name);
				var noOfLogsAfterExport = reloadInvoice.Logs.GetAllLogs().Count;
				Assert("PDF file should be created.", File.Exists(fullFilePath));
				AssertNotNull("Invoice should have the DEX event with reference 'InvoicePDFExport' after the data export", dexLog);
				AssertEquals("Dex Event should be the only log added after the data export", noOfLogsBeforeExport + 1, noOfLogsAfterExport);
			}
			finally
			{
				File.Delete(fullFilePath);
			}

			AssertNull("No Error should be reported", ErrorReporter.LastExceptionReported);
		}

		public void TestShouldExportInvoice()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "~01";

			var consolInvoice = CreateARInvoice(branch);
			LinkInvoiceToNewConsol(consolInvoice);

			SetExportInvoiceTypeInRegistry(company, true, false);

			Assert("should export consol invoice.", InvoicePDFExportLogSubscriber.ShouldExportInvoice(consolInvoice));

			SetExportInvoiceTypeInRegistry(company, false, false);

			Assert("should NOT export consol invoice.", !InvoicePDFExportLogSubscriber.ShouldExportInvoice(consolInvoice));

			var shipmentInvoice = CreateARInvoice(branch);
			LinkInvoiceToNewShipment(shipmentInvoice);

			SetExportInvoiceTypeInRegistry(company, false, true);

			Assert("should export shipment invoice.", InvoicePDFExportLogSubscriber.ShouldExportInvoice(shipmentInvoice));

			SetExportInvoiceTypeInRegistry(company, false, false);

			Assert("should NOT export shipment invoice.", !InvoicePDFExportLogSubscriber.ShouldExportInvoice(shipmentInvoice));

			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			LinkInvoiceToNewConsol(creditNote);

			SetExportInvoiceTypeInRegistry(company, true, false);

			Assert("should Export credit note.", InvoicePDFExportLogSubscriber.ShouldExportInvoice(consolInvoice));

			APInvoice unwantedInvoice = Factory.New<APInvoice>();

			Assert("should NOT export AP invoice.", !InvoicePDFExportLogSubscriber.ShouldExportInvoice(unwantedInvoice));
		}

		public void TestSuspendInvoiceCopies()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B01";
			branch.GB_BranchName = "Test Branch";

			var organisation = TestObjectCreator.CreateOrgHeader("ABC", false, false, "AUSYD");
			var orgAddress = TestObjectCreator.CreateAddress(organisation);
			orgAddress.AddAddressType(OrgAddressType.Receivables);
			company.GC_OH_OrgProxy = organisation.PK;

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("10001"));
			var invoice = CreateARInvoice(branch, job);
			Factory.Save();

			SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Temp.TempPath);
			string filePath = Path.Combine(Temp.TempPath, Subscriber.GetFilename(invoice));

			var collection = AccountingConfigurationRegistry.Instance.InvoiceCopies.Value;
			collection[0].Name = (NoResString)"Original1";
			var copy = collection.AddNew();
			copy.Name = (NoResString)"Copy1";
			copy.DeliveryMethod = "ALL";
			AssertEquals(2, collection.Count);
			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			try
			{
				var list = new List<string>();
				using (DocARInvoiceCommon.RecordDocCopies_ForTestOnly(list))
				{
					RunLogWalkerCycleForTest();

					AssertEquals(true, File.Exists(filePath));
					AssertEquals(1, list.Count);
					AssertEquals("Original1", list[0]);
				}
			}
			finally
			{
				File.Delete(filePath);
			}

			AssertNull("No Error should be reported", ErrorReporter.LastExceptionReported);
		}

		public void TestTryGetDirectory()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			string directory = string.Empty;
			Assert("the registry is never setup for this company, therefore it should return false", !Subscriber.TryGetDirectory(company, out directory));

			SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Temp.TempPath);
			Assert("Valid directory has been set up", Subscriber.TryGetDirectory(company, out directory));

			SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			Assert("No valid directory has been set up", !Subscriber.TryGetDirectory(company, out directory));
		}

		[TestDate(2007, 1, 29, 1, 30, 0)]
		public void TestGetFilename()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "~01";

			var invoice = CreateARInvoice(branch);
			LinkInvoiceToNewConsol(invoice);
			LinkInvoiceToNewShipment(invoice);

			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "~02";
			invoice.AH_GE = department.PK;

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = "TestDebtor";
			invoice.AH_OH = debtor.PK;

			string actualfilePath = Subscriber.GetFilename(invoice);
			string expectedFilePath = "~01~02TestDebtor  26FYS0PM3GCY3VHFAX26TestJobNum                                        TESTHOUSEBILL       20070129.pdf";
			AssertEquals("Filename should be: ", expectedFilePath, actualfilePath);
		}

		[TestDate(2007, 1, 29, 1, 30, 0)]
		public void TestGetFilename_WithWhiteSpace()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "~01";

			var invoice = CreateARInvoice(branch);
			LinkInvoiceToNewConsol(invoice);
			LinkInvoiceToNewShipment(invoice);

			invoice.AH_ConsolidatedInvoiceRef = " zj63e4NpQxoNYPD";

			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "~02";
			invoice.AH_GE = department.PK;

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = "TestDebtor";
			invoice.AH_OH = debtor.PK;

			string actualfilePath = Subscriber.GetFilename(invoice);
			string expectedFilePath = "~01~02TestDebtor  zj63e4NpQxoNYPD     TestJobNum                                        TESTHOUSEBILL       20070129.pdf";
			AssertEquals("Filename should be: ", expectedFilePath, actualfilePath);
		}

		public void TestShouldNotExportPDF_WhenRePrintingIsNotAllowed()
		{
			var organisation = TestObjectCreator.CreateOrgHeader("ABC", false, false, "AUSYD");
			var orgAddress = TestObjectCreator.CreateAddress(organisation);
			orgAddress.AddAddressType(OrgAddressType.Receivables);

			var (company, branch) = TestObjectCreator.CreateCompanyAndBranch("AUSYD", organisation);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"));
			var invoice = CreateARInvoice(branch, job);
			invoice.AH_InvoicePrinted = true;

			Factory.Save();

			SystemDataRegistry.Instance.InvoicesInPDFFormatExportDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Temp.TempPath);
			AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			SetExportInvoiceTypeInRegistry(company, false, true);

			var noOfLogsBeforeExport = invoice.Logs.GetAllLogs().Count;
			var fullFilePath = Path.Combine(Temp.TempPath, Subscriber.GetFilename(invoice));

			try
			{
				RunLogWalkerCycleForTest();
				var reloadInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
				var dexLog = reloadInvoice.Logs.MostRecentLogByEventTime(Events.DataExport, subscriber.Name);
				var noOfLogsAfterExport = reloadInvoice.Logs.GetAllLogs().Count;
				Assert("PDF file should not be created.", !File.Exists(fullFilePath));
				AssertNull("Invoice should not have the DEX event", dexLog);
				AssertEquals("Log should not be added", noOfLogsBeforeExport, noOfLogsAfterExport);
				Assert(NotifiedEventList.Contains("""
					[InvoicePDFExport] Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, re-printing is not allowed through this module.
					Please go to the eDocs tab of the transaction and re-print the first invoice version stored there.
					"""));
			}
			finally
			{
				File.Delete(fullFilePath);
			}

			AssertNull("No Error should be reported", ErrorReporter.LastExceptionReported);
		}

		#region Implementation
		void SetExportInvoiceTypeInRegistry(GlbCompany company, bool enableConsolInvoice, bool enableShipmentInvoice)
		{
			CodeDescriptionBoolCollection newRoles = new CodeDescriptionBoolCollection();
			CodeDescriptionBool role = newRoles.AddNew();
			role.Code = ExportInvoiceTypesInPDFFormat.ConsolInvoice;
			role.Bool = enableConsolInvoice;
			role = newRoles.AddNew();
			role.Code = ExportInvoiceTypesInPDFFormat.ShipmentInvoice;
			role.Bool = enableShipmentInvoice;

			SystemDataRegistry.Instance.InvoiceTypesToExportInPDFFormat.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, newRoles);
		}

		InvoicePDFExportLogSubscriber Subscriber
		{
			get
			{
				return (subscriber ??= new InvoicePDFExportLogSubscriber());
			}
		}

		InvoicePDFExportLogSubscriber subscriber;

		#region Create Test Data

		ARInvoice CreateARInvoice(GlbBranch branch, Job job = null)
		{
			if (job == null)
			{
				job = Factory.NewJobWithValidTestDataForTesting<Job>();
			}
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "TestTN";
			invoice.AH_JH = job.PK;
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_GB = branch.PK;
			invoice.AH_OH = TestObjectCreator.LocalClient.PK;

			ARInvoiceLine invoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_GE = Env.CurrentDepartment.PK;
			invoiceLine.AL_GB = branch.PK;

			TestObjectCreator.CreateJobCharge(invoiceLine, job, chargeCode, TestObjectCreator.AUD);

			return invoice;
		}

		void LinkInvoiceToNewConsol(InvoicingBase invoice)
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.MasterBillMAWB = "TestMAWB";
			invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			invoice.AH_JH = Guid.Empty;
		}

		void LinkInvoiceToNewShipment(InvoicingBase invoice)
		{
			var shipment = TestObjectCreator.CreateShipment("TestJobNum");
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_HouseBill = "TestHouseBill";

			var jobHeader = TestObjectCreator.CreateJob(shipment);

			Factory.Save();

			invoice.AH_JH = jobHeader.PK;
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteDirectory();
		}

		void DeleteDirectory()
		{
			var dir = new DirectoryInfo(InvoicePDFExportLogSubscriber.PKLogDirectoryPath);

			try
			{
				dir.Delete(true);
			}
			catch (UnauthorizedAccessException)
			{ }
			catch (IOException)
			{ }
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator fTestObjectCreator;

		#endregion

		#endregion
	}
}
