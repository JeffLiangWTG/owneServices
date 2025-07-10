using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ChannelCapacityCalculatorTest : ChannelCapacityTest
	{
		[TestDate(2013, 4, 16)]
		public void TestCapacity_ForUSLocale()
		{
			TestCapacityCore("en-US");
		}

		[TestDate(2013, 4, 16)]
		public void TestCapacity_ForAULocale()
		{
			TestCapacityCore("en-AU");
		}

		[TestDate(2013, 4, 16)]
		public void TestCapacity_ForCurrentUserLocal()
		{
			TestCapacityCore();
		}

		protected override string ExpectedStaff1AllocatedMessageForTestCapacityCore =>
			@"Allocated Capacity in buffer: 1 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0.5 hours (consumed 1 hours)";

		protected override string ExpectedStaff2AllocatedMessageForTestCapacityCore =>
			@"Allocated Capacity in buffer: 9 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 4.5 hours (consumed 9 hours)";

		protected override string ExpectedAllocatedMessageForTestMultiBufferCapacity2Core =>
			@"Allocated Capacity in buffer1: 1 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 1 hours

Allocated Capacity in buffer2: 2 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 2 hours";

		#region Resource Capacity

		[TestDate(2013, 11, 16, 9, 0, 0)]
		public void TestResourceChannelCapacity_ShouldCapAtBufferLoadLimit()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TNY", "Tiny Abbott");

			var buffer = CreateBuffer(system, loadLimitPercent: 10);
			var section = CreateBoardSection(buffer);

			var task1 = BMSTestHelper.CreateTaskAndWorkflow(buffer, resource, estimateDurationInMinutes: 60);
			var task2 = BMSTestHelper.CreateTask(task1.GetProcessHeader(), resource.GS_Code, 4 * 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.CreateChannelForTest(resource);
			var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
			var capacity = new ChannelCapacity(channel, cell, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

			CapacityCalculatorTestHelper.AssertCapacity(capacity, "Tiny Abbott", "Total Capacity: 9.6 hours", "Available Capacity: 2.6 hours",
@"Allocated Capacity in buffer: 7 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 7 hours", "Calculated at: 16-Nov-2013 19:00:00");
		}

		protected override void AssertComponentCapacity_InactiveAdditionalComponent_ShouldNotBeIncluded_WhenComponentActive(ChannelCapacity channelCapacity)
		{
			CapacityCalculatorTestHelper.AssertCapacity("Non-Constrained Mode board section", channelCapacity, "JohnLock :'(",
				@"Total Capacity:
    buffer: 48 hours
    Second Breakfast: 48 hours",
				@"Available Capacity:
    buffer: 48 hours
    Second Breakfast: 48 hours",
				@"Allocated Capacity in buffer: 0 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0 hours

Allocated Capacity in Second Breakfast: 0 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0 hours",
				@"Calculated at: 14-Jul-2015 10:00:00 (buffer)
Calculated at: 14-Jul-2015 10:00:00 (Second Breakfast)");
		}

		protected override void AssertComponentCapacity_InactiveAdditionalComponent_ShouldNotBeIncluded_WhenComponentInActive(ChannelCapacity channelCapacity)
		{
			CapacityCalculatorTestHelper.AssertCapacity("Non-Constrained Mode board section", channelCapacity, "JohnLock :'(",
				"Total Capacity: 48 hours",
				"Available Capacity: 48 hours",
				@"Allocated Capacity in buffer: 0 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0 hours",
				@"Calculated at: 14-Jul-2015 10:00:00");
		}

		#endregion
	}
}
