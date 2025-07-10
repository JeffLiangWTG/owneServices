using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(NetworkDiagramSectionDescriptor))]
	class NetworkDiagramSectionDescriptorTest : BoardSectionDescriptorTestCase<NetworkDiagramSectionDescriptor>
	{
		protected override string Type
		{
			get { return CCPMConstants.NetworkDiagramSectionType; }
		}

		protected override void TestGetSectionConfigurationBizoCore(IBoardSectionDescriptor descriptor)
		{
			AssertType<DiagramBoardSectionConfiguration>(descriptor.GetSectionConfigurationBizo(Factory.New<IBMBoardSection>()));
		}

		protected override void TestGetSectionConfigurationControlCore(IBoardSectionDescriptor descriptor)
		{
			using (var control = (Control)descriptor.GetSectionConfigurationControl())
			{
				AssertType<BoardSectionDiagramConfigurationControl>(control);
			}
		}

		protected override void TestGetAdditionalTabsCore(IBoardSectionDescriptor descriptor)
		{
			AssertEquals(0, descriptor.GetAdditionalTabs().Count());
		}

		protected override void TestGetSectionControlCore(IBoardSectionDescriptor descriptor)
		{
			var section = NetworkTestCase.CreateNetworkBoardSection(NetworkTestCase.CreateDiagram(Factory));
			var viewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(section.Board));

			using (var control = (Control)descriptor.GetSectionControl(section, viewModel))
			{
				AssertType<BoardSectionDiagramControl>(control);
			}
		}

		protected override void TestGetViewModelCore(IBoardSectionDescriptor descriptor)
		{
			var section = NetworkTestCase.CreateNetworkBoardSection(NetworkTestCase.CreateDiagram(Factory));

			AssertType<BoardSectionViewModel>(descriptor.GetViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(section.Board)));
		}
	}
}
