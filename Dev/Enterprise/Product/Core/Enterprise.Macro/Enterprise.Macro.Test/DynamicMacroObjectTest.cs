using System;
using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;

namespace Enterprise.Macro.Test
{
	public class DynamicMacroObjectTest : TestCaseWithMacros
	{
		const string CellInternal = "cell";

		public void TestAccessPropertiesOnMultipleObjectsInGivenOrder()
		{
			var object1 = new Object1 { Property = "Hello", Object = new Object1 { Property = "Nested" } };
			var object2 = new Object2 { Property = "Hi", Property2 = "World!" };
			var dynamicObject = new DynamicMacroObject(Context, object1, object2);

			var macroRun = new MacroRun { Data = dynamicObject, ExpectedResult = "Hello", };
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro("Property", macroRun));

			macroRun.ExpectedResult = "Nested";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro("Object.Property", macroRun));

			macroRun.ExpectedResult = "World!";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro("Property2", macroRun));

			macroRun.ExpectedResult = "Hello World!";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro("Property + \" \" + Property2", macroRun));
		}

		public void TestAccessNonExistentPropertyReturnsExpectedNotifications()
		{
			var dynamicObject = new DynamicMacroObject(Context, new Object1 { Property = "Hello" });

			var macroRun = new MacroRun
			{
				Data = dynamicObject, ExpectedNotifications = "DynamicMacroObject does not contain property 'Property2'"
			};
			AssertMacroRun("Property2", macroRun);
		}

		public void TestVariablesPassedToDynamicObject()
		{
			var object1 = new Object1 { Property = "Hello", Object = new Object1 { Property = "Nested" } };
			var object2 = new Object2 { Property = "Hi", Property2 = "World!" };
			var dynamicObject = new DynamicMacroObject(Context, object1, object2);

			var cell = new DummyCell() { BottomRow = 1, LeftColumn = 2, Code = "BBB" };

			var macroRun = new MacroRun
			{
				Data = dynamicObject, ExpectedResult = 1, Variables = { { CellInternal, cell } }
			};

			AssertEquals(macroRun.ExpectedResult, EvaluateMacro($"@{CellInternal}.BottomRow", macroRun));

			macroRun.ExpectedResult = 2;
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro($"@{CellInternal}.LeftColumn", macroRun));

			macroRun = new MacroRun
			{
				Data = new DynamicMacroObject(Context, new DummyCell { Cells = new List<DummyCell> { new() { Code = "AAA", }, new() { Code = "BBB", } } }),
				ExpectedResult = "BBB",
				Variables = { { CellInternal, cell } }
			};
			var macro = $"Where(Cells, {{Code == @{CellInternal}.Code}})[0].Code";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro(macro, macroRun));
		}

		public void TestLibrariesPassedToDynamicObject()
		{
			var macroRun = new MacroRun { Data = new DynamicMacroObject(Context, new Object1 { Property = "Hello" }), ExpectedResult = "He", };
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro("Property.Substring(0,2)", macroRun));
		}

		public void TestNestedObjectPassedToObjectMacro()
		{
			var object1 = new Object1 { Property = "Hello" };
			var macroRun = new MacroRun { Data = new DynamicMacroObject(Context, object1), ExpectedResult = true, Variables = { { "object1", object1 } }, };
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro("Equals(@object1)", macroRun));
		}

		public void TestInvokeMember()
		{
			var macroRun = new MacroRun { Data = new DynamicMacroObject(Context, new List<DummyCell> { new() { Code = "AAA", }, new() { Code = "BBB", } }), ExpectedResult = "BBB", };
			var macro = "Where({Code == \"BBB\"})[0].Code";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro(macro, macroRun));

			macroRun = new MacroRun { Data = new DynamicMacroObject(Context, new DummyCell { Cells = new List<DummyCell> { new() { Code = "AAA", }, new() { Code = "BBB", } } }), ExpectedResult = "BBB", };
			macro = "Where(Cells, {Code == \"BBB\"})[0].Code";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro(macro, macroRun));
		}
		public void TestInvokeMemberSupportsMultipleObjects()
		{
			var macroRun = new MacroRun
			{
				Data = new DynamicMacroObject(Context,
					new Object1 { Property = "Hello" },
					new List<DummyCell> { new() { Code = "AAA", }, new() { Code = "BBB", } }
				),
				ExpectedResult = "BBB"
			};
			var macro = "Where({Code == \"BBB\"})[0].Code";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro(macro, macroRun));

			macroRun = new MacroRun
			{
				Data = new DynamicMacroObject(Context,
					new[] { new { x = 1, y = 2 } },
					new List<DummyCell> { new() { Code = "AAA", }, new() { Code = "BBB", } }
				),
				ExpectedResult = "BBB"
			};
			macro = "Where({Code == \"BBB\"})[0].Code";
			AssertEquals(macroRun.ExpectedResult, EvaluateMacro(macro, macroRun));
		}

		object EvaluateMacro(string macro, MacroRun macroRun)
		{
			IMacroExpression macroExpr = new MacroExpression(macro, Context, null);
			using var scope = new MacroScope(macroRun.Data);
			foreach (var variable in macroRun.Variables)
			{
				scope.SetVariable(variable.Key, variable.Value);
			}
			((DynamicMacroObject)macroRun.Data).SetParentScope(scope);

			return macroExpr.Evaluate(scope);
		}

		IMacroEvaluationContext Context
		{
			get
			{
				_context = new IMacroLibrary[]
				{
					new StandardLibrary(),
					new DummyMacroLibrary(),
				}
				.CreateContext();

				return _context;
			}
		}
		IMacroEvaluationContext _context;
	}

	class DummyCell
	{
		public string Code { get; set; }

		public int BottomRow { get; set; }

		public int LeftColumn { get; set; }

		public List<DummyCell> Cells { get; set; }
	}

	class Object1
	{
		public string Property { get; set; }
		public Object1 Object { get; set; }
	}

	class Object2
	{
		public string Property { get; set; }
		public string Property2 { get; set; }
	}

	class DummyMacroLibrary : MacroLibraryBase
	{
		static IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<object, object, bool>>(
						"Equals",
						"Equals",
						(o1, o2) => ObjectEquals(o1, o2));
			}
		}

		static bool ObjectEquals(object o1, object o2)
		{
			return object.Equals(o1, o2);
		}

		public override IEnumerator<IMacroMetaData> GetEnumerator()
		{
			return Load(MacroHandlers).GetEnumerator();
		}
	}
}
