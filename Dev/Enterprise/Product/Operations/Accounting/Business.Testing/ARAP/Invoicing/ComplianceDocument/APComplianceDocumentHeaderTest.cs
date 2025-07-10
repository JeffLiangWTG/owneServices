using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(APComplianceDocumentHeader))]
	public class APComplianceDocumentHeaderTest : AccComplianceDocumentHeaderTest
	{
		protected override AccComplianceDocumentHeader GetComplianceDocumentHeader()
		{
			var header = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			header.ADH_Ledger = LedgerTypes.AccountsPayable;
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			return header;
		}

		[TestDate(2019, 1, 1)]
		public override void TestQueueForComplianceReports_ACQDate()
		{
			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(201901, new ZDateTime(2019, 01, 01), new ZDateTime(2019, 01, 31));
			testHelper.SetupSinglePeriod(201902, new ZDateTime(2019, 02, 01), new ZDateTime(2019, 02, 28));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var reportConfigurations = SetupConfigurationForComplianceDocument(newFactory, "TW1", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
			newFactory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var aPInvoice = creator.CreateInvoice(typeof(APInvoice), "INV002", creator.TWD, 1M, creator.ABIGAS);
				var line2 = creator.CreateInvoiceLine(aPInvoice, creator.TWD, 1M, 200M, 20M, 0M, creator.CC1.PK);
				var job = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
				line2.AL_JH = job.PK;
				line2.AL_AT = creator.GST1.PK;
				creator.CreateJobCharge(line2, job, creator.CC1);

				new ComplianceDocumentCreator(new[] { aPInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocument = newFactory.Load<APComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 1, complianceDocument.Length);

				complianceDocument[0].ADH_ComplianceSubType = "TXC";
				complianceDocument[0].ADH_DocumentNumber = "001";
				complianceDocument[0].ADH_DocumentDate = ZDateTime.Today;
				complianceDocument[0].ADH_ReportingPeriod = 201902;
				newFactory.Save();

				var queueEntries = new DynamicBusinessObjectCollection(newFactory);
				queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
				AssertEquals("only one compliance document is created in AccTransactionComplianceReportQueue table", 1, queueEntries.Count);
				AssertEquals(new ZDateTime(2019, 02, 01), (ZDateTime)queueEntries[0]["ACQ_Date"]);
				AssertEquals(complianceDocument[0].PK, (ZGuid)queueEntries[0]["ACQ_ParentID"]);
			}
		}

		public void TestInternalReference()
		{
			var complianceDocumentHeader1 = SetupAPComplianceDocumentHeader(TransactionTypes.Invoice, "AA001");
			AssertEquals("00001000", complianceDocumentHeader1.ADH_InternalReference);

			var complianceDocumentHeader2 = SetupAPComplianceDocumentHeader(TransactionTypes.Invoice, "AA002");
			AssertEquals("00001001", complianceDocumentHeader2.ADH_InternalReference);

			var complianceDocumentHeader3 = SetupAPComplianceDocumentHeader(TransactionTypes.CreditNote, "AA003");
			AssertEquals("00001000", complianceDocumentHeader3.ADH_InternalReference);
		}

		AccComplianceDocumentHeader SetupAPComplianceDocumentHeader(ZString transactionType, ZString documentNumber)
		{
			var factory = new BusinessObjectFactory();

			var complianceDocumentHeader = factory.NewWithValidTestData<APComplianceDocumentHeader>();
			complianceDocumentHeader.ADH_TransactionType = transactionType;
			complianceDocumentHeader.ADH_DocumentNumber = documentNumber;
			complianceDocumentHeader.ADH_QRCode1 = ZString.Empty;
			complianceDocumentHeader.ADH_QRCode2 = ZString.Empty;
			factory.Save();

			return complianceDocumentHeader;
		}

		public void TestPropertiesReadOnly()
		{
			var header = GetComplianceDocumentHeader() as APComplianceDocumentHeader;
			Factory.Save();
			AssertEquals(Core.Constants.ComplianceDocumentStatus.Added, header.ADH_DocumentStatus);

			Assert(!header.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
			Assert(!header.ADH_DocumentDate_ReadOnly_ForTestOnly);
			Assert(!header.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
			Assert(!header.ADH_DocumentNumber_ReadOnly_ForTestOnly);
			Assert(header.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);

			header.ADH_DocumentNumber = "TEST001";
			Factory.Save();
			AssertEquals(Core.Constants.ComplianceDocumentStatus.NumberSet, header.ADH_DocumentStatus);

			Assert(header.ADH_ComplianceSubType_ReadOnly_ForTestOnly);
			Assert(header.ADH_DocumentDate_ReadOnly_ForTestOnly);
			Assert(header.ADH_ReportingPeriod_ReadOnly_ForTestOnly);
			Assert(header.ADH_DocumentNumber_ReadOnly_ForTestOnly);
			Assert(header.ADH_XD_ComplianceBook_ReadOnly_ForTestOnly);
		}
	}
}
