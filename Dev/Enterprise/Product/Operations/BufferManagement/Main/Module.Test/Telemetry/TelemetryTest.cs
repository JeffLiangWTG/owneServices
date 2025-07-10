using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business.Telemetry;
using Enterprise.VisualBoards.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(VisualBoardController))]
	internal class TelemetryTest : VisualBoardControllerTest
	{
		public void TestVisualBoardControllerTrace()
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

			Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
			Application.OpenForms.OfType<VisualBoardForm>().FirstOrDefault().Dispose();
			Assert(traces.Any(t => t.OperationName == "VisualBoardController.ShowEditForm"));
		}
	}
}
