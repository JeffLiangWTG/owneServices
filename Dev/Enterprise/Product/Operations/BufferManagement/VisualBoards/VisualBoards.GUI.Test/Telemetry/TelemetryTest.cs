using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.Business.Telemetry;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestedType(typeof(VisualBoardFormDisplayer))]
	public class TelemetryTest : NonTransactionedTestCase
	{
		public void TestShouldAbleToTraceVisualBoardFormDisplayer()
		{
			var traces = new List<Activity>();
			using var listener = new ActivityListener
			{
				ShouldListenTo = source => source.Name == TelemetryService.SourceName,
				Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
				ActivityStarted = activity => traces.Add(activity),
				ActivityStopped = activity => { }
			};
			ActivitySource.AddActivityListener(listener);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);

			Factory.Save();

			var form = VisualBoardFormDisplayer.ShowBoard(board);
			form.Dispose();

			Assert(traces.Any(t => t.OperationName == "VisualBoardFormDisplayer.ShowBoard"));
			Assert(traces.Any(t => t.OperationName == "VisualBoardFormDisplayerStrategy.OpenNewForm"));
			Assert(traces.Any(t => t.OperationName == "VisualBoardFormDisplayerStrategy.OpenForm"));
			Assert(traces.Any(t => t.OperationName == "VisualBoardForm.RefreshBoard"));
		}
	}
}
