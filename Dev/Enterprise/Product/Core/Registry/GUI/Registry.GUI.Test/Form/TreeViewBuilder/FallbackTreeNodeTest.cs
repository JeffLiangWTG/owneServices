using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class FallbackTreeNodeTest : TestCase
	{
		public void TestGetPKsAndStatus()
		{
			FallbackTreeNode node = new FallbackTreeNode("Test");
			AssertEquals("Node's Status", FallbackStatus.NotAFallback, node.Status);

			node = new FallbackTreeNode("Test", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false);
			AssertEquals("Node's Status", FallbackStatus.NotActive, node.Status);

			ZGuid companyPK = ZGuid.NewZGuid();
			ZGuid branchPK = ZGuid.NewZGuid();
			ZGuid departmentPK = ZGuid.NewZGuid();

			node = new FallbackTreeNode("Test", companyPK, ZGuid.Empty, departmentPK, true);
			FallbackLevel fallback = node.GetFallbackLevel();
			AssertEquals("Node's Status", FallbackStatus.Active, node.Status);
			AssertEquals("CompanyPK", companyPK.ToGuid(), fallback.CompanyPK(true));
			AssertEquals("BranchPK", Guid.Empty, fallback.BranchPK);
			AssertEquals("DepartmentPK", departmentPK.ToGuid(), fallback.DepartmentPK);
			AssertEquals("Level", RegistryStorageFlags.CompanyDepartment, fallback.Level);

			node = new FallbackTreeNode("Test", ZGuid.Empty, branchPK, departmentPK, true);
			fallback = node.GetFallbackLevel();
			AssertEquals("Node's Status", FallbackStatus.Active, node.Status);
			AssertEquals("CompanyPK", Guid.Empty, fallback.CompanyPK(true));
			AssertEquals("BranchPK", branchPK.ToGuid(), fallback.BranchPK);
			AssertEquals("DepartmentPK", departmentPK.ToGuid(), fallback.DepartmentPK);
			AssertEquals("Level", RegistryStorageFlags.BranchDepartment, fallback.Level);
		}

		public void TestGetComparator()
		{
			ZGuid companyPK = ZGuid.NewZGuid();
			ZGuid branchPK = ZGuid.Empty;
			ZGuid departmentPK = ZGuid.NewZGuid();
			FallbackTreeNode node = new FallbackTreeNode("Test", companyPK, branchPK, departmentPK, true);
			AssertEquals("Comparator", "Test" + companyPK.ToString() + branchPK.ToString() + departmentPK.ToString(), node.Comparator);

			FallbackTreeNode childNode = new FallbackTreeNode("Test2", companyPK, branchPK, departmentPK, true);
			node.Nodes.Add(childNode);
			AssertEquals("Comparator", node.Comparator + "Test2" + companyPK.ToString() + branchPK.ToString() + departmentPK.ToString(), childNode.Comparator);
		}

		public void TestComparatorForUniqueness()
		{
			RegistryFormTreeViewBuilder builder = new RegistryFormTreeViewBuilder(new CargoWise.EntityFramework.BusinessObjectFactory());
			TreeView fallbackTree = new TreeView();
			IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
			RegistryItemTag item = new RegistryItemTag(regItem);
			TreeNode registryNode = new TreeNode("Registry");
			registryNode.Tag = item;
			builder.UpdateFallbackTree(fallbackTree, registryNode);

			fallbackTree.ExpandAll();
			FallbackTreeNode activeNode = (FallbackTreeNode)fallbackTree.Nodes[0];

			bool foundDuplicate = false;
			ArrayList comparatorArray = new ArrayList();

			while (activeNode != null)
			{
				if (!comparatorArray.Contains(activeNode.Comparator))
				{
					comparatorArray.Add(activeNode.Comparator);
					activeNode = (FallbackTreeNode)activeNode.NextVisibleNode;
				}
				else
				{
					foundDuplicate = true;
					break;
				}
			}

			Assert("Duplicate comparator was found - this will cause problems for the form", !foundDuplicate);
		}
	}
}
