using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	sealed class RegistryItemCheckboxTreeTest : TestCaseWithFactory
	{
		public void TestGetCompanies()
		{
			// Use current company's PK as demo company PK for testing only
			// because it's the only PK in the database we can't delete while Enterprise is running

			var demoCompanyGuid = Env.CurrentCompany.PK;
			var currentCompanyPk = Env.CurrentCompany.Country.PK;
			using (RegistryItemTreeViewBuilder.SetDemoCompanyGuid(demoCompanyGuid))
			{
				var companies = new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, demoCompanyGuid));
				companies.DeleteAll();

				var obtainedCollection = RegistryItemTreeViewBuilder.GetCompanies(Factory, false);
				AssertEquals("Only one GlbCompany should be in the collection", 1, obtainedCollection.Count);
				AssertEquals("Should be the Demo Company", demoCompanyGuid, obtainedCollection[0].PK);

				var company = Factory.NewWithValidTestData<GlbCompany>();

				obtainedCollection = RegistryItemTreeViewBuilder.GetCompanies(Factory, false);
				AssertEquals("Only one GlbCompany should be in the collection", 1, obtainedCollection.Count);
				AssertEquals("Should be the newly created Company", company.PK, obtainedCollection[0].PK);
			}
		}

		public void TestGetCompaniesFetchHint()
		{
			CreateCompaniesAndBranches();
			Factory.ResetDatabaseLoadCount();
			var companies = RegistryItemTreeViewBuilder.GetCompanies(Factory, false);

			foreach (var company in companies)
			{
				var branches = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, company.PK));
				branches.Load();
			}

			AssertDbHits(new Dictionary<string, int>
			{
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
			}, Factory);
		}

		public void TestGetCompaniesSorted_ShowCodeAtCompanyAndBranchName()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CDE";
			company1.GC_Name = "Abc";
			company1.Factory.Save();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "ABC";
			company2.GC_Name = "Bcd";
			company2.Factory.Save();

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "BBB";
			company3.GC_Name = "Cde";
			company3.Factory.Save();

			var company4 = Factory.NewWithValidTestData<GlbCompany>();
			company4.GC_Code = "AAB";
			company4.GC_Name = "Def";
			company4.Factory.Save();

			Env.Registry.ShowCodeAtCompanyAndBranchName = true;
			var companies = RegistryItemTreeViewBuilder.GetCompanies(Factory, false);

			AssertEquals(company4.GC_Code, companies[0].GC_Code);
			AssertEquals(company2.GC_Code, companies[1].GC_Code);
			AssertEquals(company3.GC_Code, companies[2].GC_Code);
			AssertEquals(company1.GC_Code, companies[3].GC_Code);

			Env.Registry.ShowCodeAtCompanyAndBranchName = false;
			companies = RegistryItemTreeViewBuilder.GetCompanies(Factory, false);

			AssertEquals(company1.GC_Code, companies[0].GC_Code);
			AssertEquals(company2.GC_Code, companies[1].GC_Code);
			AssertEquals(company3.GC_Code, companies[2].GC_Code);
			AssertEquals(company4.GC_Code, companies[3].GC_Code);
		}

		public void TestHideInactiveFallbacks()
		{
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			department.GE_IsActive = false;
			Factory.Save();

			var root = new TreeNode("Root");
			RegistryItemTreeViewBuilder.AddFallbackNodes(root.Nodes, Factory, hideInactiveFallbacks: true);

			var allItems = Flatten(root);
			Assert("We should not see the inactive department", allItems.All(node => node.Text != department.GE_Desc));
		}

		IEnumerable<TreeNode> Flatten(TreeNode node)
		{
			return new[] { node }.Concat(node.Nodes.Cast<TreeNode>().SelectMany(Flatten));
		}

		[GuiTest]
		public void TestAddFallbackNodesAfterExpanding()
		{
			var treeViewExpected = new TreeView();
			RegistryItemTreeViewBuilder.AddFallbackNodes(treeViewExpected.Nodes, Factory, hideInactiveFallbacks: true);

			var expectedItems = Flatten(treeViewExpected.Nodes);

			var treeViewActual = new TreeView();
			RegistryItemTreeViewBuilder.AddFallbackNodes(treeViewActual, Factory, hideInactiveFallbacks: true);
			var actualItems = Flatten(treeViewActual.Nodes);
			AssertLessThan("We should only load the tree nodes whose parents are expanded.", actualItems.Count(), expectedItems.Count());

			using (var form = new Form())
			{
				form.Controls.Add(treeViewActual);
				form.Show();
				treeViewActual.ExpandAll();
			}

			actualItems = Flatten(treeViewActual.Nodes);
			AssertEquals("All nodes should be loaded after all parent nodes are expanded", expectedItems.Count(), actualItems.Count());
		}

		IEnumerable<TreeNode> Flatten(TreeNodeCollection nodes)
		{
			return nodes.OfType<TreeNode>().SelectMany(Flatten);
		}

		public void TestGetCheckedChildren()
		{
			var root = new TreeNode("Root");

			var firstCheckedItem = CreateItem(root, "A/B");
			var secondCheckedItem = CreateItem(root, "A/A/A/A/A/A/A");
			var uncheckedItem = CreateItem(root, "C/D/E/F");

			firstCheckedItem.Checked = true;
			secondCheckedItem.Checked = true;
			uncheckedItem.Checked = false;

			AssertArrayEqualsByElements("Should only get the checked items", new[] { firstCheckedItem.Tag, secondCheckedItem.Tag }, RegistryItemTreeViewBuilder.GetRegistryItems(root.Nodes, true).ToArray());
			AssertArrayEqualsByElements("Should get all items", new[] { firstCheckedItem.Tag, secondCheckedItem.Tag, uncheckedItem.Tag }, RegistryItemTreeViewBuilder.GetRegistryItems(root.Nodes, false).ToArray());
		}

		public void TestGetLeafNodes()
		{
			var root = new TreeNode("Root");

			var firstCheckedItem = CreateItem(root, "A/B");
			var secondCheckedItem = CreateItem(root, "A/A/A/A/A/A/A");
			var uncheckedItem = CreateItem(root, "C/D/E/F");

			firstCheckedItem.Checked = true;
			secondCheckedItem.Checked = true;
			uncheckedItem.Checked = false;

			AssertArrayEqualsByElements("Should only get the checked items", new[] { firstCheckedItem, secondCheckedItem }, RegistryItemTreeViewBuilder.GetLeafRegistryItemNodes(root.Nodes, true).ToArray());
			AssertArrayEqualsByElements("Should get all items", new[] { firstCheckedItem, secondCheckedItem, uncheckedItem }, RegistryItemTreeViewBuilder.GetLeafRegistryItemNodes(root.Nodes, false).ToArray());
		}

		TreeNode CreateItem(TreeNode root, string path)
		{
			var node = path.Split('/').Aggregate(root, (parent, name) => parent.Nodes.Add(name));
			node.Tag = NewItem(node.Name, path);

			return node;
		}

		public void TestIsSelectable()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Bananas In Pyjamas";
			company.GC_Code = "BIP";

			var b1 = company.Branches.AddNew();
			b1.GB_BranchName = "B1";
			b1.GB_Code = "B1";

			Factory.Save();

			var root = new TreeNode();
			RegistryItemTreeViewBuilder.AddFallbackNodes(root.Nodes, Factory);

			AssertSelectable(root, "Default");
			AssertSelectable(root, "System");
			AssertSelectable(root, "Companies", expectedIsSelectable: false);
			AssertSelectable(root, "Companies/Bananas In Pyjamas");
			AssertSelectable(root, "Companies/Bananas In Pyjamas/Branches", expectedIsSelectable: false);
			AssertSelectable(root, "Companies/Bananas In Pyjamas/Branches/B1");
		}

		void AssertSelectable(TreeNode root, string path, bool expectedIsSelectable = true)
		{
			TreeViewAssertion.AssertHasPath(root, path);

			var endNode = path.Split('/').Aggregate(root, (node, childName) => node.Nodes[childName]);
			AssertEquals(path, expectedIsSelectable, RegistryItemTreeViewBuilder.IsSelectable(endNode));
		}

		IRegistryItem NewItem(string name, string category, RegistryStorageFlags storage = RegistryStorageFlags.All)
		{
			return new StringRegistryItem(name, (NoResString)category, (NoResString)"", (NoResString)"", storage);
		}

		static List<T> ListOf<T>(params T[] items)
		{
			return new List<T>(items);
		}

		public void TestBuildTree_NodeDepth()
		{
			var first = NewItem("First", "Base/Sub1/Sub2");
			var root = (new TreeNode()).Nodes;

			RegistryItemTreeViewBuilder.AddItemsToTree(root, ListOf(first), t => t);

			AssertEquals("Should only be one root tree node", 1, root.Count);
			AssertEquals("Should only be one sub tree node", 1, root[0].Nodes.Count);
			AssertEquals("Should only be one sub sub tree node", 1, root[0].Nodes[0].Nodes.Count);

			AssertEquals("Base", root[0].Text);
			AssertEquals("Sub1", root[0].Nodes[0].Text);
			AssertEquals("Sub2", root[0].Nodes[0].Nodes[0].Text);
		}

		public void TestRegistryNamesWithWhitespace()
		{
			var items = ListOf(
				NewItem("first", "Base/ Sub "),
				NewItem("second", "Base / Sub"),
				NewItem("second", "Base/Sub")
			);

			var root = (new TreeNode()).Nodes;

			RegistryItemTreeViewBuilder.AddItemsToTree(root, items, t => t);

			AssertEquals("Whitespace should be ignored in sub category paths", 1, root[0].Nodes.Count);
		}

		public void TestBuildTree_NoDuplicates()
		{
			var first = NewItem("First", "Base/Sub1/Sub2");
			var second = NewItem("Second", "Base/Sub1");

			var root = new TreeNode().Nodes;
			RegistryItemTreeViewBuilder.AddItemsToTree(root, ListOf(first, second), t => t);

			AssertEquals(1, root.Count);
			AssertEquals(1, root[0].Nodes.Count);
			AssertEquals(2, root[0].Nodes[0].Nodes.Count);

			var sub1 = root[0].Nodes[0];
			Assert("This node should be at this level in the tree", sub1.Nodes.Cast<TreeNode>().Any(node => node.Tag == second));
			Assert("This node should be at this level in the tree", sub1.Nodes["Sub2"].Nodes[0].Tag == first);
		}

		public void TestGetTag()
		{
			var root = new TreeNode().Nodes;
			var itemsToAdd = ListOf(NewItem("Blah", "Base"));
			RegistryItemTreeViewBuilder.AddItemsToTree(root, itemsToAdd, registryItem => "The best tag eva");

			var item = root[0].FirstNode;
			AssertEquals("Should use the given tag", "The best tag eva", item.Tag);
		}

		public void TestFallback()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Bananas In Pyjamas";
			company.GC_Code = "BIP";

			var b1 = company.Branches.AddNew();
			b1.GB_BranchName = "B1";
			b1.GB_Code = "B1";

			var b2 = company.Branches.AddNew();
			b2.GB_BranchName = "B2";
			b2.GB_Code = "B2";

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_Name = "Star Wars";
			anotherCompany.GC_Code = "STA";

			var c3 = anotherCompany.Branches.AddNew();
			c3.GB_BranchName = "C3PO";
			c3.GB_Code = "C3P";

			Factory.Save();

			var node = new TreeNode();
			RegistryItemTreeViewBuilder.AddFallbackNodes(node.Nodes, Factory);

			TreeViewAssertion.AssertHasPath(node, "Default");
			TreeViewAssertion.AssertHasPath(node, "System");
			TreeViewAssertion.AssertHasPath(node, "Companies/Star Wars/Branches/C3PO");
			TreeViewAssertion.AssertHasPath(node, "Companies/Bananas In Pyjamas/Branches/B1");
			TreeViewAssertion.AssertHasPath(node, "Companies/Bananas In Pyjamas/Branches/B2");
			TreeViewAssertion.AssertHasPath(node, "Companies/Bananas In Pyjamas/Branches/B2");
		}

		public void TestRegistryNamesWithEscapedForwardSlash()
		{
			var items = ListOf(NewItem("Item", @"Base/Something\/Special"));
			var root = (new TreeNode()).Nodes;
			RegistryItemTreeViewBuilder.AddItemsToTree(root, items, t => t);
			AssertEquals("Base", root[0].Text);
			AssertEquals("Something/Special", root[0].Nodes[0].Text);
		}

		#region Implementation

		void CreateCompaniesAndBranches()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "C1";
			company1.GC_Name = "COMP1";

			var branch11 = company1.Branches.AddNew();
			branch11.GB_Code = "B11";
			branch11.GB_BranchName = "B11";

			var branch12 = company1.Branches.AddNew();
			branch12.GB_Code = "B12";
			branch12.GB_BranchName = "B12";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "C2";
			company2.GC_Name = "COMP2";

			var branch21 = company2.Branches.AddNew();
			branch21.GB_Code = "B21";
			branch21.GB_BranchName = "B21";

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "C3";
			company3.GC_Name = "COMP3";

			var branchWithNoCompany = Factory.NewWithValidTestData<GlbBranch>();
			branchWithNoCompany.GB_Code = "NO1";
			branchWithNoCompany.GB_BranchName = "NO1";

			Factory.Save();
		}

		#endregion
	}
}
