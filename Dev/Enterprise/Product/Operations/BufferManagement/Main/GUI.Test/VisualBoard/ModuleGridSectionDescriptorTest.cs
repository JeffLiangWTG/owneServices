using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(ModuleGridSectionDescriptor))]
	class ModuleGridSectionDescriptorTest : BoardSectionDescriptorTestCase<ModuleGridSectionDescriptor>
	{
		protected override string Type
		{
			get { return BMConstants.ModuleGridSectionType; }
		}

		protected override void TestGetSectionConfigurationBizoCore(IBoardSectionDescriptor descriptor)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var sectionConfiguration = descriptor.GetSectionConfigurationBizo(section);
			AssertType<ModuleGridSectionConfiguration>(sectionConfiguration);
		}

		protected override void TestGetSectionConfigurationControlCore(IBoardSectionDescriptor descriptor)
		{
			using (var control = (Control)descriptor.GetSectionConfigurationControl())
			{
				AssertType<ModuleGridConfigurationControl>(control);
			}
		}

		protected override void TestGetSectionControlCore(IBoardSectionDescriptor descriptor)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_SectionType = Type;

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			using (var control = (IDisposable)descriptor.GetSectionControl(section, viewModel))
			{
				AssertType<ModuleSelectorControl>(control);
			}
		}

		protected override void TestGetViewModelCore(IBoardSectionDescriptor descriptor)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			AssertType<BoardSectionViewModel>(viewModel);
		}

		protected override void TestGetAdditionalTabsCore(IBoardSectionDescriptor descriptor)
		{
			AssertEquals(0, descriptor.GetAdditionalTabs().Count());
		}
	}
}
