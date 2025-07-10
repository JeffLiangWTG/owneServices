using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

using static Enterprise.Registry.GUI.Testing.RegistryFormTest;

namespace Enterprise.Registry.GUI.Test.Form
{
	sealed class RegistryItemsTreeViewTest : TestCase
	{
		#region TestCopyRegistryPathtoClipboard

		public void TestContextMenuExistsOnRegistryItemsTreeView()
		{
			var registry = new Mock<IRegistry>();
			using (var testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.Show();
				var registriesTreeView = testForm.GetRegistriesTreeView();

				AssertNotEquals(
					"Context Menu should not be null",
					null, registriesTreeView.ContextMenu);
			}
		}

		public void TestContextMenuHasCopyRegistryPathToClipboardOption()
		{
			var registry = new Mock<IRegistry>();
			using (var testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.Show();
				var registriesTreeView = testForm.GetRegistriesTreeView();

				AssertEquals(
					"Context Menu should have Copy Registry Path to Clipboard Item",
					true,
					registriesTreeView.ContextMenu.MenuItems.FindByText("Copy Registry Path to Clipboard").Visible);
			}
		}

		public void TestContextMenuHasDisabledCopyRegistryPathToClipboardOptionWhenNoSelectedNode()
		{
			var registry = new Mock<IRegistry>();
			using (var testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.Show();
				var registriesTreeView = testForm.GetRegistriesTreeView();

				AssertEquals(
					"Context Menu should have disabled Copy Registry Path to Clipboard Item",
					false,
					registriesTreeView.ContextMenu.MenuItems.FindByText("Copy Registry Path to Clipboard").Enabled);
			}
		}

		public void TestAddSelectedRegistryItemPathToClipboardWhenNoNodeSelected()
		{
			var registry = new Mock<IRegistry>();
			var mockRegistryItemsTreeView = getMockRegistryItemsTreeView(registry.Object);
			using (var testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.SetRegistryTreeView(mockRegistryItemsTreeView.Object);
				testForm.Show();

				var menuItem = mockRegistryItemsTreeView.Object.ContextMenu.MenuItems.FindByText("Copy Registry Path to Clipboard");
				menuItem.PerformClick();

				AssertNoExceptionThrown(() =>
				{
					mockRegistryItemsTreeView.Verify(view => view.AddTextToClipboard(It.IsAny<string>()), Times.Never);
				});
			}
			mockRegistryItemsTreeView.Object.Dispose();
		}

		public void TestAddSelectedRegistryItemPathToClipboardForRootNode()
		{
			var registry = new Mock<IRegistry>();
			var mockRegistryItemsTreeView = getMockRegistryItemsTreeView(registry.Object);
			using (var testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.SetRegistryTreeView(mockRegistryItemsTreeView.Object);
				testForm.Show();
				var rootNode = new TreeNode("Root Node");
				mockRegistryItemsTreeView.Object.Nodes.Add(rootNode);
				mockRegistryItemsTreeView.Object.SelectedNode = mockRegistryItemsTreeView.Object.Nodes[0];

				var menuItem = mockRegistryItemsTreeView.Object.ContextMenu.MenuItems.FindByText("Copy Registry Path to Clipboard");
				menuItem.PerformClick();

				AssertNoExceptionThrown(() =>
				{
					mockRegistryItemsTreeView.Verify(view => view.AddTextToClipboard(rootNode.Text));
				});
			}
			mockRegistryItemsTreeView.Object.Dispose();
		}

		public void TestAddSelectedRegistryItemPathToClipboardForLeafNode()
		{
			var registry = new Mock<IRegistry>();
			var mockRegistryItemsTreeView = getMockRegistryItemsTreeView(registry.Object);
			using (var testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.SetRegistryTreeView(mockRegistryItemsTreeView.Object);
				testForm.Show();
				var leafNode = new TreeNode("Leaf Node");
				var rootNode = new TreeNode("Root Node");
				rootNode.Nodes.Add(leafNode);
				mockRegistryItemsTreeView.Object.Nodes.Add(rootNode);
				mockRegistryItemsTreeView.Object.SelectedNode = leafNode;

				var menuItem = mockRegistryItemsTreeView.Object.ContextMenu.MenuItems.FindByText("Copy Registry Path to Clipboard");
				menuItem.PerformClick();

				AssertNoExceptionThrown(() =>
				{
					mockRegistryItemsTreeView.Verify(view =>
						view.AddTextToClipboard(string.Format("{0} > {1}", rootNode.Text, leafNode.Text)));
				});
			}
			mockRegistryItemsTreeView.Object.Dispose();
		}

		public void TestAddSelectedRegistryItemPathToClipboardForIntermediateNode()
		{
			var registry = new Mock<IRegistry>();
			var mockRegistryItemsTreeView = getMockRegistryItemsTreeView(registry.Object);
			using (var testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.SetRegistryTreeView(mockRegistryItemsTreeView.Object);
				testForm.Show();
				var leafNode = new TreeNode("Leaf Node");
				var intermediateNode = new TreeNode("Intermediate Node");
				intermediateNode.Nodes.Add(leafNode);
				var rootNode = new TreeNode("Root Node");
				rootNode.Nodes.Add(intermediateNode);
				mockRegistryItemsTreeView.Object.Nodes.Add(rootNode);
				mockRegistryItemsTreeView.Object.SelectedNode = intermediateNode;

				var menuItem = mockRegistryItemsTreeView.Object.ContextMenu.MenuItems.FindByText("Copy Registry Path to Clipboard");
				menuItem.PerformClick();

				AssertNoExceptionThrown(() =>
				{
					mockRegistryItemsTreeView.Verify(view =>
						view.AddTextToClipboard(string.Format("{0} > {1}", rootNode.Text, intermediateNode.Text)));
				});
			}
			mockRegistryItemsTreeView.Object.Dispose();
		}

		#endregion

		Mock<RegistryItemsTreeView> getMockRegistryItemsTreeView(IRegistry registry)
		{
			var businessObjectFactory = new Mock<BusinessObjectFactory>(MockBehavior.Default);
			var mockGlbCompanyCollection = new Mock<GlbCompanyCollection>(businessObjectFactory.Object);
			var mockRegistryItemsTreeView = new Mock<RegistryItemsTreeView>(MockBehavior.Default) { CallBase = true };
			mockRegistryItemsTreeView.Object.Initialise(registry, mockGlbCompanyCollection.Object);
			return mockRegistryItemsTreeView;
		}
	}
}
