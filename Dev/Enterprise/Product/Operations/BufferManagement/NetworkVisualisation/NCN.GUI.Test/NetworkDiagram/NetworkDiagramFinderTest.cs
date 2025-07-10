using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class NetworkDiagramFinderTest : NetworkTestCase
	{
		#region Tests
		public void TestFinder_FindName()
		{
			SetCustomProperty(textForProperty: "namie", property: "Name");
			AssertSearchResultCount(textToFind: "mi", expectedCount: 1);
		}

		public void TestFinder_MultipleShapesInExpectedOrder()
		{
			var shape1 = SetCustomProperty(textForProperty: "jess", property: "Name", locationX: 0, locationY: 0);
			var shape2 = SetCustomProperty("bess", "Name", 300, 0);
			var shape3 = SetCustomProperty("Tess", "JobName", 600, 0);
			var shape4 = SetCustomProperty("nESs", "Notes", 200, 0);
			var shape5 = SetCustomProperty("essY", "Name", 400, 200);
			var shape6 = SetCustomProperty("bird", "Name", 500, 0);
			var shape7 = SetCustomProperty("essay", "Name", 400, 100);

			var finder = AssertSearchResultCount(textToFind: "ess", expectedCount: 6);
			var expectedList = new List<string> { shape1.Name, shape4.Name, shape2.Name, shape7.Name, shape5.Name, shape3.Name };
			var actualList = new List<string>();
			finder.GetSearchResults_ForTest().ForEach(x => actualList.Add(new NodeViewModel(x, networkViewModel).Name));
			AssertArrayEqualsByElements(expectedList.ToArray(), actualList.ToArray());
		}

		public void TestFinder_ToggleButtonContent()
		{
			SetCustomProperty(textForProperty: "aa", property: "JobName");

			using (var form = GetSearchFinderFormForTest(networkUserControl))
			{
				form.Show();
				AssertEquals("Button should read 'Search'", "Search", form.GetSearchButtonContent_ForTest());
				form.SetSearchBox_ForTest("a");
				AssertEquals("Button should read 'Search'", "Search", form.GetSearchButtonContent_ForTest());
				form.Search_PerformClick_ForTest();

				AssertEquals("Button should read 'Next Result'", "Next Result", form.GetSearchButtonContent_ForTest());
				form.SetSearchBox_ForTest("b");
				AssertEquals("Button should read 'Search'", "Search", form.GetSearchButtonContent_ForTest());
			}
		}

		public void TestSearchButtonShouldSwitchToSearchAfterNetworkRefresh()
		{
			SetCustomProperty(textForProperty: "aa", property: "JobName");

			var someShape = networkViewModel.CreateNewShape(network.DiagramShape);
			networkViewModel.Refresh();
			var entitiesForResfreshEvent = new INetworkEntity[] { someShape };

			using (var form = GetSearchFinderFormForTest(networkUserControl))
			{
				form.Show();
				AssertEquals("Button should read 'Search'", "Search", form.GetSearchButtonContent_ForTest());

				foreach (var refreshEvent in ((RefreshType[])Enum.GetValues(typeof(RefreshType))).Except(new RefreshType[] { RefreshType.None }))
				{
					form.SetSearchBox_ForTest("a");
					form.Search_PerformClick_ForTest();
					AssertEquals("Button should read 'Next Result'", "Next Result", form.GetSearchButtonContent_ForTest());

					network.Refresh(refreshEvent, entitiesForResfreshEvent);
					AssertEquals("Button should read 'Search'", "Search", form.GetSearchButtonContent_ForTest());
				}
			}
		}

		public void TestFinder_FindJobName()
		{
			SetCustomProperty(textForProperty: "this is the job", property: "JobName");
			AssertSearchResultCount(textToFind: "job", expectedCount: 1);
		}

		public void TestFinder_FindNotes()
		{
			SetCustomProperty(textForProperty: "memo", property: "Notes");
			AssertSearchResultCount(textToFind: "emo", expectedCount: 1);
		}

		public void TestFinder_FindCompletionCriteria()
		{
			SetCustomProperty(textForProperty: "Completor", property: "CompletionCriteria");
			AssertSearchResultCount(textToFind: "completor", expectedCount: 1);
		}

		public void TestEmptyStringSearch_DisplayNotificationMessage()
		{
			using (var form = GetSearchFinderFormForTest(networkUserControl))
			{
				form.Show();
				form.Search_PerformClick_ForTest();
				AssertEquals("Should display notification asking user to enter text", "Please enter a search term.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoResults_DisplayNotificationMessage()
		{
			using (var form = GetSearchFinderFormForTest(networkUserControl))
			{
				form.Show();
				form.SetSearchBox_ForTest("text not here");
				form.Search_PerformClick_ForTest();
				AssertEquals("Should display notification informing user that there are no results found", "No results were found.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoResultsLeft_ShouldGoBackToBeginningOfResults()
		{
			var shape1 = SetCustomProperty(textForProperty: "cat", property: "Name");
			var shape2 = SetCustomProperty("hat", "Name");

			using (var form = GetSearchFinderFormForTest(networkUserControl))
			{
				form.Show();

				form.SetSearchBox_ForTest("at");
				form.Search_PerformClick_ForTest();
				form.Search_PerformClick_ForTest();
				form.Search_PerformClick_ForTest();
				AssertEquals("Should continuously cycle though results.", "cat", networkViewModel.FirstSelectedEntity.Name);
			}
		}

		public void TestChangeSearchTextDuringSearch()
		{
			var shape1 = SetCustomProperty(textForProperty: "cat", property: "Name", locationX: 0, locationY: 0);
			var shape2 = SetCustomProperty("hat", "Name", 50, 50);
			var shape3 = SetCustomProperty("bat", "Name", 100, 100);
			var shape4 = SetCustomProperty("sat", "Name", 150, 150);
			var shape5 = SetCustomProperty("mat", "Name", 200, 200);

			using (var form = GetSearchFinderFormForTest(networkUserControl))
			{
				form.Show();
				System.Windows.Forms.Application.DoEvents();

				form.SetSearchBox_ForTest("at");
				form.Search_PerformClick_ForTest();
				AssertEquals("Should select the first match", shape1.Name, networkViewModel.FirstSelectedEntity.Name);
				form.Search_PerformClick_ForTest();
				AssertEquals("Should select the second match", shape2.Name, networkViewModel.FirstSelectedEntity.Name);
				form.SetSearchBox_ForTest("mat");
				form.Search_PerformClick_ForTest();
				AssertEquals("Should begin a new search.", shape5.Name, networkViewModel.FirstSelectedEntity.Name);
			}
		}
		#endregion

		#region Test Setup

		BMNetworkViewModel viewModel;
		NetworkViewModel networkViewModel;
#pragma warning disable CS0618 // Type or member is obsolete
		NetworkUserControl networkUserControl;
#pragma warning restore CS0618 // Type or member is obsolete
		IJobNetwork network;

		protected override void SetUp()
		{
			base.SetUp();

			var parent = Factory.New<BMNCNShape>();
			viewModel = new BMNetworkViewModel(parent);
			network = CreateNetworkViewModel(CreateDiagram(Factory), viewModel).GetJobNetwork();

#pragma warning disable CS0618 // Type or member is obsolete
			networkUserControl = new NetworkUserControl(network.DiagramEntity, network.Refresher);
#pragma warning restore CS0618 // Type or member is obsolete
			networkUserControl.SetDataContext(network, isReloading: false);

			networkViewModel = networkUserControl.NetworkViewModel;

			Factory.Save();
		}

		protected override void TearDown()
		{
			networkUserControl.Dispose();

			base.TearDown();
		}

		ShapeNetworkEntity GetShapeForTest()
		{
			var shape = networkViewModel.CreateNewShape(viewModel.DiagramShape);

			return shape;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		SearchFinderForm GetSearchFinderFormForTest(NetworkUserControl networkUserControl)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return new SearchFinderForm(networkUserControl);
		}

		public NodeViewModel SetCustomProperty(string textForProperty, string property, int locationX = 0, int locationY = 0)
		{
			var node = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, GetShapeForTest());
			node.GetType().GetProperty(property).SetValue(node, textForProperty);
			node.X = locationX;
			node.Y = locationY;
			return node;
		}

		public SearchFinderViewModel AssertSearchResultCount(string textToFind, int expectedCount)
		{
			var networkViewModel = new NetworkViewModel(network);
			var shapeFinder = new SearchFinderViewModel(networkViewModel);
			shapeFinder.PerformSearch_ForTest("do not find this");
			AssertNull("Text should not have been found.", shapeFinder.GetSearchResults_ForTest());

			shapeFinder = new SearchFinderViewModel(networkViewModel);
			shapeFinder.PerformSearch_ForTest(new Regex(textToFind, RegexOptions.IgnoreCase).ToString());
			AssertEquals("Number of found shapes is not as expected.", expectedCount, shapeFinder.GetSearchResults_ForTest().Count);
			return shapeFinder;
		}
		#endregion
	}
}
