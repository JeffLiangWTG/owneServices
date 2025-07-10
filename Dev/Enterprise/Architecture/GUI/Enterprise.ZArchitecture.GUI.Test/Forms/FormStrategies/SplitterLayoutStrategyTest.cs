using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
 using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class SplitterLayoutStrategyTest : TransactionedTestCase
	{
		public void TestSaveAndRestoreSplitterLayout()
		{
			var expectScaling1 = 0m;
			var expectScaling2 = 0m;
			var expectScaling3 = 0m;
			var expectScaling4 = 0m;
			var expectScaling5 = 0m;

			using (var testForm1 = new ZFormWithMultipleSplitters())
			{
				testForm1.Show();
				Application.DoEvents();

				testForm1.splitContainerHavingSubContainers.SplitterDistance = 50;
				testForm1.splitContainerInsideAnotherContainer.SplitterDistance = 40;
				testForm1.kSplitter1.SplitPosition = 30;

				testForm1.zTabControl1.SelectedIndex = 1;
				Application.DoEvents();

				testForm1.kSplitContainer3.SplitterDistance = 40;
				testForm1.kSplitContainer4.SplitterDistance = 50;

				expectScaling1 = GetSplitterScaling(testForm1.splitContainerHavingSubContainers);
				expectScaling2 = GetSplitterScaling(testForm1.splitContainerInsideAnotherContainer);
				expectScaling3 = GetSplitterScaling(testForm1.kSplitter1);
				expectScaling4 = GetSplitterScaling(testForm1.kSplitContainer3);
				expectScaling5 = GetSplitterScaling(testForm1.kSplitContainer4);
				testForm1.Close();
			}

			AssertNotEquals(0m, expectScaling1);
			AssertNotEquals(0m, expectScaling2);
			AssertNotEquals(0m, expectScaling3);
			AssertNotEquals(0m, expectScaling4);
			AssertNotEquals(0m, expectScaling5);

			using (var testForm2 = new ZFormWithMultipleSplitters())
			{
				testForm2.Size = ControlDpiScalingHelper.NewScaledSize(1068, 964, true);
				testForm2.Show();
				Application.DoEvents();

				testForm2.zTabControl1.SelectedIndex = 1;
				Application.DoEvents();

				var actualScaling1 = GetSplitterScaling(testForm2.splitContainerHavingSubContainers);
				var actualScaling2 = GetSplitterScaling(testForm2.splitContainerInsideAnotherContainer);
				var actualScaling3 = GetSplitterScaling(testForm2.kSplitter1);
				var actualScaling4 = GetSplitterScaling(testForm2.kSplitContainer3);
				var actualScaling5 = GetSplitterScaling(testForm2.kSplitContainer4);

				AssertEquals("Position of splitter should be restored", expectScaling1, actualScaling1);
				AssertEquals("Position of splitter should be restored", expectScaling2, actualScaling2);
				AssertEquals("Position of splitter should be restored", expectScaling3, actualScaling3);
				AssertEquals("Position of splitter should be restored", expectScaling4, actualScaling4);

				Assert("The splitter of this splitContainer is fixed", testForm2.kSplitContainer4.IsSplitterFixed);
				AssertNotEquals("Position of fixed splitter should not be restored", expectScaling5, actualScaling5);

				testForm2.zTabControl1.SelectedIndex = 0;
				Application.DoEvents();
				testForm2.splitContainerInsideAnotherContainer.SplitterDistance = 60;
				testForm2.splitContainerHavingSubContainers.SplitterDistance = 80;
				testForm2.Hide();

				testForm2.Show();
				Application.DoEvents();

				AssertNotEquals("Position of splitContainer should not be restored again", expectScaling1, GetSplitterScaling(testForm2.splitContainerHavingSubContainers));
			}
		}

		decimal GetSplitterScaling(ISplitterLayoutSaveProvider splitter)
		{
			return decimal.Round(splitter.SplitterPosition / (decimal)splitter.ContainerSize, 2);
		}

		public void TestTestSplitterLayoutIsNotSavedIfNoChange()
		{
			using (var testForm1 = new ZFormWithMultipleSplitters())
			{
				testForm1.Show();
				Application.DoEvents();
				AssertNotEquals("Default splitter position", 0, testForm1.splitContainerHavingSubContainers.SplitterDistance);
				AssertNotEquals("Default splitter position", 0, testForm1.splitContainerInsideAnotherContainer.SplitterDistance);
				AssertNotEquals("Default splitter position", 0, testForm1.kSplitter1.SplitPosition);

				testForm1.zTabControl1.SelectedIndex = 1;
				Application.DoEvents();
				AssertNotEquals("Default splitter position", 0, testForm1.kSplitContainer3.SplitterDistance);
				AssertNotEquals("Default splitter position", 0, testForm1.kSplitContainer4.SplitterDistance);
				testForm1.Close();

				var key1 = SplitterLayoutStrategy.ComputeUniqueKeyForSplitter(testForm1.splitContainerHavingSubContainers);
				var key2 = SplitterLayoutStrategy.ComputeUniqueKeyForSplitter(testForm1.splitContainerInsideAnotherContainer);
				var key3 = SplitterLayoutStrategy.ComputeUniqueKeyForSplitter(testForm1.kSplitter1);
				var key4 = SplitterLayoutStrategy.ComputeUniqueKeyForSplitter(testForm1.kSplitContainer3);
				var key5 = SplitterLayoutStrategy.ComputeUniqueKeyForSplitter(testForm1.kSplitContainer4);

				var registry = ((IWinFormsEnvironment)EnvProxy.Instance).SplitterLayoutRegistry;
				AssertEquals("Splitter layout should not be saved if no change is made", 0m, registry.GetSplitterLayout(key1));
				AssertEquals("Splitter layout should not be saved if no change is made", 0m, registry.GetSplitterLayout(key2));
				AssertEquals("Splitter layout should not be saved if no change is made", 0m, registry.GetSplitterLayout(key3));
				AssertEquals("Splitter layout should not be saved if no change is made", 0m, registry.GetSplitterLayout(key4));
				AssertEquals("Splitter layout should not be saved if no change is made", 0m, registry.GetSplitterLayout(key5));
			}
		}

		public void TestSaveAndRestoreSplitterLayoutWithNegativeSplitterDistance()
		{
			AssertSaveAndRestoreSplitterLayoutWithNegativeSplitterDistance(1f);
		}

		public void TestSaveAndRestoreSplitterLayoutWithNegativeSplitterDistance_Scaled()
		{
			AssertSaveAndRestoreSplitterLayoutWithNegativeSplitterDistance(1.5f);
		}

		public void AssertSaveAndRestoreSplitterLayoutWithNegativeSplitterDistance(float scale)
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(ControlDpiScalingHelper.BaseDpiX * scale, ControlDpiScalingHelper.BaseDpiY * scale))
			{
				var expectScaling = 0m;

				using (var form1 = new ZForm())
				using (var panel1 = new ZCollapsiblePanel { Size = new Size(300, 200) })
				{
					form1.Load += (s, e) => { SplitterLayoutStrategy.RestoreSplittersLayoutCore(form1); };
					panel1.Dock = DockStyle.Right;
					form1.Controls.Add(panel1);
					panel1.IsCollapsed = false;

					((IWinFormsEnvironment)EnvProxy.Instance).SplitterLayoutRegistry.ClearSplitterLayout(SplitterLayoutStrategy.ComputeUniqueKeyForSplitter(panel1));

					form1.Show();
					Application.DoEvents();

					panel1.IsCollapsed = true;

					expectScaling = GetSplitterScaling(panel1);
					Assert(expectScaling < 0);

					form1.Close();
				}

				using (var form2 = new ZForm())
				using (var panel2 = new ZCollapsiblePanel { Size = new Size(300, 200) })
				{
					form2.Load += (s, e) => { SplitterLayoutStrategy.RestoreSplittersLayoutCore(form2); };
					panel2.Dock = DockStyle.Right;
					form2.Controls.Add(panel2);

					Assert(!panel2.IsCollapsed);

					SplitterLayoutStrategy.RestoreSplittersLayoutCore(form2);
					form2.Show();
					Application.DoEvents();

					Assert(panel2.IsCollapsed);

					var actualScaling = GetSplitterScaling(panel2);
					AssertEquals(expectScaling, actualScaling);
				}
			}
		}
	}
}
