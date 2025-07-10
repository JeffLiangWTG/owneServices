using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Client.UPE.ServiceTask.Testing
{
	[TestedType(typeof(MatchingActivitiesServiceTask))]
	public class MatchingActivitiesServiceTaskTest : ServiceTaskTestCase<MatchingActivitiesServiceTask>
	{
		public void TestDefaultSchedule()
		{
			AssertEquals("8seconds", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		[TestDate(2006, 3, 3)]
		public void TestExecute()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB.RequiresConsigneeMatch = true;
			OrgMatchApproval orgMatchApproval = uPECusHAWB.ConsigneeMatchApproval;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2);
			Factory.Save();
			ServiceTaskForTest.RunTask();
			AssertEquals("A formal dec should be created after match complete", false, uPECusHAWB.CS_JE_CustomsFormalEntry.IsEmpty);
			AssertEquals("Formal dec should be saved", true, uPECusHAWB.Declaration.IsInDatabase);

			var logs = ServiceTaskForTest.Logger.ToString();
			AssertContains($"------Start Matching Activities between 2006-03-01T01:00:00.000 and 2006-03-02T23:59:00.000 for {string.Join(", ", UPETools.Instance.UPECustomisationBranches(true).Select(b => b.GB_Code))} branches------", logs);
			AssertContains($"1 potential record(s) found from Organisation Matching", logs);
			AssertContains($"Processing 1/1 with Tracking No - \"{uPECusHAWB.CS_HAWB}\" & P2_RelatedDateForPatternMatch - \"2006-03-02T00:00:00.000\"", logs);
			AssertContains($"Running match approved activities on Air Cargo House \"{uPECusHAWB.CS_MessageReference}\"", logs);
			AssertContains($"Master Branch - \"{uPECusHAWB.MAWB.Branch?.GB_Code}\"", logs);
			AssertContains($"Declaration Branch - \"\"", logs);
			AssertContains($"Consignor Matched - \"{uPECusHAWB.Consignor != null}\" on \"{uPECusHAWB.Consignor?.OH_Code}\"", logs);
			AssertContains($"Importer or Consignee Matched - \"{!uPECusHAWB.ImporterOrConsigneeMatchedOrgPK.IsEmpty}\" on \"{Factory.Load<UPEOrgHeader>(uPECusHAWB.ImporterOrConsigneeMatchedOrgPK)?.OH_Code}\"", logs);
			AssertContains($"MAWB - \"{uPECusHAWB.MAWB.CM_MAWB}\"", logs);
			AssertContains($"HAWB - \"{uPECusHAWB.CS_HAWB}\"", logs);
			AssertContains($"Declaration - \"\"", logs);
			AssertNotContains($"Match approved for split shipment", logs);
			AssertContains($"Declaration \"{uPECusHAWB.Declaration.JobNumber}\" created", logs);
			AssertContains($"Freight rate is calculated for \"{uPECusHAWB.Declaration.JobNumber}\"", logs);
			AssertContains($"Finish processing 1/1", logs);
			AssertContains($"------Finish Matching Activities------", logs);

			uPECusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			ServiceTaskForTest.RunTask();
			AssertEquals("The formal dec should not be re-created subsequently", true, uPECusHAWB.CS_JE_CustomsFormalEntry.IsEmpty);
		}

		[TestDate(2006, 3, 3)]
		public void TestExecute_OrgMatchApprovalWithoutCusHAWB()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB.RequiresConsigneeMatch = true;
			var orgMatchApproval = uPECusHAWB.ConsigneeMatchApproval;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2);

			var orgPatternMatchAddress = Factory.LoadTop1<OrgPatternMatchAddress>(new ZQuery());
			orgPatternMatchAddress.P3_ParentID = ZGuid.NewZGuid();

			Factory.Save();
			ServiceTaskForTest.RunTask();

			var logs = ServiceTaskForTest.Logger.ToString();
			AssertContains($"------Start Matching Activities between 2006-03-01T01:00:00.000 and 2006-03-02T23:59:00.000 for {string.Join(", ", UPETools.Instance.UPECustomisationBranches(true).Select(b => b.GB_Code))} branches------", logs);
			AssertContains($"1 potential record(s) found from Organisation Matching", logs);
			AssertContains($"Processing 1/1 with Tracking No", logs);
			AssertNotContains($"Running match approved activities on Air Cargo House", logs);
			AssertContains($"Air Cargo House is not found.", logs);
			AssertContains($"Finish processing 1/1", logs);
			AssertContains($"------Finish Matching Activities------", logs);
		}

		[TestDate(2006, 3, 3)]
		public void TestExecute_Multiple()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB.RequiresConsigneeMatch = true;
			OrgMatchApproval orgMatchApproval = uPECusHAWB.ConsigneeMatchApproval;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2);

			uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111112";
			var uPECusHAWB2 = Factory.New<UPECusHAWB>();
			uPECusHAWB2.CS_CM = uPECusMAWB.PK;
			uPECusHAWB2.CS_HAWB = "HOUSEBILL2";
			uPECusHAWB2.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB2.RequiresConsigneeMatch = true;
			orgMatchApproval = uPECusHAWB2.ConsigneeMatchApproval;
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2, 1, 0, 1);

			Factory.Save();
			ServiceTaskForTest.RunTask();

			var logs = ServiceTaskForTest.Logger.ToString();
			AssertContains($"2 potential record(s) found from Organisation Matching", logs);
			AssertContains($"Processing 1/2 with Tracking No - \"{uPECusHAWB.CS_HAWB}\" & P2_RelatedDateForPatternMatch - \"2006-03-02T00:00:00.000\"", logs);
			AssertContains($"Running match approved activities on Air Cargo House \"{uPECusHAWB.CS_MessageReference}\"", logs);
			AssertNotContains($"Match approved for split shipment", logs);
			AssertContains($"Declaration \"{uPECusHAWB.Declaration.JobNumber}\" created", logs);
			AssertContains($"Freight rate is calculated for \"{uPECusHAWB.Declaration.JobNumber}\"", logs);
			AssertContains($"Finish processing 1/2", logs);
			AssertContains($"Processing 2/2 with Tracking No - \"{uPECusHAWB2.CS_HAWB}\" & P2_RelatedDateForPatternMatch - \"2006-03-02T01:00:01.000\"", logs);
			AssertContains($"Running match approved activities on Air Cargo House \"{uPECusHAWB2.CS_MessageReference}\"", logs);
			AssertContains($"Declaration \"{uPECusHAWB2.Declaration.JobNumber}\" created", logs);
			AssertContains($"Freight rate is calculated for \"{uPECusHAWB2.Declaration.JobNumber}\"", logs);
			AssertContains($"Finish processing 2/2", logs);
		}

		[TestDate(2006, 3, 3)]
		public void TestExecute_MultipleWithCancel()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB.RequiresConsigneeMatch = true;
			OrgMatchApproval orgMatchApproval = uPECusHAWB.ConsigneeMatchApproval;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2);

			uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111112";
			var uPECusHAWB2 = Factory.New<UPECusHAWB>();
			uPECusHAWB2.CS_CM = uPECusMAWB.PK;
			uPECusHAWB2.CS_HAWB = "HOUSEBILL2";
			uPECusHAWB2.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB2.RequiresConsigneeMatch = true;
			orgMatchApproval = uPECusHAWB2.ConsigneeMatchApproval;
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2, 1, 0, 1);

			Factory.Save();
			ServiceTaskForTest.RunTask(new CancellationTokenSource(), 1);

			var logs = ServiceTaskForTest.Logger.ToString();
			AssertContains($"------Start Matching Activities between 2006-03-01T01:00:00.000 and 2006-03-02T23:59:00.000", logs);
			AssertContains($"2 potential record(s) found from Organisation Matching", logs);
			AssertContains($"Processing 1/2 with Tracking No - \"{uPECusHAWB.CS_HAWB}\" & P2_RelatedDateForPatternMatch - \"2006-03-02T00:00:00.000\"", logs);
			AssertContains($"Running match approved activities on Air Cargo House \"{uPECusHAWB.CS_MessageReference}\"", logs);
			AssertNotContains($"Match approved for split shipment", logs);
			AssertContains($"Declaration \"{uPECusHAWB.Declaration.JobNumber}\" created", logs);
			AssertContains($"Freight rate is calculated for \"{uPECusHAWB.Declaration.JobNumber}\"", logs);
			AssertContains($"Finish processing 1/2", logs);
			AssertContains($"------Cancel Matching Activities & Set HWM to 2006-03-02T01:00:01.000------", logs);
			AssertNotContains($"------Finish Matching Activities------", logs);
			AssertEquals(orgMatchApproval.P2_RelatedDateForPatternMatch, UPEDataRegistry.Instance.MatchingActivitiesHWM);

			logger = new TestServiceLogger();
			ServiceTaskForTest = new MatchingActivitiesServiceTaskTestClass(Logger);
			ServiceTaskForTest.RunTask();
			logs = ServiceTaskForTest.Logger.ToString();
			AssertContains($"------Start Matching Activities between 2006-03-02T01:00:01.000 and 2006-03-02T23:59:00.000", logs);
			AssertContains($"Processing 1/1 with Tracking No - \"{uPECusHAWB2.CS_HAWB}\" & P2_RelatedDateForPatternMatch - \"2006-03-02T01:00:01.000\"", logs);
			AssertContains($"Running match approved activities on Air Cargo House \"{uPECusHAWB2.CS_MessageReference}\"", logs);
			AssertContains($"Declaration \"{uPECusHAWB2.Declaration.JobNumber}\" created", logs);
			AssertContains($"Freight rate is calculated for \"{uPECusHAWB2.Declaration.JobNumber}\"", logs);
			AssertContains($"Finish processing 1/1", logs);
			AssertContains($"------Finish Matching Activities------", logs);
			AssertNotContains($"------Cancel Matching Activities", logs);
		}

		[TestDate(2006, 3, 3)]
		public void TestExecute_SplitShipment()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB.RequiresConsigneeMatch = true;
			OrgMatchApproval orgMatchApproval = uPECusHAWB.ConsigneeMatchApproval;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2);
			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			var dec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			dec.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			uPECusHAWB.CS_JE_CustomsFormalEntry = dec.PK;

			Factory.Save();
			ServiceTaskForTest.RunTask();

			var logs = ServiceTaskForTest.Logger.ToString();
			AssertContains($"Declaration Branch - \"\"", logs);
			AssertContains($"Declaration - \"\"", logs);
			AssertContains($"Match approved for split shipment", logs);
			AssertNotContains($"Declaration \"{uPECusHAWB.Declaration.JobNumber}\" created", logs);
			AssertContains($"Freight rate is calculated for \"{uPECusHAWB.Declaration.JobNumber}\"", logs);
		}

		[TestDate(2006, 3, 3)]
		public void TestExecute_Declaration()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "23211111111";
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB.RequiresConsigneeMatch = true;
			OrgMatchApproval orgMatchApproval = uPECusHAWB.ConsigneeMatchApproval;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2);
			var dec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			dec.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Completed;
			uPECusHAWB.CS_JE_CustomsFormalEntry = dec.PK;

			Factory.Save();
			ServiceTaskForTest.RunTask();

			var logs = ServiceTaskForTest.Logger.ToString();
			AssertContains($"Declaration Branch - \"{dec.Branch?.GB_Code}\"", logs);
			AssertContains($"Declaration - \"{dec.JobNumber}\"", logs);
			AssertNotContains($"Match approved for split shipment", logs);
			AssertNotContains($"Declaration \"{uPECusHAWB.Declaration.JobNumber}\" created", logs);
			AssertContains($"Freight rate is calculated for \"{uPECusHAWB.Declaration.JobNumber}\"", logs);
		}

		[TestDate(2006, 3, 3)]
		public void TestExecute_NonUPEBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			Factory.Save();
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusMAWB.CM_MAWB = "23211111111";
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_HAWB = "HOUSEBILL";
			uPECusHAWB.CS_OA_ConsignorAddress = (Factory.NewWithValidTestData<OrgHeader>()).MainAddress.PK;
			uPECusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData<UPEJobDeclaration>().PK;
			uPECusMAWB.CM_GB = branch.PK;
			uPECusHAWB.Declaration.JE_GB = branch.PK;
			uPECusHAWB.RequiresConsigneeMatch = true;
			OrgMatchApproval orgMatchApproval = uPECusHAWB.ConsigneeMatchApproval;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			orgMatchApproval.ApproveMatchBySupervisor(organisation);
			orgMatchApproval.P2_RelatedDateForPatternMatch = new ZDateTime(2006, 3, 2);
			Factory.Save();
			ServiceTaskForTest.RunTask();
			AssertNotContains("Matching Activities should not be performed", "Matching Activities performed", Logger.ToString());
		}

		[TestDate(2005, 11, 2)]
		public void TestExecute_NoAU()
		{
			UPEDataRegistry.Instance.BranchToUseForUPECustomisations = Env.CurrentBranchPK;

			foreach (var br in Factory.Load<GlbBranch>(new ZQuery()))
			{
				br.SetCountry(CountryCodes.Taiwan);
			}
			Factory.Save();

			ServiceTaskForTest.RunTask();

			AssertEquals("", ServiceTaskForTest.Logger.ToString());
			AssertEquals(Env.CurrentBranchPK, UPETools.Instance.UPECustomisationBranches(true).FirstOrDefault().PK);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		#region Setup
		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUpCore();
			ServiceTaskForTest = new MatchingActivitiesServiceTaskTestClass(Logger);
			UPEDataRegistry.Instance.MatchingActivitiesHWM = new ZDateTime(2006, 3, 1, 1, 0, 0);
		}

		MatchingActivitiesServiceTaskTestClass ServiceTaskForTest;
		TestServiceLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestServiceLogger());
			}
		}

		TestServiceLogger logger;
		#region TestClass
		public class MatchingActivitiesServiceTaskTestClass : MatchingActivitiesServiceTask
		{
			public MatchingActivitiesServiceTaskTestClass(ILogger logger) : base(logger) { }

			CancellationTokenSource cts;
			int cancelOnCount;
			public void RunTask(CancellationTokenSource cts, int cancelOnCount)
			{
				this.cts = cts;
				this.cancelOnCount = cancelOnCount;
				base.RunTask(cts.Token);
			}

			public void RunTask(bool fakeConcurrencyCondition)
			{
				this.FakeConcurrencyCondition = fakeConcurrencyCondition;
				base.RunTask(CancellationToken.None);
			}

			int numberOfRuns;
			protected override void RunMatchApprovedActivities(UPECusHAWB uPECusHAWB)
			{
				numberOfRuns++;

				if (FakeConcurrencyCondition)
				{
					BusinessObjectFactory secondFactory = new BusinessObjectFactory();
					secondFactory.RefreshEnabled = false;
					UPECusHAWB changedUPECusHAWB = secondFactory.Load<UPECusHAWB>(uPECusHAWB.PK);
					changedUPECusHAWB.CS_CustomsStatus = "FIN";
					secondFactory.Save();
				}

				if (cts != null && numberOfRuns == cancelOnCount)
				{
					cts.Cancel();
				}

				base.RunMatchApprovedActivities(uPECusHAWB);
			}

			bool FakeConcurrencyCondition;
		}
		#endregion
		#endregion
	}
}
