using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZFindBoxColumnTest : ZDropEditColumnTest
	{
		#region TestSortExpressionUsesPropertyDescriptor

		public void TestSortExpressionUsingPropertyDescriptor()
		{
			ZFindBoxColumn column = GetNewColumn("CompanyData", OrgCompanyDataSchema.OB_GB_ControllingBranch.Name, "Lookups.ControllingBranches", typeof(MasterFiles.Business.OrgCompanyData));

			column.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals("SortExpression", "ControllingBranch.GB_Code", column.SortExpression);

			column.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;

			AssertEquals("SortExpression should be cached", "ControllingBranch.GB_Code", column.SortExpression);
			column.SortExpression = "";
			AssertEquals("SortExpression should now be updated", "ControllingBranch.GB_BranchName", column.SortExpression);

			column.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			AssertEquals("SortExpression should be cached", "ControllingBranch.GB_BranchName", column.SortExpression);
			column.SortExpression = null;
			AssertEquals("SortExpression", "ControllingBranch.GB_Code", column.SortExpression);
		}

		public void TestSortExpressionEmptyWhenColumnTypeNotSpecified()
		{
			ZFindBoxColumn column = GetNewColumn("CompanyData", OrgCompanyDataSchema.OB_GB_ControllingBranch.Name, "Lookups.ControllingBranches");

			AssertEquals("SortExpression", "", column.SortExpression);

			column.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals("SortExpression", "", column.SortExpression);

			column.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals("SortExpression", "", column.SortExpression);
		}

		#endregion

		#region setup

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZFindBoxColumn); }
		}

		ZFindBoxColumn TestFindBoxColumn
		{
			get { return TestColumn as ZFindBoxColumn; }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			ZTemplateColumn result = (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, bindTo, "BindToList" });
			return result;
		}

		protected virtual ZFindBoxColumn GetNewColumn(string header, string bindTo, string bindToList)
		{
			ZFindBoxColumn result = (ZFindBoxColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, bindTo, bindToList });
			return result;
		}

		protected virtual ZFindBoxColumn GetNewColumn(string header, string bindTo, string bindToList, Type bizOType)
		{
			ZFindBoxColumn result = (ZFindBoxColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, bindTo, bindToList, bizOType });
			return result;
		}

		#endregion

		public override void TestSortExpression()
		{
			AssertEquals("SortExpression should be blank by default", "", TestColumn.SortExpression);
		}

		public void TestBindToList()
		{
			AssertEquals("BindToList", TestFindBoxColumn.BindToList);
			TestFindBoxColumn.BindToList = "abc";
			AssertEquals("abc", TestFindBoxColumn.BindToList);
		}

		public void TestValueFieldName()
		{
			AssertEquals("", TestFindBoxColumn.ValueFieldName);
			TestFindBoxColumn.ValueFieldName = "abc";
			AssertEquals("abc", TestFindBoxColumn.ValueFieldName);
		}

		public void TestTextFieldName()
		{
			AssertEquals("", TestFindBoxColumn.TextFieldName);
			TestFindBoxColumn.TextFieldName = "abc";
			AssertEquals("abc", TestFindBoxColumn.TextFieldName);
		}

		public void TestAutoPostback()
		{
			AssertEquals(false, TestFindBoxColumn.AutoPostBack);
			TestFindBoxColumn.AutoPostBack = true;
			AssertEquals(true, TestFindBoxColumn.AutoPostBack);
		}

		public void TestDisplayStyle()
		{
			TestFindBoxColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, TestFindBoxColumn.DisplayStyle);
		}

		public void TestModuleID()
		{
			TestFindBoxColumn.ModuleID = WebModuleIDs.Dummy;
			AssertEquals(WebModuleIDs.Dummy, TestFindBoxColumn.ModuleID);
		}
	}
}
