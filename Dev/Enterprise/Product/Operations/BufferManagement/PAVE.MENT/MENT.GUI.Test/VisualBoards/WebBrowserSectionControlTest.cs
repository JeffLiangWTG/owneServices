using Enterprise.BufferManagement.Business.Test;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI.Test;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	[TestedType(typeof(WebBrowserSectionControl))]
	class WebBrowserSectionControlIBoardSectionControlTest : BoardSectionControlTestCase<WebBrowserSectionControl>
	{
		protected override WebBrowserSectionControl GetControl()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var section = board.Sections.AddNew();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;
			var sectionConfiguration = (WebBrowserSectionConfiguration)section.Configuration;
			sectionConfiguration.URL = "about:blank";

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board, new MENTTestHelper.TestExtractorFactoryProvider());
			var sectionViewModel = new BoardSectionViewModel(section, boardViewModel);

			return new WebBrowserSectionControl(section, sectionViewModel);
		}
	}
}
