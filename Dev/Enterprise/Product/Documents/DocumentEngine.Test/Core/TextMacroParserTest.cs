using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TextMacroParserTest : TestCaseWithFactory
	{
		public void TestParseMacro()
		{
			var processor = new TextMacroProcessor();
			var macro = "\"<Z0_Code>\" == \"<CompanyCode>\"";
			var result = processor.ParseMacro(macro, new[] { typeof(DummyBusinessObject) }).ToList();
			AssertEquals(3, result.Count);
			AssertMacroExpressionContent(result[0], MacroExpressionType.Method, typeof(AutoDummyBizo), "get_Z0_Code", returnType: typeof(ZString));
			AssertMacroExpressionContent(result[1], MacroExpressionType.Unknown, null, textExpression: " == ");
			AssertMacroExpressionContent(result[2], MacroExpressionType.ValueProvider, null, valueProviderName: "CompanyCode");
		}

		public void TestParseMacro2()
		{
			var processor = new TextMacroProcessor();
			var macro = "<Collection.Find(\"{Z0_Code}\" == \"<CompanyCode>\").Z0_Code>";
			var result = processor.ParseMacro(macro, new[] { Factory.New<DummyBusinessObject>() }).ToList();
			AssertEquals(3, result.Count);
			AssertMacroExpressionContent(result[0], MacroExpressionType.Method, typeof(DummyBusinessObject), "get_Collection", returnType: typeof(DummyChildBusinessObjectCollection));
			AssertMacroExpressionContent(result[1], MacroExpressionType.Method, typeof(DummyChildBusinessObjectCollection), "Find", returnType: typeof(DummyChildBusinessObject));
			AssertMacroExpressionContent(result[2], MacroExpressionType.Method, typeof(DummyChildBusinessObject), "get_Z0_Code", returnType: typeof(ZString));

			AssertEquals(1, result[1].Expressions.Count);
			AssertEquals(3, result[1].Expressions[0].Expressions.Count);
			var findExpressions = result[1].Expressions[0].Expressions;
			AssertMacroExpressionContent(findExpressions[0], MacroExpressionType.Method, typeof(AutoDummyBizo), "get_Z0_Code", returnType: typeof(ZString));
			AssertMacroExpressionContent(findExpressions[1], MacroExpressionType.Unknown, null, textExpression: " == ");
			AssertMacroExpressionContent(findExpressions[2], MacroExpressionType.ValueProvider, null, null, valueProviderName: "CompanyCode");
		}
		public void TestParseMacro3()
		{
			var processor = new TextMacroProcessor();
			var macro = "<Contains(\"<Z0_Description>\", \"<CompanyCode>\")>";
			var result = processor.ParseMacro(macro, new[] { Factory.New<DummyBusinessObject>() }).ToList();
			AssertEquals(1, result.Count);
			AssertMacroExpressionContent(result[0], MacroExpressionType.ValueProvider, null, valueProviderName: "Contains");
			AssertEquals(2, result[0].Expressions.Count);
			AssertMacroExpressionContent(result[0].Expressions[0], MacroExpressionType.Method, typeof(AutoDummyBizo), "get_Z0_Description", null, typeof(ZString));
			AssertMacroExpressionContent(result[0].Expressions[1], MacroExpressionType.ValueProvider, null, valueProviderName: "CompanyCode");
		}
		public void TestParseMacro4()
		{
			var processor = new TextMacroProcessor();
			var macro = "<Collection[1].Z0_Number>";
			var dummy = Factory.New<DummyBusinessObject>();
			var result = processor.ParseMacro(macro, new[] { dummy }).ToList();

			AssertEquals(2, result.Count);

			AssertMacroExpressionContent(result[0], MacroExpressionType.Method, typeof(DummyBusinessObject), "get_Collection", returnType: typeof(DummyChildBusinessObject));
			AssertEquals(1, result[0].Expressions.Count);
			AssertMacroExpressionContent(result[0].Expressions[0], MacroExpressionType.Unknown, null, textExpression: "1");

			AssertMacroExpressionContent(result[1], MacroExpressionType.Method, typeof(DummyChildBusinessObject), "get_Z0_Number", returnType: typeof(ZInt));
		}
		public void TestParseMacro5()
		{
			var processor = new TextMacroProcessor();
			var macro = @"""<WorkflowItems.Find(""{Company.GC_Code}""==""<CompanyCode>""&&""{P9_ShareTasksForAllCompanies}""==""Y"").P9_ShareTasksForAllCompanies>"" == ""Y""";
			var result = processor.ParseMacro(macro, new[] { Factory.New<DummyWithWorkflow>() }).ToList();
			AssertEquals(4, result.Count);

			AssertMacroExpressionContent(result[0], MacroExpressionType.Method, typeof(DummyWithWorkflow), "get_WorkflowItems", returnType: typeof(DummyProcessTaskCollection));
			AssertMacroExpressionContent(result[1], MacroExpressionType.Method, typeof(DummyProcessTaskCollection), "Find", returnType: typeof(DummyProcessTask));
			AssertMacroExpressionContent(result[2], MacroExpressionType.Method, typeof(DummyProcessTask), "get_P9_ShareTasksForAllCompanies", returnType: typeof(ZBool));
			AssertMacroExpressionContent(result[3], MacroExpressionType.Unknown, null, textExpression: " == \"Y\"");

			AssertEquals(1, result[1].Expressions.Count);
			AssertEquals(7, result[1].Expressions[0].Expressions.Count);
			var findExpressions = result[1].Expressions[0].Expressions;
			AssertMacroExpressionContent(findExpressions[0], MacroExpressionType.Method, typeof(AutoProcessTasks), "get_Company", returnType: typeof(GlbCompany));
			AssertMacroExpressionContent(findExpressions[1], MacroExpressionType.Method, typeof(GlbCompany), "get_GC_Code", returnType: typeof(ZString));
			AssertMacroExpressionContent(findExpressions[2], MacroExpressionType.Unknown, null, textExpression: "==");
			AssertMacroExpressionContent(findExpressions[3], MacroExpressionType.ValueProvider, null, valueProviderName: "CompanyCode");
			AssertMacroExpressionContent(findExpressions[4], MacroExpressionType.Unknown, null, textExpression: "&&");
			AssertMacroExpressionContent(findExpressions[5], MacroExpressionType.Method, typeof(ProcessTask), "get_P9_ShareTasksForAllCompanies", returnType: typeof(ZBool));
			AssertMacroExpressionContent(findExpressions[6], MacroExpressionType.Unknown, null, textExpression: "==\"Y\"");
		}

		public void TestParseMacroWithMultipleBusinessObjects()
		{
			var processor = new TextMacroProcessor();
			var macro = "\"<Z0_Code>\" == \"<P9_Description>\"";
			var result = processor.ParseMacro(macro, new[] { typeof(DummyWithWorkflow), typeof(ProcessTask) }).ToList();
			AssertEquals(3, result.Count);
			AssertMacroExpressionContent(result[0], MacroExpressionType.Method, typeof(AutoDummyBizo), "get_Z0_Code", returnType: typeof(ZString));
			AssertMacroExpressionContent(result[2], MacroExpressionType.Method, typeof(ProcessTask), "get_P9_Description", returnType: typeof(ZString));
		}

		void AssertMacroExpressionContent(ITextMacroExpression expr, MacroExpressionType expressionType, Type dataSourceType, string methodToCall = null, string textExpression = null, Type returnType = null, string valueProviderName = null)
		{
			AssertEquals(dataSourceType, expr.DataSourceType);
			AssertEquals(expressionType, expr.ExpressionType);
			AssertEquals(methodToCall, expr.MethodInfo?.Name);
			AssertEquals(textExpression, expr.TextExpression);
			AssertEquals(returnType, expr.ReturnType);
			if (valueProviderName != null)
			{
				AssertEquals(valueProviderName, expr.ValueProvider.GetType().Name);
			}
		}
	}
}
