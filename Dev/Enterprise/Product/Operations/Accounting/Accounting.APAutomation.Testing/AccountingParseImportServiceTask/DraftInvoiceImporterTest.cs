using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.AccountingParseImportServiceTask;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Dash.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.APAutomation.Testing.AccountingParseImportServiceTask
{
	public class DraftInvoiceImporterTest : TestCaseWithFactory
	{
		public void TestBlankDashInvoice()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice);
		}

		public void TestFullyPopulatedDashInvoice()
		{
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR1";
			var addressForAp = creditor.Addresses.AddNew(OrgAddressType.Payables, isDefault: true);
			addressForAp.OA_Address1 = "AP Address1";

			var dashInvoice = CreateDashInvoice(
				matchedIssuerPk: creditor.PK,
				matchedIssuerAddressPk: addressForAp.PK,
				invoiceDate: new ZDate(2025, 1, 31),
				invoiceNumber: "INV-1002",
				currencyCode: "AUD",
				netTotal: 132.58,
				vatTotal: 13.26,
				grossTotal: 135.84);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice,
				expectedTransactionCurrency: "AUD",
				expectedTransactionType: TransactionTypes.Invoice,
				expectedCreditorPk: creditor.PK,
				expectedCreditorAddressPk: addressForAp.PK,
				expectedTransactionNumber: "INV-1002",
				expectedTransactionDate: new ZDate(2025, 1, 31),
				expectedExTaxAmount: 132.58,
				expectedTaxAmount: 13.26,
				expectedTotalAmount: 135.84);
		}

		public void TestFullyPopulatedDashInvoiceAsCreditNote()
		{
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR1";
			var addressForAp = creditor.Addresses.AddNew(OrgAddressType.Payables, isDefault: true);
			addressForAp.OA_Address1 = "AP Address1";

			var dashInvoice = CreateDashInvoice(
				matchedIssuerPk: creditor.PK,
				matchedIssuerAddressPk: addressForAp.PK,
				invoiceDate: new ZDate(2025, 1, 31),
				invoiceNumber: "INV-1002",
				currencyCode: "AUD",
				netTotal: -132.58,
				vatTotal: -13.26,
				grossTotal: -135.84);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice,
				expectedTransactionCurrency: "AUD",
				expectedTransactionType: TransactionTypes.CreditNote,
				expectedCreditorPk: creditor.PK,
				expectedCreditorAddressPk: addressForAp.PK,
				expectedTransactionNumber: "INV-1002",
				expectedTransactionDate: new ZDate(2025, 1, 31),
				expectedExTaxAmount: 132.58,
				expectedTaxAmount: 13.26,
				expectedTotalAmount: 135.84);
		}

		public void TestCreditorWithoutAddress()
		{
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR1";
			var addressForAp = creditor.Addresses.AddNew(OrgAddressType.Payables, isDefault: true);
			addressForAp.OA_Address1 = "AP Address1";

			var dashInvoice = CreateDashInvoice(matchedIssuerPk: creditor.PK);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice,
				expectedTransactionType: TransactionTypes.Invoice,
				expectedTransactionCurrency: "AUD",
				expectedCreditorPk: creditor.PK,
				expectedCreditorAddressPk: addressForAp.PK);
		}

		public void TestCreditorWithAddress()
		{
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR1";
			var addressForAp = creditor.Addresses.AddNew(OrgAddressType.Payables, isDefault: true);
			addressForAp.OA_Address1 = "AP Address1";
			var parsedAddress = creditor.Addresses.AddNew(OrgAddressType.Receivables, isDefault: false);
			parsedAddress.OA_Address1 = "AR Address1";

			var dashInvoice = CreateDashInvoice(matchedIssuerPk: creditor.PK, matchedIssuerAddressPk: parsedAddress.PK);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice,
				expectedTransactionType: TransactionTypes.Invoice,
				expectedTransactionCurrency: "AUD",
				expectedCreditorPk: creditor.PK,
				expectedCreditorAddressPk: parsedAddress.PK);
		}

		public void TestCreditorAddressWithoutCreditor()
		{
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR1";
			var parsedAddress = Factory.New<OrgAddress>();
			parsedAddress.OA_OH = creditor.PK;
			parsedAddress.OA_Address1 = "Parsed Address1";

			var dashInvoice = CreateDashInvoice(matchedIssuerAddressPk: parsedAddress.PK);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice);
		}

		public void TestTransactionDate()
		{
			var dashInvoice = CreateDashInvoice(invoiceDate: new ZDate(2025, 1, 1));
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice, expectedTransactionDate: new ZDate(2025, 1, 1));
		}

		public void TestTransactionNumber()
		{
			var dashInvoice = CreateDashInvoice(invoiceNumber: "INV-1001");
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice, expectedTransactionNumber: "INV-1001");
		}

		public void TestExTaxAmount()
		{
			var dashInvoice = CreateDashInvoice(netTotal: 101.1);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice, expectedExTaxAmount: 101.1);
		}

		public void TestExTaxAmount_WhenNegative()
		{
			var dashInvoice = CreateDashInvoice(netTotal: -101.1);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice, expectedExTaxAmount: 101.1);
		}

		public void TestTaxAmount()
		{
			var dashInvoice = CreateDashInvoice(vatTotal: 101.1);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice, expectedTaxAmount: 101.1);
		}

		public void TestTaxAmount_WhenNegative()
		{
			var dashInvoice = CreateDashInvoice(vatTotal: -101.1);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice, expectedTaxAmount: 101.1);
		}

		public void TestTotalAmount()
		{
			var dashInvoice = CreateDashInvoice(grossTotal: 101.1);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.Invoice, expectedTotalAmount: 101.1);
		}

		public void TestTotalAmount_WhenNegative()
		{
			var dashInvoice = CreateDashInvoice(grossTotal: -101.1);
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "AUD", expectedTransactionType: TransactionTypes.CreditNote, expectedTotalAmount: 101.1);
		}

		public void TestCurrency()
		{
			var dashInvoice = CreateDashInvoice(currencyCode: "EUR");
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			AssertDraftInvoice(draftInvoice, expectedTransactionCurrency: "EUR", expectedTransactionType: TransactionTypes.Invoice);
		}

		public void TestSingleJobReference()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var headerCluster = CreateDashCluster(dashInvoice, isHeader: true);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "HBL00001234", "BOL");
			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([("HBL00001234", "BOL")]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestMultipleJobReferences()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var headerCluster = CreateDashCluster(dashInvoice, isHeader: true);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "HBL00001234", "BOL");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "S00001000", "JOB");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "C00005678", "JOB");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "AU0012345", "CNT");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "S00001001", "JOB");
			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([
				("HBL00001234", "BOL"),
				("S00001000", "JOB"),
				("C00005678", "JOB"),
				("AU0012345", "CNT"),
				("S00001001", "JOB"),
			]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestDuplicateJobReferencesDifferentClusters()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var headerCluster = CreateDashCluster(dashInvoice, isHeader: true);
			var clusterA = CreateDashCluster(dashInvoice, isHeader: false);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster, clusterA], "AU0012345", "CNT");
			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([("AU0012345", "CNT")]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestDuplicateJobReferencesNoCluster()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			CreateDashCluster(dashInvoice, isHeader: true);
			CreateDashCluster(dashInvoice, isHeader: false);

			CreateDashJobRef(dashInvoice, "S00001000", "JOB");
			CreateDashJobRef(dashInvoice, "S00001000", "JOB");
			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([("S00001000", "JOB")]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestDuplicateJobReferencesDifferentTypes()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			CreateDashJobRef(dashInvoice, "S00001000", "JOB");
			CreateDashJobRef(dashInvoice, "S00001000", "CNT");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([("S00001000", "JOB")]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestMixedJobReferences()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var headerCluster = CreateDashCluster(dashInvoice, isHeader: true);
			var clusterA = CreateDashCluster(dashInvoice, isHeader: false);
			var clusterB = CreateDashCluster(dashInvoice, isHeader: false);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster, clusterA], "S00001000", "JOB");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "AU0012345", "CNT");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [clusterA], "C00005678", "JOB");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster, clusterA, clusterB], "NZ0098765", "CNT");
			CreateDashJobRef(dashInvoice, "HBL00001234", "BOL");
			CreateDashJobRef(dashInvoice, "S00001001", "JOB");
			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([
				("S00001000", "JOB"),
				("AU0012345", "CNT"),
				("C00005678", "JOB"),
				("NZ0098765", "CNT"),
				("HBL00001234", "BOL"),
				("S00001001", "JOB")
			]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestSingleJobReferenceNoCluster()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			CreateDashCluster(dashInvoice, isHeader: true);

			CreateDashJobRef(dashInvoice, "AU0012345", "CNT");
			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([("AU0012345", "CNT")]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestUniqueJobReferenceSanitisation()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			CreateDashJobRef(dashInvoice, "S00001000 ", "JOB");
			CreateDashJobRef(dashInvoice, " \n  S00001001\r\n", "JOB");
			CreateDashJobRef(dashInvoice, "s00001002", "JOB");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([
				("S00001000", "JOB"),
				("S00001001", "JOB"),
				("S00001002", "JOB")
			]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestDuplicateJobReferenceSanitisation()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			CreateDashJobRef(dashInvoice, "S00001000 ", "JOB");
			CreateDashJobRef(dashInvoice, " \n  S00001000\r\n", "JOB");
			CreateDashJobRef(dashInvoice, "s00001000", "JOB");

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([("S00001000", "JOB")]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestSingleJobReferenceMixedCaseJobType()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var headerCluster = CreateDashCluster(dashInvoice, isHeader: true);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [headerCluster], "HBL00001234", "BoL");
			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice);

			var expectedJobRefs = CreateDraftInvoiceJobReferenceCollection([("HBL00001234", "BOL")]);
			AssertJobReferences(Factory, draftInvoice, expectedJobRefs);
		}

		public void TestJobMatchingSingleJobReference()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			CreateDashJobRef(dashInvoice, "MEDU4213877", "CNT");

			var mockJobReferenceMatcher = new Mock<IJobReferenceMatcher>();
			var jobPk = ZGuid.NewZGuid();
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDU4213877", It.IsAny<string[]>())).Returns((jobPk, "JK"));

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice, mockJobReferenceMatcher.Object);

			var draftInvoiceJobRef = (AccDraftInvoiceJobReference)draftInvoice.JobReferences.FirstOrDefault();
			AssertNotEquals("Job FK was not set.", draftInvoiceJobRef.AIR_AIJ_Job, ZGuid.Empty);

			var draftInvoiceJob = draftInvoiceJobRef.Job;
			AssertEquals(draftInvoiceJob.AIJ_ParentID, jobPk);
			AssertEquals(draftInvoiceJob.AIJ_ParentTableCode, "JK");
			AssertEquals(draftInvoiceJob.AIJ_GC_Company, draftInvoice.AIH_GC_Company);
			AssertNotEquals("Cluster FK was not set.", draftInvoiceJob.AIJ_AIC_Cluster, ZGuid.Empty);

			var draftInvoiceCluster = draftInvoiceJob.Cluster;
			AssertEquals(draftInvoiceCluster.AIC_GC_Company, draftInvoice.AIH_GC_Company);
			AssertEquals(draftInvoiceCluster.AIC_RX_NKCurrency, draftInvoice.AIH_RX_NKTransactionCurrency);

			AssertNoExceptionThrown("Failed to save", Factory.Save);
		}

		public void TestJobMatchingMultipleJobReferences()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var dashCluster1 = CreateDashCluster(dashInvoice, isHeader: false);
			var dashCluster2 = CreateDashCluster(dashInvoice, isHeader: false);
			var dashCluster3 = CreateDashCluster(dashInvoice, isHeader: true);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster1], "S00477181", "JOB");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster2], "MEDU4213877", "CNT");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster3], "MEDUQN263874", "BOL");

			var mockJobReferenceMatcher = new Mock<IJobReferenceMatcher>();
			var jobPk = ZGuid.NewZGuid();
			var cntPk = ZGuid.NewZGuid();
			var bolPk = ZGuid.NewZGuid();
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("S00477181", It.IsAny<string[]>())).Returns((jobPk, "JK"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDU4213877", It.IsAny<string[]>())).Returns((cntPk, "CSH"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDUQN263874", It.IsAny<string[]>())).Returns((bolPk, "JS"));

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice, mockJobReferenceMatcher.Object);

			AssertCollectionNotContains("Job FK was not set.", ZGuid.Empty, draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.AIR_AIJ_Job));
			Assert("Incorrect company information", draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>()
				.All(r => r.Job.AIJ_GC_Company == draftInvoice.AIH_GC_Company));

			AssertContainsExactElementsInAnyOrder(
				[
					(jobPk, "JK"),
					(cntPk, "CSH"),
					(bolPk, "JS"),
				],
				draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => (r.Job.AIJ_ParentID, r.Job.AIJ_ParentTableCode)));

			AssertCollectionNotContains("Cluster FK was not set.", ZGuid.Empty, draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.Job.Cluster));
			Assert("Incorrect cluster information ", draftInvoice.JobClusters.Cast<AccDraftInvoiceJobCluster>()
				.All(c => c.AIC_GC_Company == draftInvoice.AIH_GC_Company &&
					c.AIC_RX_NKCurrency == draftInvoice.AIH_RX_NKTransactionCurrency));
			var jobClusterFKs = draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.Job.AIJ_AIC_Cluster);
			AssertEquals(jobClusterFKs.Distinct().Count(), 3);

			AssertNoExceptionThrown("Failed to save", Factory.Save);
		}

		public void TestJobMatchingSingleJobWithMultipleReferences()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var dashCluster1 = CreateDashCluster(dashInvoice, isHeader: false);
			var dashCluster2 = CreateDashCluster(dashInvoice, isHeader: false);
			var dashCluster3 = CreateDashCluster(dashInvoice, isHeader: true);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster1], "S00477181", "JOB");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster2], "MEDU4213877", "CNT");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster2], "MEDUQN263874", "BOL");

			var mockJobReferenceMatcher = new Mock<IJobReferenceMatcher>();
			var jobPk = ZGuid.NewZGuid();
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("S00477181", It.IsAny<string[]>())).Returns((jobPk, "JK"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDU4213877", It.IsAny<string[]>())).Returns((jobPk, "JK"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDUQN263874", It.IsAny<string[]>())).Returns((jobPk, "JK"));

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice, mockJobReferenceMatcher.Object);

			AssertCollectionNotContains("Job FK was not set.", ZGuid.Empty, draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.AIR_AIJ_Job));
			Assert("Incorrect company information", draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>()
				.All(r => r.Job.AIJ_GC_Company == draftInvoice.AIH_GC_Company));

			var expectedDraftInvoiceJobFK = draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().First().AIR_AIJ_Job;
			Assert("Incorrect job FK", draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>()
				.All(r => r.AIR_AIJ_Job == expectedDraftInvoiceJobFK));

			AssertCollectionNotContains("Cluster FK was not set.", ZGuid.Empty, draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.Job.Cluster));
			Assert("Incorrect cluster information ", draftInvoice.JobClusters.Cast<AccDraftInvoiceJobCluster>()
				.All(c => c.AIC_GC_Company == draftInvoice.AIH_GC_Company &&
					c.AIC_RX_NKCurrency == draftInvoice.AIH_RX_NKTransactionCurrency));
			var jobClusterFKs = draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.Job.AIJ_AIC_Cluster);
			AssertEquals(jobClusterFKs.Distinct().Count(), 1);

			AssertNoExceptionThrown("Failed to save", Factory.Save);
		}

		public void TestJobMatchingDuplicateReferencesInSingleCluster()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			var dashCluster = CreateDashCluster(dashInvoice, isHeader: true);

			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster], "S00477181", "JOB");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster], "MEDU4213877", "CNT");
			CreateDashJobRefAndAssignToCluster(dashInvoice, [dashCluster], "MEDUQN263874", "BOL");

			var mockJobReferenceMatcher = new Mock<IJobReferenceMatcher>();
			var jobPk = ZGuid.NewZGuid();
			var cntPk = ZGuid.NewZGuid();
			var bolPk = ZGuid.NewZGuid();
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("S00477181", It.IsAny<string[]>())).Returns((jobPk, "JK"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDU4213877", It.IsAny<string[]>())).Returns((cntPk, "CSH"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDUQN263874", It.IsAny<string[]>())).Returns((bolPk, "JS"));

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice, mockJobReferenceMatcher.Object);

			AssertCollectionNotContains("Job FK was not set.", ZGuid.Empty, draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.AIR_AIJ_Job));
			Assert("Incorrect company information", draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>()
				.All(r => r.Job.AIJ_GC_Company == draftInvoice.AIH_GC_Company));

			AssertContainsExactElementsInAnyOrder(
				[
					(jobPk, "JK"),
					(cntPk, "CSH"),
					(bolPk, "JS"),
				],
				draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => (r.Job.AIJ_ParentID, r.Job.AIJ_ParentTableCode)));

			AssertCollectionNotContains("Cluster FK was not set.", ZGuid.Empty, draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.Job.Cluster));
			Assert("Incorrect cluster information ", draftInvoice.JobClusters.Cast<AccDraftInvoiceJobCluster>()
				.All(c => c.AIC_GC_Company == draftInvoice.AIH_GC_Company &&
					c.AIC_RX_NKCurrency == draftInvoice.AIH_RX_NKTransactionCurrency));
			var jobClusterFKs = draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.Job.AIJ_AIC_Cluster);
			AssertEquals(jobClusterFKs.Distinct().Count(), 3);

			AssertNoExceptionThrown("Failed to save", Factory.Save);
		}

		public void TestJobMatchingSearchFields()
		{
			var dashInvoice = CreateDashInvoice();
			var draftInvoice = objectCreator.CreateUploadedDraftInvoice("AUD");

			CreateDashJobRef(dashInvoice, "S00477181", "JOB");
			CreateDashJobRef(dashInvoice, "MEDU4213877", "cnt");
			CreateDashJobRef(dashInvoice, "MEDUQN263874", "BoL");

			var mockJobReferenceMatcher = new Mock<IJobReferenceMatcher>();
			var jobPk = ZGuid.NewZGuid();
			var cntPk = ZGuid.NewZGuid();
			var bolPk = ZGuid.NewZGuid();
			var expectedSearchFields = new Dictionary<string, string[]>
			{
				{ "JOB", ["JOBNUMBER"] },
				{ "CNT", ["CONTAINERNUMBER"] },
				{ "BOL", ["MASTERBILLNUMBER", "HOUSEBILLNUMBER"] }
			};
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("S00477181", expectedSearchFields["JOB"])).Returns((jobPk, "JK"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDU4213877", expectedSearchFields["CNT"])).Returns((cntPk, "CSH"));
			mockJobReferenceMatcher.Setup(m => m.FindBestMatching("MEDUQN263874", expectedSearchFields["BOL"])).Returns((bolPk, "JS"));

			DraftInvoiceImporter.ImportToDraftInvoice(Factory, dashInvoice, draftInvoice, mockJobReferenceMatcher.Object);

			AssertCollectionNotContains("Job FK was not set.", ZGuid.Empty, draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => r.AIR_AIJ_Job));

			AssertContainsExactElementsInAnyOrder(
				[
					(jobPk, "JK"),
					(cntPk, "CSH"),
					(bolPk, "JS")
				],
				draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>().Select(r => (r.Job.AIJ_ParentID, r.Job.AIJ_ParentTableCode)));
		}

		static void AssertDraftInvoice(AccDraftInvoiceHeader draftInvoice, ZString expectedTransactionCurrency, string expectedTransactionType, ZGuid? expectedCreditorPk = null, ZGuid? expectedCreditorAddressPk = null, ZString? expectedTransactionNumber = null, ZDate? expectedTransactionDate = null, ZDecimal? expectedExTaxAmount = null, ZDecimal? expectedTaxAmount = null, ZDecimal? expectedTotalAmount = null)
		{
			var expectedDescription = expectedTransactionType == TransactionTypes.CreditNote ? "AP Credit Note" : "AP Invoice";
			CombineAssertions(() =>
			{
				AssertEquals(expectedCreditorPk ?? ZGuid.Empty, draftInvoice.AIH_OH_Creditor);
				AssertEquals(expectedCreditorAddressPk ?? ZGuid.Empty, draftInvoice.AIH_OA_CreditorAddress);
				AssertEquals(expectedTransactionNumber ?? ZString.Empty, draftInvoice.AIH_TransactionNumber);
				AssertEquals(expectedTransactionDate ?? ZDate.Empty, draftInvoice.AIH_TransactionDate);
				AssertEquals(expectedTransactionCurrency, draftInvoice.AIH_RX_NKTransactionCurrency);
				AssertEquals(expectedExTaxAmount ?? ZDecimal.Zero, draftInvoice.AIH_ExpectedOSExTaxAmount);
				AssertEquals(expectedTaxAmount ?? ZDecimal.Zero, draftInvoice.AIH_ExpectedOSTaxAmount);
				AssertEquals(expectedTotalAmount ?? ZDecimal.Zero, draftInvoice.AIH_ExpectedOSTotalAmount);
				AssertEquals(expectedTransactionType, draftInvoice.AIH_TransactionType);
				AssertEquals(expectedDescription, draftInvoice.AIH_Description);
			});
		}

		static void AssertJobReferences(BusinessObjectFactory factory, AccDraftInvoiceHeader draftInvoice, AccDraftInvoiceJobReferenceCollection expectedJobReferences)
		{
			var message =
$"""
Expected job reference amount: {expectedJobReferences.Count}
Job reference amount found: {draftInvoice.JobReferences.Count}

Expected job references:
{string.Join("\n", expectedJobReferences.Select(expectedJobReference => $" - \"{expectedJobReference.AIR_Reference}\" (Type: {expectedJobReference.AIR_Type})"))}
Job references found:
{string.Join("\n", draftInvoice.JobReferences.Select(jobReference => $" - \"{jobReference.AIR_Reference}\" (Type: {jobReference.AIR_Type})"))}
If everything seems to match, jobReference.AIR_AIH_Header may not be set correctly
""";

			AssertContainsExactElementsInAnyOrder(
				message,
				draftInvoice.JobReferences.Select(jobRef => (jobRef.AIR_Reference, jobRef.AIR_Type)),
				expectedJobReferences.Select(jobRef => (jobRef.AIR_Reference, jobRef.AIR_Type)));
			AssertNoExceptionThrown("Failed to save\n\n", factory.Save);
		}

		DashAPInvoice CreateDashInvoice(ZGuid? matchedIssuerPk = null, ZGuid? matchedIssuerAddressPk = null,
			ZString? invoiceNumber = null, ZDate? invoiceDate = null, ZString? currencyCode = null,
			ZDecimal? grossTotal = null, ZDecimal? vatTotal = null, ZDecimal? netTotal = null)
		{
			var dashDoc = CreateDashDocument();
			var dashInvoice = Factory.New<DashAPInvoice>();
			dashInvoice.DPI_DDD_DashDocID = dashDoc.PK;
			dashInvoice.DPI_OH_MatchedIssuerID = matchedIssuerPk ?? ZGuid.Empty;
			dashInvoice.DPI_OA_MatchedIssuerAddressID = matchedIssuerAddressPk ?? ZGuid.Empty;
			dashInvoice.DPI_RX_NKInvoiceCurrency = currencyCode ?? ZString.Empty;
			dashInvoice.DPI_InvoiceNumber = invoiceNumber ?? ZString.Empty;
			dashInvoice.DPI_InvoiceDate = invoiceDate ?? ZDate.Empty;
			dashInvoice.DPI_GrossTotal = grossTotal ?? ZDecimal.Zero;
			dashInvoice.DPI_VatTotal = vatTotal ?? ZDecimal.Zero;
			dashInvoice.DPI_NetTotal = netTotal ?? ZDecimal.Zero;

			return dashInvoice;
		}

		DashAPInvoiceRef CreateDashJobRef(DashAPInvoice dashInvoice, ZString reference, ZString type)
		{
			var jobRef = Factory.NewWithValidTestData<DashAPInvoiceRef>();
			jobRef.DPR_DPI_HeaderID = dashInvoice.PK;
			jobRef.DPR_Reference = reference;
			jobRef.DPR_ReferenceType = type;
			dashInvoice.DashAPInvoiceRefs.Add(jobRef);
			return jobRef;
		}

		void CreateDashClusterRef(DashAPInvoiceCluster cluster, DashAPInvoiceRef reference)
		{
			var clusterRef = Factory.NewWithValidTestData<DashAPInvoiceClusterRef>();
			clusterRef.DRC_DPC_ClusterID = cluster.PK;
			clusterRef.DRC_DPR_RefID = reference.PK;
			cluster.DashAPInvoiceClusterRefs.Add(clusterRef);
		}

		DashAPInvoiceCluster CreateDashCluster(DashAPInvoice dashInvoice, bool isHeader)
		{
			var cluster = Factory.NewWithValidTestData<DashAPInvoiceCluster>();
			cluster.DPC_DPI_HeaderID = dashInvoice.PK;
			cluster.DPC_IsHeader = isHeader;
			dashInvoice.DashAPInvoiceClusters.Add(cluster);
			return cluster;
		}

		AccDraftInvoiceJobReferenceCollection CreateDraftInvoiceJobReferenceCollection((ZString reference, ZString type)[] expectedReferences)
		{
			var expectedDraftInvoiceJobReferences = new AccDraftInvoiceJobReferenceCollection(Factory);
			foreach(var (expectedReference, expectedType) in expectedReferences)
			{
				var jobReference = Factory.NewWithValidTestData<AccDraftInvoiceJobReference>();
				jobReference.AIR_Reference = expectedReference;
				jobReference.AIR_Type = expectedType;
				expectedDraftInvoiceJobReferences.Add(jobReference);
			}
			return expectedDraftInvoiceJobReferences;
		}

		void CreateDashJobRefAndAssignToCluster(DashAPInvoice dashInvoice, List<DashAPInvoiceCluster> clusters, ZString reference, ZString type)
		{
			var jobRef = CreateDashJobRef(dashInvoice, reference, type);
			foreach (var cluster in clusters)
			{
				CreateDashClusterRef(cluster, jobRef);
			}
		}

		DashDocument CreateDashDocument()
		{
			var dashDocument = Factory.New<DashDocument>();
			dashDocument.DDD_ParseType = "PIN";
			dashDocument.DDD_ParseStatus = "RTP";
			dashDocument.DDD_DocID = ZGuid.NewZGuid();

			return dashDocument;
		}

		protected override void SetUp()
		{
			base.SetUp();
			objectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator objectCreator;
	}
}
