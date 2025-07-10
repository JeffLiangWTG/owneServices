using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Integration.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class PeriodicInvoiceDBHitCountTests : TestCaseWithFactory
	{
		#region TestDbHitCountOnPosting

		public void TestDbHitCountOnPosting()
		{
			AssertDbHitCountOnPosting(false);
		}

		public void TestDbHitCountOnPostingWithOneChargePerJob()
		{
			AssertDbHitCountOnPosting(true);
		}

		void AssertDbHitCountOnPosting(bool oneChargePerJob)
		{
			int jobCount = 10;
			int chargesPerJobCount = 20;
			int chargesToPostCount = jobCount * (oneChargePerJob ? 1 : chargesPerJobCount);

			List<Job> jobs = new List<Job>(jobCount);
			List<ZGuid> chargePKs = new List<ZGuid>(chargesToPostCount);
			CreateDataForTestingDbHitCount(jobs, chargePKs, jobCount, chargesPerJobCount, oneChargePerJob);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			using (testFactory.EnableTableHitQueryCollection(GetAllTablesToMonitorForDbHits()))
			{
				var periodicInvoice = new PeriodicInvoice(testFactory);
				periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;

				periodicInvoice.PostDate = ZDateTime.Today;
				periodicInvoice.DueDate = ZDateTime.Today.AddDays(30);
				periodicInvoice.InvoiceDate = ZDateTime.Today;
				periodicInvoice.InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
				periodicInvoice.InvoiceTermDays = 30;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				periodicInvoice.LoadJobs();
				AssertEquals("Job Count", jobCount, periodicInvoice.Jobs.Count);
				AssertEquals("Selected Jobs Count", jobCount, periodicInvoice.SelectedJobs.Count());
				AssertEquals("Charges Count", chargesToPostCount, periodicInvoice.Charges.Count);
				var allChargesQuery = new ZQuery();
				allChargesQuery.FetchOnlyFromLocalCache = true;
				AssertEquals("Amount of charges in Factory", chargesToPostCount, testFactory.Load<Charge>(allChargesQuery).Length);

				var (maxTableHitsForLoadJobs, maxDbLoadCountForLoadJobs) = GetExpectedHitsForLoadJobs(oneChargePerJob);
				AssertHitCounts(testFactory, maxTableHitsForLoadJobs, maxDbLoadCountForLoadJobs);
				testFactory.ResetDatabaseLoadCount();

				periodicInvoice.RunPreSaveValidation();

				AssertEquals(string.Format("Errors: {0}", periodicInvoice.NotificationsIncludingChildren.ToUniqueMessageListString()), periodicInvoice.HasErrors, false);
				AssertEquals("Validation shouldn't increase amount of charges in Factory", chargesToPostCount, testFactory.Load<Charge>(allChargesQuery).Length);

				var (maxTableHitsForPreSaveValidation, maxDbLoadCountForPreSaveValidation) = GetExpectedHitsForPreSaveValidation(jobCount);
				AssertHitCounts(testFactory, maxTableHitsForPreSaveValidation, maxDbLoadCountForPreSaveValidation);
				testFactory.ResetDatabaseLoadCount();

				// PeriodicInvoiceForm and PeriodicInvoiceBulk call CreateTransactions() on a copy of the periodic invoice in a different factory.
				// As lots of caching is scoped to a factory, we need to keep a careful eye on performance.
				var periodicInvoiceInNewFactory = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();
				using (periodicInvoiceInNewFactory.Factory.EnableTableHitQueryCollection(GetAllTablesToMonitorForDbHits()))
				{
					periodicInvoiceInNewFactory.CreateTransactions();

					var (maxTableHitsForCreateInNewFactory, maxDbLoadCountForCreateInNewFactory) = GetExpectedHitsForCreateTransactionsInNewFactory(oneChargePerJob);
					AssertHitCounts(periodicInvoiceInNewFactory.Factory, maxTableHitsForCreateInNewFactory, maxDbLoadCountForCreateInNewFactory);
				}

				using (MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
				{
					periodicInvoice.CreateTransactions();
					periodicInvoice.Factory.Save();
				}

				AssertEquals("Ideally we shouldn't increase amount of charges in Factory here also, so it should be chargesToPostCount, but for now we can't easily", jobCount * chargesPerJobCount, testFactory.Load<Charge>(allChargesQuery).Length);

				var (maxTableHitsForCreateAndSave, maxDbLoadCountForCreateAndSave) = GetExpectedHitsForCreateTransactionsAndSave(oneChargePerJob);
				AssertHitCounts(testFactory, maxTableHitsForCreateAndSave, maxDbLoadCountForCreateAndSave);
				AssertInvoiceCreatedAndChargesModified(periodicInvoice, chargePKs, chargesToPostCount);
			}
		}

		#region Expected Hit Count Dictionaries
		// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
		static (Dictionary<string, int> maxTableHits, int maxDbLoadCount) GetExpectedHitsForLoadJobs(bool oneChargePerJob)
		{
			var maxTableHits = new Dictionary<string, int>();
			maxTableHits.Add(AccPeriodManagementSchema.Constants.TableName, 1);

			maxTableHits.Add(JobChargeSchema.Constants.TableName, 1);
			maxTableHits.Add(JobConShipLinkSchema.Constants.TableName, 10);
			maxTableHits.Add(JobHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(JobShipmentSchema.Constants.TableName, 10);

			maxTableHits.Add(OrgARTermsSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgCompanyDataSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgHeaderSchema.Constants.TableName, 2);
			maxTableHits.Add(OrgInvoiceTypeSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgMiscServSchema.Constants.TableName, 1);

			maxTableHits.Add(ViewGenericJobSchema.Constants.TableName, 1);

			int maxDbLoadCount;
			if (oneChargePerJob)
			{
				maxTableHits.Add(AccTransactionLinesSchema.Constants.TableName, 1);
				maxDbLoadCount = 31;
			}
			else
			{
				maxTableHits.Add(AccTransactionLinesSchema.Constants.TableName, 7);    // Lines loaded in batches by 64 from Fetch Hints
				maxDbLoadCount = 37;
			}

			return (maxTableHits, maxDbLoadCount);
		}

		// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
		static (Dictionary<string, int> maxTableHits, int maxDbLoadCount) GetExpectedHitsForPreSaveValidation(int jobCount)
		{
			var maxTableHits = new Dictionary<string, int>();
			maxTableHits.Add(AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1);
			maxTableHits.Add(AccChargeCodeSchema.Constants.TableName, 1);
			maxTableHits.Add(AccChargeRevRecOverrideSchema.Constants.TableName, 1);
			maxTableHits.Add(AccChargeTypeOverrideSchema.Constants.TableName, 2);
			maxTableHits.Add(AccTaxRateSchema.Constants.TableName, 1);

			maxTableHits.Add(GlbBranchSchema.Constants.TableName, 0);
			maxTableHits.Add(GlbCompanySchema.Constants.TableName, 0);
			maxTableHits.Add(GlbDepartmentSchema.Constants.TableName, 0);
			maxTableHits.Add(GlbStaffSchema.Constants.TableName, 0);

			maxTableHits.Add(JobChargeRevRecognitionSchema.Constants.TableName, 1);

			maxTableHits.Add(OrgAddressSchema.Constants.TableName, 2);
			maxTableHits.Add(OrgAddressCapabilitySchema.Constants.TableName, 1);
			maxTableHits.Add(OrgCompanyDataSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgInvTypeDeferredChargesSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgRelatedPartySchema.Constants.TableName, 1);
			maxTableHits.Add(OrgStaffAssignmentsSchema.Constants.TableName, 1);

			maxTableHits.Add(RefAccTaxRateSchema.Constants.TableName, 2);
			var maxDbLoadCount = 28;

			// Job Reload for reason code validation. 1 hit per Job.
			maxDbLoadCount += jobCount;
			maxTableHits.Add(JobHeader.Schema.TableName, 10);

			return (maxTableHits, maxDbLoadCount);
		}

		// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
		static (Dictionary<string, int> maxTableHits, int maxDbLoadCount) GetExpectedHitsForCreateTransactionsAndSave(bool oneChargePerJob)
		{
			var maxTableHits = new Dictionary<string, int>();
			maxTableHits.Add(AccChargeGLPostingOverrideSchema.Constants.TableName, 3);
			maxTableHits.Add(AccChargeTaxOverrideSchema.Constants.TableName, 1);
			maxTableHits.Add(AccGLHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(AccGLHeaderSubAccountSchema.Constants.TableName, 1);
			maxTableHits.Add(AccPeriodManagementSchema.Constants.TableName, 1);
			maxTableHits.Add(AccTransactionHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(AccTaxConfigurationSchema.Constants.TableName, 1);
			maxTableHits.Add(AccSurchargeApplicationSchema.Constants.TableName, 1);

			maxTableHits.Add(CusEntryNumSchema.Constants.TableName, 10);
			maxTableHits.Add(CusHAWBSchema.Constants.TableName, 1);
			maxTableHits.Add(CusInBondHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(CusSCAHouseSchema.Constants.TableName, 1);

			maxTableHits.Add(JobChargeTargetSchema.Constants.TableName, 4);
			maxTableHits.Add(JobDeclarationSchema.Constants.TableName, 11);
			maxTableHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			maxTableHits.Add(JobHeaderSchema.Constants.TableName, 14);
			maxTableHits.Add(JobOrderHeaderSchema.Constants.TableName, 10);
			maxTableHits.Add(JobRequiredDocumentSchema.Constants.TableName, 0);

			maxTableHits.Add(OrgCommissionAgreementSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgCusCodeSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgRelatedPartySchema.Constants.TableName, 1);

			maxTableHits.Add(ProcessCompanyLinkRuleSchema.Constants.TableName, 1);
			maxTableHits.Add(ProcessTasksSchema.Constants.TableName, 1);
			maxTableHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 2);

			maxTableHits.Add(RefZoneHeaderSchema.Constants.TableName, 2);
			maxTableHits.Add(RefZonePivotSchema.Constants.TableName, 2);

			maxTableHits.Add(WhsDocketJobPivotSchema.Constants.TableName, 1);

			int maxDbLoadCount;
			if (!oneChargePerJob)
			{
				maxDbLoadCount = 101;
				maxTableHits.Add(AccTransactionLinesSchema.Constants.TableName, 10);
				maxTableHits.Add(AccAlternateGLAccountDissectionSchema.Constants.TableName, 1);
				// There are 4 JR_AL_APLine hint queries added in ModifiedWIPAccrualRelatedChargeFetchHintService. Plus 1 JR_AH hint query added in previous logic.
				// None of them gets combined in Hint Fetch logic. Because they either have different hint key, or exceed number limit of query elements (MAXIMUM_PARAMETER_COUNT).
				// Hence all together 5 DB hits. Similar calculation applies to oneChargePerJob scenario.
				// For details, may refer to WI00258127 eDoc.
				maxTableHits.Add(JobChargeSchema.Constants.TableName, 5);
			}
			else
			{
				maxDbLoadCount = 104;
				maxTableHits.Add(AccTransactionLinesSchema.Constants.TableName, 20);
				maxTableHits.Add(AccAlternateGLAccountDissectionSchema.Constants.TableName, 1);
				maxTableHits.Add(JobChargeSchema.Constants.TableName, 2);
			}

			return (maxTableHits, maxDbLoadCount);
		}

		// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
		static (Dictionary<string, int> maxTableHits, int maxDbLoadCount) GetExpectedHitsForCreateTransactionsInNewFactory(bool oneChargePerJob)
		{
			var maxTableHits = new Dictionary<string, int>();
			maxTableHits.Add(AccChargeCodeSchema.Constants.TableName, 1);
			maxTableHits.Add(AccChargeGLPostingOverrideSchema.Constants.TableName, 3);
			maxTableHits.Add(AccChargeRevRecOverrideSchema.Constants.TableName, 1);
			maxTableHits.Add(AccChargeTaxOverrideSchema.Constants.TableName, 1);
			maxTableHits.Add(AccChargeTypeOverrideSchema.Constants.TableName, 2);
			maxTableHits.Add(AccGLHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(AccGLHeaderSubAccountSchema.Constants.TableName, 1);
			maxTableHits.Add(AccPeriodManagementSchema.Constants.TableName, 2);
			maxTableHits.Add(AccSurchargeApplicationSchema.Constants.TableName, 1);
			maxTableHits.Add(AccTaxConfigurationSchema.Constants.TableName, 1);
			maxTableHits.Add(AccTaxRateSchema.Constants.TableName, 1);

			maxTableHits.Add(CusEntryNumSchema.Constants.TableName, 10);
			maxTableHits.Add(CusHAWBSchema.Constants.TableName, 10);
			maxTableHits.Add(CusInBondHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(CusSCAHouseSchema.Constants.TableName, 10);

			maxTableHits.Add(JobChargeRevRecognitionSchema.Constants.TableName, 1);
			maxTableHits.Add(JobConShipLinkSchema.Constants.TableName, 10);
			maxTableHits.Add(JobDeclarationSchema.Constants.TableName, 2);
			maxTableHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			maxTableHits.Add(JobHeaderSchema.Constants.TableName, 11);
			maxTableHits.Add(JobShipmentSchema.Constants.TableName, 10);

			maxTableHits.Add(OrgAddressSchema.Constants.TableName, 2);
			maxTableHits.Add(OrgAddressCapabilitySchema.Constants.TableName, 1);
			maxTableHits.Add(OrgARTermsSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgCompanyDataSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgCusCodeSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgInvoiceTypeSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgMiscServSchema.Constants.TableName, 1);
			maxTableHits.Add(OrgRelatedPartySchema.Constants.TableName, 1);
			maxTableHits.Add(OrgStaffAssignmentsSchema.Constants.TableName, 1);

			maxTableHits.Add(RefAccTaxRateSchema.Constants.TableName, 2);

			maxTableHits.Add(ViewGenericJobSchema.Constants.TableName, 2);

			int maxDbLoadCount;
			if (!oneChargePerJob)
			{
				maxDbLoadCount = 105;
				maxTableHits.Add(AccTransactionLinesSchema.Constants.TableName, 7);
				maxTableHits.Add(AccAlternateGLAccountDissectionSchema.Constants.TableName, 1);
				maxTableHits.Add(JobChargeSchema.Constants.TableName, 2);
			}
			else
			{
				maxDbLoadCount = 99;
				maxTableHits.Add(AccTransactionLinesSchema.Constants.TableName, 1);
				maxTableHits.Add(AccAlternateGLAccountDissectionSchema.Constants.TableName, 1);
				maxTableHits.Add(JobChargeSchema.Constants.TableName, 2);
			}

			return (maxTableHits, maxDbLoadCount);
		}

		static string[] GetAllTablesToMonitorForDbHits()
			=> GetExpectedHitsForLoadJobs(false).maxTableHits.Keys
				.Union(GetExpectedHitsForPreSaveValidation(0).maxTableHits.Keys)
				.Union(GetExpectedHitsForCreateTransactionsAndSave(false).maxTableHits.Keys)
				.Union(GetExpectedHitsForCreateTransactionsInNewFactory(false).maxTableHits.Keys)
				.ToArray();

		#endregion

		#endregion

		public void TestCreditLimitWebServiceHitsWithAdditionalFactory()
		{
			int jobCount = 10;
			int chargesPerJobCount = 20;
			int chargesToPostCount = jobCount * chargesPerJobCount;

			var jobs = new List<Job>(jobCount);
			var chargePKs = new List<ZGuid>(chargesToPostCount);
			CreateDataForTestingDbHitCount(jobs, chargePKs, jobCount, chargesPerJobCount, oneChargePerJob: false);

			MockCreditLimitServiceClientProvider.Reset();
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			var testFactory = new BusinessObjectFactory();
			var periodicInvoice = new PeriodicInvoice(testFactory);
			periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;

			periodicInvoice.PostDate = ZDateTime.Today;
			periodicInvoice.DueDate = ZDateTime.Today.AddDays(30);
			periodicInvoice.InvoiceDate = ZDateTime.Today;
			periodicInvoice.InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			periodicInvoice.InvoiceTermDays = 30;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			periodicInvoice.LoadJobs();
			AssertEquals("Job Count", jobCount, periodicInvoice.Jobs.Count);
			AssertEquals("Selected Jobs Count", jobCount, periodicInvoice.SelectedJobs.Count());
			AssertEquals("Charges Count", chargesToPostCount, periodicInvoice.Charges.Count);
			AssertEquals("LoadJobs() does not trigger credit check web service", 0, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

			periodicInvoice.RunPreSaveValidation();

			AssertEquals(string.Format("Errors: {0}", periodicInvoice.NotificationsIncludingChildren.ToUniqueMessageListString()), periodicInvoice.HasErrors, false);
			AssertEquals("RunPreSaveValidation() does hit credit check web service", 1, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
			MockCreditLimitServiceClientProvider.Reset();

			// PeriodicInvoiceForm and PeriodicInvoiceBulk call CreateTransactions() on a copy of the periodic invoice in a different factory.
			// As lots of caching is scoped to a factory, we need to keep a careful eye on performance.
			var periodicInvoiceInNewFactory = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();
			using (periodicInvoiceInNewFactory.Factory.EnableTableHitQueryCollection(GetAllTablesToMonitorForDbHits()))
			{
				periodicInvoiceInNewFactory.CreateTransactions();

				AssertEquals("CreateTransactions() should not hit credit check web service, because PreSaveValidation() already ran (even if the bizo is in a different factory)", 0, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
				MockCreditLimitServiceClientProvider.Reset();
			}

			using (MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
			{
				periodicInvoice.CreateTransactions();
				AssertEquals("CreateTransactions() should not hit credit check web service, because credit check is cached at the factory level", 0, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);

				periodicInvoice.Factory.Save();
				AssertEquals("Factory.Save() should hit credit check web service, because credit check cache is cleared in InvoicingBase.OnSaved()", 1, MockCreditLimitServiceClientProvider.CountCheckCreditLimitAndBalanceDetailsWasInvoked);
				MockCreditLimitServiceClientProvider.Reset();
			}
		}

		#region Helpers

		void AssertHitCounts(BusinessObjectFactory testFactory, Dictionary<string, int> maxDbHitsPerTable, int maxFactoryDatabaseLoadCount)
		{
			CombineAssertions(delegate
			{
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				AssertMaxDbHits(maxDbHitsPerTable, testFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: false);

				var unexpectedTables = testFactory.TableSelects.Where(x => !maxDbHitsPerTable.Keys.Contains(x.TableName));
				var unexpectedTableMessage = string.Join(", ", unexpectedTables.Select(t => $"{t.TableName}: {t.Value}"));
				var unlocoCountThatWeDontCareAbout = testFactory.GetTableHitCount(RefUNLOCOSchema.Constants.TableName);
				AssertGreaterThanOrEqualTo($"Unexpected testFactory DatabaseLoadCount. List of unexpected tables:\r\n{unexpectedTableMessage}", maxFactoryDatabaseLoadCount + unlocoCountThatWeDontCareAbout, testFactory.DatabaseLoadCount);
			});
		}

		void CreateDataForTestingDbHitCount(List<Job> jobs, List<ZGuid> chargePKs, int jobCount, int chargesPerJobCount, bool oneChargePerJob)
		{
			TestObjectCreator.ABIGAS.OH_IsDebtor = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARCreditLimit = 100m;
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_APCreditLimit = 150m;

			for (int i = 0; i < jobCount; i++)
			{
				var shipment = TestObjectCreator.CreateShipment(string.Format("S90001{0}", i.ToString().PadLeft(3, '0')));
				shipment.JS_RS_NKServiceLevel = "STD";
				Job job = TestObjectCreator.CreateJob(shipment);
				job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
				jobs.Add(job);

				for (int j = 0; j < chargesPerJobCount; j++)
				{
					Charge charge = job.Charges.AddNew();
					charge.FillWithValidTestData();
					charge.JR_AC = TestObjectCreator.CC1.PK;
					charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
					charge.JR_OSSellAmt = 100m;
					charge.JR_LocalSellAmt = 100m;
					if (!oneChargePerJob || j == 0)
					{
						charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
						chargePKs.Add(charge.PK);
					}
					else
					{
						charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
					}
				}
			}

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			OrgInvoiceType type = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";

			Factory.Save();
			ReleaseFactory();
		}

		void AssertInvoiceCreatedAndChargesModified(PeriodicInvoice periodicInvoice, List<ZGuid> chargePKs, int chargesToPostCount)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			AssertEquals("Invoice Count", 1, periodicInvoice.PostManager.Poster.PostedInvoices.Count);

			var query = new ZQuery(AccTransactionHeaderSchema.PK, periodicInvoice.ARTransactionsCreatedForPosting.Select(x => x.PK));
			var invoices = newFactory.Load<ARInvoice>(query);
			AssertEquals("Invoice Count", 1, invoices.Length);
			AssertEquals("PostedARTransactions count", 1, periodicInvoice.ARTransactionsCreatedForPosting.Length);
			AssertEquals("PostedARTransactions", invoices[0].PK, periodicInvoice.ARTransactionsCreatedForPosting[0].PK);

			List<ZGuid> linePKs = new List<ZGuid>(invoices[0].Lines.Select(x => x.PK));

			AssertEquals("Charge Count", chargesToPostCount, chargePKs.Count);

			foreach (ZGuid pk in chargePKs)
			{
				Charge charge = newFactory.Load<Charge>(pk);
				AssertNotNull("Charge should exist in database", charge);
				AssertNotEquals("Charge must have JR_AL_ARLine set", ZGuid.Empty, charge.JR_AL_ARLine);
				AssertEquals("JR_AL_ARLine should have corresponding line on Invoice", true, linePKs.Contains(charge.JR_AL_ARLine));
			}
		}

		#endregion

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_internal ?? (TestObjectCreator_internal = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_internal;
	}
}
