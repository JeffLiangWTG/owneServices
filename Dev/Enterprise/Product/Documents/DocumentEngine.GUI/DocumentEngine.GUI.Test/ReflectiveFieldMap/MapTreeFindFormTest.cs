using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentEngine.ReflectiveFieldMap.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.Testing
{
	[TestedType(typeof(MapTreeFindForm))]
	sealed class MapTreeFindFormTest : ZFormBasherTest
	{
		public class MapTreeFindFormForTest : MapTreeFindForm
		{
			public MapTreeFindFormForTest(KTreeView treeView, MapTreeUserControl mapTree) : base(treeView, mapTree)
			{
			}

			public ZButton GetNextValueForTest()
			{
				return FindNextButton;
			}
			public ZButton GetPreviousValueForTest()
			{
				return FindPreviousButton;
			}
			public TextBox GetFindTextBoxForTest()
			{
				return FindTextBox;
			}
			public MapTreeNode GetFindNext()
			{
				return GiveMeNode(1);
			}
			public List<MapTreeNode> GetlistFoundNodes()
			{
				return matchingNotNodes.Select(x => x.MapTreeNode).ToList();
			}
			public MapTreeNode GetGoBackward()
			{
				return GiveMeNode(-1);
			}
		}

		public void TestFindNextExactly()
		{
			var lastSelectedNode = testTreeView.SelectedNode;
			testForm.FindExactly(" ");
			AssertEquals("Selected Node", lastSelectedNode, testTreeView.SelectedNode);

			testForm.FindExactly("CollectionWithCustomProperties");
			AssertEquals("Selected Node", "CollectionWithCustomProperties (DocumentWrapperCollectionWithCustomPropertiesForTest)", testTreeView.SelectedNode.Text);

			testForm.FindExactly("CollectionWithCustomProperties.CustomZStringInBO2");
			AssertEquals("Selected Node", "CustomZStringInBO2 (ZString)", testTreeView.SelectedNode.Text);
			AssertEquals("Selected Node", "CollectionWithCustomProperties (DocumentWrapperCollectionWithCustomPropertiesForTest)", testTreeView.SelectedNode.Parent.Text);

			testForm.FindExactly("CollectionWithCustomProperties.BO2");
			AssertEquals("Selected Node", "CustomZStringInBO2 (ZString)", testTreeView.SelectedNode.Text);
			AssertEquals("Selected Node", "CollectionWithCustomProperties (DocumentWrapperCollectionWithCustomPropertiesForTest)", testTreeView.SelectedNode.Parent.Text);

			testForm.FindExactly("DocDataProviderIsIn.AddressForSendingAPDocuments.Country");
			AssertEquals("Selected Node", "Country (RefCountry)", testTreeView.SelectedNode.Text);
			AssertEquals("Selected Node", "AddressForSendingAPDocuments (OrgAddress)", testTreeView.SelectedNode.Parent.Text);
			AssertEquals("Selected Node", "DocDataProviderIsIn (OrgHeader)", testTreeView.SelectedNode.Parent.Parent.Text);
		}

		#region Test with TreeNode

		//currently TreeNode isn't supported.
		/*public void TestFindNextWithBaseTreeNode()
		{
			var treeViewWithTreeNodes = PopulateTestTreeViewWithTreeNodes();

			using (var testForm = new MapTreeFindFormForTest(treeViewWithTreeNodes, new MapTreeUserControl()))
			{
				var findTextBox = testForm.GetFindTextBoxForTest();
				findTextBox.Text = "Sango";

				testForm.GetFindNext();
				AssertEquals("Selected Node", "Sango 2", treeViewWithTreeNodes.SelectedNode.Text);

				testForm.GetFindNext();
				AssertEquals("Selected Node", "Sangohan (ZString)", treeViewWithTreeNodes.SelectedNode.Text);

				testForm.GetFindNext();
				AssertEquals("Selected Node", "Sango (ZString)", treeViewWithTreeNodes.SelectedNode.Text);
			}
		}*/

		#endregion

		public void TestTextBoxChanged()
		{
			var findTextBox = testForm.GetFindTextBoxForTest();
			findTextBox.Text = "ZStringIsIn";
			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn (ZString)", testTreeView.SelectedNode.Text);
			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn1 (ZString)", testTreeView.SelectedNode.Text);

			findTextBox.Text = "IsIn";
			testForm.GetFindNext();
			AssertEquals("Selected Node", "DocDataProviderIsIn (OrgHeader)", testTreeView.SelectedNode.Text);
			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn (ZString)", testTreeView.SelectedNode.Text);
		}

		public void TestFindPreviousAndNextValue()
		{
			var findTextBox = testForm.GetFindTextBoxForTest();
			findTextBox.Text = "IsIn";
			testForm.GetFindNext();
			AssertEquals("Selected Node", "DocDataProviderIsIn (OrgHeader)", testTreeView.SelectedNode.Text);
			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn (ZString)", testTreeView.SelectedNode.Text);
			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn1 (ZString)", testTreeView.SelectedNode.Text);

			testTreeView.SelectedNode = testForm.GetGoBackward();
			AssertEquals("Selected Node", "ZStringIsIn (ZString)", testTreeView.SelectedNode.Text);
			testTreeView.SelectedNode = testForm.GetGoBackward();
			AssertEquals("Selected Node", "DocDataProviderIsIn (OrgHeader)", testTreeView.SelectedNode.Text);

			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn (ZString)", testTreeView.SelectedNode.Text);
			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn1 (ZString)", testTreeView.SelectedNode.Text);
			testForm.GetFindNext();
			AssertEquals("Selected Node", "ZStringIsIn2 (ZString)", testTreeView.SelectedNode.Text);
		}

		public void TestFindPreviousAfterNextValueWithoutResults()
		{
			// Arrange
			var findTextBox = testForm.GetFindTextBoxForTest();
			findTextBox.Text = "Something That Never Exists";
			testForm.GetFindNext();
			AssertEquals("Should find nothing.", null, testTreeView.SelectedNode);
			// Act & Assert
			AssertNoExceptionThrown("Should be OK when clicking go backword.", () => testForm.GetGoBackward());
		}

		//currently lacking 'find next node starting from a specific node' function
		/*public void TestFindNextValueFromSelectedNode()
		{
			var userMapTreeUserControl = new MapTreeUserControl();

			using (var testForm = new MapTreeFindFormForTest(testTreeView, userMapTreeUserControl))
			{
				var findTextBox = testForm.GetFindTextBoxForTest();
				findTextBox.Text = "Sango";
				testForm.GetFindNext();
				AssertEquals("Selected Node", "Sango (ZString)", testTreeView.SelectedNode.Text);
				testForm.GetFindNext();
				AssertEquals("Selected Node", "Sangoku (ZString)", testTreeView.SelectedNode.Text);
				testForm.GetFindNext();
				AssertEquals("Selected Node", "Sangohan (ZString)", testTreeView.SelectedNode.Text);

				userMapTreeUserControl.LoadRootNodeFindFrom = true;
				userMapTreeUserControl.SelectedTreeNode = testTreeView.Nodes.Cast<MapTreeNode>().FirstOrDefault(n => n.Text.Contains(Address));

				testForm.GetFindNext();
				AssertEquals("Selected Node", "Sangoten (ZString)", testTreeView.SelectedNode.Text);
			}
		}*/

		public KTreeView testTreeView;
		MapTreeUserControl userControl;
		MapTreeFindFormForTest testForm;

		protected override void SetUp()
		{
			base.SetUp();
			testForm = (MapTreeFindFormForTest)GetFormToBashCore();
		}
		protected override void TearDown()
		{
			stuffToDispose.Dispose();
			base.TearDown();
		}

		public override Type FormToBashType
		{
			get { return typeof(MapTreeFindForm); }
		}
		protected override Form GetFormToBashCore()
		{
			userControl = new MapTreeUserControl();
			userControl.SetReflectors([new DocDataProviderReflector(typeof(DocumentWrapperForTest))]);
			testTreeView = userControl.mapTreeView;
			var result = new MapTreeFindFormForTest(testTreeView, userControl);
			stuffToDispose.Add(userControl);
			stuffToDispose.Add(testTreeView);
			stuffToDispose.Add(result);
			return result;
		}

		readonly DisposableList stuffToDispose = new DisposableList(2);
	}
}
