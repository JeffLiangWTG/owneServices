using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagRuleThrottlingThresholdCollection))]
	public class TagRuleThrottlingThresholdCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TagRuleThrottlingThresholdCollection>
	{
		public void TestGetRunIntervalMinutesForRunTime()
		{
			const string tagRuleName = "someTagRule";
			var collection = new TagRuleThrottlingThresholdCollection();
			collection.AddNew(30, 45);
			collection.AddNew(60, 100);

			AssertEquals(TagRuleThrottlingThreshold.NotSpecifiedValue, collection.GetRunIntervalMinutesForRunTime(15, tagRuleName));
			AssertEquals(45, collection.GetRunIntervalMinutesForRunTime(30, tagRuleName));
			AssertEquals(45, collection.GetRunIntervalMinutesForRunTime(31, tagRuleName));
			AssertEquals(100, collection.GetRunIntervalMinutesForRunTime(60, tagRuleName));
			AssertEquals(100, collection.GetRunIntervalMinutesForRunTime(1000, tagRuleName));
		}

		public void TestIsRunTimeSpecified()
		{
			var collection = new TagRuleThrottlingThresholdCollection();
			collection.AddNew(30, 45);

			AssertEquals(true, collection.IsRunTimeSpecified(30));
			AssertEquals(false, collection.IsRunTimeSpecified(31));
		}

		public void TestAddDuplicateRunTime_ShouldThrowException()
		{
			var collection = new TagRuleThrottlingThresholdCollection();
			collection.AddNew(30, 45);

			AssertExceptionThrown<InvalidOperationException>("The run time was added twice, so an exception should be thrown, and yet...", () =>
			{
				collection.AddNew(30, 60);
			});
		}

		public void TestGetRunIntervalMinutesForRunTime_ShouldReportError_WhenRunTimeInSecondsIsLessThanZero()
		{
			const string tagRuleName = "someTagRule";
			var collection = new TagRuleThrottlingThresholdCollection();
			var result = collection.GetRunIntervalMinutesForRunTime(-1, tagRuleName);

			AssertEquals(TagRuleThrottlingThreshold.NotSpecifiedValue, result);
			AssertEquals("GetRunIntervalMinutesForRunTime with negative runTimeInSeconds", ErrorReporter.LastKeyReported);
			AssertEquals("runTimeInSeconds is less than 0: -1 for TagRule: someTagRule", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			result = collection.GetRunIntervalMinutesForRunTime(-1983, tagRuleName);
			AssertEquals("GetRunIntervalMinutesForRunTime with negative runTimeInSeconds", ErrorReporter.LastKeyReported);
			AssertEquals("runTimeInSeconds is less than 0: -1983 for TagRule: someTagRule", ErrorReporter.LastMessageReported);
			AssertEquals(TagRuleThrottlingThreshold.NotSpecifiedValue, result);

			ErrorReporter.Clear();
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override TagRuleThrottlingThresholdCollection GetCollectionToTest()
		{
			return new TagRuleThrottlingThresholdCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TagRuleThrottlingThreshold(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
