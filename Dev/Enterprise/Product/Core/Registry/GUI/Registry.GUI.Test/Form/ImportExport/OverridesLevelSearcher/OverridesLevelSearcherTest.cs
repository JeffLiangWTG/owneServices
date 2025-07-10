using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Forms.Internal;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class OverridesLevelSearcherTest : TestCase
	{
		[GuiTest]
		public void TestPreviousResult()
		{
			using (var form = new Form())
			using (var treeView = new TreeView())
			{
				form.Controls.Add(treeView);
				form.Show();
				var testSearcher = CreateTreeViewSearcherForTest(treeView);

				var results = testSearcher.Search(treeView, "Branches", StringComparison.CurrentCulture);

				AssertEquals("PRE: Incorrect number of results", 2, results.Total);

				results.Previous();
				AssertEquals("Index not decremented as expected", 1, ((NonRecursiveSearchResults)results).Index);
			}
		}

		[GuiTest]
		public void TestNextResult()
		{
			using (var form = new Form())
			using (var treeView = new TreeView())
			{
				form.Controls.Add(treeView);
				form.Show();
				var testSearcher = CreateTreeViewSearcherForTest(treeView);

				var results = testSearcher.Search(treeView, "Branches", StringComparison.CurrentCulture);

				AssertEquals("PRE: Incorrect number of results", 2, results.Total);

				var currNode = results.Next();
				AssertEquals("Index not decremented as expected", 0, ((NonRecursiveSearchResults)results).Index);
			}
		}

		[GuiTest]
		public void TestSearch()
		{
			using (var form = new Form())
			using (var treeViewExpected = new TreeView())
			using (var treeViewActual = new TreeView())
			{
				form.Controls.Add(treeViewActual);
				form.Show();
				var treeViewSearcher = CreateTreeViewSearcherForTest(treeViewActual);

				var resultsActual = treeViewSearcher.Search(treeViewActual, "A", StringComparison.CurrentCulture);
				var actualNodeNameList = GetAllSearchResults(resultsActual);

				treeViewActual.ExpandAll();

				RegistryItemTreeViewBuilder.AddFallbackNodes(treeViewExpected.Nodes, new BusinessObjectFactory());
				var nonRecursiveSearcher = new NonRecursiveTreeViewSearcher();
				var resultExpected = nonRecursiveSearcher.Search(treeViewExpected, "A", StringComparison.CurrentCulture);
				var expectedNodeNameList = GetAllSearchResults(resultExpected);

				AssertEquals("Incorrect number of results", resultExpected.Total, resultsActual.Total);

				AssertArrayEqualsByElements("Search results should be same", expectedNodeNameList.ToArray(), actualNodeNameList.ToArray());
			}
		}

		[GuiTest]
		public void TestSearchResult()
		{
			using (var form = new Form())
			using (var treeViewExpected = new TreeView())
			using (var treeViewActual = new TreeView())
			{
				form.Controls.Add(treeViewActual);
				form.Show();
				var treeViewSearcher = CreateTreeViewSearcherForTest(treeViewActual);

				var resultsActual = treeViewSearcher.Search(treeViewActual, "A", StringComparison.CurrentCulture);
				var actualNodeNameList = GetAllSearchResults(resultsActual);

				treeViewActual.ExpandAll();

				RegistryItemTreeViewBuilder.AddFallbackNodes(treeViewExpected.Nodes, new BusinessObjectFactory());
				var nonRecursiveSearcher = new NonRecursiveTreeViewSearcher();
				var resultExpected = nonRecursiveSearcher.Search(treeViewExpected, "A", StringComparison.CurrentCulture);
				var expectedNodeNameList = GetAllSearchResults(resultExpected);

				AssertEquals("Incorrect number of results", resultExpected.Total, resultsActual.Total);

				CombineAssertions(() =>
				{
					for (var i = 0; i < resultExpected.Total; i++)
					{
						AssertEquals(resultExpected.Next().FullPath, resultsActual.Next().FullPath);
						AssertEquals(resultExpected.Next().Index, resultsActual.Next().Index);
					}

					for (var i = 0; i < resultExpected.Total; i++)
					{
						AssertEquals(resultExpected.Previous().FullPath, resultsActual.Previous().FullPath);
						AssertEquals(resultExpected.Previous().Index, resultsActual.Previous().Index);
					}
				});
			}
		}

		List<string> GetAllSearchResults(ISearchResults results)
		{
			var nodeNameList = new List<string>();

			for (var i = 0; i < results.Total; i++)
			{
				nodeNameList.Add(results.Next().Text);
			}
			return nodeNameList;
		}

		OverridesLevelSearcher CreateTreeViewSearcherForTest(TreeView treeView)
		{
			var factory = new BusinessObjectFactory();
			var overrideLevels = RegistryItemTreeViewBuilder.AddFallbackNodes(treeView, factory);
			return new OverridesLevelSearcher(overrideLevels);
		}
	}
}
