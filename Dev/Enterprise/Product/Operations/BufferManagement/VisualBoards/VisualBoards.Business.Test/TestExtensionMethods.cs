using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Pipes.Test;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.VisualBoards.Business.Test
{
	public static class TestExtensionMethods
	{
		#region Allocate Tasks

		public static void AllocateTasks_ForTest(this ComponentGrid grid, BMBoardSection section, BMBoardSectionViewModel viewModel, ProcessTask[] tasks)
		{
			var allChannels = viewModel.AllChannels;
			if (!allChannels.Any())
			{
				allChannels = new[] { new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled) };
			}

			var loadCardSetup = new LoadCardContentSetup(viewModel, () => section.Factory);

			var engine = BoardSectionRefreshPipeEngine.Create(new BoardRefreshEventArgs(), loadCardSetup, null, new TaskChannelMap_ForTest(section, allChannels, tasks));

			var dispatcher = new MockDispatcher();
			var executionResultSet = engine.ExecuteAll(dispatcher, dispatcher);
			dispatcher.DispatchAll();
			executionResultSet.AwaitAll(); // to wait all task continuations in EngineExecutionResultSet

			section.Factory.ThreadSentry.TakeThreadOwnership(); // Normally we would not want to touch this factory again, but because we are in a test it is ok.
		}

		#endregion

		class TaskChannelMap_ForTest : TaskChannelMap
		{
			public TaskChannelMap_ForTest(BMBoardSection section, IEnumerable<IVisualBoardChannel> channels, ProcessTask[] tasks)
				: base(section, channels, BoardSectionEntities.ForTest(tasks.Select(t => t.GetProcessHeaderForCardType(section.SectionConfiguration.ShowJobWorkflowCards)).ToArray()), t => tasks.Contains(t))
			{
			}
		}
	}
}
