using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDropDownListColumnTest : ZTemplateColumnTest
	{
		#region setup

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZDropDownListColumn); }
		}

		ZDropDownListColumn TestDropDownListColumn
		{
			get { return TestColumn as ZDropDownListColumn; }
		}

		#endregion

		public void TestBindToList()
		{
			AssertEquals("", TestDropDownListColumn.BindToList);
			TestDropDownListColumn.BindToList = "abc";
			AssertEquals("abc", TestDropDownListColumn.BindToList);
		}

		public void TestValueFieldName()
		{
			AssertEquals("", TestDropDownListColumn.ValueFieldName);
			TestDropDownListColumn.ValueFieldName = "abc";
			AssertEquals("abc", TestDropDownListColumn.ValueFieldName);
		}

		public void TestTextFieldName()
		{
			AssertEquals("", TestDropDownListColumn.TextFieldName);
			TestDropDownListColumn.TextFieldName = "abc";
			AssertEquals("abc", TestDropDownListColumn.TextFieldName);
		}

		public void TestAutoPostback()
		{
			AssertEquals(false, TestDropDownListColumn.AutoPostBack);
			TestDropDownListColumn.AutoPostBack = true;
			AssertEquals(true, TestDropDownListColumn.AutoPostBack);
		}

		public void TestDisplayStyle()
		{
			TestDropDownListColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, TestDropDownListColumn.DisplayStyle);
		}

		public void TestShowHint()
		{
			AssertEquals(true, TestDropDownListColumn.ShowHint);
			TestDropDownListColumn.ShowHint = false;
			AssertEquals(false, TestDropDownListColumn.ShowHint);
		}

		public void TestShowEmptyItem()
		{
			Assert(TestDropDownListColumn.ShowEmptyItem);
			TestDropDownListColumn.ShowEmptyItem = false;
			Assert(!TestDropDownListColumn.ShowEmptyItem);
		}

		public void TestEditorWidthDefault()
		{
			AssertEquals(-1, TestDropDownListColumn.EditorWidth);
		}
	}
}
