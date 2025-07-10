using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class LayoutGroupBoxCollectionTest : TestCase
	{
		public void TestMaxDesiredWidth()
		{
			GroupBoxCollection.Add(Second);
			GroupBoxCollection.Add(First);

			TextBox thinControl = new TextBox();
			thinControl.Width = 100;
			TextBox wideControl = new TextBox();
			wideControl.Width = 200;

			First.Controls.Add(thinControl);
			AssertEquals("Collection's MaxDesiredWidth should be First's width", First.DesiredWidth, GroupBoxCollection.MaxDesiredWidth);

			Second.Controls.Add(wideControl);
			AssertEquals("Collection's MaxDesiredWidth should now be Seconds's width", Second.DesiredWidth, GroupBoxCollection.MaxDesiredWidth);
		}

		public void TestMaxDesiredHeight()
		{
			GroupBoxCollection.Add(Second);
			GroupBoxCollection.Add(First);

			TextBox shortControl = new TextBox();
			shortControl.Height = 100;
			TextBox tallControl = new TextBox();
			tallControl.Height = 200;

			First.Controls.Add(shortControl);
			AssertEquals("Collection's MaxDesiredheight should be First's height", First.DesiredHeight, GroupBoxCollection.MaxDesiredHeight);

			Second.Controls.Add(tallControl);
			AssertEquals("Collection's MaxDesiredHeight should now be Seconds's height", Second.DesiredHeight, GroupBoxCollection.MaxDesiredHeight);
		}

		public void TestContainsControls()
		{
			GroupBoxCollection.Add(Second);
			GroupBoxCollection.Add(First);
			AssertEquals("Collection contains no controls", false, GroupBoxCollection.ContainsControls);

			TextBox firstControl = new TextBox();
			First.Controls.Add(firstControl);
			AssertEquals("Collection contains controls when first group has some", true, GroupBoxCollection.ContainsControls);

			TextBox secondControl = new TextBox();
			Second.Controls.Add(secondControl);
			AssertEquals("Collection contains controls when multiple groups have them", true, GroupBoxCollection.ContainsControls);

			First.Controls.Remove(firstControl);
			AssertEquals("Collection still contains controls when only group other than first contains them", true, GroupBoxCollection.ContainsControls);

			Second.Controls.Remove(secondControl);
			AssertEquals("Collection contains no controls", false, GroupBoxCollection.ContainsControls);
		}

		public void TestAddCountContainsAndIndexer()
		{
			GroupBoxCollection.Add(First);
			GroupBoxCollection.Add(Second);

			AssertEquals("Count should be 2", 2, GroupBoxCollection.Count);
			AssertEquals("Should contain element indexed with 'First'", true, GroupBoxCollection.Contains("First"));
			AssertEquals("Should contain element indexed with 'Second'", true, GroupBoxCollection.Contains("Second"));
			AssertEquals("Should not contain element indexed with 'Third'", false, GroupBoxCollection.Contains("Third"));

			AutoLayoutGroupBox returnedFromIndexer;
			returnedFromIndexer = GroupBoxCollection["First"];
			AssertNotNull("Indexer should have returned valid object", returnedFromIndexer);
			AssertEquals("Returned should be 'First' group", "First", returnedFromIndexer.Name);

			returnedFromIndexer = GroupBoxCollection["Third"];
			AssertNull("Index doesn't exist should not be valid object", returnedFromIndexer);
		}

		public void TestEnumeratorReturnsInAddedOrder()
		{
			GroupBoxCollection.Add(First);
			GroupBoxCollection.Add(Second);
			AssertEquals("Correct Order is First, Second", "First, Second, ", GroupBoxNamesInEnumeratedOrder(GroupBoxCollection));

			AutoLayoutGroupBoxCollection groupBoxCollectionInDifferentOrder = new AutoLayoutGroupBoxCollection();
			groupBoxCollectionInDifferentOrder.Add(Second);
			groupBoxCollectionInDifferentOrder.Add(First);
			AssertEquals("Correct Order is Second, First", "Second, First, ", GroupBoxNamesInEnumeratedOrder(groupBoxCollectionInDifferentOrder));
		}

		public void TestTotalHeight()
		{
			First.Height = 200;
			GroupBoxCollection.Add(First);
			AssertEquals("Total height should be 200", 200, GroupBoxCollection.TotalHeight);

			Second.Height = 123;
			GroupBoxCollection.Add(Second);
			AssertEquals("Total height should be 200+123", 323, GroupBoxCollection.TotalHeight);
		}

		public void TestRearrangeGroupsInNColumns()
		{
			GroupBoxCollection.Add(First);
			GroupBoxCollection.Add(Second);

			CreateAndAddTextControlToGroup("First", "Control1");
			CreateAndAddTextControlToGroup("First", "Control2");
			CreateAndAddTextControlToGroup("First", "Control3");
			CreateAndAddTextControlToGroup("First", "Control4");
			CreateAndAddTextControlToGroup("First", "Control5");

			CreateAndAddTextControlToGroup("Second", "Control6");

			GroupBoxCollection.RearrangeGroupsInNColumns(2);

			foreach (AutoLayoutGroupBox group in GroupBoxCollection)
			{
				AssertEquals("Group has two columns", 2, group.NextHeights.Count);
			}
		}

		void CreateAndAddTextControlToGroup(string groupName, string controlName)
		{
			TextBox box = new TextBox();
			box.Name = controlName;
			GroupBoxCollection[groupName].AddControl(box);
		}

		public void TestMakeElementsVisibleIfTheyContainControls()
		{
			using (ZForm testForm = new ZForm())
			{
				testForm.Controls.Add(First);
				testForm.Controls.Add(Second);
				GroupBoxCollection.Add(First);
				GroupBoxCollection.Add(Second);
				testForm.Show();
				GroupBoxCollection.MakeElementsVisibleIfTheyContainControls();
				AssertEquals("First not yet visible", false, First.Visible);
				AssertEquals("Second not yet visible", false, Second.Visible);

				TextBox firstControl = new TextBox();
				TextBox secondControl = new TextBox();
				First.Controls.Add(firstControl);
				GroupBoxCollection.MakeElementsVisibleIfTheyContainControls();
				AssertEquals("First visible", true, First.Visible);
				AssertEquals("Second not yet visible", false, Second.Visible);

				Second.Controls.Add(secondControl);
				GroupBoxCollection.MakeElementsVisibleIfTheyContainControls();
				AssertEquals("First visible", true, First.Visible);
				AssertEquals("Second visible", true, Second.Visible);
			}
		}

		string GroupBoxNamesInEnumeratedOrder(AutoLayoutGroupBoxCollection groupBoxes)
		{
			string names = "";
			foreach (AutoLayoutGroupBox groupBox in groupBoxes)
			{
				names += groupBox.Name + ", ";
			}
			return names;
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			GroupBoxCollection = new AutoLayoutGroupBoxCollection();
			First = new AutoLayoutGroupBox();
			First.Name = "First";
			Second = new AutoLayoutGroupBox();
			Second.Name = "Second";
		}
		AutoLayoutGroupBoxCollection GroupBoxCollection;
		AutoLayoutGroupBox First;
		AutoLayoutGroupBox Second;

		protected override void TearDown()
		{
			base.TearDown();
			foreach (AutoLayoutGroupBox groupBox in GroupBoxCollection)
			{
				groupBox.Dispose();
			}
		}
		#endregion
	}
}
