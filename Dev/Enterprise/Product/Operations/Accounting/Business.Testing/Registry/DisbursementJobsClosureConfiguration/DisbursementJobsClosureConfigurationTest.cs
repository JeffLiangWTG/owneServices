using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DisbursementJobsClosureConfiguration))]
	class DisbursementJobsClosureConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new DisbursementJobsClosureConfiguration();

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var result = new DisbursementJobsClosureConfiguration();

			return result;
		}

		const string msg = "Value must be greater than or equal to 0.";

		public void TestValidateJobLevelOfShortfallUpTo()
		{
			BizObj.JobLevelOfShortfallUpTo = 0;
			AssertNoError(BizObj.JobLevelOfShortfallUpToInfo, msg);

			BizObj.JobLevelOfShortfallUpTo = 1;
			AssertNoError(BizObj.JobLevelOfShortfallUpToInfo, msg);

			BizObj.JobLevelOfShortfallUpTo = -1;
			AssertHasError(BizObj.JobLevelOfShortfallUpToInfo, msg);
		}

		public void TestValidateJobLevelOfSurplusUpTo()
		{
			BizObj.JobLevelOfSurplusUpTo = 0;
			AssertNoError(BizObj.JobLevelOfSurplusUpToInfo, msg);

			BizObj.JobLevelOfSurplusUpTo = 1;
			AssertNoError(BizObj.JobLevelOfSurplusUpToInfo, msg);

			BizObj.JobLevelOfSurplusUpTo = -1;
			AssertHasError(BizObj.JobLevelOfSurplusUpToInfo, msg);
		}

		public void TestValidateAggregatedLevelOfShortfallUpTo()
		{
			BizObj.AggregatedLevelOfShortfallUpTo = 0;
			AssertNoError(BizObj.AggregatedLevelOfShortfallUpToInfo, msg);

			BizObj.AggregatedLevelOfShortfallUpTo = 1;
			AssertNoError(BizObj.AggregatedLevelOfShortfallUpToInfo, msg);

			BizObj.AggregatedLevelOfShortfallUpTo = -1;
			AssertHasError(BizObj.AggregatedLevelOfShortfallUpToInfo, msg);
		}

		public void TestValidateAggregatedLevelOfSurplusUpTo()
		{
			BizObj.AggregatedLevelOfSurplusUpTo = 0;
			AssertNoError(BizObj.AggregatedLevelOfSurplusUpToInfo, msg);

			BizObj.AggregatedLevelOfSurplusUpTo = 1;
			AssertNoError(BizObj.AggregatedLevelOfSurplusUpToInfo, msg);

			BizObj.AggregatedLevelOfSurplusUpTo = -1;
			AssertHasError(BizObj.AggregatedLevelOfSurplusUpToInfo, msg);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new DisbursementJobsClosureConfiguration BizObj
		{
			get { return (DisbursementJobsClosureConfiguration)base.BizObj; }
		}

		#endregion
	}
}
