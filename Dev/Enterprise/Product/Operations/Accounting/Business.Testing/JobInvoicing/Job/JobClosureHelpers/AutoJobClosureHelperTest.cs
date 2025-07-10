namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using NUnit.Framework;

	public class AutoJobClosureHelperTest : JobClosureHelperTest
	{
		public void TestClosingJobsWithActiveUnInvoicedARCashAdvanceRequestsAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(true, true);
		}

		public void TestClosingJobsWithActiveUnInvoicedAPCashAdvanceRequestsAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(false, true);
		}

		public void TestClosingJobsWithActiveUnInvoicedARCashAdvanceRequestsAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(true, false);
		}

		public void TestClosingJobsWithActiveUnInvoicedAPCashAdvanceRequestsAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(false, false);
		}

		void AssertClosingJobsWithActiveUnInvoicedAPCashAdvanceRequests(bool isARCashAdvance, bool isCashAdvanceFunctionalityEnabled)
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCashAdvanceFunctionalityEnabled))
			{
				SetRegistryValue(Env.CurrentCompanyPK, TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

				var jobsSelectedToClose = new List<Job>();
				var expectedJobsThatShouldBeClosed = new List<Job>();

				var shipment = TestObjectCreator.CreateShipment("S00001");
				var job = TestObjectCreator.CreateJob(shipment);
				job.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
				jobsSelectedToClose.Add(job);
				expectedJobsThatShouldBeClosed.Add(job);

				var shipmentNumIncrement = 2;
				string GetShipmentNumber() => "S0000" + shipmentNumIncrement.ToString();
				var ledgerType = isARCashAdvance ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;

				foreach (var headerStatus in CashAdvanceStatusCodes.RequestHeader.CodesList.GetAllCodes())
				{
					shipment = TestObjectCreator.CreateShipment(GetShipmentNumber());
					job = TestObjectCreator.CreateJob(shipment);
					job.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
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

					jobsSelectedToClose.Add(job);
					if (!isCashAdvanceFunctionalityEnabled)
					{
						expectedJobsThatShouldBeClosed.Add(job);
					}
					else if (headerStatus == CashAdvanceStatusCodes.RequestHeader.Pending || headerStatus == CashAdvanceStatusCodes.RequestHeader.Cancelled || headerStatus == CashAdvanceStatusCodes.RequestHeader.Invoiced)
					{
						expectedJobsThatShouldBeClosed.Add(job);
					}
					shipmentNumIncrement++;
				}
				Factory.Save();

				foreach (var jobToClose in jobsSelectedToClose)
				{
					var jobCloseHelper = AutoJobClosureHelper.CreateHelperToCloseAJob(jobToClose);
					var errorMessage = jobCloseHelper.GetAllErrorMessages();
					if (expectedJobsThatShouldBeClosed.Contains(jobToClose))
					{
						AssertEquals(1, jobCloseHelper.JobsThatCanBeClosed.Count());
						Assert(string.IsNullOrEmpty(errorMessage));
					}
					else
					{
						AssertEquals(0, jobCloseHelper.JobsThatCanBeClosed.Count());
						AssertEquals($"Following job(s) could not be closed due to active Advance Payment: {jobToClose.JH_JobNum}", errorMessage);
					}
				}
			}
		}

		public void TestErrorMessageForJobsThatAreAlreadyClosed()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			Job testJob1 = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			Charge testCharge1 = testJob1.Charges.AddNew();
			testCharge1.JR_AC = TestObjectCreator.CC1.PK;
			testCharge1.JR_LocalCostAmt = 200;
			testCharge1.JR_LocalSellAmt = 211;

			Factory.Save();

			var reloadedTestJob1 = Factory.Load<Job>(testJob1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJob1);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			//After Closing few Jobs
			reloadedTestJob1.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			reloadedTestJob1 = Factory.Load<Job>(reloadedTestJob1.PK);
			helper = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJob1);
			msg = helper.GetAllErrorMessages();
			AssertContains("Jobs are already Closed", $"Following job(s) are already closed: {reloadedTestJob1.JH_JobNum}", msg);
		}

		public void TestErrorMessageForJobsRequiringProfitLossReasonCode()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			Job testJob = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.CC1.PK;
			testCharge.JR_LocalCostAmt = 200;
			testCharge.JR_LocalSellAmt = 211;

			testJob.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var reloadedTestJobs = Factory.Load<Job>(testJob.PK);

			var module = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJobs);
			ZString msg = module.GetAllErrorMessages();
			Assert("No Job requires Profit/Loss Reason Code", msg.IsEmpty);

			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 5M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.Closed.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			module = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJobs);
			msg = module.GetAllErrorMessages();
			Assert("Job requires Profit/Loss Reason Code", !msg.IsEmpty);
			AssertContains("Job Number that requires Profit/Loss Reason Code", $"Following job(s) require a Profit-Loss reason code for closing: {reloadedTestJobs.JH_JobNum}", msg);

			plRequiringReasonParameters.ProfitThreshold = 10M;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);
			module = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJobs);
			msg = module.GetAllErrorMessages();
			Assert("No Job requires Profit/Loss Reason Code", msg.IsEmpty);
		}

		public void TestErrorMessageForUserIsNotAllowedToChangeStatusOfCompleteJobs()
		{
			bool isAllowed = Env.Security.ChangeStatusOfCompleteJobs.IsAllowed;
			try
			{
				SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

				OrgHeader creditor = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false);
				Job testJob = TestObjectCreator.CreateJob(creditor, 0, null, 0);
				testJob.JH_Status = JobHeaderStatus.Complete.Code;
				testJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
				Factory.Save();

				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = false;

				var helper = AutoJobClosureHelper.CreateHelperToCloseAJob(testJob);
				var msg = helper.GetAllErrorMessages();
				AssertContains("Error message", $"{testJob.JH_JobNum}: Not allowed to Change the status of a Complete Job", msg);

				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = true;

				helper = AutoJobClosureHelper.CreateHelperToCloseAJob(testJob);
				msg = helper.GetAllErrorMessages();
				Assert("No error message", string.IsNullOrEmpty(msg));
			}
			finally
			{
				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = isAllowed;
			}
		}

		public void TestJobsWithFutureClosureDateAreNotSelectedForAutoClosure_HasNoUnpostedConsolCost()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			Job testJob1 = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			Charge testCharge1 = testJob1.Charges.AddNew();
			testCharge1.JR_AC = TestObjectCreator.CC1.PK;
			testCharge1.JR_LocalCostAmt = 200;
			testCharge1.JR_LocalSellAmt = 211;

			Factory.Save();

			var reloadedTestJob1 = Factory.Load<Job>(testJob1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJob1);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			//After Changing JOP date
			testJob1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			Factory.Save();

			reloadedTestJob1 = Factory.Load<Job>(reloadedTestJob1.PK);
			helper = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJob1);
			msg = helper.GetAllErrorMessages();
			AssertContains("Too young to be closed", $"Following job(s) are not old enough to be automatically closed: {reloadedTestJob1.JH_JobNum}", msg);
		}

		public void TestJobsWithFutureClosureDateAreNotSelectedForAutoClosure_HasUnpostedConsolCost()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			var consol = SetUpConsol();
			var job1 = consol.Shipments[0].Job as Job;
			var job2 = consol.Shipments[1].Job as Job;

			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-15);
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedJob1 = Factory.Load<Job>(job1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJobAlongWithItsPeers(reloadedJob1);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			//After Changing JOP date
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			Factory.Save();

			reloadedJob1 = Factory.Load<Job>(reloadedJob1.PK);
			helper = AutoJobClosureHelper.CreateHelperToCloseAJobAlongWithItsPeers(reloadedJob1);
			msg = helper.GetAllErrorMessages();
			AssertContains("Too young to be closed", $"Following job(s) are not old enough to be automatically closed: {job2.JH_JobNum}", msg);
		}

		public void TestCloseJobWithOpenWIPsAndAccruals()
		{
			AssertCloseJobWithOpenWIPsAndAccruals(true, "S0001");
		}

		public void TestNotCloseJobWithOpenWIPsAndAccruals()
		{
			AssertCloseJobWithOpenWIPsAndAccruals(false, "S0002");
		}

		public void TestJobsThatCanBeClosed_HasNoUnpostedConsolCost()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			Job testJob1 = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			Charge testCharge1 = testJob1.Charges.AddNew();
			testCharge1.JR_AC = TestObjectCreator.CC1.PK;
			testCharge1.JR_LocalCostAmt = 200;
			testCharge1.JR_LocalSellAmt = 211;

			Factory.Save();

			var reloadedTestJob1 = Factory.Load<Job>(testJob1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJob1);
			var msg = helper.GetAllErrorMessages();
			AssertEquals("No Error", string.Empty, msg);
			AssertEquals("JobsThatCanBeClosed", 1, helper.JobsThatCanBeClosed.Count());

			//After Changing JOP date
			testJob1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			Factory.Save();

			reloadedTestJob1 = Factory.Load<Job>(reloadedTestJob1.PK);
			helper = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJob1);
			msg = helper.GetAllErrorMessages();
			AssertContains("Too young to be closed", $"Following job(s) are not old enough to be automatically closed: {reloadedTestJob1.JH_JobNum}", msg);
			AssertEquals("JobsThatCanBeClosed", 0, helper.JobsThatCanBeClosed.Count());
		}

		public void TestJobsThatCanBeClosed_HasUnpostedConsolCost()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			var consol = SetUpConsol();
			var job1 = consol.Shipments[0].Job as Job;
			var job2 = consol.Shipments[1].Job as Job;

			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-15);
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedJob1 = Factory.Load<Job>(job1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJobAlongWithItsPeers(reloadedJob1);
			var msg = helper.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));
			AssertEquals("JobsThatCanBeClosed", 2, helper.JobsThatCanBeClosed.Count());

			//After Changing JOP date
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			Factory.Save();

			reloadedJob1 = Factory.Load<Job>(reloadedJob1.PK);
			helper = AutoJobClosureHelper.CreateHelperToCloseAJobAlongWithItsPeers(reloadedJob1);
			msg = helper.GetAllErrorMessages();
			AssertContains("Too young to be closed", $"Following job(s) are not old enough to be automatically closed: {job2.JH_JobNum}", msg);
			AssertEquals("JobsThatCanBeClosed", 0, helper.JobsThatCanBeClosed.Count());
		}

		[TestDate(2018, 08, 02)]
		public void TestAutoJobClosureVerificationDetials_WhenJobWithOpenWIPACRAllowedToBeAutoClosed()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true));

			var consol = SetUpConsol();
			var job1 = consol.Shipments[0].Job as Job;
			var job2 = consol.Shipments[1].Job as Job;

			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedJob1 = Factory.Load<Job>(job1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJobAlongWithItsPeers(reloadedJob1);
			var msg = helper.GetAllErrorMessages();
			var expectedMessage = @"[S0001]:
Job Type: SHP | Direction: EXP | Mode: SEA | Department: FES | Open WIP: Yes | Open Accrual: Yes | Status: WRK | Has Unrecognized Amount: No | Has Recognized Amount: Yes.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - Open WIP: Yes- Open Accrual: Yes- From Status: - Charge Filter: ALL) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 28 Jul 2018.
Calculated earliest job closure date: 07 Aug 2018.
Did not satisfy registry settings.

