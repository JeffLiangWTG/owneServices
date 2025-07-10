using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	[TestedType(typeof(GridLayoutContainer))]
	sealed class GridLayoutContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be 6 columns", 6, TestLayoutContainer.AllColumns.Count);
			AssertElementNumbers(TestLayoutContainer.AllColumns, -1, 2, 3, 4, 5, 6);
			AssertElementHeaders(TestLayoutContainer.AllColumns, "Column 0", "Column 1", "Column 2", "Column 3", "Column 4", "Column _5");

			AssertEquals("Should be 3 in Layout", 3, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 2, 3);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 1", "Column 2");

			AssertEquals("Should be 4 in Default Layout", 4, TestLayoutContainer.DefaultLayout.Count);
			AssertElementNumbers(TestLayoutContainer.DefaultLayout, -1, 2, 3, 4);
			AssertElementHeaders(TestLayoutContainer.DefaultLayout, "Column 0", "Column 1", "Column 2", "Column 3");

			AssertEquals("Should be 3 available", 3, TestLayoutContainer.AvailableColumns.Count);
			AssertElementNumbers(TestLayoutContainer.AvailableColumns, 6, 4, 5);
			AssertElementHeaders(TestLayoutContainer.AvailableColumns, "Column _5", "Column 3", "Column 4");

			AssertEquals("Expected CurrentLayoutString", "0,1,2", TestLayoutContainer.CurrentLayoutString);
			AssertEquals("Layout selection should be empty", ZString.Empty, TestLayoutContainer.SelectedLayoutColumn);
			AssertEquals("Selected Available should be empty", ZString.Empty, TestLayoutContainer.SelectedAvailableColumn);
		}

		public void TestCurrentLayoutString()
		{
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 2, 3);
			AssertEquals("CurrentLayoutString should be as expected", "0,1,2", TestLayoutContainer.CurrentLayoutString);

			TestLayoutContainer.CurrentLayout.Remove(TestLayoutContainer.CurrentLayout[2]);

			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 2);
			AssertEquals("CurrentLayoutString should reflect modification", "0,1", TestLayoutContainer.CurrentLayoutString);
		}

		public void TestSelectedLayoutColumn()
		{
			AssertEquals("Selection should be empty", ZString.Empty, TestLayoutContainer.SelectedLayoutColumn);

			TestLayoutContainer.SelectedLayoutColumn = "2";
			AssertEquals("SelectedLayoutColumn should be as expected", "2", TestLayoutContainer.SelectedLayoutColumn);
		}

		public void TestSelectedAvailableColumn()
		{
			AssertEquals("Selection should be empty", ZString.Empty, TestLayoutContainer.SelectedAvailableColumn);

			TestLayoutContainer.SelectedAvailableColumn = "5";
			AssertEquals("SelectedAvailableColumn should be as expected", "5", TestLayoutContainer.SelectedAvailableColumn);
		}

		public void TestAddToLayout()
		{
			TestLayoutContainer.SelectedAvailableColumn = "4";
			TestLayoutContainer.AddToLayout();

			AssertEquals("Should be 4 in Layout", 4, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 2, 3, 4);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 1", "Column 2", "Column 3");
			AssertEquals("SelectedLayoutColumn should be as expected", "4", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 2 available", 2, TestLayoutContainer.AvailableColumns.Count);
			AssertElementNumbers(TestLayoutContainer.AvailableColumns, 6, 5);
			AssertElementHeaders(TestLayoutContainer.AvailableColumns, "Column _5", "Column 4");
			AssertEquals("SelectedAvailableColumn should be as expected", "5", TestLayoutContainer.SelectedAvailableColumn);

			AssertEquals("Expected CurrentLayoutString", "0,1,2,3", TestLayoutContainer.CurrentLayoutString);

			TestLayoutContainer.AddToLayout();

			AssertEquals("Should be 5 in Layout", 5, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 2, 3, 4, 5);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 1", "Column 2", "Column 3", "Column 4");
			AssertEquals("SelectedLayoutColumn should be as expected", "5", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should 1 available", 1, TestLayoutContainer.AvailableColumns.Count);
			AssertElementNumbers(TestLayoutContainer.AvailableColumns, 6);
			AssertElementHeaders(TestLayoutContainer.AvailableColumns, "Column _5");
			AssertEquals("SelectedAvailableColumn should be as expected", "6", TestLayoutContainer.SelectedAvailableColumn);

			AssertEquals("Expected CurrentLayoutString", "0,1,2,3,4", TestLayoutContainer.CurrentLayoutString);
		}

		public void TestRemoveFromLayout()
		{
			TestLayoutContainer.SelectedLayoutColumn = "2";
			TestLayoutContainer.RemoveFromLayout();

			AssertEquals("Should be 2 in Layout", 2, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 3);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 2");
			AssertEquals("SelectedLayoutColumn should be as expected", "3", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 4 available", 4, TestLayoutContainer.AvailableColumns.Count);
			AssertElementNumbers(TestLayoutContainer.AvailableColumns, 6, 2, 4, 5);
			AssertElementHeaders(TestLayoutContainer.AvailableColumns, "Column _5", "Column 1", "Column 3", "Column 4");
			AssertEquals("SelectedAvailableColumn should be as expected", "2", TestLayoutContainer.SelectedAvailableColumn);

			AssertEquals("Expected CurrentLayoutString", "0,2", TestLayoutContainer.CurrentLayoutString);

			TestLayoutContainer.SelectedLayoutColumn = "-1";
			TestLayoutContainer.RemoveFromLayout();

			AssertEquals("Should be 2 in Layout, because you cannot remove required column", 2, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 3);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 2");
			AssertEquals("SelectedLayoutColumn should be as expected", "-1", TestLayoutContainer.SelectedLayoutColumn);

			TestLayoutContainer.SelectedLayoutColumn = "3";
			TestLayoutContainer.RemoveFromLayout();

			AssertEquals("Should be 1 in Layout", 1, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0");
			AssertEquals("SelectedLayoutColumn should be as expected", "-1", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 5 available", 5, TestLayoutContainer.AvailableColumns.Count);
			AssertElementNumbers(TestLayoutContainer.AvailableColumns, 6, 2, 3, 4, 5);
			AssertElementHeaders(TestLayoutContainer.AvailableColumns, "Column _5", "Column 1", "Column 2", "Column 3", "Column 4");
			AssertEquals("SelectedAvailableColumn should be as expected", "3", TestLayoutContainer.SelectedAvailableColumn);

			AssertEquals("Expected CurrentLayoutString", "0", TestLayoutContainer.CurrentLayoutString);

			TestLayoutContainer.SelectedLayoutColumn = "-1";
			TestLayoutContainer.RemoveFromLayout();

			AssertEquals("Should not remove the required last column from Layout", "0", TestLayoutContainer.CurrentLayoutString);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0");
		}

		public void TestRestoreDefault()
		{
			TestLayoutContainer.RestoreDefault();

			AssertEquals("Should be 4 in Layout", 4, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 2, 3, 4);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 1", "Column 2", "Column 3");
			AssertEquals("SelectedLayoutColumn should be empty", ZString.Empty, TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 2 available", 2, TestLayoutContainer.AvailableColumns.Count);
			AssertElementNumbers(TestLayoutContainer.AvailableColumns, 6, 5);
			AssertElementHeaders(TestLayoutContainer.AvailableColumns, "Column _5", "Column 4");
			AssertEquals("SelectedAvailableColumn should be empty", ZString.Empty, TestLayoutContainer.SelectedAvailableColumn);

			AssertEquals("Expected CurrentLayoutString", "0,1,2,3", TestLayoutContainer.CurrentLayoutString);
		}

		public void TestMoveSelectedUp()
		{
			TestLayoutContainer.SelectedLayoutColumn = "2";
			TestLayoutContainer.MoveSelectedUp();

			AssertEquals("Should be 3 in Layout", 3, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, 2, -1, 3);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 1", "Column 0", "Column 2");
			AssertEquals("SelectedLayoutColumn should be the same", "2", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 2 available", 3, TestLayoutContainer.AvailableColumns.Count);

			AssertEquals("Expected CurrentLayoutString", "1,0,2", TestLayoutContainer.CurrentLayoutString);

			TestLayoutContainer.MoveSelectedUp();

			AssertEquals("Should be 3 in Layout", 3, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, 2, -1, 3);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 1", "Column 0", "Column 2");
			AssertEquals("SelectedLayoutColumn should be the same", "2", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 3 available", 3, TestLayoutContainer.AvailableColumns.Count);

			AssertEquals("CurrentLayoutString should not be changed", "1,0,2", TestLayoutContainer.CurrentLayoutString);
		}

		public void TestMoveSelectedDown()
		{
			TestLayoutContainer.SelectedLayoutColumn = "2";
			TestLayoutContainer.MoveSelectedDown();

			AssertEquals("Should be 3 in Layout", 3, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 3, 2);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 2", "Column 1");
			AssertEquals("SelectedLayoutColumn should be the same", "2", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 3 available", 3, TestLayoutContainer.AvailableColumns.Count);

			AssertEquals("Expected CurrentLayoutString", "0,2,1", TestLayoutContainer.CurrentLayoutString);

			TestLayoutContainer.MoveSelectedDown();

			AssertEquals("Should be 3 in Layout", 3, TestLayoutContainer.CurrentLayout.Count);
			AssertElementNumbers(TestLayoutContainer.CurrentLayout, -1, 3, 2);
			AssertElementHeaders(TestLayoutContainer.CurrentLayout, "Column 0", "Column 2", "Column 1");
			AssertEquals("SelectedLayoutColumn should be the same", "2", TestLayoutContainer.SelectedLayoutColumn);

			AssertEquals("Should be 3 available", 3, TestLayoutContainer.AvailableColumns.Count);

			AssertEquals("CurrentLayoutString should not be changed", "0,2,1", TestLayoutContainer.CurrentLayoutString);
		}

		#region Implementation

		GridLayoutContainer TestLayoutContainer;

		void AssertElementNumbers(GridLayoutElementCollection collection, params int[] numbers)
		{
			AssertEquals("Collection Count should be as expected", numbers.Length, collection.Count);

			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals(string.Format("Column {0} in Collection should have expected Number", i), numbers[i], collection[i].ColumnNumber);
			}
		}

		void AssertElementHeaders(GridLayoutElementCollection collection, params string[] headers)
		{
			AssertEquals("Collection Count should be as expected", headers.Length, collection.Count);

			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals(string.Format("Column {0} in Collection should have expected Header", i), headers[i], collection[i].HeaderText);
			}
		}

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			TestLayoutContainer = (GridLayoutContainer)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GridLayoutContainer(new StringCollectionX("Column 0", "Column 1", "Column 2", "Column 3", "Column 4", "Column _5"), "0,1,2", "0,1,2,3", "0", "");
		}

		#endregion

		#endregion
	}
}
