using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class CommissionAgreementApproverTest : TestCaseWithFactory
	{
		#region ApproveAndQueue

		public void TestApproveAndQueue_CheckMustBeInTransaction()
		{
			var approver = CommissionAgreementApprover.New(Factory);
			AssertExceptionThrown(typeof(InvalidOperationException), "This method can only be called within a transaction.", () =>
			{
				approver.ApproveAndQueue(new CreateCommissionContext(), null);
			});
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestThrowsIfLocked()
		{
			var testData = CommissionAgreementApproverTestData.Create(Factory);

			var context = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementADraft, testData.AgreementB, testData.AgreementCNewDraft })
			};
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				if (!connection.TryGetLock(CommissionAgreementApprover.AgreementLockPrefix + testData.AgreementB.PK, out SqlApplicationLock sqlLock))
				{
					Fail("Unable to get lock");
				}

				using (sqlLock)
				using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
				{
					try
					{
						var approver = CommissionAgreementApprover.New(Factory);
						approver.ApproveAndQueue(context, null);

						transactionManager.CommitTransaction();
						Fail("Should have thrown because agreement is locked");
					}
					catch (OrgCommissionAgreementBeingProcessedException)
					{
						Assert(true);
					}
				}
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestSecondApproveDeletesOldQueue()
		{
			var testData = CommissionAgreementApproverTestData.Create(Factory);

			var context = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementADraft, testData.AgreementB, testData.AgreementCNewDraft })
			};
			using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
			{
				try
				{
					var approver = CommissionAgreementApprover.New(Factory);
					approver.ApproveAndQueue(context, null);

					var queue1 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementADraft.PK));
					var queue2 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementB.PK));
					var queue3 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementCNewDraft.PK));

					AssertEquals(0, queue1.Length);
					AssertEquals(1, queue2.Length);
					AssertEquals(1, queue3.Length);

					var queuePK2 = queue2[0].PK;
					var queuePK3 = queue3[0].PK;

					approver.ApproveAndQueue(context, null);

					queue1 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementADraft.PK));
					queue2 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementB.PK));
					queue3 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementCNewDraft.PK));

					AssertEquals(0, queue1.Length);
					AssertEquals(1, queue2.Length);
					AssertEquals(1, queue3.Length);

					AssertNotEquals(queuePK2, queue2[0].PK);
					AssertNotEquals(queuePK3, queue3[0].PK);

					transactionManager.CommitTransaction();
				}
				catch (OrgCommissionAgreementBeingProcessedException)
				{
				}
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestCreateCommissionDeletesOldQueue()
		{
			var testData = CommissionAgreementApproverTestData.Create(Factory);

			var context = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementADraft, testData.AgreementB, testData.AgreementCNewDraft })
			};
			using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
			{
				try
				{
					var approver = CommissionAgreementApprover.New(Factory);
					approver.ApproveAndQueue(context, null);

					var queue1 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementADraft.PK));
					var queue2 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementB.PK));
					var queue3 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementCNewDraft.PK));

					AssertEquals(0, queue1.Length);
					AssertEquals(1, queue2.Length);
					AssertEquals(1, queue3.Length);

					var queuePK2 = queue2[0].PK;
					var queuePK3 = queue3[0].PK;

					approver.CreateCommissions(context, null);

					queue1 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementADraft.PK));
					queue2 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementB.PK));
					queue3 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, testData.AgreementCNewDraft.PK));

					AssertEquals(0, queue1.Length);
					AssertEquals(0, queue2.Length);
					AssertEquals(0, queue3.Length);

					transactionManager.CommitTransaction();
				}
				catch (OrgCommissionAgreementBeingProcessedException)
				{
				}
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		[TestDate(2002, 2, 2)]
		public void TestApproveAndQueueAgreements()
		{
			var testData = CommissionAgreementApproverTestData.Create(Factory);

			var context = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementADraft, testData.AgreementB, testData.AgreementCNewDraft }),
				FromDate = ZDateTime.BrettsBirthday,
				OverwriteOldValues = true
			};

			int factorySaves = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(_ => ++factorySaves);

			using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
			{
				try
				{
					var approver = CommissionAgreementApprover.New(Factory);
					approver.ApproveAndQueue(context, null);

					transactionManager.CommitTransaction();
				}
				catch
				{
				}
			}

			AssertEquals("Calls to Factory Save", 4, factorySaves);

			AssertEquals(new ZDateTime(2002, 2, 2), testData.AgreementA.CA0_LastApprovedDateUtc);
			AssertEquals("should delete draft", true, testData.AgreementADraft.IsDeleted);

			AssertEquals(new ZDateTime(2002, 2, 2), testData.AgreementB.CA0_LastApprovedDateUtc);
			AssertEquals("should not delete draft: the main version was approved - not the draft", false, testData.AgreementBDraft.IsDeleted);

			AssertEquals(new ZDateTime(2002, 2, 2), testData.AgreementCNewDraft.CA0_LastApprovedDateUtc);
			AssertEquals("no longer a draft", false, testData.AgreementCNewDraft.IsDraft);

			Assert(context.AgreementsBeingApproved.Count > 0);
			foreach (var agreement in context.AgreementsBeingApproved)
			{
				var queue = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, SQLComparisonOperator.Equal, agreement.PK));
				AssertEquals(1, queue.Length);
				AssertEquals(ZDateTime.BrettsBirthday, queue[0].CAQ_MinimumInvoicePostedDate);
				AssertEquals(true, queue[0].CAQ_OverwriteExistingCommissions);
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestApprovalLogsAdded()
		{
			var testData = CommissionAgreementApproverTestData.Create(Factory);

			var context1 = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementA }),
				FromDate = ZDateTime.MinSmallDateTimeValue,
				OverwriteOldValues = true
			};

			var context2 = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementBDraft }),
				FromDate = new ZDateTime(2021, 04, 14),
				OverwriteOldValues = false
			};

			using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
			{
				try
				{
					var approver = CommissionAgreementApprover.New(Factory);
					approver.ApproveAndQueue(context1, null);
					approver.ApproveAndCreateCommissions(context2, null);

					transactionManager.CommitTransaction();
				}
				catch
				{
				}
			}

			Assert("Approval information logs should be added", testData.AgreementA.Logs.Find(x => x.SL_Reference == "Approved: From Date = ALL, Rework Old Commissions = Y").Any());
			Assert("Approval information logs should be added", testData.AgreementB.Logs.Find(x => x.SL_Reference == "Approved: From Date = 14-Apr-21, Rework Old Commissions = N").Any());
		}

		#endregion

		#region CreateCommissions

		public void TestCreateCommissions_CheckMustBeInTransaction()
		{
			var approver = CommissionAgreementApprover.New(Factory);
			AssertExceptionThrown(typeof(InvalidOperationException), "This method can only be called within a transaction.", () =>
			{
				approver.ApproveAndCreateCommissions(new CreateCommissionContext(), null);
			});
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestGetNonJobRelatedInvoiceFilter()
		{
			var testData = CommissionAgreementApproverTestData.Create(Factory);

			var approver = CommissionAgreementApprover.New(Factory);
			var agreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementADraft, testData.AgreementBDraft });
			{
				var createContext = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, OverwriteOldValues = false, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should exclude InvoiceA_2001_WithExistingCommission since should not overwrite old values",
					BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
					new AccTransactionHeader[]
					{
						testData.InvoiceA_2002,
						testData.JobRevenueJournalA_2002,
						testData.InvoiceB_2001_WithCancelledCommission,
						testData.InvoiceB_2002,
					},
					Factory.Load<AccTransactionHeader>(approver.GetNonJobRelatedTransactionFilterForTesting(createContext)));

				createContext = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, OverwriteOldValues = true, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should include all invoices, since overwriting old values",
					BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
					new AccTransactionHeader[]
					{
						testData.InvoiceA_2001_WithExistingCommission,
						testData.InvoiceA_2002,
						testData.JobRevenueJournalA_2002,
						testData.InvoiceB_2001_WithCancelledCommission,
						testData.InvoiceB_2002,
					},
					Factory.Load<AccTransactionHeader>(approver.GetNonJobRelatedTransactionFilterForTesting(createContext)));
			}

			{
				var createContext = new CreateCommissionContext() { FromDate = new ZDateTime(2002, 2, 2), OverwriteOldValues = false, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should only include invoices after FromDate",
					BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
					new AccTransactionHeader[]
					{
						testData.InvoiceA_2002,
						testData.JobRevenueJournalA_2002,
						testData.InvoiceB_2002,
					},
					Factory.Load<AccTransactionHeader>(approver.GetNonJobRelatedTransactionFilterForTesting(createContext)));

				createContext = new CreateCommissionContext() { FromDate = new ZDateTime(2002, 2, 2), OverwriteOldValues = true, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should only include invoices after FromDate",
					BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
					new AccTransactionHeader[]
					{
						testData.InvoiceA_2002,
						testData.JobRevenueJournalA_2002,
						testData.InvoiceB_2002,
					},
					Factory.Load<AccTransactionHeader>(approver.GetNonJobRelatedTransactionFilterForTesting(createContext)));
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestGetJobsFilter()
		{
			var testData = CommissionAgreementApproverTestData.Create(Factory);

			var approver = CommissionAgreementApprover.New(Factory);
			var agreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { testData.AgreementADraft, testData.AgreementBDraft, testData.AgreementCNewDraft });
			{
				var createContext = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, OverwriteOldValues = false, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should exclude JobA_AllInvoicesWithExistingCommission since not overwriting old commissions",
					BusinessObjectEqualityComparer<JobHeader>.PKOnlyComparer,
					new[]
					{
						testData.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission,
						testData.JobC_AllInvoicesWithCancelledCommission,
						testData.JobD_Apportioned,
						testData.JobE_Apportioned,
					},
					Factory.Load<JobHeader>(approver.GetJobsFilterForTesting(createContext)));

				createContext = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, OverwriteOldValues = true, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should include all jobs since we are overwriting old values",
					BusinessObjectEqualityComparer<JobHeader>.PKOnlyComparer,
					new[]
					{
						testData.JobA_AllInvoicesWithExistingCommission,
						testData.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission,
						testData.JobC_AllInvoicesWithCancelledCommission,
						testData.JobD_Apportioned,
						testData.JobE_Apportioned,
					},
					Factory.Load<JobHeader>(approver.GetJobsFilterForTesting(createContext)));
			}

			{
				var createContext = new CreateCommissionContext() { FromDate = new ZDateTime(2002, 2, 2), OverwriteOldValues = false, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should exclude JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission since no invoices without existing commission after From Date",
					BusinessObjectEqualityComparer<JobHeader>.PKOnlyComparer,
					new[]
					{
						testData.JobC_AllInvoicesWithCancelledCommission,
						testData.JobD_Apportioned,
						testData.JobE_Apportioned,
					},
					Factory.Load<JobHeader>(approver.GetJobsFilterForTesting(createContext)));

				createContext = new CreateCommissionContext() { FromDate = new ZDateTime(2002, 2, 2), OverwriteOldValues = true, AgreementsBeingApproved = agreementsBeingApproved };
				AssertContainsExactElementsInAnyOrder("Should include all jobs since we are overwriting old values",
					BusinessObjectEqualityComparer<JobHeader>.PKOnlyComparer,
					new[]
					{
						testData.JobA_AllInvoicesWithExistingCommission,
						testData.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission,
						testData.JobC_AllInvoicesWithCancelledCommission,
						testData.JobD_Apportioned,
						testData.JobE_Apportioned,
					},
					Factory.Load<JobHeader>(approver.GetJobsFilterForTesting(createContext)));
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestEffectiveDateCache()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			invoice.AH_JH = job.PK;
			invoice.AH_PostDate = new ZDateTime(2001, 1, 1);

			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var arLine = invoice.Lines.AddNew();
			arLine.AL_LineType = TransactionLineTypes.Revenue;
			arLine.AL_JH = job.PK;
			arLine.AL_AC = charge.PK;
			arLine.ExchangeRate.Currency = audCurrency.RX_Code;
			arLine.ExchangeRate.Rate = 1;
			arLine.AL_OSExTaxAmount = 2000;

			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_AC = charge.PK;
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_RX_NKSellCurrency = audCurrency.RX_Code;
			jobCharge.JR_AL_ARLine = arLine.PK;
			jobCharge.JR_LocalSellAmt = 2000;
			jobCharge.JR_OSSellAmt = 2000;

			Factory.Save();

			job.JH_A_JCL = new ZDateTime(2001, 1, 1);
			job.JH_Status = JobHeaderStatus.Closed.Code;

			Factory.Save();

			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);
			var agreement = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity, customer);
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement, adlStaff, 10);
			var agreementDraft = agreement.CreateDraft();

			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var agreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { agreementDraft });
				var createContext = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, OverwriteOldValues = true, AgreementsBeingApproved = agreementsBeingApproved };

				using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
				{
					var approver = CommissionAgreementApprover.New(Factory);
					approver.ApproveAndCreateCommissions(createContext, null);

					transactionManager.CommitTransaction();
				}

				int accountingTableCount = 0;
				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans.Where(t => t.Item1.Contains("ViewCommissionAgreementOverallItem"));
				foreach (var queryPlan in queryPlans)
				{
					var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
					var headerCount = queryPlanAnalyzer.IndexSeeks.Count(i => i.TableName == "AccTransactionHeader");
					var linesCount = queryPlanAnalyzer.IndexSeeks.Count(i => i.TableName == "AccTransactionLines");
					if (headerCount > 0 || linesCount > 0)
					{
						accountingTableCount++;
					}
				}

				AssertEquals("Accounting Tables should not be hit", 0, accountingTableCount);

				var headerQuery = new ZDBOnlyQuery(typeof(AccCommissionHeader));
				headerQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OH_Customer, customer.PK);

				AssertEquals("Commission Headers created", 1, Factory.GetDatabaseCount(typeof(AccCommissionHeader), headerQuery));
			}
		}

		#endregion
	}

	public class CommissionAgreementApproverTestData
	{
		public static CommissionAgreementApproverTestData Create(BusinessObjectFactory factory)
		{
			var result = new CommissionAgreementApproverTestData()
			{
				CustomerA = factory.NewWithValidTestData<OrgHeader>(),
				CustomerB = factory.NewWithValidTestData<OrgHeader>(),
				CustomerC = factory.NewWithValidTestData<OrgHeader>()
			};
			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(factory, result.CustomerA);
			result.AgreementA = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity, result.CustomerA);
			result.AgreementADraft = result.AgreementA.CreateDraft();

			result.AgreementB = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity, result.CustomerB);
			result.AgreementBDraft = result.AgreementB.CreateDraft();

			result.AgreementCNewDraft = opportunity.CommissionAgreements.AddNew();
			result.AgreementCNewDraft.FillWithValidTestData();
			result.AgreementCNewDraft.CA0_OH_Customer = result.CustomerC.PK;
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(result.AgreementCNewDraft, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			result.InvoiceA_2001_WithExistingCommission = factory.NewWithValidTestData<ARInvoice>();
			result.InvoiceA_2001_WithExistingCommission.AH_OH = result.CustomerA.PK;
			result.InvoiceA_2001_WithExistingCommission.AH_PostDate = new ZDateTime(2001, 1, 1);

			result.InvoiceA_2002 = factory.NewWithValidTestData<ARCreditNote>();
			result.InvoiceA_2002.AH_OH = result.CustomerA.PK;
			result.InvoiceA_2002.AH_PostDate = new ZDateTime(2002, 2, 2);

			result.JobRevenueJournalA_2002 = factory.NewWithValidTestData<JobRevenueJournal>();
			result.JobRevenueJournalA_2002.AH_OH = result.CustomerA.PK;
			result.JobRevenueJournalA_2002.AH_PostDate = new ZDateTime(2002, 2, 2);

			var invoiceA_2001reversing = new ARInvoiceReversing(result.InvoiceA_2001_WithExistingCommission);
			invoiceA_2001reversing.Reverse();
			result.InvoiceReversalA_2001 = (ARCreditNote)result.InvoiceA_2001_WithExistingCommission.ReverseInvoice;
			result.InvoiceReversalA_2001.AH_PostDate = new ZDateTime(2001, 1, 1);

			var invoiceA_2002reversing = new InvoicingBaseReversing(result.InvoiceA_2002);
			invoiceA_2002reversing.Reverse();
			result.InvoiceReversalA_2002 = (ARInvoice)result.InvoiceA_2002.ReverseInvoice;
			result.InvoiceReversalA_2002.AH_PostDate = new ZDateTime(2001, 1, 1);

			result.InvoiceB_2001_WithCancelledCommission = factory.NewWithValidTestData<ARInvoice>();
			result.InvoiceB_2001_WithCancelledCommission.AH_OH = result.CustomerB.PK;
			result.InvoiceB_2001_WithCancelledCommission.AH_PostDate = new ZDateTime(2001, 1, 1);

			result.InvoiceB_2002 = factory.NewWithValidTestData<ARCreditNote>();
			result.InvoiceB_2002.AH_OH = result.CustomerB.PK;
			result.InvoiceB_2002.AH_PostDate = new ZDateTime(2002, 2, 2);

			result.InvoiceC_2001 = factory.NewWithValidTestData<ARInvoice>();
			result.InvoiceC_2001.AH_OH = result.CustomerC.PK;
			result.InvoiceC_2001.AH_PostDate = new ZDateTime(2001, 1, 1);

			result.InvoiceC_2002 = factory.NewWithValidTestData<ARCreditNote>();
			result.InvoiceC_2002.AH_OH = result.CustomerC.PK;
			result.InvoiceC_2002.AH_PostDate = new ZDateTime(2002, 2, 2);

			var shipmentA = factory.NewWithValidTestData<ForwardingShipment>();
			result.JobA_AllInvoicesWithExistingCommission = new JobHeader.Loader(shipmentA).TryCreate();
			result.JobA_AllInvoicesWithExistingCommission.JH_OA_LocalChargesAddr = result.CustomerA.MainAddress.PK;
			result.JobInvoiceA_2001_WithExistingCommission = factory.NewWithValidTestData<ARInvoice>();
			result.JobInvoiceA_2001_WithExistingCommission.AH_JH = result.JobA_AllInvoicesWithExistingCommission.PK;
			result.JobInvoiceA_2001_WithExistingCommission.AH_PostDate = new ZDateTime(2001, 1, 1);
			result.JobInvoiceA_2002_WithExistingCommission = factory.NewWithValidTestData<ARCreditNote>();
			result.JobInvoiceA_2002_WithExistingCommission.AH_JH = result.JobA_AllInvoicesWithExistingCommission.PK;
			result.JobInvoiceA_2002_WithExistingCommission.AH_PostDate = new ZDateTime(2002, 2, 2);

			var shipmentB = factory.NewWithValidTestData<ForwardingShipment>();
			result.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission = new JobHeader.Loader(shipmentB).TryCreate();
			result.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission.JH_OA_LocalChargesAddr = result.CustomerB.MainAddress.PK;
			result.JobInvoiceB_2001 = factory.NewWithValidTestData<ARInvoice>();
			result.JobInvoiceB_2001.AH_JH = result.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission.PK;
			result.JobInvoiceB_2001.AH_PostDate = new ZDateTime(2001, 1, 1);
			result.JobInvoiceB_2002_WithExistingCommission = factory.NewWithValidTestData<ARCreditNote>();
			result.JobInvoiceB_2002_WithExistingCommission.AH_JH = result.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission.PK;
			result.JobInvoiceB_2002_WithExistingCommission.AH_PostDate = new ZDateTime(2002, 2, 2);

			var shipmentC = factory.NewWithValidTestData<ForwardingShipment>();
			result.JobC_AllInvoicesWithCancelledCommission = new JobHeader.Loader(shipmentC).TryCreate();
			result.JobC_AllInvoicesWithCancelledCommission.JH_OA_LocalChargesAddr = result.CustomerC.MainAddress.PK;
			result.JobInvoiceC_2001_WithCancelledCommission = factory.NewWithValidTestData<ARInvoice>();
			result.JobInvoiceC_2001_WithCancelledCommission.AH_JH = result.JobC_AllInvoicesWithCancelledCommission.PK;
			result.JobInvoiceC_2001_WithCancelledCommission.AH_PostDate = new ZDateTime(2001, 1, 1);
			result.JobInvoiceC_2002_WithCancelledCommission = factory.NewWithValidTestData<ARCreditNote>();
			result.JobInvoiceC_2002_WithCancelledCommission.AH_JH = result.JobC_AllInvoicesWithCancelledCommission.PK;
			result.JobInvoiceC_2002_WithCancelledCommission.AH_PostDate = new ZDateTime(2002, 2, 2);

			var shipmentD = factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentE = factory.NewWithValidTestData<ForwardingShipment>();
			result.JobD_Apportioned = new Job.Loader(shipmentD).TryCreate();
			result.JobD_Apportioned.JH_OA_LocalChargesAddr = result.CustomerA.MainAddress.PK;
			result.JobE_Apportioned = new Job.Loader(shipmentE).TryCreate();
			result.JobE_Apportioned.JH_OA_LocalChargesAddr = result.CustomerA.MainAddress.PK;

			result.ApportionedInvoice_2002 = factory.NewWithValidTestData<APInvoice>();
			result.ApportionedInvoice_2002.AH_JH = ZGuid.Empty;
			result.ApportionedInvoice_2002.AH_PostDate = new ZDateTime(2002, 2, 2);
			var apportionLineD = (TransactionLine)result.ApportionedInvoice_2002.Lines.AddNew();
			apportionLineD.FillWithValidTestData();
			apportionLineD.AL_JH = result.JobD_Apportioned.PK;
			var chargeD = result.JobD_Apportioned.Charges.AddNew();
			chargeD.JR_AL_APLine = apportionLineD.PK;
			chargeD.FillWithValidTestData();
			var apportionLineE = (TransactionLine)result.ApportionedInvoice_2002.Lines.AddNew();
			apportionLineE.FillWithValidTestData();
			apportionLineE.AL_JH = result.JobE_Apportioned.PK;
			var chargeE = result.JobE_Apportioned.Charges.AddNew();
			chargeE.JR_AL_APLine = apportionLineE.PK;
			chargeE.FillWithValidTestData();

			var commissionInvoiceA_2001 = factory.NewWithValidTestData<AccCommissionHeader>();
			commissionInvoiceA_2001.CH0_AH_Source = result.InvoiceA_2001_WithExistingCommission.PK;
			commissionInvoiceA_2001.CH0_GroupingSourceTableCode = result.InvoiceA_2001_WithExistingCommission.TablePrefix;
			commissionInvoiceA_2001.CH0_GroupingSourceID = result.InvoiceA_2001_WithExistingCommission.PK;
			commissionInvoiceA_2001.Lines.AddNew().FillWithValidTestData();

			var cancelledCommissionInvoiceB_2001 = factory.NewWithValidTestData<AccCommissionHeader>();
			cancelledCommissionInvoiceB_2001.CH0_AH_Source = result.InvoiceB_2001_WithCancelledCommission.PK;
			cancelledCommissionInvoiceB_2001.CH0_GroupingSourceTableCode = result.InvoiceB_2001_WithCancelledCommission.TablePrefix;
			cancelledCommissionInvoiceB_2001.CH0_GroupingSourceID = result.InvoiceB_2001_WithCancelledCommission.PK;
			cancelledCommissionInvoiceB_2001.CH0_OverridenDateTimeUtc = new ZDateTime(2002, 2, 2);
			cancelledCommissionInvoiceB_2001.Lines.AddNew().FillWithValidTestData();

			var commissionJobInvoiceA_2001 = factory.NewWithValidTestData<AccCommissionHeader>();
			commissionJobInvoiceA_2001.CH0_AH_Source = result.JobInvoiceA_2001_WithExistingCommission.PK;
			commissionJobInvoiceA_2001.CH0_GroupingSourceTableCode = result.JobA_AllInvoicesWithExistingCommission.TablePrefix;
			commissionJobInvoiceA_2001.CH0_GroupingSourceID = result.JobA_AllInvoicesWithExistingCommission.PK;
			commissionJobInvoiceA_2001.Lines.AddNew().FillWithValidTestData();

			var commissionJobInvoiceA_2002 = factory.NewWithValidTestData<AccCommissionHeader>();
			commissionJobInvoiceA_2002.CH0_AH_Source = result.JobInvoiceA_2002_WithExistingCommission.PK;
			commissionJobInvoiceA_2002.CH0_GroupingSourceTableCode = result.JobA_AllInvoicesWithExistingCommission.TablePrefix;
			commissionJobInvoiceA_2002.CH0_GroupingSourceID = result.JobA_AllInvoicesWithExistingCommission.PK;
			commissionJobInvoiceA_2002.Lines.AddNew().FillWithValidTestData();

			var commissionJobInvoiceB_2002 = factory.NewWithValidTestData<AccCommissionHeader>();
			commissionJobInvoiceB_2002.CH0_AH_Source = result.JobInvoiceB_2002_WithExistingCommission.PK;
			commissionJobInvoiceB_2002.CH0_GroupingSourceTableCode = result.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission.TablePrefix;
			commissionJobInvoiceB_2002.CH0_GroupingSourceID = result.JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission.PK;
			commissionJobInvoiceB_2002.Lines.AddNew().FillWithValidTestData();

			var commissionJobInvoiceC_2001 = factory.NewWithValidTestData<AccCommissionHeader>();
			commissionJobInvoiceC_2001.CH0_AH_Source = result.JobInvoiceC_2001_WithCancelledCommission.PK;
			commissionJobInvoiceC_2001.CH0_GroupingSourceTableCode = result.JobC_AllInvoicesWithCancelledCommission.TablePrefix;
			commissionJobInvoiceC_2001.CH0_GroupingSourceID = result.JobC_AllInvoicesWithCancelledCommission.PK;
			commissionJobInvoiceC_2001.CH0_OverridenDateTimeUtc = new ZDateTime(2002, 2, 2);
			commissionJobInvoiceC_2001.Lines.AddNew().FillWithValidTestData();

			var commissionJobInvoiceC_2002 = factory.NewWithValidTestData<AccCommissionHeader>();
			commissionJobInvoiceC_2002.CH0_AH_Source = result.JobInvoiceC_2002_WithCancelledCommission.PK;
			commissionJobInvoiceC_2002.CH0_GroupingSourceTableCode = result.JobC_AllInvoicesWithCancelledCommission.TablePrefix;
			commissionJobInvoiceC_2002.CH0_GroupingSourceID = result.JobC_AllInvoicesWithCancelledCommission.PK;
			commissionJobInvoiceC_2002.CH0_OverridenDateTimeUtc = new ZDateTime(2002, 2, 2);
			commissionJobInvoiceC_2002.Lines.AddNew().FillWithValidTestData();

			factory.Save();

			return result;
		}

		CommissionAgreementApproverTestData()
		{
		}

		public OrgHeader CustomerA;
		public OrgHeader CustomerB;
		public OrgHeader CustomerC;
		public OrgCommissionAgreement AgreementA;
		public OrgCommissionAgreement AgreementADraft;
		public OrgCommissionAgreement AgreementB;
		public OrgCommissionAgreement AgreementBDraft;
		public OrgCommissionAgreement AgreementCNewDraft;

		public ARInvoice InvoiceA_2001_WithExistingCommission;
		public ARCreditNote InvoiceA_2002;
		public JobRevenueJournal JobRevenueJournalA_2002;

		public ARCreditNote InvoiceReversalA_2001;
		public ARInvoice InvoiceReversalA_2002;

		public ARInvoice InvoiceB_2001_WithCancelledCommission;
		public ARCreditNote InvoiceB_2002;

		public ARInvoice InvoiceC_2001;
		public ARCreditNote InvoiceC_2002;

		public JobHeader JobA_AllInvoicesWithExistingCommission;
		public ARInvoice JobInvoiceA_2001_WithExistingCommission;
		public ARCreditNote JobInvoiceA_2002_WithExistingCommission;

		public JobHeader JobB_FirstInvoiceNoCommission_SecondInvoiceHasCommission;
		public ARInvoice JobInvoiceB_2001;
		public ARCreditNote JobInvoiceB_2002_WithExistingCommission;

		public JobHeader JobC_AllInvoicesWithCancelledCommission;
		public ARInvoice JobInvoiceC_2001_WithCancelledCommission;
		public ARCreditNote JobInvoiceC_2002_WithCancelledCommission;

		public APInvoice ApportionedInvoice_2002;
		public Job JobD_Apportioned;
		public Job JobE_Apportioned;
	}
}
