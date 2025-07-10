using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagRuleThrottlingThreshold))]
	public class TagRuleThrottlingThresholdTest : RegistryBusinessObjectTemplateTestCase<TagRuleThrottlingThreshold>
	{
		public void TestValidateRunTime()
		{
			var threshold = NewPopulatedBusinessObject();
			threshold.RunTime = 1;

			AssertNoErrors(threshold.RunTimeInfo);

			threshold.RunTime = 0;

			AssertHasError(threshold.RunTimeInfo, "Please enter a 'Run Time' greater than 0.");
		}

		public void TestValidateRunInterval()
		{
			var threshold = NewPopulatedBusinessObject();
			threshold.RunTime = 30;
			threshold.RunInterval = 1;

			AssertNoErrors(threshold.RunIntervalInfo);

			threshold.RunInterval = 0;

			AssertHasError(threshold.RunIntervalInfo, "Please enter a 'Run Interval' greater than 0.");

			threshold.RunTime = 90;
			threshold.RunInterval = 1;

			AssertHasError(threshold.RunIntervalInfo, "Value must be longer in duration than the specified run time. Run time: 90 seconds. Interval entered: 60 seconds.");
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override TagRuleThrottlingThreshold GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override TagRuleThrottlingThreshold GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		TagRuleThrottlingThreshold NewPopulatedBusinessObject()
		{
			return new TagRuleThrottlingThreshold(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
