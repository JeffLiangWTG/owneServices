using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.Importer
{
	[TestedType(typeof(InvoicingBaseLineImporter))]
	public class InvoicingBaseLineImporterTest : TestCaseWithFactory
	{
		public void TestDependencyInjection()
		{
			AssertNotNull(Importer);
			AssertType<InvoicingBaseLineImporter>(Importer);
		}

		public void TestImportLinesFromChargeCollection()
		{
			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var shipment1 = TestObjectCreator.CreateShipment("STEST01");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			TestObjectCreator.SetExchangeRate(job1, TestObjectCreator.USD, 2.5m);
			var charge11 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);
			var charge12 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC2, null, TestObjectCreator.USD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);

			var shipment2 = TestObjectCreator.CreateShipment("STEST02");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.GBP, 1.5m);
			var charge21 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);
			var charge22 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC4, null, TestObjectCreator.GBP, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);

			Factory.Save();

			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice)
				, TestObjectCreator.AUD
				, organisation: TestObjectCreator.Creditor1);
			AssertEquals("PreCondtion", 0, apInvoice.Lines.Count);

			Importer.ImportLinesFromChargeCollection(apInvoice, new[] { charge11, charge12, charge21, charge22 });
			AssertEquals("Should have 4 lines which are converted from 4 charges", 4, apInvoice.Lines.Count);
			AssertImportedInvoicingLine("Line from charge11", apInvoice, charge11);
			AssertImportedInvoicingLine("Line from charge12", apInvoice, charge12);
			AssertImportedInvoicingLine("Line from charge21", apInvoice, charge21);
			AssertImportedInvoicingLine("Line from charge22", apInvoice, charge22);

			void AssertImportedInvoicingLine(string comment, InvoicingBase invoice, Charge charge)
			{
				var importedLine = invoice.Lines.Cast<InvoicingLineBase>().Single(x => x.OriginalJobCharge == charge);
				CombineAssertions(comment, () => {
					AssertEquals(nameof(importedLine.GenericCharge) + "_ReadOnly", true, importedLine.GenericChargeInfo.ReadOnly);
					AssertEquals(nameof(importedLine.AL_JH), charge.JR_JH, importedLine.AL_JH);
					AssertEquals(nameof(importedLine.AL_JH) + "_ReadOnly", true, importedLine.AL_JHInfo.ReadOnly);
					AssertEquals(nameof(importedLine.AL_AT), charge.JR_AT_CostGSTRate, importedLine.AL_AT);

					AssertEquals(nameof(importedLine.AL_LocalExTaxAmount),charge.JR_LocalCostAmt, importedLine.AL_LocalExTaxAmount);
					AssertEquals(nameof(importedLine.AL_LocalTotalAmount), charge.JR_Calc_LocalCostAmtWithGST, importedLine.AL_LocalTotalAmount);

					AssertEquals(nameof(importedLine.AL_RX_NKTransactionCurrency),charge.JR_RX_NKCostCurrency, importedLine.AL_RX_NKTransactionCurrency);
					AssertEquals(nameof(importedLine.AL_OSExTaxAmount),charge.JR_OSCostAmt, importedLine.AL_OSExTaxAmount);
					AssertEquals(nameof(importedLine.AL_OSAmount), charge.JR_Calc_OSCostAmtWithGST, importedLine.AL_OverseasTotal);
				});
			}
		}

		public void TestImportLinesFromChargeCollection_FetchHints()
		{
			var shipment1 = TestObjectCreator.CreateShipment("STEST01");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			TestObjectCreator.SetExchangeRate(job1, TestObjectCreator.USD, 1m);
			var charge11 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);
			var charge12 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC2, null, TestObjectCreator.USD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);

			var shipment2 = TestObjectCreator.CreateShipment("STEST02");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.GBP, 1m);
			var charge21 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);
			var charge22 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC2, null, TestObjectCreator.GBP, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);

			Factory.Save();

			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var newFactory = new BusinessObjectFactory();
				var newTestObjectCreator = new TestObjectCreator(newFactory);

				var apInvoice = (APInvoice)newTestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, null, null, null);
				var charges = newFactory.Load<Charge>(
					new ZQuery(JobChargeSchema.PK, new[] { charge11.PK, charge12.PK, charge21.PK, charge22.PK })
				);
				var fetchHintTables = new[] {
				new FetchHintsForTable() { TableName = JobExRateSchema.Constants.TableName
					, ExpectedActiveFetchHintCount = 3 , ExpectedDbHitCount = 0
					, FetchHintTesting = () => {
						newFactory.Load<ExchangeRate>(new ZQuery(JobExRateSchema.JF_JH, job1.PK));
						newFactory.Load<ExchangeRate>(new ZQuery(JobExRateSchema.JF_JH, job2.PK));

						AssertEquals("[JobExRate] Fetch Hint is applied.", 3, newFactory.GetLoadedFetchHintCountForTable(JobExRateSchema.Constants.TableName));
						AssertEquals("[JobExRate] Should not cause any additional db hits when query match to fetch hint", 1, newFactory.GetTableHitCount(JobExRateSchema.Constants.TableName));
					}
				}
				, new FetchHintsForTable() { TableName = RefCurrencySchema.Constants.TableName
					, ExpectedLoadedFetchHintCount = 1 , ExpectedDbHitCount = 0
					, FetchHintTesting = () => {
						newFactory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, TestObjectCreator.AUD.RX_Code));
						newFactory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, TestObjectCreator.USD.RX_Code));
						newFactory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, TestObjectCreator.GBP.RX_Code));

						AssertEquals("[RefCurrency] Should not cause any additional hint.", 1, newFactory.GetLoadedFetchHintCountForTable(RefCurrencySchema.Constants.TableName));
					}
				}
				, new FetchHintsForTable() { TableName = AccChargeGLPostingOverrideSchema.Constants.TableName
					// FetchHint is not applied for all, the applied FetchHint cause only 1 DbHit
					// However, the more DB hits come from ColumnValueRanker.GetFullQuery that it uses ZDBOnlyQuery to prevent from caching issue.
					, ExpectedLoadedFetchHintCount = 2, ExpectedDbHitCount = 3
					, FetchHintTesting = () => {
						newFactory.Load<AccChargeGLPostingOverride>(new ZQuery(AccChargeGLPostingOverrideSchema.Y1_AC, TestObjectCreator.CC1.PK));
						newFactory.Load<AccChargeGLPostingOverride>(new ZQuery(AccChargeGLPostingOverrideSchema.Y1_AC, TestObjectCreator.CC2.PK));

						AssertEquals("[AccChargeGLPostingOverride] Should not cause any additional db hits when query match to fetch hint", 3, newFactory.GetTableHitCount(AccChargeGLPostingOverrideSchema.Constants.TableName));
					}
				}
				, new FetchHintsForTable() { TableName = AccChargeRevRecOverrideSchema.Constants.TableName
					, ExpectedLoadedFetchHintCount = 2 , ExpectedDbHitCount = 1
					, FetchHintTesting = () => {
						newFactory.Load<AccChargeRevRecOverride>(new ZQuery(AccChargeRevRecOverrideSchema.AE_AC, TestObjectCreator.CC1.PK));
						newFactory.Load<AccChargeRevRecOverride>(new ZQuery(AccChargeRevRecOverrideSchema.AE_AC, TestObjectCreator.CC2.PK));

						AssertEquals("[AccChargeRevRecOverride] Should not cause any additional db hits when query match to fetch hint", 1, newFactory.GetTableHitCount(AccChargeRevRecOverrideSchema.Constants.TableName));
					}
				}
				, new FetchHintsForTable() { TableName = JobChargeSchema.Constants.TableName
					, ExpectedActiveFetchHintCount = 1
					, FetchHintTesting = () => {
						newFactory.Load<Charge>(new ZQuery(JobChargeSchema.JR_JH, job1.PK));
						newFactory.Load<Charge>(new ZQuery(JobChargeSchema.JR_JH, job2.PK));

						AssertEquals("[JobCharge] Fetch Hint is applied.", 1, newFactory.GetLoadedFetchHintCountForTable(JobChargeSchema.Constants.TableName));
						AssertEquals("[JobCharge] Should not cause any additional db hits when query match to fetch hint", 1, newFactory.GetTableHitCount(JobChargeSchema.Constants.TableName));
					}
				}
				, new FetchHintsForTable() { TableName = ViewGenericChargeSchema.Constants.TableName
					, ExpectedLoadedFetchHintCount = 1, ExpectedDbHitCount = 1
					, FetchHintTesting = () => {
						newFactory.Load<GenericCharge.GenericCharge>(new ZQuery(ViewGenericChargeSchema.PK, TestObjectCreator.CC1.PK));
						newFactory.Load<GenericCharge.GenericCharge>(new ZQuery(ViewGenericChargeSchema.PK, TestObjectCreator.CC2.PK));

						AssertEquals("[ViewGenericCharge] Should not cause any additional db hits when query match to fetch hint", 1, newFactory.GetTableHitCount(ViewGenericChargeSchema.Constants.TableName));
					}
				}
				, new FetchHintsForTable() { TableName = JobHeaderSchema.Constants.TableName
					, ExpectedLoadedFetchHintCount = 2, ExpectedDbHitCount = 2
					, FetchHintTesting = () => {
						newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, job1.PK));
						newFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, job2.PK));
						newFactory.Load<Job>(new ZQuery(JobHeaderSchema.JH_JH_ParentJob, job2.PK).AddToFilter(JobHeaderSchema.JH_GC, apInvoice.AH_GC));
						newFactory.Load<Job>(new ZQuery(JobHeaderSchema.JH_JH_ParentJob, job1.PK).AddToFilter(JobHeaderSchema.JH_GC, apInvoice.AH_GC));

						AssertEquals("[JobHeader] Should not cause any additional db hits when query match to fetch hint", 2, newFactory.GetTableHitCount(JobHeaderSchema.Constants.TableName));
					}
				}
			};

				newFactory.DropHints();
				newFactory.ResetDatabaseLoadCount();
				Importer.ImportLinesFromChargeCollection(apInvoice, charges);

				CombineAssertions("Fetch hint count after ImportLinesFromChargeCollection", () => {
					fetchHintTables.ForEach(x => {
						AssertEquals($"Active Fetch hint count for {x.TableName}", x.ExpectedActiveFetchHintCount, newFactory.ActiveFetchHintsForTable(x.TableName));
						AssertEquals($"Loaded Fetch hint count for {x.TableName}", x.ExpectedLoadedFetchHintCount, newFactory.GetLoadedFetchHintCountForTable(x.TableName));
						AssertEquals($"Table hit count for {x.TableName}", x.ExpectedDbHitCount, newFactory.GetTableHitCount(x.TableName));

						x.FetchHintTesting.Invoke();
					});
				});
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		class FetchHintsForTable
		{
			public string TableName { get; set; }
			public int ExpectedActiveFetchHintCount { get; set; }
			public int ExpectedLoadedFetchHintCount { get; set; }
			public int ExpectedDbHitCount { get; set; }
			public Action FetchHintTesting { get; set; }
		}

		public void TestImportLinesFromChargeCollection_DeveloperExceptionsAfterExecuteFetchHints()
		{
			var consol = TestObjectCreator.CreateConsol("D", "S", "CTEST01");
			consol.JK_MasterBillNum = "222222";
			var shipment = TestObjectCreator.CreateShipment("STEST01", consol);
			shipment.JS_HouseBill = "11111";
			var sJob = TestObjectCreator.CreateJob(shipment);
			var sCharge = TestObjectCreator.CreateCharge(sJob, TestObjectCreator.CC1, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, null);
			Factory.Save();

			Factory.SeedQueryCache("JobHeader", new ZQuery(JobHeaderSchema.PK, sJob.PK));

			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, null, null, null);
			Importer.ImportLinesFromChargeCollection(apInvoice, sJob.Charges.Cast<Charge>());
			AssertEquals("There should be 1 Invoice Line", apInvoice.Lines.Count, 1);
			AssertEquals("JH_GC should be the same", apInvoice.AH_GC, sJob.JH_GC);

			Factory.ExecuteAllFetchHints();
			AssertEquals("No Exception", "", ErrorReporter.LastMessageReported);
		}

		public void TestImportLinesFromChargeCollection_AutoTickFinalFlagValidation()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0m, null, 0m);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, null, TestObjectCreator.AUD, 100m, TestObjectCreator.LocalClient, null, TestObjectCreator.AUD, 0m, null);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			Importer.ImportLinesFromChargeCollection(invoice, job.Charges.Cast<Charge>());
			AssertAutoTickFinalFlagValidation(expectedHasError: true);

			invoice.Lines.RemoveAndDeleteAll();

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			Importer.ImportLinesFromChargeCollection(invoice, job.Charges.Cast<Charge>());
			AssertAutoTickFinalFlagValidation(expectedHasError: false);

			void AssertAutoTickFinalFlagValidation(bool expectedHasError)
			{
				AssertEquals("Should have 1 line", 1, invoice.Lines.Count);
				AssertEquals("Should be ticked", ZBool.True, invoice.Lines[0].AL_IsFinalCharge);
				if (expectedHasError)
				{
					AssertHasError("Should have the error"
						, invoice.Lines[0].AL_IsFinalChargeInfo
						, @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice");
				}
				else
				{
					AssertNoErrors("Should have no errors", invoice.Lines[0].AL_IsFinalChargeInfo);
				}
			}
		}

		public void TestImportLinesFromConsolCostCollection()
		{
			var dirtyConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			Factory.Save();
			AssertEquals("PreCondition", false, dirtyConsolCost.HasChanges);

			var dummyAPInvoiceConsolCosting = new APInvoiceConsolCosting(Factory, Factory.New<APInvoice>());
			dummyAPInvoiceConsolCosting.ConsolCosts.Add(dirtyConsolCost);

			var dummyConsolCostings = new[] { Factory.NewWithValidTestData<JobConsolCost>(), Factory.NewWithValidTestData<JobConsolCost>() };
			var mockedConsolCostImporter = new Mock<IConsolCostImporter>();
			mockedConsolCostImporter.Setup(x => x.ImportCostsToCollection(dummyAPInvoiceConsolCosting.ConsolCosts, dummyConsolCostings, true))
				.Callback<APInvoiceConsolCostCollection, IEnumerable<JobConsolCost>, bool>((passinConsolCostCollection, _, _) => {
					AssertEquals("Since we do sync parent consol via importing consolCosts, the collection should not set parent info when running AddNew().", true, passinConsolCostCollection.IsSettingDefaultConsolSuspended_ForTestOnly);
					AssertEquals("Before calling ImportCostsToCollection, the pass-in consol cost collection should be cleared.", 0, passinConsolCostCollection.Count);
				});

			var mockInvoicingBaseImporterTarget = new Mock<IInvoicingBaseImporterTarget>();
			mockInvoicingBaseImporterTarget.Setup(x => x.Factory).Returns(Factory);
			mockInvoicingBaseImporterTarget.Setup(x => x.ConsolCosting).Returns(dummyAPInvoiceConsolCosting);
			mockInvoicingBaseImporterTarget.Setup(x => x.ImportAllApportionmentsFromCosting_SuspendListChanged())
				.Callback(() => {
					mockedConsolCostImporter.Verify(x => x.ImportCostsToCollection(mockInvoicingBaseImporterTarget.Object.ConsolCosting.ConsolCosts, dummyConsolCostings, true)
						, Times.Exactly(1)
						, "We should call ImportCostsToCollection before running ImportAllApportionmentsFromCosting_SuspendListChanged");
				});

			using(ObjectFactory.Substitute<IConsolCostImporter>(mockedConsolCostImporter.Object))
			{
				AssertContainsExactElementsInAnyOrder("PreCondition, set dirty consol cost before running Import method"
					, new[] { dirtyConsolCost }
					, mockInvoicingBaseImporterTarget.Object.ConsolCosting.ConsolCosts);
				Importer.ImportLinesFromConsolCostCollection(mockInvoicingBaseImporterTarget.Object, dummyConsolCostings);
				AssertEquals("Invoice's ConsolCosting will be cleared by default.", 0, mockInvoicingBaseImporterTarget.Object.ConsolCosting.ConsolCosts.Count);
			}

			mockedConsolCostImporter.Verify(x => x.ImportCostsToCollection(It.IsAny<APInvoiceConsolCostCollection>(), It.IsAny<IEnumerable<JobConsolCost>>(), It.IsAny<bool>())
				, Times.Exactly(1));
			mockInvoicingBaseImporterTarget.Verify(x => x.ImportAllApportionmentsFromCosting_SuspendListChanged()
				, Times.Exactly(1));

			AssertEquals("We just remove the dirty consol cost from collection and do not change it", false, dirtyConsolCost.HasChanges);
		}

		IInvoicingBaseLineImporter Importer => importer ??= ObjectFactory.Get<IInvoicingBaseLineImporter>();
		IInvoicingBaseLineImporter importer;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}