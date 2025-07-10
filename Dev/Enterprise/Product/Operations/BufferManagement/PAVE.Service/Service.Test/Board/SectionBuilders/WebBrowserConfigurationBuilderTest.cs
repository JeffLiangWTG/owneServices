using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.PAVE.MENT.Business;

namespace Enterprise.BufferManagement.Service.Test
{
	class WebBrowserConfigurationBuilderTest : TestCaseWithFactory
	{
		public void TestGetConfiguration_ShouldReturnCorrectWebBrowserSectionConfigurationDTOs()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			section.MS_SectionType = "WEB";
			var sectionConfiguration = (WebBrowserSectionConfiguration)section.Configuration;
			sectionConfiguration.URL = "https://www.google.com.au";
			sectionConfiguration.Validation.ValidateURL();
			Factory.Save();

			var sectionConfigDTOs = new BoardService().GetConfiguration(board.PK.ToGuid()).Sections.ToList();
			var webBrowserSectionConfigurationDTO = sectionConfigDTOs.First() as WebBrowserSectionConfigurationDTO;
			AssertEquals("https://www.google.com.au", webBrowserSectionConfigurationDTO.URL);
		}
	}
}
