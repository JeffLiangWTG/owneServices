using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class TreeViewBuilderTest : TransactionedTestCase
	{
		public void TestUpdateFallbackTree()
		{
			TreeView tree = new TreeView();
			TreeNode node = new TreeNode();

			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTag item = new RegistryItemTag(regItem);
			node.Tag = item;
			Assert("UpdateFallbackTree should return true if the Node has a valid RegistryItemTag",
				Builder.UpdateFallbackTree(tree, node));
			AssertEquals("Node Count", 2, tree.GetNodeCount(false));
			AssertEquals("First Node Text", Builder.SystemTextForTest, tree.Nodes[0].Text);
			AssertEquals("Second Node Text", Builder.CompanyTextForTest, tree.Nodes[1].Text);

			node = new TreeNode();
			Assert("UpdateFallbackTree should return false if the Node does not have a valid RegistryItemTag",
				!Builder.UpdateFallbackTree(tree, node));
			Assert("There should only be one node that informs the user to select a registry item", tree.GetNodeCount(true) == 1);
			AssertEquals("Node Text", Builder.RegistryItemNotSelectedTextForTest, tree.Nodes[0].Text);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Company);
			regItem.CountryFilterPKs = new[] { Guid.NewGuid() };
			item = new RegistryItemTag(regItem);
			node = new TreeNode();
			node.Tag = item;
			Assert("UpdateFallbackTree should return false if no fallbacks were found", !Builder.UpdateFallbackTree(tree, node));
			Assert("There should only be one node that informs the user that no fallbacks were found", tree.GetNodeCount(true) == 1);
			AssertEquals("Node Text", Builder.NoFallbackNodesTextForTest, tree.Nodes[0].Text);
		}

		public void TestAddNodeToCategory()
		{
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, RegistryOptions.IsHidden);
			RegistryItemTag item = new RegistryItemTag(regItem);
			TreeNode node = new TreeNode("Root");

			regItem.Options = RegistryOptions.Default;
			item = new RegistryItemTag(regItem);
			Builder.AddNodeToCategoryForTest(node, item);
			AssertEquals("Root/First Node's Text", "TestCategory", node.FirstNode.Text);
			AssertEquals("Root/First Node/First Node's Tag", item, (RegistryItemTag)node.FirstNode.FirstNode.Tag);

			regItem = new StringRegistryItem("TestItem", (NoResString)"TestCategory/SubCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			item = new RegistryItemTag(regItem);
			Builder.AddNodeToCategoryForTest(node, item);
			AssertEquals("Root/First Node/Second Node's Text", "SubCategory", node.FirstNode.Nodes[1].Text);
			AssertEquals("Root/First Node/Second Node/First Node's Tag", item, (RegistryItemTag)node.FirstNode.Nodes[1].FirstNode.Tag);

			regItem = new StringRegistryItem("TestItem", (NoResString)"TestCategory2", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			item = new RegistryItemTag(regItem);
			Builder.AddNodeToCategoryForTest(node, item);
			AssertEquals("Root/Second Node's Text", "TestCategory2", node.Nodes[1].Text);
			AssertEquals("Root/Second Node/First Node's Tag", item, (RegistryItemTag)node.Nodes[1].FirstNode.Tag);
		}

		public void TestAddItemToCategory()
		{
			TreeNode node = new TreeNode("Root");
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTag item = new RegistryItemTag(regItem);

			Builder.AddItemToCategoryForTest(node, item, "Category");
			AssertEquals("Root Node's Count", 1, node.GetNodeCount(false));
			AssertEquals("Root/First Node's Text", "Category", node.FirstNode.Text);
			AssertEquals("Root/First Node's Count", 1, node.FirstNode.GetNodeCount(true));
			AssertEquals("Root/First Node/First Node's Text", "TestCaption", node.FirstNode.FirstNode.Text);

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption2", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTag item2 = new RegistryItemTag(regItem);

			Builder.AddItemToCategoryForTest(node, item2, "Category");
			AssertEquals("Root Node's Count", 1, node.GetNodeCount(false));
			AssertEquals("Root/First Node's Text", "Category", node.FirstNode.Text);
			AssertEquals("Root/First Node's Count", 2, node.FirstNode.GetNodeCount(true));
			AssertEquals("Root/First Node/First Node's Text", "TestCaption", node.FirstNode.FirstNode.Text);
			AssertEquals("Root/First Node/Second Node's Text", "TestCaption2", node.FirstNode.Nodes[1].Text);
		}

		public void TestInsertSystemNodes()
		{
			TreeView tree = new TreeView();
			int departmentsCount = Builder.GetDepartmentsForTest().Count;

			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTag item = new RegistryItemTag(regItem);

			Builder.InsertSystemNodesForTest(tree, item);
			FallbackLevel fallback = ((FallbackTreeNode)tree.Nodes[0]).GetFallbackLevel();
			AssertEquals("Root Node's Count", 1, tree.GetNodeCount(false));
			AssertEquals("Root Node's Text", Builder.SystemTextForTest, tree.Nodes[0].Text);
			AssertEquals("Root Node's CompanyPK", Guid.Empty, fallback.CompanyPK(false));
			AssertEquals("Root Node's BranchPK", Guid.Empty, fallback.BranchPK);
			AssertEquals("Root Node's DepartmentPK", Guid.Empty, fallback.DepartmentPK);
			AssertEquals("Root Node's Status", FallbackStatus.Active, ((FallbackTreeNode)tree.Nodes[0]).Status);
			AssertEquals("Root/First Node's Count", 1, tree.Nodes[0].GetNodeCount(false));
			AssertEquals("Root/First Node/First Node's Text", Builder.DepartmentTextForTest, tree.Nodes[0].FirstNode.Text);
			AssertEquals("Root/First Node/First Node's Count", departmentsCount, tree.Nodes[0].FirstNode.GetNodeCount(true));

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.SystemDepartment);
			item = new RegistryItemTag(regItem);
			tree.Nodes.Clear();
			Builder.InsertSystemNodesForTest(tree, item);
			fallback = ((FallbackTreeNode)tree.Nodes[0]).GetFallbackLevel();
			AssertEquals("Root Node's Count", 1, tree.GetNodeCount(false));
			AssertEquals("Root Node's Text", Builder.SystemTextForTest, tree.Nodes[0].Text);
			AssertEquals("Root Node's CompanyPK", Guid.Empty, fallback.CompanyPK(false));
			AssertEquals("Root Node's BranchPK", Guid.Empty, fallback.BranchPK);
			AssertEquals("Root Node's DepartmentPK", Guid.Empty, fallback.DepartmentPK);
			AssertEquals("Root Node's Status", FallbackStatus.NotActive, ((FallbackTreeNode)tree.Nodes[0]).Status);
			AssertEquals("Root/First Node's Count", 1, tree.Nodes[0].GetNodeCount(false));
			AssertEquals("Root/First Node/First Node's Text", Builder.DepartmentTextForTest, tree.Nodes[0].FirstNode.Text);
			AssertEquals("Root/First Node/First Node's Count", departmentsCount, tree.Nodes[0].FirstNode.GetNodeCount(true));

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.System);
			item = new RegistryItemTag(regItem);
			tree.Nodes.Clear();
			Builder.InsertSystemNodesForTest(tree, item);
			fallback = ((FallbackTreeNode)tree.Nodes[0]).GetFallbackLevel();
			AssertEquals("Root Node's Count", 1, tree.GetNodeCount(false));
			AssertEquals("Root Node's Text", Builder.SystemTextForTest, tree.Nodes[0].Text);
			AssertEquals("Root Node's CompanyPK", Guid.Empty, fallback.CompanyPK(false));
			AssertEquals("Root Node's BranchPK", Guid.Empty, fallback.BranchPK);
			AssertEquals("Root Node's DepartmentPK", Guid.Empty, fallback.DepartmentPK);
			AssertEquals("Root Node's Status", FallbackStatus.Active, ((FallbackTreeNode)tree.Nodes[0]).Status);
			AssertEquals("Root/First Node's Count", 0, tree.Nodes[0].GetNodeCount(true));
		}

		public void TestInsertCompanyNodes()
		{
			using (var tree = new TreeView())
			using (RegistryItemTreeViewBuilder.SetDemoCompanyGuid(Env.CurrentCompany.PK))
			{
				var builder = new TreeViewBuilderForTest(Factory);
				int departmentsCount = builder.GetDepartmentsForTest().Count;
				GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
				companies.ApplySort(GlbCompanySchema.GC_Name.Name, ListSortDirection.Ascending);
				int companiesCount = (companies.Count) > 1 ? companies.Count - 1 : companies.Count;

				if (companies.Count > 1)
				{
					ZQuery query = new ZQuery(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK));
					companies.AdditionalFilter = query;
				}

				IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
				RegistryItemTag item = new RegistryItemTag(regItem);

				builder.InsertCompanyNodesForTest(tree, item);
				AssertEquals("Root Node's Count", 1, tree.GetNodeCount(false));
				AssertEquals("Root Node's Text", builder.CompanyTextForTest, tree.Nodes[0].Text);
				AssertEquals("Root/First Node's Count", companiesCount, tree.Nodes[0].GetNodeCount(false));
				for (int x = 0; x < tree.Nodes[0].GetNodeCount(false); ++x)
				{
					FallbackLevel fallback = ((FallbackTreeNode)tree.Nodes[0].Nodes[x]).GetFallbackLevel();
					AssertEquals("Root/First Node/Node Number " + x + "'s Text", companies[x].HumanReadableNameForRegistry, tree.Nodes[0].Nodes[x].Text);
					AssertEquals("Root/First Node/Node Number " + x + "'s CompanyPK", companies[x].PK.ToGuid(), fallback.CompanyPK(false));
					AssertEquals("Root/First Node/Node Number " + x + "'s BranchPK", Guid.Empty, fallback.BranchPK);
					AssertEquals("Root/First Node/Node Number " + x + "'s DepartmentPK", Guid.Empty, fallback.DepartmentPK);
					AssertEquals("Root/First Node/Node Number " + x + "'s Status", FallbackStatus.Active, ((FallbackTreeNode)tree.Nodes[0].Nodes[x]).Status);
					AssertEquals("Root/First Node/Node Number " + x + "'s Count", 2, tree.Nodes[0].Nodes[x].GetNodeCount(false));
					AssertEquals("Root/First Node/Node Number " + x + "/First Node's Text", builder.DepartmentTextForTest, tree.Nodes[0].Nodes[x].FirstNode.Text);
					AssertEquals("Root/First Node/Node Number " + x + "/First Node's Count", departmentsCount, tree.Nodes[0].Nodes[x].FirstNode.GetNodeCount(true));

					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, companies[x].PK);
					GlbBranchCollection branches = new GlbBranchCollection(Factory, filter);
					branches.Load();
					branches.Sort(GlbBranchSchema.GB_BranchName.Name, ListSortDirection.Ascending);
					AssertEquals("Root/First Node/Node Number " + x + "/Second Node's Text", builder.BranchTextForTest, tree.Nodes[0].Nodes[x].Nodes[1].Text);
					AssertEquals("Root/First Node/Node Number " + x + "/Second Nodes' Count", branches.Count, tree.Nodes[0].Nodes[x].Nodes[1].GetNodeCount(false));
					for (int y = 0; y < tree.Nodes[0].Nodes[x].Nodes[1].GetNodeCount(false); ++y)
					{
						fallback = ((FallbackTreeNode)tree.Nodes[0].Nodes[x].Nodes[1].Nodes[y]).GetFallbackLevel();
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "'s Text", branches[y].HumanReadableNameForRegistry, tree.Nodes[0].Nodes[x].Nodes[1].Nodes[y].Text);
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "'s CompanyPK", companies[x].PK.ToGuid(), fallback.CompanyPK(false));
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "'s BranchPK", branches[y].PK.ToGuid(), fallback.BranchPK);
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "'s DepartmentPK", Guid.Empty, fallback.DepartmentPK);
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "'s Status", FallbackStatus.Active, ((FallbackTreeNode)tree.Nodes[0].Nodes[x].Nodes[1].Nodes[y]).Status);
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "'s Count", 1, tree.Nodes[0].Nodes[x].Nodes[1].Nodes[y].GetNodeCount(false));
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "/First Node's Text", builder.DepartmentTextForTest, tree.Nodes[0].Nodes[x].Nodes[1].Nodes[y].FirstNode.Text);
						AssertEquals("Root/First Node/Node Number " + x + "/Second Node/Node Number " + y + "/First Node's Count", departmentsCount, tree.Nodes[0].Nodes[x].Nodes[1].Nodes[y].FirstNode.GetNodeCount(true));
					}
				}

				regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.CompanyDepartment);
				item = new RegistryItemTag(regItem);
				tree.Nodes.Clear();
				builder.InsertCompanyNodesForTest(tree, item);
				AssertEquals("Root Node's Count", 1, tree.GetNodeCount(false));
				AssertEquals("Root Node's Text", builder.CompanyTextForTest, tree.Nodes[0].Text);
				AssertEquals("Root/First Node's Count", companiesCount, tree.Nodes[0].GetNodeCount(false));
				for (int x = 0; x < tree.Nodes[0].GetNodeCount(false); ++x)
				{
					AssertEquals("Root/First Node/Node Number " + x + "'s Text", companies[x].HumanReadableNameForRegistry, tree.Nodes[0].Nodes[x].Text);
					AssertEquals("Root/First Node/Node Number " + x + "'s Status", FallbackStatus.NotActive, ((FallbackTreeNode)tree.Nodes[0].Nodes[x]).Status);
					AssertEquals("Root/First Node/Node Number " + x + "'s Count", 1, tree.Nodes[0].Nodes[x].GetNodeCount(false));
					AssertEquals("Root/First Node/Node Number " + x + "/First Node's Text", builder.DepartmentTextForTest, tree.Nodes[0].Nodes[x].FirstNode.Text);
					AssertEquals("Root/First Node/Node Number " + x + "/First Node's Count", departmentsCount, tree.Nodes[0].Nodes[x].FirstNode.GetNodeCount(true));
				}

				regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Company);
				item = new RegistryItemTag(regItem);
				tree.Nodes.Clear();
				builder.InsertCompanyNodesForTest(tree, item);
				AssertEquals("Root Node's Count", 1, tree.GetNodeCount(false));
				AssertEquals("Root Node's Text", builder.CompanyTextForTest, tree.Nodes[0].Text);
				AssertEquals("Root/First Node's Count", companiesCount, tree.Nodes[0].GetNodeCount(false));
				for (int x = 0; x < tree.Nodes[0].GetNodeCount(false); ++x)
				{
					FallbackLevel fallback = ((FallbackTreeNode)tree.Nodes[0].Nodes[x]).GetFallbackLevel();
					AssertEquals("Root/First Node/Node Number " + x + "'s Text", companies[x].HumanReadableNameForRegistry, tree.Nodes[0].Nodes[x].Text);
					AssertEquals("Root/First Node/Node Number " + x + "'s CompanyPK", companies[x].PK.ToGuid(), fallback.CompanyPK(false));
					AssertEquals("Root/First Node/Node Number " + x + "'s BranchPK", Guid.Empty, fallback.BranchPK);
					AssertEquals("Root/First Node/Node Number " + x + "'s DepartmentPK", Guid.Empty, fallback.DepartmentPK);
					AssertEquals("Root/First Node/Node Number " + x + "'s Status", FallbackStatus.Active, ((FallbackTreeNode)tree.Nodes[0].Nodes[x]).Status);
					AssertEquals("Root/First Node/Node Number " + x + "'s Count", 0, tree.Nodes[0].Nodes[x].GetNodeCount(false));
				}
			}
		}

		public void TestInsertBranchNodes()
		{
			FallbackTreeNode node = new FallbackTreeNode("Branches");
			int departmentsCount = Builder.GetDepartmentsForTest().Count;

			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			GlbCompany company = companies[0];

			ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, company.PK);
			GlbBranchCollection branches = new GlbBranchCollection(Factory, filter);
			branches.Load();
			branches.Sort(GlbBranchSchema.GB_BranchName.Name, ListSortDirection.Ascending);

			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTag item = new RegistryItemTag(regItem);

			Builder.InsertBranchNodesForTest(node, company.PK, item);
			AssertEquals("Root Node's Count", 1, node.GetNodeCount(false));
			AssertEquals("Root Node's Text", Builder.BranchTextForTest, node.Nodes[0].Text);
			AssertEquals("Root/First Node's Count", branches.Count, node.Nodes[0].GetNodeCount(false));
			for (int x = 0; x < node.Nodes[0].GetNodeCount(false); ++x)
			{
				FallbackLevel fallback = ((FallbackTreeNode)node.Nodes[0].Nodes[x]).GetFallbackLevel();
				AssertEquals("Root/First Node/Node Number " + x + "'s Text", branches[x].HumanReadableNameForRegistry, node.Nodes[0].Nodes[x].Text);
				AssertEquals("Root/First Node/Node Number " + x + "'s CompanyPK", company.PK.ToGuid(), fallback.CompanyPK(false));
				AssertEquals("Root/First Node/Node Number " + x + "'s BranchPK", branches[x].PK.ToGuid(), fallback.BranchPK);
				AssertEquals("Root/First Node/Node Number " + x + "'s DepartmentPK", Guid.Empty, fallback.DepartmentPK);
				AssertEquals("Root/First Node/Node Number " + x + "'s Status", FallbackStatus.Active, ((FallbackTreeNode)node.Nodes[0].Nodes[x]).Status);
				AssertEquals("Root/First Node/Node Number " + x + "'s Count", 1, node.Nodes[0].Nodes[x].GetNodeCount(false));
				AssertEquals("Root/First Node/Node Number " + x + "/First Node's Text", Builder.DepartmentTextForTest, node.Nodes[0].Nodes[x].FirstNode.Text);
				AssertEquals("Root/First Node/Node Number " + x + "/First Node's Count", departmentsCount, node.Nodes[0].Nodes[x].FirstNode.GetNodeCount(true));
			}

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.BranchDepartment);
			item = new RegistryItemTag(regItem);
			node.Nodes.Clear();
			Builder.InsertBranchNodesForTest(node, company.PK, item);
			AssertEquals("Root Node's Count", 1, node.GetNodeCount(false));
			AssertEquals("Root Node's Text", Builder.BranchTextForTest, node.Nodes[0].Text);
			AssertEquals("Root/First Node's Count", branches.Count, node.Nodes[0].GetNodeCount(false));
			for (int x = 0; x < node.Nodes[0].GetNodeCount(false); ++x)
			{
				AssertEquals("Root/First Node/Node Number " + x + "'s Text", branches[x].HumanReadableNameForRegistry, node.Nodes[0].Nodes[x].Text);
				AssertEquals("Root/First Node/Node Number " + x + "'s Status", FallbackStatus.NotActive, ((FallbackTreeNode)node.Nodes[0].Nodes[x]).Status);
				AssertEquals("Root/First Node/Node Number " + x + "'s Count", 1, node.Nodes[0].Nodes[x].GetNodeCount(false));
				AssertEquals("Root/First Node/Node Number " + x + "/First Node's Text", Builder.DepartmentTextForTest, node.Nodes[0].Nodes[x].FirstNode.Text);
				AssertEquals("Root/First Node/Node Number " + x + "/First Node's Count", departmentsCount, node.Nodes[0].Nodes[x].FirstNode.GetNodeCount(true));
			}

			regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Branch);
			item = new RegistryItemTag(regItem);
			node.Nodes.Clear();
			Builder.InsertBranchNodesForTest(node, company.PK, item);
			AssertEquals("Root Node's Count", 1, node.GetNodeCount(false));
			AssertEquals("Root Node's Text", Builder.BranchTextForTest, node.Nodes[0].Text);
			AssertEquals("Root/First Node's Count", branches.Count, node.Nodes[0].GetNodeCount(false));
			for (int x = 0; x < node.Nodes[0].GetNodeCount(false); ++x)
			{
				FallbackLevel fallback = ((FallbackTreeNode)node.Nodes[0].Nodes[x]).GetFallbackLevel();
				AssertEquals("Root/First Node/Node Number " + x + "'s Text", branches[x].HumanReadableNameForRegistry, node.Nodes[0].Nodes[x].Text);
				AssertEquals("Root/First Node/Node Number " + x + "'s Text", branches[x].HumanReadableNameForRegistry, node.Nodes[0].Nodes[x].Text);
				AssertEquals("Root/First Node/Node Number " + x + "'s CompanyPK", company.PK.ToGuid(), fallback.CompanyPK(false));
				AssertEquals("Root/First Node/Node Number " + x + "'s BranchPK", branches[x].PK.ToGuid(), fallback.BranchPK);
				AssertEquals("Root/First Node/Node Number " + x + "'s DepartmentPK", Guid.Empty, fallback.DepartmentPK);
				AssertEquals("Root/First Node/Node Number " + x + "'s Status", FallbackStatus.Active, ((FallbackTreeNode)node.Nodes[0].Nodes[x]).Status);
				AssertEquals("Root/First Node/Node Number " + x + "'s Count", 0, node.Nodes[0].Nodes[x].GetNodeCount(false));
			}
		}

		public void TestInsertDepartmentNodes()
		{
			GlbDepartmentCollection departments = Builder.GetDepartmentsForTest();

			StringRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Branch);
			RegistryItemTag item = new RegistryItemTag(regItem);

			ZGuid companyPK = ZGuid.NewZGuid();
			FallbackTreeNode parentNode = new FallbackTreeNode("Parent");
			Builder.InsertDepartmentNodesForTest(parentNode, companyPK, ZGuid.Empty, item);

			AssertEquals("Root/First Node's Text", Builder.DepartmentTextForTest, parentNode.FirstNode.Text);
			AssertEquals("Root/First Node's Count", departments.Count, parentNode.FirstNode.GetNodeCount(true));

			foreach (TreeNode currentNode in parentNode.FirstNode.Nodes)
			{
				GlbDepartment department = departments[currentNode.Index];
				FallbackLevel fallback = ((FallbackTreeNode)currentNode).GetFallbackLevel();
				AssertEquals("Node " + currentNode.Index + "'s Text", department.GE_Desc, currentNode.Text);
				AssertEquals("Node " + currentNode.Index + "'s CompanyPK", companyPK.ToGuid(), fallback.CompanyPK(false));
				AssertEquals("Node " + currentNode.Index + "'s BranchPK", Guid.Empty, fallback.BranchPK);
				AssertEquals("Node " + currentNode.Index + "'s DepartmentPK", department.PK.ToGuid(), fallback.DepartmentPK);
				AssertEquals("Node " + currentNode.Index + "'s Status", FallbackStatus.Active, ((FallbackTreeNode)currentNode).Status);
			}

			ZGuid branchPK = ZGuid.NewZGuid();
			parentNode.Nodes.Clear();
			Builder.InsertDepartmentNodesForTest(parentNode, ZGuid.Empty, branchPK, item);

			AssertEquals("Root/First Node's Text", Builder.DepartmentTextForTest, parentNode.FirstNode.Text);
			AssertEquals("Root/First Node's Count", departments.Count, parentNode.FirstNode.GetNodeCount(true));

			foreach (TreeNode currentNode in parentNode.FirstNode.Nodes)
			{
				GlbDepartment department = departments[currentNode.Index];
				FallbackLevel fallback = ((FallbackTreeNode)currentNode).GetFallbackLevel();
				AssertEquals("Node " + currentNode.Index + "'s Text", department.GE_Desc, currentNode.Text);
				AssertEquals("Node " + currentNode.Index + "'s CompanyPK", Guid.Empty, fallback.CompanyPK(false));
				AssertEquals("Node " + currentNode.Index + "'s BranchPK", branchPK.ToGuid(), fallback.BranchPK);
				AssertEquals("Node " + currentNode.Index + "'s DepartmentPK", department.PK.ToGuid(), fallback.DepartmentPK);
				AssertEquals("Node " + currentNode.Index + "'s Status", FallbackStatus.Active, ((FallbackTreeNode)currentNode).Status);
			}
		}

		public void TestDbHitsInsertDepartmentNodes()
		{
			GlbDepartmentCollection departments = Builder.GetDepartmentsForTest();

			StringRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Branch);
			regItem.DepartmentsAllowed = DepartmentFlags.Sea;
			RegistryItemTag item = new RegistryItemTag(regItem);

			ZGuid companyPK = ZGuid.NewZGuid();
			FallbackTreeNode parentNode = new FallbackTreeNode("Parent");
			var originalHitCount = Db.Connection.ExecutedCommandCountForAllConnections;
			Builder.InsertDepartmentNodesForTest(parentNode, companyPK, ZGuid.Empty, item);
			AssertEquals(1, Db.Connection.ExecutedCommandCountForAllConnections - originalHitCount);
		}

		public void TestGetCompanies()
		{
			var demoCompanyGuid = Env.CurrentCompany.PK;
			using (RegistryItemTreeViewBuilder.SetDemoCompanyGuid(demoCompanyGuid))
			{
				var builder = new TreeViewBuilderForTest(Factory);
				var companies = new GlbCompanyCollection(Factory);

				if (companies.Count > 1)
				{
					Assert("Company Collection should not contain DemoCompany if there is more than one Company in the system",
						!builder.GetCompaniesForTest().Contains(Factory.Load<GlbCompany>(demoCompanyGuid)));
				}

				int initialCount = companies.Count;
				ZQuery query = new ZQuery(GlbCompanySchema.GC_IsActive, true);
				companies.AdditionalFilter = query;
				int activeCount = companies.Count;

				var activeCompany = Factory.NewWithValidTestData<GlbCompany>();
				activeCompany.GC_IsActive = true;
				activeCompany.Factory.Save();

				var inactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
				inactiveCompany.GC_IsActive = false;
				inactiveCompany.Factory.Save();

				builder.SetHideInactiveFallbacks(false);
				companies = builder.GetCompaniesForTest();
				AssertEquals("Companies Count", initialCount + 1, companies.Count);

				Assert("ActiveCompany not found", companies.Contains(activeCompany));
				Assert("InactiveCompany not found", companies.Contains(inactiveCompany));

				builder.SetHideInactiveFallbacks(true);
				companies = builder.GetCompaniesForTest();
				AssertEquals("Companies Count", activeCount, companies.Count);
				Assert("ActiveCompany not found", companies.Contains(activeCompany));
				Assert("InactiveCompany was found", !companies.Contains(inactiveCompany));

				ClearAllCompaniesExceptForCurrent(demoCompanyGuid);

				builder.SetHideInactiveFallbacks(false);
				companies = new GlbCompanyCollection(Factory);
				Assert("Company Collection should contain DemoCompany if it is the only Company in the system",
					companies.Contains(Factory.Load<GlbCompany>(demoCompanyGuid)));
			}
		}

		public void TestGetBranches()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.Factory.Save();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.Factory.Save();

			var activeBranch = Factory.NewWithValidTestData<GlbBranch>();
			activeBranch.GB_GC = company1.PK;
			activeBranch.GB_IsActive = true;
			activeBranch.Factory.Save();

			var anotherActiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherActiveBranch.GB_GC = company2.PK;
			anotherActiveBranch.GB_IsActive = true;
			anotherActiveBranch.Factory.Save();

			var inactiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			inactiveBranch.GB_GC = company1.PK;
			inactiveBranch.GB_IsActive = false;
			inactiveBranch.Factory.Save();

			Builder.SetHideInactiveFallbacks(false);
			GlbBranchCollection branches = Builder.GetBranchesForTest(company1.PK);
			AssertEquals("Branches Count", 2, branches.Count);
			Assert("Active Branch not found", branches.Contains(activeBranch.PK));
			Assert("Inactive Branch not found", branches.Contains(inactiveBranch.PK));

			Builder.SetHideInactiveFallbacks(true);
			branches = Builder.GetBranchesForTest(company1.PK);
			Assert("Active Branch not found", branches.Contains(activeBranch.PK));
			Assert("Inactive Branch was found", !branches.Contains(inactiveBranch.PK));

			branches = Builder.GetBranchesForTest(company2.PK);
			AssertEquals("There should be 1 branch", 1, branches.Count);
			Assert("Active Branch not found", branches.Contains(anotherActiveBranch.PK));
		}

		public void TestGetBranchesSorted_ShowCodeAtCompanyAndBranchName()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.Factory.Save();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "TTT";
			branch1.GB_BranchName = "Abc";
			branch1.GB_GC = company.PK;
			branch1.GB_IsActive = true;
			branch1.Factory.Save();

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "DDD";
			branch2.GB_BranchName = "Bcd";
			branch2.GB_GC = company.PK;
			branch2.GB_IsActive = true;
			branch2.Factory.Save();

			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_Code = "BBB";
			branch2.GB_BranchName = "Cde";
			branch3.GB_GC = company.PK;
			branch3.GB_IsActive = true;
			branch3.Factory.Save();

			var branch4 = Factory.NewWithValidTestData<GlbBranch>();
			branch4.GB_Code = "EEE";
			branch2.GB_BranchName = "Def";
			branch4.GB_GC = company.PK;
			branch4.GB_IsActive = true;
			branch4.Factory.Save();

			Env.Registry.ShowCodeAtCompanyAndBranchName = true;
			var branches = Builder.GetBranchesForTest(company.PK);

			AssertEquals(branch3.GB_Code, branches[0].GB_Code);
			AssertEquals(branch2.GB_Code, branches[1].GB_Code);
			AssertEquals(branch4.GB_Code, branches[2].GB_Code);
			AssertEquals(branch1.GB_Code, branches[3].GB_Code);

			Env.Registry.ShowCodeAtCompanyAndBranchName = false;
			branches = Builder.GetBranchesForTest(company.PK);

			AssertEquals(branch1.GB_Code, branches[0].GB_Code);
			AssertEquals(branch2.GB_Code, branches[1].GB_Code);
			AssertEquals(branch3.GB_Code, branches[2].GB_Code);
			AssertEquals(branch4.GB_Code, branches[3].GB_Code);
		}

		public void TestGetDepartments()
		{
			GlbDepartmentCollection departments = new GlbDepartmentCollection(Factory);
			int count = departments.Count;
			int activeCount = new List<GlbDepartment>(departments.Find(new ZQuery(GlbDepartmentSchema.GE_IsActive, ZBool.True))).Count;

			GlbDepartment activeDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment inactiveDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			activeDepartment.GE_IsActive = true;
			inactiveDepartment.GE_IsActive = false;
			Factory.Save();

			Builder.SetHideInactiveFallbacks(false);
			departments = Builder.GetDepartmentsForTest();
			AssertEquals("Departments Count", count + 2, departments.Count);
			AssertEquals("Active Department should be in the list.", true, departments.Contains(activeDepartment));
			AssertEquals("Inactive department should be in the list.", true, departments.Contains(activeDepartment));

			Builder.SetHideInactiveFallbacks(true);
			departments = Builder.GetDepartmentsForTest();
			AssertEquals("Departments Count", activeCount + 1, departments.Count);
			AssertEquals("Active department should be found.", true, departments.Contains(activeDepartment));
			AssertEquals("Inactive department should not be in the list.", false, departments.Contains(inactiveDepartment));
		}

		public void TestInsertingNonVisibleCompanies()
		{
			TreeView tree = new TreeView();

			var country = Factory.NewWithValidTestData<RefCountry>();

			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			regItem.CountryFilterPKs = new[] { country.PK.ToGuid() };
			RegistryItemTag item = new RegistryItemTag(regItem);

			Builder.InsertCompanyNodesForTest(tree, item);
			AssertEquals("Root Node's Count", 0, tree.GetNodeCount(false));

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "NewCountry";
			company.GC_RN_NKCountryCode = country.Code;
			Factory.Save();

			tree.Nodes.Clear();
			Builder.SetHideInactiveFallbacks(true);
			Builder.InsertCompanyNodesForTest(tree, item);
			AssertEquals("Root/First Node's Count", 1, tree.Nodes[0].GetNodeCount(false));
			AssertEquals("Root/First Node/First Node's Text", company.HumanReadableNameForRegistry, tree.Nodes[0].FirstNode.Text);
		}

		public void TestInsertingNonVisibleBranches()
		{
			FallbackTreeNode companyNode = new FallbackTreeNode("Company");

			var country = Factory.NewWithValidTestData<RefCountry>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = country.Code;

			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint",
				RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment);
			regItem.CountryFilterPKs = new[] { country.PK.ToGuid() };
			RegistryItemTag item = new RegistryItemTag(regItem);

			Builder.InsertBranchNodesForTest(companyNode, company.PK, item);
			AssertEquals("Root Node's Count", 0, companyNode.GetNodeCount(false));

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_BranchName = "NewBranch";
			branch.GB_GC = company.PK;
			Factory.Save();

			companyNode.Nodes.Clear();
			Builder.InsertBranchNodesForTest(companyNode, company.PK, item);
			AssertEquals("Root/First Node's Count", 1, companyNode.Nodes[0].GetNodeCount(false));
			AssertEquals("Root/First Node/First Node's Text", branch.HumanReadableNameForRegistry, companyNode.Nodes[0].FirstNode.Text);
		}

		public void TestInsertingNonVisibleDepartments()
		{
			ZQuery filter = new ZQuery(GlbDepartmentSchema.GE_Air, ZBool.True);
			GlbDepartmentCollection departments = new GlbDepartmentCollection(Factory, filter);

			FallbackTreeNode parentNode = new FallbackTreeNode("Parent");

			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.CompanyDepartment);
			regItem.DepartmentsAllowed = DepartmentFlags.Air;
			RegistryItemTag item = new RegistryItemTag(regItem);

			Builder.InsertDepartmentNodesForTest(parentNode, ZGuid.Empty, ZGuid.Empty, item);
			AssertEquals("Root/First Node's Count", departments.Count, parentNode.Nodes[0].GetNodeCount(false));

			regItem.DepartmentsAllowed = DepartmentFlags.All;
			departments = new GlbDepartmentCollection(Factory);

			parentNode.Nodes.Clear();
			Builder.SetHideInactiveFallbacks(false);
			Builder.InsertDepartmentNodesForTest(parentNode, ZGuid.Empty, ZGuid.Empty, item);
			AssertEquals("Root/First Node's Count", departments.Count, parentNode.Nodes[0].GetNodeCount(false));
		}

		public void TestAddNodeToMultipleCategories()
		{
			IRegistryItem registryItem = new RegistryItemImpl("", (NoResString)"Caption", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)"Category1", (NoResString)"Category2/Category3");

			RegistryItemTag tag = new RegistryItemTag(registryItem);

			TreeNode node = new TreeNode("");
			Builder.AddNodeToCategoryForTest(node, tag);

			AssertEquals("There should be 2 nodes added.", 2, node.GetNodeCount(false));

			AssertEquals("Node 1's Text", "Category1", node.Nodes[0].Text);
			AssertEquals("Node 1 should have 1 subnode.", 1, node.Nodes[0].GetNodeCount(true));
			AssertEquals("Node 1's subnode's text", "Caption", node.Nodes[0].FirstNode.Text);
			AssertEquals("Node 1's subnode should not have any subnodes.", 0, node.Nodes[0].FirstNode.GetNodeCount(true));

			AssertEquals("Node 2's Text", "Category2", node.Nodes[1].Text);
			AssertEquals("Node 2 should have 1 subnode.", 1, node.Nodes[1].GetNodeCount(false));
			AssertEquals("Node 2's subnode's text", "Category3", node.Nodes[1].FirstNode.Text);
			AssertEquals("Node 2's subnode should have 1 subnode.", 1, node.Nodes[1].FirstNode.GetNodeCount(true));
			AssertEquals("Node 2's subnode's subnode's text", "Caption", node.Nodes[1].FirstNode.FirstNode.Text);
			AssertEquals("Node 2's subnode's subnode should not have any subnodes.", 0, node.Nodes[1].FirstNode.FirstNode.GetNodeCount(true));
		}

		public void TestCategoryWhitespaceIgnored()
		{
			var rootNode = new TreeNode("");

			var registryItem = new RegistryItemImpl("", (NoResString)"Item 1", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)"Category/Subcategory");
			Builder.AddNodeToCategoryForTest(rootNode, new RegistryItemTag(registryItem));
			registryItem = new RegistryItemImpl("", (NoResString)"Item 2", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)"Category /Subcategory");
			Builder.AddNodeToCategoryForTest(rootNode, new RegistryItemTag(registryItem));
			registryItem = new RegistryItemImpl("", (NoResString)"Item 3", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)"Category/ Subcategory");
			Builder.AddNodeToCategoryForTest(rootNode, new RegistryItemTag(registryItem));
			registryItem = new RegistryItemImpl("", (NoResString)"Item 4", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)"Category / Subcategory");
			Builder.AddNodeToCategoryForTest(rootNode, new RegistryItemTag(registryItem));
			registryItem = new RegistryItemImpl("", (NoResString)"Item 5", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)" Category/Subcategory");
			Builder.AddNodeToCategoryForTest(rootNode, new RegistryItemTag(registryItem));
			registryItem = new RegistryItemImpl("", (NoResString)"Item 6", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)"Category/Subcategory ");
			Builder.AddNodeToCategoryForTest(rootNode, new RegistryItemTag(registryItem));
			registryItem = new RegistryItemImpl("", (NoResString)"Item 7", (NoResString)"", new StringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.Default, "", false, (NoResString)"  Category  /  Subcategory  ");
			Builder.AddNodeToCategoryForTest(rootNode, new RegistryItemTag(registryItem));

			AssertEquals("There should be only 1 category node.", 1, rootNode.GetNodeCount(false));
			AssertEquals("Root Category text", "Category", rootNode.Nodes[0].Text);
			AssertEquals("There should be only 1 sub-category node.", 1, rootNode.Nodes[0].GetNodeCount(false));
			AssertEquals("Subcategory text", "Subcategory", rootNode.Nodes[0].Nodes[0].Text);
			AssertEquals("There should be 7 item nodes", 7, rootNode.Nodes[0].Nodes[0].GetNodeCount(false));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Factory = new BusinessObjectFactory();
			Builder = new TreeViewBuilderForTest(Factory);
		}

		void ClearAllCompaniesExceptForCurrent(Guid currentCompanyGuid)
		{
			var companies = new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, currentCompanyGuid));
			companies.DeleteAll();
		}

		TreeViewBuilderForTest Builder;
		BusinessObjectFactory Factory;

		#region region TreeViewBuilderForTest

		class TreeViewBuilderForTest : RegistryFormTreeViewBuilder
		{
			public TreeViewBuilderForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public BusinessObjectFactory FactoryForTest
			{
				get { return Factory; }
			}

			public string SystemTextForTest
			{
				get { return SystemText; }
			}

			public string CompanyTextForTest
			{
				get { return CompanyText; }
			}

			public string BranchTextForTest
			{
				get { return BranchText; }
			}

			public string DepartmentTextForTest
			{
				get { return DepartmentText; }
			}

			public string RegistryItemNotSelectedTextForTest
			{
				get { return RegistryItemNotSelectedText; }
			}

			public string NoFallbackNodesTextForTest
			{
				get { return NoFallbackNodesText; }
			}

			public GlbCompanyCollection GetCompaniesForTest() => companiesForCreatingFallbacks;
			public GlbCompanyCollection GetCompaniesForFilteringRegistryItemsForTest() => companiesForFilteringRegistryItems;

			public void AddNodeToCategoryForTest(TreeNode node, RegistryItemTag item)
			{
				RegistryItemTreeViewBuilder.AddItemsToTree(node.Nodes, new[] { item.RegistryItem }, (_) => item);
			}

			public void AddItemToCategoryForTest(TreeNode node, RegistryItemTag item, string category)
			{
				var parent = RegistryItemTreeViewBuilder.GetOrCreateNodePath(node.Nodes, category, category);
				parent.Add(new TreeNode(item.Caption) { Tag = item });
			}

			public void InsertSystemNodesForTest(TreeView fallbackTree, RegistryItemTag item)
			{
				InsertSystemNodes(fallbackTree, item);
			}

			public void InsertCompanyNodesForTest(TreeView fallbackTree, RegistryItemTag item)
			{
				InsertCompanyNodes(fallbackTree, item);
			}

			public void InsertBranchNodesForTest(FallbackTreeNode companyNode, ZGuid companyPK, RegistryItemTag item)
			{
				InsertBranchNodes(companyNode, companyPK, item);
			}

			public void InsertDepartmentNodesForTest(FallbackTreeNode parentNode, ZGuid companyPK, ZGuid branchPK, RegistryItemTag item)
			{
				InsertDepartmentNodes(parentNode, companyPK, branchPK, item);
			}

			public GlbBranchCollection GetBranchesForTest(ZGuid companyPK)
			{
				return GetBranches(companyPK);
			}

			public GlbDepartmentCollection GetDepartmentsForTest()
			{
				return GetDepartments();
			}
		}

		#endregion

		#endregion
	}
}
