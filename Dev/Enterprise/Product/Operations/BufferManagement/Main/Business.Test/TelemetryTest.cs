using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Pipes.Test;
using Enterprise.VisualBoards.Business.Telemetry;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test;

public class TelemetryTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestLoadBoardTraces()
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

		var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
		var setup = new LoadCardContentSetup(viewModel);
		var dispatcher = new MockDispatcher();
		var engine = BoardSectionRefreshPipeEngine.Create(new() { TriggeredByUserRefreshingBoardOrSection = true }, setup, Mock.Of<IBoardSectionRefreshable>());
		engine.ExecuteAll(dispatcher, dispatcher);
		dispatcher.DispatchAll();

		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.Execute"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.ChannelViewModelsPipe"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.RefreshHeadings"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.OnUpdateBackgroundsAndFades"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.OnRefreshComponent"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.OnShowLoadingIndicator"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.RemoveLoadingIndicator"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.GetAcceptabilityBandResults"));
		Assert(traces.Any(t => t.OperationName == "BoardSectionRefreshPipeEngine.DisplayAcceptabilityBandResults"));
		Assert(traces.Any(t => t.OperationName == "LoadCardContents.MakeTaskChannelMap"));
		Assert(traces.Any(t => t.OperationName == "LoadCardContents.CreateAllocationMap"));
	}
}
