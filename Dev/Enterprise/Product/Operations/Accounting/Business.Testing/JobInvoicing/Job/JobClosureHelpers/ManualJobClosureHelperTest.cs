using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ManualJobClosureHelperTest : JobClosureHelperTest
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
				var jobsSelectedToClose = new List<Job>();
				var expectedJobsThatShouldBeClosed = new List<Job>();

				var shipment = TestObjectCreator.CreateShipment("S00001");
				var job = TestObjectCreator.CreateJob(shipment);
				jobsSelectedToClose.Add(job);
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

				var jobCloseHelper = ManualJobClosureHelper.CreateHelper(jobsSelectedToClose);
				AssertContainsExactElementsInAnyOrder(expectedJobsThatShouldBeClosed, jobCloseHelper.JobsThatCanBeClosed);
				var jobsThatCanNotBeClosed = jobsSelectedToClose.Except(expectedJobsThatShouldBeClosed);
				var errorMessage = jobCloseHelper.GetAllErrorMessages();
				if (isCashAdvanceFunctionalityEnabled)
				{
					AssertEquals($"Following job(s) could not be closed due to active Advance Payment: {JobClosureHelper.GetJobNumbers(jobsThatCanNotBeClosed)}", errorMessage);
				}
				else
				{
					Assert(errorMessage.IsNullOrEmpty());
				}
			}
		}

		public void TestJobSelectionForSingleConsolCost()
		{
			var consol = SetUpConsol();
			var job1 = consol.Shipments[0].Job as Job;
			var job2 = consol.Shipments[1].Job as Job;
			var job3 = consol.Shipments[2].Job as Job;
			var job4 = consol.Shipments[3].Job as Job;

			//Before Posting

			Factory.Save();
			var selectedJobs = new[] { job1, job2, job3 };
			var module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			var errorMessage = module.GetAllErrorMessages();
			AssertEquals("SJSC1.2: Should be no missing Jobs. (Correct selection)", string.Empty, errorMessage);

			selectedJobs = new[] { job1, job2 };
			module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			errorMessage = module.GetAllErrorMessages();
			AssertContains("SJSC2.2: Should be one Missing Job (Incorrect selection). Job Number", job3.JH_JobNum, errorMessage);

			//After Posting	

			TestObjectCreator.PostCost(job3.Charges[0], new Guid("6d935954-0e05-4fab-a93b-4f344ed529de"));

			selectedJobs = new[] { job1, job2 };
			module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			errorMessage = module.GetAllErrorMessages();
			AssertEquals("SJSC3.2: Should be no missing Jobs. (Correct selection)", string.Empty, errorMessage);

			selectedJobs = new[] { job1 };
			module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			errorMessage = module.GetAllErrorMessages();
			AssertContains("SJSC4.2: Should be one Missing Job (Incorrect selection). Job Number", job2.JH_JobNum, errorMessage);
		}

		public void TestJobSelectionForMultipleConsolCost()
		{
			var consol = SetUpConsol();
			AddOneMoreCostToConsol(consol);

			var job1 = consol.Shipments[0].Job as Job;
			var job2 = consol.Shipments[1].Job as Job;
			var job3 = consol.Shipments[2].Job as Job;
			var job4 = consol.Shipments[3].Job as Job;

			//Before Posting
			//For costCC1 S1, S2 and S3 have Apportioned Charges. For costCC2 S2 and S3 have Apportioned Charges, S4 has no apportioned Charge

			Factory.Save();

			var selectedJobs = new[] { job1, job2, job3 };
			var module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			var errorMessage = module.GetAllErrorMessages();
			AssertEquals("SJMC1.2: Should be no missing Jobs. (Correct selection)", string.Empty, errorMessage);

			selectedJobs = new[] { job1, job2 };
			module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			errorMessage = module.GetAllErrorMessages();
			AssertContains("SJMC2.3: Should be one Missing Job (Incorrect selection). Job Number", job3.JH_JobNum, errorMessage);

			//After Posting
			//For costCC1 S1, S2 and S3 have Apportioned Charges. For costCC2 S2 and S3 have Apportioned Charges, S4 has no apportioned Charge
			//Posted Charges: costCC1 for S1 and costCC2 for S3

			TestObjectCreator.PostCost(job1.Charges[0], new Guid("6d935954-0e05-4fab-a93b-4f344ed529de"));
			TestObjectCreator.PostCost(job3.Charges[1], new Guid("3dedb412-6ebd-4c86-82f3-47f8c9daba86"));

			selectedJobs = new[] { job2, job3 };
			module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			errorMessage = module.GetAllErrorMessages();
			AssertEquals("SJMC3.2: Should be no missing Jobs. (Correct selection)", string.Empty, errorMessage);

			selectedJobs = new[] { job1, job2 };
			module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			errorMessage = module.GetAllErrorMessages();
			AssertContains("SJMC4.2: Should be one Missing Job (Incorrect selection). Job Pk Value", job3.JH_JobNum, errorMessage);
		}

		public void TestUnpostedConsolCostDeletion()
		{
			ForwardingConsol consol = SetUpConsol();
			AddOneMoreCostToConsol(consol);

			Factory.Save();

			Job job1 = consol.Shipments[0].Job as Job;
			Job job2 = consol.Shipments[1].Job as Job;
			Job job3 = consol.Shipments[2].Job as Job;
			Job job4 = consol.Shipments[3].Job as Job;

			Assert("Has Consol Costs", consol.HasConsolCosts(GlbCompany.CurrentCompany));
			AssertNotNull("S1 CC1", ((Job)consol.Shipments[0].Job).Charges[0].ParentConsolCost);
			AssertNotNull("S2 CC1", ((Job)consol.Shipments[1].Job).Charges[0].ParentConsolCost);
			AssertNotNull("S3 CC1", ((Job)consol.Shipments[2].Job).Charges[0].ParentConsolCost);

			AssertNotNull("S2 CC2", ((Job)consol.Shipments[1].Job).Charges[1].ParentConsolCost);
			AssertNotNull("S3 CC2", ((Job)consol.Shipments[2].Job).Charges[1].ParentConsolCost);

			var selectedJobs = new[] { job1, job2, job3 };
			var module = ManualJobClosureHelper.CreateHelper(selectedJobs);
			module.DeleteConsolCostsLinkedToUnpostedApportionedChargesIfAny();
			Factory.Save();
			AssertEquals("No Consol Costs", false, consol.HasConsolCosts(GlbCompany.CurrentCompany));
		}

		public void TestErrorMessageForJobsRequiringProfitLossReasonCode()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			Job testJob = testObjectCreator.CreateJob(null, 0, null, 0);

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = testObjectCreator.CC1.PK;
			testCharge.JR_LocalCostAmt = 200;
			testCharge.JR_LocalSellAmt = 211;

			testJob.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var reloadedTestJobs = Factory.Load<Job>(testJob.PK);

			var module = ManualJobClosureHelper.CreateHelper(new Job[] { reloadedTestJobs });
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

			module = ManualJobClosureHelper.CreateHelper(new Job[] { reloadedTestJobs });
			msg = module.GetAllErrorMessages();
			Assert("Job requires Profit/Loss Reason Code", !msg.IsEmpty);
			AssertContains("Job Number that requires Profit/Loss Reason Code", testJob.JH_JobNum, msg);

			plRequiringReasonParameters.ProfitThreshold = 10M;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);
			module = ManualJobClosureHelper.CreateHelper(new Job[] { reloadedTestJobs });
			msg = module.GetAllErrorMessages();
			Assert("No Job requires Profit/Loss Reason Code", msg.IsEmpty);
		}

		public void TestErrorMessageForJobsThatAreAlreadyClosed()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			Job testJob1 = testObjectCreator.CreateJob(null, 0, null, 0);
			Charge testCharge1 = testJob1.Charges.AddNew();
			testCharge1.JR_AC = testObjectCreator.CC1.PK;
			testCharge1.JR_LocalCostAmt = 200;
			testCharge1.JR_LocalSellAmt = 211;

			Job testJob2 = testObjectCreator.CreateJob(null, 0, null, 0);
			Charge testCharge2 = testJob2.Charges.AddNew();
			testCharge2.JR_AC = testObjectCreator.CC1.PK;
			testCharge2.JR_LocalCostAmt = 300;
			testCharge2.JR_LocalSellAmt = 311;

			Job testJob3 = testObjectCreator.CreateJob(null, 0, null, 0);
			Charge testCharge3 = testJob3.Charges.AddNew();
			testCharge3.JR_AC = testObjectCreator.CC3.PK;
			testCharge3.JR_LocalCostAmt = 400;
			testCharge3.JR_LocalSellAmt = 411;

			Factory.Save();

			//Before Closing any Job
			var reloadedTestJob1 = Factory.Load<Job>(testJob1.PK);
			var reloadedTestJob2 = Factory.Load<Job>(testJob2.PK);
			var reloadedTestJob3 = Factory.Load<Job>(testJob3.PK);
			var module = ManualJobClosureHelper.CreateHelper(new Job[] { reloadedTestJob1, reloadedTestJob2, reloadedTestJob3 });
			var msg = module.GetAllErrorMessages();
			Assert("No Jobs", string.IsNullOrEmpty(msg));

			//After Closing few Jobs
			reloadedTestJob1.JH_Status = JobHeaderStatus.Closed.Code;
			reloadedTestJob2.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			reloadedTestJob1 = Factory.Load<Job>(reloadedTestJob1.PK);
			reloadedTestJob2 = Factory.Load<Job>(reloadedTestJob2.PK);
			reloadedTestJob3 = Factory.Load<Job>(reloadedTestJob3.PK);

			module = ManualJobClosureHelper.CreateHelper(new Job[] { reloadedTestJob1, reloadedTestJob2, reloadedTestJob3 });
			msg = module.GetAllErrorMessages();
			AssertContains("Jobs are already Closed", ManualJobClosureHelper.GetJobNumbers(new Job[] { reloadedTestJob1, reloadedTestJob2 }), msg);
		}

		public void TestErrorMessageForUserIsNotAllowedToChangeStatusOfCompleteJobs()
		{
			bool isAllowed = Env.Security.ChangeStatusOfCompleteJobs.IsAllowed;
			try
			{
				OrgHeader creditor = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false);
				Job testJob = TestObjectCreator.CreateJob(creditor, 0, null, 0);
				testJob.JH_Status = JobHeaderStatus.Complete.Code;
				Factory.Save();

				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = false;

				var module = ManualJobClosureHelper.CreateHelper(new Job[] { testJob });
				var msg = module.GetAllErrorMessages();
				AssertContains("Message Displayed", "The following job(s) are complete and you are not allowed to change their status due to security rights", msg);

				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = true;

				module = ManualJobClosureHelper.CreateHelper(new Job[] { testJob });
				msg = module.GetAllErrorMessages();
				Assert("No Jobs", string.IsNullOrEmpty(msg));
			}
			finally
			{
				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = isAllowed;
			}
		}

		public void TestJobsThatCanBeClosed()
		{
			bool isAllowed = Env.Security.ChangeStatusOfCompleteJobs.IsAllowed;
			try
			{
				var creditor = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false);

				//Closed Job
				var testJobClosed = TestObjectCreator.CreateJob(creditor, 0, null, 0);
				testJobClosed.JH_Status = JobHeaderStatus.Closed.Code;

				//Complete Job
				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = false;
				var testJobCompleted = TestObjectCreator.CreateJob(creditor, 0, null, 0);
				testJobCompleted.JH_Status = JobHeaderStatus.Complete.Code;

				//Requires Profit and Loss reason
				var testJobRequiresReasonCode = TestObjectCreator.CreateJob(null, 0, null, 0);
				var testCharge = testJobRequiresReasonCode.Charges.AddNew();
				testCharge.JR_AC = TestObjectCreator.CC1.PK;
				testCharge.JR_LocalCostAmt = 200;
				testCharge.JR_LocalSellAmt = 211;
				testJobRequiresReasonCode.JH_ProfitLossReasonCode = string.Empty;

				JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
				JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
				plReasonCode.Code = "TST";
				plReasonCode.Description = (NoResString)"Test";
				AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

				JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
				plRequiringReasonParameters.ProfitThreshold = 5M;
				plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.Closed.Code;
				AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

				//linked to a consol
				var consol = SetUpConsol();
				AddOneMoreCostToConsol(consol);

				var linkedJob1 = consol.Shipments[0].Job as Job;
				var linkedJob2 = consol.Shipments[1].Job as Job;
				var linkedJob3 = consol.Shipments[2].Job as Job;
				var linkedJob4 = consol.Shipments[3].Job as Job;

				//Job that can be closed
				var job1 = TestObjectCreator.CreateJob(creditor, 0, null, 0);

				Factory.Save();

				var selectedJobs = new[] { testJobClosed, testJobCompleted, testJobRequiresReasonCode, linkedJob1, linkedJob2, job1 };
				var jobCloseHelper = ManualJobClosureHelper.CreateHelper(selectedJobs);
				Assert("Job That can be closed", jobCloseHelper.JobsThatCanBeClosed.Contains(job1));
			}
			finally
			{
				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = isAllowed;
			}
		}

		new ForwardingConsol SetUpConsol()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job1.Charges.RemoveAll();

			var shipment2 = TestObjectCreator.CreateShipment("S0002", "AUSYD", "USLAX");
			shipment2.JS_ActualWeight = 251.73m;
			shipment2.JS_ActualChargeable = 23m;
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job2.Charges.RemoveAll();

			var shipment3 = TestObjectCreator.CreateShipment("S0003", "AUSYD", "USLAX");
			shipment3.JS_ActualWeight = 351.73m;
			shipment3.JS_ActualChargeable = 33m;
			var job3 = TestObjectCreator.CreateJob(shipment3, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job3.Charges.RemoveAll();

			var shipment4 = TestObjectCreator.CreateShipment("S0004", "AUSYD", "USLAX");
			shipment4.JS_ActualWeight = 451.73m;
			shipment4.JS_ActualChargeable = 43m;
			var job4 = TestObjectCreator.CreateJob(shipment4, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job4.Charges.RemoveAll();

			ForwardingShipment[] shipments = new ForwardingShipment[] { shipment1, shipment2, shipment3, shipment4 };

			//Creating Consol Test
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipments[0]);
			consol.Shipments.Add(shipments[1]);
			consol.Shipments.Add(shipments[2]);
			consol.Shipments.Add(shipments[3]);

			ApportionmentListing listing = new ApportionmentListing(Factory, consol);

			//ConsolCost1
			JobConsolCost costCC1 = listing.CostsCollection.TryAddNew();
			costCC1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costCC1.E6_OSCostAmount = 50m;

			//s1
			costCC1.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			costCC1.ApportionmentCharges[0].JR_E6 = costCC1.PK;
			AddJobCharge(job1, costCC1);

			//s2
			costCC1.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			costCC1.ApportionmentCharges[1].JR_E6 = costCC1.PK;
			AddJobCharge(job2, costCC1);

			//s3
			costCC1.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			costCC1.ApportionmentCharges[2].JR_E6 = costCC1.PK;
			AddJobCharge(job3, costCC1);

			//s4
			costCC1.ApportionmentCharges[3].JR_IsUsedForApportionment = false;
			costCC1.ApportionmentCharges[3].JR_E6 = ZGuid.Empty;

			return consol;
		}

		void AddOneMoreCostToConsol(ForwardingConsol consol)
		{
			JobConsolCost cost2 = null;

			var listing = consol.GetApportionments();

			//ConsolCost2
			cost2 = listing.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			cost2.E6_OSCostAmount = 100m;

			//s1
			cost2.ApportionmentCharges[0].JR_IsUsedForApportionment = false;
			cost2.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;

			//s2
			cost2.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			cost2.ApportionmentCharges[1].JR_E6 = cost2.PK;
			AddJobCharge(consol.Shipments[1].Job as Job, cost2);

			//s3
			cost2.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			cost2.ApportionmentCharges[2].JR_E6 = cost2.PK;
			AddJobCharge(consol.Shipments[2].Job as Job, cost2);

			//s4
			cost2.ApportionmentCharges[3].JR_IsUsedForApportionment = false;
			cost2.ApportionmentCharges[3].JR_E6 = ZGuid.Empty;
		}

		public override void TestErrorMessageForJobsThatCannotbeClosedDueToInactive()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var testJob1 = testObjectCreator.CreateJob(null, 0, null, 0);
			var testJob2 = testObjectCreator.CreateJob(null, 0, null, 0);
			var testJob3 = testObjectCreator.CreateJob(null, 0, null, 0);

			Factory.Save();

			Assert("testJob1 should be active", testJob1.JH_IsActive);
			Assert("testJob2 should be active", testJob2.JH_IsActive);
			Assert("testJob3 should be active", testJob3.JH_IsActive);

			var module = ManualJobClosureHelper.CreateHelper(new Job[] { testJob1, testJob2, testJob3 });
			var msg = module.GetAllErrorMessages();
			Assert("No Error Message", string.IsNullOrEmpty(msg));

			testJob1.MarkAsInactive();
			testJob2.MarkAsInactive();
			Factory.Save();

			module = ManualJobClosureHelper.CreateHelper(new Job[] { testJob1, testJob2, testJob3 });
			msg = module.GetAllErrorMessages();
			AssertContains("Jobs are inactive", ManualJobClosureHelper.GetJobNumbers(new Job[] { testJob1, testJob2 }), msg);

			testJob3.JH_Status = JobHeaderStatus.Closed.Code;
			testJob3.MarkAsInactive();
			Factory.Save();

			module = ManualJobClosureHelper.CreateHelper(new Job[] { testJob3 });
			var errMessage = $"The following job(s) are inactive. Please activate them before closing:" + "\n" + AutoJobClosureHelper.GetJobNumbers(new Job[] { testJob3 });
			var actMessage = $"The following job(s) are already closed:" + "\n" + AutoJobClosureHelper.GetJobNumbers(new Job[] { testJob3 });
			AssertNotContains("Jobs are already Closed", errMessage, module.GetAllErrorMessages());
			AssertContains("Jobs are already Closed", actMessage, module.GetAllErrorMessages());
		}
	}
}
