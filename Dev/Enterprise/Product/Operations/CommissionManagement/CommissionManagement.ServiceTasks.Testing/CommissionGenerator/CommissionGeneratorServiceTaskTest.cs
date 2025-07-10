using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.Business.Testing;
using Enterprise.CommissionManagement.ServiceTasks.CommissionGenerator;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

#if DEBUG
namespace Enterprise.CommissionManagement.ServiceTasks.Testing.CommissionGenerator
{
	[TestedType(typeof(CommissionGeneratorServiceTask))]
	public class CommissionGeneratorServiceTaskTest : ServiceTaskTestCase<CommissionGeneratorServiceTask>
	{
		[TestDateIncremental(0, 0, 1)]
		public void TestCommissionGenerator_SuccessfulRun()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var date1 = new ZDateTime(2016, 1, 8, 11, 54, 0);
			var date2 = new ZDateTime(2015, 8, 19, 12, 0, 0);
			var date3 = new ZDateTime(1984, 7, 10, 7, 34, 0);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date1;
			queue1.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();

			var queue2 = Factory.New<OrgCommissionCalculationQueue>();
			queue2.CAQ_CA0 = agreement2.PK;
			queue2.CAQ_MinimumInvoicePostedDate = date2;
			queue2.CAQ_OverwriteExistingCommissions = false;

			Factory.Save();

