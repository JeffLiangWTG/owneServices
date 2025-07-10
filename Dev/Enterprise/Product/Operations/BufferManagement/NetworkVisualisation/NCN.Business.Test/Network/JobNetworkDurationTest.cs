using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestDate(2019, 1, 1)]
	class JobNetworkDurationTest : NetworkTestCase
	{
		public void TestDiagramDuration_NoDatesEntered()
		{
			AssertEquals(ZDateTime.Empty, diagram.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.Empty, diagram.ScheduledFinishTimeUtc);

			AssertEquals("Diagram duration should be the diagram surface width containing all shapes starting from the leftmost edge of the diagram (including gap at the start of the diagram).", UnRestrictedDiagramDuration, diagram.ExplicitDurationMinutes);
		}

		public void TestDiagramDuration_ScheduledStartEntered()
		{
			SetScheduleTimes(startTimeDaysFromToday: -1, finishTimeDaysFromToday: null);

			AssertEquals(ZDateTime.UtcNow.AddDays(-1), diagram.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.Empty, diagram.ScheduledFinishTimeUtc);

			AssertEquals("Diagram duration should be the diagram surface width containing all shapes starting from the leftmost edge of the diagram (including gap at the start of the diagram).", UnRestrictedDiagramDuration, diagram.ExplicitDurationMinutes);
		}

		public void TestDiagramDuration_ScheduledFinishEntered()
		{
			SetScheduleTimes(startTimeDaysFromToday: null, finishTimeDaysFromToday: 20);

			AssertEquals(ZDateTime.Empty, diagram.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(20), diagram.ScheduledFinishTimeUtc);

			AssertEquals("Diagram duration should be the diagram surface width containing all shapes starting from the leftmost edge of the diagram (including gap at the start of the diagram).", UnRestrictedDiagramDuration, diagram.ExplicitDurationMinutes);
		}

		public void TestDiagramDuration_BothScheduledStartAndFinishEntered()
		{
			SetScheduleTimes(startTimeDaysFromToday: -1, finishTimeDaysFromToday: 20);

			AssertEquals(ZDateTime.UtcNow.AddDays(-1), diagram.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(20), diagram.ScheduledFinishTimeUtc);

			AssertEquals("Diagram duration should be the amount of time between start and end time of the diagram.", 21 * BMConstants.WorkingHoursPerDay * 60, diagram.ExplicitDurationMinutes);
		}

		public void TestEmptyDiagramSurface_BothScheduledStartAndFinishEntered()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);

			network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddHours(2);
			diagram.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddMonths(3);
			network.RefreshSchedules();

			AssertEquals("Diagram duration should be the amount of time between start and end time of the diagram.", 43080, diagram.ExplicitDurationMinutes);
			AssertEquals("Diagram Width", 8975.0, network.DiagramEntity.Width);
		}

		#region Implementation

		BMNCNShape diagram;
		BMNCNShape shape1, shape2, shape3, shape4;

		JobNetwork network;

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			diagram = NetworkTestCase.CreateDiagram(Factory);

			shape1 = NetworkTestCase.CreateShape(diagram, "shape1");
			shape2 = NetworkTestCase.CreateShape(diagram, "shape2");
			shape3 = NetworkTestCase.CreateShape(diagram, "shape3");
			shape4 = NetworkTestCase.CreateShape(diagram, "shape4");

			network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			// Position shapes in a line, with a gap at the start of the diagram.
			network.Entities.GetInstance(shape1).SetCoordinates(300, 100, 100, 0);
			network.Entities.GetInstance(shape2).SetCoordinates(300, 100, 400, 0);
			network.Entities.GetInstance(shape3).SetCoordinates(300, 100, 700, 0);

			// This shape overlaps other shapes, so shouldn't affect the diagram's duration.
			network.Entities.GetInstance(shape4).SetCoordinates(300, 100, 500, 200);

			network.RefreshSchedules();
			Factory.Save();
		}

		void SetScheduleTimes(int? startTimeDaysFromToday, int? finishTimeDaysFromToday)
		{
			if (startTimeDaysFromToday != null)
			{
				diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(startTimeDaysFromToday.Value);
			}

			if (finishTimeDaysFromToday != null)
			{
				diagram.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(finishTimeDaysFromToday.Value);
			}

			network.RefreshSchedules();
			Factory.Save();
		}

		const int UnRestrictedDiagramDuration = 10 * BMConstants.WorkingHoursPerDay * 60; // 10 time slots of one working day per slot.

		#endregion
	}
}
