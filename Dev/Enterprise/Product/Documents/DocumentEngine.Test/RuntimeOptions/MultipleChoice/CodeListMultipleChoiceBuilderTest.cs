using System;
using System.Collections.Specialized;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CodeListMultipleChoiceBuilderTest : FilterBuilderTest
	{
		public void TestGetFilterField()
		{
			var filterBuilder = new CodeListMultipleChoiceBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(CodeListMultipleChoice), filterBuilder.NewField().GetType());
		}

		public void TestCanBuild()
		{
			var filterBuilder = new CodeListMultipleChoiceBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("codelist"));
		}

		public void TestUSEntryTypeList()
		{
			GlbCompany.CurrentCompany.SetCountry("NZ");
			var filterBuilder = new CodeListMultipleChoiceBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);

			var filterField = new CodeListMultipleChoice(Factory);
			filterField.DisplayName = "Entry Type";
			AssertEquals("FilterField's List", null, filterField.List);

			var codeListTypeNode = new StringTreeNode();
			codeListTypeNode.Value = "declarationentrytype codelist";

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(codeListTypeNode);

			var entryTypeNode = new StringTreeNode();
			entryTypeNode.Value = "EntryType";

			var fieldNode = new StringTreeNode();
			fieldNode.Value = "Field";
			fieldNode.Children.Add(entryTypeNode);

			var tree = new StringTreeNode();
			tree.Value = "Entry Type";
			tree.Children.Add(typeNode);
			tree.Children.Add(fieldNode);

			filterBuilder.DoCustomBuilding(tree, filterField);
			var list = filterField.List;
			AssertNotNull("FilterField.List", list);
			AssertEquals("FilterField.List.Count", 0, list.Count);

			GlbCompany.CurrentCompany.SetCountry("US");
			filterBuilder.DoCustomBuilding(tree, filterField);
			AssertEquals("FilterField", typeof(CodeListMultipleChoice), filterField.GetType());
			AssertEquals("Saving OK", true, filterField.CanContinueWithSave);
			AssertEquals("FilterField's List is declarationentrytype", "Enterprise.ZArchitecture.Core.CodeDescriptionPairList", filterField.List.GetType().ToString());
		}

		[UseSnapshotProtection]
		public void TestGetCodeDescriptionListFromDatabase()
		{
			var dummyBiz1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBiz1.Z0_Code = "ZC1";
			dummyBiz1.Z0_Description = "Test Description 1";

			var dummyBiz2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBiz2.Z0_Code = "ZC2";
			dummyBiz2.Z0_Description = "Test Description 2";

			Factory.Save();

			var filterBuilder = (CodeListMultipleChoiceBuilder)GetFilterBuilderToTest();
			var filterField = new CodeListMultipleChoice(Factory);
			filterField.DisplayName = "Test Filter";
			AssertEquals("FilterField's List", null, filterField.List);

			var codeListTypeNode = new StringTreeNode();
			codeListTypeNode.Value = "codelist";

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(codeListTypeNode);

			var sqlStatementNode = new StringTreeNode();
			var dummyBizPK = Guid.NewGuid();
			var insertSqlText = "INSERT INTO dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description) VALUES('" + dummyBizPK + "', 'ZC3', 'Test Description 3')";
			sqlStatementNode.Value = "SELECT Z0_Code, Z0_Description FROM dbo.DummyBizo;" + insertSqlText;

			var sqlDataSourceNode = new StringTreeNode();
			sqlDataSourceNode.Value = "SqlDataSource";
			sqlDataSourceNode.Children.Add(sqlStatementNode);

			var testFilterNode = new StringTreeNode();
			testFilterNode.Value = "TestFilter";

			var fieldNode = new StringTreeNode();
			fieldNode.Value = "Field";
			fieldNode.Children.Add(testFilterNode);

			var tree = new StringTreeNode();
			tree.Value = "Test Filter";
			tree.Children.Add(typeNode);
			tree.Children.Add(sqlDataSourceNode);
			tree.Children.Add(fieldNode);

			filterBuilder.DoCustomBuilding(tree, filterField);
			var list = filterField.List;
			AssertNotNull("FilterField.List", list);
			AssertEquals("FilterField.List.Count", 2, list.Count);

			AssertEquals("ZC1", list[0].Code);
			AssertEquals("Test Description 1", list[0].Description);
			AssertEquals("ZC2", list[1].Code);
			AssertEquals("Test Description 2", list[1].Description);

			AssertNull(Factory.Load<DummyBaseBusinessObject>(dummyBizPK));

			tree.FindChild("SqlDataSource").Child().Value = insertSqlText + ";SELECT Z0_Code, Z0_Description FROM dbo.DummyBizo";
			var expectedExceptionMessage = string.Format("Could not get code list from SqlDataSource [INSERT INTO dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Description) VALUES('{0}', 'ZC3', 'Test Description 3');SELECT Z0_Code, Z0_Description FROM dbo.DummyBizo]: The INSERT permission was denied on the object 'DummyBizo'", dummyBizPK);
			AssertExceptionThrown(typeof(TemplateDefinitionException), expectedExceptionMessage, () => filterBuilder.Build(tree, new StringCollection()), true);

			tree.FindChild("SqlDataSource").Child().Value = "SELECT Z0_Code, Z0_Description FROM DummyBizo11";
			expectedExceptionMessage = "Could not get code list from SqlDataSource [SELECT Z0_Code, Z0_Description FROM DummyBizo11]: Invalid object name 'DummyBizo11'";
			AssertExceptionThrown(typeof(TemplateDefinitionException), expectedExceptionMessage, () => filterBuilder.Build(tree, new StringCollection()), true);

			tree.FindChild("Type").Child().Value = "shipmenttransportmode codelist";
			filterBuilder.DoCustomBuilding(tree, filterField);
			AssertNotNull("FilterField.List", filterField.List);
			AssertEquals("FilterField List should hail from code list type instead of database.", 7, filterField.List.Count);
		}

		public void TestGetCodeDescriptionListFromDatabase_WhenNoDescriptionColumnIsReturned()
		{
			var dummyBiz1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBiz1.Z0_Code = "ZC1";
			dummyBiz1.Z0_Description = "Test Description 1";

			var dummyBiz2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBiz2.Z0_Code = "ZC2";
			dummyBiz2.Z0_Description = "Test Description 2";

			Factory.Save();

			var filterBuilder = (CodeListMultipleChoiceBuilder)GetFilterBuilderToTest();
			var filterField = new CodeListMultipleChoice(Factory);
			filterField.DisplayName = "Test Filter";

			var codeListTypeNode = new StringTreeNode();
			codeListTypeNode.Value = "codelist";

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(codeListTypeNode);

			var sqlStatementNode = new StringTreeNode();
			sqlStatementNode.Value = "SELECT Z0_Code FROM dbo.DummyBizo";

			var sqlDataSourceNode = new StringTreeNode();
			sqlDataSourceNode.Value = "SqlDataSource";
			sqlDataSourceNode.Children.Add(sqlStatementNode);

			var testFilterNode = new StringTreeNode();
			testFilterNode.Value = "TestFilter";

			var fieldNode = new StringTreeNode();
			fieldNode.Value = "Field";
			fieldNode.Children.Add(testFilterNode);

			var tree = new StringTreeNode();
			tree.Value = "Test Filter";
			tree.Children.Add(typeNode);
			tree.Children.Add(sqlDataSourceNode);
			tree.Children.Add(fieldNode);

			filterBuilder.DoCustomBuilding(tree, filterField);
			var list = filterField.List;
			AssertEquals("FilterField.List.Count", 2, list.Count);
			AssertEquals("ZC1", list[0].Code);
			AssertEquals("ZC1", list[0].Description);
			AssertEquals("ZC2", list[1].Code);
			AssertEquals("ZC2", list[1].Description);
		}

		public void TestDependenceListProvider()
		{
			GlbCompany.CurrentCompany.SetCountry("ZA");
			var filterBuilder = new CodeListMultipleChoiceBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);

			var filterField = new CodeListMultipleChoice(Factory);
			filterField.DisplayName = "PPC";
			AssertEquals("FilterField's List", null, filterField.List);

			var codeListTypeNode = new StringTreeNode();
			codeListTypeNode.Value = "previousprocedurecodes codelist";

			var typeNode = new StringTreeNode();
			typeNode.Value = "Type";
			typeNode.Children.Add(codeListTypeNode);

			var entryTypeNode = new StringTreeNode();
			entryTypeNode.Value = "PPC";

			var fieldNode = new StringTreeNode();
			fieldNode.Value = "Field";
			fieldNode.Children.Add(entryTypeNode);

			var tree = new StringTreeNode();
			tree.Value = "PPC";
			tree.Children.Add(typeNode);
			tree.Children.Add(fieldNode);

			filterBuilder.DoCustomBuilding(tree, filterField);
			AssertEquals("FilterField", typeof(CodeListMultipleChoice), filterField.GetType());
			AssertEquals("Saving OK", true, filterField.CanContinueWithSave);
			AssertEquals("DependenceListProvider", "Enterprise.Customs.Module.PreviousProcedureCodesCodeDescriptionPairProvider", filterField.DependenceListProvider.GetType().ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllowInvalidCode()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("CodeListMultipleChoiceWithAllowInvalidCode.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("CodeListMultipleChoice of fields", 3, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Number of options", 4, ((CodeListMultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List.Count);
				AssertEquals("AllowInvalidCode", false, ((CodeListMultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).AllowInvalidCode);
				AssertEquals("Number of options", 4, ((CodeListMultipleChoice)testFilterCollectionBuilder.IFilterCollection[2]).List.Count);
				AssertEquals("AllowInvalidCode", true, ((CodeListMultipleChoice)testFilterCollectionBuilder.IFilterCollection[2]).AllowInvalidCode);
			}
		}

		#region Implementation
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new CodeListMultipleChoiceBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}
		#endregion
	}
}
