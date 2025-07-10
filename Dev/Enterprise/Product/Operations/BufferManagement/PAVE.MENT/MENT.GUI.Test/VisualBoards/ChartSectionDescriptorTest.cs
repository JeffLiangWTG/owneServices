using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	[TestedType(typeof(ChartSectionDescriptor))]
	class ChartSectionDescriptorTest : BoardSectionDescriptorTestCase<ChartSectionDescriptor>
	{
		protected override string Type
		{
			get { return MENTConstants.ChartSectionType; }
		}

		protected override void TestGetSectionConfigurationBizoCore(IBoardSectionDescriptor descriptor)
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			section.MS_SectionType = MENTConstants.ChartSectionType;

			var sectionConfigurationBizo = descriptor.GetSectionConfigurationBizo(section);

			AssertNotNull(sectionConfigurationBizo);
			AssertType<ChartSectionConfiguration>(sectionConfigurationBizo);
		}

		protected override void TestGetSectionConfigurationControlCore(IBoardSectionDescriptor descriptor)
		{
			using (var control = (Control)descriptor.GetSectionConfigurationControl())
			{
				AssertType<ChartSectionConfigurationControl>(control);
			}
		}

		protected override void TestGetAdditionalTabsCore(IBoardSectionDescriptor descriptor)
		{
			AssertEquals(0, descriptor.GetAdditionalTabs().Count());
		}

		protected override void TestGetSectionControlCore(IBoardSectionDescriptor descriptor)
		{
			var board = Factory.NewWithValidTestData<BMBoard>();
			var section = board.Sections.AddNew();
			section.MS_SectionType = MENTConstants.ChartSectionType;

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			using (var control = (IDisposable)descriptor.GetSectionControl(section, viewModel))
			{
				AssertType<ChartSectionControl>(control);
			}
		}

		protected override void TestGetViewModelCore(IBoardSectionDescriptor descriptor)
		{
			var board = Factory.NewWithValidTestData<BMBoard>();
			var section = board.Sections.AddNew();
			section.MS_SectionType = MENTConstants.ChartSectionType;

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			AssertType<BoardSectionViewModel>(viewModel);
		}
	}
}
