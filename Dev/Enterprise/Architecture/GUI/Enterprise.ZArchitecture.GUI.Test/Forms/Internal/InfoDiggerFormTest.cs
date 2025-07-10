using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI.Forms.Internal;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.Testing.TreeFindFormTest;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR
	[TestedType(typeof(InfoDiggerForm))]
	sealed class InfoDiggerFormTest : ZFormBasherTest
	{
		Form infoDiggerWithControl;
		KForm controlToDig;

		protected override Form GetFormToBashCore()
		{
			controlToDig = GetControlsToDigTest(out var textbox);
			infoDiggerWithControl = new InfoDiggerForm(textbox);
			return infoDiggerWithControl;
		}

		protected override void TearDown()
		{
			controlToDig?.Dispose();
			infoDiggerWithControl?.Dispose();
			base.TearDown();
		}

		#region SuppressResourceStringsCheckRegion

		KForm GetControlsToDigTest(out KTextBox textbox)
		{
			var form = new KForm();
			textbox = new KTextBox { Name = "Sango", Location = ControlDpiScalingHelper.NewScaledPoint(10, 10) };
			var textbox2 = new KTextBox { Name = "Sangohan", Location = ControlDpiScalingHelper.NewScaledPoint(25, 10) };
			var button = new KButton { Name = "", Location = ControlDpiScalingHelper.NewScaledPoint(39, 10) };

			form.Controls.Add(textbox);
			form.Controls.Add(textbox2);
			form.Controls.Add(button);
			form.Show();

			return form;
		}

		public void TestFormInfoDigger()
		{
			using (var form = GetControlsToDigTest(out var textbox))
			using (var infoDigger = new InfoDiggerForm(textbox))
			{
				AssertEquals(typeof(KTextBox), infoDigger.ControlToDig.GetType());
			}
		}

		public void TestFormInfoDiggerIncludesFormAsParent()
		{
			using (var form = GetControlsToDigTest(out var textbox))
			using (var infoDigger = new InfoDiggerForm(textbox))
			{
				infoDigger.Show();

				var treeView = (ZTreeView)infoDigger.Controls.Find("zTreeView1", true).First();
				AssertEquals("First Node is the Form", form, treeView.TopNode.Tag);
			}
		}

		public void TestOverlayAppliedOnNodeSelection()
		{
			using (var form = GetControlsToDigTest(out var textbox))
			using (var infoDigger = new InfoDiggerForm(textbox))
			{
				infoDigger.Show();

				var treeNodeControls = (ZTreeView)infoDigger.Controls.Find("zTreeView1", true).First();

				var firstNode = treeNodeControls.Nodes[0];
				infoDigger.SelectNode(firstNode);

				Assert("Overlay has been applied to the control that is selected", infoDigger.OverlayForm.HighlightedControl == firstNode.Tag);

				var secondNode = treeNodeControls.TopNode.Nodes[1];
				infoDigger.SelectNode(secondNode);

				Assert("Overlay has been changed to newly selected control", infoDigger.OverlayForm.HighlightedControl == secondNode.Tag);
			}
		}

		[DeveloperOnlyTest]
		public void TestOverlayHiddenOnDispose()
		{
			using (var form = GetControlsToDigTest(out var textbox))
			{
				using (var infoDigger = new InfoDiggerForm(textbox))
				{
					infoDigger.Show();
					var treeNodeControls = (ZTreeView)infoDigger.Controls.Find("zTreeView1", true).First();
					var firstNode = treeNodeControls.Nodes[0];
					infoDigger.SelectNode(firstNode);
				}

				Application.DoEvents();
				Application.DoEvents();
				Application.DoEvents();

				Assert("Overlay is hidden when infoDigger is closed", !form.Overlay.Visible);
			}
		}

		public void TestPopulateTreeNodeAndPropertyGrid()
		{
			using (var form = GetControlsToDigTest(out var textbox)) //These values are probs the reason for failing
			using (var infoDigger = new InfoDiggerForm(textbox))
			{
				infoDigger.Show();

				var treeView = (ZTreeView)infoDigger.Controls.Find("zTreeView1", true).First();
				Assert(treeView.Nodes.Count > 0);

				var propertiesGridView = (ZGrid)infoDigger.Controls.Find("propertiesGridView", true).First();
				Assert("The Grid view contains control's properties", propertiesGridView.List.Count > 0);

				AssertEquals("Sango", propertiesGridView.List.Cast<ControlProperty>().Where(p => p.Name == "Name").Select(x => x.Value).First().ToString());

				infoDigger.SelectNode(treeView.TopNode.Nodes[1]);
				AssertEquals("Sangohan", propertiesGridView.List.Cast<ControlProperty>().Where(p => p.Name == "Name").Select(x => x.Value).First().ToString());

				infoDigger.SelectNode(treeView.TopNode.Nodes[2]);
				AssertEquals("", propertiesGridView.List.Cast<ControlProperty>().Where(p => p.Name == "Name").Select(x => x.Value).First().ToString());
				AssertEquals("If name is empty <Unnamed> should be replace it.", "<Unnamed> - [KButton]", treeView.TopNode.Nodes[2].Text);
			}
		}

		public void TestFindNextPopulatesTreeNodeDynamically()
		{
			using (var form = new KForm())
			{
				var outerPanel = new KPanel { Name = "Allison Wonderland - Cry", Location = ControlDpiScalingHelper.NewScaledPoint(42, 10) };
				var innerPanel = new KPanel { Name = "Common - Be", Location = ControlDpiScalingHelper.NewScaledPoint(42, 10) };
				var textbox3 = new KTextBox { Name = "Chance - Intro", Location = ControlDpiScalingHelper.NewScaledPoint(56, 10) };
				var textbox4 = new KTextBox { Name = "Mos Def - Mathematics", Location = ControlDpiScalingHelper.NewScaledPoint(63, 10) };
				innerPanel.Controls.Add(textbox3);
				innerPanel.Controls.Add(textbox4);
				outerPanel.Controls.Add(innerPanel);

				form.Controls.Add(outerPanel);
				form.Show();

				using (var infoDigger = new InfoDiggerForm(outerPanel))
				{
					infoDigger.Show();

					var treeView = (ZTreeView)infoDigger.Controls.Find("zTreeView1", true).First();
					var treeViewSearcher = new NonRecursiveTreeViewSearcher();
					treeViewSearcher.BeforeIteratingChildren += (s, e) => Populate(e.Node);
					treeView.TreeViewSearcher = treeViewSearcher;

					using (var findForm = new TreeViewFindFormForTest(treeView))
					{
						AssertEquals("PRE: Before populating, the only child node is our placeholder", "Fake child", treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Cast<TreeNode>().Single().Text);

						var findTextBox = findForm.GetFindTextBox();
						var findNextButton = findForm.GetFindNextButton();

						findTextBox.Text = "Chance";
						findForm.StartFindForTest(true);

						AssertEquals("Tree node is populated when Find Next is used on node that has children", 2, treeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Count);
					}
				}
			}
		}

		void Populate(TreeNodeCollection root, Control c)
		{
			root.Clear();
			foreach (Control child in c.Controls)
			{
				var nodeDisplayedName = (string.IsNullOrEmpty(child.Name) ? "<Unnamed>" : child.Name) + " - [" + child.GetType().Name + "]"; // NoTranslationNeeded
				var tempsChildNode = new TreeNode(nodeDisplayedName) { Tag = child };
				if (child.Controls.Count > 0)
				{
					tempsChildNode.Nodes.Add(new TreeNode("Fake Child"));
				}

				root.Add(tempsChildNode);
			}
		}

		void Populate(TreeNode node)
			=> Populate(node.Nodes, (Control)node.Tag);

		#endregion
	}
#endif
}
