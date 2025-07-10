using System;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.VisualBoards.GUI.Test
{
	class VisualBoardSectionsRendererTest : VisualBoardsTestCase
	{
		public void TestRender_ForInvalidSectionType_ShouldDisposeControl()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var board = Factory.New<BMBoard>();
			board.MB_FS_System = system.PK;

			var section1 = CreateBoardSection(bucket, board);
			var section2 = CreateBoardSection(bucket, board);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section1);

			using (DisableAsyncBehaviour())
			{
				AssertExceptionThrown<ArgumentNullException>("Should throw a null ref ex, but also control should get disposed (detected by after test check)",
					() => VisualBoardSectionsRenderer.Render(new[]
					{
						new BoardSectionViewModelPair(section1, viewModel), new BoardSectionViewModelPair(section2, null)
					}));
			}
		}
	}
}
