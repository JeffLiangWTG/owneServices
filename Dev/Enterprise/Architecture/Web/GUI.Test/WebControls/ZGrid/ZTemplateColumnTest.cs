using System;
using System.Web.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZTemplateColumnTest : TestCase
	{
		public void TestTextTransform()
		{
			AssertEquals(TextTransformOptions.None, TestColumn.TextTransform);

			TestColumn.TextTransform = TextTransformOptions.UpperCase;
			AssertEquals(TextTransformOptions.UpperCase, TestColumn.TextTransform);

			TestColumn.TextTransform = TextTransformOptions.None;
			AssertEquals(TextTransformOptions.None, TestColumn.TextTransform);
		}

		public void TestAllowEdit()
		{
			AssertNull(TestColumn.ZOwner);
			Assert(TestColumn.AllowEdit);

			ZDataGrid testGrid = new ZDataGrid();
			testGrid.Columns.Add(TestColumn);
			AssertNotNull(TestColumn.ZOwner);
			AssertEquals(testGrid, TestColumn.ZOwner);

			testGrid.AllowEdit = false;
			Assert(!TestColumn.AllowEdit);

			testGrid.AllowEdit = true;
			Assert(TestColumn.AllowEdit);
		}

		public void TestZOwner()
		{
			AssertNull(TestColumn.ZOwner);

			ZDataGrid testGrid = new ZDataGrid();
			testGrid.Columns.Add(TestColumn);
			AssertNotNull(TestColumn.ZOwner);
			AssertEquals(testGrid, TestColumn.ZOwner);
		}

		public virtual void TestHeader()
		{
			AssertEquals("Header incorrect", "TestHeader", TestColumn.HeaderText);
		}

		public void TestInitialize()
		{
			//AssertNull("ItemTemplate should be null", TestColumn.ItemTemplate);
			//AssertNull("EditItemTemplate should be null", TestColumn.EditItemTemplate);

			TestColumn.Initialize();

			AssertNotNull("ItemTemplate should be not null", TestColumn.ItemTemplate);
			//Assert("EditItemTemplate should be not null", TestColumn.ReadOnly || TestColumn.EditItemTemplate != null);
			//Assert("Item Template and EditItemTemplate should be equal", TestColumn.ItemTemplate == TestColumn.EditItemTemplate);
			ITemplate itemTemplate = TestColumn.ItemTemplate;
			//ITemplate editTemplate = TestColumn.EditItemTemplate;

			TestColumn.Initialize();

			AssertEquals("ItemTemplate should not be changed", itemTemplate.GetType(), TestColumn.ItemTemplate.GetType());
			//AssertEquals("EditItemTemplate should be the same", editTemplate.GetType(), TestColumn.EditItemTemplate.GetType());
		}

		public virtual void TestBindTo()
		{
			AssertEquals("BindTo incorrect", GetExpectedBindTo(), TestColumn.BindTo);
		}

		protected virtual string GetExpectedBindTo()
		{
			return "TestBindTo";
		}

		public virtual void TestSortExpression()
		{
			AssertEquals("SortExpression incorrect", GetExpectedBindTo(), TestColumn.SortExpression);
		}

		public void TestReadOnlyFalse()
		{
			AssertNull(TestColumn.ZOwner);
			TestColumn.ReadOnly = false;
			AssertEquals(false, TestColumn.ReadOnly);

			ZDataGrid grid = new ZDataGrid();
			grid.Columns.Add(TestColumn);
			AssertNotNull(TestColumn.ZOwner);
			TestColumn.ZOwner.AllowEdit = true;
			TestColumn.ReadOnly = false;
			AssertEquals(false, TestColumn.ReadOnly);
		}

		public void TestReadOnlyTrue()
		{
			AssertNull(TestColumn.ZOwner);
			TestColumn.ReadOnly = true;
			AssertEquals(true, TestColumn.ReadOnly);

			ZDataGrid grid = new ZDataGrid();
			grid.Columns.Add(TestColumn);
			AssertNotNull(TestColumn.ZOwner);
			TestColumn.ZOwner.AllowEdit = true;
			TestColumn.ReadOnly = true;
			AssertEquals(true, TestColumn.ReadOnly);

			TestColumn.ZOwner.AllowEdit = false;
			TestColumn.ReadOnly = true;
			AssertEquals(true, TestColumn.ReadOnly);

			TestColumn.ZOwner.AllowEdit = false;
			TestColumn.ReadOnly = false;
			AssertEquals(true, TestColumn.ReadOnly);

			TestColumn.Initialize();
			AssertEquals("ReadOnly column should NOT EditItem template", null, TestColumn.EditItemTemplate);
		}

		public void TestNoWrap()
		{
			AssertEquals("Default NoWrap", ExpectedNoWrapDefaultValue, TestColumn.NoWrap);
			TestColumn.NoWrap = !ExpectedNoWrapDefaultValue;
			AssertEquals("Assigned Value", !ExpectedNoWrapDefaultValue, TestColumn.NoWrap);
			TestColumn.NoWrap = ExpectedNoWrapDefaultValue;
			AssertEquals("Assigned Value", ExpectedNoWrapDefaultValue, TestColumn.NoWrap);
		}

		protected ZTemplateColumn fTestColumn;
		protected ZTemplateColumn TestColumn
		{
			get
			{
				if (fTestColumn == null)
				{
					fTestColumn = GetNewColumn("TestHeader", "TestBindTo");
				}

				return fTestColumn;
			}
		}

		protected abstract Type ExpectedColumnType { get; }

		protected virtual bool ExpectedNoWrapDefaultValue
		{
			get { return false; }
		}

		protected virtual ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			ZTemplateColumn result = (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, bindTo });
			return result;
		}
	}
}
