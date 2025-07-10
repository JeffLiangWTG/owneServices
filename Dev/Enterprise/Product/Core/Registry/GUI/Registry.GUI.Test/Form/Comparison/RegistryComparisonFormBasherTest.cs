using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryComparisonForm))]
	sealed class RegistryComparisonFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		StringRegistryItem NewItem(string code = null, string catagory = "My/Node/Catagory")
		{
			return new StringRegistryItem(code ?? ZGuid.NewZGuid().ToString(), (NoResString)catagory, (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");
		}

		protected override Form GetFormToBashCore()
		{
			var itemsWithOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();
			var itemsWithoutOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();

			var bizo = new RegistryComparisonBusinessObject(itemsWithOverrides.Concat(itemsWithoutOverrides), new DummyOverrideLevel(itemsWithOverrides), new DefaultOverrideLevel());
			return new RegistryComparisonForm(bizo);
		}

		#endregion

		#region Test check boxes

		[RequiresSTA]
		public void TestAllItemsAreCheckedInitially()
		{
			var items = new[]
			{
				NewItem("First", "A/A"),
				NewItem("Second", "A/A/A"),
				NewItem("Third", "A/B"),
				NewItem("Fourth", "B/A"),
				NewItem("Fifth", "B/B"),
				NewItem("Sixth", "B/B"),
			};

			var defaultLevel = new DefaultOverrideLevel();
			var overrideLevel = new SystemOverrideLevel();

			items.ForEach(item => overrideLevel.SetValueOf(item, "An overriden value"));

			var bizo = new RegistryComparisonBusinessObject(items, defaultLevel, overrideLevel);

			using (var form = new RegistryComparisonForm(bizo))
			{
				Assert("All nodes should be checked", AllChildren(form.treeView.Nodes).All(treeNode => treeNode.Checked));
			}
		}

		public void TestUncheckingAParentUpdatesTheChildren()
		{
			var items = new[]
			{
				NewItem("First", "A/A"),
				NewItem("Second", "A/A/A"),
				NewItem("Third", "A/B"),
				NewItem("Fourth", "B/A"),
				NewItem("Fifth", "B/B"),
				NewItem("Sixth", "B/B"),
				NewItem("Seventh", "C/A"),
			};

			var defaultLevel = new DefaultOverrideLevel();
			var overrideLevel = new SystemOverrideLevel();

			items.ForEach(item => overrideLevel.SetValueOf(item, "An overriden value"));

			var bizo = new RegistryComparisonBusinessObject(items, defaultLevel, overrideLevel);

			using (var form = new RegistryComparisonForm(bizo))
			{
				var root = form.treeView.Nodes;

				Assert("All nodes should be checked", AllChildren(root).All(treeNode => treeNode.Checked));

				var node = root["A"];
				node.Checked = false;

				Assert("The children should be updated to be unchecked as well to match the parent", AllChildren(node.Nodes).All(child => !child.Checked));
				Assert("Neighbors are unnaffected", AllChildren(root["B"].Nodes).All(child => child.Checked));

				var nodeb = node.Nodes["A"];
				nodeb.Checked = true;

				Assert("The children should be updated to be unchecked as well to match the parent", AllChildren(nodeb.Nodes).All(child => child.Checked));
				Assert("Neighbors are unnaffected", AllChildren(node.Nodes["B"].Nodes).All(child => !child.Checked));
			}
		}

		IEnumerable<TreeNode> AllChildren(TreeNodeCollection collection)
		{
			foreach (TreeNode node in collection)
			{
				yield return node;
				foreach (var innerNode in AllChildren(node.Nodes))
				{
					yield return innerNode;
				}
			}
		}

		#endregion

		#region Test GUI populates

		public void TestGetCheckedItems()
		{
			var items = new[]
			{
				NewItem("First", "A/A"),
				NewItem("Second", "A/A/A"),
				NewItem("Third", "A/B"),
				NewItem("Fourth", "B/A"),
				NewItem("Fifth", "B/B"),
				NewItem("Sixth", "B/B"),
				NewItem("Seventh", "C/A"),
			};

			var defaultLevel = new DefaultOverrideLevel();
			var overrideLevel = new SystemOverrideLevel();

			items.ForEach(item => overrideLevel.SetValueOf(item, "An overriden value"));

			var bizo = new RegistryComparisonBusinessObject(items, defaultLevel, overrideLevel);

			using (var form = new RegistryComparisonForm(bizo))
			{
				var root = form.treeView.Nodes;
				GetNode(root, "A/A").Checked = false; // UNCHECK First, Second
				GetNode(root, "B/A/Fourth").Checked = false; // Uncheck Fourth

				var actualCheckedItems = form.CheckedRegistryItems.Select(item => item.Name);
				var expected = new[] { "Third", "Fifth", "Sixth", "Seventh" };

				AssertContainsExactElementsInAnyOrder(expected, actualCheckedItems);
			}
		}

		public void TestBaseAndOverrideLabelsAreGivenNames()
		{
			var baseLevel = new DefaultOverrideLevel();
			var overrideLevel = new SystemOverrideLevel();

			var diffBizo = new RegistryComparisonBusinessObject(new[] { NewItem(), NewItem() }, baseLevel, overrideLevel);

			using (var form = new RegistryComparisonForm(diffBizo))
			{
				AssertEquals(string.Format("Value for {0} (Base)", baseLevel.Description), GetCaption(form.baseLevelPanel));
				AssertEquals(string.Format("Value for {0} (Override)", overrideLevel.Description), GetCaption(form.overrideLevelPanel));
			}
		}

		string GetCaption(Control c)
		{
			return c.GetExtension<ILabelCaptionRenderer>().Caption;
		}

		public void TestGroupBoxsArePopulated()
		{
			var item = NewItem("MAHCODE");
			var baseLevel = new DefaultOverrideLevel();
			var overrideLevel = new SystemOverrideLevel();

			overrideLevel.SetValueOf(item, "Override");

			var bizo = new RegistryComparisonBusinessObject(new[] { item }, baseLevel, overrideLevel);
			using (var form = new RegistryComparisonForm(bizo))
			{
				var node = form.treeView.Nodes.Find(item.Name, true)[0];
				form.treeView.PerformSelect(node);

				AssertPluginPaneIsCorrect("Override: ", form.overrideLevelPanel, overrideLevel, item);
				AssertPluginPaneIsCorrect("Base: ", form.baseLevelPanel, baseLevel, item);
			}
		}

		void AssertPluginPaneIsCorrect(string prefix, ZGroupBox parent, IOverrideLevel level, IRegistryItem item)
		{
			var editor = RegistryItemEditorFactory.NewEditor(item, level.GetFallbackLevel(), item.DataType, item.EditorInfo, Factory);
			var expectedType = GetPluginControlType(editor);

			Control control;
			Assert(prefix + "Should be in parent control", TryGetPluginControl(parent, out control));
			AssertEquals(prefix + "Control should be of correct type", expectedType, control.GetType());
			Assert(prefix + "Should be read only", control.GetReadOnly());
			AssertEquals(prefix + "Should have the value set", level.GetValueOf(item), editor.GetValueFromEditorPane(control));
		}

		Type GetPluginControlType(RegistryItemEditor editor)
		{
			using (var control = editor.NewWinFormsEditorPane())
			{
				return control.GetType();
			}
		}

		bool TryGetPluginControl(ZGroupBox parent, out Control pluginControl)
		{
			pluginControl = parent.Controls[RegistryComparisonForm.ActivePluginControlName];
			return pluginControl != null;
		}

		#endregion

		[RequiresSTA]
		public void TestTreeIsPopulated()
		{
			var itemsWithOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();
			var itemsWithoutOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();

			var random = new Random();
			var shuffledItems = itemsWithOverrides.Concat(itemsWithoutOverrides).OrderBy(_ => random.Next(-1, 2)).ToList();

			var bizo = new RegistryComparisonBusinessObject(shuffledItems, new DummyOverrideLevel(itemsWithOverrides), new DefaultOverrideLevel());
			using (var form = new RegistryComparisonForm(bizo))
			{
				CombineAssertions(() =>
				{
					foreach (var itemPath in itemsWithOverrides.SelectMany(PathsFor))
					{
						TreeViewAssertion.AssertHasPath("root", form.treeView.Nodes, itemPath);
					}

					foreach (var itemPath in itemsWithoutOverrides.SelectMany(PathsFor))
					{
						TreeViewAssertion.AssertDoesNotHavePath(form.treeView.Nodes, itemPath);
					}
				});
			}
		}

		IEnumerable<string> PathsFor(IRegistryItem item)
		{
			return item.Categories.Select(catagory => catagory + '/' + item.Name);
		}

		TreeNode GetNode(TreeNodeCollection root, string path)
		{
			var paths = path.Split('/');
			var parent = paths.Take(paths.Length - 1).Aggregate(root, (nodeCollection, childName) => nodeCollection[childName].Nodes);

			return parent[paths.Last()];
		}
	}
}
