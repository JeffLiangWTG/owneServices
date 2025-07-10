#if DEBUG
using System;
using System.Linq;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(EDIDataRegistry))]
	class IssueWorkItemCreationThresholdRegistryTest : RegistryItemSetTestCaseWithFactory<EDIDataRegistry>
	{
		public void TestIssueWorkItemCreation_DefaultValue()
		{
			AssertEquals("Default value is not correct", 1, ItemSet.IssueWorkItemCreationThresholdClientVisible.Value.Cast<IssueWorkItemCreationThreshold>().Single().IssueOccurrenceThreshold);
			AssertEquals("Default value is not correct", 14, ItemSet.IssueWorkItemCreationThresholdClientVisible.Value.Cast<IssueWorkItemCreationThreshold>().Single().ThresholdTimespan);
		}
	}

	[TestedType(typeof(IssueWorkItemCreationThreshold))]
	class IssueWorkItemCreationThresholdTest : RegistryBusinessObjectTemplateTestCase<IssueWorkItemCreationThreshold>
	{
		protected override IssueWorkItemCreationThreshold GetBusinessObjectToClone()
		{
			return new IssueWorkItemCreationThreshold(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override IssueWorkItemCreationThreshold GetBusinessObjectToSerialise()
		{
			return new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 12, ThresholdTimespan = 5 };
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

	public void TestIssueWorkItemCreation_CanChangeValue_OccurrenceThreshold()
		{
			var threshold = new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 12, ThresholdTimespan = 5 };

			const int newValue = 69;
			threshold.IssueOccurrenceThreshold = newValue;
			AssertEquals("The value has not changed", newValue, threshold.IssueOccurrenceThreshold);
		}

		public void TestIssueWorkItemCreation_CanChangeValue_ThresholdTimespan()
		{
			var threshold = new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 12, ThresholdTimespan = 5 };

			const int newValue = 69;
			threshold.ThresholdTimespan = newValue;
			AssertEquals("The value has not changed", newValue, threshold.ThresholdTimespan);
		}

		public void TestIssueWorkItemCreation_Occurrences_MinValidation()
		{
			var threshold = new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 12, ThresholdTimespan = 5 };
			AssertNoErrors(threshold.IssueOccurrenceThresholdInfo);

			const int invalidValue = 0;
			threshold.IssueOccurrenceThreshold = invalidValue;
			AssertHasErrors(threshold.IssueOccurrenceThresholdInfo);
		}

		public void TestIssueWorkItemCreation_Timespan_MinValidation()
		{
			var threshold = new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 12, ThresholdTimespan = 5 };
			AssertNoErrors(threshold.ThresholdTimespanInfo);

			const int invalidValue = 0;
			threshold.ThresholdTimespan = invalidValue;
			AssertHasErrors(threshold.ThresholdTimespanInfo);
		}
	}
}
#endif
