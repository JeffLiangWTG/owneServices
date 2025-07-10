using System.Windows.Forms;
using CargoWise.Windows.UI.Layout;
using NUnit.Framework;
using static System.Windows.Forms.Orientation;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KSplitContainerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestSetSplitterDistance()
		{
			using (var testForm = new Form())
			using (var splitContainer = new KSplitContainer())
			{
				testForm.Name = "TestForm";
				testForm.Controls.Add(splitContainer);
				splitContainer.Name = "TestSplitContainer";
				splitContainer.Height = 200;
				splitContainer.Width = 200;
				splitContainer.Orientation = Orientation.Horizontal;
				splitContainer.Panel1MinSize = 20;
				splitContainer.Panel2MinSize = 199;
				((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition = 20;
			}
		}

		public void TestSetSplitterDistanceToValuesTooSmallOrTooLarge()
		{
			// Arrange
			using (var testForm = new Form())
			using (var splitContainer = new KSplitContainer())
			{
				testForm.Name = "TestForm";
				testForm.Controls.Add(splitContainer);
				splitContainer.Name = "TestSplitContainer";
				splitContainer.Height = 200;
				splitContainer.Width = 200;
				splitContainer.Orientation = Horizontal;
				splitContainer.Panel1MinSize = 20;
				splitContainer.Panel2MinSize = 100;
				((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition = 50;
				AssertEquals(50, ((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition);
				// Act
				((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition = splitContainer.Panel1MinSize - 10;
				var splitterPositionTooSmall = ((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition;
				((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition = splitContainer.Height - splitContainer.Panel2MinSize - splitContainer.SplitterWidth + 10;
				var splitterPositionTooLarge = ((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition;
				// Assert
				AssertEquals(splitContainer.Panel1MinSize, splitterPositionTooSmall);
				AssertEquals(splitContainer.Height - splitContainer.Panel2MinSize - splitContainer.SplitterWidth, splitterPositionTooLarge);
			}
		}

		#region Tab Control

		public void TestKSplitContainerTabbingSkipsReadonlyItems()
		{
			using (var form = new Form())
			{
				var childContainer1 = new KSplitContainer();
				var childContainer2 = new KSplitContainer();
				var childContainer3 = new KSplitContainer();

				childContainer1.TabIndex = 0;
				childContainer2.TabIndex = 1;
				childContainer3.TabIndex = 2;

				childContainer1.Text = "Text 1";
				childContainer2.Text = "Text 2";
				childContainer3.Text = "Text 3";

				childContainer1.SetReadOnly(false);
				childContainer2.SetReadOnly(true);
				childContainer3.SetReadOnly(false);

				var splitContainer = new KSplitContainerForTest();

				form.Controls.Add(splitContainer);
				splitContainer.Panel1.Controls.Add(childContainer1);
				splitContainer.Panel1.Controls.Add(childContainer2);
				splitContainer.Panel1.Controls.Add(childContainer3);

				form.Controls.Add(splitContainer);
				form.ActiveControl = childContainer1;

				form.Show();

				var tabResult = splitContainer.ProcessTabKeyExposed();
				AssertNotEquals("Failed to find a control to tab to", false, tabResult);

				var newActive = form.ActiveControl.GetFrontMostActiveControl();
				AssertEquals("Tab should skip text2 as it is read only", childContainer3, newActive);
			}

			Assert(true);
		}

		public void TestKSplitContainerTabbingStopsOnTabStopItems()
		{
			using (var form = new Form())
			{
				var childContainer1 = new KSplitContainer();
				var childContainer2 = new KSplitContainer();
				var childContainer3 = new KSplitContainer();

				childContainer1.TabIndex = 0;
				childContainer2.TabIndex = 1;
				childContainer3.TabIndex = 2;

				childContainer1.Text = "Text 1";
				childContainer2.Text = "Text 2";
				childContainer3.Text = "Text 3";

				childContainer1.TabStop = true;
				childContainer2.TabStop = false;
				childContainer3.TabStop = true;

				var splitContainer = new KSplitContainerForTest();

				form.Controls.Add(splitContainer);
				splitContainer.Panel1.Controls.Add(childContainer1);
				splitContainer.Panel1.Controls.Add(childContainer2);
				splitContainer.Panel1.Controls.Add(childContainer3);

				form.Controls.Add(splitContainer);
				form.ActiveControl = childContainer1;

				form.Show();

				var tabResult = splitContainer.ProcessTabKeyExposed();
				AssertNotEquals("Failed to find a control to tab to", false, tabResult);

				var newActive = form.ActiveControl.GetFrontMostActiveControl();
				AssertEquals("Tab should stop on text 3 because text2.tabstop = false", childContainer3, newActive);
			}

			Assert(true);
		}

#if !WINZOR
		public void TestSplitterWidthForDpiAware()
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(96, 96))
			using (var splitContainer = new KSplitContainer())
			{
				AssertEquals("in DPI 96, default SplitterWidth is 4", 4, splitContainer.SplitterWidth);

				splitContainer.SplitterWidth = 8;
				AssertEquals("in DPI 96, SplitterWidth should not be scaled", 8, splitContainer.SplitterWidth);
			}

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(192, 192))
			using (var splitContainer = new KSplitContainer())
			{
				AssertEquals("in DPI 192, default SplitterWidth should scaled to 8", 8, splitContainer.SplitterWidth);

				splitContainer.SplitterWidth = 8;
				AssertEquals("in DPI 192, SplitterWidth should scaled to 16", 16, splitContainer.SplitterWidth);
			}
		}
#endif

		#endregion

		#region Implimentation

		class KSplitContainerForTest : KSplitContainer
		{
			public bool ProcessTabKeyExposed()
			{
				return ProcessDialogKey(Keys.Tab);
			}
		}

		#endregion
	}
}