[S0002]:
Job Type: SHP | Direction: EXP | Mode: SEA | Department: FES | Open WIP: Yes | Open Accrual: Yes | Status: WRK | Has Unrecognized Amount: No | Has Recognized Amount: Yes.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - Open WIP: Yes- Open Accrual: Yes- From Status: - Charge Filter: ALL) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 23 Jul 2018.
Calculated earliest job closure date: 02 Aug 2018.
Satisfied registry settings.";
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoJobClosureEligibilityVerificationDetails());
		}

		[TestDate(2018, 08, 02)]
		public void TestAutoJobClosureVerificationDetials_WhenJobWithOpenWIPACRNotAllowedToBeAutoClosed_Case1()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, false, false));

			var consol = SetUpConsol();
			var job1 = consol.Shipments[0].Job as Job;
			var job2 = consol.Shipments[1].Job as Job;

			job1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-5);
			job2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();

			var reloadedJob1 = Factory.Load<Job>(job1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJobAlongWithItsPeers(reloadedJob1);
			var msg = helper.GetAllErrorMessages();
			var expectedMessage = @"[S0001]:
Job Type: SHP | Direction: EXP | Mode: SEA | Department: FES | Open WIP: Yes | Open Accrual: Yes | Status: WRK | Has Unrecognized Amount: No | Has Recognized Amount: Yes.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - Open WIP: No- Open Accrual: No- From Status: - Charge Filter: ALL) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 28 Jul 2018.
Calculated earliest job closure date: 07 Aug 2018.
Did not satisfy registry settings.

