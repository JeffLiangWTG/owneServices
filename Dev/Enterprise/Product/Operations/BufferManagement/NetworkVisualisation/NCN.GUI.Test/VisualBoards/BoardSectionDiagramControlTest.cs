using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class BoardSectionDiagramControlTest : NetworkTestCase
	{
		public void TestConstructorForPreview_ShouldCreatePlaceholderLabel()
		{
			using (Globals.SetIsWinzorForTest(false))
			{
				var diagram = CreateDiagram(Factory);
				var section = CreateNetworkBoardSection(diagram);
				var viewModel = CreateBoardSectionViewModel(section, isPreview: true);

				using (var control = new BoardSectionDiagramControl(section, viewModel))
				{
					AssertEquals(1, control.Controls.Count);
					var label = (ZLabel)control.Controls[0];
					AssertEquals("Diagram cannot be shown in preview", label.Text);
					AssertNull(control.Network);
				}
			}
		}

		public void TestConstructorForWinzor_ShouldCreateErrorLabel()
		{
			using (Globals.SetIsWinzorForTest(true))
			{
				var diagram = CreateDiagram(Factory);
				var section = CreateNetworkBoardSection(diagram);
				var viewModel = CreateBoardSectionViewModel(section, isPreview: true);

				using (var control = new BoardSectionDiagramControl(section, viewModel))
				{
					AssertEquals(1, control.Controls.Count);
					var label = (ZLabel)control.Controls[0];
					AssertEquals("Network Diagram section on a Visual Board is not supported in CargoWise Experimental Web Version.", label.Text);
					AssertNull(control.Network);
				}
			}
		}

		public void TestConstructorForNonPreview_ShouldSetupDiagram()
		{
			using (Globals.SetIsWinzorForTest(false))
			{
				var diagram = CreateDiagram(Factory, name: "diagram");
				var section = CreateNetworkBoardSection(diagram);
				var viewModel = CreateBoardSectionViewModel(section, isPreview: true);

				Factory.Save();

				using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
				{
					var networkControl = form.FindAll<BoardSectionDiagramControl>().Single();
					AssertNotNull(networkControl.Network);
					AssertEquals(0, networkControl.Network.Entities.Count);

					CreateShape(diagram);
					Factory.Save();

					AssertEquals("The diagram should be updated automatically via data refresh bus", 1, networkControl.Network.Entities.Count);
				}
			}
		}

		public void TestReload_ShouldSwitchDiagram()
		{
			using (Globals.SetIsWinzorForTest(false))
			{
				var diagram1 = CreateDiagram(Factory, name: "diagram1");
				var diagram2 = CreateDiagram(Factory, name: "diagram2");
				var section = CreateNetworkBoardSection(diagram1);
				var viewModel = CreateBoardSectionViewModel(section, isPreview: true);

				Factory.Save();

				using (var form = new VisualBoardForm(VisualBoardsTestHelper.CreateSlideshowViewModel(section.Board)))
				{
					form.Show();

					var networkControl = form.FindAll<BoardSectionDiagramControl>().Single();
					AssertNotNull(networkControl.Network);
					AssertEquals(diagram1.PK, networkControl.Network.DiagramEntity.PK);

					((DiagramBoardSectionConfiguration)section.Configuration).DiagramPK = diagram2.PK;
					Factory.Save();

					form.RefreshNow_ForTest(forceReload: true);
					AssertEquals(true, networkControl.IsDisposed);

					networkControl = form.FindAll<BoardSectionDiagramControl>().Single();
					AssertEquals(diagram2.PK, networkControl.Network.DiagramEntity.PK);
				}
			}
		}

		public void TestShouldNotShowRibbon()
		{
			using (Globals.SetIsWinzorForTest(false))
			{
				var diagram1 = CreateDiagram(Factory, name: "diagram1");
				var section = CreateNetworkBoardSection(diagram1);
				var viewModel = CreateBoardSectionViewModel(section, isPreview: true);

				Factory.Save();

				using (var form = new VisualBoardForm(VisualBoardsTestHelper.CreateSlideshowViewModel(section.Board)))
				{
					form.Show();
					Application.DoEvents();

					var sectionDiagramControl = form.FindAll<BoardSectionDiagramControl>().Single();
					var networkUserControl = sectionDiagramControl.NetworkUserControl;
					AssertNotNull(networkUserControl);
					AssertEquals("Ribbon should not be visible", false, networkUserControl.RibbonControlIsVisible_ForTesting);
				}
			}
		}

		protected override bool ShouldDisableAsyncBehaviour => true;
	}

	[TestedType(typeof(BoardSectionDiagramControl))]
	class BoardSectionDiagramControlIBoardSectionControlTest : BoardSectionControlTestCase<BoardSectionDiagramControl>
	{
		protected override BoardSectionDiagramControl GetControl()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var section = NetworkTestCase.CreateNetworkBoardSection(diagram);
			var viewModel = VisualBoardsTestCase.CreateBoardSectionViewModel(section, isPreview: true);

			return new BoardSectionDiagramControl(section, viewModel);
		}
	}
}
