using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	// BulkJobCloseProcessor is tested in Module due to dependency on JobManagementFilterBusinessObject.GetAdditionalFilterForBulkJobClosure()

	[TestedType(typeof(BulkJobCloseProcessor))]
	public class BulkJobCloseProcessorTest : NonPersistentBusinessObjectTestCase
	{
		public class InnerBulkJobCloseProcessorTest : JobClosureProcessorTestHelper
		{
			public void TestBulkJobClosureFilterForActiveUnInvoicedARCashAdvanceRequestsAndCashAdvanceFunctionalityIsEnabled()
			{
				AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(true, true);
			}

			public void TestBulkJobClosureFilterForActiveUnInvoicedAPCashAdvanceRequestsAndCashAdvanceFunctionalityIsEnabled()
			{
				AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(false, true);
			}

			public void TestBulkJobClosureFilterForActiveUnInvoicedARCashAdvanceRequestsAndCashAdvanceFunctionalityIsDisabled()
			{
				AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(true, false);
			}

			public void TestBulkJobClosureFilterForActiveUnInvoicedAPCashAdvanceRequestsAndCashAdvanceFunctionalityIsDisabled()
			{
				AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(false, false);
			}

			void AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(bool isARCashAdvance, bool isCashAdvanceFunctionalityEnabled)
			{
				using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCashAdvanceFunctionalityEnabled))
				{
					var expectedJobsThatShouldBeClosed = new List<Job>();
					var shipment = TestObjectCreator.CreateShipment("S00001");
					var job = TestObjectCreator.CreateJob(shipment);
					expectedJobsThatShouldBeClosed.Add(job);

					var shipmentNumIncrement = 2;
					string GetShipmentNumber() => "S0000" + shipmentNumIncrement.ToString();
					var ledgerType = isARCashAdvance ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;

					foreach (var headerStatus in CashAdvanceStatusCodes.RequestHeader.CodesList.GetAllCodes())
					{
						shipment = TestObjectCreator.CreateShipment(GetShipmentNumber());
						job = TestObjectCreator.CreateJob(shipment);
						if (headerStatus == CashAdvanceStatusCodes.RequestHeader.Pending)
						{
							var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
							if (isARCashAdvance)
							{
								charge.JR_IsARCashAdvance = true;
							}
							else
							{
								charge.JR_IsAPCashAdvance = true;
							}
						}
						else
						{
							var cashAdvance = TestObjectCreator.CreateCashAdvanceRequestHeader(job.PK, TestObjectCreator.AALSHI.PK, ledgerType, 100m, 100m, TestObjectCreator.AUD.RX_Code, headerStatus);
							if (headerStatus == CashAdvanceStatusCodes.RequestHeader.PartiallyPaid)
							{
								cashAdvance.CAH_LocalPaidAmount = cashAdvance.CAH_OSPaidAmount = 50m;
							}
							else if (headerStatus == CashAdvanceStatusCodes.RequestHeader.Paid || headerStatus == CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced || headerStatus == CashAdvanceStatusCodes.RequestHeader.Invoiced)
							{
								cashAdvance.CAH_LocalPaidAmount = cashAdvance.CAH_OSPaidAmount = 100m;
							}
						}

						if (!isCashAdvanceFunctionalityEnabled && headerStatus != CashAdvanceStatusCodes.RequestHeader.Pending)
						{
							//Pending = unposted charge, exisiting SQL filter blocks deletion of this job.
							expectedJobsThatShouldBeClosed.Add(job);
						}
						else if (headerStatus == CashAdvanceStatusCodes.RequestHeader.Cancelled || headerStatus == CashAdvanceStatusCodes.RequestHeader.Invoiced)
						{
							expectedJobsThatShouldBeClosed.Add(job);
						}
						shipmentNumIncrement++;
					}
					Factory.Save();

					var processor = new BulkJobCloseProcessor((q) => ObjectFactory.Get<IAccountingModuleUtilityForTest>().GetAdditionalFilterForBulkJobClosure(q));
					processor.JobStatusFilter = JobHeaderStatus.Working.Code;
					processor.JobOpenDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
					processor.JobLastEditDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
					processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
					processor.JobOpenDateFilter.Property2 = ZDateTime.Today.AddDays(1);
					var msg = processor.Find();
					AssertEquals("No Error Message", string.Empty, msg);
					AssertContainsExactElementsInAnyOrder(expectedJobsThatShouldBeClosed.Select(x => x.PK), processor.JobPKs);
				}
			}

			public void TestBulkJobClosureClosesJob()
			{
				var jobTypes = JobInvoicingConsumerTypes.New();
				int i = 0;
				var allJobsToclose = new HashSet<ZGuid>();

				foreach (string item in AllowedJobTypeList)
				{
					i++;
					var job1 = CreateJob(jobTypes[item], Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentUtcDate.AddDays((-1) * i));
					allJobsToclose.Add(job1.PK);
				}

				Factory.Save();

				var processor = new BulkJobCloseProcessor();
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-(i + 5));
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today;

				var msg = processor.Find();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader", "Total Number of Jobs in the JobHeader Table", allJobsToclose.Count);
				AssertEquals("Total Number of Jobs to be closed", processor.JobPKs.Count, allJobsToclose.Count);
				AssertEquals("No Error Message", string.Empty, msg);

				int closedJob = processor.CloseJobInBulk();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Clsoed Jobs in the JobHeader Table", allJobsToclose.Count);
				AssertEquals("Total Number of Clsoed Jobs in the JobHeader Table", allJobsToclose.Count, closedJob);
				AssertJobs(allJobsToclose, true);
			}

			[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
			public void TestBulkJobClosureClosesJobOfCurrentCompanyOnly()
			{
				var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-10));
				var job2 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));
				var job3 = CreateJob(JobInvoicingConsumerTypes.CFSLoadList, TestObjectCreator.NonCurrentCompany.PK, TestObjectCreator.NonCurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-20));
				var job4 = CreateJob(JobInvoicingConsumerTypes.ForwardingConsol, TestObjectCreator.NonCurrentCompany.PK, TestObjectCreator.NonCurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));

				Factory.Save();

				var processor = new BulkJobCloseProcessor();
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-30);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today;

				var msg = processor.Find();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader", "Total Number of Jobs in the JobHeader Table", 4);
				AssertEquals("Total Number of Jobs to be closed", processor.JobPKs.Count, 2);
				AssertEquals("No Error Message", string.Empty, msg);

				int closedJob = processor.CloseJobInBulk();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Clsoed Jobs in the JobHeader Table", 2);
				AssertEquals("Total Number of Clsoed Jobs in the JobHeader Table", 2, closedJob);
				AssertJobs(new HashSet<ZGuid>() { job2.PK, job1.PK }, true);
			}

			public void TestBulkJobClosureFilterForJobShouldBeClosedByDsbBatch()
			{
				var processor = new BulkJobCloseProcessor();
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-30);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today;

				TestObjectCreator.CC1.AC_ChargeType = Constants.ChargeType.Disbursement;
				TestObjectCreator.CC1.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
				TestObjectCreator.CC1.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

				var job = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, ZDateTime.Today);
				CreateAndPostCharge(job, TestObjectCreator.CC1, "10001", "Freight", Env.CurrentCompany.PK, Env.CurrentBranch.PK);
				Factory.Save();

				var apline = job.Charges[0].APLine;
				var arline = job.Charges[0].ARLine;
				AssertEquals("Precondition", "CST", apline.AL_LineType);
				AssertEquals("Precondition", "REV", arline.AL_LineType);

				using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Precondition", 0m, arline.AL_LineAmount + apline.AL_LineAmount);
					Assert("Precondition", !Db.Connection.Exists($@"FROM ShouldJobBeClosedByDsbBatch('{job.PK}')"));
					processor.Find();
					AssertEquals("Total Number of Jobs to be closed", 1, processor.JobPKs.Count);

					var imbalancingCharge = CreateCharge(job, TestObjectCreator.CC1, "Also Freight", TestObjectCreator.AUD, 16.0m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 16.0m, TestObjectCreator.Debtor, "FIN", Env.CurrentBranch.PK);
					var imbalancingInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "10002", TestObjectCreator.AUD, 1.0m);
					var imbalancingApLine = TestObjectCreator.CreateAPInvoiceLine(imbalancingInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Also Freight", 16.0m);
					imbalancingApLine.AL_AT = imbalancingCharge.JR_AT_CostGSTRate;
					imbalancingCharge.JR_AL_APLine = imbalancingApLine.PK;
					Factory.Save();

					Assert("Precondition", Db.Connection.Exists($@"FROM ShouldJobBeClosedByDsbBatch('{job.PK}')"));
					processor.Find();
					AssertEquals("Total Number of Jobs to be closed", 0, processor.JobPKs.Count);
				}

				using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					Assert("Precondition", Db.Connection.Exists($@"FROM ShouldJobBeClosedByDsbBatch('{job.PK}')"));
					processor.Find();
					AssertEquals("Total Number of Jobs to be closed", 1, processor.JobPKs.Count);
				}
			}

			public void TestBulkJobClosureFilterForRecognisedRevenueAndCost()
			{
				RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
				RevenueRecognition setting = valuesForTest.AddNew();
				setting.JobType = "ALL";
				setting.DirectionCode = "";
				setting.Mode = "";
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
				Factory.Save();

				var jobWithRecognisedCostAndRevenue = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, ZDateTime.Today);
				CreateAndPostCharge(jobWithRecognisedCostAndRevenue, TestObjectCreator.FRT, "10001", "Freight", Env.CurrentCompany.PK, Env.CurrentBranch.PK);

				var recognisedCost = jobWithRecognisedCostAndRevenue.Charges[0].APLine;
				var recognisedRevenue = jobWithRecognisedCostAndRevenue.Charges[0].ARLine;

				AssertEquals("Precondition.", "CST", recognisedCost.AL_LineType);
				AssertEquals("Precondition.", "REV", recognisedRevenue.AL_LineType);

				Factory.Save();

				Assert("Precondition.", !recognisedCost.AL_ReverseDate.IsEmpty);
				Assert("Precondition.", !recognisedRevenue.AL_ReverseDate.IsEmpty);

				var processor = new BulkJobCloseProcessor((q) => ObjectFactory.Get<IAccountingModuleUtilityForTest>().GetAdditionalFilterForBulkJobClosure(q));
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today.AddDays(1);
				var msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Should only close jobWithRecognisedCostAndRevenue", 1, processor.JobPKs.Count);
				AssertEquals(processor.JobPKs[0], jobWithRecognisedCostAndRevenue.PK);
			}

			public void TestBulkJobClosureFilterWhenUseNOTINAndExistNull()
			{
				var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1m, 220m, 0m, 0m, 220m, 0m, 0m);
				Factory.Save();

				DataUtils.GetDataTableFromQuery(Db.Connection, $"Update dbo.AccTransactionLines SET AL_ReverseDate = NULL, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = '{apInv.Lines[0].PK}'");

				var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				testJob.JH_JobNum = "J001";
				var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);
				charge.JR_APInvoiceNum = "INV002";
				charge.JR_APInvoiceDate = ZDateTime.Now;
				new InvoicingPostManager(testJob).CreateTransactions(JobInvoicingPostingOption.All);
				Factory.Save();

				var processor = new BulkJobCloseProcessor((q) => ObjectFactory.Get<IAccountingModuleUtilityForTest>().GetAdditionalFilterForBulkJobClosure(q));
				processor.JobStatusFilter = JobHeaderStatus.Working.Code;
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today.AddDays(1);
				var msg = processor.Find();
				AssertEquals("Should contain one job", 1, processor.JobPKs.Count);
				AssertEquals(processor.JobPKs[0], testJob.PK);
			}

			public void TestBulkJobClosureFilterForUnrecognisedCost()
			{
				RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
				RevenueRecognition setting = valuesForTest.AddNew();
				setting.JobType = "ALL";
				setting.DirectionCode = "";
				setting.Mode = "";
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
				Factory.Save();

				var jobWithUnrecognisedCost = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, ZDateTime.Today);
				CreateAndPostCharge(jobWithUnrecognisedCost, TestObjectCreator.FRT, "10001", "Freight", Env.CurrentCompany.PK, Env.CurrentBranch.PK);

				var unrecognisedCost = jobWithUnrecognisedCost.Charges[0].APLine;

				AssertEquals("Precondition.", "CST", unrecognisedCost.AL_LineType);

				Factory.Save();

				Assert("Precondition.", unrecognisedCost.AL_ReverseDate.IsEmpty);

				var processor = new BulkJobCloseProcessor((q) => ObjectFactory.Get<IAccountingModuleUtilityForTest>().GetAdditionalFilterForBulkJobClosure(q));
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today.AddDays(1);
				var msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Should not close jobs with unrecognised cost", 0, processor.JobPKs.Count);
			}

			public void TestBulkJobClosureFilterForUnrecognisedRevenue()
			{
				RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
				RevenueRecognition setting = valuesForTest.AddNew();
				setting.JobType = "ALL";
				setting.DirectionCode = "";
				setting.Mode = "";
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
				Factory.Save();

				var jobWithUnrecognisedRevenue = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, ZDateTime.Today);
				CreateAndPostCharge(jobWithUnrecognisedRevenue, TestObjectCreator.FRT, "10001", "Freight", Env.CurrentCompany.PK, Env.CurrentBranch.PK);

				var unrecognisedRevenue = jobWithUnrecognisedRevenue.Charges[0].ARLine;

				AssertEquals("Precondition.", "REV", unrecognisedRevenue.AL_LineType);

				Factory.Save();

				Assert("Precondition.", unrecognisedRevenue.AL_ReverseDate.IsEmpty);

				var processor = new BulkJobCloseProcessor((q) => ObjectFactory.Get<IAccountingModuleUtilityForTest>().GetAdditionalFilterForBulkJobClosure(q));
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today.AddDays(1);
				var msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Should not close jobs with unrecognised revenue", 0, processor.JobPKs.Count);
			}

			public void TestBulkJobClosureDoesNotCloseAlreadyClosedJob()
			{
				var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-10));
				var job2 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));
				job2.JH_Status = "CLS";
				var job3 = CreateJob(JobInvoicingConsumerTypes.CFSLoadList, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-20));
				var job4 = CreateJob(JobInvoicingConsumerTypes.ForwardingConsol, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));

				Factory.Save();

				var processor = new BulkJobCloseProcessor();
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-30);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today;

				var msg = processor.Find();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader", "Total Number of Jobs in the JobHeader Table", 4);
				AssertEquals("Total Number of Jobs to be closed", processor.JobPKs.Count, 3);
				AssertEquals("No Error Message", string.Empty, msg);

				int closedJob = processor.CloseJobInBulk();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Clsoed Jobs in the JobHeader Table", 4);
				AssertEquals("Total Number of Clsoed Jobs in the JobHeader Table", 3, closedJob);
				AssertJobs(new HashSet<ZGuid>() { job1.PK, job2.PK, job3.PK, job4.PK }, false);
				AssertLogExist(new HashSet<ZGuid>() { job1.PK, job3.PK, job4.PK });
				AssertLogDoesNotExist(new HashSet<ZGuid>() { job2.PK });
			}

			public void TestBulkJobClosureDoesNotCloseInactiveJob()
			{
				var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-10));
				var job2 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));
				var job3 = CreateJob(JobInvoicingConsumerTypes.CFSLoadList, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-20));
				var job4 = CreateJob(JobInvoicingConsumerTypes.ForwardingConsol, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));

				Factory.Save();

				job2.MarkAsInactive();

				Factory.Save();

				var processor = new BulkJobCloseProcessor();
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-30);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today;

				var msg = processor.Find();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader", "Total Number of Jobs in the JobHeader Table", 4);
				AssertEquals("Total Number of Jobs to be closed", processor.JobPKs.Count, 3);
				AssertEquals("No Error Message", string.Empty, msg);
				AssertCollectionNotContains(job2.PK, processor.JobPKs);

				var closedJob = processor.CloseJobInBulk();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Clsoed Jobs in the JobHeader Table", 3);
				AssertEquals("Total Number of Closed Jobs in the JobHeader Table", 3, closedJob);
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_IsActive = '1'", "Total Number of Active Jobs in the JobHeader Table", 3);
				AssertLogExist(new HashSet<ZGuid>() { job1.PK, job3.PK, job4.PK });
				AssertLogDoesNotExist(new HashSet<ZGuid>() { job2.PK });
			}

			public void TestBulkJobClosureFilter()
			{
				var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-10));
				var job2 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));
				var job3 = CreateJob(JobInvoicingConsumerTypes.CFSLoadList, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-20));
				var job4 = CreateJob(JobInvoicingConsumerTypes.ForwardingConsol, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-25));
				var job5 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-35));
				var job6 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-45));

				job1.JH_Status = JobHeaderStatus.Working.Code;
				job2.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				job3.JH_Status = JobHeaderStatus.Working.Code;
				job4.JH_Status = JobHeaderStatus.Complete.Code;
				job5.JH_Status = JobHeaderStatus.Closed.Code;
				job6.JH_Status = JobHeaderStatus.JobInvoiced.Code;

				Factory.Save();

				var processor = new BulkJobCloseProcessor();
				string msg = processor.Find();
				AssertEquals("Error Message", @"Please enter a value.
You must fill at least one of the following filters
- Job Open Date
- Job Last Edit Time", msg);

				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-30);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today;
				msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Total Number of Jobs to be closed: job 1,2,3,4", 2, processor.JobPKs.Count);

				processor.JobStatusFilter = JobHeaderStatus.Complete.Code;
				msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Total Number of Jobs to be closed: No Job", 1, processor.JobPKs.Count);

				processor.JobStatusFilter = JobHeaderStatus.Working.Code;
				processor.JobLastEditDateFilter.Property1 = ZDateTime.Today.AddDays(-10);
				processor.JobLastEditDateFilter.Property2 = ZDateTime.Today;
				msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Total Number of Jobs to be closed: job 1", 2, processor.JobPKs.Count);

				processor.JobLastEditDateFilter.Property1 = ZDateTime.Today.AddDays(-10);
				processor.JobLastEditDateFilter.Property2 = ZDateTime.Today.AddDays(-2);
				msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Total Number of Jobs to be closed: job 1", 0, processor.JobPKs.Count);
			}

			public void TestBulkJobClosureFilterDoesnotIncludeActiveJobManagemntModuleFilter()
			{
				var jobFilterObject = ObjectFactory.Get<IAccountingModuleUtilityForTest>();
				var jobOpenDateFilter = jobFilterObject.GetModuleDateFilter("Job Open");
				jobOpenDateFilter.IsActive = true;
				jobOpenDateFilter.PropertySearch = "Date range";
				jobOpenDateFilter.Property1 = new ZDateTime(2017, 1, 1);
				jobOpenDateFilter.Property2 = new ZDateTime(2017, 1, 1);

				var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, new ZDateTime(2018, 1, 1));
				var job2 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, new ZDateTime(2018, 5, 2));
				var job3 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, new ZDateTime(2018, 5, 9));
				job1.JH_Status = JobHeaderStatus.Working.Code;
				job2.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				job3.JH_Status = JobHeaderStatus.Working.Code;

				Factory.Save();

				var processor = new BulkJobCloseProcessor((q) => jobFilterObject.GetAdditionalFilterForBulkJobClosure(q));
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = new ZDateTime(2018, 1, 1);
				processor.JobOpenDateFilter.Property2 = new ZDateTime(2018, 5, 9);
				var msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Total Number of Jobs to be closed: job 1,2,3", 2, processor.JobPKs.Count);
			}

			public void TestBulkJobClosureClosesJobWithCorrectDate()
			{
				var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-10));
				var job2 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));
				var job3 = CreateJob(JobInvoicingConsumerTypes.CFSLoadList, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-20));
				var job4 = CreateJob(JobInvoicingConsumerTypes.ForwardingConsol, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-25));
				var job5 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-35));
				var job6 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-45));

				Factory.Save();

				var processor = new BulkJobCloseProcessor();
				processor.JobStatusFilter = "WRK";
				processor.JobOpenDateFilter.PropertySearch = "Date range";
				processor.JobLastEditDateFilter.PropertySearch = "Date range";
				processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-50);
				processor.JobOpenDateFilter.Property2 = ZDateTime.Today;
				var msg = processor.Find();
				AssertEquals("No Error Message", string.Empty, msg);
				AssertEquals("Total Number of Jobs to be closed: job 1,2,3,4,5,6", processor.JobPKs.Count, 6);

				processor.JobCloseDate = ZDateTime.Today.AddDays(-12);
				var closedJob = processor.CloseJobInBulk();
				AssertNumbeOfRows("SELECT COUNT(*) FROM dbo.JobHeader WHERE JH_Status = 'CLS'", "Total Number of Clsoed Jobs in the JobHeader Table", 6);
				AssertEquals("Total Number of Clsoed Jobs in the JobHeader Table", 6, closedJob);
				AssertJobs(new HashSet<ZGuid>() { job1.PK, job2.PK, job3.PK, job4.PK, job5.PK, job6.PK }, true);
				AssertJobCloseDate(job1, job1.JH_A_JOP); //As Job Open Date is later than proposed Job Close Date, that's why job was closed with Job Open Date.
				AssertJobCloseDate(job2, processor.JobCloseDate);
				AssertJobCloseDate(job3, processor.JobCloseDate);
				AssertJobCloseDate(job4, processor.JobCloseDate);
				AssertJobCloseDate(job5, processor.JobCloseDate);
				AssertJobCloseDate(job6, processor.JobCloseDate);
			}
		}
	}

	[TestedType(typeof(BulkJobCloseProcessor.EmptyFilterStripBusinessObject))]
	public class EmptyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BulkJobCloseProcessor.EmptyFilterStripBusinessObject() ;
		}
	}
}
