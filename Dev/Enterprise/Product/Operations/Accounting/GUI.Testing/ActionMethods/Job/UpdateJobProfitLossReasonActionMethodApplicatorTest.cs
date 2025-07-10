using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobProfitLossReasonActionMethodApplicator))]
	public class UpdateJobProfitLossReasonActionMethodApplicatorTest : UpdateJobActionMethodApplicatorBaseTest
	{
		public void TestProfitLossReason()
		{
			SetupJobPropertySameWithApplicatorValue();

			AssertEquals("TST", Applicator.ProfitLossReason);
			AssertEquals(AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Value.GetCodeDescriptionPairList(), Applicator.ProfitLossReasonCodeList);
			AssertNotNull(Applicator.ProfitLossReasonInfo);
		}

		public void TestValidateProfitLossReason()
		{
			SetupApplicatorPropertyForValidation();
			AssertHasError(Applicator.ProfitLossReasonInfo, "Enter a valid selection.");
		}

		protected override void SetupJobPropertySameWithApplicatorValue()
		{
			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, jobReasonCollection);

			Applicator.ProfitLossReason = "TST";
			Shipment1.Job.JH_ProfitLossReasonCode = "TST";
		}

		protected override void SetupJobPropertyInfoReadOnly()
		{
			Applicator.ProfitLossReason = "TST";
			Shipment1.Job.SetReadOnlyIncludingChildren(true);
		}

		protected override void SetupApplicatorPropertyForValidation()
		{
			Applicator.ProfitLossReason = "XXX";
		}

		protected override void AssertUpdateJobPropertySuccessful()
		{
			var profitLossReason = "TST";
			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = profitLossReason;

			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, jobReasonCollection);

			AssertNotEquals(profitLossReason, Shipment1.Job.JH_ProfitLossReasonCode);
			AssertNotEquals(profitLossReason, Shipment2.Job.JH_ProfitLossReasonCode);
			Applicator.ProfitLossReason = profitLossReason;
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			var jobNumber2 = Shipment2.Job.JH_JobNum;

			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
INFO: Job {jobNumber1}: Processed.

INFO: Job {jobNumber2}: Start Process.
INFO: Job {jobNumber2}: Processed.");

			AssertEquals(profitLossReason, Shipment1.Job.JH_ProfitLossReasonCode);
			AssertEquals(profitLossReason, Shipment2.Job.JH_ProfitLossReasonCode);
		}

		public override string ExpectedValidationErrorLog => $@"INFO: Job {Shipment1.Job.JH_JobNum}: Start Process.
ERROR: Job has errors: Enter a valid Profit/Loss Reason.
Job Profit/Loss Reason Codes registry item (Accounting/Job Invoicing/Job Profit Reason) is empty. Please set correct values.
";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateJobProfitLossReasonActionMethodApplicator(Factory);
		}

		new UpdateJobProfitLossReasonActionMethodApplicator Applicator => (UpdateJobProfitLossReasonActionMethodApplicator)base.Applicator;
	}
}
