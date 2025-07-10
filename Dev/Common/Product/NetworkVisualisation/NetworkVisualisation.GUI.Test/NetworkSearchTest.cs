using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class NetworkSearchTest : TestCase
	{
		public void TestSearchConfiguation_ShouldReturnCorrectProperties()
		{
			AssertEquals("Not all default properties are collected. Should return 7 properties", 7, NetworkSearchConfiguration.GetSearchableProperties().Length);
		}

		public void TestFinderViewModel()
		{
			var network = SetupNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new SearchFinderViewModel(networkViewModel);

			viewModel.PerformSearch_ForTest("aa");
			Assert("View model should have performed the search.", viewModel.HasPerformedSearch_ForTest);
			AssertEquals("View model should have found 2 shapes.", 2, viewModel.GetSearchResults_ForTest().Count);
		}

		public void TestShouldResetSearchResultsAfterRefresherEvent()
		{
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity");

			foreach (var refreshEvent in (RefreshType[])Enum.GetValues(typeof(RefreshType)))
			{
				var viewModel = new SearchFinderViewModel(networkViewModel);

				viewModel.PerformSearch_ForTest("Entity");
				Assert("View model should have performed the search", viewModel.HasPerformedSearch_ForTest);

				networkViewModel.Network.Refresh(refreshEvent);
				Assert("View model should reset search results", !viewModel.HasPerformedSearch_ForTest);
			}
		}

		public void TestShouldBeAbleToFindAddedShape()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var networkViewModel = config.Control.NetworkViewModel;

				var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
				AssertEquals(1, networkViewModel.Nodes.Count());
				AssertEquals(false, node1.IsSelected);

				var viewModel = new SearchFinderViewModel(networkViewModel);

				viewModel.PerformSearch_ForTest("Entity");
				Assert("View model should have performed the search", viewModel.HasPerformedSearch_ForTest);
				AssertEquals("View model should have found 1 shape", 1, viewModel.GetSearchResults_ForTest().Count);

				NetworkTestHelper.CreateNewEntityUsingContextMenu(config.Control, "Entity2");
				System.Windows.Forms.Application.DoEvents();
				Assert("View model should reset search results", !viewModel.HasPerformedSearch_ForTest);

				viewModel.PerformSearch_ForTest("Entity");
				Assert("View model should have performed the search", viewModel.HasPerformedSearch_ForTest);
				AssertEquals("View model should have found 2 shapes", 2, viewModel.GetSearchResults_ForTest().Count);
			}
		}

		public void TestShouldBeAbleToFindAddedShapeUsingSearchForm()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var networkViewModel = config.Control.NetworkViewModel;

				var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
				AssertEquals(1, networkViewModel.Nodes.Count());
				AssertEquals(false, node1.IsSelected);

				var viewModel = new SearchFinderViewModel(networkViewModel);
				var searchForm = config.Control.OpenFinderForm();

				searchForm.SetSearchBox_ForTest("Entity");
				searchForm.Search_PerformClick_ForTest();
				AssertEquals(true, node1.IsSelected);

				NetworkTestHelper.CreateNewEntityUsingContextMenu(config.Control, "Entity2");
				System.Windows.Forms.Application.DoEvents();
				Assert("View model should reset search results", !viewModel.HasPerformedSearch_ForTest);
				AssertEquals(2, networkViewModel.Nodes.Count());
				var node2 = networkViewModel.Nodes.Last();

				AssertEquals(true, node1.IsSelected);
				AssertEquals(false, node2.IsSelected);

				searchForm.Search_PerformClick_ForTest();
				CombineAssertions("Should find the newly created shape", () =>
				{
					AssertEquals(false, node1.IsSelected);
					AssertEquals(true, node2.IsSelected);
				});

				config.Control.CloseFinderForm();
			}
		}

		DummyNetwork SetupNetwork()
		{
			var network = new DummyNetwork();
			var shape1 = network.CreateNewEntity(ShapeTypes.Shape);
			shape1.Name = "a";
			var shape2 = network.CreateNewEntity(ShapeTypes.Shape);
			shape2.Name = "aa";
			var shape3 = network.CreateNewEntity(ShapeTypes.Shape);
			shape3.Name = "aaa";

			return network;
		}
	}

#pragma warning disable CS0618
	[TestedType(typeof(SearchFinderForm))]
	public class SearchFinderFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, network.Refresher);
			var searchFinderViewModel = new SearchFinderViewModel(networkViewModel);
			return new SearchFinderForm(networkUserControl);
		}
	}
#pragma warning restore CS0618
}
