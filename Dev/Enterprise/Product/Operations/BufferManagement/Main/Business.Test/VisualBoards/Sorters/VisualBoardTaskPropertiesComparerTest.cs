using System;
using CargoWise.Types;
using Moq;

namespace Enterprise.BufferManagement.Business.Test
{
	public class VisualBoardTaskPropertiesComparerTest : BMSTestCaseWithFactory
	{
		public void TestCompare()
		{
			var now = DateTime.Now;

			CombineAssertions(() =>
			{
				AssertEquals("Compare both Null", 0, Comparer.Compare(null, null));
				AssertEquals("Compare first Null", -1, Comparer.Compare(null, CreateOrderableTask()));
				AssertEquals("Compare second Null", 1, Comparer.Compare(CreateOrderableTask(), null));

				// Inverted because higher nudge comes first
				AssertEquals("Nudge", -1, Comparer.Compare(CreateOrderableTask(nudge: 1), CreateOrderableTask(nudge: 0)));
				AssertEquals("Nudge", 1, Comparer.Compare(CreateOrderableTask(nudge: 0), CreateOrderableTask(nudge: 1)));

				AssertEquals("ReleaseDate", 1, Comparer.Compare(CreateOrderableTask(releaseDate: now.AddHours(1)), CreateOrderableTask(releaseDate: now)));
				AssertEquals("ReleaseDate", -1, Comparer.Compare(CreateOrderableTask(releaseDate: now), CreateOrderableTask(releaseDate: now.AddHours(1))));

				AssertEquals("Sequence", 1, Comparer.Compare(CreateOrderableTask(sequence: 1), CreateOrderableTask(sequence: 0)));
				AssertEquals("Sequence", -1, Comparer.Compare(CreateOrderableTask(sequence: 0), CreateOrderableTask(sequence: 1)));

				AssertEquals("TaskID", 1, Comparer.Compare(CreateOrderableTask(taskID: "B"), CreateOrderableTask(taskID: "A")));
				AssertEquals("TaskID", -1, Comparer.Compare(CreateOrderableTask(taskID: "A"), CreateOrderableTask(taskID: "B")));

				AssertEquals("Compare all equals", 0, Comparer.Compare(CreateOrderableTask(), CreateOrderableTask()));
			});
		}

		#region Implementation

		readonly VisualBoardTaskPropertiesComparer Comparer = new VisualBoardTaskPropertiesComparer();

		ITaskOrderable CreateOrderableTask(decimal nudge = 0, ZDateTime? releaseDate = null, int sequence = 0, string taskID = null)
		{
			var mock = new Mock<ITaskOrderable>();
			mock.Setup(task => task.Nudge).Returns(nudge);
			mock.Setup(task => task.ReleaseDate).Returns(releaseDate ?? ZDateTime.Empty);
			mock.Setup(task => task.Sequence).Returns(sequence);
			mock.Setup(task => task.TaskID).Returns(taskID);
			return mock.Object;
		}
	}

	#endregion
}
