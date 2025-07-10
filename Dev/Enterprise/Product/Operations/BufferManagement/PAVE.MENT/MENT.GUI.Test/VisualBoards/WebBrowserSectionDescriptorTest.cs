using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.GUI
{
	[TestedType(typeof(WebBrowserSectionDescriptor))]
	class WebBrowserSectionDescriptorTest : BoardSectionDescriptorTestCase<WebBrowserSectionDescriptor>
	{
		protected override string Type
		{
			get { return MENTConstants.WebBrowserSectionType; }
		}

		protected override void TestGetSectionConfigurationBizoCore(IBoardSectionDescriptor descriptor)
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;

			var sectionConfigurationBizo = descriptor.GetSectionConfigurationBizo(section);

			AssertNotNull(sectionConfigurationBizo);
			AssertType<WebBrowserSectionConfiguration>(sectionConfigurationBizo);
		}

		protected override void TestGetSectionConfigurationControlCore(IBoardSectionDescriptor descriptor)
		{
			using (var control = (Control)descriptor.GetSectionConfigurationControl())
			{
				AssertType<WebBrowserSectionConfigurationControl>(control);
			}
		}

		protected override void TestGetAdditionalTabsCore(IBoardSectionDescriptor descriptor)
		{
			var tabs = descriptor.GetAdditionalTabs().ToArray();

			AssertEquals(0, tabs.Length);
		}

		protected override void TestGetSectionControlCore(IBoardSectionDescriptor descriptor)
		{
			var board = Factory.NewWithValidTestData<BMBoard>();
			var section = board.Sections.AddNew();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			using (var control = (IDisposable)descriptor.GetSectionControl(section, viewModel))
			{
				AssertType<WebBrowserSectionControl>(control);
			}
		}

		protected override void TestGetViewModelCore(IBoardSectionDescriptor descriptor)
		{
			var board = Factory.NewWithValidTestData<BMBoard>();
			var section = board.Sections.AddNew();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			AssertType<BoardSectionViewModel>(viewModel);
		}
	}
}