			var queue3 = Factory.New<OrgCommissionCalculationQueue>();
			queue3.CAQ_CA0 = agreement3.PK;
			queue3.CAQ_MinimumInvoicePostedDate = date3;
			queue3.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTask();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals("Information|Commission generation starting.", log[0]);
			AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement1.PK, agreement1.AgreementId, date1), log[1]);
			AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: N.", agreement2.PK, agreement2.AgreementId, date2), log[2]);
			AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement3.PK, agreement3.AgreementId, date3), log[3]);

			AssertEquals("Information|Commission generation completed.", log[4]);
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestCommissionGenerator_TokenIsCancelled()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var date = new ZDateTime(2016, 1, 8, 11, 54, 0);

			var queue = Factory.New<OrgCommissionCalculationQueue>();
			queue.CAQ_CA0 = agreement.PK;
			queue.CAQ_MinimumInvoicePostedDate = date;
			queue.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTask();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask, new CancellationToken(true));

			AssertEquals("Information|Commission generation starting.", log[0]);
			AssertEquals("Information|Commission generation completed.", log[1]);
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestCommissionGenerator_ExceptionDuringRun_CheckMessage()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var date1 = new ZDateTime(2015, 1, 8, 11, 54, 0);
			var date2 = new ZDateTime(2015, 8, 19, 12, 0, 0);
			var date3 = new ZDateTime(2016, 8, 19, 12, 0, 0);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date1;
			queue1.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();

			try
			{
				TypeDecider.AddSubstitution(typeof(CommissionAgreementApprover), typeof(CommissionAgreementApproverWithExceptionForYear2015));

				var serviceTask = new CommissionGeneratorServiceTask();
				var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals("Error|1 out of 1 Commission generations have failed.", log[3]);
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(CommissionAgreementApprover));
			}

			var queue2 = Factory.New<OrgCommissionCalculationQueue>();
			queue2.CAQ_CA0 = agreement2.PK;
			queue2.CAQ_MinimumInvoicePostedDate = date2;
			queue2.CAQ_OverwriteExistingCommissions = false;

			Factory.Save();

			var queue3 = Factory.New<OrgCommissionCalculationQueue>();
			queue3.CAQ_CA0 = agreement3.PK;
			queue3.CAQ_MinimumInvoicePostedDate = date3;
			queue3.CAQ_OverwriteExistingCommissions = false;

			Factory.Save();

			try
			{
				TypeDecider.AddSubstitution(typeof(CommissionAgreementApprover), typeof(CommissionAgreementApproverWithExceptionForYear2015));

				var serviceTask = new CommissionGeneratorServiceTask();
				var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals("Error|2 out of 3 Commission generations have failed.", log[6]);
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(CommissionAgreementApprover));
			}
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestCommissionGenerator_ShouldReinstate()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var agreement = CreateAgreementForReapproval();
			var date1 = new ZDateTime(2016, 1, 8, 11, 54, 0);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date1;
			queue1.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();
			AssertEquals("Pre-condition", 1, Factory.Load<AccCommissionLine>(new ZQuery()).Length);

			var serviceTask = new CommissionGeneratorServiceTaskForTest();
			InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals("1 (original line) + 2 (reversal & reinstate line) should have been created.", 3, Factory.Load<AccCommissionLine>(new ZQuery()).Length);
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestCommissionGenerator_ShouldNotReinstate()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var agreement = CreateAgreementForReapproval();
			var date1 = new ZDateTime(2016, 1, 8, 11, 54, 0);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date1;
			queue1.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();

			var lines = Factory.Load<AccCommissionLine>(new ZQuery());
			AssertEquals("Pre-condition", 1, lines.Length);

			lines[0].CL0_ShouldReinstate = false;

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTaskForTest();
			InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals("Reversal & Reinstate line should NOT have been created.", 1, Factory.Load<AccCommissionLine>(new ZQuery()).Length);
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestCommissionGenerator_ExceptionDuringRun_Approval()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var date1 = new ZDateTime(2015, 1, 1);
			var date2 = new ZDateTime(2016, 1, 1);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date1;
			queue1.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();

			var queue2 = Factory.New<OrgCommissionCalculationQueue>();
			queue2.CAQ_CA0 = agreement2.PK;
			queue2.CAQ_MinimumInvoicePostedDate = date2;
			queue2.CAQ_OverwriteExistingCommissions = false;

			Factory.Save();

			try
			{
				TypeDecider.AddSubstitution(typeof(CommissionAgreementApprover), typeof(CommissionAgreementApproverWithExceptionForYear2015));

				var serviceTask = new CommissionGeneratorServiceTask();
				var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement1.PK, agreement1.AgreementId, date1), log[1]);
				AssertEquals("Debug|Error generating commissions: Woops", log[2]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: N.", agreement2.PK, agreement2.AgreementId, date2), log[3]);
				AssertEquals("Error|1 out of 2 Commission generations have failed.", log[4]);
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(CommissionAgreementApprover));
			}
		}

		public void TestCommissionGenerator_ExceptionDuringRun_Regeneration()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var date = new ZDateTime(2016, 1, 8, 11, 54, 0);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date;
			queue1.CAQ_OverwriteExistingCommissions = true;

			var queue2 = Factory.New<OrgCommissionCalculationQueue>();
			queue2.CAQ_CA0 = agreement2.PK;
			queue2.CAQ_MinimumInvoicePostedDate = date;
			queue2.CAQ_OverwriteExistingCommissions = true;

			var queue3 = Factory.New<OrgCommissionCalculationQueue>();
			queue3.CAQ_CA0 = agreement3.PK;
			queue3.CAQ_MinimumInvoicePostedDate = date;
			queue3.CAQ_OverwriteExistingCommissions = true;

			var objectCreator = new CommissionTestObjectCreator(Factory);
			objectCreator.SetupTransactionsAndCommissions(false);

			var queue4 = Factory.New<OrgCommissionCalculationQueue>();
			queue4.CAQ_AH = objectCreator.JobInvoice.PK;
			queue4.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

			var queue5 = Factory.New<OrgCommissionCalculationQueue>();
			queue5.CAQ_JH = objectCreator.Job.PK;
			queue5.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

			Factory.Save();

			queue3.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 1);
			queue4.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 2);
			queue2.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 3);
			queue5.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 4);
			queue1.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 5);

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTask();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			CombineAssertions(() =>
			{
				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement3.PK, agreement3.AgreementId, date), log[1]);
				AssertEquals($"Debug|Regenerating Commissions for Transaction: {objectCreator.JobInvoice.AH_TransactionNum}, PK: {objectCreator.JobInvoice.PK}", log[2]);
				AssertEquals($"Debug|Error regenerating commissions: Must be non job-related transaction, but passed in transaction (PK:{objectCreator.JobInvoice.PK}) with AH_JH:00000000-0000-0000-0000-000000000000", log[3]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement2.PK, agreement2.AgreementId, date), log[4]);
				AssertEquals($"Debug|Regenerating Commissions for Job: {objectCreator.Job.JH_JobNum}, PK: {objectCreator.Job.PK}", log[5]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement1.PK, agreement1.AgreementId, date), log[6]);
				AssertEquals("Error|1 out of 5 Commission generations have failed.", log[7]);
			});
		}

		public void TestCommissionGenerator_UnableToLock()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var date1 = new ZDateTime(2016, 1, 8, 11, 54, 0);
			var date2 = new ZDateTime(2015, 8, 19, 12, 0, 0);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date1;
			queue1.CAQ_OverwriteExistingCommissions = true;

			var queue2 = Factory.New<OrgCommissionCalculationQueue>();
			queue2.CAQ_CA0 = agreement2.PK;
			queue2.CAQ_MinimumInvoicePostedDate = date2;
			queue2.CAQ_OverwriteExistingCommissions = false;

			Factory.Save();

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				if (connection.TryGetLock(CommissionAgreementApprover.AgreementLockPrefix + agreement1.PK, out SqlApplicationLock sqlLock))
				{
					try
					{
						var serviceTask = new CommissionGeneratorServiceTask();
						string log = string.Empty;
						using (var transactionManager = ((ITransactionParticipant)Factory).BeginTransactionWithManager())
						{
							log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask).ToString();
							transactionManager.CommitTransaction();
						}

						AssertContains("Information|Commission generation starting.", log);
						AssertContains(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: N", agreement2.PK, agreement2.AgreementId, date2), log);
						AssertContains(string.Format("Debug|Unable to acquire lock on agreement with PK: {0}", agreement1.PK), log);
						AssertContains("Information|Commission generation completed.", log);
					}
					finally
					{
						sqlLock.Dispose();
					}
				}
				else
				{
					Fail("Unable to get lock");
				}
			}
		}

		public void TestCreateCommissions_RerunAfterFailureOn2ndJob()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var chargeCodeX = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCodeY = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			void CheckCommission(string commissionHeaderName, AccCommissionHeader commissionHeader, ZDateTime snapshotDate, Job job, ARInvoice invoice)
			{
				CombineAssertions($"{commissionHeaderName} properties", () =>
				{
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);

					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, snapshotDate, commissionHeader.CH0_SnapshotDateTime);
					AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Posted, commissionHeader.CH0_SnapshotEventCode);
				});
				{
					var lineGroupAudChargeX = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeCodeX.PK);
					CombineAssertions($"{commissionHeaderName} {nameof(lineGroupAudChargeX)}", () =>
					{
						AssertEquals(110m, lineGroupAudChargeX.CLG_TransactionAmount);
						AssertEquals("AUD", lineGroupAudChargeX.CLG_RX_NKTransactionCurrency);
						AssertEquals(110m, lineGroupAudChargeX.CLG_TotalCommissionableAmount);
						AssertEquals("AUD", lineGroupAudChargeX.CLG_RX_NKCommissionCurrency);
						AssertEquals(2, lineGroupAudChargeX.Lines.Count);
					});
				}

				{
					var lineGroupUsdChargeX = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeCodeX.PK);
					CombineAssertions($"{commissionHeaderName} {nameof(lineGroupUsdChargeX)}", () =>
					{
						AssertEquals(10000m, lineGroupUsdChargeX.CLG_TransactionAmount);
						AssertEquals("USD", lineGroupUsdChargeX.CLG_RX_NKTransactionCurrency);
						AssertEquals(20000m, lineGroupUsdChargeX.CLG_TotalCommissionableAmount);
						AssertEquals("AUD", lineGroupUsdChargeX.CLG_RX_NKCommissionCurrency);
						AssertEquals(2, lineGroupUsdChargeX.Lines.Count);
					});
				}

				{
					var lineGroupAudChargeY = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeCodeY.PK);
					CombineAssertions($"{commissionHeaderName} {nameof(lineGroupAudChargeY)}", () =>
					{
						AssertEquals(1000m, lineGroupAudChargeY.CLG_TransactionAmount);
						AssertEquals("AUD", lineGroupAudChargeY.CLG_RX_NKTransactionCurrency);
						AssertEquals(1000m, lineGroupAudChargeY.CLG_TotalCommissionableAmount);
						AssertEquals("AUD", lineGroupAudChargeY.CLG_RX_NKCommissionCurrency);
						AssertEquals(2, lineGroupAudChargeY.Lines.Count);
					});
				}

				{
					var lineGroupUsdChargeY = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeCodeY.PK);
					CombineAssertions($"{commissionHeaderName} {nameof(lineGroupUsdChargeY)}", () =>
					{
						AssertEquals(1100000m, lineGroupUsdChargeY.CLG_TransactionAmount);
						AssertEquals("USD", lineGroupUsdChargeY.CLG_RX_NKTransactionCurrency);
						AssertEquals(2200000m, lineGroupUsdChargeY.CLG_TotalCommissionableAmount);
						AssertEquals("AUD", lineGroupUsdChargeY.CLG_RX_NKCommissionCurrency);
						AssertEquals(2, lineGroupUsdChargeY.Lines.Count);
					});
				}

				AssertEquals($"{commissionHeaderName} should have grouped the transaction lines into charge code and invoice currency pairs", 4, commissionHeader.LineGroups.Count);

				AssertEquals($"{commissionHeaderName} should have 1 fixed commission line", 1, commissionHeader.Lines.Count);
			}

			Factory.Save();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = new Job.Loader(shipment1).TryCreate();
			job1.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_JH = job1.PK;
			invoice1.AH_RX_NKTransactionCurrency = "USD";
			invoice1.AH_PostDate = new ZDateTime(2002, 2, 2);
			testObjectCreator.CreateARInvoiceLine(invoice1, job1, chargeCodeX, audCurrency, 1, "", 10);
			testObjectCreator.CreateARInvoiceLine(invoice1, job1, chargeCodeX, audCurrency, 1, "", 100);
			testObjectCreator.CreateARInvoiceLine(invoice1, job1, chargeCodeY, audCurrency, 1, "", 1000);
			testObjectCreator.CreateARInvoiceLine(invoice1, job1, chargeCodeX, usdCurrency, 0.5m, "", 10000);
			testObjectCreator.CreateARInvoiceLine(invoice1, job1, chargeCodeY, usdCurrency, 0.5m, "", 100000);
			testObjectCreator.CreateARInvoiceLine(invoice1, job1, chargeCodeY, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice1.Lines)
			{
				testObjectCreator.CreateJobCharge(line, job1, line.ChargeCode, line.TransactionCurrency);
			}

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = new Job.Loader(shipment2).TryCreate();
			job2.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_JH = job2.PK;
			invoice2.AH_RX_NKTransactionCurrency = "USD";
			invoice2.AH_PostDate = new ZDateTime(2002, 3, 2);
			testObjectCreator.CreateARInvoiceLine(invoice2, job2, chargeCodeX, audCurrency, 1, "", 10);
			testObjectCreator.CreateARInvoiceLine(invoice2, job2, chargeCodeX, audCurrency, 1, "", 100);
			testObjectCreator.CreateARInvoiceLine(invoice2, job2, chargeCodeY, audCurrency, 1, "", 1000);
			testObjectCreator.CreateARInvoiceLine(invoice2, job2, chargeCodeX, usdCurrency, 0.5m, "", 10000);
			testObjectCreator.CreateARInvoiceLine(invoice2, job2, chargeCodeY, usdCurrency, 0.5m, "", 100000);
			testObjectCreator.CreateARInvoiceLine(invoice2, job2, chargeCodeY, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice2.Lines)
			{
				testObjectCreator.CreateJobCharge(line, job2, line.ChargeCode, line.TransactionCurrency);
			}

			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;

			var minimumInvoicePostedDate = new ZDateTime(2002, 1, 1);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement.PK;
			queue1.CAQ_MinimumInvoicePostedDate = minimumInvoicePostedDate;
			queue1.CAQ_OverwriteExistingCommissions = true;

			Factory.Save();

			try
			{
				TypeDecider.AddSubstitution(typeof(CommissionAgreementApprover), typeof(CommissionAgreementApproverFailingOn2ndJob));

				var serviceTask = new CommissionGeneratorServiceTask();
				var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

				AssertEquals(4, log.Count);
				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals($"Debug|Generating commissions for agreement with PK: {agreement.PK}, ID: {agreement.AgreementId}, minumum invoice posted date: {minimumInvoicePostedDate}, override existing commissions: Y.", log[1]);
				AssertEquals("Debug|Error generating commissions: Fail in test as designed.", log[2]);
				AssertEquals("Error|1 out of 1 Commission generations have failed.", log[3]);
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(CommissionAgreementApprover));
			}

			var commissionHeaders1 = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice1.PK));
			var commissionHeaders2 = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice2.PK));

			AssertEquals(1, commissionHeaders1.Length);
			AssertEquals(1, commissionHeaders2.Length);
			// Currently task does not save intermediate results.
			// If this changes check contents of commission header that was generated for one of the jobs.
			/*
				if (commissionHeaders1.Length == 1)
				{
					var commissionHeader = commissionHeaders1[0];
					CheckCommission("commissionHeader1", commissionHeader, new ZDateTime(2002, 2, 2), job1, invoice1);
				}
				else
				{
					var commissionHeader = commissionHeaders2[0];
					CheckCommission("commissionHeader2", commissionHeader, new ZDateTime(2002, 3, 2), job2, invoice2);
				}
			*/
			{
				var serviceTask = new CommissionGeneratorServiceTask();
				var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

				AssertEquals(3, log.Count);
				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals($"Debug|Generating commissions for agreement with PK: {agreement.PK}, ID: {agreement.AgreementId}, minumum invoice posted date: {minimumInvoicePostedDate}, override existing commissions: Y.", log[1]);
				AssertEquals("Information|Commission generation completed.", log[2]);
			}

			commissionHeaders1 = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice1.PK));
			commissionHeaders2 = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice2.PK));

			// CommissionHeaderBuilder.cs - 
			// "existing commission headers already exist for this commission stream, and at least one of them is already up to date. Therefore do not create any new commissions for this stream"
			// so we will still only have 1 commission header for each invoice.
			AssertEquals(1, commissionHeaders1.Length);
			AssertEquals(1, commissionHeaders2.Length);

			var commissionHeader1 = commissionHeaders1[0];
			var commissionHeader2 = commissionHeaders2[0];

			CheckCommission("commissionHeader1", commissionHeader1, new ZDateTime(2002, 2, 2), job1, invoice1);
			CheckCommission("commissionHeader2", commissionHeader2, new ZDateTime(2002, 3, 2), job2, invoice2);
		}

		public void TestCommissionGenerator_NullRef()
		{
			var serviceTask = new CommissionGeneratorServiceTaskForNullRefTest();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);
			AssertEquals("Information|Commission generation starting.", log[0]);
			AssertEquals("Information|Commission generation completed.", log[1]);
		}

		public void TestCommissionGenerator_RegenerateCommissions()
		{
			var objectCreator = new CommissionTestObjectCreator(Factory);
			objectCreator.SetupTransactionsAndCommissions(closeJob: true);

			AssertEquals("Pre-condition", 3, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			var queueItemInvoice = Factory.New<OrgCommissionCalculationQueue>();
			queueItemInvoice.CAQ_AH = objectCreator.Invoice.PK;
			queueItemInvoice.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

			var queueItemJob = Factory.New<OrgCommissionCalculationQueue>();
			queueItemJob.CAQ_JH = objectCreator.Job.PK;
			queueItemJob.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTask();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			Factory.Save();

			AssertEquals("3 original + 3 Reversal + 3 Regenerated", 9, Factory.GetDatabaseCount(typeof(AccCommissionLine)));
			AssertEquals("Information|Commission generation starting.", log[0]);
			AssertEquals($"Debug|Regenerating Commissions for Transaction: {objectCreator.Invoice.AH_TransactionNum}, PK: {objectCreator.Invoice.PK}", log[1]);
			AssertEquals($"Debug|Regenerating Commissions for Job: {objectCreator.Job.JH_JobNum}, PK: {objectCreator.Job.PK}", log[2]);
			AssertEquals("Information|Commission generation completed.", log[3]);

			AssertEquals("Queue items should be deleted after regeneration", 0, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));
		}

		public void TestCommissionGenerator_AllItemsRunInCreatedTimeOrder()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var date = new ZDateTime(2016, 1, 8, 11, 54, 0);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;
			queue1.CAQ_MinimumInvoicePostedDate = date;
			queue1.CAQ_OverwriteExistingCommissions = true;

			var queue2 = Factory.New<OrgCommissionCalculationQueue>();
			queue2.CAQ_CA0 = agreement2.PK;
			queue2.CAQ_MinimumInvoicePostedDate = date;
			queue2.CAQ_OverwriteExistingCommissions = true;

			var queue3 = Factory.New<OrgCommissionCalculationQueue>();
			queue3.CAQ_CA0 = agreement3.PK;
			queue3.CAQ_MinimumInvoicePostedDate = date;
			queue3.CAQ_OverwriteExistingCommissions = true;

			var objectCreator = new CommissionTestObjectCreator(Factory);
			objectCreator.SetupTransactionsAndCommissions(false);

			var queue4 = Factory.New<OrgCommissionCalculationQueue>();
			queue4.CAQ_AH = objectCreator.Invoice.PK;
			queue4.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

			var queue5 = Factory.New<OrgCommissionCalculationQueue>();
			queue5.CAQ_JH = objectCreator.Job.PK;
			queue5.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

			Factory.Save();

			queue3.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 1);
			queue5.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 2);
			queue2.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 3);
			queue4.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 4);
			queue1.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 5);

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTaskForTest();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals("Current Factory Count", 5, serviceTask.CurrentFactoryCount);
			CombineAssertions(() =>
			{
				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement3.PK, agreement3.AgreementId, date), log[1]);
				AssertEquals($"Debug|Regenerating Commissions for Job: {objectCreator.Job.JH_JobNum}, PK: {objectCreator.Job.PK}", log[2]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement2.PK, agreement2.AgreementId, date), log[3]);
				AssertEquals($"Debug|Regenerating Commissions for Transaction: {objectCreator.Invoice.AH_TransactionNum}, PK: {objectCreator.Invoice.PK}", log[4]);
				AssertEquals(string.Format("Debug|Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: Y.", agreement1.PK, agreement1.AgreementId, date), log[5]);
				AssertEquals("Information|Commission generation completed.", log[6]);
			});
		}

		public void TestCommissionGenerator_RegenerateCommissions_CancelledTransaction()
		{
			var objectCreator = new CommissionTestObjectCreator(Factory);
			objectCreator.SetupTransactionsAndCommissions(false);

			AssertEquals("Pre-condition", 1, Factory.GetDatabaseCount(typeof(AccCommissionLine)));
			objectCreator.TestObjectCreator.ReverseTransaction(objectCreator.Invoice, out _);

			var queueItemInvoice = Factory.New<OrgCommissionCalculationQueue>();
			queueItemInvoice.CAQ_AH = objectCreator.Invoice.PK;
			queueItemInvoice.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTask();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			AssertEquals("2 line (original + reverse)", 2, Factory.GetDatabaseCount(typeof(AccCommissionLine)));
			AssertEquals("Information|Commission generation starting.", log[0]);
			AssertEquals($"Debug|Regenerating Commissions for Transaction: {objectCreator.Invoice.AH_TransactionNum}, PK: {objectCreator.Invoice.PK}", log[1]);
			AssertEquals($"Information|{objectCreator.Invoice.AH_TransactionNum} - This transaction has been canceled, therefore commissions cannot be regenerated for it.", log[2]);
			AssertEquals("Information|Commission generation completed.", log[3]);
		}

		[TestDate(2000, 12, 31)]
		public void TestCommissionGenerator_CreateCommissions()
		{
			var objectCreator = new CommissionTestObjectCreator(Factory);
			objectCreator.SetupTransactionsAndCommissions(false);

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_JH = objectCreator.Job.PK;
			queue1.CAQ_JobClosedDate = new ZDateTime(2021, 1, 1);
			queue1.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Creation;
			queue1.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 2);

			var queue2 = Factory.New<OrgCommissionCalculationQueue>();
			queue2.CAQ_AH = objectCreator.Invoice.PK;
			queue2.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Creation;
			queue2.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 4);

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTask();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			CombineAssertions(() =>
			{
				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals($"Debug|Creating Commissions for Job: {objectCreator.Job.JH_JobNum}, PK: {objectCreator.Job.PK}, Closed Date: {queue1.CAQ_JobClosedDate}", log[1]);
				AssertEquals($"Debug|Creating Commissions for Transaction: {objectCreator.Invoice.AH_TransactionNum}, PK: {objectCreator.Invoice.PK}", log[2]);
				AssertEquals("Information|Commission generation completed.", log[3]);
			});

			var commissionHeaders1 = Factory.LoadTop1<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_GroupingSourceID, objectCreator.Job.PK));

			AssertEquals("Invoice Commission Header, Customer", commissionHeaders1.CH0_OH_Customer, objectCreator.Org.PK);
			AssertEquals("Invoice Commission Header, Agreement", commissionHeaders1.CH0_CA0, objectCreator.CommissionAgreement1.PK);
			AssertEquals("Invoice Commission Header Line Groups", 2, commissionHeaders1.LineGroups.Count);

			var commissionHeaders2 = Factory.LoadTop1<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_GroupingSourceID, objectCreator.Invoice.PK));

			AssertEquals("Invoice Commission Header, Customer", commissionHeaders2.CH0_OH_Customer, objectCreator.Org.PK);
			AssertEquals("Invoice Commission Header, Agreement", commissionHeaders2.CH0_CA0, objectCreator.CommissionAgreement2.PK);
			AssertEquals("Invoice Commission Header Line Groups", 1, commissionHeaders2.LineGroups.Count);
		}

		[TestDate(2000, 12, 31)]
		public void TestCommissionGenerator_CreateCommissions_JobRelatedInvoice()
		{
			var objectCreator = new CommissionTestObjectCreator(Factory);
			objectCreator.SetupTransactionsAndCommissions(false);
			objectCreator.JobInvoice.AH_JH = objectCreator.Job.PK;

			var queue = Factory.New<OrgCommissionCalculationQueue>();
			queue.CAQ_AH = objectCreator.JobInvoice.PK;
			queue.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Creation;
			queue.CAQ_SystemCreateTimeUtc = new ZDateTime(2021, 1, 4);

			Factory.Save();

			var serviceTask = new CommissionGeneratorServiceTask();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask);

			CombineAssertions(() =>
			{
				AssertEquals("Information|Commission generation starting.", log[0]);
				AssertEquals($"Debug|Creating Commissions for Transaction: {objectCreator.JobInvoice.AH_TransactionNum}, PK: {objectCreator.JobInvoice.PK}", log[1]);
				AssertEquals("Information|Commission generation completed.", log[2]);
			});
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "CGN", hostedServiceAttribute.Code);
				AssertEquals("Description", "Commission Generator Service Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "SAL", hostedServiceAttribute.Category);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "30minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						OrgCommissionCalculationQueueSchema.Constants.TableName,
						"Commission Generator Queue"),
				};
			}
		}

		OrgCommissionAgreement CreateAgreementForReapproval()
		{
			var postDate = new ZDateTime(2016, 2, 2);
			var customerA = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_IsSalesRep = true;

			var unapprovedAgreement1 = opportunity.CommissionAgreements.AddNew();
			var recipient = unapprovedAgreement1.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = staff.GS_Code;
			recipient.CAR_IsCommissionRateOverriden = true;
			recipient.CAR_Share = 10;
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;

			var rate = recipient.Rates.AddNew();
			rate.FillWithValidTestData();

			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(
				unapprovedAgreement1,
				OrgCommissionAgreementItemLookups.AllProductsCode,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			unapprovedAgreement1.FillWithValidTestData();
			unapprovedAgreement1.CA0_CommissionBasis = CommissionBasisType.Codes.REV;

			var tHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			tHeader.AH_PostDate = postDate;
			tHeader.AH_TransactionType = TransactionTypes.Invoice;

			var shipmentA = Factory.NewWithValidTestData<ForwardingShipment>();
			var jHeader = new JobHeader.Loader(shipmentA).TryCreate();
			jHeader.JH_OA_LocalChargesAddr = customerA.MainAddress.PK;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TESTCHRG";

			var inv = Factory.NewWithValidTestData<ARInvoice>();
			inv.AH_JH = jHeader.PK;
			inv.AH_PostDate = postDate;

			var tLine = (TransactionLine)inv.Lines.AddNew();
			tLine.FillWithValidTestData();
			tLine.AL_LineType = TransactionLineTypes.Revenue;
			tLine.AL_JH = jHeader.PK;
			tLine.AL_AC = chargeCode.PK;
			tLine.AL_LineAmount = 200;
			tLine.AL_OSAmount = 200;

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AL_ARLine = tLine.PK;
			charge.JR_JH = tLine.AL_JH;
			charge.JR_LocalSellAmt = 200;

			var header = Factory.NewWithValidTestData<AccCommissionHeader>();
			header.CH0_AH_Source = inv.PK;
			header.CH0_GroupingSourceID = jHeader.PK;
			header.CH0_GroupingSourceTableCode = jHeader.TablePrefix;
			header.CH0_OH_Customer = customerA.PK;
			header.CH0_Product = "SHP";
			header.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			header.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			header.CH0_CA0 = unapprovedAgreement1.PK;

			var lineGroup = header.LineGroups.AddNew();
			lineGroup.FillWithValidTestData();
			lineGroup.CLG_AC = chargeCode.PK;

			var line = lineGroup.Lines.AddNew();
			line.FillWithValidTestData();
			line.CL0_GS_NKStaff = staff.GS_Code;
			line.CL0_RX_NKTransactionCurrency = "AUD";
			line.CL0_OH_Party = recipient.CAR_OH_Party;
			line.Cancel();

			Factory.Save();

			return unapprovedAgreement1;
		}

		TestServiceLogger InitialiseAndRunTaskScheduleWithAnyBranchContext(CommissionGeneratorServiceTask serviceTask)
		{
			return InitialiseAndRunTaskScheduleWithAnyBranchContext(serviceTask, CancellationToken.None);
		}

		TestServiceLogger InitialiseAndRunTaskScheduleWithAnyBranchContext(CommissionGeneratorServiceTask serviceTask, CancellationToken token)
		{
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask, token);
			return log;
		}
	}

	class CommissionAgreementApproverWithExceptionForYear2015 : CommissionAgreementApprover
	{
		protected CommissionAgreementApproverWithExceptionForYear2015(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void CreateCommissionsCore(CreateCommissionContext context, Progress progress)
		{
			if (context.FromDate.Year == 2015)
			{
				throw new Exception("Woops");
			}
			else
			{
				base.CreateCommissionsCore(context, progress);
			}
		}
	}

	class CommissionAgreementApproverFailingOn2ndJob : CommissionAgreementApprover
	{
		protected CommissionAgreementApproverFailingOn2ndJob(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void CreateCommissionsCore(CreateCommissionContext context, Progress progress)
		{
			base.CreateCommissionsCore(context, progress + Progress);
		}

		void Progress(string status, int percentComplete)
		{
			if (status.StartsWith("Creating Job Related Commissions: 2 of "))
			{
				throw new InvalidOperationException("Fail in test as designed.");
			}
		}
	}

	class CommissionGeneratorServiceTaskForNullRefTest : CommissionGeneratorServiceTask
	{
		protected override DbOnlyBusinessObjectQueue<OrgCommissionCalculationQueue> GetQueueReader()
		{
			return new DbOnlyBusinessObjectQueueForNullRefTest<OrgCommissionCalculationQueue>();
		}

		class DbOnlyBusinessObjectQueueForNullRefTest<T> : DbOnlyBusinessObjectQueue<T> where T : BusinessObject
		{
			protected override ZGuid[] GetUnprocessedItemPKs_Core()
			{
				return new[] { ZGuid.Empty, ZGuid.NewZGuid() };
			}
		}
	}

	class CommissionGeneratorServiceTaskForTest : CommissionGeneratorServiceTask
	{
		public int CurrentFactoryCount => currentFactoryCount;

		protected override BusinessObjectFactoryProvider CreateBusinessObjectFactoryProvider()
		{
			var factoryProvider = base.CreateBusinessObjectFactoryProvider();
			factoryProvider.CurrentFactoryChanged += FactoryProvider_CurrentFactoryChanged;
			return factoryProvider;
		}

		protected override CreateCommissionContext GetCreateCommissionContext(ZDateTime date, ZBool overwrite, OrgCommissionAgreement agreement)
		{
			var context = base.GetCreateCommissionContext(date, overwrite, agreement);
			context.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { ZString.Empty, new CommissionAgreementAndRates(agreement, date.Date) } };

			return context;
		}

		void FactoryProvider_CurrentFactoryChanged(object sender, EventArgs e)
		{
			currentFactoryCount++;
		}

		int currentFactoryCount;
	}
}
#endif