[S0002]:
Job Type: SHP | Direction: EXP | Mode: SEA | Department: FES | Open WIP: Yes | Open Accrual: Yes | Status: WRK | Has Unrecognized Amount: No | Has Recognized Amount: Yes.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - Open WIP: No- Open Accrual: No- From Status: - Charge Filter: ALL) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 23 Jul 2018.
Calculated earliest job closure date: 02 Aug 2018.
Did not satisfy registry settings.";
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoJobClosureEligibilityVerificationDetails());
		}

		[TestDate(2018, 08, 02)]
		public void TestAutoJobClosureVerificationDetials_WhenJobWithOpenWIPACRNotAllowedToBeAutoClosed_Case2()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, false, false));

			Job testJob1 = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
			Factory.Save();

			var reloadedTestJob1 = Factory.Load<Job>(testJob1.PK);
			var helper = AutoJobClosureHelper.CreateHelperToCloseAJob(reloadedTestJob1);
			var msg = helper.GetAllErrorMessages();
			var expectedMessage = $@"[{testJob1.JH_JobNum}]:
Job Type:  | Direction: UKN | Mode:  | Department: BRN | Open WIP: No | Open Accrual: No | Status: WRK | Has Unrecognized Amount: No | Has Recognized Amount: No.
Matched Configuration: (JobType: ALL-Direction: - Mode: - Department: - Open WIP: No- Open Accrual: No- From Status: - Charge Filter: ALL) -> (Date Option: JOP- Offset: 10 DAY).
Calculated significant Date: 23 Jul 2018.
Calculated earliest job closure date: 02 Aug 2018.
Satisfied registry settings.";
			AssertMultilineASCIIEquals("VerificationDetailsText", expectedMessage, helper.GetAutoJobClosureEligibilityVerificationDetails());
		}

		public void TestCloseJobWithOpenWIPsAndNoAccruals_Case1()
		{
			AssertCloseJobWithEitherOpenWIPsOrAccruals(createWip: true, createAccrual: false, closeJobWithOpenWip: false, closeJobWithOpenAccrual: false, "S0001");
		}

		public void TestCloseJobWithOpenWIPsAndNoAccruals_Case2()
		{
			AssertCloseJobWithEitherOpenWIPsOrAccruals(createWip: true, createAccrual: false, closeJobWithOpenWip: false, closeJobWithOpenAccrual: true, "S0001");
		}

		public void TestCloseJobWithNoWIPsAndOpenAccruals_Case1()
		{
			AssertCloseJobWithEitherOpenWIPsOrAccruals(createWip: false, createAccrual: true, closeJobWithOpenWip: false, closeJobWithOpenAccrual: false, "S0001");
		}

		public void TestCloseJobWithNoWIPsAndOpenAccruals_Case2()
		{
			AssertCloseJobWithEitherOpenWIPsOrAccruals(createWip: false, createAccrual: true, closeJobWithOpenWip: true, closeJobWithOpenAccrual: false, "S0001");
		}

		public void TestCloseJobWithAllowedStatusOnly()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			job.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			Factory.Save();

			foreach (var close in new[] { true, false })
			{
				var regValue = creator.CreateJobClosureConfiguration(
					creator.CreateJobClosureConfigLine(offset: 1, fromJobStatus: "WRK, INV"));
				AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
				job.JH_Status = close ? "WRK" : "IHL";
				Factory.Save();

				var message = AutoJobClosureHelper.CreateHelperToCloseAJob(job).GetAllErrorMessages();
				if (close)
				{
					AssertEquals(string.Empty, message);
				}
				else
				{
					AssertEquals($"Following job(s) could not be automatically closed due to their current status: {job.JH_JobNum}", message);
				}
			}
		}

		public void TestJobClosure_RecognizedChargeFilter()
		{
			var creator = new TestObjectCreator(Factory);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var regValue = creator.CreateJobClosureConfiguration(
					creator.CreateJobClosureConfigLine(offset: 1, closeJobWithOpenAcr: true, closeJobWithOpenWip: true, departmentPK: creator.FISDepartment.PK, recognizedChargeFilter: "REC"),
					creator.CreateJobClosureConfigLine(offset: 1, closeJobWithOpenAcr: true, closeJobWithOpenWip: true, departmentPK: creator.FESDepartment.PK, recognizedChargeFilter: "NRC"),
					creator.CreateJobClosureConfigLine(offset: 1, closeJobWithOpenAcr: true, closeJobWithOpenWip: true, departmentPK: creator.FIADepartment.PK, recognizedChargeFilter: "REC"),
					creator.CreateJobClosureConfigLine(offset: 1, closeJobWithOpenAcr: true, closeJobWithOpenWip: true, departmentPK: creator.FEADepartment.PK, recognizedChargeFilter: "NRC"));

			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			var shipment1 = creator.CreateShipment("S0001");
			var jobWithRecognizedCharge = creator.CreateJob(shipment1);
			jobWithRecognizedCharge.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			jobWithRecognizedCharge.JH_GE = creator.FISDepartment.PK;

			var shipment11 = creator.CreateShipment("S00011");
			var jobWithRecognizedCharge1 = creator.CreateJob(shipment11);
			jobWithRecognizedCharge1.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			jobWithRecognizedCharge1.JH_GE = creator.FISDepartment.PK;

			var shipment2 = creator.CreateShipment("S0002");
			var jobWithUnrecognizedCharge = creator.CreateJob(shipment2);
			jobWithUnrecognizedCharge.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			jobWithUnrecognizedCharge.JH_GE = creator.FESDepartment.PK;

			var shipment22 = creator.CreateShipment("S00022");
			var jobWithUnrecognizedCharge1 = creator.CreateJob(shipment22);
			jobWithUnrecognizedCharge1.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			jobWithUnrecognizedCharge1.JH_GE = creator.FESDepartment.PK;

			var shipment3 = creator.CreateShipment("S0003");
			var jobWithRecognizedCharge2 = creator.CreateJob(shipment3);
			jobWithRecognizedCharge2.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			jobWithRecognizedCharge2.JH_GE = creator.FIADepartment.PK;

			var shipment4 = creator.CreateShipment("S0004");
			var jobWithUnrecognizedCharge2 = creator.CreateJob(shipment4);
			jobWithUnrecognizedCharge2.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			jobWithUnrecognizedCharge2.JH_GE = creator.FEADepartment.PK;

			Factory.Save();

			CreateCharge(jobWithRecognizedCharge, true, false);
			CreateCharge(jobWithRecognizedCharge1, false, true);
			CreateCharge(jobWithUnrecognizedCharge, true, false);
			CreateCharge(jobWithUnrecognizedCharge1, false, true);
			CreateCharge(jobWithRecognizedCharge2, true, true);
			CreateCharge(jobWithUnrecognizedCharge2, false, false);

			foreach (var data in new[]
					{
						new { Job = jobWithRecognizedCharge, Message = ($"Following job(s) could not be automatically closed due to charges' cost/sell recognition status: {jobWithRecognizedCharge.JH_JobNum}") },
						new { Job = jobWithRecognizedCharge1, Message = ($"Following job(s) could not be automatically closed due to charges' cost/sell recognition status: {jobWithRecognizedCharge1.JH_JobNum}") },
						new { Job = jobWithUnrecognizedCharge, Message = ($"Following job(s) could not be automatically closed due to charges' cost/sell recognition status: {jobWithUnrecognizedCharge.JH_JobNum}") },
						new { Job = jobWithUnrecognizedCharge1, Message = ($"Following job(s) could not be automatically closed due to charges' cost/sell recognition status: {jobWithUnrecognizedCharge1.JH_JobNum}") },
						new { Job = jobWithRecognizedCharge2, Message = "" },
						new { Job = jobWithUnrecognizedCharge2, Message = "" }
					})
			{
				data.Job.Charges.Reload(true);
				var message = AutoJobClosureHelper.CreateHelperToCloseAJob(data.Job).GetAllErrorMessages();
				AssertEquals(data.Job.JH_JobNum, data.Message, message);
			}

			void CreateCharge(Job job, bool acr, bool wip)
			{
				var charge = Factory.New<BaseCharge>();
				charge.JR_JH = job.PK;
				charge.FillWithValidTestData();
				charge.JR_OH_CostAccount = charge.CalculatedCompany.OrgProxy.PK;
				charge.JR_OH_SellAccount = charge.CalculatedCompany.OrgProxy.PK;
				if (acr)
				{
					creator.CreateAccrual(charge);
				}
				else
				{
					charge.JR_OSCostAmt = 90M;
				}

				if (wip)
				{
					creator.CreateWIP(charge);
				}
				else
				{
					charge.JR_OSSellAmt = 90M;
				}
			}
		}

		void AssertCloseJobWithOpenWIPsAndAccruals(bool close, string num)
		{
			var creator = new TestObjectCreator(Factory);

			var regValue = creator.CreateJobClosureConfiguration(
			creator.CreateJobClosureConfigLine(offset: 1, closeJobWithOpenWip: close, closeJobWithOpenAcr: close));
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			var shipment = creator.CreateShipment(num);
			var job = creator.CreateJob(shipment);
			job.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			var postedCharge = creator.CreateCharge(job, creator.CC1, "Desc", creator.AUD, 111.11m, creator.Creditor1, creator.AUD, 222.22m, creator.LocalClient);
			postedCharge.JR_APInvoiceNum = "123";
			postedCharge.JR_APInvoiceDate = ZDateTime.Today;
			postedCharge.JR_PaymentDate = ZDateTime.Today;

			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			AssertNull(postedCharge.WIP);
			AssertNull(postedCharge.Accrual);

			// Create charge and not post
			var unpostedCharge = creator.CreateCharge(job, creator.FRT, "Desc", creator.AUD, 333.33m, creator.Creditor1, creator.AUD, 444.44m, creator.Debtor);

			Factory.Save();

			AssertNotNull(unpostedCharge.WIP);
			AssertNotNull(unpostedCharge.Accrual);

			var message = AutoJobClosureHelper.CreateHelperToCloseAJob(job).GetAllErrorMessages();

			if (close)
			{
				AssertEquals(string.Empty, message);
			}
			else
			{
				AssertEquals($"Following job(s) has open WIP/Accrual: {job.JH_JobNum}", message);
			}
		}

		void AssertCloseJobWithEitherOpenWIPsOrAccruals(bool createWip, bool createAccrual, bool closeJobWithOpenWip, bool closeJobWithOpenAccrual, string num)
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment(num);
			var job = creator.CreateJob(shipment);
			job.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			Factory.Save();

			var charge = Factory.New<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.FillWithValidTestData();

			if (createAccrual)
			{
				creator.CreateAccrual(charge);
			}
			else if (createWip)
			{
				creator.CreateWIP(charge);
			}
			Factory.Save();
			job.Charges.Reload(true);

			var regValue = creator.CreateJobClosureConfiguration(
				creator.CreateJobClosureConfigLine(offset: 1, closeJobWithOpenWip: closeJobWithOpenWip, closeJobWithOpenAcr: closeJobWithOpenAccrual));
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			Factory.Save();

			var message = AutoJobClosureHelper.CreateHelperToCloseAJob(job).GetAllErrorMessages();
			AssertEquals($"Following job(s) has open WIP/Accrual: {job.JH_JobNum}", message);
		}

		public override void TestErrorMessageForJobsThatCannotbeClosedDueToInactive()
		{
			SetRegistryValue(Env.CurrentCompanyPK,
								TestObjectCreator.CreateJobClosureConfigLine("ALL", "ALL", "ALL", "JOP", 10, true, true));

			var testJob1 = TestObjectCreator.CreateJob(null, 0, null, 0);
			testJob1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

			Factory.Save();
			Assert("testJob1 should be active", testJob1.JH_IsActive);

			var module = AutoJobClosureHelper.CreateHelperToCloseAJob(testJob1);
			var msg = module.GetAllErrorMessages();

			Assert("No Error Message", string.IsNullOrEmpty(msg));

			testJob1.MarkAsInactive();
			Factory.Save();

			module = AutoJobClosureHelper.CreateHelperToCloseAJob(testJob1);
			var errorMsgForInactive = $"The following job(s) are inactive. Please activate them before closing:" + "\n" + AutoJobClosureHelper.GetJobNumbers(new Job[] { testJob1 });
			AssertContains("Jobs are inactive", errorMsgForInactive, module.GetAllErrorMessages());

			testJob1.JH_Status = JobHeaderStatus.Closed.Code;
			testJob1.MarkAsInactive();
			Factory.Save();

			module = AutoJobClosureHelper.CreateHelperToCloseAJob(testJob1);
			var errMessage = $"The following job(s) are inactive. Please activate them before closing:" + "\n" + AutoJobClosureHelper.GetJobNumbers(new Job[] { testJob1 });
			var actMessage = $"Following job(s) are already closed: " + AutoJobClosureHelper.GetJobNumbers(new Job[] { testJob1 });
			AssertNotContains("Jobs are inactive", errMessage, module.GetAllErrorMessages());
			AssertContains("Jobs are inactive", actMessage, module.GetAllErrorMessages());
		}
	}
}
