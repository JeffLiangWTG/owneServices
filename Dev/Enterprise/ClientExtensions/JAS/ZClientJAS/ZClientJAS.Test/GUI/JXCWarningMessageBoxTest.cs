using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI
{
	class JXCWarningMessageBoxTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestShowDialog_NullReference()
		{
			JXCWarningMessageBox.ShowDialog(null);
		}

		public void TestTreeNodesShouldNotBePopulatedOnLoaded()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject dummyChild = dummy.Collection.AddNew();
			using (dummy.SuspendValidationTesting())
			using (dummyChild.SuspendValidationTesting())
			{
				ValidationHelper validationHelper = new ValidationHelper();
				validationHelper.AddJXCWarning(dummy.Z0_AnotherDateInfo, "MEH");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherDateInfo, "teapot");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning1");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning2");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning3");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning4");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning4");
				validationHelper.AddJXCWarning(dummyChild.Z0_AnotherDecimalInfo, "Warning5");
			}

			JXCWarningMessageBox.ShowDialog(new JXCWarningInfoCollector(dummy));
			using (JXCWarningMessageBox messageBox = (JXCWarningMessageBox)ZFormModaliser.LastFormShownDialogForTest)
			{
				AssertEquals("TreeView should only be populated when the Show Details button is clicked", 0, messageBox.WarningTreeView.Nodes.Count);
			}
		}

		public void TestTreeNodesArePopulatedCorrectly()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject dummyChild = dummy.Collection.AddNew();
			dummy.RegisterEditableChildObject(dummy.Collection);
			using (dummy.SuspendValidationTesting())
			using (dummyChild.SuspendValidationTesting())
			{
				ValidationHelper validationHelper = new ValidationHelper();
				validationHelper.AddJXCWarning(dummy.Z0_AnotherDateInfo, "MEH");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherDateInfo, "teapot");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning1");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning2");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning3");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning4");
				validationHelper.AddJXCWarning(dummy.Z0_AnotherNumberInfo, "warning4");
				validationHelper.AddJXCWarning(dummyChild.Z0_AnotherDecimalInfo, "Warning5");
			}

			JXCWarningMessageBox.ShowDialog(new JXCWarningInfoCollector(dummy));
			using (JXCWarningMessageBox messageBox = (JXCWarningMessageBox)ZFormModaliser.LastFormShownDialogForTest)
			{
				messageBox.DetailsButton.PerformClick();
				AssertEquals("2 Business Object Nodes", 2, messageBox.WarningTreeView.Nodes.Count);
				TreeNode dummyNode = FindUniqueNode(messageBox.WarningTreeView.Nodes, dummy.PK.ToStringKey());
				AssertEquals("Should be showing the BizO's HumanReadableName as the node text", dummy.HumanReadableName, dummyNode.Text);
				AssertEquals("2 Property Nodes under Dummy BizO", 2, dummyNode.Nodes.Count);
				TreeNode anotherDateWarningNode = FindUniqueNode(dummyNode.Nodes, dummy.Z0_AnotherDateInfo.Name);
				AssertEquals("Should be showing the property's HumanReadableName as the node text", dummy.Z0_AnotherDateInfo.HumanReadableName, anotherDateWarningNode.Text);
				AssertEquals("2 warnings for AnotherDate property", 2, anotherDateWarningNode.Nodes.Count);
				AssertContainsWarning(anotherDateWarningNode.Nodes, "MEH");
				AssertContainsWarning(anotherDateWarningNode.Nodes, "teapot");
				TreeNode anotherNumberWarningNode = FindUniqueNode(dummyNode.Nodes, dummy.Z0_AnotherNumberInfo.Name);
				AssertEquals("Should be showing the property's HumanReadableName as the node text", dummy.Z0_AnotherNumberInfo.HumanReadableName, anotherNumberWarningNode.Text);
				AssertEquals("2 warnings for AnotherNumber property", 4, anotherNumberWarningNode.Nodes.Count);
				AssertContainsWarning(anotherNumberWarningNode.Nodes, "warning1");
				AssertContainsWarning(anotherNumberWarningNode.Nodes, "warning2");
				AssertContainsWarning(anotherNumberWarningNode.Nodes, "warning3");
				AssertContainsWarning(anotherNumberWarningNode.Nodes, "warning4");
				TreeNode dummyChildNode = FindUniqueNode(messageBox.WarningTreeView.Nodes, dummyChild.PK.ToStringKey());
				AssertEquals("Should be showing the BizO's HumanReadableName as the node text", dummyChild.HumanReadableName, dummyChildNode.Text);
				AssertEquals("1 Property Node under Dummy Child BizO", 1, dummyChildNode.Nodes.Count);
				AssertEquals("Should contain warning from AnotherDecimal property", dummyChild.Z0_AnotherDecimalInfo.Name, dummyChildNode.Nodes[0].Name);
				AssertEquals("Should be showing the property's HumanReadableName as the node text", dummy.Z0_AnotherDecimalInfo.HumanReadableName, dummyChildNode.Nodes[0].Text);
				AssertEquals("1 warning for AnotherDecimal property", 1, dummyChildNode.Nodes[0].Nodes.Count);
				AssertContainsWarning(dummyChildNode.Nodes[0].Nodes, "Warning5");
			}
		}

		void AssertContainsWarning(TreeNodeCollection collection, string warningMessage)
		{
			TreeNode[] nodesFound = collection.Find(warningMessage, false);
			if (nodesFound.Length > 1)
			{
				Fail("Duplicate warning messages found: \"" + warningMessage + "\"");
			}

			Assert("Should contain JXC warning \"" + warningMessage + "\"", nodesFound.Length > 0);
			AssertEquals("Should be showing the WarningMessage as the Node Text", warningMessage, nodesFound[0].Text);
		}

		TreeNode FindUniqueNode(TreeNodeCollection collection, string key)
		{
			TreeNode[] nodesFound = collection.Find(key, false);
			AssertEquals("Should be unique", 1, nodesFound.Length);
			return nodesFound[0];
		}
	}
}
