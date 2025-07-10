using CargoWise.ComponentModel;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobProfitLossRequiringReasonParameters))]
	public class JobProfitLossRequiringReasonParametersTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestRunPreSaveValidation()
		{
			BizObj.ProfitThreshold = -1M;
			BizObj.LossThreshold = 0M;
			BizObj.RunPreSaveValidation();
			Assert("Margin Threshold can't be less than loss threshold", BizObj.ProfitThresholdInfo.HasErrors());

			BizObj.ProfitThreshold = 1M;
			BizObj.RunPreSaveValidation();
			Assert("Profit Threshold is greater than zero", !BizObj.ProfitThresholdInfo.HasErrors());
			AssertHasRowError("For Margin Threshold > 0 Job Statuses should be added.", BizObj, "At least one Status Code must be added to the Code Grid.");

			BizObj.ProfitThreshold = 0M;
			BizObj.RunPreSaveValidation();
			Assert("Profit Threshold can be zero", !BizObj.ProfitThresholdInfo.HasErrors());
			AssertNoErrors("It's default registry item state, so no validation errors", BizObj);

			BizObj.JobStatusCollection.AddNew();
			BizObj.RunPreSaveValidation();
			Assert("Job Status Code should be set", BizObj.JobStatusCollection.HasErrors());
			Assert("Margin Threshold can be zero", !BizObj.ProfitThresholdInfo.HasErrors());
			AssertNoRowErrors(BizObj);

			BizObj.JobStatusCollection[0].Code = "WRK";
			BizObj.RunPreSaveValidation();
			Assert("Job Status Code already set", !BizObj.JobStatusCollection.HasErrors());
			Assert("Margin Threshold can be zero", !BizObj.ProfitThresholdInfo.HasErrors());
			AssertNoRowErrors(BizObj);

			BizObj.ProfitThreshold = 1M;
			BizObj.RunPreSaveValidation();
			Assert("Margin Threshold already greater than zero", !BizObj.ProfitThresholdInfo.HasErrors());
			AssertNoErrors(BizObj);

			BizObj.LossThreshold = -1M;
			BizObj.RunPreSaveValidation();
			Assert("Loss Threshold can be less than zero", !BizObj.LossThresholdInfo.HasErrors());

			BizObj.LossThreshold = 2M;
			BizObj.RunPreSaveValidation();
			Assert("Loss Threshold can't be greater than profit threshold", BizObj.LossThresholdInfo.HasErrors());
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			//BizObj.IsEnabled = true;
			BizObj.ProfitThreshold = 1M;
			BizObj.JobStatusCollection.AddNew().Code = "WRK";

			return BizObj;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new JobProfitLossRequiringReasonParameters BizObj
		{
			get { return (JobProfitLossRequiringReasonParameters)base.BizObj; }
		}

		#endregion
	}
}
