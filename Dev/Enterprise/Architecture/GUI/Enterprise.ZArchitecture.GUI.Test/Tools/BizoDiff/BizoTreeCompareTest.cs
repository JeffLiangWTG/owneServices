using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DevTools;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class BizoTreeCompareTest : TestCaseWithFactory
	{
		public void TestCompare_When2BizoAreSame()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions(() =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
				AssertEquals("Root", sourceTree.Text);
				AssertEquals("Root", targetTree.Text);
			});
		}
		public void TestCompare_When2BizoAreSame_ButExcluded()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			sourceTree.ForeColor = Color.Gray;
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			targetTree.ForeColor = Color.Gray;

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions(() =>
			{
				AssertEquals(Color.Gray, sourceTree.ForeColor);
				AssertEquals(Color.Gray, targetTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentGuid()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Guid = System.Guid.NewGuid();
			objSource.Z0_Number = 1;
			objTarget.Z0_Guid = System.Guid.NewGuid();
			objTarget.Z0_Number = 1;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions(() =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentNumber_TextUpdates()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 2;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions(() =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in value", sourceTree.Text);
				AssertEquals("Root || Difference in value", targetTree.Text);
			});
		}

		public void TestCompare_When2BizoHavingSameChild()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			var objTargetChild = Factory.New<DummyBusinessObject>();

			objSourceChild.Z0_Number = 1;
			objTargetChild.Z0_Number = 1;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceChild;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetChild;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions(() =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
			});

			CombineAssertions(() =>
			{
				AssertEquals(Color.Green, sourceChildTree.ForeColor);
				AssertEquals(Color.Green, targetChildTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentChild_TextUpdates()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			var objTargetChild = Factory.New<DummyBusinessObject>();

			objSourceChild.Z0_Number = 1;
			objTargetChild.Z0_Number = 2;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceChild;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetChild;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions(() =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in children || Children count: 1", sourceTree.Text);
				AssertEquals("Root || Difference in children || Children count: 1", targetTree.Text);
			});

			CombineAssertions(() =>
			{
				AssertEquals(Color.Red, sourceChildTree.ForeColor);
				AssertEquals(Color.Red, targetChildTree.ForeColor);
				AssertEquals("Child || Difference in value", sourceChildTree.Text);
				AssertEquals("Child || Difference in value", targetChildTree.Text);
			});
		}

		public void TestCompare_When2BizoHavingDifferentChild_ButExcluded()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			var objTargetChild = Factory.New<DummyBusinessObject>();

			objSourceChild.Z0_Number = 1;
			objTargetChild.Z0_Number = 2;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceChild;
			sourceChildTree.ForeColor = Color.Gray;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetChild;
			targetChildTree.ForeColor = Color.Gray;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Gray, sourceChildTree.ForeColor);
				AssertEquals(Color.Gray, targetChildTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingSameAndDifferentChild()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			var objSourceChild2 = Factory.New<DummyBusinessObject>();
			var objTargetChild = Factory.New<DummyBusinessObject>();
			var objTargetChild2 = Factory.New<DummyBusinessObject>();

			objSourceChild.Z0_Number = 1;
			objSourceChild2.Z0_Number = 1;
			objTargetChild.Z0_Number = 1;
			objTargetChild2.Z0_Number = 2;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceChild;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildTree2 = new TreeNode("Child");
			sourceChildTree2.Tag = objSourceChild2;
			sourceTree.Nodes.Add(sourceChildTree2);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetChild;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildTree2 = new TreeNode("Child");
			targetChildTree2.Tag = objTargetChild2;
			targetTree.Nodes.Add(targetChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Orange, sourceChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree.ForeColor);
				AssertEquals(Color.Orange, sourceChildTree2.ForeColor);
				AssertEquals(Color.Black, targetChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentCollection()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			var objSourceCollection = new DummyBusinessObjectCollection(Factory) { objSourceChild };

			var objTargetCollection = new DummyBusinessObjectCollection(Factory);

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceCollection;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildChildTree = new TreeNode("Child");
			sourceChildChildTree.Tag = objSourceChild;
			sourceChildTree.Nodes.Add(sourceChildChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetCollection;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Red, sourceChildTree.ForeColor);
				AssertEquals(Color.Red, targetChildTree.ForeColor);
				AssertEquals(Color.Black, sourceChildChildTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentCollection_ButExcluded()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			var objSourceCollection = new DummyBusinessObjectCollection(Factory) { objSourceChild };

			var objTargetCollection = new DummyBusinessObjectCollection(Factory);

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceCollection;
			sourceChildTree.ForeColor = Color.Gray;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildChildTree = new TreeNode("Child");
			sourceChildChildTree.Tag = objSourceChild;
			sourceChildTree.Nodes.Add(sourceChildChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetCollection;
			targetChildTree.ForeColor = Color.Gray;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Gray, sourceChildTree.ForeColor);
				AssertEquals(Color.Gray, targetChildTree.ForeColor);
				AssertEquals(Color.Black, sourceChildChildTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentCollectionButHaveTypeMatcher_CanFoundSingleMatch()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			objSourceChild.Z0_Number = 1;
			var objSourceChild2 = Factory.New<DummyBusinessObject>();
			objSourceChild2.Z0_Number = 2;
			var objSourceCollection = new DummyBusinessObjectCollection(Factory) { objSourceChild, objSourceChild2 };

			var targetChild = Factory.New<DummyBusinessObject>();
			targetChild.Z0_Number = 1;
			var objTargetCollection = new DummyBusinessObjectCollection(Factory) { targetChild };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceCollection;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildChildTree = new TreeNode("Child");
			sourceChildChildTree.Tag = objSourceChild;
			sourceChildTree.Nodes.Add(sourceChildChildTree);
			var sourceChildChildTree2 = new TreeNode("Child");
			sourceChildChildTree2.Tag = objSourceChild2;
			sourceChildTree.Nodes.Add(sourceChildChildTree2);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetCollection;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildChildTree = new TreeNode("Child");
			targetChildChildTree.Tag = targetChild;
			targetChildTree.Nodes.Add(targetChildChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in children || Children count: 1", sourceTree.Text);
				AssertEquals("Root || Difference in children || Children count: 1", targetTree.Text);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Red, sourceChildTree.ForeColor);
				AssertEquals(Color.Red, targetChildTree.ForeColor);
				AssertEquals("Child || Difference in comparable children || Children count: 2", sourceChildTree.Text);
				AssertEquals("Child || Difference in comparable children || Children count: 1", targetChildTree.Text);
				AssertEquals(Color.Green, sourceChildChildTree.ForeColor);
				AssertEquals(Color.Orange, sourceChildChildTree2.ForeColor);
				AssertEquals(Color.Green, targetChildChildTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentCollectionButHaveTypeMatcher_CanFoundSingleMatch_Case2()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			objSourceChild.Z0_Number = 1;
			var objSourceCollection = new DummyBusinessObjectCollection(Factory) { objSourceChild };

			var targetChild = Factory.New<DummyBusinessObject>();
			targetChild.Z0_Number = 1;
			var targetChild2 = Factory.New<DummyBusinessObject>();
			targetChild2.Z0_Number = 2;
			var objTargetCollection = new DummyBusinessObjectCollection(Factory) { targetChild, targetChild2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceCollection;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildChildTree = new TreeNode("Child");
			sourceChildChildTree.Tag = objSourceChild;
			sourceChildTree.Nodes.Add(sourceChildChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetCollection;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildChildTree = new TreeNode("Child");
			targetChildChildTree.Tag = targetChild;
			targetChildTree.Nodes.Add(targetChildChildTree);
			var targetChildChildTree2 = new TreeNode("Child");
			targetChildChildTree2.Tag = targetChild2;
			targetChildTree.Nodes.Add(targetChildChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();

			CombineAssertions(() =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in children || Children count: 1", sourceTree.Text);
				AssertEquals("Root || Difference in children || Children count: 1", targetTree.Text);
			});

			CombineAssertions(() =>
			{
				AssertEquals(Color.Red, sourceChildTree.ForeColor);
				AssertEquals(Color.Red, targetChildTree.ForeColor);
				AssertEquals("Child || Difference in comparable children || Children count: 1", sourceChildTree.Text);
				AssertEquals("Child || Difference in comparable children || Children count: 2", targetChildTree.Text);
				AssertEquals(Color.Green, sourceChildChildTree.ForeColor);
				AssertEquals(Color.Green, targetChildChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentCollectionButHaveTypeMatcher_CanFoundZeroMatch()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			objSourceChild.Z0_Number = 1;
			var objSourceCollection = new DummyBusinessObjectCollection(Factory) { objSourceChild };

			var objTargetCollection = new DummyBusinessObjectCollection(Factory) { };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceCollection;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildChildTree = new TreeNode("Child");
			sourceChildChildTree.Tag = objSourceChild;
			sourceChildTree.Nodes.Add(sourceChildChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetCollection;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();
			CombineAssertions(() =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in children || Children count: 1", sourceTree.Text);
				AssertEquals("Root || Difference in children || Children count: 1", targetTree.Text);
			});

			CombineAssertions(() =>
			{
				AssertEquals(Color.Red, sourceChildTree.ForeColor);
				AssertEquals(Color.Red, targetChildTree.ForeColor);
				AssertEquals("Child || Difference in comparable children || Children count: 1", sourceChildTree.Text);
				AssertEquals("Child || Difference in comparable children", targetChildTree.Text);
				AssertEquals(Color.Orange, sourceChildChildTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentCollectionButHaveTypeMatcher_CanFoundMulitpleMatches()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 1;

			var objSourceChild = Factory.New<DummyBusinessObject>();
			objSourceChild.Z0_Number = 1;
			var objSourceCollection = new DummyBusinessObjectCollection(Factory) { objSourceChild };

			var targetChild = Factory.New<DummyBusinessObject>();
			targetChild.Z0_Number = 1;
			var targetChild2 = Factory.New<DummyBusinessObject>();
			targetChild2.Z0_Number = 1;
			var objTargetCollection = new DummyBusinessObjectCollection(Factory) { targetChild, targetChild2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSourceCollection;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildChildTree = new TreeNode("Child");
			sourceChildChildTree.Tag = objSourceChild;
			sourceChildTree.Nodes.Add(sourceChildChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTargetCollection;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildChildTree = new TreeNode("Child");
			targetChildChildTree.Tag = targetChild;
			targetChildTree.Nodes.Add(targetChildChildTree);
			var targetChildChildTree2 = new TreeNode("Child");
			targetChildChildTree2.Tag = targetChild2;
			targetChildTree.Nodes.Add(targetChildChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Red, sourceChildTree.ForeColor);
				AssertEquals(Color.Red, targetChildTree.ForeColor);
				AssertEquals(Color.Orange, sourceChildChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingDifferentCount()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();
			var objTarget2 = Factory.New<DummyBusinessObject>();

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget, objTarget2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildTree2 = new TreeNode("Child");
			targetChildTree2.Tag = objTarget2;
			targetTree.Nodes.Add(targetChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in comparable children || Children count: 1", sourceTree.Text);
				AssertEquals("Root || Difference in comparable children || Children count: 2", targetTree.Text);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Black, sourceChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingDifferentCount_ButExcluded()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();
			var objTarget2 = Factory.New<DummyBusinessObject>();

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget, objTarget2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildTree2 = new TreeNode("Child");
			targetChildTree2.Tag = objTarget2;
			targetChildTree2.ForeColor = Color.Gray;
			targetTree.Nodes.Add(targetChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Green, sourceChildTree.ForeColor);
				AssertEquals(Color.Green, targetChildTree.ForeColor);
				AssertEquals(Color.Gray, targetChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingSameCountButDiffrentChild()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			objSource.Z0_Number = 1;
			var objTarget = Factory.New<DummyBusinessObject>();
			objTarget.Z0_Number = 2;

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in children || Children count: 1", sourceTree.Text);
				AssertEquals("Root || Difference in children || Children count: 1", targetTree.Text);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Red, sourceChildTree.ForeColor);
				AssertEquals(Color.Red, targetChildTree.ForeColor);
				AssertEquals("Child || Difference in value", sourceChildTree.Text);
				AssertEquals("Child || Difference in value", targetChildTree.Text);
			});
		}

		public void TestCompare_When2BizoCollectionHavingSameCountSameChild()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			objSource.Z0_Number = 1;
			var objTarget = Factory.New<DummyBusinessObject>();
			objTarget.Z0_Number = 1;

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Green, sourceChildTree.ForeColor);
				AssertEquals(Color.Green, targetChildTree.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingDifferentCountButHaveTypeMatcher_CanFoundSingleMatch()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			objSource.Z0_Number = 1;
			var objTarget = Factory.New<DummyBusinessObject>();
			objTarget.Z0_Number = 1;
			var objTarget2 = Factory.New<DummyBusinessObject>();
			objTarget2.Z0_Number = 2;

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget, objTarget2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildTree2 = new TreeNode("Child");
			targetChildTree2.Tag = objTarget2;
			targetTree.Nodes.Add(targetChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, new Dictionary<Type, List<string>>());

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in comparable children || Children count: 1", sourceTree.Text);
				AssertEquals("Root || Difference in comparable children || Children count: 2", targetTree.Text);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Green, sourceChildTree.ForeColor);
				AssertEquals(Color.Green, targetChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingDifferentCountButHaveTypeMatcher_CanFoundSingleMatch_Case2()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			objSource.Z0_Number = 1;
			var objSource2 = Factory.New<DummyBusinessObject>();
			objSource2.Z0_Number = 2;
			var objTarget = Factory.New<DummyBusinessObject>();
			objTarget.Z0_Number = 1;

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource, objSource2 };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var sourceChildTree2 = new TreeNode("Child");
			sourceChildTree2.Tag = objSource2;
			sourceTree.Nodes.Add(sourceChildTree2);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("Root || Difference in comparable children || Children count: 2", sourceTree.Text);
				AssertEquals("Root || Difference in comparable children || Children count: 1", targetTree.Text);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Green, sourceChildTree.ForeColor);
				AssertEquals(Color.Green, targetChildTree.ForeColor);
				AssertEquals(Color.Orange, sourceChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingDifferentCountButHaveTypeMatcher_CanFoundZeroMatch()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			objSource.Z0_Number = 1;
			var objTarget = Factory.New<DummyBusinessObject>();
			objTarget.Z0_Number = 2;
			var objTarget2 = Factory.New<DummyBusinessObject>();
			objTarget2.Z0_Number = 3;

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget, objTarget2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildTree2 = new TreeNode("Child");
			targetChildTree2.Tag = objTarget2;
			targetTree.Nodes.Add(targetChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
				AssertEquals("source", "Root || Difference in comparable children || Children count: 1", sourceTree.Text);
				AssertEquals("target", "Root || Difference in comparable children || Children count: 2", targetTree.Text);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Orange, sourceChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingDifferentCountButHaveTypeMatcher_CanFoundMultipleMatches()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			objSource.Z0_Number = 1;
			var objTarget = Factory.New<DummyBusinessObject>();
			objTarget.Z0_Number = 1;
			var objTarget2 = Factory.New<DummyBusinessObject>();
			objTarget2.Z0_Number = 1;

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget, objTarget2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildTree2 = new TreeNode("Child");
			targetChildTree2.Tag = objTarget2;
			targetTree.Nodes.Add(targetChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Red, sourceTree.ForeColor);
				AssertEquals(Color.Red, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Orange, sourceChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree.ForeColor);
				AssertEquals(Color.Black, targetChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoCollectionHavingDifferentCountButHaveTypeMatcher_CanFoundMultipleMatches_ButExcludedOne()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			objSource.Z0_Number = 1;
			var objTarget = Factory.New<DummyBusinessObject>();
			objTarget.Z0_Number = 1;
			var objTarget2 = Factory.New<DummyBusinessObject>();
			objTarget2.Z0_Number = 1;

			var objCollectionSource = new DummyBusinessObjectCollection(Factory) { objSource };
			var objCollectionTarget = new DummyBusinessObjectCollection(Factory) { objTarget, objTarget2 };

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objCollectionSource;
			var sourceChildTree = new TreeNode("Child");
			sourceChildTree.Tag = objSource;
			sourceTree.Nodes.Add(sourceChildTree);
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objCollectionTarget;
			var targetChildTree = new TreeNode("Child");
			targetChildTree.Tag = objTarget;
			targetTree.Nodes.Add(targetChildTree);
			var targetChildTree2 = new TreeNode("Child");
			targetChildTree2.Tag = objTarget2;
			targetChildTree2.ForeColor = Color.Gray;
			targetTree.Nodes.Add(targetChildTree2);

			var columnProvider = new BizoDiffColumnProvider();
			var typeMatcher = new Dictionary<string, List<string>> { { "DummyBusinessObject", new List<string> { "Z0_Number" } } };
			var ignoreObjects = new Dictionary<Type, List<string>> { { objSource.GetType(), new List<string> { "Z0_Number" } } };
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, typeMatcher, ignoreObjects);

			comparer.Compare();

			CombineAssertions("Root Nodes", () =>
			{
				AssertEquals(Color.Green, sourceTree.ForeColor);
				AssertEquals(Color.Green, targetTree.ForeColor);
			});

			CombineAssertions("Child Nodes", () =>
			{
				AssertEquals(Color.Green, sourceChildTree.ForeColor);
				AssertEquals(Color.Green, targetChildTree.ForeColor);
				AssertEquals(Color.Gray, targetChildTree2.ForeColor);
			});
		}

		public void TestCompare_When2BizoHavingDifferentNumber_ButIgnore()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 2;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;

			var columnProvider = new BizoDiffColumnProvider();
			var testIgnoreFieldDictionary = new Dictionary<Type, List<string>>
			{
				{ objSource.GetType(), new List<string> { "Z0_Number" } }
			};
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), testIgnoreFieldDictionary);

			comparer.Compare();

			AssertEquals(Color.Green, sourceTree.ForeColor);
			AssertEquals(Color.Green, targetTree.ForeColor);
		}

		public void TestCompare_When2BizoHavingDifferentNumber_ButNotIgnore()
		{
			var objSource = Factory.New<DummyBusinessObject>();
			var objTarget = Factory.New<DummyBusinessObject>();

			objSource.Z0_Number = 1;
			objTarget.Z0_Number = 2;

			var sourceTree = new TreeNode("Root");
			sourceTree.Tag = objSource;
			var targetTree = new TreeNode("Root");
			targetTree.Tag = objTarget;

			var columnProvider = new BizoDiffColumnProvider();

			var testIgnoreFieldDictionary = new Dictionary<Type, List<string>>();
			var comparer = new BizoTreeCompare(sourceTree, targetTree, columnProvider, new Dictionary<string, List<string>>(), testIgnoreFieldDictionary);

			comparer.Compare();

			AssertEquals(Color.Red, sourceTree.ForeColor);
			AssertEquals(Color.Red, targetTree.ForeColor);
		}

		public void TestCompare_WhenObject1IgnoresName_Object2DoesNotIgnoreName_ShouldMarkObject1Green_Object2Red()
		{
			var objSource1 = Factory.New<DummyBusinessObject>();
			var objSource2 = Factory.New<AnotherDummyBusinessObject>();
			var objTarget1 = Factory.New<DummyBusinessObject>();
			var objTarget2 = Factory.New<AnotherDummyBusinessObject>();

			objSource1.Z0_Number = 1;
			objSource2.Z0_Number = 1;
			objTarget1.Z0_Number = 2;
			objTarget2.Z0_Number = 2;

			var sourceTree1 = new TreeNode("Root");
			sourceTree1.Tag = objSource1;
			var sourceTree2 = new TreeNode("Root");
			sourceTree2.Tag = objSource2;
			var targetTree1 = new TreeNode("Root");
			targetTree1.Tag = objTarget1;
			var targetTree2 = new TreeNode("Root");
			targetTree2.Tag = objTarget2;

			var columnProvider = new BizoDiffColumnProvider();

			var testIgnoreFieldDictionary = new Dictionary<Type, List<string>>
			{
				{ objSource1.GetType(), new List<string> { "Z0_Number" } }
			};

			var emptyKeyFieldDictionary = new Dictionary<string, List<string>>();

			var comparer1 = new BizoTreeCompare(
				sourceTree1,
				targetTree1,
				columnProvider,
				emptyKeyFieldDictionary,
				testIgnoreFieldDictionary
			);
			var comparer2 = new BizoTreeCompare(
				sourceTree2,
				targetTree2,
				columnProvider,
				emptyKeyFieldDictionary,
				testIgnoreFieldDictionary
			);

			comparer1.Compare();
			comparer2.Compare();

			AssertEquals(Color.Green, sourceTree1.ForeColor);
			AssertEquals(Color.Red, sourceTree2.ForeColor);
			AssertEquals(Color.Green, targetTree1.ForeColor);
			AssertEquals(Color.Red, targetTree2.ForeColor);
		}
	}
	public class AnotherDummyBusinessObject : DummyBaseBusinessObject
	{
		public AnotherDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public static AnotherDummyBusinessObject New(BusinessObjectFactory factory)
		{
			return factory.New<AnotherDummyBusinessObject>();
		}
	}
}
