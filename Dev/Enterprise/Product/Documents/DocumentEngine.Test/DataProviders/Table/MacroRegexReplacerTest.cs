using System;
using System.Data;
using System.IO;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class MacroRegexReplacerTest : TestCaseWithFactory
	{
		public void TestParametersWithTypeName()
		{
			using (var testReport = new Report(new DocumentPack(), TestEmptyAndValidTemplate))
			{
				var macroReplacer = new MacroRegexReplacerForTesting(testReport);
				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList(TestData.DocEngineTestTableName);

				var expectedType = "TVP";
				var expectedValue = new DataTable();
				var param = macroReplacer.GetSqlParameterForMacroExposed("Macro", expectedValue, expectedType);

				AssertNotEquals("param.ParameterName", String.Empty, param.ParameterName);
				AssertEquals("param.Value", expectedValue, param.Value);
				AssertEquals("param.TypeName", expectedType, param.TypeName);
			}
		}

		public void TestMacroIsTheUniqueKeyOfSqlParameters()
		{
			using (var testReport = new Report(new DocumentPack(), TestEmptyAndValidTemplate))
			{
				var macroReplacer = new MacroRegexReplacerForTesting(testReport);
				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList(TestData.DocEngineTestTableName);

				var param1 = macroReplacer.GetSqlParameterForMacroExposed("Macro1", 1);
				var param2 = macroReplacer.GetSqlParameterForMacroExposed("Macro2", 2);
				var param3 = macroReplacer.GetSqlParameterForMacroExposed("Macro1", 1);
				var expectedType = "tvp";
				var param4 = macroReplacer.GetSqlParameterForMacroExposed("Macro3", 1, expectedType);

				AssertNotNullOrEmpty("Name of Parameter 1 should not be empty", param1.ParameterName);
				AssertEquals("param 1 and 3 must be the same. Something is wrong with the related Hashtable.", param1.ParameterName, param3.ParameterName);
				AssertEquals("param 1 and 3 must be the same. Something is wrong with the related Hashtable.", param1.Value, param3.Value);
				AssertEquals("param 2 should be different from param1. Something is wrong with the related Hashtable.", true, param1.ParameterName != param2.ParameterName);
			}
		}

		public void TestParametersForUserDefinedFunctionsDoesNotHaveAnyDuplicateValue()
		{
			using (var testReport = new Report(new DocumentPack(), TestEmptyAndValidTemplate))
			{
				var macroReplacer = new MacroRegexReplacerForTesting(testReport);
				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList(TestData.DocEngineTestTableName);

				var param1 = macroReplacer.GetSqlParameterForMacroExposed("Macro1", 1);
				var param2 = macroReplacer.GetSqlParameterForMacroExposed("Macro2", 2);
				var param3 = macroReplacer.GetSqlParameterForMacroExposed("Macro1", 1);

				var newParameters = macroReplacer.UdfParameters;
				AssertEquals("Two Parameters expexted", 2, newParameters.Length);
				AssertEquals("Param1 Must Exist in the list", true,
					newParameters[0].Equals(param1) || newParameters[1].Equals(param1));
				AssertEquals("Param2 Must Exist in the list", true,
					newParameters[0].Equals(param2) || newParameters[1].Equals(param2));
			}
		}

		public void TestReplaceParameterlessMacro()
		{
			using (var report = new Report(new DocumentPack(), TestEmptyAndValidTemplate))
			{
				var expectedDataSourceString = "select * from " + Db.DatabaseName + DbUserRepository.RepositoryDbSuffix + ".dbo.AView";
				var macroReplacer = new MacroRegexReplacerForTesting(report);
				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList("select * from <UserRepository>.dbo.AView");
				AssertEquals(expectedDataSourceString, dataSourceString);
			}
		}

		public void TestReplaceMacrosWhenMacroValueContainsAngleBracket()
		{
			using (var report = new Report(new DocumentPack(), TestEmptyAndValidTemplate))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test1", "AB<"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test2", "AB>"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test3", "CD<"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test4", "CD>"));

				var dataSourceString = "select * from dbo.glbcompany where gc_code in ('<Test1>','<Test2>','<<IF(1==1,\"<<Test3>>\",\"<<Test4>>\")>>','<<IF(1==2,\"<<Test3>>\",\"<<Test4>>\")>>')";
				var macroReplacer = new MacroRegexReplacerForTesting(report);
				dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList(dataSourceString);

				AssertEquals("UdfParameters.Length", 2, macroReplacer.UdfParameters.Length);

				AssertEquals("Param1", "AB<", macroReplacer.UdfParameters[0].Value.ToString());
				AssertEquals("Param1", "AB>", macroReplacer.UdfParameters[1].Value.ToString());

				var expectedDataSourceString = $"select * from dbo.glbcompany where gc_code in ('{macroReplacer.UdfParameters[0].ParameterName}','{macroReplacer.UdfParameters[1].ParameterName}','CD<','CD>')";
				AssertEquals(expectedDataSourceString, dataSourceString);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplaceParametersWithType()
		{
			var template = new ExcelTemplateForUnitTesting("MultipleSelectionLookup.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				report.PrepareForRender();

				var lookup = report.FilterCollection[1] as MultipleSelectionLookup;
				AssertNotNull("Precondition: MultipleSelectionLookup is chosen.", lookup);
				AssertEquals("Precondition: DisplayName", "Orgs", lookup.DisplayName);

				var collectionProvider = lookup.CollectionProvider;
				AssertEquals("Precondition: collectionProvider.Collection is empty", 0, collectionProvider.Collection.Count);

				var org1 = Factory.New<OrgHeader>();
				org1.OH_Code = "ABCDEFG";
				collectionProvider.Collection.Add(org1);

				var org2 = Factory.New<OrgHeader>();
				org2.OH_Code = "HIJKLMNOP";
				collectionProvider.Collection.Add(org2);

				var macroReplacer = new MacroRegexReplacerForTesting(report);
				AssertEquals("Precondition: UdfParameters.Length", 0, macroReplacer.UdfParameters.Length);
				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList("select * from func(<Orgs.CodesAsTVP>)");
				AssertEquals("UdfParameters.Length", 1, macroReplacer.UdfParameters.Length);
				var parameter = macroReplacer.UdfParameters[0];
				AssertEquals("parameter.SqlDbType", SqlDbType.Structured, parameter.SqlDbType);
				AssertEquals("parameter.TypeName", TVPHelper.TVP_varchar, parameter.TypeName);
				AssertEquals("dataSourceString", string.Format("select * from func({0})", parameter.ParameterName), dataSourceString);
				AssertType("parameter.Value", typeof(DataTable), parameter.Value);
				var valueAsTable = (DataTable)parameter.Value;
				AssertEquals("Columns count", 1, valueAsTable.Columns.Count);
				var expectedColumnName = "Value";
				AssertEquals("ColumnName", expectedColumnName, valueAsTable.Columns[0].ColumnName);
				AssertEquals("Rows count", 2, valueAsTable.Rows.Count);
				AssertEquals("Row value", org1.OH_Code, valueAsTable.Rows[0][expectedColumnName]);
				AssertEquals("Row value", org2.OH_Code, valueAsTable.Rows[1][expectedColumnName]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplaceParametersWithSqlDbType()
		{
			var template = new ExcelTemplateForUnitTesting("DateFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				report.PrepareForRender();

				var dateField = (DateField)report.FilterCollection[1];
				dateField.Value = new DateTime(2003, 1, 25);

				var macroReplacer = new MacroRegexReplacerForTesting(report);
				AssertEquals("Precondition: UdfParameters.Length", 0, macroReplacer.UdfParameters.Length);

				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList("select * from func(<Some date.ValueForSQLParameter>)");
				AssertEquals("UdfParameters.Length", 1, macroReplacer.UdfParameters.Length);

				var parameter = macroReplacer.UdfParameters[0];
				AssertEquals("parameter.SqlDbType", SqlDbType.DateTime, parameter.SqlDbType);
				AssertEquals("dataSourceString", string.Format("select * from func({0})", parameter.ParameterName), dataSourceString);
				AssertEquals("parameter.SqlDbType", SqlDbType.DateTime, parameter.SqlDbType);

				var parameterValue = parameter.Value;
				AssertType<DateTime>("parameter.Value type", parameterValue);
				AssertEquals("parameter.Value value", new DateTime(2003, 1, 25), parameter.Value);
			}
		}

		public void TestReplaceStringParameters_WithoutSqlDbTypeSpecified_ChoosesTypeAdequately()
		{
			using (var testReport = new Report(new DocumentPack(), TestEmptyAndValidTemplate))
			{
				var macroReplacer = new MacroRegexReplacerForTesting(testReport);
				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList(TestData.DocEngineTestTableName);

				var param1 = macroReplacer.GetSqlParameterForMacroExposed("Macro1", "ABC");
				AssertEquals(SqlDbType.VarChar, param1.SqlDbType);

				var param2 = macroReplacer.GetSqlParameterForMacroExposed("Macro2", "日本");
				AssertEquals(SqlDbType.NVarChar, param2.SqlDbType);

				var param3 = macroReplacer.GetSqlParameterForMacroExposed("Macro3", "New 日本");
				AssertEquals(SqlDbType.NVarChar, param3.SqlDbType);

				var param4 = macroReplacer.GetSqlParameterForMacroExposed("Macro4", "");
				AssertEquals(SqlDbType.VarChar, param4.SqlDbType);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplaceParametersWithSqlDbType_EmptyDate()
		{
			var template = new ExcelTemplateForUnitTesting("DateFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				report.PrepareForRender();

				var macroReplacer = new MacroRegexReplacerForTesting(report);
				AssertEquals("Precondition: UdfParameters.Length", 0, macroReplacer.UdfParameters.Length);

				var dataSourceString = macroReplacer.ReplaceMacrosAndRebuildUdfParameterList("select * from func(<Some date.ValueForSQLParameter>)");
				AssertEquals("UdfParameters.Length", 1, macroReplacer.UdfParameters.Length);

				var parameter = macroReplacer.UdfParameters[0];
				AssertEquals("parameter.SqlDbType", SqlDbType.DateTime, parameter.SqlDbType);
				AssertEquals("dataSourceString", string.Format("select * from func({0})", parameter.ParameterName), dataSourceString);
				AssertEquals("parameter.SqlDbType", SqlDbType.DateTime, parameter.SqlDbType);

				var parameterValue = parameter.Value;
				AssertType<DBNull>("parameter.Value type", parameterValue);
				AssertEquals("parameter.Value value", DBNull.Value, parameter.Value);
			}
		}

		public void TestReplaceParametersForSQLQueryHints()
		{
			using (var report = new Report(new DocumentPack(), TestEmptyAndValidTemplate))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test1", "AB<"));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test2", "AB>"));

				var queryHintsString = "OPTIMIZE FOR (<Test1> = 'N',<Test2> UNKNOWN)";
				var macroReplacer = new MacroRegexReplacerForTesting(report);
				queryHintsString = macroReplacer.ReplaceParametersForSQLQueryHints(queryHintsString);

				var expectedDataSourceString = $"OPTIMIZE FOR ({macroReplacer.UdfParameters[0].ParameterName} = 'N',{macroReplacer.UdfParameters[1].ParameterName} UNKNOWN)";
				AssertEquals(expectedDataSourceString, queryHintsString);
			}
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		ExcelTemplateForUnitTesting testEmptyAndValidTemplate;
		ExcelTemplateForUnitTesting TestEmptyAndValidTemplate
		{
			get
			{
				if (testEmptyAndValidTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					testEmptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return testEmptyAndValidTemplate;
			}
		}

		sealed class MacroRegexReplacerForTesting : MacroRegexReplacer
		{
			public MacroRegexReplacerForTesting(Report reportObject)
				: base(reportObject)
			{
			}

			public SqlParameter GetSqlParameterForMacroExposed(string macro, object value, string parameterTypeName = "")
			{
				return GetSqlParameterForMacroAndAddTheNewOnesToParametersForUserDefinedFunctions(macro, value, parameterTypeName);
			}
		}
	}
}
