using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	class EInvoicingBatchTransactionExporterTest : TestCaseWithFactory
	{
		[TestDate(2018, 10, 23, 10, 37, 0)]
		public void TestExportTransactionBatch()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var transactionCreator = new TransactionCreator();

				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.GoodsClassChargeCode, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.AALSHI);
				Factory.Save();

				var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				apInvoice.Lines.Add(TestObjectCreator.CreateCostLine(charge, apInvoice.PK));
				var apCreditNote = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote) as InvoicingBase;
				apCreditNote.AH_TransactionNum = "AP002";
				var arAdjustmentNote = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote) as InvoicingBase;

				var apInvoiceToReverse = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				apInvoiceToReverse.Lines.Add(TestObjectCreator.CreateCostLine(charge2, apInvoiceToReverse.PK));
				Factory.Save();

				var reverser = new ReversingFactory().NewReversing(apInvoiceToReverse);
				reverser.Reverse();
				apInvoiceToReverse.ReverseInvoice.AH_TransactionNum = "REV001";
				apInvoiceToReverse.ReverseInvoice.AH_ReceiptType = "IDE";
				Factory.Save();

				AssertEquals("PreCondition", "AP001", apInvoice.AH_TransactionNum);
				AssertNotNull(apInvoice.Lines[0].AL_JH);
				AssertEquals("AP002", apCreditNote.AH_TransactionNum);
				AssertEquals("00001000", arAdjustmentNote.AH_TransactionNum);
				AssertEquals("AP002", apInvoiceToReverse.AH_TransactionNum);
				AssertNotNull(apInvoiceToReverse.ReverseInvoice);
				Assert(apInvoiceToReverse.IsCancelled);

				var invoicingBatch = Factory.New<AccEInvoicingBatch>();
				invoicingBatch.AIB_BatchNumber = 1;
				invoicingBatch.AIB_GC = GlbCompany.CurrentCompany.PK;
				invoicingBatch.AIB_Status = Core.Constants.EInvoicingBatchState.Ready;
				invoicingBatch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow.ToDateTime();
				invoicingBatch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code.ToString();

				LinkTransactionPivot(Factory, apInvoice.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, apCreditNote.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, arAdjustmentNote.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, apInvoiceToReverse.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var invoicingBatchReload = newFactory.Load<AccEInvoicingBatch>(invoicingBatch.PK);
				var exportor = CreateExporter();

				var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);
				CleanupTransactionBatch(transactionBatch);

				AssertBatchWithEmbeddedResource(transactionBatch, "AccBatchRequests_EInvoicing.xml");
			}
		}

		[TestDate(2018, 10, 23, 10, 37, 0)]
		public void TestExportTransactionBatch_IsSortedByPostDateThenLedgerThenTypeThenNumber()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var transactionCreator = new TransactionCreator();
				var apInvoiceYesterday = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP096", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				apInvoiceYesterday.AH_PostDate = ZDate.Today.AddDays(-1);
				var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001000", TestObjectCreator.AUD, 2m, TestObjectCreator.AALSHI);
				var apCreditNote = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote) as InvoicingBase;
				apCreditNote.AH_TransactionNum = "00001000";
				var apCreditNote2 = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote) as InvoicingBase;
				apCreditNote2.AH_TransactionNum = "00001001";
				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 3m, TestObjectCreator.AALSHI);
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("PreCondition", "AP096", apInvoiceYesterday.AH_TransactionNum);
					AssertEquals("PreCondition", "00001000", apInvoice.AH_TransactionNum);
					AssertEquals("PreCondition", "00001000", apCreditNote.AH_TransactionNum);
					AssertEquals("PreCondition", "00001001", apCreditNote2.AH_TransactionNum);
					AssertEquals("PreCondition", "00001000", arInvoice.AH_TransactionNum);
				});

				var invoicingBatch = Factory.New<AccEInvoicingBatch>();
				invoicingBatch.AIB_BatchNumber = 1;
				invoicingBatch.AIB_GC = GlbCompany.CurrentCompany.PK;
				invoicingBatch.AIB_Status = Core.Constants.EInvoicingBatchState.Ready;
				invoicingBatch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow.ToDateTime();
				invoicingBatch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code.ToString();

				LinkTransactionPivot(Factory, apCreditNote2.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, apInvoice.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, apInvoiceYesterday.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, apCreditNote.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, arInvoice.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var exportor = CreateExporter();

				var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);
				AssertEquals("Should be five transactions in batch", 5, transactionBatch.TransactionCollection.Count);
				var expectedSortedKeys = new[] { "AP INV AP096", "AP CRD 00001000", "AP INV 00001000", "AR INV 00001000", "AP CRD 00001001" };
				var actualSortedKeys = transactionBatch.TransactionCollection.Select(t => t.DataContext.DataSourceCollection.First().Key.GetValueOrDefault().ToString()).ToArray();
				AssertSequencesEqual("Sort criteria should be PostDate, TransactionNum, Ledger, TransactionType", expectedSortedKeys, actualSortedKeys);
			}
		}

		void AssertInvoiceAddedToEdocs(string ledgerType, ZString transactionNum, ZInt batchNum, bool arRegistryValue, bool expectedResult)
		{
			AccountingConfigurationRegistry.Instance.GenerateARInvoiceAttachmentForEReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, arRegistryValue);
			var transactionCreator = new TransactionCreator();
			InvoicingBase invoice = null;

			if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				Factory.Save();

				invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNum, TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, invoice.PK));
				Factory.Save();
			}
			else if (ledgerType == LedgerTypes.AccountsPayable)
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0m, TestObjectCreator.Agent, 1.0m);
				invoice = TestObjectCreator.CreateAPInvoice<APInvoice>(transactionNum, TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor1);
				invoice.AH_AB = TestObjectCreator.AUDBankAccount.PK;
				invoice.Lines.RemoveAndDeleteAll();
				var line = TestObjectCreator.CreateAPInvoiceLine((APInvoice)invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "AP Line 001", 250m);
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);
			}

			AssertEquals("PreCondition", 0, invoice.DocManagerInfo.Files.Count);

			var invoicingBatch = Factory.New<AccEInvoicingBatch>();
			invoicingBatch.AIB_BatchNumber = batchNum;
			invoicingBatch.AIB_GC = GlbCompany.CurrentCompany.PK;
			invoicingBatch.AIB_Status = Core.Constants.EInvoicingBatchState.Ready;
			invoicingBatch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow.ToDateTime();
			invoicingBatch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code.ToString();

			LinkTransactionPivot(Factory, invoice.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var invoicingBatchReload = newFactory.Load<AccEInvoicingBatch>(invoicingBatch.PK);
			var exporter = CreateExporter();

			var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);
			Factory.Save();

			if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				invoice = newFactory.Load<ARInvoice>(invoice.PK);
			}
			else if (ledgerType == LedgerTypes.AccountsPayable)
			{
				invoice = newFactory.Load<APInvoice>(invoice.PK);
			}

			if (expectedResult)
			{
				AssertEquals("An edoc has been generated and attached for the invoice", 1, invoice.DocManagerInfo.Files.Count);
				AssertEquals(ZArchitecture.Core.TransactionTypes.Invoice, invoice.DocManagerInfo.Files[0].DocType);
				AssertEquals(string.Format("AR {0} {1}.pdf", invoice.AH_TransactionType, invoice.AH_TransactionNum), invoice.DocManagerInfo.Files[0].FileName);

				exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);
				AssertEquals("The edoc has not been duplicated upon rebatching invoice", 1, invoice.DocManagerInfo.Files.Count);
			}
			else
			{
				AssertEquals("No edoc has been generated and attached for the invoice", 0, invoice.DocManagerInfo.Files.Count);
			}
		}

		public void TestExportTransactionBatch_WithPublishedEDoc()
		{
			var docQuery = new ZQuery(RefDocTypeSchema.RT_DocType, "INV");
			docQuery.AddToFilter(RefDocTypeSchema.RT_ReferenceType, "ALL");
			var docType = Factory.LoadTop1<RefDocType>(docQuery);
			docType.RT_IsPublished = true;
			Factory.Save();

			TestObjectCreator.SetTemporaryControlAccounts();
			AccountingConfigurationRegistry.Instance.GenerateARInvoiceAttachmentForEReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			var transactionCreator = new TransactionCreator();

			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, invoice.PK));
			Factory.Save();

			var invoicingBatch = Factory.New<AccEInvoicingBatch>();
			invoicingBatch.AIB_BatchNumber = 2;
			invoicingBatch.AIB_GC = GlbCompany.CurrentCompany.PK;
			invoicingBatch.AIB_Status = Core.Constants.EInvoicingBatchState.Ready;

			LinkTransactionPivot(Factory, invoice.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var exporter = CreateExporter();
			exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);
			Factory.Save();

			AssertEquals($"AR {invoice.AH_TransactionType} {invoice.AH_TransactionNum}.pdf", invoice.DocManagerInfo.Files[0].FileName);
			AssertEquals(true, invoice.DocManagerInfo.Files[0].IsPublished);

			exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			invoice = newFactory.Load<ARInvoice>(invoice.PK);

			AssertEquals($"AR {invoice.AH_TransactionType} {invoice.AH_TransactionNum}[2].pdf", invoice.DocManagerInfo.Files[1].FileName);
			AssertEquals(true, invoice.DocManagerInfo.Files[1].IsPublished);
			AssertEquals("Original eDoc should be un-published", false, invoice.DocManagerInfo.Files[0].IsPublished);
			AssertEquals("New eDoc should be generated every time.", 2, invoice.DocManagerInfo.Files.Count);
		}

		public void TestInvoiceAddedToEdocsForTransactionBatchBasedOnARRegistry()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				AssertInvoiceAddedToEdocs(LedgerTypes.AccountsReceivable, "AR001", 1, true, true);
				AssertInvoiceAddedToEdocs(LedgerTypes.AccountsReceivable, "AR002", 2, false, false);
				AssertInvoiceAddedToEdocs(LedgerTypes.AccountsPayable, "AP001", 3, true, false);
				AssertInvoiceAddedToEdocs(LedgerTypes.AccountsPayable, "AP002", 4, false, false);
			}
		}

		public void TestPopulateOptionalXUTFieldsSetting_DefaultValueShouldBeFalse()
		{
			var setting = new PopulateOptionalXUTFieldsSetting();
			Assert(!setting.PopulateAttachedDocuments);
			Assert(!setting.PopulateShipments);
			Assert(!setting.PopulateAuthorizationDetails);
		}

		public void TestPopulateOptionalXUTFieldsSetting_AllSetToFalse()
		{
			var setting = new PopulateOptionalXUTFieldsSetting(
				populateAttachedDocuments: false,
				populateShipments: false,
				populateAuthorizationDetails: false);

			var transactionInfo = CreateTransactionAndPopulateOptionalXUTFieldsSetting(setting);

			AssertNull("ShipmentCollection", transactionInfo.ShipmentCollection);
			AssertNull("AttachedDocumentCollection", transactionInfo.AttachedDocumentCollection);
			AssertNull("TransactionInfo.AuthorizationDetailCollection", transactionInfo.AuthorizationDetailCollection);
			AssertNull("OriginalReference.AuthorizationDetailCollection", transactionInfo.OriginalReference.AuthorizationDetailCollection);
		}

		public void TestPopulateOptionalXUTFieldsSetting_AllSetToTrue()
		{
			var setting = new PopulateOptionalXUTFieldsSetting(
				populateAttachedDocuments: true,
				populateShipments: true,
				populateAuthorizationDetails: true);

			var transactionInfo = CreateTransactionAndPopulateOptionalXUTFieldsSetting(setting);

			AssertNotNull("ShipmentCollection", transactionInfo.ShipmentCollection);
			AssertEquals("Expect 1 Job Line", 1,
				transactionInfo.ShipmentCollection
					.Count(s => s.DataContext.DataSourceCollection
						.Any(ds => ds.Key?.ToString() == TestShipmentNum_Job)));

			Assert("Expect Consol Lines",
				transactionInfo.ShipmentCollection
					.Any(s => s.DataContext.DataSourceCollection.Count() == 1
						   && s.DataContext.DataSourceCollection
									.Any(ds => ds.Key?.ToString() == TestShipmentNum_Consol)));

			AssertNotNull("AttachedDocumentCollection", transactionInfo.AttachedDocumentCollection);
			AssertEquals("Expect 1 eDocs", transactionInfo.AttachedDocumentCollection.Count, 1);

			AssertNotNull("TransactionInfo.AuthorizationDetailCollection", transactionInfo.AuthorizationDetailCollection);
			AssertEquals("Expect 1 Authorization Record", transactionInfo.AuthorizationDetailCollection.Count, 1);

			AssertNotNull("OriginalReference.AuthorizationDetailCollection", transactionInfo.OriginalReference.AuthorizationDetailCollection);
			AssertEquals("Expect 1 Authorization Record in OriginalReference", transactionInfo.OriginalReference.AuthorizationDetailCollection.Count, 1);
		}

		public void TestPopulateOptionalXUTFieldsSetting_OnlyPopulateShipments()
		{
			var setting = new PopulateOptionalXUTFieldsSetting(
				populateAttachedDocuments: false,
				populateShipments: true,
				populateAuthorizationDetails: false);

			var transactionInfo = CreateTransactionAndPopulateOptionalXUTFieldsSetting(setting);

			AssertNotNull("ShipmentCollection", transactionInfo.ShipmentCollection);
			AssertEquals("Expect 1 Job Line", 1,
				transactionInfo.ShipmentCollection
					.Count(s => s.DataContext.DataSourceCollection
						.Any(ds => ds.Key?.ToString() == TestShipmentNum_Job)));

			Assert("Expect Consol Lines",
				transactionInfo.ShipmentCollection
					.Any(s => s.DataContext.DataSourceCollection.Count() == 1
						   && s.DataContext.DataSourceCollection
									.Any(ds => ds.Key?.ToString() == TestShipmentNum_Consol)));
		}

		public void TestPopulateOptionalXUTFieldsSetting_OnlyPopulateAttachedDocuments()
		{
			var setting = new PopulateOptionalXUTFieldsSetting(
				populateAttachedDocuments: true,
				populateShipments: false,
				populateAuthorizationDetails: false);

			var transactionInfo = CreateTransactionAndPopulateOptionalXUTFieldsSetting(setting);

			AssertNotNull("AttachedDocumentCollection", transactionInfo.AttachedDocumentCollection);
			AssertEquals("Expect 1 eDocs", transactionInfo.AttachedDocumentCollection.Count, 1);
		}

		public void TestPopulateOptionalXUTFieldsSetting_OnlyPopulateAuthorizationDetailCollection()
		{
			var setting = new PopulateOptionalXUTFieldsSetting(
				populateAttachedDocuments: false,
				populateShipments: false,
				populateAuthorizationDetails: true);

			var transactionInfo = CreateTransactionAndPopulateOptionalXUTFieldsSetting(setting);

			AssertNotNull("AuthorizationDetailCollection", transactionInfo.AuthorizationDetailCollection);
			AssertEquals("Expect 1 Authorization Record", transactionInfo.AuthorizationDetailCollection.Count, 1);
			AssertNotNull("OriginalReference.AuthorizationDetailCollection", transactionInfo.OriginalReference.AuthorizationDetailCollection);
			AssertEquals("Expect 1 Authorization Record in OriginalReference", transactionInfo.OriginalReference.AuthorizationDetailCollection.Count, 1);
		}

		#region Implementation

		const string TestShipmentNum_Job = "JOB_001";
		const string TestShipmentNum_Consol = "CONSOL_001";

		TransactionInfo CreateTransactionAndPopulateOptionalXUTFieldsSetting(PopulateOptionalXUTFieldsSetting setting)
		{
			var shipment = TestObjectCreator.CreateShipment(TestShipmentNum_Job);
			var job = TestObjectCreator.CreateJob(shipment);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", TestShipmentNum_Consol);
			consol.Shipments.Add(shipment);
			Factory.Save();

			var postedConsolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			postedConsolCost.E6_LocalCostAmount = 500m;
			postedConsolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
			Factory.Save();

			AssertEquals($"Expect Job contains 1 charge", 1, job.Charges.Count);

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), TestShipmentNum_Consol, TestObjectCreator.AUD, 1m, 500m, 0m, 0m, 0m);
			CreateAuthorizationRecord(apInvoice);

			var apCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), TestShipmentNum_Consol, TestObjectCreator.AUD, 1m, 500m, 0m, 0m, 0m);
			apCreditNote.Lines.Add(TestObjectCreator.CreateCostLine(job.Charges[0], apCreditNote.PK));

			postedConsolCost.E6_AH_APInvoice = apCreditNote.PK;
			Factory.Save();

			apCreditNote.AH_TransactionBelongsToGroup = apInvoice.PK;
			Factory.Save();

			foreach (InvoicingLineBase line in apCreditNote.Lines)
			{
				_ = line.ConsolIDFromApportionedCharge; //Force ConsolID value available
			}
			apCreditNote.AH_ConsolidatedInvoiceRef = TestShipmentNum_Consol;
			Factory.Save();

			AddTestEDocs(apCreditNote);
			CreateAuthorizationRecord(apCreditNote);

			var transactionBatch = CreateBatchForInvoice(apCreditNote, setting);

			AssertNotNull(transactionBatch);
			AssertNotNull(transactionBatch.TransactionCollection);
			AssertNotEquals(0, transactionBatch.TransactionCollection.Count);
			return transactionBatch.TransactionCollection[0];
		}

		AccTransactionHeaderAuthorisationRecord CreateAuthorizationRecord(InvoicingBase transaction)
		{
			var authRecordTransaction = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(transaction);
			authRecordTransaction.AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.Panama; //Country that implements EInvoicing.
			authRecordTransaction.AHF_Number = "GVT#-" + transaction.AH_TransactionNum;
			Factory.Save();

			return authRecordTransaction;
		}

		TransactionBatch CreateBatchForInvoice(InvoicingBase apInvoice, PopulateOptionalXUTFieldsSetting setting)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoicingBatch = Factory.New<AccEInvoicingBatch>();
				invoicingBatch.AIB_BatchNumber = 1;
				invoicingBatch.AIB_GC = GlbCompany.CurrentCompany.PK;
				invoicingBatch.AIB_Status = Core.Constants.EInvoicingBatchState.Ready;
				invoicingBatch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow.ToDateTime();
				invoicingBatch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code.ToString();

				LinkTransactionPivot(Factory, apInvoice.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);

				Factory.Save();

				var exportor = CreateExporter(setting);
				return exportor.CreateTransactionBatch(invoicingBatch);
			}
		}

		void AddTestEDocs(InvoicingBase invoice)
		{
			var arInvoiceeDoc = ((IDocManagerSupport)invoice).DocManagerInfo.AddFileOrDocument(
				new ZBlob(new byte[] { 1, 2, 3 }), "TestInvoiceFile", "INV");
			arInvoiceeDoc.IsPublished = true;
			Factory.Save();
		}

		protected void LinkTransactionPivot(BusinessObjectFactory factory, ZGuid transactionPK, ZGuid invoicingBatchPK, GlbCompany company, ZString status, string errorDescription = null)
		{
			var pivot = factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = transactionPK;
			pivot.AIP_AIB = invoicingBatchPK;
			pivot.AIP_Status = status;
			pivot.AIP_ErrorDescription = errorDescription;
			pivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddHours(-2).ToDateTime();
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow.ToDateTime();
			pivot.SetCompanyAndCountryCode(company);
		}

		void AssertBatchWithEmbeddedResource(TransactionBatch batch, string testFilePath)
		{
			var expectedXml = EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String(testFilePath, EInvoicingTestHelper.CommonXmlFilesEmbeddedLocation);
			using (var batchStream = (SubStreamableStream)new MemoryStream())
			{
				new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter().WriteXML(batch, batchStream, false);
				batchStream.Position = 0;

				using (var batchReader = new StreamReader(batchStream))
				{
					var actualXml = batchReader.ReadToEnd();

					this.AssertXMLEqualsIgnoreChildOrder("Exported XML should match XML file", expectedXml, actualXml);
				}
			}
		}

		protected virtual TransactionBatchExporter CreateExporter(PopulateOptionalXUTFieldsSetting setting = null)
		{
			return new TransactionBatchExporter(DataAccess, optionalXUTFieldsSetting: setting);
		}

		protected virtual void CleanupTransactionBatch(TransactionBatch batch)
		{
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
			TestObjectCreator = new TestObjectCreator(Factory);

			var glHeader = TestObjectCreator.GLHeader1;
			glHeader.AG_AccountNum = "1234.56.78";
			glHeader.AG_Description = "Test GL Header";
		}

		protected TestObjectCreator TestObjectCreator;
		protected BatchExportDataAccess DataAccess;

		#endregion Implementation
	}
}
